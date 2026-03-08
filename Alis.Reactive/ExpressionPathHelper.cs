using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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
        /// Extracts the CLR property/field type from a member expression, unwrapping
        /// any Convert node that the compiler inserts for boxing value types.
        /// </summary>
        public static Type GetPropertyType<TSource>(Expression<Func<TSource, object?>> expression)
        {
            var body = expression.Body;
            if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
                body = unary.Operand;

            if (body is MemberExpression member)
            {
                if (member.Member is PropertyInfo prop) return prop.PropertyType;
                if (member.Member is FieldInfo field) return field.FieldType;
            }

            return typeof(object);
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

        private static string CamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
