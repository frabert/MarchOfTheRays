using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MarchOfTheRays.Editor
{
    class EdgeModifiedEventArgs : EventArgs
    {
        public EdgeModifiedEventArgs(NodeElement source, NodeElement destination, int index)
        {
            Source = source;
            Destination = destination;
            DestinationIndex = index;
        }

        public NodeElement Source { get; private set; }
        public NodeElement Destination { get; private set; }
        public int DestinationIndex { get; private set; }
    }

    class EdgeDictionary
    {
        Dictionary<NodeElement, List<(NodeElement destination, int index)>> forwardStars;
        Dictionary<NodeElement, List<(NodeElement source, int index)>> backwardStars;
        Dictionary<(NodeElement destination, int index), NodeElement> inwardNodes;

        public EdgeDictionary()
        {
            forwardStars = new Dictionary<NodeElement, List<(NodeElement, int)>>();
            backwardStars = new Dictionary<NodeElement, List<(NodeElement, int)>>();
            inwardNodes = new Dictionary<(NodeElement, int), NodeElement>();
        }

        public void Clear()
        {
            forwardStars.Clear();
            backwardStars.Clear();
            inwardNodes.Clear();
        }

        public IEnumerable<(NodeElement destination, int index)> ForwardStar(NodeElement source)
        {
            if (forwardStars.TryGetValue(source, out var list))
            {
                foreach (var e in list) yield return e;
            }
        }

        public IEnumerable<(NodeElement source, int index)> BackwardStar(NodeElement destination)
        {
            if (backwardStars.TryGetValue(destination, out var list))
            {
                foreach (var e in list) yield return e;
            }
        }

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

        public void AddElement(NodeElement elem)
        {
            forwardStars.Add(elem, new List<(NodeElement, int)>());
            backwardStars.Add(elem, new List<(NodeElement, int)>());
        }

        public void RemoveElement(NodeElement elem)
        {
            foreach (var (destination, index) in forwardStars[elem])
            {
                inwardNodes.Remove((destination, index));
            }
            forwardStars.Remove(elem);
            foreach(var (source, index) in backwardStars[elem])
            {
                inwardNodes.Remove((elem, index));
            }
            backwardStars.Remove(elem);
        }

        public void AddEdge(NodeElement source, NodeElement destination, int index)
        {
            forwardStars[source].Add((destination, index));
            backwardStars[destination].Add((source, index));
            inwardNodes.Add((destination, index), source);
        }

        public void RemoveEdge(NodeElement destination, int index)
        {
            var source = inwardNodes[(destination, index)];
            forwardStars[source].Remove((destination, index));
            backwardStars[destination].Remove((source, index));
            inwardNodes.Remove((destination, index));
        }

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
    }

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
                foreach (var elem in elems)
                {
                    elem.NeedsRepaint += canvas.element_NeedsRepaint;
                    canvas.elements.Add(elem);
                    canvas.edges.AddElement(elem);
                }

                foreach (var edge in edges)
                {
                    canvas.edges.AddEdge(edge.source, edge.destination, edge.index);
                }
                canvas.Invalidate();
            }

            public void Undo()
            {
                foreach (var elem in elems)
                {
                    foreach(var bs in canvas.edges.BackwardStar(elem))
                    {
                        var tuple = (bs.source, elem, bs.index);
                        if(!edges.Contains(tuple)) edges.Add(tuple);
                    }

                    foreach(var fs in canvas.edges.ForwardStar(elem))
                    {
                        var tuple = (elem, fs.destination, fs.index);
                        if (!edges.Contains(tuple)) edges.Add(tuple);
                    }
                    canvas.elements.Remove(elem);
                    canvas.edges.RemoveElement(elem);
                }
                canvas.Invalidate();
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

                foreach (var elem in elems)
                {
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
            private IList<NodeElement> elems;
            private SizeF amount;

            public MoveElementsCommand(IList<NodeElement> elems, SizeF amount)
            {
                this.elems = elems;
                this.amount = amount;
            }

            public void Execute()
            {
                foreach (var elem in elems)
                {
                    elem.Location += amount;
                }
            }

            public void Undo()
            {
                foreach (var elem in elems)
                {
                    elem.Location -= amount;
                }
            }
        }
        #endregion

        List<NodeElement> elements = new List<NodeElement>();
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

        public NodeCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        static float lerp(float a, float b, float x)
        {
            return (1 - x) * a + x * b;
        }

        static PointF lerp(PointF a, PointF b, float x)
        {
            return new PointF((1 - x) * a.X + x * b.X, (1 - x) * a.Y + x * b.Y);
        }

        static RectangleF RectFromPoints(PointF a, PointF b)
        {
            return RectangleF.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
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
            foreach(var edge in e)
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

        public IReadOnlyList<NodeElement> Elements => elements;

        private void element_NeedsRepaint(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            startMouseCoords = e.Location;
            if (e.Button == MouseButtons.Left)
            {
                mouseCoords = e.Location;
                selecting = true;
                var coords = wvMatrix.TransformVW(mouseCoords);

                foreach (var elem in elements)
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
                        if(source != null)
                        {
                            draggedElem = source;
                            draggingEdge = true;
                            RemoveEdge(elem, inputHandle);
                            break;
                        }
                    }

                    if (elem.ClipRegion.Contains(coords))
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
                if (moveDistance.Width > 0 || moveDistance.Height > 0)
                {
                    var cmd = new MoveElementsCommand(SelectedElements.ToList(), moveDistance);
                    commands.Add(cmd);
                }
            }

            if (selecting)
            {
                var selection = elements.Where(x =>
                {
                    var origin = wvMatrix.TransformVW(startMouseCoords);
                    var end = wvMatrix.TransformVW(mouseCoords);

                    var rect = RectFromPoints(origin, end);
                    return x.ClipRegion.IntersectsWith(rect);
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
            //if (!dragging)
            {
                foreach (var elem in elements)
                {
                    var wP = wvMatrix.TransformVW(e.Location);
                    wP -= new SizeF(elem.Location);
                    if (elem.IsOverInputHandle(wP) >= 0 || elem.IsOverOutputHandle(wP))
                    {
                        Cursor = Cursors.Hand;
                        goto skip;
                    }
                }
                Cursor = Cursors.Default;
            }

            skip:
            if (dragging)
            {
                var a = wvMatrix.TransformVW(mouseCoords);
                var b = wvMatrix.TransformVW(e.Location);

                if (draggedElem == null)
                    wvMatrix.TranslateW(b.X - a.X, b.Y - a.Y);
                else if (!draggingEdge)
                {
                    var delta = new SizeF(b.X - a.X, b.Y - a.Y);
                    foreach (var elem in SelectedElements)
                    {
                        elem.Location += delta;
                    }
                    moveDistance += delta;
                }
            }
            mouseCoords = e.Location;
            Invalidate();
            base.OnMouseMove(e);
        }

        void DrawCurve(Graphics g, NodeElement a, NodeElement b, int i)
        {
            var ax = a.ClipRegion.Right - a.HandleSize / 2;
            var ay = a.ClipRegion.Bottom - a.Size.Height / 2;

            var bx = b.Location.X + a.HandleSize / 2;
            float spacing = b.Size.Height / (b.InputCount + 1);
            var by = b.Location.Y + (spacing * (i + 1));

            using (var p = new Pen(Brushes.Black))
            {
                p.StartCap = LineCap.RoundAnchor;
                p.EndCap = LineCap.ArrowAnchor;

                g.DrawBezier(p, ax, ay, lerp(ax, bx, 0.5f), ay, lerp(bx, ax, 0.5f), by, bx, by);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Transform = wvMatrix.WorldView;

            var selectionRectangles = new List<RectangleF>();

            foreach (var elem in elements)
            {
                if (elem.Selected)
                {
                    var rect = elem.ClipRegion;
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


            foreach (var rect in selectionRectangles)
            {
                var color = Color.FromArgb(80, SystemColors.Highlight);
                using (var alphaBrush = new SolidBrush(color))
                using (var pen = (Pen)(SystemPens.Highlight.Clone()))
                {
                    pen.Width = 1.0f / currentScale;

                    g.FillRectangle(alphaBrush, rect);
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
            }

            foreach (var elem in elements)
            {

                var ctx = g.BeginContainer();
                g.TranslateTransform(elem.Location.X, elem.Location.Y);

                elem.Paint(g);
                g.EndContainer(ctx);
            }

            foreach (var edge in edges.Edges)
            {
                DrawCurve(g, edge.source, edge.destination, edge.index);
            }

            if (draggingEdge)
            {
                var ax = draggedElem.ClipRegion.Right - draggedElem.HandleSize / 2;
                var ay = draggedElem.ClipRegion.Bottom - draggedElem.Size.Height / 2;

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
                if (elem.ClipRegion.Contains(coords)) return elem;
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

        public void FitToView(Func<NodeElement, bool> predicate)
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

            var center = new PointF((maxX - minX) / 2.0f + minX, (maxY - minY) / 2.0f + minY);
            var w = maxX - minX;
            var h = maxY - minY;
            var ratio = w / h;
            var targetRatio = (float)Width / (float)Height;
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

        public void ResetHistory()
        {
            commands.Clear();
        }
    }
}
