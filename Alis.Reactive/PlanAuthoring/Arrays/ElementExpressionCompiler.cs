using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// Compiles a captured per-element lambda into the closed plan-node algebra. A predicate
    /// becomes a SYNC <see cref="Condition"/> (compare/all/any/not) whose leaves read the
    /// element scope; reads rooted at the lambda parameter become element reads, everything else
    /// is evaluated to a literal. Anything outside the supported set throws at plan render time
    /// (when the Razor view is first requested) — keeping the captured DSL deterministic.
    /// </summary>
    internal static class ElementExpressionCompiler
    {
        // The methods a lambda may call on an item, each a C# method with a JavaScript equivalent that is
        // pure: a per-element call is a value projection, so it must not mutate. The text methods, and the
        // child element's own. The table is read by the method the lambda calls, so a C# name never misses
        // its JavaScript one in silence.
        private static readonly IReadOnlyDictionary<MethodInfo, string> PureMethods = new Dictionary<MethodInfo, string>
        {
            [MethodOf<ChildElement>(child => child.GetAttribute(""))] = "getAttribute",
            [MethodOf<string>(text => text.Trim())] = "trim",
            [MethodOf<string>(text => text.TrimStart())] = "trimStart",
            [MethodOf<string>(text => text.TrimEnd())] = "trimEnd",
            [MethodOf<string>(text => text.ToUpper())] = "toUpperCase",
            [MethodOf<string>(text => text.ToUpperInvariant())] = "toUpperCase",
            [MethodOf<string>(text => text.ToLower())] = "toLowerCase",
            [MethodOf<string>(text => text.ToLowerInvariant())] = "toLowerCase",
        };

        private static MethodInfo MethodOf<T>(Expression<Func<T, object?>> call) =>
            ((MethodCallExpression)Expressions.StripConvert(call.Body)).Method;

        internal static Condition CompilePredicate(LambdaExpression predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return CompileCondition(Expressions.StripConvert(predicate.Body), predicate.Parameters[0]);
        }

        /// <summary>
        /// Compiles a per-element value selector (<c>x =&gt; x.Balance</c>, <c>x =&gt; x</c>) to a
        /// value expression read against the element scope. v1 supports element member reads and
        /// the element itself; richer projections (object init, arithmetic) throw at plan render time.
        /// </summary>
        internal static ValueExpression CompileProjection(LambdaExpression projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            return CompileValue(Expressions.StripConvert(projection.Body), projection.Parameters[0]);
        }

        private static Condition CompileCondition(Expression body, ParameterExpression element)
        {
            if (body is BinaryExpression binary)
            {
                switch (binary.NodeType)
                {
                    case ExpressionType.AndAlso:
                        return Condition.All(
                            CompileCondition(Expressions.StripConvert(binary.Left), element),
                            CompileCondition(Expressions.StripConvert(binary.Right), element));
                    case ExpressionType.OrElse:
                        return Condition.Any(
                            CompileCondition(Expressions.StripConvert(binary.Left), element),
                            CompileCondition(Expressions.StripConvert(binary.Right), element));
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        return CompileComparison(binary, element);
                }
            }

            if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Not)
                return Condition.Not(CompileCondition(Expressions.StripConvert(unary.Operand), element));

            if (body is MethodCallExpression call && call.Object != null && TryTextTest(call, out var textTest))
            {
                var argument = Expressions.StripConvert(call.Arguments[0]);
                if (Expressions.Reads(argument, element))
                    throw new InvalidOperationException(
                        "String operation arguments (Contains/StartsWith/EndsWith) must be constants or captured " +
                        "values, not element-scope reads — the runtime text operand requires a literal. Got: " + argument);

                return Condition.Text(textTest, CompileValue(Expressions.StripConvert(call.Object), element), FixedText(argument), Shape.String);
            }

            if (body.Type == typeof(bool))
                return Condition.Unary(ValueTest.Truthy, CompileValue(body, element), Shape.Boolean);

            throw new InvalidOperationException(
                "Array predicate supports comparisons (== != > >= < <=), logical (&& || !), " +
                "string Contains/StartsWith/EndsWith, and boolean members. Unsupported node: " +
                body.NodeType + " (" + body + ").");
        }

        private static Condition CompileComparison(BinaryExpression binary, ParameterExpression element)
        {
            var left = Expressions.StripConvert(binary.Left);
            var right = Expressions.StripConvert(binary.Right);

            // Runtime coerces BOTH operands to one Shape before comparing (sync-condition
            // applies condition.shape to each side, then ===), so the comparison Shape must come
            // from the typed member operand — never positionally from whichever side is written
            // first. This mirrors ConditionSourceBuilder, which derives its single shape from the
            // member's declared type, not from a literal. C# already forbids cross-category
            // comparisons, so this keeps a literal-on-left predicate (e.g. 65 < x.Age) shaped by
            // the member, not the literal.
            Expression memberOperand;
            if (Expressions.Reads(left, element)) memberOperand = left;
            else if (Expressions.Reads(right, element)) memberOperand = right;
            else throw new InvalidOperationException(
                "Array predicate comparison must reference the element on at least one side; " +
                "comparing two constants is a developer mistake. Got: " + binary);

            var shape = Shape.FromClrType(memberOperand.Type);

            return Condition.Ordered(
                ComparisonFor(binary.NodeType),
                CompileOperand(left, element, memberOperand.Type),
                CompileOperand(right, element, memberOperand.Type),
                shape);
        }

        // An operand compared with the member. C# compares an enum as its underlying number, so beside an
        // enum member a constant or captured value arrives folded to that number; the data holds the member's
        // JavaScript text, so the operand is the enum member again, written as every literal is.
        private static ValueExpression CompileOperand(Expression operand, ParameterExpression element, Type memberType)
        {
            var enumType = EnumMember.TypeOf(memberType);
            if (enumType == null || Expressions.Reads(operand, element)) return CompileValue(operand, element);

            var value = Expressions.Evaluate(operand);
            var member = value == null ? null : EnumMember.Of(enumType, value);
            return ValueExpression.LiteralFor(member, Shape.FromClrType(memberType));
        }

        private static ValueExpression CompileValue(Expression expression, ParameterExpression element)
        {
            var node = Expressions.StripConvert(expression);

            if (ExpressionPathHelper.TryJsonPath(node, element, out var path))
            {
                var shape = Shape.FromClrType(node.Type);
                return path.IsNone
                    ? ValueExpression.Get(Target.Item, shape)
                    : ValueExpression.Get(Target.Item, path.Text, shape);
            }

            // A method called on an item's value (x => x.Name.Trim()): the value is rooted at the element and
            // the method is one of the pure table, written as its JavaScript equivalent.
            if (node is MethodCallExpression call && call.Object != null && ExpressionPathHelper.TryJsonPath(call.Object, element, out var targetPath))
            {
                if (!PureMethods.TryGetValue(call.Method, out var methodName))
                    throw new InvalidOperationException(
                        $"'{call.Method.DeclaringType?.Name}.{call.Method.Name}' has no pure JavaScript form the plan can call on an item; " +
                        "the lambda may call " + string.Join(", ", PureMethods.Keys.Select(method => method.DeclaringType?.Name + "." + method.Name).Distinct()) +
                        ". Project the value on the server, or write a reaction for a call that changes the item. Got: " + node);

                var methodArgs = new List<ValueExpression>();
                foreach (var methodArgument in call.Arguments)
                    methodArgs.Add(CompileValue(Expressions.StripConvert(methodArgument), element));

                var unconverted = methodArgs.Select(_ => Shape.None).ToList();
                return ValueExpression.Call(
                    Target.Item, targetPath.Then(methodName).TargetText, methodArgs, unconverted, Shape.FromClrType(node.Type), methodName);
            }

            if (Expressions.Reads(node, element))
                throw new InvalidOperationException(
                    "Element selector must be a member access (x => x.Field, x => x.A.B) or the element " +
                    "itself (x => x). Object-init and arithmetic projections are not supported in v1. Got: " + node);

            // Parameter-free: a constant or captured value, evaluated once and written as every literal is
            // (an enum by its JSON name, a date as text).
            var value = Expressions.Evaluate(node);
            return ValueExpression.LiteralFor(value, Shape.FromValue(value));
        }

        private static Comparison ComparisonFor(ExpressionType nodeType) =>
            nodeType switch
            {
                ExpressionType.Equal => Comparison.Equal,
                ExpressionType.NotEqual => Comparison.NotEqual,
                ExpressionType.GreaterThan => Comparison.GreaterThan,
                ExpressionType.GreaterThanOrEqual => Comparison.GreaterThanOrEqual,
                ExpressionType.LessThan => Comparison.LessThan,
                ExpressionType.LessThanOrEqual => Comparison.LessThanOrEqual,
                _ => throw new InvalidOperationException("Unsupported comparison node: " + nodeType),
            };

        private static bool TryTextTest(MethodCallExpression call, out TextTest test)
        {
            if (call.Object?.Type == typeof(string) && call.Arguments.Count == 1)
            {
                switch (call.Method.Name)
                {
                    case "Contains": test = TextTest.Contains; return true;
                    case "StartsWith": test = TextTest.StartsWith; return true;
                    case "EndsWith": test = TextTest.EndsWith; return true;
                }
            }

            test = TextTest.Contains;
            return false;
        }

        // The text a string test compares with: a constant or a captured value, evaluated once here.
        private static string FixedText(Expression argument) =>
            Expressions.Evaluate(argument) as string
            ?? throw new InvalidOperationException("String operation arguments must be text. Got: " + argument);

    }
}
