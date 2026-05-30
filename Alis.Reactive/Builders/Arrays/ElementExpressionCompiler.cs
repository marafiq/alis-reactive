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
    /// is evaluated to a literal. Anything outside the whitelist throws at C# build time — the
    /// same discipline as <c>ExpressionPathHelper</c>, and what keeps the DSL deterministic.
    /// </summary>
    internal static class ElementExpressionCompiler
    {
        internal static ConditionGraph CompilePredicate(LambdaExpression predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return CompileCondition(Unwrap(predicate.Body), predicate.Parameters[0]);
        }

        /// <summary>
        /// Compiles a per-element value selector (<c>x =&gt; x.Balance</c>, <c>x =&gt; x</c>) to a
        /// value expression read against the element scope. v1 supports element member reads and
        /// the element itself; richer projections (object init, arithmetic) throw at build time.
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
                return ConditionGraph.Compare(
                    textOp,
                    ComparisonOperands.Binary(
                        CompileValue(Unwrap(call.Object), element),
                        CompileValue(Unwrap(call.Arguments[0]), element),
                        Shape.String));

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
            var shape = Shape.FromClrType(left.Type);
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
            if (call.Object != null && call.Object.Type == typeof(string) && call.Arguments.Count == 1)
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
