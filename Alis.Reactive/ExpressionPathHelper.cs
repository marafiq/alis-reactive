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
        /// <returns>A dot-path like <c>address.city</c>.</returns>
        public static string ToEventPath<TSource>(Expression<Func<TSource, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members);
        }

        /// <summary>
        /// Converts a typed expression to an event payload dot-path, preserving type safety for value types.
        /// </summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <typeparam name="TProp">The property type.</typeparam>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>facilityId</c>.</returns>
        public static string ToEventPath<TSource, TProp>(Expression<Func<TSource, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members);
        }

        /// <summary>
        /// Converts an expression to an HTTP response body dot-path (<c>responseBody.</c> prefix).
        /// </summary>
        /// <typeparam name="TSource">The response body type.</typeparam>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>data.name</c>.</returns>
        public static string ToResponsePath<TSource>(Expression<Func<TSource, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members);
        }

        /// <summary>
        /// Converts a typed expression to an HTTP response body dot-path, preserving type safety for value types.
        /// </summary>
        public static string ToResponsePath<TSource, TProp>(Expression<Func<TSource, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members);
        }

        private static List<string> ExtractMemberChain(Expression expr)
        {
            expr = UnwrapConvert(expr);

            if (expr is ParameterExpression)
                return new List<string>();

            if (expr is MemberExpression member)
            {
                var members = ExtractMemberChain(member.Expression!);
                members.Add(CamelCase(member.Member.Name));
                return members;
            }

            if (TryIndexAccess(expr, out var collection, out var index))
            {
                var members = ExtractMemberChain(collection);
                members[members.Count - 1] += "[" + index + "]";
                return members;
            }

            throw new InvalidOperationException(
                $"ExpressionPathHelper only supports property-access chains and MVC indexer paths " +
                $"(e.g. m => m.Address.City or m => m.Items[0].Sku). Got unsupported expression node: {expr.NodeType}.");
        }

        /// <summary>
        /// Extracts the model binding path from a model expression.
        /// </summary>
        /// <remarks>
        /// <c>m =&gt; m.FacilityId</c> becomes <c>"FacilityId"</c>,
        /// <c>m =&gt; m.Address.City</c> becomes <c>"Address.City"</c>.
        /// Dot-notation preserves the model structure for HTTP gather.
        /// </remarks>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="expression">The model property expression.</param>
        /// <returns>A dot-separated binding path like <c>"Address.City"</c>.</returns>
        public static string ToPropertyName<TModel>(Expression<Func<TModel, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members.ConvertAll(PascalRestore));
        }

        internal static Type ToPropertyType(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return UnwrapConvert(expression.Body).Type;
        }

        /// <summary>
        /// Extracts the model binding path from a typed model expression, preserving type safety for value types.
        /// </summary>
        public static string ToPropertyName<TModel, TProp>(Expression<Func<TModel, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members.ConvertAll(PascalRestore));
        }

        /// <summary>
        /// Converts a model expression to the DOM element ID that ASP.NET generates.
        /// </summary>
        /// <remarks>
        /// <c>m =&gt; m.FacilityId</c> becomes <c>"FacilityId"</c>,
        /// <c>m =&gt; m.Address.City</c> becomes <c>"Address_City"</c>.
        /// Underscores match the <c>Html.IdFor()</c> convention.
        /// </remarks>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="expression">The model property expression.</param>
        /// <returns>An underscore-separated element ID like <c>"Address_City"</c>.</returns>
        public static string ToElementId<TModel>(Expression<Func<TModel, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToMvcElementId(string.Join(".", members.ConvertAll(PascalRestore)));
        }

        /// <summary>
        /// Converts a typed model expression to a DOM element ID, preserving type safety for value types.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The property type.</typeparam>
        /// <param name="expression">The model property expression.</param>
        /// <returns>An underscore-separated element ID like <c>"Address_City"</c>.</returns>
        public static string ToElementId<TModel, TProp>(Expression<Func<TModel, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToMvcElementId(string.Join(".", members.ConvertAll(PascalRestore)));
        }

        internal static string ToMvcElementId(string propertyPath) =>
            propertyPath
                .Replace(".", "_")
                .Replace("[", "_")
                .Replace("]", "_");

        private static bool TryIndexAccess(
            Expression expr,
            out Expression collection,
            out int index)
        {
            collection = expr;
            index = 0;

            if (expr is BinaryExpression binary && binary.NodeType == ExpressionType.ArrayIndex)
            {
                collection = binary.Left;
                index = EvaluateIndex(binary.Right);
                return true;
            }

            if (expr is IndexExpression indexer && indexer.Object != null && indexer.Arguments.Count == 1)
            {
                collection = indexer.Object;
                index = EvaluateIndex(indexer.Arguments[0]);
                return true;
            }

            if (expr is MethodCallExpression call &&
                call.Method.Name == "get_Item" &&
                call.Object != null &&
                call.Arguments.Count == 1)
            {
                collection = call.Object;
                index = EvaluateIndex(call.Arguments[0]);
                return true;
            }

            return false;
        }

        private static int EvaluateIndex(Expression expression)
        {
            var value = Expression.Lambda(UnwrapConvert(expression)).Compile().DynamicInvoke();
            return Convert.ToInt32(value);
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

        private static Expression UnwrapConvert(Expression expr)
        {
            while (expr is UnaryExpression unary &&
                   (unary.NodeType == ExpressionType.Convert ||
                    unary.NodeType == ExpressionType.ConvertChecked))
            {
                expr = unary.Operand;
            }

            return expr;
        }
    }
}
