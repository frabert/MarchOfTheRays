using System;
using System.ComponentModel;

namespace MarchOfTheRays.Core
{
    public enum NodeType
    {
        Float,
        Float2,
        Float3,
        Float4,
        Bool,
        Indeterminate,
        Invalid,
        None
    }

    public interface INode : ICloneable
    {
        [Browsable(false)]
        NodeType OutputType { get; }
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
        void SetInput(int i);
        int InputCount();

        event EventHandler InputCountChanged;
    }

    [Serializable]
    public class FloatConstantNode : INode
    {
        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType => NodeType.Float;

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

        [field: NonSerialized]
        public event EventHandler ValueChanged;

        public object Clone()
        {
            return new FloatConstantNode()
            {
                m_Value = m_Value
            };
        }
    }

    [Serializable]
    public class Float2ConstantNode : INode
    {
        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType => NodeType.Float2;

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

        public object Clone()
        {
            return new Float2ConstantNode()
            {
                m_X = m_X,
                m_Y = m_Y
            };
        }
    }

    [Serializable]
    public class Float3ConstantNode : INode
    {
        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType => NodeType.Float3;

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

        public object Clone()
        {
            return new Float3ConstantNode()
            {
                m_X = m_X,
                m_Y = m_Y,
                m_Z = m_Z
            };
        }
    }

    [Serializable]
    public class Float4ConstantNode : INode
    {
        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType => NodeType.Float4;

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
    }

    [Serializable]
    public class LengthNode : IUnaryNode
    {
        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType => NodeType.Float;

        [Category("Type information")]
        [DisplayName("Input type")]
        public NodeType InputType => Input == null ? NodeType.Indeterminate : Input.OutputType;

        [Browsable(false)]
        public INode Input { get; set; }

        public object Clone()
        {
            return new LengthNode()
            {
                Input = Input
            };
        }
    }

    [Serializable]
    public class AbsNode : IUnaryNode
    {
        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType => Input.OutputType;

        [Category("Type information")]
        [DisplayName("Input type")]
        public NodeType InputType => Input == null ? NodeType.Indeterminate : Input.OutputType;

        [Browsable(false)]
        public INode Input { get; set; }

        public object Clone()
        {
            return new AbsNode()
            {
                Input = Input
            };
        }
    }

    [Serializable]
    public class MinMaxNode : IBinaryNode
    {
        [Browsable(false)]
        public INode Left { get; set; }
        [Browsable(false)]
        public INode Right { get; set; }

        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType
        {
            get
            {
                if (Left == null || Right == null) return NodeType.Indeterminate;

                if (Left.OutputType == NodeType.Float4 || Right.OutputType == NodeType.Float4) return NodeType.Float4;
                if (Left.OutputType == NodeType.Float3 || Right.OutputType == NodeType.Float3) return NodeType.Float3;
                if (Left.OutputType == NodeType.Float2 || Right.OutputType == NodeType.Float2) return NodeType.Float2;
                return NodeType.Float;
            }
        }

        [Category("Type information")]
        [DisplayName("Left input type")]
        public NodeType LeftInputType => Left == null ? NodeType.Indeterminate : Left.OutputType;

        [Category("Type information")]
        [DisplayName("Right input type")]
        public NodeType RightInputType => Right == null ? NodeType.Indeterminate : Right.OutputType;

        bool m_IsMin;

        public bool IsMin
        {
            get => m_IsMin;
            set
            {
                m_IsMin = value;
                IsMinChanged?.Invoke(this, new EventArgs());
            }
        }

        [field: NonSerialized]
        public event EventHandler IsMinChanged;

        public object Clone()
        {
            return new MinMaxNode()
            {
                m_IsMin = m_IsMin,
                Left = Left,
                Right = Right
            };
        }
    }

    public enum ArithOp
    {
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Dot,
        Cross
    }

    [Serializable]
    public class ArithmeticNode : IBinaryNode
    {
        [Browsable(false)]
        public INode Left { get; set; }
        [Browsable(false)]
        public INode Right { get; set; }

        ArithOp m_Operation;

        public ArithOp Operation
        {
            get => m_Operation;
            set
            {
                m_Operation = value;
                OperationChanged?.Invoke(this, new EventArgs());
            }
        }

        [field: NonSerialized]
        public event EventHandler OperationChanged;

        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType
        {
            get
            {
                if (Operation == ArithOp.Cross) return NodeType.Float3;
                if (Operation == ArithOp.Dot) return NodeType.Float;

                if (Left == null || Right == null) return NodeType.Indeterminate;

                if (Left.OutputType == NodeType.Float4 || Right.OutputType == NodeType.Float4) return NodeType.Float4;
                if (Left.OutputType == NodeType.Float3 || Right.OutputType == NodeType.Float3) return NodeType.Float3;
                if (Left.OutputType == NodeType.Float2 || Right.OutputType == NodeType.Float2) return NodeType.Float2;
                return NodeType.Float;
            }
        }

        [Category("Type information")]
        [DisplayName("Left input type")]
        public NodeType LeftInputType => Left == null ? NodeType.Indeterminate : Left.OutputType;

        [Category("Type information")]
        [DisplayName("Right input type")]
        public NodeType RightInputType => Right == null ? NodeType.Indeterminate : Right.OutputType;

        public object Clone()
        {
            return new ArithmeticNode()
            {
                Left = Left,
                Right = Right
            };
        }
    }

    [Serializable]
    public class InputNode : INode
    {
        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType { get; set; }

        public object Clone()
        {
            return new InputNode();
        }
    }

    [Serializable]
    public class OutputNode : IUnaryNode
    {
        [Category("Type information")]
        [DisplayName("Output type")]
        public NodeType OutputType => NodeType.None;

        [Browsable(false)]
        public INode Input { get; set; }

        [Category("Type information")]
        [DisplayName("Input type")]
        public NodeType InputType => Input == null ? NodeType.Indeterminate : Input.OutputType;

        public object Clone()
        {
            return new OutputNode()
            {
                Input = Input
            };
        }
    }
}
