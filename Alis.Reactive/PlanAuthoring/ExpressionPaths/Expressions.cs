using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

        /// <summary>
        /// Whether the member belongs to a value .NET defines (a date's <c>Year</c>, a text's <c>Length</c>, a
        /// list's <c>Count</c>) rather than to the app's own data, whose members the browser reads by name.
        /// </summary>
        internal static bool IsValueMember(MemberInfo member) =>
            member.DeclaringType?.Namespace?.StartsWith("System", StringComparison.Ordinal) == true;

        /// <summary>
        /// A <c>Contains</c> call, as a collection and the item it is asked for: <c>Enumerable.Contains(source, item)</c>;
        /// <c>MemoryExtensions.Contains(span, item)</c> or <c>(span, item, null)</c>, which C# 14 binds an array's
        /// <c>Contains</c> to (through the comparer overload for an item type that is not <c>IEquatable</c>); or an
        /// instance <c>Contains</c> on a collection (<c>List&lt;T&gt;</c>, a set). Text's own <c>Contains</c> is not one.
        /// </summary>
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

        // The default comparer, which C# passes as null when the overload takes one.
        private static bool IsNullComparer(Expression comparer) =>
            StripConvert(comparer) is ConstantExpression constant && constant.Value == null;

        // The array or collection under an implicit conversion to a span.
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
