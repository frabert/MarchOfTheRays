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
        public EdgeModifiedEventArgs() { }

        public NodeElement Source { get; set; }
        public NodeElement Destination { get; set; }
        public int DestinationIndex { get; set; }
    }

    class PreviewEdgeAddedEventArgs : EventArgs
    {
        public PreviewEdgeAddedEventArgs() { }

        public NodeElement Source { get; set; }
        public NodeElement Destination { get; set; }
        public int DestinationIndex { get; set; }
        public bool Allow { get; set; }
    }

    class NodeCanvas : UserControl
    {
        List<NodeElement> elements = new List<NodeElement>();
        Dictionary<(NodeElement, int), NodeElement> edgeDictionary = new Dictionary<(NodeElement, int), NodeElement>();

        WorldViewMatrix wvMatrix = new WorldViewMatrix();
        float currentScale = 1.0f;

        bool dragging = false;
        bool draggingEdge = false;
        bool selecting = false;
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
            edgeDictionary.Clear();
            Invalidate();
        }

        public void AddElement(NodeElement e)
        {
            e.NeedsRepaint += element_NeedsRepaint;
            elements.Add(e);
        }

        public void RemoveElement(NodeElement e)
        {
            elements.Remove(e);
            Invalidate();
        }

        public void AddEdge(NodeElement source, NodeElement destination, int index)
        {
            edgeDictionary.Add((destination, index), source);
            OnEdgeAdded(new EdgeModifiedEventArgs()
            {
                Destination = destination,
                DestinationIndex = index,
                Source = source
            });
        }

        public void RemoveEdge(NodeElement destination, int index)
        {
            var e = edgeDictionary[(destination, index)];
            edgeDictionary.Remove((destination, index));
            OnEdgeRemoved(new EdgeModifiedEventArgs()
            {
                Destination = destination,
                DestinationIndex = index,
                Source = e
            });
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
                        var source = edgeDictionary[(elem, inputHandle)];
                        draggedElem = source;
                        draggingEdge = true;
                        edgeDictionary.Remove((elem, inputHandle));
                        OnEdgeRemoved(new EdgeModifiedEventArgs()
                        {
                            Destination = elem,
                            DestinationIndex = inputHandle,
                            Source = source
                        });
                        break;
                    }

                    if (elem.ClipRegion.Contains(coords))
                    {
                        if (!elem.Selected)
                        {
                            foreach (var elem_ in elements)
                            {
                                elem_.Selected = false;
                            }
                        }
                        selecting = false;
                        dragging = true;
                        draggedElem = elem;
                        elem.Selected = true;
                        OnSelectionChanged();
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

        public event EventHandler<PreviewEdgeAddedEventArgs> PreviewEdgeAdded;
        public event EventHandler<EdgeModifiedEventArgs> EdgeAdded;
        public event EventHandler<EdgeModifiedEventArgs> EdgeRemoved;
        public event EventHandler SelectionChanged;

        protected virtual void OnPreviewEdgeAdded(PreviewEdgeAddedEventArgs e)
        {
            PreviewEdgeAdded?.Invoke(this, e);
        }

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
                        var args = new PreviewEdgeAddedEventArgs()
                        {
                            Allow = true,
                            Destination = elem,
                            DestinationIndex = i,
                            Source = draggedElem
                        };
                        OnPreviewEdgeAdded(args);

                        if (args.Allow && !edgeDictionary.ContainsKey((args.Destination, args.DestinationIndex)))
                        {
                            edgeDictionary.Add((args.Destination, args.DestinationIndex), args.Source);


                            var args2 = new EdgeModifiedEventArgs()
                            {
                                Destination = args.Destination,
                                DestinationIndex = args.DestinationIndex,
                                Source = args.Source
                            };
                            OnEdgeAdded(args2);
                        }
                        else
                        {
                            System.Media.SystemSounds.Beep.Play();
                            var toolTip = new ToolTip();
                            toolTip.Show("Cannot insert edge", this);
                        }

                        break;
                    }
                }
            }

            if (selecting)
            {
                foreach (var elem in elements)
                {
                    var origin = wvMatrix.TransformVW(startMouseCoords);
                    var end = wvMatrix.TransformVW(mouseCoords);

                    var rect = RectFromPoints(origin, end);

                    elem.Selected = elem.ClipRegion.IntersectsWith(rect);
                }
                OnSelectionChanged();
            }


            dragging = false;
            Cursor = Cursors.Default;
            draggedElem = null;
            draggingEdge = false;
            selecting = false;
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

                if (draggedElem == null) wvMatrix.TranslateW(b.X - a.X, b.Y - a.Y);
                else if (!draggingEdge)
                {
                    foreach (var elem in elements)
                    {
                        if (elem.Selected)
                        {
                            var newLoc = elem.Location;

                            newLoc.X += b.X - a.X;
                            newLoc.Y += b.Y - a.Y;
                            elem.Location = newLoc;
                        }
                    }
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

            foreach (var edge in edgeDictionary)
            {
                DrawCurve(g, edge.Value, edge.Key.Item1, edge.Key.Item2);
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

        public void FitToView()
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

        public void SelectElements(Predicate<NodeElement> filter)
        {
            elements.ForEach(x => x.Selected = filter(x));
            OnSelectionChanged();
        }

        public void DeleteElements(Predicate<NodeElement> filter)
        {
            elements.RemoveAll(filter);
            Invalidate();
        }
    }
}
