using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Threading;

namespace MarchOfTheRays.CpuRenderer
{
    public class Renderer
    {
        Vector3 cameraOrigin = new Vector3(2.1f, 2.0f, 2.5f);
        Vector3 cameraTarget = Vector3.Zero;
        Vector3 upDirection = Vector3.UnitY;

        const int MAX_ITER = 100; // 100 is a safe number to use, it won't produce too many artifacts and still be quite fast
        const float MAX_DIST = 20.0f; // Make sure you change this if you have objects farther than 20 units away from the camera
        const float EPSILON = 0.001f; // At this distance we are close enough to the object that we have essentially hit it

        float distfunc(Vector3 pos)
        {
            //return sphere(pos, 1.0f);
            //return opRep(pos, new Vector3(0.4f), x => sphere(x, 0.1f));
            //return opRep(new Vector3(2f), opSubtract(sphere(0.5f), udBox(new Vector3(0.4f))))(pos);
            return 0;
        }

        Vector2 Map(Func<float, float> f, Vector2 v)
        {
            return new Vector2(f(v.X), f(v.Y));
        }

        Vector3 Map(Func<float, float> f, Vector3 v)
        {
            return new Vector3(f(v.X), f(v.Y), f(v.Z));
        }

        Vector4 Map(Func<float, float> f, Vector4 v)
        {
            return new Vector4(f(v.X), f(v.Y), f(v.Z), f(v.W));
        }

        Vector2 Map(Func<float, float, float> f, Vector2 a, Vector2 b)
        {
            return new Vector2(f(a.X, b.X), f(a.Y, b.Y));
        }

        Vector3 Map(Func<float, float, float> f, Vector3 a, Vector3 b)
        {
            return new Vector3(f(a.X, b.X), f(a.Y, b.Y), f(a.Z, b.Z));
        }

        Vector4 Map(Func<float, float, float> f, Vector4 a, Vector4 b)
        {
            return new Vector4(f(a.X, b.X), f(a.Y, b.Y), f(a.Z, b.Z), f(a.W, b.W));
        }

        float mod(float x, float y)
        {
            return x - y * (float)Math.Floor(x / y);
        }

        Vector3 evalNodeVector3(Vector3 pos, Core.INode node)
        {
            switch (node)
            {
                case Core.InputNode n: return pos;
                case Core.Float3ConstantNode n: return new Vector3(n.X, n.Y, n.Z);
                case Core.AbsNode n:
                    {
                        var input = evalNodeVector3(pos, n.Input);
                        return Map(Math.Abs, input);
                    }
                case Core.MinMaxNode n:
                    {
                        var left = evalNodeVector3(pos, n.Left);
                        var right = evalNodeVector3(pos, n.Right);
                        return n.IsMin ? Map(Math.Min, left, right) : Map(Math.Max, left, right);
                    }
                case Core.ArithmeticNode n:
                    {
                        var left = new Vector3();
                        var right = new Vector3();
                        if (n.Left.OutputType == Core.NodeType.Float && n.Right.OutputType == Core.NodeType.Float) throw new NotImplementedException();
                        if (n.Left.OutputType == Core.NodeType.Float3)
                        {
                            left = evalNodeVector3(pos, n.Left);
                        }
                        else
                        {
                            left = new Vector3(evalNodeFloat(pos, n.Left));
                        }

                        if (n.Right.OutputType == Core.NodeType.Float3)
                        {
                            right = evalNodeVector3(pos, n.Right);
                        }
                        else
                        {
                            right = new Vector3(evalNodeFloat(pos, n.Right));
                        }

                        switch (n.Operation)
                        {
                            case Core.ArithOp.Add: return left + right;
                            case Core.ArithOp.Cross: return Vector3.Cross(left, right);
                            case Core.ArithOp.Div: return Map((x, y) => x / y, left, right);
                            case Core.ArithOp.Mod: return Map(mod, left, right);
                            case Core.ArithOp.Mul: return Map((x, y) => x * y, left, right);
                            case Core.ArithOp.Sub: return left - right;
                            default: throw new NotImplementedException();
                        }
                    }
                default: throw new NotImplementedException();
            }
        }

        float evalNodeFloat(Vector3 pos, Core.INode node)
        {
            switch (node)
            {
                case Core.OutputNode n: return evalNodeFloat(pos, n.Input);
                case Core.FloatConstantNode n: return n.Value;
                case Core.AbsNode n: return Math.Abs(evalNodeFloat(pos, n.Input));
                case Core.LengthNode n:
                    {
                        switch (n.Input.OutputType)
                        {
                            case Core.NodeType.Float: return Math.Abs(evalNodeFloat(pos, n.Input));
                            case Core.NodeType.Float3: return evalNodeVector3(pos, n.Input).Length();
                            default: throw new NotImplementedException();
                        }
                    }
                case Core.MinMaxNode n:
                    {
                        var left = evalNodeFloat(pos, n.Left);
                        var right = evalNodeFloat(pos, n.Right);
                        if (n.IsMin) return Math.Min(left, right);
                        return Math.Max(left, right);
                    }
                case Core.ArithmeticNode n:
                    {
                        if (n.Operation == Core.ArithOp.Dot)
                        {
                            var l = evalNodeVector3(pos, n.Left);
                            var r = evalNodeVector3(pos, n.Right);
                            return Vector3.Dot(l, r);
                        }

                        var left = evalNodeFloat(pos, n.Left);
                        var right = evalNodeFloat(pos, n.Right);
                        switch (n.Operation)
                        {
                            case Core.ArithOp.Add: return left + right;
                            case Core.ArithOp.Div: return left / right;
                            case Core.ArithOp.Mul: return left * right;
                            case Core.ArithOp.Sub: return left - right;
                            case Core.ArithOp.Mod: return mod(left, right);
                            default: throw new NotImplementedException();
                        }
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        void MainRender(float x, float y, float width, float height, out Vector3 outColor, Core.INode node)
        {
            var cameraDir = Vector3.Normalize(cameraTarget - cameraOrigin);

            var cameraRight = Vector3.Normalize(Vector3.Cross(upDirection, cameraOrigin));
            var cameraUp = Vector3.Cross(cameraDir, cameraRight);

            var screenPos = new Vector2(-1.0f + 2.0f * x / width, -1.0f + 2.0f * y / height);
            screenPos.X *= width / height; // Correct aspect ratio

            var rayDir = Vector3.Normalize(cameraRight * screenPos.X + cameraUp * screenPos.Y + cameraDir);

            float totalDist = 0.0f;
            var pos = cameraOrigin;
            float dist = EPSILON;

            for (int i = 0; i < MAX_ITER; i++)
            {
                // Either we've hit the object or hit nothing at all, either way we should break out of the loop
                if (dist < EPSILON || totalDist > MAX_DIST)
                    break; // If you use windows and the shader isn't working properly, change this to continue;

                dist = evalNodeFloat(pos, node); // Evalulate the distance at the current point
                totalDist += dist;
                pos += dist * rayDir; // Advance the point forwards in the ray direction by the distance

            }

            if (dist < EPSILON)
            {
                var eps = new Vector2(0.0f, EPSILON);
                var yxx = new Vector3(eps.Y, eps.X, eps.X);
                var xyx = new Vector3(eps.X, eps.Y, eps.X);
                var xxy = new Vector3(eps.X, eps.X, eps.Y);

                var normal = Vector3.Normalize(new Vector3(
                    evalNodeFloat(pos + yxx, node) - evalNodeFloat(pos - yxx, node),
                    evalNodeFloat(pos + xyx, node) - evalNodeFloat(pos - xyx, node),
                    evalNodeFloat(pos + xxy, node) - evalNodeFloat(pos - xxy, node)));

                float diffuse = Math.Max(0.0f, Vector3.Dot(-rayDir, normal));
                float specular = (float)Math.Pow(diffuse, 64.0f);
                outColor = new Vector3(diffuse + specular);
            }
            else
            {
                outColor = Vector3.Zero;
            }
        }

        unsafe void RenderChunk(byte* rawBytes, int xoff, int width, int totalWidth, int totalHeight, Core.INode node)
        {
            for (int i = 0; i < width * totalHeight; i++)
            {
                int x = i % width;
                int y = (i - x) / width;
                Vector3 color;
                MainRender(x + xoff, y, totalWidth, totalHeight, out color, node);
                int idx = (x + xoff + y * totalWidth) * 3;
                rawBytes[idx + 0] = (byte)(Math.Min(color.X, 1.0f) * 255);
                rawBytes[idx + 1] = (byte)(Math.Min(color.Y, 1.0f) * 255);
                rawBytes[idx + 2] = (byte)(Math.Min(color.Z, 1.0f) * 255);
            }
        }

        public Image RenderImage(int width, int height, int nthreads, Core.INode node)
        {
            var img = new Bitmap(width, height);

            var data = img.LockBits(new Rectangle(Point.Empty, new Size(width, height)), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            var threads = new Thread[nthreads];
            int slice = width / nthreads;

            int start = 0;
            bool ok = true;

            unsafe
            {
                for (int i = 0; i < nthreads; i++)
                {
                    var xoff = start;
                    var w = Math.Min(width - start, slice);
                    start += slice + 1;
                    threads[i] = new Thread((d) =>
                    {
                        try
                        {
                            RenderChunk((byte*)(IntPtr)d, xoff, w, width, height, node);
                        }
                        catch (NotImplementedException)
                        {
                            ok = false;
                        }
                    });
                    threads[i].Start(data.Scan0);
                }
            }

            for (int i = 0; i < nthreads; i++)
            {
                threads[i].Join();
            }

            img.UnlockBits(data);

            return ok ? img : null;
        }

        public Image RenderImage(int width, int height, Core.INode node)
        {
            var img = new Bitmap(width, height);

            var data = img.LockBits(new Rectangle(Point.Empty, new Size(width, height)), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* rawBytes = (byte*)data.Scan0;
                for (int i = 0; i < width * height; i++)
                {
                    var x = i % width;
                    var y = (i - x) / width;

                    Vector3 color;
                    MainRender(x, y, width, height, out color, node);
                    int idx = i * 3;
                    rawBytes[idx + 0] = (byte)(Math.Min(color.X, 1.0f) * 255);
                    rawBytes[idx + 1] = (byte)(Math.Min(color.Y, 1.0f) * 255);
                    rawBytes[idx + 2] = (byte)(Math.Min(color.Z, 1.0f) * 255);
                }
            }

            img.UnlockBits(data);

            return img;
        }
    }
}
