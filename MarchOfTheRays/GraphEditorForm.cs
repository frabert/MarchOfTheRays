using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace MarchOfTheRays
{
    partial class GraphEditorForm : DockContent
    {
        public Editor.NodeCanvas Canvas { get; }

        Graph graph;
        bool disableCanvasEvents = false;

        public Graph Graph
        {
            get => graph;

            set
            {
                graph = value;

                Canvas.Clear();
                Elements.Clear();

                disableCanvasEvents = true;

                foreach (var node in value.Nodes)
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
                        case Core.INAryNode n:
                            {
                                for(int i = 0; i < n.InputCount; i++)
                                {
                                    if(n.GetInput(i) != null)
                                    {
                                        var source = Elements[n.GetInput(i)];
                                        edges.Add((source, dest, i));
                                    }
                                }
                            }
                            break;
                    }
                }
                Canvas.AddEdges(edges);

                disableCanvasEvents = false;

                Canvas.ResetHistory();
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
            Canvas.FitToView(x => true, true);
            base.OnShown(e);
        }

        public event EventHandler SelectionChanged;

        protected void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler<Core.INode> NodeOpened;

        protected void OnNodeOpened(Core.INode node)
        {
            NodeOpened?.Invoke(this, node);
        }

        public event EventHandler<(Core.INode node, Graph graph)> GraphCreated;

        protected void OnGraphCreated(Core.INode node, Graph g)
        {
            GraphCreated?.Invoke(this, (node, g));
        }

        public event EventHandler<(byte[] data, string name)> NodeImported;

        protected void OnNodeImported(byte[] data, string name)
        {
            NodeImported?.Invoke(this, (data, name));
        }

        static string OperationToLabel(Core.UnaryOp op)
        {
            switch(op)
            {
                case Core.UnaryOp.Abs: return "|x|";
                case Core.UnaryOp.Acos: return "cos⁻¹(x)";
                case Core.UnaryOp.Asin: return "sin⁻¹(x)";
                case Core.UnaryOp.Atan: return "tan⁻¹(x)";
                case Core.UnaryOp.Ceil: return "⌈x⌉";
                case Core.UnaryOp.Cos: return "cos(x)";
                case Core.UnaryOp.Degrees: return "rad → deg";
                case Core.UnaryOp.Exp: return "exp(x)";
                case Core.UnaryOp.Floor: return "⌊x⌋";
                case Core.UnaryOp.Invert: return "-x";
                case Core.UnaryOp.Length: return "||x\u20d1||";
                case Core.UnaryOp.Normalize: return "x\u20d1 / (||x\u20d1||)";
                case Core.UnaryOp.Radians: return "deg → rad";
                case Core.UnaryOp.Sin: return "sin(x)";
                case Core.UnaryOp.Tan: return "tan(x)";
                case Core.UnaryOp.X: return "X";
                case Core.UnaryOp.Y: return "Y";
                case Core.UnaryOp.Z: return "Z";
                default: throw new NotImplementedException();
            }
        }


        static string OperationToLabel(Core.BinaryOp op)
        {
            switch (op)
            {
                case Core.BinaryOp.Add: return "x + y";
                case Core.BinaryOp.Atan2: return "atan(x, y)";
                case Core.BinaryOp.Cross: return "x\u20d1 ⨯ y\u20d1";
                case Core.BinaryOp.Div: return "x / y";
                case Core.BinaryOp.Dot: return "⟨x\u20d1, y\u20d1⟩";
                case Core.BinaryOp.Max: return "max(x, y)";
                case Core.BinaryOp.Min: return "min(x, y)";
                case Core.BinaryOp.Mod: return "x mod y";
                case Core.BinaryOp.Mul: return "x × y";
                case Core.BinaryOp.Sub: return "x - y";
                default: throw new NotImplementedException();
            }
        }

        public GraphEditorForm()
        {
            Canvas = new Editor.NodeCanvas();
            Canvas.Dock = DockStyle.Fill;
            Controls.Add(Canvas);
            DockAreas = DockAreas.Document;

            Canvas.EdgeAdded += (s, e) =>
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
                    case Core.INAryNode n:
                        n.SetInput(e.DestinationIndex, src);
                        break;
                }
                OnGraphChanged();
            };

            Canvas.EdgeRemoved += (s, e) =>
            {
                if (disableCanvasEvents) return;

                switch (e.Destination.Tag)
                {
                    case Core.IUnaryNode n: n.Input = null; break;
                    case Core.IBinaryNode n:
                        if (e.DestinationIndex == 0) n.Left = null;
                        else n.Right = null;
                        break;
                    case Core.INAryNode n:
                        n.SetInput(e.DestinationIndex, null);
                        break;
                }
                OnGraphChanged();
            };

            Canvas.ElementAdded += (s, e) =>
            {
                if (disableCanvasEvents) return;

                var node = (Core.INode)e.Element.Tag;
                Graph.Nodes.Add(node);
                Graph.NodePositions.Add(node, e.Element.Location);
                OnDocumentChanged();
            };

            Canvas.ElementRemoved += (s, e) =>
            {
                if (disableCanvasEvents) return;

                var node = (Core.INode)e.Element.Tag;
                Graph.Nodes.Remove(node);
                Graph.NodePositions.Remove(node);
                OnDocumentChanged();
            };

            Canvas.ElementMoved += (s, e) =>
            {
                if (disableCanvasEvents) return;

                Graph.NodePositions[(Core.INode)e.Element.Tag] = e.Element.Location;
                OnDocumentChanged();
            };

            Canvas.SelectionChanged += (s, e) =>
            {
                if (disableCanvasEvents) return;

                OnSelectionChanged();
            };

            Canvas.MouseDoubleClick += (s, e) =>
            {
                var elem = Canvas.GetElementByCoords(Canvas.GetWorldCoordinates(e.Location));
                if (elem != null)
                {
                    OnNodeOpened((Core.INode)elem.Tag);
                }
            };

            InitializeContextMenu();
        }

        public Editor.NodeElement AddNode(PointF location, Core.INode node)
        {
            if (node == null) throw new ArgumentNullException();
            var elem = CreateNode(location, node);
            Canvas.AddElements(elem);
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
            Canvas.AddElements(list);
            return list;
        }

        Editor.NodeElement CreateNode(PointF location, Core.INode node)
        {
            if (node == null) throw new ArgumentNullException();
            Editor.NodeElement elem;
            switch (node)
            {
                case Core.UnaryOperationNode n: elem = CreateNode(location, n); break;
                case Core.BinaryOperationNode n: elem = CreateNode(location, n); break;
                case Core.Float3ConstantNode n: elem = CreateNode(location, n); break;
                case Core.FloatConstantNode n: elem = CreateNode(location, n); break;
                case Core.InputNode n: elem = CreateNode(location, n); break;
                case Core.OutputNode n: elem = CreateNode(location, n); break;
                case Core.ICompositeNode n: elem = CreateNode(location, n); break;
                case Core.Float3Constructor n: elem = CreateNode(location, n); break;
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
                Text = $"Input #{node.InputNumber + 1}",
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

        Editor.NodeElement CreateNode(PointF location, Core.BinaryOperationNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = OperationToLabel(node.Operation),
                Location = location,
                InputCount = 2,
                HasOutput = true,
                Tag = node
            };

            node.OperationChanged += (_, __) =>
            {
                elem.Text = OperationToLabel(node.Operation);
                OnGraphChanged();
            };

            Elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.UnaryOperationNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;
            var elem = new Editor.NodeElement()
            {
                Text = OperationToLabel(node.Operation),
                Location = location,
                InputCount = 1,
                HasOutput = true,
                Tag = node
            };

            node.OperationChanged += (_, __) =>
            {
                elem.Text = OperationToLabel(node.Operation);
                OnGraphChanged();
            };

            Elements.Add(node, elem);
            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.ICompositeNode node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;

            var elem = new Editor.NodeElement()
            {
                Text = node.Name,
                Location = location,
                InputCount = node is Core.CompositeUnaryNode ? 1 : 2,
                HasOutput = true,
                Tag = node
            };

            node.NameChanged += (_, __) =>
            {
                elem.Text = node.Name;

                OnGraphChanged();
            };

            Elements.Add(node, elem);

            return elem;
        }

        Editor.NodeElement CreateNode(PointF location, Core.Float3Constructor node)
        {
            if (node == null) throw new ArgumentNullException();
            if (Elements.TryGetValue(node, out var e)) return e;

            var elem = new Editor.NodeElement()
            {
                Text = "3D Vector",
                Location = location,
                InputCount = node.InputCount,
                HasOutput = true,
                Tag = node
            };

            Elements.Add(node, elem);
            return elem;
        }
    }
}
