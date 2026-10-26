using System;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>What every reader of a lambda needs of its expression tree, in one place.</summary>
    internal static class Expressions
    {
        /// <summary>The expression under the conversions C# wraps it in: a boxing, a widening.</summary>
        internal static Expression StripConvert(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            while (expression is UnaryExpression unary
                   && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                expression = unary.Operand;
            return expression;
        }

        /// <summary>The type the expression has under its conversions.</summary>
        internal static Type TypeOf(Expression expression) => StripConvert(expression).Type;

        /// <summary>A value fixed when the view renders: a constant, or a variable the lambda captured, evaluated once.</summary>
        internal static object? Evaluate(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return Expression.Lambda<Func<object?>>(Expression.Convert(expression, typeof(object))).Compile()();
        }

        /// <summary>Whether the expression reads the parameter anywhere.</summary>
        internal static bool Reads(Expression expression, ParameterExpression parameter)
        {
            var reads = new ParameterReads(parameter);
            reads.Visit(expression);
            return reads.Found;
        }

        private sealed class ParameterReads : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;

            internal ParameterReads(ParameterExpression parameter)
            {
                _parameter = parameter;
            }

            internal bool Found { get; private set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == _parameter) Found = true;
                return node;
            }
        }
    }
}
