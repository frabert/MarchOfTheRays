using MarchOfTheRays.Core.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace MarchOfTheRays.Core
{
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string resourceId)
            : base(GetMessageFromResource(resourceId))
        { }

        private static string GetMessageFromResource(string resourceId)
        {
            var type = typeof(Strings);
            var prop = type.GetProperty(resourceId, BindingFlags.NonPublic | BindingFlags.Static);
            var value = prop.GetValue(null);
            return (string)value;
        }
    }

    public class LocalizedEnumConverter : EnumConverter
    {
        public LocalizedEnumConverter(Type type) : base(type) { }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            var oldCulture = Strings.Culture;
            Strings.Culture = culture;

            var name = Enum.GetName(EnumType, value);
            var type = typeof(Strings);
            var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Static);
            var str = prop.GetValue(null);

            Strings.Culture = oldCulture;

            return str;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var type = typeof(Strings);

            foreach (var name in Enum.GetNames(EnumType))
            {
                var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Static);
                var str = (string)prop.GetValue(null);
                if (str == (string)value)
                {
                    return Enum.Parse(EnumType, name);
                }
            }

            return Enum.Parse(EnumType, (string)value);
        }
    }

    [TypeConverter(typeof(LocalizedEnumConverter))]
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
        event EventHandler OutputTypeChanged;
        Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters);
        void InitializeEvents();
    }

    [Serializable]
    public abstract class Node : INode
    {
        NodeType outType = NodeType.Indeterminate;

        [ReadOnly(true)]
        [LocalizedDisplayName("OutputType")]
        public virtual NodeType OutputType
        {
            get => outType;
            protected set
            {
                var oldType = outType;
                outType = value;
                if (oldType != outType) OutputTypeChanged?.Invoke(this, new EventArgs());
            }
        }

        [field: NonSerialized]
        public event EventHandler OutputTypeChanged;

        public abstract Node Clone();

        public abstract Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters);

        public virtual void InitializeEvents() { }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public interface IUnaryNode : INode
    {
        [Browsable(false)]
        INode Input { get; set; }
    }

    [Serializable]
    public abstract class UnaryNode : Node, IUnaryNode
    {
        INode input;

        [Browsable(false)]
        public INode Input
        {
            get => input;
            set
            {
                if (input != null) input.OutputTypeChanged -= InputChanged;
                input = value;
                if (input != null) input.OutputTypeChanged += InputChanged;
                OnInputChanged();
            }
        }

        [LocalizedDisplayName("InputType")]
        public NodeType InputType => input == null ? NodeType.None : input.OutputType;

        void InputChanged(object s, EventArgs e)
        {
            if (s == input) OnInputChanged();
        }

        public override void InitializeEvents()
        {
            if (input != null) input.OutputTypeChanged += InputChanged;
            base.InitializeEvents();
        }

        protected abstract void OnInputChanged();
    }

    public interface IBinaryNode : INode
    {
        [Browsable(false)]
        INode Left { get; set; }
        [Browsable(false)]
        INode Right { get; set; }
    }

    [Serializable]
    public abstract class BinaryNode : Node, IBinaryNode
    {
        INode left, right;

        [Browsable(false)]
        public INode Left
        {
            get => left;
            set
            {
                if (left != null) left.OutputTypeChanged -= InputChanged;
                left = value;
                if (left != null) left.OutputTypeChanged += InputChanged;
                OnInputsChanged();
            }
        }

        [Browsable(false)]
        public INode Right
        {
            get => right;
            set
            {
                right = value;
                OnInputsChanged();
            }
        }

        [LocalizedDisplayName("LeftInputType")]
        public NodeType LeftInputType => left == null ? NodeType.None : left.OutputType;

        [LocalizedDisplayName("RightInputType")]
        public NodeType RightInputType => right == null ? NodeType.None : right.OutputType;

        void InputChanged(object sender, EventArgs e)
        {
            if (sender == left || sender == right) OnInputsChanged();
        }

        protected abstract void OnInputsChanged();

        public override void InitializeEvents()
        {
            if (left != null) left.OutputTypeChanged += InputChanged;
            if (right != null) right.OutputTypeChanged += InputChanged;
            base.InitializeEvents();
        }
    }

    public interface INAryNode : INode
    {
        INode GetInput(int i);
        void SetInput(int i, INode node);
        int InputCount { get; }

        event EventHandler InputCountChanged;
    }

    public interface ICompositeNode : INode
    {
        string Name { get; set; }
        event EventHandler NameChanged;
    }

    [Serializable]
    public class FloatConstantNode : Node
    {
        public FloatConstantNode()
        {
            OutputType = NodeType.Float;
        }

        float m_Value;

        [LocalizedDisplayName("Value")]
        public float Value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public override Node Clone()
        {
            return new FloatConstantNode()
            {
                m_Value = m_Value
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            var expr = Expression.Constant(m_Value);
            nodeDictionary[this] = expr;
            return expr;
        }
    }

    [Serializable]
    public class Float2ConstantNode : Node
    {
        public Float2ConstantNode()
        {
            OutputType = NodeType.Float2;
        }

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

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public override Node Clone()
        {
            return new Float2ConstantNode()
            {
                m_X = m_X,
                m_Y = m_Y
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            var expr = Expression.Constant(new Vector2(m_X, m_Y));
            nodeDictionary[this] = expr;
            return expr;
        }
    }

    [Serializable]
    public class Float3ConstantNode : Node
    {
        public Float3ConstantNode()
        {
            OutputType = NodeType.Float3;
        }

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

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public override Node Clone()
        {
            return new Float3ConstantNode()
            {
                m_X = m_X,
                m_Y = m_Y,
                m_Z = m_Z
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            var expr = Expression.Constant(new Vector3(m_X, m_Y, m_Z));
            nodeDictionary[this] = expr;
            return expr;
        }
    }

    [Serializable]
    public class Float4ConstantNode : INode
    {
        [field: NonSerialized]
        public event EventHandler OutputTypeChanged;

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

        [ReadOnly(true)]
        [LocalizedDisplayName("OutputType")]
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

        public Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            var expr = Expression.Constant(new Vector4(m_X, m_Y, m_Z, m_W));
            nodeDictionary[this] = expr;
            return expr;
        }

        public void InitializeEvents()
        {

        }
    }

    [TypeConverter(typeof(LocalizedEnumConverter))]
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

    [TypeConverter(typeof(LocalizedEnumConverter))]
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
    public class UnaryOperationNode : UnaryNode
    {
        NodeType CalcType()
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

        UnaryOp m_Operation;

        [LocalizedDisplayName("Operation")]
        public UnaryOp Operation
        {
            get => m_Operation;
            set
            {
                m_Operation = value;
                CalcType();
                OperationChanged?.Invoke(this, new EventArgs());
            }
        }

        [field: NonSerialized]
        public event EventHandler OperationChanged;

        public override Node Clone()
        {
            return new UnaryOperationNode()
            {
                Input = Input,
                m_Operation = m_Operation
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Input == null) throw new InvalidNodeException(this);

            if (nodeDictionary.TryGetValue(this, out var result)) return result;

            var arg = Input.Compile(nodeDictionary, parameters);

            var inputType = Input.OutputType;
            Expression<Func<float, float>> expr = null;
            Type t = null;

            switch (inputType)
            {
                case NodeType.Float: break;
                case NodeType.Float2: t = typeof(Vector2); break;
                case NodeType.Float3: t = typeof(Vector3); break;
                case NodeType.Float4: t = typeof(Vector4); break;
                default: throw new InvalidNodeException(Input);
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
                        Expression e;
                        if (inputType == NodeType.Float) e = Expression.Invoke(CompilerTools.ToExpr<float>(x => Math.Abs(x)), arg);
                        e = Expression.Call(arg, t.GetMethod("Length"));
                        nodeDictionary[this] = e;
                        return e;
                    }
                case UnaryOp.Normalize:
                    {
                        var e = Expression.Call(null, t.GetMethod("Normalize"), arg);
                        nodeDictionary[this] = e;
                        return e;
                    }
                case UnaryOp.X:
                    {
                        var e = Expression.Field(arg, "X");
                        nodeDictionary[this] = e;
                        return e;
                    }
                case UnaryOp.Y:
                    {
                        var e = Expression.Field(arg, "Y");
                        nodeDictionary[this] = e;
                        return e;
                    }
                case UnaryOp.Z:
                    {
                        var e = Expression.Field(arg, "Z");
                        nodeDictionary[this] = e;
                        return e;
                    }
            }

            Expression res;
            switch (inputType)
            {
                case NodeType.Float:
                    {
                        res = Expression.Invoke(expr, arg);
                        break;
                    }
                case NodeType.Float2:
                    {
                        res = CompilerTools.MapExprFloat2(expr, arg);
                        break;
                    }
                case NodeType.Float3:
                    {
                        res = CompilerTools.MapExprFloat3(expr, arg);
                        break;
                    }
                case NodeType.Float4:
                    {
                        res = CompilerTools.MapExprFloat4(expr, arg);
                        break;
                    }
                default: throw new InvalidNodeException(Input);
            }
            nodeDictionary[this] = res;
            return res;
        }

        protected override void OnInputChanged()
        {
            OutputType = CalcType();
        }
    }

    [Serializable]
    public class BinaryOperationNode : BinaryNode
    {
        BinaryOp m_Operation;

        [LocalizedDisplayName("Operation")]
        public BinaryOp Operation
        {
            get => m_Operation;
            set
            {
                m_Operation = value;
                OperationChanged?.Invoke(this, new EventArgs());
            }
        }

        NodeType CalcType()
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

        [field: NonSerialized]
        public event EventHandler OperationChanged;

        public override Node Clone()
        {
            return new BinaryOperationNode()
            {
                Left = Left,
                Right = Right,
                m_Operation = m_Operation
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Left == null || Right == null) throw new InvalidNodeException(this);
            if (nodeDictionary.TryGetValue(this, out var result)) return result;

            var leftType = Left.OutputType;
            var rightType = Right.OutputType;
            var left = Left.Compile(nodeDictionary, parameters);
            var right = Right.Compile(nodeDictionary, parameters);
            if (m_Operation == BinaryOp.Cross)
            {
                if (leftType != NodeType.Float3) throw new InvalidNodeException(Left);
                if (rightType != NodeType.Float3) throw new InvalidNodeException(Right);
                var cross = typeof(Vector3).GetMethod("Cross", new Type[] { typeof(Vector3), typeof(Vector3) });
                return Expression.Call(null, cross, left, right);
            }

            NodeType outType = NodeType.Float;

            if (leftType == NodeType.Float4)
            {
                outType = NodeType.Float4;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat4(right);
                else if (rightType != NodeType.Float4) throw new InvalidNodeException(Right);
            }
            else if (leftType == NodeType.Float3)
            {
                outType = NodeType.Float3;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat3(right);
                else if (rightType != NodeType.Float3) throw new InvalidNodeException(Right);
            }
            else if (leftType == NodeType.Float2)
            {
                outType = NodeType.Float2;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat2(right);
                else if (rightType != NodeType.Float2) throw new InvalidNodeException(Right);
            }
            else if (rightType == NodeType.Float4)
            {
                outType = NodeType.Float4;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat4(left);
                else if (leftType != NodeType.Float4) throw new InvalidNodeException(Left);
            }
            else if (rightType == NodeType.Float3)
            {
                outType = NodeType.Float3;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat3(left);
                else if (leftType != NodeType.Float3) throw new InvalidNodeException(Left);
            }
            else if (rightType == NodeType.Float2)
            {
                outType = NodeType.Float2;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat2(left);
                else if (leftType != NodeType.Float2) throw new InvalidNodeException(Left);
            }

            Expression<Func<float, float, float>> expr = null;

            switch (m_Operation)
            {
                case BinaryOp.Add:
                    {
                        result = Expression.Add(left, right);
                        nodeDictionary[this] = result;
                        return result;
                    }
                case BinaryOp.Sub:
                    {
                        result = Expression.Subtract(left, right);
                        nodeDictionary[this] = result;
                        return result;
                    }
                case BinaryOp.Dot:
                    {
                        if (leftType != rightType) throw new InvalidNodeException(this);
                        Type t = null;
                        switch (leftType)
                        {
                            case NodeType.Float2: t = typeof(Vector2); break;
                            case NodeType.Float3: t = typeof(Vector3); break;
                            case NodeType.Float4: t = typeof(Vector4); break;
                            default: throw new InvalidNodeException(Left);
                        }
                        var dot = t.GetMethod("Dot");
                        result = Expression.Call(null, dot, left, right);
                        nodeDictionary[this] = result;
                        return result;
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
                case NodeType.Float2: result = CompilerTools.MapExprFloat2(expr, left, right); break;
                case NodeType.Float3: result = CompilerTools.MapExprFloat3(expr, left, right); break;
                case NodeType.Float4: result = CompilerTools.MapExprFloat4(expr, left, right); break;
                case NodeType.Float: result = Expression.Invoke(expr, left, right); break;
                default: throw new InvalidNodeException(this);
            }
            nodeDictionary[this] = result;
            return result;
        }

        protected override void OnInputsChanged()
        {
            OutputType = CalcType();
        }
    }

    [Serializable]
    public class InputNode : Node
    {
        [ReadOnly(true)]
        [LocalizedDisplayName("OutputType")]
        public new NodeType OutputType
        {
            get => base.OutputType;
            set
            {
                base.OutputType = value;
            }
        }

        [Browsable(false)]
        public int InputNumber { get; set; }

        public override Node Clone()
        {
            return new InputNode()
            {
                OutputType = OutputType,
                InputNumber = InputNumber
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            return parameters[InputNumber];
        }
    }

    [Serializable]
    public class CompositeUnaryNode : UnaryNode, ICompositeNode
    {
        public CompositeUnaryNode()
            : base()
        {
            InputNode = new InputNode() { OutputType = NodeType.Indeterminate };
        }

        void CalcType()
        {
            if (Input == null)
            {
                InputNode.OutputType = NodeType.Indeterminate;
                OutputType = NodeType.Indeterminate;
            }
            else
            {
                InputNode.OutputType = Input.OutputType;
                OutputType = Body.OutputType;
            }
        }

        OutputNode onode;

        [Browsable(false)]
        public OutputNode Body
        {
            get => onode;
            set
            {
                onode = value;
                if (onode != null)
                {
                    onode.OutputTypeChanged += (s, e) =>
                    {
                        if (s == onode) CalcType();
                    };
                }
            }
        }

        [Browsable(false)]
        public InputNode InputNode { get; private set; }

        string m_Name;

        [LocalizedDisplayName("Name")]
        public string Name
        {
            get => m_Name;
            set
            {
                m_Name = value;
                NameChanged?.Invoke(this, new EventArgs());
            }
        }

        [field: NonSerialized]
        public event EventHandler NameChanged;

        public override Node Clone()
        {
            return new CompositeUnaryNode()
            {
                Input = Input,
                Body = Body,
                Name = Name
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Input == null || Body == null) throw new InvalidNodeException(this);

            if (nodeDictionary.TryGetValue(this, out var result)) return result;

            var arg = Input.Compile(nodeDictionary, parameters);
            result = Body.Compile(nodeDictionary, arg);
            nodeDictionary[this] = result;
            return result;
        }

        public override void InitializeEvents()
        {
            if (onode != null)
            {
                onode.OutputTypeChanged += (s, e) =>
                {
                    if (s == onode) CalcType();
                };
            }
            base.InitializeEvents();
        }

        protected override void OnInputChanged()
        {
            CalcType();
        }
    }

    [Serializable]
    public class CompositeBinaryNode : BinaryNode, ICompositeNode
    {
        public CompositeBinaryNode()
        {
            LeftNode = new InputNode() { OutputType = NodeType.Indeterminate };
            RightNode = new InputNode() { OutputType = NodeType.Indeterminate, InputNumber = 1 };
        }

        void CalcType()
        {
            if (Body == null) OutputType = NodeType.Invalid;

            if (Left == null)
            {
                LeftNode.OutputType = NodeType.Indeterminate;
            }
            else
            {
                LeftNode.OutputType = Left.OutputType;
            }

            if (Right == null)
            {
                RightNode.OutputType = NodeType.Indeterminate;
            }
            else
            {
                RightNode.OutputType = Right.OutputType;
            }

            OutputType = Body.OutputType;
        }

        OutputNode onode;

        [Browsable(false)]
        public OutputNode Body
        {
            get => onode;
            set
            {
                onode = value;
                if (onode != null)
                {
                    onode.OutputTypeChanged += (s, e) =>
                    {
                        if (s == onode) CalcType();
                    };
                }
            }
        }

        [Browsable(false)]
        public InputNode LeftNode { get; private set; }

        [Browsable(false)]
        public InputNode RightNode { get; private set; }

        string m_Name;

        [LocalizedDisplayName("Name")]
        public string Name
        {
            get => m_Name;
            set
            {
                m_Name = value;
                NameChanged?.Invoke(this, new EventArgs());
            }
        }

        [field: NonSerialized]
        public event EventHandler NameChanged;

        public override Node Clone()
        {
            return new CompositeBinaryNode()
            {
                Body = Body,
                Left = Left,
                Right = Right,
                Name = Name
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Left == null || Right == null || Body == null)
                throw new InvalidNodeException(this);
            if (nodeDictionary.TryGetValue(this, out var result)) return result;

            var l = Left.Compile(nodeDictionary, parameters);
            var r = Right.Compile(nodeDictionary, parameters);
            result = Body.Compile(nodeDictionary, l, r);
            nodeDictionary[this] = result;
            return result;
        }

        protected override void OnInputsChanged()
        {
            CalcType();
        }

        public override void InitializeEvents()
        {
            if (onode != null)
            {
                onode.OutputTypeChanged += (s, e) =>
                {
                    if(s == onode) CalcType();
                };
            }
            base.InitializeEvents();
        }
    }

    [Serializable]
    public class Float3Constructor : Node, INAryNode
    {
        public int InputCount => 3;

        INode[] inputs = new INode[3];

        void UpdateType()
        {
            if (inputs[0] == null || inputs[1] == null || inputs[2] == null
                || inputs[0].OutputType != NodeType.Float
                || inputs[1].OutputType != NodeType.Float
                || inputs[2].OutputType != NodeType.Float)
            {
                OutputType = NodeType.Invalid;
            }
            else
            {
                OutputType = NodeType.Float3;
            }
        }

        void InputsChanged(object s, EventArgs e)
        {
            UpdateType();
        }

        [field: NonSerialized]
        public event EventHandler InputCountChanged;

        public override Node Clone()
        {
            return new Float3Constructor()
            {
                inputs = new INode[] { inputs[0], inputs[1], inputs[2] },
                OutputType = OutputType
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (OutputType == NodeType.Invalid) throw new InvalidNodeException(this);
            if (nodeDictionary.TryGetValue(this, out var result)) return result;
            var arg1 = inputs[0].Compile(nodeDictionary, parameters);
            var arg2 = inputs[1].Compile(nodeDictionary, parameters);
            var arg3 = inputs[2].Compile(nodeDictionary, parameters);

            var tfloat = typeof(float);
            var tfloat3 = typeof(Vector3);

            var ctor = tfloat3.GetConstructor(new Type[] { tfloat, tfloat, tfloat });
            result = Expression.New(ctor, arg1, arg2, arg3);
            nodeDictionary[this] = result;
            return result;
        }

        public INode GetInput(int i)
        {
            return inputs[i];
        }

        public void SetInput(int i, INode node)
        {
            if (inputs[i] != null) inputs[i].OutputTypeChanged -= InputsChanged;
            inputs[i] = node;
            UpdateType();
            if (node == null) node.OutputTypeChanged += InputsChanged;
        }

        public override void InitializeEvents()
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i] != null) inputs[i].OutputTypeChanged += InputsChanged;
            }
            base.InitializeEvents();
        }
    }

    [Serializable]
    public class OutputNode : UnaryNode
    {
        void CalcType()
        {
            if (Input == null) OutputType = NodeType.Indeterminate;
            else OutputType = Input.OutputType;
        }

        public override Node Clone()
        {
            return new OutputNode()
            {
                Input = Input
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Input == null) throw new InvalidNodeException(this);
            return Input.Compile(nodeDictionary, parameters);
        }

        public Expression Compile(NodeType wantedtype, Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Input == null) throw new InvalidNodeException(this);
            var compiledInput = Input.Compile(nodeDictionary, parameters);
            if (Input.OutputType != wantedtype) throw new InvalidNodeException(Input);
            return compiledInput;
        }

        protected override void OnInputChanged()
        {
            CalcType();
        }
    }
}
