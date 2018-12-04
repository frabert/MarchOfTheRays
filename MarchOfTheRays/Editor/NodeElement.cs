using System;

using System.Drawing;
using System.Windows.Forms;

namespace MarchOfTheRays.Editor
{
    abstract class LightweightControl : IComparable<LightweightControl>
    {
        public RectangleF ClientRectangle => new RectangleF(m_Location, m_Size);

        public virtual Region GetRegion()
        {
            return new Region(ClientRectangle);
        }

        SizeF m_Size;
        public SizeF Size
        {
            get => m_Size;
            set
            {
                m_Size = value;
                OnResize();
            }
        }

        public float Width
        {
            get => m_Size.Width;
            set
            {
                m_Size = new SizeF(value, Height);
            }
        }

        public float Height
        {
            get => m_Size.Height;
            set
            {
                m_Size = new SizeF(Width, value);
            }
        }

        public event EventHandler Resize;

        protected virtual void OnResize()
        {
            Resize?.Invoke(this, new EventArgs());
        }

        PointF m_Location;
        public PointF Location
        {
            get => m_Location;
            set
            {
                m_Location = value;
                OnMove();
            }
        }

        public event EventHandler Move;

        protected virtual void OnMove()
        {
            Move?.Invoke(this, new EventArgs());
        }

        public event PaintEventHandler Paint;

        protected virtual void OnPaint(PaintEventArgs e)
        {
            Paint?.Invoke(this, e);
        }

        public void Invalidate(PaintEventArgs e)
        {
            OnPaint(e);
        }

        int m_ZIndex = 0;
        public int ZIndex
        {
            get => m_ZIndex;
            set
            {
                m_ZIndex = value;
                OnZIndexChanged();
            }
        }

        public event EventHandler ZIndexChanged;

        protected virtual void OnZIndexChanged()
        {
            ZIndexChanged?.Invoke(this, new EventArgs());
        }

        public int CompareTo(LightweightControl other)
        {
            return m_ZIndex.CompareTo(other.m_ZIndex);
        }
    }

    class NodeElement : LightweightControl
    {
        public object Tag { get; set; }

        public RectangleF GetInputRectangle(int i)
        {
            var voff = HandleSize * (i * 2 + 1);
            return new RectangleF(0, voff, HandleSize, HandleSize);
        }

        public int IsOverInputHandle(PointF p)
        {
            float spacing = Height / (m_InputCount + 1);
            for (int i = 0; i < m_InputCount; i++)
            {
                var rect = GetInputRectangle(i);
                if (rect.Contains(p)) return i;
            }
            return -1;
        }

        public bool IsOverOutputHandle(PointF p)
        {
            if (!m_HasOutput) return false;
            var rect = new RectangleF(Width - m_HandleSize, Height / 2.0f - m_HandleSize / 2.0f, m_HandleSize, m_HandleSize);
            return rect.Contains(p);
        }

        void AutoResize()
        {
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var textSize = g.MeasureString(Text, Font);
                var halfHandleSize = HandleSize / 2;
                textSize += new SizeF(m_HandleSize * 4 + 2, 2);

                var n_inputs = Math.Max(m_HasOutput ? 1 : 0, m_InputCount);
                var min_height = Math.Max(textSize.Height, HandleSize * (n_inputs * 2 + 1));
                var min_width = textSize.Width;

                Size = new SizeF((float)Math.Ceiling(min_width / halfHandleSize) * halfHandleSize, (float)Math.Ceiling(min_height / halfHandleSize) * halfHandleSize);
            }
        }

        #region AutoSize
        bool m_AutoSize = true;
        public bool AutoSize
        {
            get => m_AutoSize;
            set
            {
                m_AutoSize = value;
                if (m_AutoSize)
                {
                    AutoResize();
                }
            }
        }
        #endregion

        #region Text
        string m_Text;
        public string Text
        {
            get => m_Text;
            set
            {
                m_Text = value;
                OnTextChanged();
            }
        }

        public event EventHandler TextChanged;

        protected virtual void OnTextChanged()
        {
            TextChanged?.Invoke(this, new EventArgs());
            if (m_AutoSize) AutoResize();
            OnNeedsRepaint();
        }
        #endregion

        #region Font
        Font m_Font = SystemFonts.DefaultFont;
        public Font Font
        {
            get => m_Font;
            set
            {
                m_Font = value;
                OnFontChanged();
            }
        }

        public event EventHandler FontChanged;

        protected virtual void OnFontChanged()
        {
            FontChanged?.Invoke(this, new EventArgs());
            if (m_AutoSize) AutoResize();
            OnNeedsRepaint();
        }
        #endregion

        #region BackColor
        Color m_BackColor = SystemColors.Window;
        public Color BackColor
        {
            get => m_BackColor;
            set
            {
                m_BackColor = value;
                OnBackColorChanged();
            }
        }

        public event EventHandler BackColorChanged;

        protected virtual void OnBackColorChanged()
        {
            BackColorChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region BorderColor
        Color m_BorderColor = Color.Black;
        public Color BorderColor
        {
            get => m_BorderColor;
            set
            {
                m_BorderColor = value;
                OnBorderColorChanged();
            }
        }

        public event EventHandler BorderColorChanged;

        protected virtual void OnBorderColorChanged()
        {
            BorderColorChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region ForeColor
        Color m_ForeColor = SystemColors.WindowText;
        public Color ForeColor
        {
            get => m_ForeColor;
            set
            {
                m_ForeColor = value;
                OnForeColorChanged();
            }
        }

        public event EventHandler ForeColorChanged;

        protected virtual void OnForeColorChanged()
        {
            ForeColorChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region HandleSize
        float m_HandleSize = 10;
        public float HandleSize
        {
            get => m_HandleSize;
            set
            {
                m_HandleSize = value;
                OnHandleSizeChanged();
            }
        }

        public event EventHandler HandleSizeChanged;

        protected virtual void OnHandleSizeChanged()
        {
            HandleSizeChanged?.Invoke(this, new EventArgs());
            if (m_AutoSize) AutoResize();
            OnNeedsRepaint();
        }
        #endregion

        #region InputCount
        int m_InputCount = 1;
        public int InputCount
        {
            get => m_InputCount;
            set
            {
                m_InputCount = value;
                OnInputCountChanged();
            }
        }

        public event EventHandler InputCountChanged;

        protected virtual void OnInputCountChanged()
        {
            InputCountChanged?.Invoke(this, new EventArgs());
            if (m_AutoSize) AutoResize();
            OnNeedsRepaint();
        }
        #endregion

        #region HasOutput
        bool m_HasOutput = true;
        public bool HasOutput
        {
            get => m_HasOutput;
            set
            {
                m_HasOutput = value;
                OnHasOutputChanged();
            }
        }

        public event EventHandler HasOutputChanged;

        protected virtual void OnHasOutputChanged()
        {
            HasOutputChanged?.Invoke(this, new EventArgs());
            if (m_AutoSize) AutoResize();
            OnNeedsRepaint();
        }
        #endregion

        #region Selected
        bool m_Selected;

        public bool Selected
        {
            get => m_Selected;
            set
            {
                m_Selected = value;
            }
        }

        public event EventHandler SelectedChanged;

        protected virtual void OnSelectedChanged()
        {
            SelectedChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region ErrorColor
        Color m_ErrorColor = Color.LightPink;
        public Color ErrorColor
        {
            get => m_ErrorColor;
            set
            {
                m_ErrorColor = value;
                OnErrorColorChanged();
            }
        }

        public event EventHandler ErrorColorChanged;

        protected virtual void OnErrorColorChanged()
        {
            ErrorColorChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        #region Errored
        bool m_Errored;

        public bool Errored
        {
            get => m_Errored;
            set
            {
                m_Errored = value;
                OnErroredChanged();
            }
        }

        public event EventHandler ErroredChanged;

        protected virtual void OnErroredChanged()
        {
            ErroredChanged?.Invoke(this, new EventArgs());
            OnNeedsRepaint();
        }
        #endregion

        public event EventHandler NeedsRepaint;

        protected virtual void OnNeedsRepaint()
        {
            NeedsRepaint?.Invoke(this, new EventArgs());
        }

        public override Region GetRegion()
        {
            var region = new Region();
            region.MakeEmpty();
            var backRect = new RectangleF(m_HandleSize / 2.0f, 0, Width - m_HandleSize, Height);
            backRect.Offset(Location);
            region.Union(backRect);

            float spacing = Height / (m_InputCount + 1);
            for (int i = 0; i < m_InputCount; i++)
            {
                var rect = new RectangleF(0, spacing * (i + 1) - m_HandleSize / 2.0f, m_HandleSize, m_HandleSize);
                rect.Offset(Location);
                region.Union(rect);
            }

            if (m_HasOutput)
            {
                var rect = new RectangleF(Width - m_HandleSize, (Height - m_HandleSize) / 2.0f, m_HandleSize, m_HandleSize);
                rect.Offset(Location);
                region.Union(rect);
            }
            return region;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            var backRect = new RectangleF(m_HandleSize / 2.0f, 0, Width - m_HandleSize, Height);

            using (var backBrush = new SolidBrush(m_Errored ? ErrorColor : BackColor))
            using (var borderPen = new Pen(BorderColor))
            using (var foreBrush = new SolidBrush(ForeColor))
            {
                g.FillRectangle(backBrush, backRect);
                g.DrawRectangle(borderPen, backRect.X, backRect.Y, backRect.Width, backRect.Height);

                float spacing = Height / (m_InputCount + 1);
                for (int i = 0; i < m_InputCount; i++)
                {
                    var rect = GetInputRectangle(i);
                    g.FillRectangle(backBrush, rect);
                    g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                if (m_HasOutput)
                {
                    var rect = new RectangleF(Width - m_HandleSize, (Height - m_HandleSize) / 2.0f, m_HandleSize, m_HandleSize);
                    g.FillRectangle(backBrush, rect);
                    g.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                backRect.Inflate(-(m_HandleSize + 2), -2);
                var sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                g.DrawString(m_Text, m_Font, foreBrush, backRect, sf);
            }
        }
    }
}
