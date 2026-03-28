using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>
    /// Converts lambda expressions like <c>x =&gt; x.Address.City</c> into camelCase
    /// dot-paths for use as source bindings in the plan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each path gets a prefix that identifies where to resolve the value in the browser:
    /// <c>"evt"</c> for event payloads (<c>evt.address.city</c>),
    /// <c>"responseBody"</c> for HTTP response data (<c>responseBody.data.name</c>).
    /// </para>
    /// <para>
    /// Only simple property-access chains are supported. Computed expressions
    /// (method calls, arithmetic) throw <see cref="InvalidOperationException"/>.
    /// </para>
    /// </remarks>
    public static class ExpressionPathHelper
    {
        /// <summary>
        /// Converts an expression to a prefixed camelCase dot-path.
        /// </summary>
        /// <typeparam name="TSource">The source type containing the property chain.</typeparam>
        /// <param name="prefix">The resolution context prefix (e.g. <c>"evt"</c>, <c>"responseBody"</c>).</param>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>evt.address.city</c>.</returns>
        public static string ToPath<TSource>(string prefix, Expression<Func<TSource, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return prefix + "." + string.Join(".", members);
        }

        /// <summary>
        /// Converts a typed expression to a prefixed camelCase dot-path, avoiding boxing for value types.
        /// </summary>
        /// <typeparam name="TSource">The source type containing the property chain.</typeparam>
        /// <typeparam name="TProp">The property type.</typeparam>
        /// <param name="prefix">The resolution context prefix.</param>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>evt.facilityId</c>.</returns>
        public static string ToPath<TSource, TProp>(string prefix, Expression<Func<TSource, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return prefix + "." + string.Join(".", members);
        }

        /// <summary>
        /// Converts an expression to an event payload dot-path (<c>evt.</c> prefix).
        /// </summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>evt.address.city</c>.</returns>
        public static string ToEventPath<TSource>(Expression<Func<TSource, object?>> expression)
            => ToPath("evt", expression);

        /// <summary>
        /// Typed overload — no boxing Convert node needed because TProp matches the property type.
        /// Used by the typed condition builders.
        /// </summary>
        public static string ToEventPath<TSource, TProp>(Expression<Func<TSource, TProp>> expression)
            => ToPath<TSource, TProp>("evt", expression);

        /// <summary>
        /// Converts an expression to an HTTP response body dot-path (<c>responseBody.</c> prefix).
        /// </summary>
        /// <typeparam name="TSource">The response body type.</typeparam>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>responseBody.data.name</c>.</returns>
        public static string ToResponsePath<TSource>(Expression<Func<TSource, object?>> expression)
            => ToPath("responseBody", expression);

        private static List<string> ExtractMemberChain(Expression expr)
        {
            var members = new List<string>();

            // Unwrap Convert (boxing of value types)
            if (expr is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
                expr = unary.Operand;

            while (expr is MemberExpression member)
            {
                members.Insert(0, CamelCase(member.Member.Name));
                expr = member.Expression!;
            }

            // After walking all MemberExpression nodes, we must be at a ParameterExpression.
            // Anything else (MethodCallExpression, BinaryExpression, etc.) is a computed
            // expression — not a property path — and must fail fast.
            if (!(expr is ParameterExpression))
            {
                throw new InvalidOperationException(
                    $"ExpressionPathHelper only supports simple property-access chains " +
                    $"(e.g. m => m.Address.City). Got unsupported expression node: {expr.NodeType}.");
            }

            return members;
        }

        /// <summary>
        /// Extracts the model binding path from a model expression.
        /// m => m.FacilityId → "FacilityId", m => m.Address.City → "Address.City".
        /// Dot-notation preserves the model structure for HTTP gather (JSON/FormData).
        /// </summary>
        public static string ToPropertyName<TModel>(Expression<Func<TModel, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members.ConvertAll(PascalRestore));
        }

        /// <summary>
        /// Converts a model expression to the DOM element ID that ASP.NET / SF generates.
        /// m => m.FacilityId → "FacilityId", m => m.Address.City → "Address_City".
        /// Underscores match Html.IdFor() convention. Used by Component&lt;T&gt;(expr) for target resolution.
        /// </summary>
        public static string ToElementId<TModel>(Expression<Func<TModel, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join("_", members.ConvertAll(PascalRestore));
        }

        /// <summary>
        /// Typed overload — converts a model expression to a DOM element ID without boxing.
        /// </summary>
        public static string ToElementId<TModel, TProp>(Expression<Func<TModel, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join("_", members.ConvertAll(PascalRestore));
        }

        private static string PascalRestore(string camel)
        {
            if (string.IsNullOrEmpty(camel)) return camel;
            return char.ToUpperInvariant(camel[0]) + camel.Substring(1);
        }

        private static string CamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
