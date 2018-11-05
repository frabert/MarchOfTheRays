using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;

namespace MarchOfTheRays.Core
{
    static class CompilerTools
    {
        public static Expression<Func<T, T>> ToExpr<T>(Expression<Func<T, T>> f)
        {
            return f;
        }

        public static Expression<Func<T, T, T>> ToExpr<T>(Expression<Func<T, T, T>> f)
        {
            return f;
        }

        public static Expression MapExprFloat2(Expression<Func<float, float>> f, Expression inp)
        {
            var vec2ctor = typeof(Vector2).GetConstructor(new Type[] { typeof(float), typeof(float) });
            var x = Expression.Field(inp, "X");
            var y = Expression.Field(inp, "Y");
            return Expression.New(vec2ctor, Expression.Invoke(f, x), Expression.Invoke(f, y));
        }

        public static Expression MapExprFloat2(Expression<Func<float, float, float>> f, Expression l, Expression r)
        {
            var vec2ctor = typeof(Vector2).GetConstructor(new Type[] { typeof(float), typeof(float) });
            var lx = Expression.Field(l, "X");
            var ly = Expression.Field(l, "Y");

            var rx = Expression.Field(r, "X");
            var ry = Expression.Field(r, "Y");
            return Expression.New(vec2ctor, Expression.Invoke(f, lx, rx), Expression.Invoke(f, ly, ry));
        }

        public static Expression MapExprFloat3(Expression<Func<float, float>> f, Expression inp)
        {
            var vec3ctor = typeof(Vector3).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float) });
            var x = Expression.Field(inp, "X");
            var y = Expression.Field(inp, "Y");
            var z = Expression.Field(inp, "Z");
            return Expression.New(vec3ctor, Expression.Invoke(f, x), Expression.Invoke(f, y), Expression.Invoke(f, z));
        }

        public static Expression MapExprFloat3(Expression<Func<float, float, float>> f, Expression l, Expression r)
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

        public static Expression MapExprFloat4(Expression<Func<float, float>> f, Expression inp)
        {
            var vec4ctor = typeof(Vector4).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) });
            var x = Expression.Field(inp, "X");
            var y = Expression.Field(inp, "Y");
            var z = Expression.Field(inp, "Z");
            var w = Expression.Field(inp, "W");
            return Expression.New(vec4ctor, Expression.Invoke(f, x), Expression.Invoke(f, y), Expression.Invoke(f, z), Expression.Invoke(f, w));
        }

        public static Expression MapExprFloat4(Expression<Func<float, float, float>> f, Expression l, Expression r)
        {
            var vec4ctor = typeof(Vector4).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) });
            var lx = Expression.Field(l, "X");
            var ly = Expression.Field(l, "Y");
            var lz = Expression.Field(l, "Z");
            var lw = Expression.Field(l, "W");

            var rx = Expression.Field(r, "X");
            var ry = Expression.Field(r, "Y");
            var rz = Expression.Field(r, "Z");
            var rw = Expression.Field(r, "W");
            return Expression.New(vec4ctor, Expression.Invoke(f, lx, rx), Expression.Invoke(f, ly, ry), Expression.Invoke(f, lz, rz), Expression.Invoke(f, lw, rw));
        }

        public static Expression FloatToFloat2(Expression a)
        {
            var vec2ctor = typeof(Vector2).GetConstructor(new Type[] { typeof(float) });
            return Expression.New(vec2ctor, a);
        }

        public static Expression FloatToFloat3(Expression a)
        {
            var vec3ctor = typeof(Vector3).GetConstructor(new Type[] { typeof(float) });
            return Expression.New(vec3ctor, a);
        }

        public static Expression FloatToFloat4(Expression a)
        {
            var vec4ctor = typeof(Vector4).GetConstructor(new Type[] { typeof(float) });
            return Expression.New(vec4ctor, a);
        }
    }

    public static class Compiler
    {
        enum NodeColor
        {
            White,
            Gray,
            Black
        }

        /// <summary>
        /// Search for cycles in the node.
        /// Returns a list containing the nodes belonging to a cycle, or an empty list if none are found.
        /// </summary>
        /// <param name="outputNode">The node from which to start the search</param>
        /// <param name="nodes">The graph to scan</param>
        /// <returns>A list of nodes belonging to a cycle</returns>
        public static IList<INode> CheckForCycles(OutputNode outputNode, HashSet<INode> nodes)
        {
            var colors = new Dictionary<INode, NodeColor>();
            var parents = new Dictionary<INode, INode>();
            var cycle = new List<INode>();
            var stack = new Stack<INode>();
            INode cycleStart = null;

            foreach (var node in nodes)
            {
                colors.Add(node, NodeColor.White);
            }

            foreach (var node in nodes)
            {
                if (colors[node] == NodeColor.White) DFSVisit(node);
            }

            void DFSVisit(INode n)
            {
                colors[n] = NodeColor.Gray;
                var edges = new List<INode>();
                switch (n)
                {
                    case IUnaryNode u:
                        if (u.Input != null) edges.Add(u.Input);
                        break;
                    case IBinaryNode b:
                        if (b.Left != null) edges.Add(b.Left);
                        if (b.Right != null) edges.Add(b.Right);
                        break;
                    case INAryNode a:
                        for (int i = 0; i < a.InputCount; i++)
                        {
                            if (a.GetInput(i) != null) edges.Add(a.GetInput(i));
                        }
                        break;
                }

                foreach (var edge in edges)
                {
                    if (colors[edge] == NodeColor.White)
                    {
                        colors[edge] = NodeColor.Gray;
                        parents[edge] = n;
                        DFSVisit(edge);
                    }
                    else if (colors[edge] == NodeColor.Gray)
                    {
                        cycleStart = edge;
                        parents[edge] = n;
                    }
                }
                colors[n] = NodeColor.Black;
            }

            if (cycleStart != null)
            {
                cycle.Add(cycleStart);
                var parent = parents[cycleStart];
                while (parent != cycleStart)
                {
                    cycle.Add(parent);
                    parent = parents[parent];
                }
            }

            return cycle;
        }
    }
}
