﻿using MarchOfTheRays.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MarchOfTheRays
{
    class MainForm : Form
    {
        RichTextBox helpBox;
        Editor.NodeCanvas canvas;
        ContextMenuStrip canvasContextMenu;
        Core.OutputNode outputNode;
        ToolStripMenuItem paste;
        bool documentModifiedSinceLastSave = false;
        string documentPath = null;

        ToolStripItem statusLabel;

        Dictionary<Core.INode, Editor.NodeElement> elements = new Dictionary<Core.INode, Editor.NodeElement>();

        RenderingSettings settings;

        RenderForm previewForm;

        public MainForm()
        {
            Text = "March of the Rays";
            Width = 800;
            Height = 600;
            WindowState = FormWindowState.Maximized;
            SuspendLayout();

            var mainMenu = new MenuStrip();

            var fileMenu = new ToolStripMenuItem("&File");

            var menuNew = new ToolStripMenuItem("&New", Resources.NewFile, (s, e) =>
            {
                if (documentModifiedSinceLastSave)
                {
                    var res = MessageBox.Show("Save changes to the file?", "March of the Rays", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (res == DialogResult.Yes)
                    {
                        if (documentPath == null)
                        {
                            if (SaveAs()) NewDocument();
                        }
                        else
                        {
                            Save(documentPath);
                            NewDocument();
                        }
                    }
                    else if (res == DialogResult.No)
                    {
                        NewDocument();
                    }
                }
                else
                {
                    NewDocument();
                }
            });
            menuNew.ShortcutKeys = Keys.Control | Keys.N;

            var menuOpen = new ToolStripMenuItem("&Open", Resources.Open, (s, e) =>
            {
                Open();
            });
            menuOpen.ShortcutKeys = Keys.Control | Keys.O;

            var menuSave = new ToolStripMenuItem("&Save", Resources.Save, (s, e) =>
            {
                if (documentPath == null) SaveAs();
                else
                {
                    Save(documentPath);
                }
            });
            menuSave.ShortcutKeys = Keys.Control | Keys.S;

            var menuSaveAs = new ToolStripMenuItem("Save As...", Resources.SaveAs, (s, e) =>
            {
                SaveAs();
            });

            var menuExit = new ToolStripMenuItem("E&xit", Resources.Exit, (s, e) =>
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


            var editMenu = new ToolStripMenuItem("&Edit");

            var undo = new ToolStripMenuItem("&Undo", Resources.Undo, (s, e) =>
            {
                canvas.Undo();
            });
            undo.ShortcutKeys = Keys.Control | Keys.Z;
            editMenu.DropDownItems.Add(undo);

            var redo = new ToolStripMenuItem("&Redo", Resources.Redo, (s, e) =>
            {
                canvas.Redo();
            });
            redo.ShortcutKeys = Keys.Control | Keys.Y;
            editMenu.DropDownItems.Add(redo);

            editMenu.DropDownItems.Add(new ToolStripSeparator());

            var cut = new ToolStripMenuItem("Cu&t", Resources.Cut, (s, e) =>
            {
                Copy();
                canvas.DeleteElements(x => x.Selected && !(x.Tag is Core.OutputNode));
            });
            cut.ShortcutKeys = Keys.Control | Keys.X;
            cut.Enabled = false;
            editMenu.DropDownItems.Add(cut);

            var copy = new ToolStripMenuItem("&Copy", Resources.Copy, (s, e) =>
            {
                Copy();
            });
            copy.ShortcutKeys = Keys.Control | Keys.C;
            copy.Enabled = false;
            editMenu.DropDownItems.Add(copy);

            paste = new ToolStripMenuItem("&Paste", Resources.Paste, (s, e) =>
            {
                Paste();
            });
            paste.ShortcutKeys = Keys.Control | Keys.V;
            // Enable paste command only if the clipboard is not empty
            paste.Enabled = (List<(Core.INode, PointF)>)Clipboard.GetData("MarchOfTheRays") != null;
            editMenu.DropDownItems.Add(paste);

            var delete = new ToolStripMenuItem("&Delete", Resources.Delete, (s, e) =>
            {
                canvas.DeleteElements(x => x.Selected && !(x.Tag is Core.OutputNode));
            });
            delete.ShortcutKeys = Keys.Delete;
            delete.Enabled = false;
            editMenu.DropDownItems.Add(delete);

            editMenu.DropDownItems.Add(new ToolStripSeparator());

            var selectAll = new ToolStripMenuItem("Select &All", Resources.SelectAll, (s, e) =>
            {
                canvas.SelectElements(x => true);
            });
            selectAll.ShortcutKeys = Keys.Control | Keys.A;
            editMenu.DropDownItems.Add(selectAll);

            var deselect = new ToolStripMenuItem("&Deselect", null, (s, e) =>
            {
                canvas.SelectElements(x => false);
            });
            deselect.ShortcutKeys = Keys.Control | Keys.D;
            editMenu.DropDownItems.Add(deselect);

            mainMenu.Items.Add(editMenu);

            var viewMenu = new ToolStripMenuItem("&View");
            var fitToScreen = new ToolStripMenuItem("Fit to screen", Resources.ZoomToFit, (s, e) =>
            {
                canvas.FitToView(_ => true);
            });
            fitToScreen.ShortcutKeys = Keys.Control | Keys.Shift | Keys.W;
            viewMenu.DropDownItems.Add(fitToScreen);

            var fitToSelection = new ToolStripMenuItem("Fit to selection", Resources.ZoomToWidth, (s, e) =>
            {
                canvas.FitToView(x => x.Selected);
            });
            fitToSelection.ShortcutKeys = Keys.Control | Keys.W;
            viewMenu.DropDownItems.Add(fitToSelection);

            var resetZoom = new ToolStripMenuItem("Reset zoom", Resources.ZoomOriginalSize, (s, e) =>
            {
                canvas.ResetZoom();
                canvas.Center();
            });
            resetZoom.ShortcutKeys = Keys.Control | Keys.D0;
            viewMenu.DropDownItems.Add(resetZoom);

            var zoomIn = new ToolStripMenuItem("Zoom in", Resources.ZoomIn, (s, e) =>
            {
                canvas.ZoomCenter(1.1f);
            });
            zoomIn.ShortcutKeys = Keys.Control | Keys.Oemplus;
            viewMenu.DropDownItems.Add(zoomIn);

            var zoomOut = new ToolStripMenuItem("Zoom out", Resources.ZoomOut, (s, e) =>
            {
                canvas.ZoomCenter(1.0f / 1.1f);
            });
            zoomOut.ShortcutKeys = Keys.Control | Keys.OemMinus;
            viewMenu.DropDownItems.Add(zoomOut);

            mainMenu.Items.Add(viewMenu);

            var renderingMenu = new ToolStripMenuItem("&Rendering");
            var renderPreviewWindow = new ToolStripMenuItem("Show preview window", null, (s, e) =>
            {
                ShowPreviewForm();
            });
            renderPreviewWindow.ShortcutKeys = Keys.F9;
            renderingMenu.DropDownItems.Add(renderPreviewWindow);

            var livePreview = new ToolStripMenuItem("Live preview")
            {
                CheckOnClick = true,
                Checked = Settings.Default.LivePreview
            };
            livePreview.Click += (s, e) =>
            {
                Settings.Default.LivePreview = livePreview.Checked;
            };
            renderingMenu.DropDownItems.Add(livePreview);

            var renderPreview = new ToolStripMenuItem("Render preview", null, (s, e) =>
            {
                UpdatePreview();
            });
            renderPreview.ShortcutKeys = Keys.F5;
            renderingMenu.DropDownItems.Add(renderPreview);

            renderingMenu.DropDownItems.Add(new ToolStripSeparator());
            var renderSettings = new ToolStripMenuItem("Settings", Resources.Settings, (s, e) =>
            {
                var dialog = new RenderingSettingsDialog();
                dialog.Value = settings;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    settings = dialog.Value;
                    documentModifiedSinceLastSave = true;
                }
            });
            renderingMenu.DropDownItems.Add(renderSettings);

            mainMenu.Items.Add(renderingMenu);

            var statusStrip = new StatusStrip();
            statusLabel = statusStrip.Items.Add("Ready.");

            var splitContainerV = new SplitContainer();
            splitContainerV.Orientation = Orientation.Vertical;
            splitContainerV.Dock = DockStyle.Fill;
            splitContainerV.BorderStyle = BorderStyle.Fixed3D;
            Controls.Add(splitContainerV);

            var splitContainerH = new SplitContainer();
            splitContainerH.Orientation = Orientation.Horizontal;
            splitContainerH.Dock = DockStyle.Fill;
            splitContainerH.BorderStyle = BorderStyle.Fixed3D;

            helpBox = new RichTextBox();
            helpBox.ReadOnly = true;
            helpBox.Dock = DockStyle.Fill;
            helpBox.BorderStyle = BorderStyle.None;
            helpBox.Margin = new Padding(10, 10, 10, 10);

            var propertyBox = new PropertyGrid();
            propertyBox.Dock = DockStyle.Fill;

            splitContainerH.Panel1.Controls.Add(propertyBox);
            splitContainerH.Panel2.Controls.Add(helpBox);

            canvas = new Editor.NodeCanvas();
            canvas.Dock = DockStyle.Fill;

            splitContainerV.Panel1.Controls.Add(canvas);
            splitContainerV.Panel2.Controls.Add(splitContainerH);
            splitContainerV.FixedPanel = FixedPanel.Panel2;

            splitContainerV.Location = new Point(0, mainMenu.Height);

            canvasContextMenu = new ContextMenuStrip();
            canvasContextMenu.Items.Add("Float constant", null, (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.FloatConstantNode());
            });

            canvasContextMenu.Items.Add("Float3 constant", null, (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float3ConstantNode());
            });

            canvasContextMenu.Items.Add("Binary operation", null, (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.BinaryNode());
            });

            canvasContextMenu.Items.Add("Unary operation", null, (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.UnaryNode());
            });

            canvas.ContextMenuStrip = canvasContextMenu;

            canvas.EdgeAdded += (s, e) =>
            {
                var src = (Core.INode)e.Source.Tag;
                switch (e.Destination.Tag)
                {
                    case Core.IUnaryNode n: n.Input = src; break;
                    case Core.IBinaryNode n:
                        if (e.DestinationIndex == 0) n.Left = src;
                        else n.Right = src;
                        break;
                }
                documentModifiedSinceLastSave = true;
                UpdateLivePreview();
            };

            canvas.EdgeRemoved += (s, e) =>
            {
                switch (e.Destination.Tag)
                {
                    case Core.IUnaryNode n: n.Input = null; break;
                    case Core.IBinaryNode n:
                        if (e.DestinationIndex == 0) n.Left = null;
                        else n.Right = null;
                        break;
                }
                documentModifiedSinceLastSave = true;
                UpdateLivePreview();
            };

            canvas.ElementAdded += (s, e) =>
            {
                documentModifiedSinceLastSave = true;
            };

            canvas.ElementRemoved += (s, e) =>
            {
                documentModifiedSinceLastSave = true;
            };

            canvas.ElementMoved += (s, e) =>
            {
                documentModifiedSinceLastSave = true;
            };

            canvas.SelectionChanged += (s, e) =>
            {
                var selectedItems = canvas.SelectedElements.ToList();
                if (selectedItems.Count == 1)
                {
                    propertyBox.SelectedObject = selectedItems[0].Tag;

                    var name = propertyBox.SelectedObject.GetType().Name;
                    var filePath = $"HelpFiles/{name}.xml";
                    helpBox.Rtf = "";
                    if (File.Exists(filePath))
                    {
                        using (var file = File.OpenRead(filePath))
                        {
                            var doc = new Docs.Document(file);
                            helpBox.Rtf = doc.ToRtf();
                        }
                    }
                }
                else
                {
                    propertyBox.SelectedObject = null;
                    helpBox.Rtf = "";
                }

                if (selectedItems.Count == 0)
                {
                    copy.Enabled = false;
                    cut.Enabled = false;
                    delete.Enabled = false;
                }
                else
                {
                    copy.Enabled = true;
                    cut.Enabled = true;
                    delete.Enabled = true;
                }
            };

            MainMenuStrip = mainMenu;
            Controls.Add(mainMenu);
            Controls.Add(statusStrip);

            ResumeLayout();
            NewDocument();
            splitContainerV.SplitterDistance = 500;

            if (Settings.Default.PreviewWindowVisible) ShowPreviewForm();
        }

        public MainForm(string[] args) : this()
        {
            if (args.Length == 0) return;

            using (var stream = File.OpenRead(args[0]))
            {
                Deserialize(stream);
            }
            documentPath = args[0];
            documentModifiedSinceLastSave = false;
            Text = "March of the Rays - " + Path.GetFileName(documentPath);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (documentModifiedSinceLastSave)
            {
                var res = MessageBox.Show("Save changes to the file?", "March of the Rays", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    if (documentPath == null)
                    {
                        e.Cancel = !SaveAs();
                    }
                    else
                    {
                        Save(documentPath);
                        e.Cancel = false;
                    }
                }
                else if (res == DialogResult.No)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }

            Settings.Default.Save();

            base.OnFormClosing(e);
        }

        protected override void OnShown(EventArgs e)
        {
            canvas.FitToView(_ => true);
            base.OnShown(e);
        }

        void ShowPreviewForm()
        {
            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Show();
                return;
            }

            previewForm = new RenderForm();
            previewForm.Owner = this;
            previewForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            previewForm.ShowInTaskbar = false;
            previewForm.Text = "Preview";
            previewForm.Size = Settings.Default.PreviewWindowSize;
            previewForm.BackgroundImageLayout = ImageLayout.Center;
            previewForm.FormClosed += (s, e) =>
            {
                Settings.Default.PreviewWindowVisible = false;
            };
            previewForm.Resize += (s, e) =>
            {
                Settings.Default.PreviewWindowSize = previewForm.Size;
            };

            Settings.Default.PreviewWindowVisible = true;

            previewForm.Show();
        }

        void UpdateLivePreview()
        {
            if (Settings.Default.LivePreview) UpdatePreview();
        }

        async void UpdatePreview()
        {
            if (previewForm == null || previewForm.IsDisposed) return;

            var renderer = new CpuRenderer.Renderer(
                    settings.CameraPosition,
                    settings.CameraTarget,
                    settings.CameraUp,
                    settings.MaximumIterations,
                    settings.MaximumDistance,
                    settings.Epsilon,
                    settings.StepSize);

            previewForm.Loading = true;
            previewForm.Progress = 0;
            foreach (var elem in elements)
            {
                elem.Value.Errored = false;
            }
            statusLabel.Text = "Checking graph for cycles...";

            var cycles = Core.Compiler.CheckForCycles(outputNode, elements.Keys.ToList());
            if (cycles.Count > 0)
            {
                foreach (var elem in elements)
                {
                    elem.Value.Errored = cycles.Contains(elem.Key);
                }
                statusLabel.Text = "Graph contains a cycle.";
                previewForm.Loading = false;
                return;
            }

            Func<Vector3, float> func;

            try
            {
                var param = Expression.Parameter(typeof(Vector3), "pos");
                var body = outputNode.Compile(Core.NodeType.Float, param);
                var lambda = Expression.Lambda<Func<Vector3, float>>(body, param);
                func = lambda.Compile();

                float totalProgress = 0;
                float total = previewForm.ClientSize.Width * previewForm.ClientSize.Height;

                var prog = new Progress<int>();
                prog.ProgressChanged += (s1, e1) =>
                {
                    totalProgress += e1;
                    previewForm.Progress = totalProgress / total;
                };

                statusLabel.Text = "Rendering started...";
                var img = renderer.RenderImageAsync(previewForm.ClientSize.Width, previewForm.ClientSize.Height, outputNode, func, 4, System.Threading.CancellationToken.None, prog);

                if (previewForm == null || previewForm.IsDisposed) return;

                previewForm.Cursor = Cursors.WaitCursor;
                previewForm.BackgroundImage = await img;
                previewForm.Cursor = Cursors.Default;

                previewForm.Loading = false;
                statusLabel.Text = "Ready.";
            }
            catch (Core.InvalidNodeException ex)
            {
                elements[ex.Node].Errored = true;
                previewForm.Loading = false;
                statusLabel.Text = "Invalid graph elements found.";
            }
        }

        void Save(string path)
        {
            var nodes = canvas.Elements.Select(x => (x.Tag as Core.INode, x.Location)).ToList();

            using (var stream = File.Open(path, FileMode.Create))
            {
                Serialize(stream, new Document() { Nodes = nodes, Settings = settings });
            }

            documentPath = path;
            Text = "March of the Rays - " + Path.GetFileName(documentPath);
            documentModifiedSinceLastSave = false;
        }

        bool SaveAs()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "March of the Rays file|*.mtr";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Save(saveFileDialog.FileName);

                return true;
            }
            else
            {
                return false;
            }
        }

        bool Open()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "March of the Rays file|*.mtr";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (var stream = File.OpenRead(openFileDialog.FileName))
                {
                    Deserialize(stream);
                }

                documentPath = openFileDialog.FileName;
                documentModifiedSinceLastSave = false;
                Text = "March of the Rays - " + Path.GetFileName(documentPath);
                canvas.FitToView(x => true);

                return true;
            }
            else
            {
                return false;
            }
        }

        Editor.NodeElement AddNode(PointF location, Core.INode node)
        {
            if (node == null) throw new ArgumentNullException();
            var elem = CreateNode(location, node);
            canvas.AddElements(elem);
            return elem;
        }

        IEnumerable<Editor.NodeElement> AddNodes(IEnumerable<(PointF, Core.INode)> nodes)
        {
            if (nodes == null) throw new ArgumentNullException();
            var list = new List<Editor.NodeElement>();
            foreach (var (location, node) in nodes)
            {
                list.Add(CreateNode(location, node));
            }
            canvas.AddElements(list);
            return list;
        }

        Editor.NodeElement CreateNode(PointF location, Core.INode node)
        {
            if (node == null) throw new ArgumentNullException();
            Editor.NodeElement elem;
            switch (node)
            {
                case Core.UnaryNode n: elem = CreateNode(location, n); break;
                case Core.BinaryNode n: elem = CreateNode(location, n); break;
                case Core.Float3ConstantNode n: elem = CreateNode(location, n); break;
                case Core.FloatConstantNode n: elem = CreateNode(location, n); break;
                case Core.InputNode n: elem = CreateNode(location, n); break;
                case Core.OutputNode n: elem = CreateNode(location, n); break;
                default: throw new NotImplementedException();
            }
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.InputNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Location = location,
                Text = "Position",
                InputCount = 0,
                HasOutput = true,
                Tag = node
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.OutputNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Location = location,
                Text = "Output",
                InputCount = 1,
                HasOutput = false,
                Tag = node
            };

            elements.Add(node, elem);

            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.FloatConstantNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = node.Value.ToString(),
                Location = location,
                InputCount = 0,
                HasOutput = true,
                Tag = node
            };

            node.ValueChanged += (_, __) =>
            {
                elem.Text = node.Value.ToString();
                UpdateLivePreview();
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.Float2ConstantNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = $"({node.X}; {node.Y})",
                Location = location,
                InputCount = 0,
                HasOutput = true,
                Tag = node
            };

            node.ValueChanged += (_, __) =>
            {
                elem.Text = $"({node.X}; {node.Y})";
                UpdateLivePreview();
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.Float3ConstantNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = $"({node.X}; {node.Y}; {node.Z})",
                Location = location,
                InputCount = 0,
                HasOutput = true,
                Tag = node
            };

            node.ValueChanged += (_, __) =>
            {
                elem.Text = $"({node.X}; {node.Y}; {node.Z})";
                UpdateLivePreview();
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.Float4ConstantNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = $"({node.X}; {node.Y}, {node.Z}; {node.W})",
                Location = location,
                InputCount = 0,
                HasOutput = true,
                Tag = node
            };

            node.ValueChanged += (_, __) =>
            {
                elem.Text = $"({node.X}; {node.Y}; {node.Z}; {node.W})";
                UpdateLivePreview();
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.BinaryNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = node.Operation.ToString(),
                Location = location,
                InputCount = 2,
                HasOutput = true,
                Tag = node
            };

            node.OperationChanged += (_, __) =>
            {
                elem.Text = node.Operation.ToString();
                UpdateLivePreview();
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.UnaryNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = node.Operation.ToString(),
                Location = location,
                InputCount = 1,
                HasOutput = true,
                Tag = node
            };

            node.OperationChanged += (_, __) =>
            {
                elem.Text = node.Operation.ToString();
                UpdateLivePreview();
            };

            elements.Add(node, elem);
            return elem;
        }

        void Serialize(Stream stream, Document doc)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, doc);
        }

        void Deserialize(Stream stream)
        {
            var livePreviewSetting = Settings.Default.LivePreview;
            Settings.Default.LivePreview = false;
            canvas.Clear();
            elements.Clear();

            var formatter = new BinaryFormatter();
            var doc = formatter.Deserialize(stream) as Document;

            settings = doc.Settings;

            foreach (var tuple in doc.Nodes)
            {
                AddNode(tuple.Item2, tuple.Item1);
                if (tuple.Item1 is Core.OutputNode n) outputNode = n;
            }

            foreach (var tuple in doc.Nodes)
            {
                var dest = elements[tuple.Item1];
                switch (tuple.Item1)
                {
                    case Core.IUnaryNode n:
                        if (n.Input != null)
                        {
                            var source = elements[n.Input];
                            canvas.AddEdge(source, dest, 0);
                        }
                        break;
                    case Core.IBinaryNode n:
                        if (n.Left != null)
                        {
                            var source = elements[n.Left];
                            canvas.AddEdge(source, dest, 0);
                        }
                        if (n.Right != null)
                        {
                            var source = elements[n.Right];
                            canvas.AddEdge(source, dest, 1);
                        }
                        break;
                }
            }

            canvas.ResetHistory();
            Settings.Default.LivePreview = livePreviewSetting;
            UpdateLivePreview();
        }

        void NewDocument()
        {
            elements.Clear();
            canvas.Clear();
            AddNode(PointF.Empty, new Core.InputNode() { OutputType = Core.NodeType.Float3 });

            outputNode = new Core.OutputNode();
            AddNode(new PointF(200, 0), outputNode);
            canvas.ResetHistory();
            documentModifiedSinceLastSave = false;
            documentPath = null;
            settings = new RenderingSettings();
            Text = "March of the Rays";
        }

        void Copy()
        {
            var clones = new List<(Core.INode, PointF)>();
            var originalsToClones = new Dictionary<Core.INode, Core.INode>();

            Core.INode CloneElement(Editor.NodeElement element)
            {
                if (element.Tag is Core.InputNode) return null;

                var original = (Core.INode)element.Tag;
                if (originalsToClones.TryGetValue(original, out var clone))
                {
                    return clone;
                }
                else
                {
                    clone = (Core.INode)original.Clone();
                    originalsToClones.Add(original, clone);
                    clones.Add((clone, element.Location));
                    switch (clone)
                    {
                        case Core.IUnaryNode n:
                            {
                                if (n.Input == null) break;
                                var inputNode = elements[n.Input];
                                n.Input = inputNode.Selected ? CloneElement(inputNode) : null;
                            }
                            break;
                        case Core.IBinaryNode n:
                            {
                                if (n.Left != null)
                                {
                                    var leftNode = elements[n.Left];
                                    n.Left = leftNode.Selected ? CloneElement(leftNode) : null;
                                }

                                if (n.Right != null)
                                {
                                    var rightNode = elements[n.Right];
                                    n.Right = rightNode.Selected ? CloneElement(rightNode) : null;
                                }
                            }
                            break;
                    }
                    return clone;
                }
            }

            foreach (var selectedElem in canvas.SelectedElements)
            {
                if (selectedElem.Tag is Core.OutputNode) continue;
                if (selectedElem.Tag is Core.InputNode) continue;
                CloneElement(selectedElem);
            }

            Clipboard.SetData("MarchOfTheRays", clones);
            paste.Enabled = true;
        }

        void Paste()
        {
            var clipboardData = (List<(Core.INode, PointF)>)Clipboard.GetData("MarchOfTheRays");
            if (clipboardData == null) return;
            canvas.SelectElements(_ => false);

            var nodes = AddNodes(clipboardData.Select(tuple => (tuple.Item2 + new SizeF(10, 10), tuple.Item1)));
            foreach (var node in nodes)
            {
                node.Selected = true;
            }

            var edges = new List<(Editor.NodeElement, Editor.NodeElement, int)>();

            foreach (var (node, pos) in clipboardData)
            {
                var dest = elements[node];
                switch (node)
                {
                    case Core.IUnaryNode n:
                        if (n.Input != null)
                        {
                            var source = elements[n.Input];
                            edges.Add((source, dest, 0));
                        }
                        break;
                    case Core.IBinaryNode n:
                        if (n.Left != null)
                        {
                            var source = elements[n.Left];
                            edges.Add((source, dest, 0));
                        }
                        if (n.Right != null)
                        {
                            var source = elements[n.Right];
                            edges.Add((source, dest, 1));
                        }
                        break;
                }
            }
            canvas.AddEdges(edges);
            canvas.Center(clipboardData[0].Item2);
        }
    }
}
