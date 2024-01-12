using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        readonly StringBuilder _resultStringBuilder;

        public ExpressionToFtsRequestTranslator()
        {
            _resultStringBuilder = new StringBuilder();
        }

        public string Translate(Expression exp)
        {
            Visit(exp);

            return _resultStringBuilder.ToString();
        }

        #region protected methods

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }

            if (node.Method.DeclaringType == typeof(string)
                && (node.Method.Name == "Equals"
                || node.Method.Name == "Contains"
                || node.Method.Name == "StartsWith"
                || node.Method.Name == "EndsWith"))
            {
                Visit(node.Object);

                _resultStringBuilder.Append("(");

                if (node.Method.Name == "Contains" || node.Method.Name == "EndsWith")
                {
                    _resultStringBuilder.Append("*");
                }

                var predicate = node.Arguments[0];

                Visit(predicate);

                if (node.Method.Name == "Contains" || node.Method.Name == "StartsWith")
                {
                    _resultStringBuilder.Append("*");
                }

                _resultStringBuilder.Append(")");

                return node;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    var (left, right) = node.Right.NodeType == ExpressionType.Constant
                        ? (node.Left, node.Right)
                        : (node.Right, node.Left);

                    Visit(left);
                    _resultStringBuilder.Append("(");
                    Visit(right);
                    _resultStringBuilder.Append(")");
                    break;

                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    Visit(node.Left);
                    _resultStringBuilder.Append(" AND ");
                    Visit(node.Right);
                    break;

                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    Visit(node.Left);

                    _resultStringBuilder.Append(" OR ");
                    Visit(node.Right);
                    break;

                default:
                    throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _resultStringBuilder.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _resultStringBuilder.Append(node.Value);

            return node;
        }

        #endregion
    }
}