﻿using System;
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

            if (node.Method.Name == "Equals")
            {
                Visit(node.Object);
                _resultStringBuilder.Append("(");
                Visit(node.Arguments[0]);
                _resultStringBuilder.Append(")");

                return node;
            }

            if (node.Method.Name == "StartsWith")
            {
                Visit(node.Object);
                _resultStringBuilder.Append("(");
                Visit(node.Arguments[0]);
                _resultStringBuilder.Append("*");
                _resultStringBuilder.Append(")");

                return node;
            }

            if (node.Method.Name == "EndsWith")
            {
                Visit(node.Object);
                _resultStringBuilder.Append("(");
                _resultStringBuilder.Append("*");
                Visit(node.Arguments[0]);
                _resultStringBuilder.Append(")");

                return node;
            }

            if (node.Method.Name == "Contains")
            {
                Visit(node.Object);
                _resultStringBuilder.Append("(");
                _resultStringBuilder.Append("*");
                Visit(node.Arguments[0]);
                _resultStringBuilder.Append("*");
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
                    var nodeMember = node.Left.NodeType == ExpressionType.MemberAccess
                                   ? node.Left
                                   : node.Right;

                    var nodeConstant = node.Right.NodeType == ExpressionType.Constant
                                   ? node.Right
                                   : node.Left;

                    Visit(nodeMember);
                    _resultStringBuilder.Append("(");
                    Visit(nodeConstant);
                    _resultStringBuilder.Append(")");
                    break;
                case ExpressionType.AndAlso:
                    Visit(node.Left);
                    _resultStringBuilder.Append(";");
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