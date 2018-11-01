using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace MarchOfTheRays
{
    partial class GraphEditorForm : DockContent
    {
        Editor.NodeCanvas canvas;

        public Editor.NodeCanvas Canvas => canvas;

        Graph graph;
        bool disableCanvasEvents = false;

        public Graph Graph
        {
            get => graph;

            set
            {
                graph = value;

                canvas.Clear();
                Elements.Clear();

                disableCanvasEvents = true;

                foreach(var node in value.Nodes)
                {
                    AddNode(graph.NodePositions[node], node);
                }

                var edges = new List<(Editor.NodeElement, Editor.NodeElement, int)>();

                foreach (var node in value.Nodes)
                {
                    var dest = Elements[node];
                    switch (node)
                    {
                        case Core.IUnaryNode n:
                            if (n.Input != null)
                            {
                                var source = Elements[n.Input];
                                edges.Add((source, dest, 0));
                            }
                            break;
                        case Core.IBinaryNode n:
                            if (n.Left != null)
                            {
                                var source = Elements[n.Left];
                                edges.Add((source, dest, 0));
                            }
                            if (n.Right != null)
                            {
                                var source = Elements[n.Right];
                                edges.Add((source, dest, 1));
                            }
                            break;
                    }
                }
                canvas.AddEdges(edges);

                disableCanvasEvents = false;

                canvas.ResetHistory();
            }
        }

        public Dictionary<Core.INode, Editor.NodeElement> Elements { get; } = new Dictionary<Core.INode, Editor.NodeElement>();

        public event EventHandler GraphChanged;

        protected void OnGraphChanged()
        {
            GraphChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler DocumentChanged;

        protected void OnDocumentChanged()
        {
            DocumentChanged?.Invoke(this, new EventArgs());
        }

        protected override void OnShown(EventArgs e)
        {
            Canvas.FitToView(x => true);
            base.OnShown(e);
        }

        public event EventHandler SelectionChanged;

        protected void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new EventArgs());
        }

        public GraphEditorForm()
        {
            canvas = new Editor.NodeCanvas();
            canvas.Dock = DockStyle.Fill;
            Controls.Add(canvas);

            canvas.EdgeAdded += (s, e) =>
            {
                if (disableCanvasEvents) return;

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
                if (disableCanvasEvents) return;

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
                if (disableCanvasEvents) return;

                var node = (Core.INode)e.Element.Tag;
                Graph.Nodes.Add(node);
                Graph.NodePositions.Add(node, e.Element.Location);
                OnDocumentChanged();
            };

            canvas.ElementRemoved += (s, e) =>
            {
                if (disableCanvasEvents) return;

                var node = (Core.INode)e.Element.Tag;
                Graph.Nodes.Remove(node);
                Graph.NodePositions.Remove(node);
                OnDocumentChanged();
            };

            canvas.ElementMoved += (s, e) =>
            {
                if (disableCanvasEvents) return;

                Graph.NodePositions[(Core.INode)e.Element.Tag] = e.Element.Location;
                OnDocumentChanged();
            };

            canvas.SelectionChanged += (s, e) =>
            {
                if (disableCanvasEvents) return;

                OnSelectionChanged();
            };

            InitializeContextMenu();
        }

        public Editor.NodeElement AddNode(PointF location, Core.INode node)
        {
            if (node == null) throw new ArgumentNullException();
            var elem = CreateNode(location, node);
            canvas.AddElements(elem);
            return elem;
        }

        public IEnumerable<Editor.NodeElement> AddNodes(IEnumerable<(PointF, Core.INode)> nodes)
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
            if (Elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Location = location,
                Text = "Position",
                InputCount = 0,
                HasOutput = true,
                Tag = node
            };

            Elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.OutputNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Location = location,
                Text = "Output",
                InputCount = 1,
                HasOutput = false,
                Tag = node
            };

            Elements.Add(node, elem);
            Graph.OutputNodes.Add(node);

            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.FloatConstantNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
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

            Elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.Float2ConstantNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
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

            Elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.Float3ConstantNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
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

            Elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.Float4ConstantNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
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

            Elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.BinaryNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
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

            Elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.UnaryNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
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

            Elements.Add(node, elem);
            return elem;
        }
    }
}
