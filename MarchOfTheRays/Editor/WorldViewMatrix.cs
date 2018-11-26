using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Drawing2D;
using System.Drawing;

namespace MarchOfTheRays.Editor
{
    class WorldViewMatrix : IDisposable
    {
        public Matrix WorldView { get; }
        public Matrix ViewWorld { get; }

        public WorldViewMatrix()
        {
            WorldView = new Matrix();
            ViewWorld = new Matrix();
        }

        public void TranslateW(float x, float y)
        {
            WorldView.Translate(x, y);
            ViewWorld.Translate(-x, -y, MatrixOrder.Append);
        }

        public void ScaleW(float x, float y)
        {
            WorldView.Scale(x, y);
            ViewWorld.Scale(1.0f / x, 1.0f / y, MatrixOrder.Append);
        }

        public void TranslateV(float x, float y)
        {
            ViewWorld.Translate(x, y);
            WorldView.Translate(-x, -y, MatrixOrder.Append);
        }

        public void ScaleV(float x, float y)
        {
            ViewWorld.Scale(x, y);
            WorldView.Scale(1.0f / x, 1.0f / y, MatrixOrder.Append);
        }

        public PointF TransformWV(PointF p)
        {
            var points = new PointF[] { p };

            WorldView.TransformPoints(points);

            return points[0];
        }

        public PointF TransformVW(PointF p)
        {
            var points = new PointF[] { p };

            ViewWorld.TransformPoints(points);

            return points[0];
        }

        public RectangleF TransformWV(RectangleF r)
        {
            var points = new PointF[] { r.Location, r.Location + r.Size };
            WorldView.TransformPoints(points);
            return RectangleF.FromLTRB(points[0].X, points[0].Y, points[1].X, points[1].Y);
        }

        public RectangleF TransformVW(RectangleF r)
        {
            var points = new PointF[] { r.Location, r.Location + r.Size };
            ViewWorld.TransformPoints(points);
            return RectangleF.FromLTRB(points[0].X, points[0].Y, points[1].X, points[1].Y);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    WorldView.Dispose();
                    ViewWorld.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
