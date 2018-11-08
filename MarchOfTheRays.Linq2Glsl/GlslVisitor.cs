using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace MarchOfTheRays.Linq2Glsl
{
    /// <summary>
    /// Converts a Linq expression into a GLSL expression
    /// </summary>
    public class GlslVisitor : ExpressionVisitor
    {
        StringBuilder sb;

        /// <summary>
        /// Creates a new GlslVisitor which will output the resulting expression in the specified StringBuilder
        /// </summary>
        /// <param name="builder">The StringBuilder which will contain the resulting expression</param>
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
            void AppendFloats(params float[] floats)
            {
                bool isFirst = true;
                foreach (var fl in floats)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0:0.0######}", fl);
                }
            }

            if (node.Value is float f)
            {
                AppendFloats(f);
            }
            else if (node.Value is Vector2 vec2)
            {
                sb.Append("vec2(");
                AppendFloats(vec2.X, vec2.Y);
                sb.Append(")");
            }
            else if (node.Value is Vector3 vec3)
            {
                sb.Append("vec3(");
                AppendFloats(vec3.X, vec3.Y, vec3.Z);
                sb.Append(")");
            }
            else if (node.Value is Vector4 vec4)
            {
                sb.Append("vec4(");
                AppendFloats(vec4.X, vec4.Y, vec4.Z, vec4.W);
                sb.Append(")");
            }
            else
            {
                throw new NotImplementedException();
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var decltype = node.Member.DeclaringType;
            if (decltype == typeof(Vector2) || decltype == typeof(Vector3) || decltype == typeof(Vector4))
            {
                Visit(node.Expression);
                sb.Append(".");
                sb.Append(node.Member.Name.ToLower());
            }
            else
            {
                throw new NotImplementedException();
            }
            return node;
        }

        void VisitArguments(IReadOnlyCollection<Expression> args)
        {
            bool first = true;
            foreach (var arg in args)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }
                Visit(arg);
            }
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Type == typeof(Vector2)) sb.Append("vec2");
            else if (node.Type == typeof(Vector3)) sb.Append("vec3");
            else if (node.Type == typeof(Vector4)) sb.Append("vec4");
            else throw new NotImplementedException();

            sb.Append("(");
            VisitArguments(node.Arguments);
            sb.Append(")");

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
                        case "Min":
                        case "Max":
                            sb.Append(node.Method.Name.ToLower());
                            sb.Append("(");
                            VisitArguments(node.Arguments);
                            sb.Append(")");
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (node.Method.DeclaringType == typeof(Core.MathExtensions) || node.Method.DeclaringType == typeof(Vector3))
                {
                    sb.Append(node.Method.Name.ToLower());
                    sb.Append("(");
                    VisitArguments(node.Arguments);
                    sb.Append(")");
                }
                else if (node.Method.DeclaringType == typeof(Core.Swizzle))
                {
                    Visit(node.Arguments[0]);
                    sb.Append(".");
                    sb.Append(node.Method.Name.ToLower());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Negate:
                    sb.Append("(-");
                    Visit(node.Operand);
                    sb.Append(")");
                    break;
                default: throw new NotImplementedException();
            }
            return node;
        }
    }
}
