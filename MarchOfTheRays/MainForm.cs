using MarchOfTheRays.Properties;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace MarchOfTheRays
{
    class DocumentClosingEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }

    partial class MainForm : Form
    {
        DockPanel dockPanel;
        Document document;

        public MainForm()
        {
            Text = "March of the Rays";
            Width = 800;
            Height = 600;
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            IsMdiContainer = true;
            SuspendLayout();

            dockPanel = new DockPanel();
            dockPanel.Dock = DockStyle.Fill;
            dockPanel.Theme = new VS2015DarkTheme();
            Controls.Add(dockPanel);

            InitializeHelpBox();
            InitializePropertyBox();

            var mainMenu = InitializeMenus();
            var statusStrip = InitializeStatusStrip();

            MainMenuStrip = mainMenu;
            Controls.Add(mainMenu);
            Controls.Add(statusStrip);

            ResumeLayout();

            InitializeDocument();
            InitializeRendering();

            if (Settings.Default.PreviewWindowVisible) ShowPreviewForm();
        }

        GraphEditorForm ActiveEditor
        {
            get
            {
                if (dockPanel.ActiveDocument == null)
                {
                    if (dockPanel.ActivePane == null)
                    {
                        return null;
                    }
                    else
                    {
                        return dockPanel.ActivePane.ActiveContent as GraphEditorForm;
                    }
                }
                else
                {
                    return (GraphEditorForm)dockPanel.ActiveDocument;
                }
            }
        }

        public MainForm(string[] args) : this()
        {
            if (args.Length > 0) Open(args[0]);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Settings.Default.Save();
            base.OnFormClosed(e);
        }

        event EventHandler ClipboardChanged;

        protected void OnClipboardChanged()
        {
            ClipboardChanged?.Invoke(this, new EventArgs());
        }

        event EventHandler GraphChanged;

        protected void OnGraphChanged()
        {
            GraphChanged?.Invoke(this, new EventArgs());
        }

        event EventHandler SelectionChanged;

        protected void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new EventArgs());
        }

        event EventHandler DocumentChanged;

        protected void OnDocumentChanged()
        {
            DocumentChanged?.Invoke(this, new EventArgs());
        }

        event EventHandler<string> DocumentSaved;

        protected void OnDocumentSaved(string path)
        {
            DocumentSaved?.Invoke(this, path);
        }

        event EventHandler<string> DocumentOpened;

        protected void OnDocumentOpened(string path)
        {
            OnDocumentChanged();
            OnGraphChanged();
            OnSelectionChanged();

            DocumentOpened?.Invoke(this, path);
        }

        event EventHandler<DocumentClosingEventArgs> DocumentClosing;

        protected void OnDocumentClosing(DocumentClosingEventArgs e)
        {
            DocumentClosing?.Invoke(this, e);
        }

        event EventHandler RenderPreview;

        protected void OnRenderPreview()
        {
            RenderPreview?.Invoke(this, new EventArgs());
        }

        event EventHandler<string> StatusChange;

        protected void OnStatusChange(string newStatus)
        {
            StatusChange?.Invoke(this, newStatus);
        }

        event EventHandler ShowPropertyPanel;

        protected void OnShowPropertyPanel()
        {
            ShowPropertyPanel?.Invoke(this, new EventArgs());
        }

        event EventHandler ShowHelpPanel;

        protected void OnShowHelpPanel()
        {
            ShowHelpPanel?.Invoke(this, new EventArgs());
        }

        event EventHandler<(byte[] data, string name)> NodeImported;

        protected void OnNodeImported(byte[] data, string name)
        {
            NodeImported?.Invoke(this, (data, name));
        }
    }
}
