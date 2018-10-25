using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace MarchOfTheRays
{
    class MainForm : Form
    {
        RichTextBox helpBox;
        Editor.NodeCanvas canvas;
        ContextMenu canvasContextMenu;
        PictureBox renderBox;
        Core.OutputNode outputNode;

        Dictionary<Core.INode, Editor.NodeElement> elements = new Dictionary<Core.INode, Editor.NodeElement>();

        public MainForm()
        {
            Text = "March of the Rays";
            Width = 800;
            Height = 600;
            WindowState = FormWindowState.Maximized;
            SuspendLayout();

            var mainMenu = new MenuStrip();

            {
                {
                    var fileMenu = new ToolStripMenuItem("&File");

                    var menuNew = new ToolStripMenuItem("&New", null, (s, e) =>
                    {
                        var res = MessageBox.Show("Save changes to the file?", "March of the Rays", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (res == DialogResult.Yes)
                        {
                            if (Save()) NewDocument();
                        }
                        else if (res == DialogResult.No)
                        {
                            NewDocument();
                        }
                    });
                    menuNew.ShortcutKeys = Keys.Control | Keys.N;

                    var menuOpen = new ToolStripMenuItem("&Open", null, (s, e) =>
                    {
                        Open();
                    });
                    menuOpen.ShortcutKeys = Keys.Control | Keys.O;

                    var menuSave = new ToolStripMenuItem("&Save", null, (s, e) =>
                    {
                        Save();
                    });
                    menuSave.ShortcutKeys = Keys.Control | Keys.S;

                    var menuExit = new ToolStripMenuItem("E&xit", null, (s, e) =>
                    {
                        Close();
                    });
                    menuExit.ShortcutKeys = Keys.Control | Keys.Q;

                    fileMenu.DropDownItems.Add(menuNew);
                    fileMenu.DropDownItems.Add(menuOpen);
                    fileMenu.DropDownItems.Add(menuSave);
                    fileMenu.DropDownItems.Add(new ToolStripSeparator());
                    fileMenu.DropDownItems.Add(menuExit);

                    mainMenu.Items.Add(fileMenu);
                }

                {
                    var editMenu = new ToolStripMenuItem("&Edit");

                    var undo = new ToolStripMenuItem("&Undo", null, (s, e) =>
                    {
                        canvas.Undo();
                    });
                    undo.ShortcutKeys = Keys.Control | Keys.Z;
                    editMenu.DropDownItems.Add(undo);

                    var redo = new ToolStripMenuItem("&Redo", null, (s, e) =>
                    {
                        canvas.Redo();
                    });
                    redo.ShortcutKeys = Keys.Control | Keys.Y;
                    editMenu.DropDownItems.Add(redo);

                    editMenu.DropDownItems.Add(new ToolStripSeparator());

                    var cut = new ToolStripMenuItem("Cu&t", null, (s, e) =>
                    {
                        Copy();
                        canvas.DeleteElements(x => x.Selected && !(x.Tag is Core.OutputNode));
                    });
                    cut.ShortcutKeys = Keys.Control | Keys.X;
                    editMenu.DropDownItems.Add(cut);

                    var copy = new ToolStripMenuItem("&Copy", null, (s, e) =>
                    {
                        Copy();
                    });
                    copy.ShortcutKeys = Keys.Control | Keys.C;
                    editMenu.DropDownItems.Add(copy);

                    var paste = new ToolStripMenuItem("&Paste", null, (s, e) =>
                    {
                        Paste();
                    });
                    paste.ShortcutKeys = Keys.Control | Keys.V;
                    editMenu.DropDownItems.Add(paste);

                    var delete = new ToolStripMenuItem("&Delete", null, (s, e) =>
                    {
                        canvas.DeleteElements(x => x.Selected && !(x.Tag is Core.OutputNode));
                    });
                    delete.ShortcutKeys = Keys.Delete;
                    editMenu.DropDownItems.Add(delete);

                    editMenu.DropDownItems.Add(new ToolStripSeparator());

                    var selectAll = new ToolStripMenuItem("Select &All", null, (s, e) =>
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
                }

                {
                    var viewMenu = new ToolStripMenuItem("&View");
                    var fitToScreen = new ToolStripMenuItem("Fit to screen", null, (s, e) =>
                    {
                        canvas.FitToView(_ => true);
                    });
                    fitToScreen.ShortcutKeys = Keys.Control | Keys.Shift | Keys.W;
                    viewMenu.DropDownItems.Add(fitToScreen);

                    var fitToSelection = new ToolStripMenuItem("Fit to selection", null, (s, e) =>
                    {
                        canvas.FitToView(x => x.Selected);
                    });
                    fitToSelection.ShortcutKeys = Keys.Control | Keys.W;
                    viewMenu.DropDownItems.Add(fitToSelection);

                    var resetZoom = new ToolStripMenuItem("Reset zoom", null, (s, e) =>
                    {
                        canvas.ResetZoom();
                        canvas.Center();
                    });
                    resetZoom.ShortcutKeys = Keys.Control | Keys.D0;
                    viewMenu.DropDownItems.Add(resetZoom);

                    var zoomIn = new ToolStripMenuItem("Zoom in", null, (s, e) =>
                    {
                        canvas.ZoomCenter(1.1f);
                    });
                    zoomIn.ShortcutKeys = Keys.Control | Keys.Oemplus;
                    viewMenu.DropDownItems.Add(zoomIn);

                    var zoomOut = new ToolStripMenuItem("Zoom out", null, (s, e) =>
                    {
                        canvas.ZoomCenter(1.0f / 1.1f);
                    });
                    zoomOut.ShortcutKeys = Keys.Control | Keys.OemMinus;
                    viewMenu.DropDownItems.Add(zoomOut);

                    mainMenu.Items.Add(viewMenu);
                }
            }

            var statusStrip = new StatusStrip();

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

            var propertyBox = new PropertyGrid();
            propertyBox.Dock = DockStyle.Fill;

            var tabContainer = new TabControl();
            tabContainer.Dock = DockStyle.Fill;
            tabContainer.TabPages.Add(new TabPage()
            {
                Text = "Configuration",
                Controls = { propertyBox }
            });
            tabContainer.TabPages.Add(new TabPage()
            {
                Text = "Help",
                Controls = { helpBox }
            });

            renderBox = new PictureBox();
            renderBox.Dock = DockStyle.Fill;

            splitContainerH.Panel1.Controls.Add(renderBox);
            splitContainerH.Panel2.Controls.Add(tabContainer);

            canvas = new Editor.NodeCanvas();
            canvas.Dock = DockStyle.Fill;

            splitContainerV.Panel1.Controls.Add(canvas);
            splitContainerV.Panel2.Controls.Add(splitContainerH);
            splitContainerV.FixedPanel = FixedPanel.Panel2;

            splitContainerV.Location = new Point(0, mainMenu.Height);

            canvasContextMenu = new ContextMenu();
            canvasContextMenu.MenuItems.Add("Float constant", (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.FloatConstantNode());
            });

            canvasContextMenu.MenuItems.Add("Float3 constant", (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float3ConstantNode());
            });

            canvasContextMenu.MenuItems.Add("Arithmetic operation", (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.ArithmeticNode());
            });

            canvasContextMenu.MenuItems.Add("Min/max operation", (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.MinMaxNode());
            });

            canvasContextMenu.MenuItems.Add("Length operation", (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.LengthNode());
            });

            canvasContextMenu.MenuItems.Add("Abs operation", (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.AbsNode());
            });

            canvas.ContextMenu = canvasContextMenu;

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
            };

            canvas.SelectionChanged += (s, e) =>
            {
                var selectedItems = canvas.SelectedElements.ToList();
                if (selectedItems.Count == 1)
                {
                    propertyBox.SelectedObject = selectedItems[0].Tag;
                }
                else
                {
                    propertyBox.SelectedObject = null;
                }
            };

            var renderer = new CpuRenderer.Renderer();
            renderBox.MouseClick += async (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;

                var img = renderer.RenderImageAsync(renderBox.Width, renderBox.Height, outputNode, 4);

                renderBox.Cursor = Cursors.WaitCursor;
                renderBox.Image = await img;
                renderBox.Cursor = Cursors.Default;
            };

            MainMenuStrip = mainMenu;
            Controls.Add(mainMenu);
            Controls.Add(statusStrip);

            ResumeLayout();
            splitContainerV.SplitterDistance = 500;
            NewDocument();
        }

        public MainForm(string[] args) : this()
        {
            if (args.Length == 0) return;

            using (var stream = File.OpenRead(args[0]))
            {
                Deserialize(stream);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            canvas.FitToView(_ => true);
            base.OnLoad(e);
        }

        bool Save()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "March of the Rays file|*.mtr";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var nodes = canvas.Elements.Select(x => (x.Tag as Core.INode, x.Location)).ToList();
                using (var stream = File.Open(saveFileDialog.FileName, FileMode.Create))
                {
                    Serialize(stream, nodes);
                }
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
                case Core.AbsNode n: elem = CreateNode(location, n); break;
                case Core.ArithmeticNode n: elem = CreateNode(location, n); break;
                case Core.Float3ConstantNode n: elem = CreateNode(location, n); break;
                case Core.FloatConstantNode n: elem = CreateNode(location, n); break;
                case Core.InputNode n: elem = CreateNode(location, n); break;
                case Core.LengthNode n: elem = CreateNode(location, n); break;
                case Core.MinMaxNode n: elem = CreateNode(location, n); break;
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
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.ArithmeticNode node)
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
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.MinMaxNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = node.IsMin ? "Min" : "Max",
                Location = location,
                InputCount = 2,
                HasOutput = true,
                Tag = node
            };

            node.IsMinChanged += (_, __) =>
            {
                elem.Text = node.IsMin ? "Min" : "Max";
            };

            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.LengthNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = "Length",
                Location = location,
                InputCount = 1,
                HasOutput = true,
                Tag = node
            };
            elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.AbsNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = "Abs",
                Location = location,
                InputCount = 1,
                HasOutput = true,
                Tag = node
            };
            elements.Add(node, elem);
            return elem;
        }

        void Serialize(Stream stream, List<(Core.INode, PointF)> nodes)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, nodes);
        }

        void Deserialize(Stream stream)

        {
            canvas.Clear();
            elements.Clear();

            var formatter = new BinaryFormatter();
            var nodes = formatter.Deserialize(stream) as List<(Core.INode, PointF)>;
            foreach (var tuple in nodes)
            {
                AddNode(tuple.Item2, tuple.Item1);
                if (tuple.Item1 is Core.OutputNode n) outputNode = n;
            }

            foreach (var tuple in nodes)
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
        }

        void NewDocument()
        {
            elements.Clear();
            canvas.Clear();
            AddNode(PointF.Empty, new Core.InputNode() { OutputType = Core.NodeType.Float3 });

            outputNode = new Core.OutputNode();
            AddNode(new PointF(200, 0), outputNode);
            canvas.ResetHistory();
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
                                if(n.Left != null)
                                {
                                    var leftNode = elements[n.Left];
                                    n.Left = leftNode.Selected ? CloneElement(leftNode) : null;
                                }

                                if(n.Right != null)
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
        }

        void Paste()
        {
            var clipboardData = (List<(Core.INode, PointF)>)Clipboard.GetData("MarchOfTheRays");
            canvas.SelectElements(_ => false);

            var nodes = AddNodes(clipboardData.Select(tuple => (tuple.Item2 + new SizeF(10, 10), tuple.Item1)));
            foreach(var node in nodes)
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
