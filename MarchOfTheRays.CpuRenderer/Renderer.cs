using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;

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
                case Core.FloatConstantNode n: return new Vector3(n.Value);
                case Core.AbsNode n:
                    {
                        var input = evalNodeVector3(pos, n.Input);
                        return Map(Math.Abs, input);
                    }
                case Core.MinMaxNode n:
                    {
                        Vector3 left, right;

                        if (n.Left.OutputType == Core.NodeType.Float3) left = evalNodeVector3(pos, n.Left);
                        else left = new Vector3(evalNodeFloat(pos, n.Left));

                        if (n.Right.OutputType == Core.NodeType.Float3) right = evalNodeVector3(pos, n.Right);
                        else right = new Vector3(evalNodeFloat(pos, n.Right));

                        return n.IsMin ? Map(Math.Min, left, right) : Map(Math.Max, left, right);
                    }
                case Core.ArithmeticNode n:
                    {
                        Vector3 left, right;
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

        Expression toExpr<T>(Expression<Func<T, T>> f)
        {
            return f;
        }

        Expression toExpr<T>(Expression<Func<T, T, T>> f)
        {
            return f;
        }

        Expression mapExpr(Expression<Func<float, float>> f, Expression inp)
        {
            var vec3ctor = typeof(Vector3).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float) });
            var x = Expression.Field(inp, "X");
            var y = Expression.Field(inp, "Y");
            var z = Expression.Field(inp, "Z");
            return Expression.New(vec3ctor, Expression.Invoke(f, x), Expression.Invoke(f, y), Expression.Invoke(f, z));
        }

        Expression mapExpr(Expression<Func<float, float, float>> f, Expression l, Expression r)
        {
            var vec3ctor = typeof(Vector3).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float) });
            var lx = Expression.Field(l, "X");
            var ly = Expression.Field(l, "Y");
            var lz = Expression.Field(l, "Z");

            var rx = Expression.Field(r, "X");
            var ry = Expression.Field(r, "Y");
            var rz = Expression.Field(r, "Z");
            return Expression.New(vec3ctor, Expression.Invoke(f, lx, rx), Expression.Invoke(f, ly, ry), Expression.Invoke(f, lz, rz));
        }

        Expression compileToVec3(ParameterExpression input, Core.INode node, Dictionary<Core.INode, Expression> nodes)
        {
            if (nodes.TryGetValue(node, out var val)) return val;

            var vec3ctor = typeof(Vector3).GetConstructor(new Type[] { typeof(float) });
            var cross = typeof(Vector3).GetMethod("Cross");

            switch (node)
            {
                case Core.InputNode n:
                    nodes.Add(node, input);
                    return input;
                case Core.Float3ConstantNode n:
                    var exp = Expression.Constant(new Vector3(n.X, n.Y, n.Z));
                    nodes.Add(node, exp);
                    return exp;
                case Core.FloatConstantNode n:
                    exp = Expression.Constant(new Vector3(n.Value));
                    nodes.Add(node, exp);
                    return exp;
                case Core.AbsNode n:
                    {
                        var i = compileToVec3(input, n.Input, nodes);
                        var res = mapExpr(x => Math.Abs(x), i);
                        nodes.Add(node, res);
                        return res;
                    }
                case Core.MinMaxNode n:
                    {
                        Expression left, right;

                        if (n.Left.OutputType == Core.NodeType.Float3) left = compileToVec3(input, n.Left, nodes);
                        else
                        {
                            var fexpr = compileNodeFloat(input, n.Left, nodes);
                            left = Expression.New(vec3ctor, fexpr);
                        }

                        if (n.Right.OutputType == Core.NodeType.Float3) right = compileToVec3(input, n.Right, nodes);
                        else
                        {
                            var fexpr = compileNodeFloat(input, n.Right, nodes);
                            right = Expression.New(vec3ctor, fexpr);
                        }

                        Func<float, float, float> max = Math.Max;
                        Func<float, float, float> min = Math.Min;

                        var res = n.IsMin ? mapExpr((x, y) => Math.Min(x, y), left, right) : mapExpr((x, y) => Math.Max(x, y), left, right);

                        nodes.Add(node, res);
                        return res;
                    }
                case Core.ArithmeticNode n:
                    {
                        Expression left, right;
                        if (n.Left.OutputType == Core.NodeType.Float3)
                        {
                            left = compileToVec3(input, n.Left, nodes);
                        }
                        else
                        {
                            var fexpr = compileNodeFloat(input, n.Left, nodes);
                            left = Expression.New(vec3ctor, fexpr);
                        }

                        if (n.Right.OutputType == Core.NodeType.Float3)
                        {
                            right = compileToVec3(input, n.Right, nodes);
                        }
                        else
                        {
                            var fexpr = compileNodeFloat(input, n.Right, nodes);
                            right = Expression.New(vec3ctor, fexpr);
                        }

                        Expression res;

                        switch (n.Operation)
                        {
                            case Core.ArithOp.Add: res = Expression.Add(left, right); break;
                            case Core.ArithOp.Cross: res = Expression.Call(cross, left, right); break;
                            case Core.ArithOp.Div: res = mapExpr((x, y) => x / y, left, right); break;
                            case Core.ArithOp.Mod: res = mapExpr((x, y) => x - y * (float)Math.Floor(x / y), left, right); break;
                            case Core.ArithOp.Mul: res = mapExpr((x, y) => x * y, left, right); break;
                            case Core.ArithOp.Sub: res = Expression.Subtract(left, right); break;
                            default: throw new NotImplementedException();
                        }
                        nodes.Add(node, res);
                        return res;
                    }
                default: throw new NotImplementedException();
            }
        }

        Expression compileNodeFloat(ParameterExpression input, Core.INode node, Dictionary<Core.INode, Expression> nodes)
        {
            var abs = typeof(Math).GetMethod("Abs", new Type[] { typeof(float) });
            var min = typeof(Math).GetMethod("Min", new Type[] { typeof(float), typeof(float) });
            var max = typeof(Math).GetMethod("Max", new Type[] { typeof(float), typeof(float) });
            var dot = typeof(Vector3).GetMethod("Dot");
            var length = typeof(Vector3).GetMethod("Length");
            var mod = toExpr<float>((x, y) => x - y * (float)Math.Floor(x / y));
            switch (node)
            {
                case Core.FloatConstantNode n: return Expression.Constant(n.Value);
                case Core.AbsNode n:
                    {
                        if (n.OutputType != Core.NodeType.Float)
                        {
                            throw new InvalidOperationException();
                        }
                        var expr = compileNodeFloat(input, n.Input, nodes);
                        var res = Expression.Call(abs, expr);
                        nodes.Add(node, res);
                        return res;
                    }
                case Core.LengthNode n:
                    {
                        switch (n.Input.OutputType)
                        {
                            case Core.NodeType.Float:
                                {
                                    var res = Expression.Call(abs, compileNodeFloat(input, n.Input, nodes));
                                    nodes.Add(node, res);
                                    return res;
                                }
                            case Core.NodeType.Float3:
                                {
                                    var inst = compileToVec3(input, n.Input, nodes);
                                    var res = Expression.Call(inst, length);
                                    nodes.Add(node, res);
                                    return res;
                                }
                            default: throw new InvalidOperationException();
                        }
                    }
                case Core.MinMaxNode n:
                    {
                        var left = compileNodeFloat(input, n.Left, nodes);
                        var right = compileNodeFloat(input, n.Right, nodes);
                        var res = Expression.Call(n.IsMin ? min : max, left, right);
                        nodes.Add(node, res);
                        return res;
                    }
                case Core.ArithmeticNode n:
                    {
                        if (n.Operation == Core.ArithOp.Dot)
                        {
                            var l = compileToVec3(input, n.Left, nodes);
                            var r = compileToVec3(input, n.Right, nodes);
                            var res = Expression.Call(dot, l, r);
                            nodes.Add(node, res);
                            return res;
                        }

                        var left = compileNodeFloat(input, n.Left, nodes);
                        var right = compileNodeFloat(input, n.Right, nodes);
                        Expression result;
                        switch (n.Operation)
                        {
                            case Core.ArithOp.Add: result = Expression.Add(left, right); break;
                            case Core.ArithOp.Div: result = Expression.Divide(left, right); break;
                            case Core.ArithOp.Mul: result = Expression.Multiply(left, right); break;
                            case Core.ArithOp.Sub: result = Expression.Subtract(left, right); break;
                            case Core.ArithOp.Mod: result = Expression.Invoke(mod, left, right); break;
                            default: throw new NotImplementedException();
                        }
                        nodes.Add(node, result);
                        return result;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        Func<Vector3, float> compileToFunc(Core.OutputNode node)
        {
            var input = Expression.Parameter(typeof(Vector3), "pos");
            var body = compileNodeFloat(input, node.Input, new Dictionary<Core.INode, Expression>());
            var lambda = Expression.Lambda<Func<Vector3, float>>(body, input);
            return lambda.Compile();
        }

        void MainRender(float x, float y, float width, float height, out Vector3 outColor, Func<Vector3, float> distFunc)
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

                dist = distFunc(pos); // Evalulate the distance at the current point
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
                    distFunc(pos + yxx) - distFunc(pos - yxx),
                    distFunc(pos + xyx) - distFunc(pos - xyx),
                    distFunc(pos + xxy) - distFunc(pos - xxy)));

                float diffuse = Math.Max(0.0f, Vector3.Dot(-rayDir, normal));
                float specular = (float)Math.Pow(diffuse, 64.0f);
                outColor = new Vector3(diffuse + specular);
            }
            else
            {
                outColor = Vector3.Zero;
            }
        }

        public Image RenderImage(int width, int height, Core.OutputNode node, Func<Vector3, float> func)
        {
            var img = new Bitmap(width, height);

            var data = img.LockBits(new Rectangle(Point.Empty, new Size(width, height)), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* rawBytes = (byte*)data.Scan0;
                int ptr = 0;
                int additional = (data.Stride - width * 3);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Vector3 color;
                        MainRender(x, y, width, height, out color, func);
                        rawBytes[ptr++] = (byte)(Math.Min(color.X, 1.0f) * 255);
                        rawBytes[ptr++] = (byte)(Math.Min(color.Y, 1.0f) * 255);
                        rawBytes[ptr++] = (byte)(Math.Min(color.Z, 1.0f) * 255);
                    }
                    ptr += additional;
                }
            }

            img.UnlockBits(data);

            return img;
        }

        public Task<Image> RenderImageAsync(int width, int height, Core.OutputNode node)
        {
            return Task<Image>.Factory.StartNew(() => RenderImage(width, height, node, compileToFunc(node)));
        }

        public Task<Image> RenderImageSlowAsync(int width, int height, Core.OutputNode node)
        {
            return Task<Image>.Factory.StartNew(() => RenderImage(width, height, node, x => evalNodeFloat(x, node)));
        }
    }
}
