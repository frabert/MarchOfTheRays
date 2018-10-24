using System;
using System.Collections;
using System.Collections.Generic;

using System.Drawing;

namespace MarchOfTheRays.Editor
{
    interface IClippable
    {
        RectangleF ClipRegion { get; }
    }

    class QuadTree<T> where T : class, IClippable
    {
        QuadTree<T> a, b, c, d;
        bool split = false;
        T obj;

        public QuadTree(RectangleF boundaries)
        {
            Boundaries = boundaries;
        }

        public QuadTree(PointF location, SizeF size)
        {
            Boundaries = new RectangleF(location, size);
        }

        public QuadTree(float x, float y, float width, float height)
        {
            Boundaries = new RectangleF(x, y, width, height);
        }

        public RectangleF Boundaries { get; private set; }

        public bool Contains(PointF p) => Boundaries.Contains(p);

        void Split()
        {
            float halfW = Boundaries.Width / 2;
            float halfH = Boundaries.Height / 2;
            a = new QuadTree<T>(new RectangleF(Boundaries.X, Boundaries.Y, halfW, halfH));
            b = new QuadTree<T>(new RectangleF(Boundaries.X + halfW, Boundaries.Y, halfW, halfH));
            c = new QuadTree<T>(new RectangleF(Boundaries.X, Boundaries.Y + halfH, halfW, halfH));
            d = new QuadTree<T>(new RectangleF(Boundaries.X + halfW, Boundaries.Y + halfH, halfW, halfH));

            if (a.Contains(obj.ClipRegion.Location))
            {
                a.Add(obj);
            }
            else if (b.Contains(obj.ClipRegion.Location))
            {
                b.Add(obj);
            }
            else if (c.Contains(obj.ClipRegion.Location))
            {
                c.Add(obj);
            }
            else if (d.Contains(obj.ClipRegion.Location))
            {
                d.Add(obj);
            }
            obj = null;

            split = true;
        }

        public void Add(T value)
        {
            if (!Boundaries.Contains(value.ClipRegion))
            {
                throw new InvalidOperationException();
            }

            var oBounds = value.ClipRegion;

            if (obj == null && !split)
            {
                obj = value;
                return;
            }
            else if (obj != null && !split)
            {
                Split();
            }

            if (a.Contains(oBounds.Location))
            {
                a.Add(value);
            }
            else if (b.Contains(oBounds.Location))
            {
                b.Add(value);
            }
            else if (c.Contains(oBounds.Location))
            {
                c.Add(value);
            }
            else if (d.Contains(oBounds.Location))
            {
                d.Add(value);
            }
        }

        public bool Remove(T value)
        {
            if (!Boundaries.Contains(value.ClipRegion))
            {
                throw new InvalidOperationException();
            }

            var oBounds = value.ClipRegion;

            if (obj == null && !split)
            {
                return false;
            }
            else if(obj == value)
            {
                obj = null;
                return true;
            }

            if (a.Contains(oBounds.Location))
            {
                return a.Remove(value);
            }
            else if (b.Contains(oBounds.Location))
            {
                return b.Remove(value);
            }
            else if (c.Contains(oBounds.Location))
            {
                return c.Remove(value);
            }
            else if (d.Contains(oBounds.Location))
            {
                return d.Remove(value);
            }

            return false;
        }

        public IEnumerable<T> Query(RectangleF rect)
        {
            if(!Boundaries.IntersectsWith(rect))
            {
                throw new InvalidOperationException();
            }

            if(obj != null)
            {
                if (obj.ClipRegion.IntersectsWith(rect)) yield return obj;
            }
            else if(split)
            {
                if(a.Boundaries.IntersectsWith(rect))
                {
                    foreach(var x in a.Query(rect))
                    {
                        yield return x;
                    }
                }

                if (b.Boundaries.IntersectsWith(rect))
                {
                    foreach (var x in b.Query(rect))
                    {
                        yield return x;
                    }
                }

                if (c.Boundaries.IntersectsWith(rect))
                {
                    foreach (var x in c.Query(rect))
                    {
                        yield return x;
                    }
                }

                if (c.Boundaries.IntersectsWith(rect))
                {
                    foreach (var x in c.Query(rect))
                    {
                        yield return x;
                    }
                }
            }
        }

        public IEnumerable<T> Query(PointF p)
        {
            if (!Boundaries.Contains(p))
            {
                throw new InvalidOperationException();
            }

            if (obj != null)
            {
                if (obj.ClipRegion.Contains(p)) yield return obj;
            }
            else if (split)
            {
                if (a.Contains(p))
                {
                    foreach (var x in a.Query(p))
                    {
                        yield return x;
                    }
                }

                if (b.Contains(p))
                {
                    foreach (var x in b.Query(p))
                    {
                        yield return x;
                    }
                }

                if (c.Contains(p))
                {
                    foreach (var x in c.Query(p))
                    {
                        yield return x;
                    }
                }

                if (d.Contains(p))
                {
                    foreach (var x in d.Query(p))
                    {
                        yield return x;
                    }
                }
            }
        }
    }
}
