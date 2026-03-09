using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>
    /// Converts a member expression like x => x.Address.City into a camelCase dot-path
    /// prefixed with "evt." for use as a plan source binding.
    /// </summary>
    public static class ExpressionPathHelper
    {
        public static string ToEventPath<TSource>(Expression<Func<TSource, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return "evt." + string.Join(".", members);
        }

        /// <summary>
        /// Typed overload — no boxing Convert node needed because TProp matches the property type.
        /// Used by the typed condition builders.
        /// </summary>
        public static string ToEventPath<TSource, TProp>(Expression<Func<TSource, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return "evt." + string.Join(".", members);
        }

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
