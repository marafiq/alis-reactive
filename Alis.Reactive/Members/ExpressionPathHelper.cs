using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    // The one walk of a member chain. A chain from a lambda's parameter is read once into steps, a
    // member by name, a list item by position or a dictionary entry by key, and written in the three
    // forms the framework uses: a JSON path (camel-cased names, read in the browser), a body path (the
    // members' own names, sent to the server) and an MVC binding name (text, the boundary MVC reads).
    // Its last member names the class that declares it, which scopes the id of the member's input.
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

        /// <summary>The MVC binding name of a chain of members from <paramref name="root"/>; false for any other expression, the root itself included.</summary>
        internal static bool TryMemberName(Expression memberChain, ParameterExpression root, out string name)
        {
            name = TrySteps(memberChain, out var steps, out var parameter) && parameter == root ? MvcName(steps) : "";
            return name.Length > 0;
        }

        /// <summary>
        /// The class whose scope the id of the member's input takes: the class that declares the chain's last
        /// member, by its base definition (<c>KinCard</c> for <c>m =&gt; m.NextOfKin.Name</c>, the class that
        /// declares <c>Phones</c> for <c>m =&gt; m.Phones[0]</c>). A member an interface declares, read on a class,
        /// is declared by the class that implements it; read on the interface itself it is the interface, which
        /// scopes no id.
        /// </summary>
        /// <remarks>
        /// A lambda with no member (<c>m =&gt; m</c> in a template of a text member) is refused: the view's own
        /// model is the member's type, not the class that declares the member, so its id would be one the page's
        /// rules, sends and reads never derive.
        /// </remarks>
        internal static Type ToDeclarer(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var bound = Steps(expression.Body).LastOrDefault(step => step.IsMember);
            if (!bound.IsMember)
                throw new InvalidOperationException(
                    $"{expression} binds no member. An input's id is scoped by the class that declares the member it binds: " +
                    "render the input in a view of that class, like m => m.Name.");
            return DeclarerOf(bound.Access!);
        }

        // Component IDs match MVC's Html.IdFor(), including nested-property underscores.
        internal static string ToMvcElementId(string propertyPath) =>
            propertyPath
                .Replace(".", "_")
                .Replace("[", "_")
                .Replace("]", "_");

        internal static Type ToPropertyType(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return Expressions.TypeOf(expression.Body);
        }

        // A gathered model member: its MVC binding name, and its body path read from the member's types.
        internal static BindingPath ToMemberBindingPath(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return BindingPath.ForMember(ToMemberName(expression.Body), ToBodyPath(expression));
        }

        private static Type DeclarerOf(MemberExpression access)
        {
            if (!(access.Member is PropertyInfo property)) return access.Member.DeclaringType!;

            var getter = property.GetMethod!;
            var owner = Expressions.TypeOf(access.Expression!);
            var interfaceMemberReadOnAClass = getter.DeclaringType!.IsInterface && !owner.IsInterface && !owner.IsArray;
            if (interfaceMemberReadOnAClass)
            {
                var map = owner.GetInterfaceMap(getter.DeclaringType);
                getter = map.TargetMethods[Array.IndexOf(map.InterfaceMethods, getter)];
            }
            return getter.GetBaseDefinition().DeclaringType!;
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
                $"Select a member by its path, like m => m.Address.City or m => m.Items[0].Sku; '{Expressions.StripConvert(expr)}' is not one.");
        }

        // The steps from the parameter the chain starts at; false when the expression is not such a chain.
        private static bool TrySteps(Expression expr, out List<AccessStep> steps, out ParameterExpression? parameter)
        {
            expr = Expressions.StripConvert(expr);

            if (expr is ParameterExpression root)
            {
                steps = new List<AccessStep>();
                parameter = root;
                return true;
            }

            if (expr is MemberExpression { Expression: { } owner } member)
            {
                if (!TrySteps(owner, out steps, out parameter)) return false;
                steps.Add(AccessStep.Member(member));
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

        private static int EvaluateIndex(Expression expression) =>
            Convert.ToInt32(Expressions.Evaluate(expression), CultureInfo.InvariantCulture);

        // One step of a member chain: a member by name, a list item by position, or a dictionary entry by key.
        private readonly struct AccessStep
        {
            private readonly string _kind;

            private AccessStep(string kind, string name, int position, MemberExpression? access)
            {
                _kind = kind;
                Name = name;
                Position = position;
                Access = access;
            }

            internal static AccessStep Member(MemberExpression access) => new AccessStep("member", access.Member.Name, -1, access);

            internal static AccessStep AtPosition(int position) => new AccessStep("position", "", position, null);

            internal static AccessStep ForKey(int key) => new AccessStep("key", key.ToString(CultureInfo.InvariantCulture), -1, null);

            internal bool IsMember => _kind == "member";

            internal bool IsPosition => _kind == "position";

            internal bool IsKey => _kind == "key";

            internal string Name { get; }

            internal string Key => Name;

            internal int Position { get; }

            // The member read, on the owner it is read on: what names the class that declares it.
            internal MemberExpression? Access { get; }
        }
    }
}
