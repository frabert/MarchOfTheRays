using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace MarchOfTheRays.Linq2Glsl
{
    public class GlslVisitor : ExpressionVisitor
    {
        StringBuilder sb;

        public GlslVisitor(StringBuilder builder)
        {
            sb = builder;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            sb.Append("(");
            Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.Add: sb.Append(" + "); break;
                case ExpressionType.Divide: sb.Append(" / "); break;
                case ExpressionType.Modulo: sb.Append(" % "); break;
                case ExpressionType.Multiply: sb.Append(" * "); break;
                case ExpressionType.Subtract: sb.Append(" - "); break;
                default: throw new NotImplementedException();
            }
            Visit(node.Right);
            sb.Append(")");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {

            if (node.Value is float f)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:0.0######}", f);
            }
            else if (node.Value is Vector3 vec3)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "vec3({0:0.0######}, {1:0.0######}, {2:0.0######})", vec3.X, vec3.Y, vec3.Z);
            }
            else
            {
                throw new NotImplementedException();
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.DeclaringType == typeof(Vector3))
            {
                Visit(node.Expression);
                switch (node.Member.Name)
                {
                    case "X": sb.Append(".x"); break;
                    case "Y": sb.Append(".y"); break;
                    case "Z": sb.Append(".z"); break;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Type == typeof(Vector3))
            {
                bool first = true;
                sb.Append("vec3(");
                foreach (var arg in node.Arguments)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }
                    Visit(arg);
                }
                sb.Append(")");
            }
            else
            {
                throw new NotImplementedException();
            }
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            sb.Append(node.Name);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object != null)
            {
                if (node.Object.Type == typeof(Vector3))
                {
                    switch (node.Method.Name)
                    {
                        case "Length":
                            sb.Append("length(");
                            Visit(node.Object);
                            sb.Append(")");
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (node.Method.DeclaringType == typeof(Math))
                {
                    switch (node.Method.Name)
                    {
                        case "Abs":
                            sb.Append("abs(");
                            Visit(node.Arguments[0]);
                            sb.Append(")");
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return node;
        }
    }
}
