using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// Compiles a captured per-element lambda into the closed plan-node algebra. A predicate
    /// becomes a SYNC <see cref="ConditionGraph"/> (compare/all/any/not) whose leaves read the
    /// element scope; reads rooted at the lambda parameter become element reads, everything else
    /// is evaluated to a literal. Anything outside the supported set throws at plan render time
    /// (when the Razor view is first requested) — keeping the captured DSL deterministic.
    /// </summary>
    internal static class ElementExpressionCompiler
    {
        // Pure, deterministic, side-effect-free element methods only. A per-element method call is a
        // value projection, so it must not mutate; side-effecting methods (e.g. grid Row.setCellValue)
        // belong in a per-element reaction (foreach), not a projection. Grow this set deliberately.
        private static readonly HashSet<string> WhitelistedMethods = new HashSet<string>(StringComparer.Ordinal)
        {
            "getDay", "getMonth", "getFullYear", "getDate", "getHours", "getMinutes", "getSeconds", "getTime",
            "toUpperCase", "toLowerCase", "trim",
            "getAttribute", "hasAttribute",
        };

        internal static ConditionGraph CompilePredicate(LambdaExpression predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return CompileCondition(Unwrap(predicate.Body), predicate.Parameters[0]);
        }

        /// <summary>
        /// Compiles a per-element value selector (<c>x =&gt; x.Balance</c>, <c>x =&gt; x</c>) to a
        /// value expression read against the element scope. v1 supports element member reads and
        /// the element itself; richer projections (object init, arithmetic) throw at plan render time.
        /// </summary>
        internal static ValueExpression CompileProjection(LambdaExpression projection)
        {
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            return CompileValue(Unwrap(projection.Body), projection.Parameters[0]);
        }

        private static ConditionGraph CompileCondition(Expression body, ParameterExpression element)
        {
            if (body is BinaryExpression binary)
            {
                switch (binary.NodeType)
                {
                    case ExpressionType.AndAlso:
                        return ConditionGraph.All(
                            CompileCondition(Unwrap(binary.Left), element),
                            CompileCondition(Unwrap(binary.Right), element));
                    case ExpressionType.OrElse:
                        return ConditionGraph.Any(
                            CompileCondition(Unwrap(binary.Left), element),
                            CompileCondition(Unwrap(binary.Right), element));
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
                return ConditionGraph.Not(CompileCondition(Unwrap(unary.Operand), element));

            if (body is MethodCallExpression call && call.Object != null && TryStringOperator(call, out var textOp))
            {
                var argument = Unwrap(call.Arguments[0]);
                if (ReferencesParameter(argument, element))
                    throw new InvalidOperationException(
                        "String operation arguments (Contains/StartsWith/EndsWith) must be constants or captured " +
                        "values, not element-scope reads — the runtime text operand requires a literal. Got: " + argument);

                return ConditionGraph.Compare(
                    textOp,
                    ComparisonOperands.Binary(
                        CompileValue(Unwrap(call.Object), element),
                        CompileValue(argument, element),
                        Shape.String));
            }

            if (body.Type == typeof(bool))
                return ConditionGraph.Compare(
                    CompareOperator.Truthy,
                    ComparisonOperands.Unary(CompileValue(body, element), Shape.Boolean));

            throw new InvalidOperationException(
                "Array predicate supports comparisons (== != > >= < <=), logical (&& || !), " +
                "string Contains/StartsWith/EndsWith, and boolean members. Unsupported node: " +
                body.NodeType + " (" + body + ").");
        }

        private static ConditionGraph CompileComparison(BinaryExpression binary, ParameterExpression element)
        {
            var left = Unwrap(binary.Left);
            var right = Unwrap(binary.Right);

            // The runtime coerces BOTH operands to one Shape before comparing (sync-condition
            // applies condition.shape to each side, then ===), so the comparison Shape must come
            // from the typed member operand — never positionally from whichever side is written
            // first. This mirrors ConditionSourceBuilder, which derives its single shape from the
            // member's declared type, not from a literal. C# already forbids cross-category
            // comparisons, so this keeps a literal-on-left predicate (e.g. 65 < x.Age) shaped by
            // the member, not the literal.
            Expression memberOperand;
            if (ReferencesParameter(left, element)) memberOperand = left;
            else if (ReferencesParameter(right, element)) memberOperand = right;
            else throw new InvalidOperationException(
                "Array predicate comparison must reference the element on at least one side; " +
                "comparing two constants is a developer mistake. Got: " + binary);

            var shape = Shape.FromClrType(memberOperand.Type);

            return ConditionGraph.Compare(
                CompareOperatorFor(binary.NodeType),
                ComparisonOperands.Binary(CompileValue(left, element), CompileValue(right, element), shape));
        }

        private static ValueExpression CompileValue(Expression expression, ParameterExpression element)
        {
            var node = Unwrap(expression);

            if (TryElementPath(node, element, out var path))
            {
                var shape = Shape.FromClrType(node.Type);
                return path.Length == 0
                    ? ValueExpression.ReadWholeElement(shape)
                    : ValueExpression.ReadPayload(PayloadSource.Element(), path, shape);
            }

            // Per-element method call (x => x.GetDay(), x => x.Address.GetFormatted()) — the receiver must
            // be rooted at the element; the method must be whitelisted (pure). Reuses the RuntimePath.call engine.
            if (node is MethodCallExpression call && call.Object != null && TryElementPath(call.Object, element, out var receiverPath))
            {
                var methodName = CamelCase(call.Method.Name);
                if (!WhitelistedMethods.Contains(methodName))
                    throw new InvalidOperationException(
                        "Element method '" + methodName + "' is not whitelisted for the array DSL. Whitelist a PURE, " +
                        "deterministic method in ElementExpressionCompiler.WhitelistedMethods, or use a server-side " +
                        "projection. Side-effecting per-element calls need a reaction, not a projection. Got: " + node);

                var methodArgs = new List<ValueExpression>();
                foreach (var arg in call.Arguments)
                    methodArgs.Add(CompileValue(Unwrap(arg), element));

                return ValueExpression.InvokeElement(receiverPath, methodName, Shape.FromClrType(node.Type), methodArgs);
            }

            if (ReferencesParameter(node, element))
                throw new InvalidOperationException(
                    "Element selector must be a member access (x => x.Field, x => x.A.B) or the element " +
                    "itself (x => x). Object-init and arithmetic projections are not supported in v1. Got: " + node);

            // Parameter-free: a constant or captured value — evaluate it to a literal.
            var value = Expression.Lambda(node).Compile().DynamicInvoke();
            return ValueExpression.LiteralFromValue(value);
        }

        private static bool ReferencesParameter(Expression expression, ParameterExpression element)
        {
            var finder = new ParameterUsageVisitor(element);
            finder.Visit(expression);
            return finder.Found;
        }

        private sealed class ParameterUsageVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;

            internal ParameterUsageVisitor(ParameterExpression parameter) => _parameter = parameter;

            internal bool Found { get; private set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == _parameter) Found = true;
                return base.VisitParameter(node);
            }
        }

        private static bool TryElementPath(Expression expression, ParameterExpression element, out string path)
        {
            var segments = new List<string>();
            var current = Unwrap(expression);
            while (current is MemberExpression member && member.Expression != null)
            {
                segments.Insert(0, CamelCase(member.Member.Name));
                current = Unwrap(member.Expression);
            }

            if (current == element)
            {
                path = string.Join(".", segments);
                return true;
            }

            path = string.Empty;
            return false;
        }

        private static CompareOperator CompareOperatorFor(ExpressionType nodeType) =>
            nodeType switch
            {
                ExpressionType.Equal => CompareOperator.Eq,
                ExpressionType.NotEqual => CompareOperator.Neq,
                ExpressionType.GreaterThan => CompareOperator.Gt,
                ExpressionType.GreaterThanOrEqual => CompareOperator.Gte,
                ExpressionType.LessThan => CompareOperator.Lt,
                ExpressionType.LessThanOrEqual => CompareOperator.Lte,
                _ => throw new InvalidOperationException("Unsupported comparison node: " + nodeType),
            };

        private static bool TryStringOperator(MethodCallExpression call, out CompareOperator op)
        {
            if (call.Object?.Type == typeof(string) && call.Arguments.Count == 1)
            {
                switch (call.Method.Name)
                {
                    case "Contains": op = CompareOperator.Contains; return true;
                    case "StartsWith": op = CompareOperator.StartsWith; return true;
                    case "EndsWith": op = CompareOperator.EndsWith; return true;
                }
            }

            op = CompareOperator.Eq;
            return false;
        }

        private static Expression Unwrap(Expression expression)
        {
            while (expression is UnaryExpression unary &&
                   (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
            {
                expression = unary.Operand;
            }

            return expression;
        }

        private static string CamelCase(string name) =>
            string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
