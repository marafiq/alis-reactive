using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    // The one walk of a member chain. A chain from a lambda's parameter is read once into steps, a
    // member by name, a list item by position or a dictionary entry by key, and written in the three
    // forms the framework uses: a JSON path (camel-cased names, read in the browser), a body path (the
    // members' own names, sent to the server) and an MVC binding name (text, the boundary MVC reads).
    internal static class ExpressionPathHelper
    {
        /// <summary>The JSON path a member chain reads in the browser: a response, a payload, an item.</summary>
        internal static Path ToJsonPath(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return JsonPath(Steps(expression.Body));
        }

        /// <summary>The JSON path of a member chain rooted at <paramref name="root"/>; false for any other expression.</summary>
        internal static bool TryJsonPath(Expression expression, ParameterExpression root, out Path path)
        {
            if (TrySteps(expression, out var steps, out var parameter) && parameter == root)
            {
                path = JsonPath(steps);
                return true;
            }

            path = Path.None;
            return false;
        }

        /// <summary>The body path a gathered member is sent along: the members' own names, list positions, dictionary keys as names.</summary>
        internal static Path ToBodyPath(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return BodyPath(Steps(expression.Body));
        }

        /// <summary>The MVC binding name of a member chain: <c>Address.City</c>, <c>Lines[0].Sku</c>, <c>Administered[1042]</c>.</summary>
        internal static string ToMemberName(Expression memberChain)
        {
            if (memberChain == null) throw new ArgumentNullException(nameof(memberChain));
            return MvcName(Steps(memberChain));
        }

        /// <summary>The MVC binding name of a member chain rooted at <paramref name="root"/>; false for any other expression.</summary>
        internal static bool TryMemberName(Expression memberChain, ParameterExpression root, out string name)
        {
            if (TrySteps(memberChain, out var steps, out var parameter) && parameter == root)
            {
                name = MvcName(steps);
                return true;
            }

            name = string.Empty;
            return false;
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

        // A gathered model member: its MVC binding name, and its body path read from the member's types.
        internal static BindingPath ToMemberBindingPath(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return BindingPath.ForMember(ToMemberName(expression.Body), ToBodyPath(expression));
        }

        private static Path JsonPath(IReadOnlyList<AccessStep> steps) =>
            steps.Aggregate(Path.None, (path, step) => step.IsPosition
                ? path.AtIndex(step.Position)
                : path.Then(step.IsKey ? step.Key : JsonMemberName.Of(step.Name)));

        // A JSON body needs the member's structure, which the MVC name text cannot carry: Lines[0] and
        // Administered[1042] read alike, yet one is a list item and the other a dictionary entry.
        private static Path BodyPath(IReadOnlyList<AccessStep> steps) =>
            steps.Aggregate(Path.None, (path, step) => step.IsPosition
                ? path.AtIndex(step.Position)
                : path.Then(step.IsKey ? step.Key : step.Name));

        private static string MvcName(IReadOnlyList<AccessStep> steps)
        {
            var name = new StringBuilder();
            foreach (var step in steps)
            {
                if (step.IsMember)
                {
                    if (name.Length > 0) name.Append('.');
                    name.Append(step.Name);
                }
                else
                {
                    name.Append('[').Append(step.IsKey ? step.Key : step.Position.ToString(CultureInfo.InvariantCulture)).Append(']');
                }
            }

            return name.ToString();
        }

        private static List<AccessStep> Steps(Expression expr)
        {
            if (TrySteps(expr, out var steps, out _)) return steps;
            throw new InvalidOperationException(
                "ExpressionPathHelper only supports property-access chains and MVC indexer paths " +
                $"(e.g. m => m.Address.City or m => m.Items[0].Sku). Got unsupported expression node: {UnwrapConvert(expr).NodeType}.");
        }

        // The steps from the parameter the chain starts at; false when the expression is not such a chain.
        private static bool TrySteps(Expression expr, out List<AccessStep> steps, out ParameterExpression? parameter)
        {
            expr = UnwrapConvert(expr);

            if (expr is ParameterExpression root)
            {
                steps = new List<AccessStep>();
                parameter = root;
                return true;
            }

            if (expr is MemberExpression { Expression: { } owner } member)
            {
                if (!TrySteps(owner, out steps, out parameter)) return false;
                steps.Add(AccessStep.Member(member.Member.Name));
                return true;
            }

            if (TryIndexAccess(expr, out var collection, out var index))
            {
                if (!TrySteps(collection, out steps, out parameter)) return false;
                steps.Add(IsDictionary(collection.Type) ? AccessStep.ForKey(index) : AccessStep.AtPosition(index));
                return true;
            }

            steps = new List<AccessStep>();
            parameter = null;
            return false;
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
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
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

        // One step of a member chain: a member by name, a list item by position, or a dictionary entry by key.
        private readonly struct AccessStep
        {
            private readonly string _kind;

            private AccessStep(string kind, string name, int position)
            {
                _kind = kind;
                Name = name;
                Position = position;
            }

            internal static AccessStep Member(string name) => new AccessStep("member", name, -1);

            internal static AccessStep AtPosition(int position) => new AccessStep("position", "", position);

            internal static AccessStep ForKey(int key) => new AccessStep("key", key.ToString(CultureInfo.InvariantCulture), -1);

            internal bool IsMember => _kind == "member";

            internal bool IsPosition => _kind == "position";

            internal bool IsKey => _kind == "key";

            internal string Name { get; }

            internal string Key => Name;

            internal int Position { get; }
        }
    }
}
