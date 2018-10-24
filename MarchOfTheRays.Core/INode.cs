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
        None
    }

    public interface INode
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
        [Browsable(false)]
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
    }

    [Serializable]
    public class Float2ConstantNode : INode
    {
        [Browsable(false)]
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
    }

    [Serializable]
    public class Float3ConstantNode : INode
    {
        [Browsable(false)]
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
    }

    [Serializable]
    public class Float4ConstantNode : INode
    {
        [Browsable(false)]
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
    }

    [Serializable]
    public class LengthNode : IUnaryNode
    {
        [Browsable(false)]
        public NodeType OutputType => NodeType.Float;

        [Browsable(false)]
        public INode Input { get; set; }
    }

    [Serializable]
    public class AbsNode : IUnaryNode
    {
        [Browsable(false)]
        public NodeType OutputType => Input.OutputType;

        [Browsable(false)]
        public INode Input { get; set; }
    }

    [Serializable]
    public class MinMaxNode : IBinaryNode
    {
        [Browsable(false)]
        public INode Left { get; set; }
        [Browsable(false)]
        public INode Right { get; set; }
        [Browsable(false)]
        public NodeType OutputType
        {
            get
            {
                if (Left.OutputType == NodeType.Float4 || Right.OutputType == NodeType.Float4) return NodeType.Float4;
                if (Left.OutputType == NodeType.Float3 || Right.OutputType == NodeType.Float3) return NodeType.Float3;
                if (Left.OutputType == NodeType.Float2 || Right.OutputType == NodeType.Float2) return NodeType.Float2;
                return NodeType.Float;
            }
        }

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

        [Browsable(false)]
        public NodeType OutputType
        {
            get
            {
                if (Operation == ArithOp.Cross) return NodeType.Float3;
                if (Operation == ArithOp.Dot) return NodeType.Float;

                if (Left.OutputType == NodeType.Float4 || Right.OutputType == NodeType.Float4) return NodeType.Float4;
                if (Left.OutputType == NodeType.Float3 || Right.OutputType == NodeType.Float3) return NodeType.Float3;
                if (Left.OutputType == NodeType.Float2 || Right.OutputType == NodeType.Float2) return NodeType.Float2;
                return NodeType.Float;
            }
        }
    }

    [Serializable]
    public class InputNode : INode
    {
        [Browsable(false)]
        public NodeType OutputType { get; set; }
    }

    [Serializable]
    public class OutputNode : IUnaryNode
    {
        [Browsable(false)]
        public NodeType OutputType => NodeType.None;

        [Browsable(false)]
        public INode Input { get; set; }
    }
}
