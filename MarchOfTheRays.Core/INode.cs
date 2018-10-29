using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;

namespace MarchOfTheRays.Core
{
    public enum NodeType
    {
        Float,
        Float2,
        Float3,
        Float4,
        Indeterminate,
        Invalid,
        None
    }

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

    public class InvalidNodeException : Exception
    {
        public INode Node { get; private set; }

        public InvalidNodeException(INode node)
            : base()
        {
            Node = node;
        }
    }

    public interface INode : ICloneable
    {
        NodeType OutputType { get; }
        Expression Compile(params Expression[] parameters);
    }

    public interface IUnaryNode : INode
    {
        [Browsable(false)]
        INode Input { get; set; }
    }

    public interface IBinaryNode : INode
    {
        [Browsable(false)]
        INode Left { get; set; }
        [Browsable(false)]
        INode Right { get; set; }
    }

    public interface INAryNode : INode
    {
        INode GetInput(int i);
        void SetInput(int i, INode node);
        int InputCount { get; }

        event EventHandler InputCountChanged;
    }

    [Serializable]
    public class FloatConstantNode : INode
    {
        float m_Value;

        public float Value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        [Browsable(false)]
        public NodeType OutputType => NodeType.Float;

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public object Clone()
        {
            return new FloatConstantNode()
            {
                m_Value = m_Value
            };
        }

        public Expression Compile(params Expression[] parameters)
        {
            return Expression.Constant(m_Value);
        }
    }

    [Serializable]
    public class Float2ConstantNode : INode
    {
        float m_X, m_Y;

        public float X
        {
            get => m_X;
            set
            {
                m_X = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public float Y
        {
            get => m_Y;
            set
            {
                m_Y = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        [Browsable(false)]
        public NodeType OutputType => NodeType.Float2;

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public object Clone()
        {
            return new Float2ConstantNode()
            {
                m_X = m_X,
                m_Y = m_Y
            };
        }

        public Expression Compile(params Expression[] parameters)
        {
            return Expression.Constant(new Vector2(m_X, m_Y));
        }
    }

    [Serializable]
    public class Float3ConstantNode : INode
    {
        float m_X, m_Y, m_Z;

        public float X
        {
            get => m_X;
            set
            {
                m_X = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public float Y
        {
            get => m_Y;
            set
            {
                m_Y = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public float Z
        {
            get => m_Z;
            set
            {
                m_Z = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        [Browsable(false)]
        public NodeType OutputType => NodeType.Float3;

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public object Clone()
        {
            return new Float3ConstantNode()
            {
                m_X = m_X,
                m_Y = m_Y,
                m_Z = m_Z
            };
        }

        public Expression Compile(params Expression[] parameters)
        {
            return Expression.Constant(new Vector3(m_X, m_Y, m_Z));
        }
    }

    [Serializable]
    public class Float4ConstantNode : INode
    {
        float m_X, m_Y, m_Z, m_W;

        public float X
        {
            get => m_X;
            set
            {
                m_X = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public float Y
        {
            get => m_Y;
            set
            {
                m_Y = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public float Z
        {
            get => m_Z;
            set
            {
                m_Z = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public float W
        {
            get => m_W;
            set
            {
                m_W = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        [Browsable(false)]
        public NodeType OutputType => NodeType.Float4;

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public object Clone()
        {
            return new Float4ConstantNode()
            {
                m_X = m_X,
                m_Y = m_Y,
                m_Z = m_Z,
                m_W = m_W
            };
        }

        public Expression Compile(params Expression[] parameters)
        {
            return Expression.Constant(new Vector4(m_X, m_Y, m_Z, m_W));
        }
    }

    public enum BinaryOp
    {
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Dot,
        Cross,
        Atan2,
        Min,
        Max
    }

    public enum UnaryOp
    {
        Abs,
        Length,
        Normalize,
        Invert,
        Sin,
        Cos,
        Tan,
        Asin,
        Acos,
        Atan,
        Exp,
        Radians,
        Degrees,
        Floor,
        Ceil,
        X,
        Y,
        Z
    }

    [Serializable]
    public class UnaryNode : IUnaryNode
    {
        [Browsable(false)]
        public INode Input { get; set; }

        UnaryOp m_Operation;

        public UnaryOp Operation
        {
            get => m_Operation;
            set
            {
                m_Operation = value;
                OperationChanged?.Invoke(this, new EventArgs());
            }
        }

        [Browsable(false)]
        public NodeType OutputType
        {
            get
            {
                if (Input == null
                    || Input.OutputType == NodeType.Indeterminate
                    || Input.OutputType == NodeType.Invalid) return NodeType.Indeterminate;

                if ((m_Operation == UnaryOp.Normalize
                    || m_Operation == UnaryOp.X
                    || m_Operation == UnaryOp.Y
                    || m_Operation == UnaryOp.Z) && Input.OutputType == NodeType.Float) return NodeType.Invalid;


                if (m_Operation == UnaryOp.Length
                    || m_Operation == UnaryOp.X
                    || m_Operation == UnaryOp.Y
                    || m_Operation == UnaryOp.Z)
                {
                    return NodeType.Float;
                }
                else
                {
                    return Input.OutputType;
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler OperationChanged;

        public object Clone()
        {
            return new UnaryNode()
            {
                Input = Input,
                m_Operation = m_Operation
            };
        }

        public Expression Compile(params Expression[] parameters)
        {
            if (Input == null
                || Input.OutputType == NodeType.Indeterminate
                || Input.OutputType == NodeType.Invalid) throw new InvalidNodeException(this);

            var arg = Input.Compile(parameters);
            var inputType = Input.OutputType;
            Expression<Func<float, float>> expr = null;
            Type t = null;

            switch (inputType)
            {
                case NodeType.Float: break;
                case NodeType.Float2: t = typeof(Vector2); break;
                case NodeType.Float3: t = typeof(Vector3); break;
                case NodeType.Float4: t = typeof(Vector4); break;
                default: throw new InvalidNodeException(this);
            }

            switch (m_Operation)
            {
                case UnaryOp.Abs:
                    expr = CompilerTools.ToExpr<float>(x => Math.Abs(x));
                    break;
                case UnaryOp.Acos:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Acos(x));
                    break;
                case UnaryOp.Asin:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Asin(x));
                    break;
                case UnaryOp.Atan:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Atan(x));
                    break;
                case UnaryOp.Ceil:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Ceiling(x));
                    break;
                case UnaryOp.Cos:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Cos(x));
                    break;
                case UnaryOp.Degrees:
                    expr = CompilerTools.ToExpr<float>(x => (float)((180 * x) / Math.PI));
                    break;
                case UnaryOp.Exp:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Exp(x));
                    break;
                case UnaryOp.Floor:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Floor(x));
                    break;
                case UnaryOp.Invert:
                    expr = CompilerTools.ToExpr<float>(x => -x);
                    break;
                case UnaryOp.Radians:
                    expr = CompilerTools.ToExpr<float>(x => (float)((Math.PI * x) / 180.0f));
                    break;
                case UnaryOp.Sin:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Sin(x));
                    break;
                case UnaryOp.Tan:
                    expr = CompilerTools.ToExpr<float>(x => (float)Math.Tan(x));
                    break;
                case UnaryOp.Length:
                    {
                        if(inputType == NodeType.Float) return Expression.Invoke(CompilerTools.ToExpr<float>(x => Math.Abs(x)), arg);
                        return Expression.Call(arg, t.GetMethod("Length"));
                    }
                case UnaryOp.Normalize:
                    {
                        return Expression.Call(null, t.GetMethod("Normalize"), arg);
                    }
                case UnaryOp.X:
                    {
                        return Expression.Field(arg, "X");
                    }
                case UnaryOp.Y:
                    {
                        return Expression.Field(arg, "Y");
                    }
                case UnaryOp.Z:
                    {
                        return Expression.Field(arg, "Z");
                    }
            }

            switch (inputType)
            {
                case NodeType.Float: return Expression.Invoke(expr, arg);
                case NodeType.Float2: return CompilerTools.MapExprFloat2(expr, arg);
                case NodeType.Float3: return CompilerTools.MapExprFloat3(expr, arg);
                case NodeType.Float4: return CompilerTools.MapExprFloat4(expr, arg);
                default: throw new InvalidNodeException(this);
            }
        }
    }

    [Serializable]
    public class BinaryNode : IBinaryNode
    {
        [Browsable(false)]
        public INode Left { get; set; }
        [Browsable(false)]
        public INode Right { get; set; }

        BinaryOp m_Operation;

        public BinaryOp Operation
        {
            get => m_Operation;
            set
            {
                m_Operation = value;
                OperationChanged?.Invoke(this, new EventArgs());
            }
        }

        [Browsable(false)]
        public NodeType OutputType
        {
            get
            {
                if (Left == null || Right == null) return NodeType.Indeterminate;
                if (Left.OutputType == NodeType.Indeterminate
                    || Left.OutputType == NodeType.Invalid
                    || Right.OutputType == NodeType.Indeterminate
                    || Right.OutputType == NodeType.Invalid) return NodeType.Indeterminate;

                if (m_Operation == BinaryOp.Cross)
                {
                    if (Left.OutputType != NodeType.Float3 || Right.OutputType != NodeType.Float3) return NodeType.Invalid;
                    return NodeType.Float3;
                }
                else
                {
                    if (Left.OutputType == NodeType.Float4)
                    {
                        if (Right.OutputType != NodeType.Float4 && Right.OutputType != NodeType.Float) return NodeType.Invalid;
                        return NodeType.Float4;
                    }
                    else if (Left.OutputType == NodeType.Float3)
                    {
                        if (Right.OutputType != NodeType.Float3 && Right.OutputType != NodeType.Float) return NodeType.Invalid;
                        return NodeType.Float3;
                    }
                    else if (Left.OutputType == NodeType.Float2)
                    {
                        if (Right.OutputType != NodeType.Float2 && Right.OutputType != NodeType.Float) return NodeType.Invalid;
                        return NodeType.Float2;
                    }
                    else if (Right.OutputType == NodeType.Float4)
                    {
                        if (Left.OutputType != NodeType.Float4 && Left.OutputType != NodeType.Float) return NodeType.Invalid;
                        return NodeType.Float4;
                    }
                    else if (Right.OutputType == NodeType.Float3)
                    {
                        if (Left.OutputType != NodeType.Float3 && Left.OutputType != NodeType.Float) return NodeType.Invalid;
                        return NodeType.Float3;
                    }
                    else if (Right.OutputType == NodeType.Float2)
                    {
                        if (Left.OutputType != NodeType.Float2 && Left.OutputType != NodeType.Float) return NodeType.Invalid;
                        return NodeType.Float2;
                    }

                    return NodeType.Float;
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler OperationChanged;

        public object Clone()
        {
            return new BinaryNode()
            {
                Left = Left,
                Right = Right,
                m_Operation = m_Operation
            };
        }

        public Expression Compile(params Expression[] parameters)
        {
            if (Left == null || Right == null) throw new InvalidNodeException(this);
            var leftType = Left.OutputType;
            var rightType = Right.OutputType;
            var left = Left.Compile(parameters);
            var right = Right.Compile(parameters);
            if (m_Operation == BinaryOp.Cross)
            {
                if (leftType != NodeType.Float3 || rightType != NodeType.Float3)
                    throw new InvalidNodeException(this);
                var cross = typeof(Vector3).GetMethod("Cross", new Type[] { typeof(Vector3), typeof(Vector3) });
                return Expression.Call(null, cross, left, right);
            }

            NodeType outType = NodeType.Float;

            if (leftType == NodeType.Float4)
            {
                outType = NodeType.Float4;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat4(right);
                else if (rightType != NodeType.Float4) throw new InvalidNodeException(this);
            }
            else if (leftType == NodeType.Float3)
            {
                outType = NodeType.Float3;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat3(right);
                else if (rightType != NodeType.Float3) throw new InvalidNodeException(this);
            }
            else if (leftType == NodeType.Float2)
            {
                outType = NodeType.Float2;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat2(right);
                else if (rightType != NodeType.Float2) throw new InvalidNodeException(this);
            }
            else if (rightType == NodeType.Float4)
            {
                outType = NodeType.Float4;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat4(left);
                else if (leftType != NodeType.Float4) throw new InvalidNodeException(this);
            }
            else if (rightType == NodeType.Float3)
            {
                outType = NodeType.Float3;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat3(left);
                else if (leftType != NodeType.Float3) throw new InvalidNodeException(this);
            }
            else if (rightType == NodeType.Float2)
            {
                outType = NodeType.Float2;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat2(left);
                else if (leftType != NodeType.Float2) throw new InvalidNodeException(this);
            }

            Expression<Func<float, float, float>> expr = null;

            switch (m_Operation)
            {
                case BinaryOp.Add: return Expression.Add(left, right);
                case BinaryOp.Sub: return Expression.Subtract(left, right);
                case BinaryOp.Dot:
                    {
                        if (leftType != rightType) throw new InvalidNodeException(this);
                        Type t = null;
                        switch (leftType)
                        {
                            case NodeType.Float2: t = typeof(Vector2); break;
                            case NodeType.Float3: t = typeof(Vector3); break;
                            case NodeType.Float4: t = typeof(Vector4); break;
                            default: throw new InvalidNodeException(this);
                        }
                        var dot = t.GetMethod("Dot");
                        return Expression.Call(null, dot, left, right);
                    }
                case BinaryOp.Atan2:
                    expr = CompilerTools.ToExpr<float>((x, y) => (float)Math.Atan2(x, y));
                    break;
                case BinaryOp.Div:
                    expr = CompilerTools.ToExpr<float>((x, y) => x / y);
                    break;
                case BinaryOp.Mul:
                    expr = CompilerTools.ToExpr<float>((x, y) => x * y);
                    break;
                case BinaryOp.Mod:
                    expr = CompilerTools.ToExpr<float>((x, y) => x - y * (float)Math.Floor(x / y));
                    break;
                case BinaryOp.Min:
                    expr = CompilerTools.ToExpr<float>((x, y) => Math.Min(x, y));
                    break;
                case BinaryOp.Max:
                    expr = CompilerTools.ToExpr<float>((x, y) => Math.Max(x, y));
                    break;
            }
            switch (outType)
            {
                case NodeType.Float2: return CompilerTools.MapExprFloat2(expr, left, right);
                case NodeType.Float3: return CompilerTools.MapExprFloat3(expr, left, right);
                case NodeType.Float4: return CompilerTools.MapExprFloat4(expr, left, right);
                case NodeType.Float: return Expression.Invoke(expr, left, right);
                default: throw new InvalidNodeException(this);
            }
        }
    }

    [Serializable]
    public class InputNode : INode
    {
        public NodeType OutputType { get; set; }

        public int InputNumber { get; set; }

        public object Clone()
        {
            return new InputNode()
            {
                OutputType = OutputType,
                InputNumber = InputNumber
            };
        }

        public Expression Compile(params Expression[] parameters)
        {
            return parameters[InputNumber];
        }
    }

    [Serializable]
    public class OutputNode : IUnaryNode
    {
        [Browsable(false)]
        public INode Input { get; set; }

        public NodeType OutputType => NodeType.None;

        public object Clone()
        {
            return new OutputNode()
            {
                Input = Input
            };
        }

        public Expression Compile(params Expression[] parameters)
        {
            if (Input == null) throw new InvalidNodeException(this);
            return Input.Compile(parameters);
        }
    }
}
