﻿using MarchOfTheRays.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
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
        Dictionary<Graph, GraphEditorForm> graphForms = new Dictionary<Graph, GraphEditorForm>();

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

            document = new Document();
            document.MainGraph = new Graph();
            document.Graphs.Add(document.MainGraph);

            
            InitializeDocument();
            InitializeRendering();

            if (Settings.Default.PreviewWindowVisible) ShowPreviewForm();
        }

        public MainForm(string[] args) : this()
        {
            // TODO: Open document at startup
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
    }
}
