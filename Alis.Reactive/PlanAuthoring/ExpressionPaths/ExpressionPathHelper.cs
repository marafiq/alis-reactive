using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    // Converts property expressions into the four framework path shapes:
    // Reactive Plan value paths, MVC model-binding names, MVC element IDs,
    // and JSON request-body paths.
    // Scoped paths add an explicit prefix such as "evt" or "responseBody";
    // event and response helpers stay scope-relative because PayloadSource
    // already identifies the value scope.
    // Only property chains and MVC-style constant indexers are valid.
    internal static class ExpressionPathHelper
    {
        public static string ToPath<TSource>(string prefix, Expression<Func<TSource, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return prefix + "." + ToRuntimePath(members);
        }

        public static string ToPath<TSource, TProp>(string prefix, Expression<Func<TSource, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return prefix + "." + ToRuntimePath(members);
        }

        public static string ToEventPath<TPayload>(Expression<Func<TPayload, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToRuntimePath(members);
        }

        public static string ToEventPath<TPayload, TProp>(Expression<Func<TPayload, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToRuntimePath(members);
        }

        public static string ToResponsePath<TResponse>(Expression<Func<TResponse, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToRuntimePath(members);
        }

        public static string ToResponsePath<TResponse, TProp>(Expression<Func<TResponse, TProp>> expression)
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
                var suffix = "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
                if (members.Count == 0)
                    members.Add(suffix);
                else
                    members[members.Count - 1] += suffix;
                return members;
            }

            throw UnsupportedExpression(expr);
        }

        // A gathered model member: its MVC binding name, and its body path read from the member's types.
        internal static BindingPath ToMemberBindingPath<TModel, TProp>(Expression<Func<TModel, TProp>> expression) =>
            BindingPath.ForMember(ToPropertyName(expression), ToBodyPath(expression));

        // A JSON request body needs the member's structure, which the MVC name text cannot carry:
        // Lines[0] and Administered[1042] read alike, yet one is a list item and the other a
        // dictionary entry. The expression's types tell them apart.
        internal static Path ToBodyPath(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return BodyPathOf(expression.Body);
        }

        private static Path BodyPathOf(Expression expr)
        {
            expr = UnwrapConvert(expr);

            if (expr is ParameterExpression)
                return Path.None;

            if (expr is MemberExpression member)
                return BodyPathOf(member.Expression!).Then(member.Member.Name);

            if (TryIndexAccess(expr, out var collection, out var index))
            {
                var collectionPath = BodyPathOf(collection);
                return IsDictionary(collection.Type)
                    ? collectionPath.Then(index.ToString(CultureInfo.InvariantCulture))
                    : collectionPath.AtIndex(index);
            }

            throw UnsupportedExpression(expr);
        }

        // A dictionary indexer takes a key, not a position, so its entries are named in a JSON body.
        private static bool IsDictionary(Type collectionType) =>
            typeof(System.Collections.IDictionary).IsAssignableFrom(collectionType)
            || new[] { collectionType }.Concat(collectionType.GetInterfaces()).Any(IsGenericDictionaryContract);

        private static bool IsGenericDictionaryContract(Type contract)
        {
            if (!contract.IsGenericType) return false;

            var definition = contract.GetGenericTypeDefinition();
            return definition == typeof(IDictionary<,>) || definition == typeof(IReadOnlyDictionary<,>);
        }

        private static InvalidOperationException UnsupportedExpression(Expression expr) =>
            new InvalidOperationException(
                $"ExpressionPathHelper only supports property-access chains and MVC indexer paths " +
                $"(e.g. m => m.Address.City or m => m.Items[0].Sku). Got unsupported expression node: {expr.NodeType}.");

        // HTTP gather needs MVC model-binding names such as "Address.City".
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

        public static string ToPropertyName<TModel, TProp>(Expression<Func<TModel, TProp>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return string.Join(".", members.ConvertAll(PascalRestore));
        }

        // Component IDs must match MVC Html.IdFor(), including nested-property underscores.
        public static string ToElementId<TModel>(Expression<Func<TModel, object?>> expression)
        {
            var members = ExtractMemberChain(expression.Body);
            return ToMvcElementId(string.Join(".", members.ConvertAll(PascalRestore)));
        }

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
