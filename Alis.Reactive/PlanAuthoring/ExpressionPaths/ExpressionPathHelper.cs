using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    // Converts property expressions into the four framework path shapes:
    // Reactive Plan value paths, MVC model-binding names, MVC element IDs,
    // and JSON request-body paths.
    // Only property chains and MVC-style constant indexers are valid.
    internal static class ExpressionPathHelper
    {
        // A runtime path reads JSON: a response body, a dispatched or pushed payload, or a component's
        // event arguments. The framework and ASP.NET Core write members with System.Text.Json's camel-case
        // naming ("MRN" is "mrn"), and browser and component events already name theirs in camel case.
        internal static string ToEventPath(LambdaExpression expression) =>
            string.Join(".", MemberChain(expression.Body, JsonMemberName.Of).Select(step => step.Trim('[', ']')));

        // A member chain rooted at the model parameter (a rule's lambda body, or part of a condition),
        // named the way MVC binds it: "Address.City", "Lines[0].Sku".
        internal static string ToMemberName(Expression memberChain)
        {
            if (memberChain == null) throw new ArgumentNullException(nameof(memberChain));
            var name = new StringBuilder();
            foreach (var step in MemberChain(memberChain, member => member))
            {
                var isIndex = step[0] == '[';
                if (name.Length > 0 && !isIndex) name.Append('.');
                name.Append(step);
            }
            return name.ToString();
        }

        // Component IDs match MVC's Html.IdFor(), including nested-property underscores.
        internal static string ToElementId(LambdaExpression expression) => ToMvcElementId(ToMemberName(expression.Body));

        internal static string ToMvcElementId(string propertyPath) =>
            propertyPath
                .Replace(".", "_")
                .Replace("[", "_")
                .Replace("]", "_");

        internal static Type ToPropertyType(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return UnwrapConvert(expression.Body).Type;
        }

        // The steps from the parameter: each member by its name, each constant index as "[n]".
        private static List<string> MemberChain(Expression expr, Func<string, string> memberName)
        {
            expr = UnwrapConvert(expr);

            if (expr is ParameterExpression)
                return new List<string>();

            if (expr is MemberExpression { Expression: { } owner } member)
            {
                var steps = MemberChain(owner, memberName);
                steps.Add(memberName(member.Member.Name));
                return steps;
            }

            if (TryIndexAccess(expr, out var collection, out var index))
            {
                var steps = MemberChain(collection, memberName);
                steps.Add("[" + index.ToString(CultureInfo.InvariantCulture) + "]");
                return steps;
            }

            throw UnsupportedExpression(expr);
        }

        // A gathered model member: its MVC binding name, and its body path read from the member's types.
        internal static BindingPath ToMemberBindingPath(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return BindingPath.ForMember(ToMemberName(expression.Body), ToBodyPath(expression));
        }

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

            if (expr is MemberExpression { Expression: { } owner } member)
                return BodyPathOf(owner).Then(member.Member.Name);

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
