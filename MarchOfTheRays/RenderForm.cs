using System;
using System.Drawing;
using System.Windows.Forms;

namespace MarchOfTheRays
{
    class RenderForm : Form
    {
        ProgressBar progressBar;
        ContextMenuStrip contextMenu;
        ToolStripItem saveImage;

        public RenderForm()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            progressBar = new ProgressBar();

            progressBar.Visible = false;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;

            contextMenu = new ContextMenuStrip();
            saveImage = contextMenu.Items.Add("Save image", null, (s, e) =>
            {
                if(BackgroundImage != null && !Loading)
                {
                    var dialog = new SaveFileDialog();
                    dialog.Filter = "PNG Image|*.png";
                    if(dialog.ShowDialog() == DialogResult.OK)
                    {
                        BackgroundImage.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            });
            saveImage.Enabled = false;

            ContextMenuStrip = contextMenu;

            Controls.Add(progressBar);
        }

        protected override void OnResize(EventArgs e)
        {
            progressBar.Width = (ClientSize.Width * 3) / 4;
            progressBar.Location = new Point((ClientSize.Width - progressBar.Width) / 2, (ClientSize.Height - progressBar.Height) / 2);
            Invalidate();
            base.OnResize(e);
        }

        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            saveImage.Enabled = !Loading && BackgroundImage != null;
            base.OnBackgroundImageChanged(e);
        }

        public bool NoActivation { get; set; }

        protected override bool ShowWithoutActivation => NoActivation;

        bool m_Loading;

        public bool Loading
        {
            get => m_Loading;
            set
            {
                m_Loading = value;
                if (value)
                {
                    progressBar.Visible = true;
                    saveImage.Enabled = false;
                    Invalidate();
                }
                else
                {
                    progressBar.Visible = false;
                    saveImage.Enabled = true;
                    Invalidate();
                }
            }
        }

        public float Progress
        {
            get => progressBar.Value / 100.0f;
            set => progressBar.Value = Math.Min(100, (int)(value * 100));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (Loading)
            {
                using (Brush b = new SolidBrush(Color.FromArgb(100, BackColor)))
                {
                    g.FillRectangle(b, e.ClipRectangle);
                }
            }
            base.OnPaint(e);
        }
    }
}
