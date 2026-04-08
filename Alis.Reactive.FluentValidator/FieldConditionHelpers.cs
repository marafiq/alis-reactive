using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Shared helpers for field-condition authoring.
    /// Extracted from ReactiveValidator so FieldConditionBuilder can use them
    /// without coupling to the generic <c>ReactiveValidator&lt;T&gt;</c> type.
    /// </summary>
    internal static class FieldConditionHelpers
    {
        /// <summary>
        /// Serializes a condition value for plan JSON.
        /// DateTime/DateTimeOffset/DateOnly become Unix ms (long) via ToUnixTimeMilliseconds.
        /// All other types pass through as-is.
        /// Developer controls timezone by passing DateTime with the intended Kind.
        /// TimeSpan.Zero forces UTC interpretation for DateTime without explicit Kind.
        /// </summary>
        internal static object? SerializeConditionValue<TProp>(TProp value) => value switch
        {
            DateTime dt => new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
            DateTimeOffset dto => dto.ToUnixTimeMilliseconds(),
            DateOnly d => new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                .ToUnixTimeMilliseconds(),
            _ => value
        };

        /// <summary>
        /// Extracts the property name from a simple member-access expression.
        /// Throws if the expression is not a direct property access (e.g. x => x.Name).
        /// </summary>
        internal static string ExtractPropertyName<TSource, TResult>(
            Expression<Func<TSource, TResult>> expression)
        {
            var body = expression.Body;
            if (body is UnaryExpression unary)
                body = unary.Operand;

            if (body is MemberExpression member && member.Member is PropertyInfo)
                return member.Member.Name;

            throw new ArgumentException(
                $"WhenField() requires a simple property access expression (e.g. x => x.IsEmployed). " +
                $"Got: {expression}");
        }
    }
}
