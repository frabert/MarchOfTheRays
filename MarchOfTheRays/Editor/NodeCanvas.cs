using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MarchOfTheRays.Editor
{
    /// <summary>
    /// Provides data for the <see cref="NodeCanvas.EdgeAdded"/> and <see cref="NodeCanvas.EdgeRemoved"/> events
    /// </summary>
    class EdgeModifiedEventArgs : EventArgs
    {
        public EdgeModifiedEventArgs(NodeElement source, NodeElement destination, int index)
        {
            Source = source;
            Destination = destination;
            DestinationIndex = index;
        }

        /// <summary>
        /// The source element of the edge
        /// </summary>
        public NodeElement Source { get; private set; }

        /// <summary>
        /// The destination element of the edge
        /// </summary>
        public NodeElement Destination { get; private set; }

        /// <summary>
        /// The input index of the destination element
        /// </summary>
        public int DestinationIndex { get; private set; }
    }

    /// <summary>
    /// Provides data for the <see cref="NodeCanvas.ElementAdded"/>, <see cref="NodeCanvas.ElementMoved"/> and <see cref="NodeCanvas.ElementRemoved"/> events
    /// </summary>
    class ElementModifiedEventArgs : EventArgs
    {
        public ElementModifiedEventArgs(NodeElement elem)
        {
            Element = elem;
        }

        public NodeElement Element { get; private set; }
    }

    /// <summary>
    /// Stores a graph of <see cref="NodeElement"/>
    /// </summary>
    class EdgeDictionary
    {
        Dictionary<NodeElement, HashSet<(NodeElement destination, int index)>> forwardStars;
        Dictionary<NodeElement, HashSet<(NodeElement source, int index)>> backwardStars;
        Dictionary<(NodeElement destination, int index), NodeElement> inwardNodes;

        public EdgeDictionary()
        {
            forwardStars = new Dictionary<NodeElement, HashSet<(NodeElement, int)>>();
            backwardStars = new Dictionary<NodeElement, HashSet<(NodeElement, int)>>();
            inwardNodes = new Dictionary<(NodeElement, int), NodeElement>();
        }

        public void Clear()
        {
            forwardStars.Clear();
            backwardStars.Clear();
            inwardNodes.Clear();
        }

        /// <summary>
        /// Returns an enumeration of edges that start from <paramref name="source"/>
        /// </summary>
        /// <param name="source">The element of which to enumerate the outward edges</param>
        /// <returns>An enumeration of the outward edges of <paramref name="source"/></returns>
        public IEnumerable<(NodeElement destination, int index)> ForwardStar(NodeElement source)
        {
            if (forwardStars.TryGetValue(source, out var list))
            {
                foreach (var e in list) yield return e;
            }
        }

        /// <summary>
        /// Returns an enumerations of edges that have <paramref name="destination"/> as their destination
        /// </summary>
        /// <param name="destination">The element of which to enumerate the inward edges</param>
        /// <returns>An enumeration of the inward nodes of <paramref name="destination"/></returns>
        public IEnumerable<(NodeElement source, int index)> BackwardStar(NodeElement destination)
        {
            if (backwardStars.TryGetValue(destination, out var list))
            {
                foreach (var e in list) yield return e;
            }
        }

        /// <summary>
        /// Returns a list of the elements that have an edge that start from them and end on <paramref name="destination"/> at <paramref name="index"/>
        /// </summary>
        /// <param name="destination">The element on which the edges must end</param>
        /// <param name="index">The index on which the edges must end</param>
        /// <returns>An enumeration of the incident nodes on at the specified element and index</returns>
        public NodeElement IncidentNode(NodeElement destination, int index)
        {
            if (inwardNodes.TryGetValue((destination, index), out var e))
            {
                return e;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Adds an element to the graph
        /// </summary>
        /// <param name="elem">The element to be added to the graph</param>
        public void AddElement(NodeElement elem)
        {
            forwardStars.Add(elem, new HashSet<(NodeElement, int)>());
            backwardStars.Add(elem, new HashSet<(NodeElement, int)>());
        }

        /// <summary>
        /// Removes an element (and all of its outward and inward edges) from the graph
        /// </summary>
        /// <param name="elem">The element to be removed from the graph</param>
        public void RemoveElement(NodeElement elem)
        {
            foreach (var (destination, index) in forwardStars[elem])
            {
                if (inwardNodes.Remove((destination, index)))
                    EdgeRemoved?.Invoke(this, new EdgeModifiedEventArgs(elem, destination, index));
            }
            forwardStars.Remove(elem);
            foreach (var (source, index) in backwardStars[elem])
            {
                if (inwardNodes.Remove((elem, index)))
                    EdgeRemoved?.Invoke(this, new EdgeModifiedEventArgs(source, elem, index));
            }
            backwardStars.Remove(elem);
        }

        /// <summary>
        /// Adds an edge to the graph
        /// </summary>
        /// <param name="source">The starting element</param>
        /// <param name="destination">The end element</param>
        /// <param name="index">The index of the end element's input</param>
        public void AddEdge(NodeElement source, NodeElement destination, int index)
        {
            forwardStars[source].Add((destination, index));
            backwardStars[destination].Add((source, index));
            inwardNodes.Add((destination, index), source);

            EdgeAdded?.Invoke(this, new EdgeModifiedEventArgs(source, destination, index));
        }

        /// <summary>
        /// Removes an edge from the graph
        /// </summary>
        /// <param name="destination">The destination of the edge to remove</param>
        /// <param name="index">The index of <paramref name="destination"/>'s input</param>
        public void RemoveEdge(NodeElement destination, int index)
        {
            var source = inwardNodes[(destination, index)];
            forwardStars[source].Remove((destination, index));
            backwardStars[destination].Remove((source, index));
            inwardNodes.Remove((destination, index));

            EdgeRemoved?.Invoke(this, new EdgeModifiedEventArgs(source, destination, index));
        }

        /// <summary>
        /// Enumerates all the edges in the graph
        /// </summary>
        public IEnumerable<(NodeElement source, NodeElement destination, int index)> Edges
        {
            get
            {
                foreach (var e in inwardNodes)
                {
                    yield return (e.Value, e.Key.destination, e.Key.index);
                }
            }
        }

        /// <summary>
        /// Called whenever an edge is added to the graph
        /// </summary>
        public event EventHandler<EdgeModifiedEventArgs> EdgeAdded;

        /// <summary>
        /// Called whenever an edge is removed from the graph
        /// </summary>
        public event EventHandler<EdgeModifiedEventArgs> EdgeRemoved;
    }

    /// <summary>
    /// A control used to draw a graph structure via nodes
    /// </summary>
    class NodeCanvas : UserControl
    {
        #region Commands
        class AddElementsCommand : ICommand
        {
            private NodeCanvas canvas;
            private IList<NodeElement> elems;
            private List<(NodeElement source, NodeElement destination, int index)> edges;

            public AddElementsCommand(NodeCanvas canvas, IList<NodeElement> elems)
            {
                this.canvas = canvas;
                this.elems = elems;
                edges = new List<(NodeElement, NodeElement, int)>();
            }

            public void Execute()
            {
                var maxZindex = canvas.elements.Select(x => x.ZIndex).DefaultIfEmpty().Max();
                foreach (var elem in elems)
                {
                    elem.ZIndex = ++maxZindex;
                    elem.NeedsRepaint += canvas.element_NeedsRepaint;
                    canvas.elements.Add(elem);
                    canvas.edges.AddElement(elem);
                    canvas.OnElementAdded(new ElementModifiedEventArgs(elem));
                }

                foreach (var edge in edges)
                {
                    canvas.edges.AddEdge(edge.source, edge.destination, edge.index);
                }
            }

            public void Undo()
            {
                foreach (var elem in elems)
                {
                    foreach (var bs in canvas.edges.BackwardStar(elem))
                    {
                        var tuple = (bs.source, elem, bs.index);
                        if (!edges.Contains(tuple)) edges.Add(tuple);
                    }

                    foreach (var fs in canvas.edges.ForwardStar(elem))
                    {
                        var tuple = (elem, fs.destination, fs.index);
                        if (!edges.Contains(tuple)) edges.Add(tuple);
                    }
                    canvas.elements.Remove(elem);
                    canvas.edges.RemoveElement(elem);
                    canvas.OnElementRemoved(new ElementModifiedEventArgs(elem));
                }
            }
        }

        class AddEdgeCommand : ICommand
        {
            private NodeCanvas canvas;
            private NodeElement source;
            private NodeElement destination;
            private int index;

            public AddEdgeCommand(NodeCanvas canvas, NodeElement source, NodeElement destination, int index)
            {
                this.canvas = canvas;
                this.source = source;
                this.destination = destination;
                this.index = index;
            }

            public void Execute()
            {
                canvas.edges.AddEdge(source, destination, index);
                canvas.OnEdgeAdded(new EdgeModifiedEventArgs(source, destination, index));
            }

            public void Undo()
            {
                canvas.edges.RemoveEdge(destination, index);
                canvas.OnEdgeRemoved(new EdgeModifiedEventArgs(source, destination, index));
            }
        }

        class SelectCommand : ICommand
        {
            private NodeCanvas canvas;
            private IList<NodeElement> elems;
            private Dictionary<NodeElement, bool> selectionState;

            public SelectCommand(NodeCanvas canvas, IList<NodeElement> elems)
            {
                this.canvas = canvas;
                this.elems = elems;
                this.selectionState = new Dictionary<NodeElement, bool>();
                foreach (var elem in canvas.elements)
                {
                    selectionState.Add(elem, elem.Selected);
                }
            }

            public void Execute()
            {
                foreach (var elem in canvas.elements)
                {
                    elem.Selected = false;
                }
                var maxZind = canvas.elements.Select(x => x.ZIndex).DefaultIfEmpty().Max();

                foreach (var elem in elems.OrderByDescending(x => x.ZIndex))
                {
                    elem.ZIndex = ++maxZind;
                    elem.Selected = true;
                }
                canvas.OnSelectionChanged();
            }

            public void Undo()
            {
                foreach (var elem in canvas.elements)
                {
                    elem.Selected = selectionState[elem];
                }
                canvas.OnSelectionChanged();
            }
        }

        class MoveElementsCommand : ICommand
        {
            private NodeCanvas canvas;
            private IList<NodeElement> elems;
            private SizeF amount;

            public MoveElementsCommand(NodeCanvas canvas, IList<NodeElement> elems, SizeF amount)
            {
                this.canvas = canvas;
                this.elems = elems;
                this.amount = amount;
            }

            public void Execute()
            {
                foreach (var elem in elems)
                {
                    elem.Location += amount;
                    canvas.OnElementMoved(new ElementModifiedEventArgs(elem));
                }
            }

            public void Undo()
            {
                foreach (var elem in elems)
                {
                    elem.Location -= amount;
                    canvas.OnElementMoved(new ElementModifiedEventArgs(elem));
                }
            }
        }
        #endregion
        HashSet<NodeElement> elements = new HashSet<NodeElement>();
        EdgeDictionary edges = new EdgeDictionary();

        CommandList commands = new CommandList();

        WorldViewMatrix wvMatrix = new WorldViewMatrix();
        float currentScale = 1.0f;

        bool dragging = false;
        bool draggingEdge = false;
        bool selecting = false;
        SizeF moveDistance = SizeF.Empty;
        NodeElement draggedElem;
        PointF mouseCoords;
        PointF startMouseCoords;
        PointF lastSnap;

        public NodeCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            edges.EdgeAdded += (s, e) =>
            {
                OnEdgeAdded(e);
            };

            edges.EdgeRemoved += (s, e) =>
            {
                OnEdgeRemoved(e);
            };
        }

        static float lerp(float a, float b, float x)
        {
            return (1 - x) * a + x * b;
        }

        static PointF lerp(PointF a, PointF b, float x)
        {
            return new PointF(lerp(a.X, b.X, x), lerp(a.Y, b.Y, x));
        }

        static RectangleF RectFromPoints(PointF a, PointF b)
        {
            return new RectangleF(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }

        RectangleF ViewRegionWorld
        {
            get
            {
                var points = new PointF[] { PointF.Empty, new PointF(Width, Height) };
                wvMatrix.ViewWorld.TransformPoints(points);

                return RectFromPoints(points[0], points[1]);
            }
        }

        public void Clear()
        {
            elements.Clear();
            edges.Clear();
            commands.Clear();
            Invalidate();
        }

        public void AddElements(params NodeElement[] e)
        {
            var cmd = new AddElementsCommand(this, e);
            cmd.Execute();
            commands.Add(cmd);
        }

        public void AddElements(IList<NodeElement> e)
        {
            var cmd = new AddElementsCommand(this, e);
            cmd.Execute();
            commands.Add(cmd);
        }

        public void RemoveElements(params NodeElement[] e)
        {
            var cmd = new InverseCommand(new AddElementsCommand(this, e));
            cmd.Execute();
            commands.Add(cmd);
        }

        public void AddEdge(NodeElement source, NodeElement destination, int index)
        {
            if (edges.IncidentNode(destination, index) != null) return;
            var cmd = new AddEdgeCommand(this, source, destination, index);
            cmd.Execute();
            commands.Add(cmd);
        }

        public void AddEdges(IList<(NodeElement source, NodeElement destination, int index)> e)
        {
            var list = new List<ICommand>();
            foreach (var edge in e)
            {
                if (edges.IncidentNode(edge.destination, edge.index) != null) continue;
                var cmd = new AddEdgeCommand(this, edge.source, edge.destination, edge.index);
                list.Add(cmd);
            }

            var aggregate = new AggregateCommand(list);
            aggregate.Execute();
            commands.Add(aggregate);
        }

        public void RemoveEdge(NodeElement destination, int index)
        {
            var e = edges.IncidentNode(destination, index);
            var cmd = new InverseCommand(new AddEdgeCommand(this, e, destination, index));
            cmd.Execute();
            commands.Add(cmd);
        }

        public IReadOnlyCollection<NodeElement> Elements => elements;

        private void element_NeedsRepaint(object sender, EventArgs e)
        {
            Invalidate();
        }

        (float, float) CalcGridSize(float initialGridSize = 5)
        {
            float gridSize = initialGridSize;
            var a = wvMatrix.TransformWV(PointF.Empty);
            var b = wvMatrix.TransformWV(new PointF(initialGridSize, 0));

            var dist = Math.Abs(a.X - b.X);
            
            while(dist < initialGridSize)
            {
                dist *= 2;
                gridSize *= 2;
            }

            return (dist, gridSize);
        }

        PointF SnapToGrid(PointF worldCoords)
        {
            (float dist, float gridSize) = CalcGridSize();

            return new PointF(
                (float)Math.Round(worldCoords.X / gridSize) * gridSize,
                (float)Math.Round(worldCoords.Y / gridSize) * gridSize
                );
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            startMouseCoords = e.Location;
            lastSnap = SnapToGrid(wvMatrix.TransformVW(e.Location));
            if (e.Button == MouseButtons.Left)
            {
                mouseCoords = e.Location;
                selecting = true;
                var coords = wvMatrix.TransformVW(mouseCoords);

                foreach (var elem in elements.OrderByDescending(x => x.ZIndex))
                {
                    var localCoords = coords - new SizeF(elem.Location);
                    var inputHandle = elem.IsOverInputHandle(localCoords);
                    if (elem.IsOverOutputHandle(localCoords))
                    {
                        selecting = false;
                        dragging = true;
                        draggedElem = elem;
                        draggingEdge = true;
                        break;
                    }
                    else if (inputHandle >= 0)
                    {
                        selecting = false;
                        dragging = true;
                        var source = edges.IncidentNode(elem, inputHandle);
                        if (source != null)
                        {
                            draggedElem = source;
                            draggingEdge = true;
                            RemoveEdge(elem, inputHandle);
                            break;
                        }
                    }

                    var clipRegion = new RectangleF(elem.Location, elem.Size);
                    if (clipRegion.Contains(coords))
                    {
                        if (!elem.Selected)
                        {
                            var cmd = new SelectCommand(this, new[] { elem });
                            cmd.Execute();
                            commands.Add(cmd);
                        }
                        selecting = false;
                        dragging = true;
                        draggedElem = elem;
                        break;
                    }
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                dragging = true;
                mouseCoords = e.Location;
            }
            base.OnMouseDown(e);
        }

        public event EventHandler<EdgeModifiedEventArgs> EdgeAdded;
        public event EventHandler<EdgeModifiedEventArgs> EdgeRemoved;
        public event EventHandler<ElementModifiedEventArgs> ElementAdded;
        public event EventHandler<ElementModifiedEventArgs> ElementRemoved;
        public event EventHandler<ElementModifiedEventArgs> ElementMoved;
        public event EventHandler SelectionChanged;

        protected virtual void OnEdgeAdded(EdgeModifiedEventArgs e)
        {
            EdgeAdded?.Invoke(this, e);
            Invalidate();
        }

        protected virtual void OnEdgeRemoved(EdgeModifiedEventArgs e)
        {
            EdgeRemoved?.Invoke(this, e);
            Invalidate();
        }

        protected virtual void OnElementAdded(ElementModifiedEventArgs e)
        {
            ElementAdded?.Invoke(this, e);
            Invalidate();
        }

        protected virtual void OnElementRemoved(ElementModifiedEventArgs e)
        {
            ElementRemoved?.Invoke(this, e);
            Invalidate();
        }

        protected virtual void OnElementMoved(ElementModifiedEventArgs e)
        {
            ElementMoved?.Invoke(this, e);
            Invalidate();
        }

        protected virtual void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new EventArgs());
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (draggingEdge)
            {
                var coords = wvMatrix.TransformVW(mouseCoords);
                foreach (var elem in elements)
                {
                    var localCoords = coords - new SizeF(elem.Location);
                    var i = elem.IsOverInputHandle(localCoords);
                    if (i >= 0)
                    {
                        AddEdge(draggedElem, elem, i);
                    }
                }
            }

            if (dragging)
            {
                if (moveDistance.Width != 0 || moveDistance.Height != 0)
                {
                    var cmd = new MoveElementsCommand(this, SelectedElements.ToList(), moveDistance);
                    commands.Add(cmd);

                    foreach (var elem in SelectedElements)
                    {
                        OnElementMoved(new ElementModifiedEventArgs(elem));
                    }
                }
            }

            if (selecting)
            {
                var selection = elements.Where(x =>
                {
                    var origin = wvMatrix.TransformVW(startMouseCoords);
                    var end = wvMatrix.TransformVW(mouseCoords);

                    var rect = RectFromPoints(origin, end);

                    var clipRegion = new RectangleF(x.Location, x.Size);
                    return clipRegion.IntersectsWith(rect);
                }).ToList();
                var cmd = new SelectCommand(this, selection);
                cmd.Execute();
                commands.Add(cmd);
            }


            dragging = false;
            Cursor = Cursors.Default;
            draggedElem = null;
            draggingEdge = false;
            selecting = false;
            moveDistance = SizeF.Empty;
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var zero1 = wvMatrix.TransformVW(e.Location);
            if (e.Delta > 0)
            {
                for (int i = 0; i < e.Delta; i++)
                {
                    Zoom(1.001f);
                }
            }
            else
            {
                for (int i = 0; i < -e.Delta; i++)
                {
                    Zoom(1 / 1.001f);
                }
            }
            var zero2 = wvMatrix.TransformVW(e.Location);

            wvMatrix.TranslateW(zero2.X - zero1.X, zero2.Y - zero1.Y);

            Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            foreach (var elem in elements)
            {
                var wP = wvMatrix.TransformVW(e.Location);
                wP -= new SizeF(elem.Location);
                if (elem.IsOverInputHandle(wP) >= 0 || elem.IsOverOutputHandle(wP))
                {
                    Cursor = Cursors.Hand;
                    break;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }

            if (dragging)
            {
                var a = wvMatrix.TransformVW(mouseCoords);
                var b = wvMatrix.TransformVW(e.Location);

                if (draggedElem == null)
                    wvMatrix.TranslateW(b.X - a.X, b.Y - a.Y);
                else if (!draggingEdge)
                {
                    var delta = new SizeF(b.X - a.X, b.Y - a.Y);

                    var newSnap = SnapToGrid(b);

                    var snapDistance = new SizeF(newSnap.X - lastSnap.X, newSnap.Y - lastSnap.Y);

                    foreach (var elem in SelectedElements)
                    {
                        var oldLocation = SnapToGrid(elem.Location);

                        elem.Location = oldLocation + snapDistance;
                    }
                    moveDistance += snapDistance;
                    lastSnap = newSnap;
                }
            }
            mouseCoords = e.Location;
            Invalidate();
            base.OnMouseMove(e);
        }

        RectangleF GetCurveRect(NodeElement a, NodeElement b, int i)
        {
            var clipRegion = new RectangleF(a.Location, a.Size);
            var ax = clipRegion.Right - a.HandleSize / 2;
            var ay = clipRegion.Bottom - a.Size.Height / 2;

            var bx = b.Location.X + a.HandleSize / 2;
            float spacing = b.Size.Height / (b.InputCount + 1);
            var by = b.Location.Y + (spacing * (i + 1));

            return new RectangleF(Math.Min(ax, bx), Math.Min(ay, by), Math.Abs(ax - bx), Math.Abs(ay - by));
        }

        /// <summary>
        /// Draws half of a bezier curve connecting two nodes
        /// </summary>
        /// <param name="g">Graphics context</param>
        /// <param name="a">Source node</param>
        /// <param name="b">Destination node</param>
        /// <param name="i">Index of destination node's handle</param>
        /// <param name="lowerHalf">Whether to draw lower or upper half of the curve</param>
        void DrawCurve(Graphics g, NodeElement a, NodeElement b, int i, bool lowerHalf, float opacity = 1.0f)
        {
            var clipRegion = new RectangleF(a.Location, a.Size);
            var ax = clipRegion.Right - a.HandleSize / 2;
            var ay = clipRegion.Bottom - a.Size.Height / 2;

            var targetRect = b.GetInputRectangle(i);

            var bx = b.Location.X + targetRect.X + targetRect.Width / 2;
            var by = b.Location.Y + targetRect.Y + targetRect.Height / 2;

            var w = Math.Abs(ax - bx);
            var h = Math.Abs(ay - by);

            var ctx = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float sx, sy;
            if (lowerHalf)
            {
                sx = ax < bx ? ax : ax - w / 2;
                sy = ay < by ? ay : ay - h / 2;
            }
            else
            {
                sx = ax < bx ? ax + w / 2 : bx;
                sy = ay < by ? ay + h / 2 : by;
            }
            var rect = new RectangleF(sx, sy, w / 2, h / 2);
            rect.Inflate(2, 2);
            g.Clip = new Region(rect);

            using (var brush = new SolidBrush(Color.FromArgb((byte)(opacity * 255), 0, 0, 0)))
            using (var p = new Pen(brush))
            {
                p.StartCap = LineCap.RoundAnchor;
                p.EndCap = LineCap.ArrowAnchor;

                g.DrawBezier(p, ax, ay, lerp(ax, bx, 0.5f), ay, lerp(bx, ax, 0.5f), by, bx, by);
            }
            g.Restore(ctx);
        }

        void DrawShadows(Graphics g, Region backReg, int minLevel)
        {
            using (var shadowRegion = new Region())
            {
                shadowRegion.MakeEmpty();

                int shadowLevel = 0;
                var shadowSet = elements.OrderBy(x => x.ZIndex).Where(x => x.ZIndex > minLevel).ToList();

                while (shadowSet.Count > 0)
                {
                    var levelRegion = new Region();
                    levelRegion.MakeEmpty();
                    foreach (var elem in shadowSet.ToArray())
                    {
                        using (var reg = elem.GetRegion())
                        {
                            reg.Translate(3 << shadowLevel, 3 << shadowLevel);

                            using (var testReg = new Region())
                            {
                                testReg.MakeEmpty();
                                testReg.Union(reg);
                                testReg.Intersect(levelRegion);

                                if (testReg.IsEmpty(g))
                                {
                                    levelRegion.Union(reg);
                                    shadowSet.Remove(elem);
                                }
                            }
                        }
                    }
                    shadowRegion.Union(levelRegion);
                    shadowLevel++;
                }

                var ctx = g.BeginContainer();
                g.Clip = backReg;
                var shadowColor = Color.FromArgb(80, 0, 0, 0);
                using (var shadowBrush = new SolidBrush(shadowColor))
                {
                    g.FillRegion(shadowBrush, shadowRegion);
                }
                g.EndContainer(ctx);
            }
        }

        void DrawGrid(Graphics g, float gridSize = 5)
        {
            (var dist, var gSize) = CalcGridSize(gridSize);

            var p0 = wvMatrix.TransformWV(PointF.Empty);
            float x = p0.X;
            float y = p0.Y;

            int xpos = 0;
            int ypos = 0;
            
            if (x < 0)
            {
                while (x < 0)
                {
                    x += dist;
                    xpos++;
                }
            }
            else
            {
                while (x > 0)
                {
                    x -= dist;
                    xpos--;
                }
            }

            if (y < 0)
            {
                while (y < 0)
                {
                    y += dist;
                    ypos++;
                }
            }
            else
            {
                while (y > 0)
                {
                    y -= dist;
                    ypos--;
                }
            }

            while (x < Width)
            {
                var pen = xpos % 10 == 0 ? Pens.Gray : Pens.LightGray;
                g.DrawLine(pen, x, 0, x, Height);

                x += dist;
                xpos++;
            }

            while (y < Height)
            {
                var pen = ypos % 10 == 0 ? Pens.Gray : Pens.LightGray;
                g.DrawLine(pen, 0, y, Width, y);

                y += dist;
                ypos++;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            DrawGrid(g);
            g.Transform = wvMatrix.WorldView;

            DrawShadows(g, g.Clip, 0);

            var selectionRectangles = new List<RectangleF>();
            foreach (var elem in elements)
            {
                if (elem.Selected)
                {
                    var rect = elem.ClientRectangle;
                    rect.Inflate(2, 2 + elem.HandleSize / 2.0f);

                    selectionRectangles.Add(rect);
                }
            }

            for (int i = 0; i < selectionRectangles.Count; i++)
            {
                for (int j = 0; j < selectionRectangles.Count; j++)
                {
                    if (i == j) continue;
                    var ri = selectionRectangles[i];
                    var rj = selectionRectangles[j];

                    var ri2 = ri;
                    var rj2 = rj;
                    ri2.Inflate(1, 1);
                    rj2.Inflate(1, 1);

                    if (ri2.IntersectsWith(rj2))
                    {
                        int min = Math.Min(i, j);
                        int max = Math.Max(i, j);
                        selectionRectangles.RemoveAt(max);
                        selectionRectangles.RemoveAt(min);
                        selectionRectangles.Add(RectangleF.Union(ri, rj));
                        i = 0;
                        break;
                    }
                }
            }

            // First draw the non-selected nodes and edges...
            foreach (var elem in elements.Where(x => !x.Selected).OrderBy(x => x.ZIndex))
            {
                var ctx = g.BeginContainer();
                g.TranslateTransform(elem.Location.X, elem.Location.Y);

                elem.Invalidate(new PaintEventArgs(g, new Rectangle(Point.Empty, elem.Size.ToSize())));
                g.EndContainer(ctx);

                foreach (var edge in edges.Edges.Where(x => x.source == elem).OrderBy(x => x.destination.ZIndex))
                {
                    DrawCurve(g, edge.source, edge.destination, edge.index, false, 0.5f);
                    DrawCurve(g, edge.source, edge.destination, edge.index, true);
                }
                foreach (var edge in edges.Edges.Where(x => x.destination == elem).OrderBy(x => x.source.ZIndex))
                {
                    DrawCurve(g, edge.source, edge.destination, edge.index, true, 0.5f);
                    DrawCurve(g, edge.source, edge.destination, edge.index, false);
                }

                DrawShadows(g, elem.GetRegion(), elem.ZIndex);
            }

            // ...then the semi-transparent selection rectangles...
            foreach (var rect in selectionRectangles)
            {
                var color = Color.FromArgb(80, Focused ? SystemColors.Highlight : SystemColors.InactiveCaption);
                var original_pen = Focused ? SystemPens.Highlight : SystemPens.InactiveCaption;
                using (var alphaBrush = new SolidBrush(color))
                using (var pen = (Pen)(original_pen.Clone()))
                {
                    pen.Width = 1.0f / currentScale;

                    g.FillRectangle(alphaBrush, rect);
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }

            // ...then the selected nodes and edges.
            foreach (var elem in elements.Where(x => x.Selected).OrderBy(x => x.ZIndex))
            {
                var ctx = g.BeginContainer();
                g.TranslateTransform(elem.Location.X, elem.Location.Y);

                elem.Invalidate(new PaintEventArgs(g, new Rectangle(Point.Empty, elem.Size.ToSize())));
                g.EndContainer(ctx);

                foreach (var edge in edges.Edges.Where(x => x.source == elem).OrderBy(x => x.destination.ZIndex))
                {
                    DrawCurve(g, edge.source, edge.destination, edge.index, false, 0.5f);
                    DrawCurve(g, edge.source, edge.destination, edge.index, true);
                }
                foreach (var edge in edges.Edges.Where(x => x.destination == elem).OrderBy(x => x.source.ZIndex))
                {
                    DrawCurve(g, edge.source, edge.destination, edge.index, true, 0.5f);
                    DrawCurve(g, edge.source, edge.destination, edge.index, false);
                }

                DrawShadows(g, elem.GetRegion(), elem.ZIndex);
            }

            if (draggingEdge)
            {
                var clipRegion = new RectangleF(draggedElem.Location, draggedElem.Size);
                var ax = clipRegion.Right - draggedElem.HandleSize / 2;
                var ay = clipRegion.Bottom - draggedElem.Size.Height / 2;

                var b = wvMatrix.TransformVW(mouseCoords);

                using (var p = new Pen(Color.Black))
                {
                    p.StartCap = LineCap.RoundAnchor;
                    p.EndCap = LineCap.ArrowAnchor;
                    g.DrawLine(p, ax, ay, b.X, b.Y);
                }
            }

            if (selecting)
            {
                var color = Color.FromArgb(80, SystemColors.Highlight);
                using (var alphaBrush = new SolidBrush(color))
                using (var pen = (Pen)(SystemPens.Highlight.Clone()))
                {
                    pen.Width = 1.0f / currentScale;

                    var origin = wvMatrix.TransformVW(startMouseCoords);
                    var end = wvMatrix.TransformVW(mouseCoords);

                    var rect = RectFromPoints(origin, end);

                    g.FillRectangle(alphaBrush, rect);
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }

            base.OnPaint(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        public PointF GetWorldCoordinates(PointF coords)
        {
            return wvMatrix.TransformVW(coords);
        }

        public PointF GetViewCoordinates(PointF coords)
        {
            return wvMatrix.TransformWV(coords);
        }

        public NodeElement GetElementByCoords(PointF coords)
        {
            foreach (var elem in elements)
            {
                var clipRegion = new RectangleF(elem.Location, elem.Size);
                if (clipRegion.Contains(coords)) return elem;
            }
            return null;
        }

        public void Center(PointF worldPoint)
        {
            var viewCoords = wvMatrix.TransformWV(worldPoint);
            wvMatrix.TranslateV(viewCoords.X - (Width / 2.0f), viewCoords.Y - (Height / 2.0f));
            Invalidate();
        }

        public void Center()
        {
            var minX = float.PositiveInfinity;
            var minY = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var maxY = float.NegativeInfinity;

            foreach (var elem in elements)
            {
                var minpos = elem.Location;
                var maxpos = elem.Location + elem.Size;

                minX = Math.Min(minX, minpos.X);
                minY = Math.Min(minY, minpos.Y);

                maxX = Math.Max(maxX, maxpos.X);
                maxY = Math.Max(maxY, maxpos.Y);
            }

            var center = new PointF((maxX - minX) / 2.0f + minX, (maxY - minY) / 2.0f + minY);
            Center(center);
        }

        public void ResetZoom()
        {
            wvMatrix.ScaleW(1.0f / currentScale, 1.0f / currentScale);
            currentScale = 1.0f;
            Invalidate();
        }

        public void Zoom(float value)
        {
            wvMatrix.ScaleW(value, value);
            currentScale *= value;
        }

        public void ZoomCenter(float value)
        {
            var zero1 = wvMatrix.TransformVW(new PointF(Width / 2.0f, Height / 2.0f));
            wvMatrix.ScaleW(value, value);
            currentScale *= value;
            var zero2 = wvMatrix.TransformVW(new PointF(Width / 2.0f, Height / 2.0f));

            wvMatrix.TranslateW(zero2.X - zero1.X, zero2.Y - zero1.Y);
            Invalidate();
        }

        public IEnumerable<NodeElement> SelectedElements
        {
            get => elements.Where(x => x.Selected);
        }

        public void FitToView(Func<NodeElement, bool> predicate, bool zoomOutOnly = false)
        {
            var minX = float.PositiveInfinity;
            var minY = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var maxY = float.NegativeInfinity;

            foreach (var elem in elements.Where(predicate))
            {
                var minpos = elem.Location;
                var maxpos = elem.Location + elem.Size;

                minX = Math.Min(minX, minpos.X);
                minY = Math.Min(minY, minpos.Y);

                maxX = Math.Max(maxX, maxpos.X);
                maxY = Math.Max(maxY, maxpos.Y);
            }

            if (float.IsInfinity(minX)) return;

            if (!zoomOutOnly || (maxX - minX > Width || maxY - minY > Height))
            {
                var center = new PointF((maxX - minX) / 2.0f + minX, (maxY - minY) / 2.0f + minY);
                var w = maxX - minX;
                var h = maxY - minY;
                var ratio = w / h;
                var targetRatio = Width / Height;
                if (ratio > targetRatio)
                {
                    var scale = Width / (w * currentScale);
                    wvMatrix.ScaleW(scale, scale);
                    currentScale *= scale;
                }
                else
                {
                    var scale = Height / (h * currentScale);
                    wvMatrix.ScaleW(scale, scale);
                    currentScale *= scale;
                }
                Center(center);
            }
            else
            {
                ResetZoom();
                Center();
            }
        }

        public void SelectElements(Func<NodeElement, bool> filter)
        {
            var cmd = new SelectCommand(this, elements.Where(filter).ToList());
            cmd.Execute();
            commands.Add(cmd);
        }

        public void DeleteElements(Func<NodeElement, bool> filter)
        {
            var cmd = new InverseCommand(new AddElementsCommand(this, elements.Where(filter).ToList()));
            cmd.Execute();
            commands.Add(cmd);
        }

        public void Undo()
        {
            commands.Undo();
        }

        public void Redo()
        {
            commands.Redo();
        }

        public bool CanUndo => commands.CanUndo;
        public bool CanRedo => commands.CanRedo;

        public void ResetHistory()
        {
            commands.Clear();
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    e.IsInputKey = true;
                    break;
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Left))
            {
                wvMatrix.TranslateV(-5, 0);
            }
            if (e.KeyData == (Keys.Right))
            {
                wvMatrix.TranslateV(5, 0);
            }
            if (e.KeyData == (Keys.Up))
            {
                wvMatrix.TranslateV(0, -5);
            }
            if (e.KeyData == (Keys.Down))
            {
                wvMatrix.TranslateV(0, 5);
            }
            Invalidate();
            base.OnKeyDown(e);
        }
    }
}
