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
    /// <summary>
    /// Displays a property's name according to its localized equivalent found in <see cref="Strings"/>
    /// </summary>
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

    /// <summary>
    /// Displays an enum's value according to its localized equivalent found in <see cref="Strings"/>
    /// </summary>
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

    /// <summary>
    /// The type of a node's output
    /// </summary>
    [TypeConverter(typeof(LocalizedEnumConverter))]
    public enum NodeType
    {
        /// <summary>
        /// A floating point value
        /// </summary>
        Float,

        /// <summary>
        /// A 2-tuple of float values
        /// </summary>
        Float2,

        /// <summary>
        /// A 3-tuple of float values
        /// </summary>
        Float3,

        /// <summary>
        /// A 4-tuple of float values
        /// </summary>
        Float4,

        /// <summary>
        /// The type cannot be determined
        /// </summary>
        Indeterminate,

        /// <summary>
        /// The type is not valid
        /// </summary>
        Invalid,

        /// <summary>
        /// No output type
        /// </summary>
        None
    }

    /// <summary>
    /// This exception is thrown during compilation, when a node contains invalid inputs or parameters
    /// </summary>
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
        /// <summary>
        /// The output type of the node
        /// </summary>
        NodeType OutputType { get; }

        /// <summary>
        /// Compiles the node into a Linq expression
        /// </summary>
        /// <param name="nodeDictionary">A cache of already compiled nodes</param>
        /// <param name="parameters">A list of input parameters as expressions</param>
        /// <returns>The compiled node</returns>
        /// <exception cref="InvalidNodeException">Thrown if the graph contains invalid nodes</exception>
        Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters);
    }

    /// <summary>
    /// Provides some helper properties and events to <see cref="INode"/>
    /// </summary>
    [Serializable]
    public abstract class Node : INode
    {
        bool calculatingType = false;
        [LocalizedDisplayName("OutputType")]
        public NodeType OutputType
        {
            get
            {
                if (calculatingType) return NodeType.Invalid;
                calculatingType = true;
                var type = GetOutputType();
                calculatingType = false;
                return type;
            }
        }

        protected abstract NodeType GetOutputType();

        public abstract Node Clone();

        public abstract Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters);

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    /// <summary>
    /// A node with a single input value
    /// </summary>
    public interface IUnaryNode : INode
    {
        [Browsable(false)]
        INode Input { get; set; }
    }

    /// <summary>
    /// Provides some helper methods and properties to <see cref="IUnaryNode"/>
    /// </summary>
    [Serializable]
    public abstract class UnaryNode : Node, IUnaryNode
    {
        [Browsable(false)]
        public INode Input { get; set; }

        [LocalizedDisplayName("InputType")]
        public NodeType InputType => Input == null ? NodeType.None : Input.OutputType;
    }

    /// <summary>
    /// A node with two input values
    /// </summary>
    public interface IBinaryNode : INode
    {
        [Browsable(false)]
        INode Left { get; set; }
        [Browsable(false)]
        INode Right { get; set; }
    }

    /// <summary>
    /// Provides some helper methods and properties to <see cref="IBinaryNode"/>
    /// </summary>
    [Serializable]
    public abstract class BinaryNode : Node, IBinaryNode
    {
        [Browsable(false)]
        public INode Left { get; set; }

        [Browsable(false)]
        public INode Right { get; set; }

        [LocalizedDisplayName("LeftInputType")]
        public NodeType LeftInputType => Left == null ? NodeType.None : Left.OutputType;

        [LocalizedDisplayName("RightInputType")]
        public NodeType RightInputType => Right == null ? NodeType.None : Right.OutputType;
    }

    /// <summary>
    /// A node with an arbitrary number of input values
    /// </summary>
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

        protected override NodeType GetOutputType() => NodeType.Float;

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
        protected override NodeType GetOutputType() => NodeType.Float2;

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
        protected override NodeType GetOutputType() => NodeType.Float3;

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

        public void CalcType()
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
        Z,
        W
    }

    [Serializable]
    public class UnaryOperationNode : UnaryNode
    {
        protected override NodeType GetOutputType()
        {
            if (Input == null
                    || Input.OutputType == NodeType.Indeterminate
                    || Input.OutputType == NodeType.Invalid)
            {
                return NodeType.Indeterminate;
            }

            // Normalization is not defined on scalars
            if ((Operation == UnaryOp.Normalize
                || Operation == UnaryOp.X
                || Operation == UnaryOp.Y
                || Operation == UnaryOp.Z
                || Operation == UnaryOp.W) && Input.OutputType == NodeType.Float)
            {
                return NodeType.Invalid;
            }

            // All unary operations preserve the input type,
            // except these.
            if (Operation == UnaryOp.Length
                || Operation == UnaryOp.X
                || Operation == UnaryOp.Y
                || Operation == UnaryOp.Z
                || Operation == UnaryOp.W)
            {
                return NodeType.Float;
            }
            else
            {
                return Input.OutputType;
            }
        }

        UnaryOp op;

        [LocalizedDisplayName("Operation")]
        public UnaryOp Operation
        {
            get => op;
            set
            {
                op = value;
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
                Operation = Operation
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Input == null) throw new InvalidNodeException(this);

            if (nodeDictionary.TryGetValue(this, out var result)) return result;

            var arg = Input.Compile(nodeDictionary, parameters);

            var inputType = Input.OutputType;
            Type t;
            switch (inputType)
            {
                case NodeType.Float: t = typeof(float); break;
                case NodeType.Float2: t = typeof(Vector2); break;
                case NodeType.Float3: t = typeof(Vector3); break;
                case NodeType.Float4: t = typeof(Vector4); break;
                default: throw new InvalidNodeException(this);
            }

            Expression MathExpr(string name, Type c = null)
            {
                c = c == null ? (t == typeof(float) ? typeof(Math) : t) : c;
                var method = c.GetMethod(name, new Type[] { t });
                if (method == null) throw new InvalidNodeException(this);
                return Expression.Call(method, arg);
            }

            Expression InstanceExpr(string Name)
            {
                var method = t.GetMethod(Name);
                if (method == null) throw new InvalidNodeException(this);
                return Expression.Call(arg, method);
            }

            Expression FieldExpr(string Name)
            {
                var field = t.GetField(Name);
                if (field == null) throw new InvalidNodeException(this);
                return Expression.Field(arg, field);
            }

            var mathEx = typeof(MathExtensions);

            Expression res;

            switch (Operation)
            {
                case UnaryOp.Abs: res = MathExpr("Abs"); break;
                case UnaryOp.Acos: res = MathExpr("Acos", mathEx); break;
                case UnaryOp.Asin: res = MathExpr("Asin", mathEx); break;
                case UnaryOp.Atan: res = MathExpr("Atan", mathEx); break;
                case UnaryOp.Ceil: res = MathExpr("Ceil", mathEx); break;
                case UnaryOp.Cos: res = MathExpr("Cos", mathEx); break;
                case UnaryOp.Degrees: res = MathExpr("Degrees", mathEx); break;
                case UnaryOp.Exp: res = MathExpr("Exp", mathEx); break;
                case UnaryOp.Floor: res = MathExpr("Floor", mathEx); break;
                case UnaryOp.Invert: res = Expression.Negate(arg); break;
                case UnaryOp.Length: res = InstanceExpr("Length"); break;
                case UnaryOp.Normalize: res = MathExpr("Normalize"); break;
                case UnaryOp.Radians: res = MathExpr("Radians", mathEx); break;
                case UnaryOp.Sin: res = MathExpr("Sin", mathEx); break;
                case UnaryOp.Tan: res = MathExpr("Tan", mathEx); break;
                case UnaryOp.X: res = FieldExpr("X"); break;
                case UnaryOp.Y: res = FieldExpr("Y"); break;
                case UnaryOp.Z: res = FieldExpr("Z"); break;
                case UnaryOp.W: res = FieldExpr("W"); break;
                default: throw new NotImplementedException();
            }

            if (arg is ConstantExpression exp)
            {
                res = Expression.Constant(Expression.Lambda(res).Compile().DynamicInvoke());
            }

            nodeDictionary[this] = res;
            return res;
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

        protected override NodeType GetOutputType()
        {

            if (Left == null || Right == null)
            {
                return NodeType.Indeterminate;
            }
            if (Left.OutputType == NodeType.Indeterminate
                || Left.OutputType == NodeType.Invalid
                || Right.OutputType == NodeType.Indeterminate
                || Right.OutputType == NodeType.Invalid)
            {
                return NodeType.Indeterminate;
            }

            // Cross is only defined on Float3 inputs
            if (m_Operation == BinaryOp.Cross)
            {
                if (Left.OutputType != NodeType.Float3 || Right.OutputType != NodeType.Float3) return NodeType.Invalid;
                else return NodeType.Float3;
            }
            else if (m_Operation == BinaryOp.Dot)
            {
                if (Left.OutputType != Right.OutputType) return NodeType.Invalid;
                else return NodeType.Float;
            }
            else
            {
                // In case one input is a vector and the other is scalar,
                // the scalar is converted into a vector of the same size.
                // All other combinations are not supported
                if (Left.OutputType == NodeType.Float4)
                {
                    if (Right.OutputType != NodeType.Float4 && Right.OutputType != NodeType.Float) return NodeType.Invalid;
                    else return NodeType.Float4;
                }
                else if (Left.OutputType == NodeType.Float3)
                {
                    if (Right.OutputType != NodeType.Float3 && Right.OutputType != NodeType.Float) return NodeType.Invalid;
                    else return NodeType.Float3;
                }
                else if (Left.OutputType == NodeType.Float2)
                {
                    if (Right.OutputType != NodeType.Float2 && Right.OutputType != NodeType.Float) return NodeType.Invalid;
                    else return NodeType.Float2;
                }
                else if (Right.OutputType == NodeType.Float4)
                {
                    if (Left.OutputType != NodeType.Float4 && Left.OutputType != NodeType.Float) return NodeType.Invalid;
                    else return NodeType.Float4;
                }
                else if (Right.OutputType == NodeType.Float3)
                {
                    if (Left.OutputType != NodeType.Float3 && Left.OutputType != NodeType.Float) return NodeType.Invalid;
                    else return NodeType.Float3;
                }
                else if (Right.OutputType == NodeType.Float2)
                {
                    if (Left.OutputType != NodeType.Float2 && Left.OutputType != NodeType.Float) return NodeType.Invalid;
                    else return NodeType.Float2;
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
                var cross = typeof(Vector3).GetMethod("Cross");
                return Expression.Call(null, cross, left, right);
            }

            NodeType opType = NodeType.Float;

            if (leftType == NodeType.Float4)
            {
                opType = NodeType.Float4;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat4(right);
                else if (rightType != NodeType.Float4) throw new InvalidNodeException(Right);
            }
            else if (leftType == NodeType.Float3)
            {
                opType = NodeType.Float3;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat3(right);
                else if (rightType != NodeType.Float3) throw new InvalidNodeException(Right);
            }
            else if (leftType == NodeType.Float2)
            {
                opType = NodeType.Float2;
                if (rightType == NodeType.Float) right = CompilerTools.FloatToFloat2(right);
                else if (rightType != NodeType.Float2) throw new InvalidNodeException(Right);
            }
            else if (rightType == NodeType.Float4)
            {
                opType = NodeType.Float4;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat4(left);
                else if (leftType != NodeType.Float4) throw new InvalidNodeException(Left);
            }
            else if (rightType == NodeType.Float3)
            {
                opType = NodeType.Float3;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat3(left);
                else if (leftType != NodeType.Float3) throw new InvalidNodeException(Left);
            }
            else if (rightType == NodeType.Float2)
            {
                opType = NodeType.Float2;
                if (leftType == NodeType.Float) left = CompilerTools.FloatToFloat2(left);
                else if (leftType != NodeType.Float2) throw new InvalidNodeException(Left);
            }

            Type t;
            switch (opType)
            {
                case NodeType.Float: t = typeof(float); break;
                case NodeType.Float2: t = typeof(Vector2); break;
                case NodeType.Float3: t = typeof(Vector3); break;
                case NodeType.Float4: t = typeof(Vector4); break;
                default: throw new NotImplementedException();
            }

            Expression MathExpr(string name, Type c = null)
            {
                c = c == null ? (t == typeof(float) ? typeof(Math) : t) : c;
                var method = c.GetMethod(name, new Type[] { t, t });
                if (method == null) throw new InvalidNodeException(this);
                return Expression.Call(method, left, right);
            }

            Expression TryExpr(Func<Expression, Expression, Expression> f)
            {
                try
                {
                    return f(left, right);
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidNodeException(this);
                }
            }

            switch (m_Operation)
            {
                case BinaryOp.Add: result = TryExpr(Expression.Add); break;
                case BinaryOp.Div: result = TryExpr(Expression.Divide); break;
                case BinaryOp.Mul: result = TryExpr(Expression.Multiply); break;
                case BinaryOp.Sub: result = TryExpr(Expression.Subtract); break;
                case BinaryOp.Dot: result = MathExpr("Dot"); break;
                case BinaryOp.Atan2: result = MathExpr("Atan", typeof(MathExtensions)); break;
                case BinaryOp.Max: result = MathExpr("Max"); break;
                case BinaryOp.Min: result = MathExpr("Min"); break;
                case BinaryOp.Mod: result = MathExpr("Mod", typeof(MathExtensions)); break;
                default: throw new NotImplementedException();
            }

            if (left is ConstantExpression c1 && right is ConstantExpression c2)
            {
                result = Expression.Constant(Expression.Lambda(result).Compile().DynamicInvoke());
            }

            nodeDictionary[this] = result;
            return result;
        }
    }

    [Serializable]
    public class SwizzleNode : UnaryNode
    {
        SwizzleType type;

        [LocalizedDisplayName("SwizzleType")]
        public SwizzleType SwizzleType
        {
            get => type;
            set
            {
                type = value;
                SwizzleTypeChanged?.Invoke(this, new EventArgs());
            }
        }

        [field: NonSerialized]
        public event EventHandler SwizzleTypeChanged;

        protected override NodeType GetOutputType()
        {
            if (Input == null)
            {
                return NodeType.Indeterminate;
            }
            else
            {
                var swizzleString = Enum.GetName(typeof(SwizzleType), SwizzleType);
                switch (swizzleString.Length)
                {
                    case 2: return NodeType.Float2;
                    case 3: return NodeType.Float3;
                    case 4: return NodeType.Float4;
                }
                return NodeType.Invalid;
            }
        }

        public override Node Clone()
        {
            return new SwizzleNode()
            {
                Input = Input,
                SwizzleType = SwizzleType
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Input == null) throw new InvalidNodeException(this);
            if (nodeDictionary.TryGetValue(this, out var res)) return res;

            var tswizzle = typeof(Swizzle);
            Type inputType;
            switch (Input.OutputType)
            {
                case NodeType.Float2: inputType = typeof(Vector2); break;
                case NodeType.Float3: inputType = typeof(Vector3); break;
                case NodeType.Float4: inputType = typeof(Vector4); break;
                default: throw new InvalidNodeException(Input);
            }
            var name = Enum.GetName(typeof(SwizzleType), SwizzleType);

            var method = tswizzle.GetMethod(name, new Type[] { inputType });

            if (method == null) throw new InvalidNodeException(Input);

            var arg = Input.Compile(nodeDictionary, parameters);

            res = Expression.Call(method, arg);
            nodeDictionary[this] = res;
            return res;
        }
    }

    [Serializable]
    public class InputNode : Node
    {
        protected override NodeType GetOutputType() => OutputType;

        [LocalizedDisplayName("OutputType")]
        [ReadOnly(true)]
        public new NodeType OutputType { get; set; }

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

        protected override NodeType GetOutputType()
        {
            if (Input == null || Body == null)
            {
                InputNode.OutputType = NodeType.Indeterminate;
                return NodeType.Indeterminate;
            }
            else
            {
                InputNode.OutputType = Input.OutputType;
                return Body.OutputType;
            }
        }

        [Browsable(false)]
        public OutputNode Body { get; set; }

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
    }

    [Serializable]
    public class CompositeBinaryNode : BinaryNode, ICompositeNode
    {
        public CompositeBinaryNode()
        {
            LeftNode = new InputNode() { OutputType = NodeType.Indeterminate };
            RightNode = new InputNode() { OutputType = NodeType.Indeterminate, InputNumber = 1 };
        }

        protected override NodeType GetOutputType()
        {
            if (Body == null) return NodeType.Invalid;

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

            return Body.OutputType;
        }

        [Browsable(false)]
        public OutputNode Body { get; set; }

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
    }

    [Serializable]
    public class Float3Constructor : Node, INAryNode
    {
        public int InputCount => 3;

        INode[] inputs = new INode[3];

        protected override NodeType GetOutputType()
        {
            if (inputs[0] == null || inputs[1] == null || inputs[2] == null
            || inputs[0].OutputType != NodeType.Float
            || inputs[1].OutputType != NodeType.Float
            || inputs[2].OutputType != NodeType.Float)
            {
                return NodeType.Invalid;
            }
            else
            {
                return NodeType.Float3;
            }
        }

        [field: NonSerialized]
        public event EventHandler InputCountChanged;

        public override Node Clone()
        {
            return new Float3Constructor()
            {
                inputs = new INode[] { inputs[0], inputs[1], inputs[2] }
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (OutputType == NodeType.Invalid) throw new InvalidNodeException(this);
            if (nodeDictionary.TryGetValue(this, out var result)) return result;
            var arg1 = inputs[0].Compile(nodeDictionary, parameters);
            var arg2 = inputs[1].Compile(nodeDictionary, parameters);
            var arg3 = inputs[2].Compile(nodeDictionary, parameters);

            if (arg1 is ConstantExpression c1 && arg2 is ConstantExpression c2 && arg3 is ConstantExpression c3)
            {
                result = Expression.Constant(new Vector3((float)c1.Value, (float)c2.Value, (float)c3.Value));
            }
            else
            {
                var tfloat = typeof(float);
                var tfloat3 = typeof(Vector3);

                var ctor = tfloat3.GetConstructor(new Type[] { tfloat, tfloat, tfloat });
                result = Expression.New(ctor, arg1, arg2, arg3);
            }
            nodeDictionary[this] = result;
            return result;
        }

        public INode GetInput(int i)
        {
            return inputs[i];
        }

        public void SetInput(int i, INode node)
        {
            inputs[i] = node;
        }
    }

    [Serializable]
    public class Float2Constructor : BinaryNode
    {
        public override Node Clone()
        {
            return new Float2Constructor()
            {
                Left = Left,
                Right = Right
            };
        }

        public override Expression Compile(Dictionary<INode, Expression> nodeDictionary, params Expression[] parameters)
        {
            if (Left == null || Right == null || OutputType != NodeType.Float2) throw new InvalidNodeException(this);
            if (nodeDictionary.TryGetValue(this, out var result)) return result;
            var left = Left.Compile(nodeDictionary, parameters);
            var right = Right.Compile(nodeDictionary, parameters);

            if (left is ConstantExpression c1 && right is ConstantExpression c2)
            {
                result = Expression.Constant(new Vector2((float)c1.Value, (float)c2.Value));
            }
            else
            {

                var ctor = typeof(Vector2).GetConstructor(new Type[] { typeof(float), typeof(float) });
                result = Expression.New(ctor, left, right);
            }

            nodeDictionary[this] = result;
            return result;
        }

        protected override NodeType GetOutputType()
        {
            if (Left == null || Right == null) return NodeType.Invalid;
            if (Left.OutputType != NodeType.Float || Right.OutputType != NodeType.Float) return NodeType.Invalid;
            return NodeType.Float2;
        }
    }

    [Serializable]
    public class OutputNode : UnaryNode
    {
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

        protected override NodeType GetOutputType()
        {
            return Input == null ? NodeType.Indeterminate : Input.OutputType;
        }
    }
}
