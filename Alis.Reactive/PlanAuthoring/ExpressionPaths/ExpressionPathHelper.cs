using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>
    /// Converts lambda expressions like <c>x =&gt; x.Address.City</c> into runtime,
    /// model-binding, or MVC element-ID paths used by Reactive Plan bindings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>ToPath(...)</c> adds a caller-provided runtime scope prefix such as
    /// <c>"evt"</c> or <c>"responseBody"</c>. Event and response helpers return
    /// scope-relative paths because the surrounding <c>PayloadSource</c> already
    /// identifies the value scope.
    /// </para>
    /// <para>
    /// Property-access chains and MVC-style constant indexer paths are
    /// supported. Computed expressions such as method calls or arithmetic throw
    /// <see cref="InvalidOperationException"/>.
    /// </para>
    /// </remarks>
    public static class ExpressionPathHelper
    {
        /// <summary>
        /// Builds a scoped camelCase runtime path from a property-access expression.
        /// </summary>
        /// <typeparam name="TSource">The source type containing the property chain.</typeparam>
        /// <param name="prefix">The runtime scope prefix to prepend, such as <c>"evt"</c> or <c>"responseBody"</c>.</param>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>evt.address.city</c>.</returns>
        public static string ToPath<TSource>(string prefix, Expression<Func<TSource, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return prefix + "." + ToRuntimePath(members);
        }

        /// <summary>
        /// Builds a scoped camelCase runtime path while preserving the selected value type.
        /// </summary>
        /// <typeparam name="TSource">The source type containing the property chain.</typeparam>
        /// <typeparam name="TProp">The selected value type.</typeparam>
        /// <param name="prefix">The runtime scope prefix to prepend.</param>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>evt.facilityId</c>.</returns>
        public static string ToPath<TSource, TProp>(string prefix, Expression<Func<TSource, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return prefix + "." + ToRuntimePath(members);
        }

        /// <summary>
        /// Builds a camelCase path relative to the current event payload scope.
        /// </summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>address.city</c>.</returns>
        public static string ToEventPath<TSource>(Expression<Func<TSource, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToRuntimePath(members);
        }

        /// <summary>
        /// Builds a camelCase event-payload path while preserving the selected value type.
        /// </summary>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <typeparam name="TProp">The selected payload value type.</typeparam>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>facilityId</c>.</returns>
        public static string ToEventPath<TSource, TProp>(Expression<Func<TSource, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToRuntimePath(members);
        }

        /// <summary>
        /// Builds a camelCase path relative to the current HTTP response body scope.
        /// </summary>
        /// <typeparam name="TSource">The response body type.</typeparam>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>data.name</c>.</returns>
        public static string ToResponsePath<TSource>(Expression<Func<TSource, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToRuntimePath(members);
        }

        /// <summary>
        /// Builds a camelCase response-body path while preserving the selected value type.
        /// </summary>
        /// <typeparam name="TSource">The response body type.</typeparam>
        /// <typeparam name="TProp">The selected response value type.</typeparam>
        /// <param name="expression">The property-access expression to convert.</param>
        /// <returns>A dot-path like <c>data.name</c>.</returns>
        public static string ToResponsePath<TSource, TProp>(Expression<Func<TSource, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToRuntimePath(members);
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
                var suffix = "[" + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
                if (members.Count == 0)
                    members.Add(suffix);
                else
                    members[members.Count - 1] += suffix;
                return members;
            }

            throw new InvalidOperationException(
                $"ExpressionPathHelper only supports property-access chains and MVC indexer paths " +
                $"(e.g. m => m.Address.City or m => m.Items[0].Sku). Got unsupported expression node: {expr.NodeType}.");
        }

        /// <summary>
        /// Extracts the MVC model-binding path from a model expression.
        /// </summary>
        /// <remarks>
        /// <c>m =&gt; m.FacilityId</c> becomes <c>"FacilityId"</c>,
        /// <c>m =&gt; m.Address.City</c> becomes <c>"Address.City"</c>.
        /// Dot-notation preserves the model structure for HTTP gather.
        /// </remarks>
        /// <typeparam name="TModel">The view model that owns the property path.</typeparam>
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
        /// Extracts the MVC model-binding path while preserving the selected value type.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the property path.</typeparam>
        /// <typeparam name="TProp">The selected model value type.</typeparam>
        /// <param name="expression">The model property expression.</param>
        /// <returns>A dot-separated binding path like <c>"Address.City"</c>.</returns>
        public static string ToPropertyName<TModel, TProp>(Expression<Func<TModel, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members.ConvertAll(PascalRestore));
        }

        /// <summary>
        /// Converts a model expression to the MVC DOM element ID generated by <c>Html.IdFor()</c>.
        /// </summary>
        /// <remarks>
        /// <c>m =&gt; m.FacilityId</c> becomes <c>"FacilityId"</c>,
        /// <c>m =&gt; m.Address.City</c> becomes <c>"Address_City"</c>.
        /// Underscores match the <c>Html.IdFor()</c> convention.
        /// </remarks>
        /// <typeparam name="TModel">The view model that owns the property path.</typeparam>
        /// <param name="expression">The model property expression.</param>
        /// <returns>An underscore-separated element ID like <c>"Address_City"</c>.</returns>
        public static string ToElementId<TModel>(Expression<Func<TModel, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToMvcElementId(string.Join(".", members.ConvertAll(PascalRestore)));
        }

        /// <summary>
        /// Converts a model expression to the MVC DOM element ID while preserving the selected value type.
        /// </summary>
        /// <typeparam name="TModel">The view model that owns the property path.</typeparam>
        /// <typeparam name="TProp">The selected model value type.</typeparam>
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

        private static string ToRuntimePath(List<string> members)
        {
            var segments = new List<string>();
            foreach (var member in members)
                AddRuntimePathSegments(member, segments);

            return string.Join(".", segments);
        }

        private static void AddRuntimePathSegments(string member, List<string> segments)
        {
            var position = 0;
            while (position < member.Length)
            {
                var bracket = member.IndexOf('[', position);
                if (bracket < 0)
                {
                    segments.Add(member.Substring(position));
                    return;
                }

                if (bracket > position)
                    segments.Add(member.Substring(position, bracket - position));

                var close = member.IndexOf(']', bracket + 1);
                segments.Add(member.Substring(bracket + 1, close - bracket - 1));
                position = close + 1;
            }
        }

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
