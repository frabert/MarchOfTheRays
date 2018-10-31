using MarchOfTheRays.Properties;
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

namespace MarchOfTheRays
{
    class DocumentClosingEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }

    partial class MainForm : Form
    {
        RichTextBox helpBox;
        Editor.NodeCanvas canvas;
        ContextMenuStrip canvasContextMenu;
        Core.OutputNode outputNode;

        ToolStripItem statusLabel;

        Dictionary<Core.INode, Editor.NodeElement> elements = new Dictionary<Core.INode, Editor.NodeElement>();

        RenderingSettings settings;

        public MainForm()
        {
            Text = "March of the Rays";
            Width = 800;
            Height = 600;
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            SuspendLayout();

            InitializeMenus();

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
                OnGraphChanged();
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
                OnGraphChanged();
            };

            canvas.ElementAdded += (s, e) =>
            {
                OnDocumentChanged();
            };

            canvas.ElementRemoved += (s, e) =>
            {
                OnDocumentChanged();
            };

            canvas.ElementMoved += (s, e) =>
            {
                OnDocumentChanged();
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

                OnSelectionChanged();
            };

            MainMenuStrip = mainMenu;
            Controls.Add(mainMenu);
            Controls.Add(statusStrip);

            ResumeLayout();
            InitializeDocument();
            InitializeRendering();
            NewDocument();
            splitContainerV.SplitterDistance = 500;

            DocumentOpened += (s, e) =>
            {
                canvas.FitToView(x => true);
            };

            if (Settings.Default.PreviewWindowVisible) ShowPreviewForm();
        }

        public MainForm(string[] args) : this()
        {
            if (args.Length == 0) return;

            using (var stream = File.OpenRead(args[0]))
            {
                Deserialize(stream);
            }

            OnDocumentOpened(args[0]);
        }

        protected override void OnShown(EventArgs e)
        {
            canvas.FitToView(_ => true);
            base.OnShown(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Settings.Default.Save();
            base.OnFormClosed(e);
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
                OnGraphChanged();
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
                OnGraphChanged();
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
                OnGraphChanged();
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
                OnGraphChanged();
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
                OnGraphChanged();
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
                OnGraphChanged();
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
            OnGraphChanged();
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
    }
}
