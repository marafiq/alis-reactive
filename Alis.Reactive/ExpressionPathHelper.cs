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
