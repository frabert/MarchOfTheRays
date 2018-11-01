using MarchOfTheRays.Properties;
using System;
using System.Linq;
using System.Windows.Forms;

namespace MarchOfTheRays
{
    partial class MainForm
    {
        void InitializeFileMenu(MenuStrip mainMenu)
        {
            var fileMenu = new ToolStripMenuItem(Strings.FileMenu);

            var menuNew = new ToolStripMenuItem(Strings.NewDocument, Resources.NewFile, (s, e) =>
            {
                NewDocument();
            });
            menuNew.ShortcutKeys = Keys.Control | Keys.N;

            var menuOpen = new ToolStripMenuItem(Strings.OpenDocument, Resources.Open, (s, e) =>
            {
                Open();
            });
            menuOpen.ShortcutKeys = Keys.Control | Keys.O;

            var menuSave = new ToolStripMenuItem(Strings.SaveDocument, Resources.Save, (s, e) =>
            {
                if (DocumentPath == null) SaveAs();
                else
                {
                    Save(DocumentPath);
                }
            });
            menuSave.ShortcutKeys = Keys.Control | Keys.S;

            var menuSaveAs = new ToolStripMenuItem(Strings.SaveDocumentAs, Resources.SaveAs, (s, e) =>
            {
                SaveAs();
            });
            menuSaveAs.ShortcutKeys = Keys.Shift | Keys.Control | Keys.S;

            var menuExit = new ToolStripMenuItem(Strings.Exit, Resources.Exit, (s, e) =>
            {
                Close();
            });
            menuExit.ShortcutKeys = Keys.Control | Keys.Q;

            fileMenu.DropDownItems.Add(menuNew);
            fileMenu.DropDownItems.Add(menuOpen);
            fileMenu.DropDownItems.Add(menuSave);
            fileMenu.DropDownItems.Add(menuSaveAs);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(menuExit);

            mainMenu.Items.Add(fileMenu);
        }

        void InitializeEditMenu(MenuStrip mainMenu)
        {
            var editMenu = new ToolStripMenuItem(Strings.EditMenu);

            var undo = new ToolStripMenuItem(Strings.Undo, Resources.Undo, (s, e) =>
            {
                ActiveEditor?.Canvas.Undo();
            });
            undo.Enabled = false;
            undo.ShortcutKeys = Keys.Control | Keys.Z;
            editMenu.DropDownItems.Add(undo);

            var redo = new ToolStripMenuItem(Strings.Redo, Resources.Redo, (s, e) =>
            {
                ActiveEditor?.Canvas.Redo();
            });
            undo.Enabled = false;
            redo.ShortcutKeys = Keys.Control | Keys.Y;
            editMenu.DropDownItems.Add(redo);

            editMenu.DropDownItems.Add(new ToolStripSeparator());

            var cut = new ToolStripMenuItem(Strings.Cut, Resources.Cut, (s, e) =>
            {
                Copy();
                ActiveEditor?.Canvas.DeleteElements(x => x.Selected && !(x.Tag is Core.OutputNode));
            });
            cut.ShortcutKeys = Keys.Control | Keys.X;
            cut.Enabled = false;
            editMenu.DropDownItems.Add(cut);

            var copy = new ToolStripMenuItem(Strings.Copy, Resources.Copy, (s, e) =>
            {
                Copy();
            });
            copy.ShortcutKeys = Keys.Control | Keys.C;
            copy.Enabled = false;
            editMenu.DropDownItems.Add(copy);

            var paste = new ToolStripMenuItem(Strings.Paste, Resources.Paste, (s, e) =>
            {
                Paste();
            });
            paste.ShortcutKeys = Keys.Control | Keys.V;
            // Enable paste command only if the clipboard is not empty
            paste.Enabled = CanPaste;
            editMenu.DropDownItems.Add(paste);

            var delete = new ToolStripMenuItem(Strings.Delete, Resources.Delete, (s, e) =>
            {
                ActiveEditor?.Canvas.DeleteElements(x => x.Selected && !(x.Tag is Core.OutputNode || x.Tag is Core.InputNode));
            });
            delete.ShortcutKeys = Keys.Delete;
            delete.Enabled = false;
            editMenu.DropDownItems.Add(delete);

            editMenu.DropDownItems.Add(new ToolStripSeparator());

            var selectAll = new ToolStripMenuItem(Strings.SelectAll, Resources.SelectAll, (s, e) =>
            {
                ActiveEditor?.Canvas.SelectElements(x => true);
            });
            selectAll.ShortcutKeys = Keys.Control | Keys.A;
            editMenu.DropDownItems.Add(selectAll);

            var deselect = new ToolStripMenuItem(Strings.Deselect, null, (s, e) =>
            {
                ActiveEditor?.Canvas.SelectElements(x => false);
            });
            deselect.ShortcutKeys = Keys.Control | Keys.D;
            deselect.Enabled = false;
            editMenu.DropDownItems.Add(deselect);

            ClipboardChanged += (s, e) =>
            {
                paste.Enabled = CanPaste;
            };
            
            void UpdateUndoRedo()
            {
                if (ActiveEditor != null)
                {
                    undo.Enabled = ActiveEditor.Canvas.CanUndo;
                    redo.Enabled = ActiveEditor.Canvas.CanRedo;
                }
                else
                {
                    undo.Enabled = false;
                    redo.Enabled = false;
                }
            }

            void UpdateSelectionTools()
            {
                var canCopy = ActiveEditor?.Canvas.SelectedElements.Count() > 0;
                copy.Enabled = canCopy;
                cut.Enabled = canCopy;
                delete.Enabled = canCopy;
                deselect.Enabled = canCopy;
            }

            SelectionChanged += (s, e) =>
            {
                UpdateSelectionTools();
            };

            void ActiveFormChanged(object sender, EventArgs e)
            {
                UpdateSelectionTools();
                UpdateUndoRedo();
            }

            dockPanel.ActiveDocumentChanged += ActiveFormChanged;
            dockPanel.ActivePaneChanged += ActiveFormChanged;

            mainMenu.Items.Add(editMenu);
        }

        void InitializeViewMenu(MenuStrip mainMenu)
        {
            var viewMenu = new ToolStripMenuItem(Strings.ViewMenu);

            var panels = new ToolStripMenuItem(Strings.Panels);
            panels.DropDownItems.Add(Strings.ShowPropertiesPanel, Resources.Property, (s, e) =>
            {
                OnShowPropertyPanel();
            });
            panels.DropDownItems.Add(Strings.ShowHelpPanel, Resources.Question, (s, e) =>
            {
                OnShowHelpPanel();
            });
            viewMenu.DropDownItems.Add(panels);

            var showMainGraph = new ToolStripMenuItem(Strings.ShowMainGraph, null, (s, e) =>
            {
                ShowGraph(document.MainGraph);
            });
            showMainGraph.ShortcutKeys = Keys.Control | Keys.M;
            viewMenu.DropDownItems.Add(showMainGraph);

            viewMenu.DropDownItems.Add(new ToolStripSeparator());

            var fitToScreen = new ToolStripMenuItem(Strings.FitToScreen, Resources.ZoomToFit, (s, e) =>
            {
                ActiveEditor?.Canvas.FitToView(_ => true);
            });
            fitToScreen.ShortcutKeys = Keys.Control | Keys.Shift | Keys.W;
            viewMenu.DropDownItems.Add(fitToScreen);

            var fitToSelection = new ToolStripMenuItem(Strings.FitToScreen, Resources.ZoomToWidth, (s, e) =>
            {
                ActiveEditor?.Canvas.FitToView(x => x.Selected);
            });
            fitToSelection.ShortcutKeys = Keys.Control | Keys.W;
            viewMenu.DropDownItems.Add(fitToSelection);

            var resetZoom = new ToolStripMenuItem(Strings.ResetZoom, Resources.ZoomOriginalSize, (s, e) =>
            {
                ActiveEditor?.Canvas.ResetZoom();
                ActiveEditor?.Canvas.Center();
            });
            resetZoom.ShortcutKeys = Keys.Control | Keys.D0;
            viewMenu.DropDownItems.Add(resetZoom);

            var zoomIn = new ToolStripMenuItem(Strings.ZoomIn, Resources.ZoomIn, (s, e) =>
            {
                ActiveEditor?.Canvas.ZoomCenter(1.1f);
            });
            zoomIn.ShortcutKeys = Keys.Control | Keys.Oemplus;
            viewMenu.DropDownItems.Add(zoomIn);

            var zoomOut = new ToolStripMenuItem(Strings.ZoomOut, Resources.ZoomOut, (s, e) =>
            {
                ActiveEditor?.Canvas.ZoomCenter(1.0f / 1.1f);
            });
            zoomOut.ShortcutKeys = Keys.Control | Keys.OemMinus;
            viewMenu.DropDownItems.Add(zoomOut);

            mainMenu.Items.Add(viewMenu);
        }

        void InitializeRenderingMenu(MenuStrip mainMenu)
        {
            var renderingMenu = new ToolStripMenuItem(Strings.RenderingMenu);
            var renderPreviewWindow = new ToolStripMenuItem(Strings.ShowPreviewWindow, Resources.Image, (s, e) =>
            {
                ShowPreviewForm();
            });
            renderPreviewWindow.ShortcutKeys = Keys.F9;
            renderingMenu.DropDownItems.Add(renderPreviewWindow);

            var livePreview = new ToolStripMenuItem(Strings.LivePreview)
            {
                CheckOnClick = true,
                Checked = Settings.Default.LivePreview
            };
            livePreview.Click += (s, e) =>
            {
                Settings.Default.LivePreview = livePreview.Checked;
            };
            renderingMenu.DropDownItems.Add(livePreview);

            var renderPreview = new ToolStripMenuItem(Strings.RenderPreview, Resources.Render, (s, e) =>
            {
                OnRenderPreview();
            });
            renderPreview.ShortcutKeys = Keys.F5;
            renderingMenu.DropDownItems.Add(renderPreview);

            renderingMenu.DropDownItems.Add(new ToolStripSeparator());
            var renderSettings = new ToolStripMenuItem(Strings.RenderSettings, Resources.Settings, (s, e) =>
            {
                var dialog = new RenderingSettingsDialog();
                dialog.Value = document.Settings;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    document.Settings = dialog.Value;
                    OnDocumentChanged();
                }
            });
            renderingMenu.DropDownItems.Add(renderSettings);

            mainMenu.Items.Add(renderingMenu);
        }

        MenuStrip InitializeMenus()
        {
            var mainMenu = new MenuStrip();

            InitializeFileMenu(mainMenu);
            InitializeEditMenu(mainMenu);
            InitializeViewMenu(mainMenu);
            InitializeRenderingMenu(mainMenu);

            return mainMenu;
        }
    }
}