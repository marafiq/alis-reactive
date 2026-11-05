using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.Members
{
    internal static class ExpressionPathHelper
    {
        internal static Path ToJsonPath(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return JsonPath(Steps(expression.Body));
        }

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

        internal static Path ToBodyPath(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return BodyPath(Steps(expression.Body));
        }

        internal static string ToMemberName(Expression memberChain)
        {
            if (memberChain == null) throw new ArgumentNullException(nameof(memberChain));
            return MvcName(Steps(memberChain));
        }

        internal static bool TryMemberName(Expression memberChain, ParameterExpression root, out string name)
        {
            name = TrySteps(memberChain, out var steps, out var parameter) && parameter == root ? MvcName(steps) : "";
            return name.Length > 0;
        }

        internal static Type ToDeclarer(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var bound = Steps(expression.Body).LastOrDefault(step => step.IsMember);
            if (!bound.IsMember)
            {
                throw new InvalidOperationException(
                    $"{expression} binds no member. An input's id is scoped by the class that declares the member it binds: " +
                    "render the input in a view of that class, like m => m.Name.");
            }

            return DeclarerOf(bound.Access!);
        }

        internal static string ToMvcElementId(string propertyPath)
            => propertyPath
                .Replace(".", "_")
                .Replace("[", "_")
                .Replace("]", "_");

        internal static Type ToPropertyType(LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return Expressions.TypeOf(expression.Body);
        }

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

        private static Path JsonPath(IReadOnlyList<AccessStep> steps)
            => steps.Aggregate(Path.None, (path, step) => step.IsPosition
                ? path.AtIndex(step.Position)
                : path.Then(step.IsKey ? step.Key : JsonMemberName.Of(step.Access!.Member)));

        private static Path BodyPath(IReadOnlyList<AccessStep> steps)
            => steps.Aggregate(Path.None, (path, step) => step.IsPosition
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

        private static bool IsDictionary(Type collectionType)
            => typeof(System.Collections.IDictionary).IsAssignableFrom(collectionType)
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

        private static int EvaluateIndex(Expression expression)
            => Convert.ToInt32(Expressions.Evaluate(expression), CultureInfo.InvariantCulture);

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

            internal MemberExpression? Access { get; }
        }
    }
}
