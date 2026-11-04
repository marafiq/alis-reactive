using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Alis.Reactive.Members
{
    internal static class Expressions
    {
        internal static Expression StripConvert(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            while (expression is UnaryExpression unary
                   && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                expression = unary.Operand;
            return expression;
        }

        internal static Type TypeOf(Expression expression) => StripConvert(expression).Type;

        internal static object? Evaluate(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return Expression.Lambda<Func<object?>>(Expression.Convert(expression, typeof(object))).Compile()();
        }

        internal static bool Reads(Expression expression, ParameterExpression parameter)
        {
            var reads = new ParameterReads(parameter);
            reads.Visit(expression);
            return reads.Found;
        }

        internal static bool IsValueMember(MemberInfo member) =>
            member.DeclaringType?.Namespace?.StartsWith("System", StringComparison.Ordinal) == true;

        internal static bool TryContains(MethodCallExpression call, out Expression source, out Expression item)
        {
            source = null!;
            item = null!;
            if (call.Method.Name != "Contains") return false;

            var isStaticContains = call.Object == null
                && (call.Arguments.Count == 2 || (call.Arguments.Count == 3 && IsNullComparer(call.Arguments[2])))
                && (call.Method.DeclaringType == typeof(Enumerable) || call.Method.DeclaringType?.FullName == "System.MemoryExtensions");
            if (isStaticContains)
            {
                source = WithoutSpanConversion(call.Arguments[0]);
                item = call.Arguments[1];
                return true;
            }

            var isCollectionContains = call.Object != null && call.Arguments.Count == 1
                && call.Object.Type != typeof(string)
                && typeof(IEnumerable).IsAssignableFrom(call.Object.Type);
            if (!isCollectionContains) return false;

            source = call.Object!;
            item = call.Arguments[0];
            return true;
        }

        private static bool IsNullComparer(Expression comparer) =>
            StripConvert(comparer) is ConstantExpression constant && constant.Value == null;

        private static Expression WithoutSpanConversion(Expression expression)
        {
            if (expression is MethodCallExpression conversion && conversion.Method.Name == "op_Implicit" && conversion.Arguments.Count == 1)
                return conversion.Arguments[0];
            return StripConvert(expression);
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
