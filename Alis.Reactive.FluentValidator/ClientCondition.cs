using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// The browser's form of a <c>When</c> condition: a condition over the fields of the model, and the
    /// fields it reads. A C# condition has a browser form when every part of it is a comparison of a
    /// model member with a value the condition fixes, joined by <c>&amp;&amp;</c>, <c>||</c> and <c>!</c>.
    /// </summary>
    internal sealed class ClientCondition
    {
        private ClientCondition(FieldCondition condition, IReadOnlyList<ClientValidationFieldReference> fields)
        {
            Condition = condition;
            Fields = fields;
        }

        internal FieldCondition Condition { get; }

        internal IReadOnlyList<ClientValidationFieldReference> Fields { get; }

        /// <summary>The condition's browser form, or null when part of it has none (it stays server-only).</summary>
        internal static ClientCondition? From(LambdaExpression predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new Translator(predicate.Parameters[0]).Translate(predicate.Body);
        }

        internal ClientCondition Negated() => new ClientCondition(FieldCondition.Not(Condition), Fields);

        internal static ClientCondition AllOf(IReadOnlyList<ClientCondition> conditions) =>
            conditions.Count == 1
                ? conditions[0]
                : new ClientCondition(
                    FieldCondition.All(conditions.Select(condition => condition.Condition).ToArray()),
                    FieldsOf(conditions.SelectMany(condition => condition.Fields)));

        private static ClientCondition Combine(ClientCondition left, ClientCondition right, Func<FieldCondition, FieldCondition, FieldCondition> join) =>
            new ClientCondition(join(left.Condition, right.Condition), FieldsOf(left.Fields.Concat(right.Fields)));

        private static ClientCondition Compare(ClientValidationFieldReference field, CompareOperator op, FieldComparisonValue value) =>
            new ClientCondition(FieldCondition.Compare(field.Path, op, value), new[] { field });

        // One reference per field; a field read with two shapes is a contradiction in the condition.
        private static IReadOnlyList<ClientValidationFieldReference> FieldsOf(IEnumerable<ClientValidationFieldReference> fields)
        {
            var byPath = new Dictionary<string, ClientValidationFieldReference>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                if (!byPath.TryGetValue(field.Path.Value, out var existing))
                {
                    byPath.Add(field.Path.Value, field);
                    continue;
                }

                if (!existing.Shape.Equals(field.Shape))
                    throw new InvalidOperationException(
                        $"Validation condition field '{field.Path.Value}' is read with two shapes: " +
                        $"'{existing.Shape.Kind}' and '{field.Shape.Kind}'.");
            }

            return byPath.Values.ToArray();
        }

        private sealed class Translator
        {
            private readonly ParameterExpression _model;

            internal Translator(ParameterExpression model)
            {
                _model = model;
            }

            internal ClientCondition? Translate(Expression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.AndAlso:
                        return Join((BinaryExpression)expression, (l, r) => FieldCondition.All(l, r));
                    case ExpressionType.OrElse:
                        return Join((BinaryExpression)expression, (l, r) => FieldCondition.Any(l, r));
                    case ExpressionType.Not:
                        return Negate(((UnaryExpression)expression).Operand);
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        return Comparison((BinaryExpression)expression);
                    case ExpressionType.Call:
                        return Call((MethodCallExpression)expression);
                    default:
                        if (IsHasValue(expression, out var nullable))
                            return Compare(FieldOf(nullable), CompareOperator.NotNull, FieldComparisonValue.None);
                        return IsField(expression) ? Truthy(expression) : null;
                }
            }

            private ClientCondition? Join(BinaryExpression expression, Func<FieldCondition, FieldCondition, FieldCondition> join)
            {
                var left = Translate(expression.Left);
                var right = Translate(expression.Right);
                return left == null || right == null ? null : Combine(left, right, join);
            }

            // !x.Flag reads as falsy, !string.IsNullOrEmpty(x.Name) as not empty, and !set.Contains(x.Code)
            // as not in; any other negation keeps its operand's form.
            private ClientCondition? Negate(Expression operand)
            {
                if (IsField(operand))
                    return Compare(FieldOf(operand), CompareOperator.Falsy, FieldComparisonValue.None);

                if (operand is MethodCallExpression call)
                {
                    if (IsStringMethod(call, nameof(string.IsNullOrEmpty)) && IsField(call.Arguments[0]))
                        return Compare(FieldOf(call.Arguments[0]), CompareOperator.NotEmpty, FieldComparisonValue.None);

                    if (TryMembership(call, out var field, out var values))
                        return Compare(field, CompareOperator.NotIn, FieldComparisonValue.Array(values));
                }

                return Translate(operand)?.Negated();
            }

            private ClientCondition? Comparison(BinaryExpression expression)
            {
                var fieldOnLeft = IsField(expression.Left) && IsFixed(expression.Right);
                var fieldOnRight = IsField(expression.Right) && IsFixed(expression.Left);
                if (!fieldOnLeft && !fieldOnRight) return null;

                var member = fieldOnLeft ? expression.Left : expression.Right;
                var field = FieldOf(member);
                var value = Literal(member, fieldOnLeft ? expression.Right : expression.Left);
                var op = fieldOnLeft ? expression.NodeType : Mirrored(expression.NodeType);

                if (value == null)
                {
                    if (op == ExpressionType.Equal) return Compare(field, CompareOperator.IsNull, FieldComparisonValue.None);
                    if (op == ExpressionType.NotEqual) return Compare(field, CompareOperator.NotNull, FieldComparisonValue.None);
                    return null;
                }

                return Compare(field, OperatorOf(op), FieldComparisonValue.Literal(value));
            }

            private ClientCondition? Call(MethodCallExpression call)
            {
                if (IsStringMethod(call, nameof(string.IsNullOrEmpty)) && IsField(call.Arguments[0]))
                    return Compare(FieldOf(call.Arguments[0]), CompareOperator.IsEmpty, FieldComparisonValue.None);

                if (TryText(call, out var textField, out var op, out var operand))
                    return Compare(textField, op, FieldComparisonValue.Text(operand));

                if (TryMembership(call, out var field, out var values))
                    return Compare(field, CompareOperator.In, FieldComparisonValue.Array(values));

                if (TryCollectionContains(call, out var collection, out var item, out var itemShape))
                    return Compare(collection, CompareOperator.ArrayContains, FieldComparisonValue.CollectionItem(item, itemShape));

                return null;
            }

            private ClientCondition Truthy(Expression member) =>
                Compare(FieldOf(member), CompareOperator.Truthy, FieldComparisonValue.None);

            // x.Name.Contains("a"), x.Name.StartsWith("a"), x.Name.EndsWith("a"), Regex.IsMatch(x.Name, "a").
            private bool TryText(MethodCallExpression call, out ClientValidationFieldReference field, out CompareOperator op, out string operand)
            {
                field = null!;
                op = null!;
                operand = "";

                var isRegex = call.Method.DeclaringType == typeof(Regex) && call.Method.Name == nameof(Regex.IsMatch)
                    && call.Object == null && call.Arguments.Count == 2;
                if (isRegex && IsField(call.Arguments[0]) && IsFixed(call.Arguments[1]))
                {
                    field = FieldOf(call.Arguments[0]);
                    op = CompareOperator.Matches;
                    operand = (string)Evaluate(call.Arguments[1])!;
                    return true;
                }

                var isTextMethod = call.Method.DeclaringType == typeof(string) && call.Object != null
                    && call.Arguments.Count == 1 && call.Arguments[0].Type == typeof(string);
                if (!isTextMethod || !IsField(call.Object!) || !IsFixed(call.Arguments[0])) return false;

                switch (call.Method.Name)
                {
                    case nameof(string.Contains): op = CompareOperator.Contains; break;
                    case nameof(string.StartsWith): op = CompareOperator.StartsWith; break;
                    case nameof(string.EndsWith): op = CompareOperator.EndsWith; break;
                    default: return false;
                }

                field = FieldOf(call.Object!);
                operand = (string)Evaluate(call.Arguments[0])!;
                return true;
            }

            // values.Contains(x.Code): the field is one of a fixed set of values.
            private bool TryMembership(MethodCallExpression call, out ClientValidationFieldReference field, out IReadOnlyList<object?> values)
            {
                field = null!;
                values = Array.Empty<object?>();
                if (!TryContains(call, out var source, out var candidate)) return false;
                if (!IsFixed(source) || !IsField(candidate)) return false;

                field = FieldOf(candidate);
                var member = candidate;
                values = ((IEnumerable)Evaluate(source)!).Cast<object?>().Select(value => LiteralOf(member, value)).ToArray();
                return true;
            }

            // x.Tags.Contains("fall-risk"): a collection field holds a fixed item.
            private bool TryCollectionContains(
                MethodCallExpression call,
                out ClientValidationFieldReference collection,
                out object? item,
                out Shape itemShape)
            {
                collection = null!;
                item = null;
                itemShape = Shape.None;
                if (!TryContains(call, out var source, out var candidate)) return false;
                if (!IsField(source) || !IsFixed(candidate)) return false;

                collection = FieldOf(source);
                item = LiteralOf(candidate, Evaluate(candidate));
                itemShape = Shape.FromClrType(StripConvert(candidate).Type);
                return true;
            }

            // Enumerable.Contains(source, item), MemoryExtensions.Contains(span, item) (C# 14 binds an
            // array's Contains to a span), or an instance Contains on a collection (List<T>, a set).
            private static bool TryContains(MethodCallExpression call, out Expression source, out Expression item)
            {
                source = null!;
                item = null!;
                if (call.Method.Name != "Contains") return false;

                var isStaticContains = call.Object == null && call.Arguments.Count == 2
                    && (call.Method.DeclaringType == typeof(Enumerable) || call.Method.DeclaringType?.FullName == "System.MemoryExtensions");
                if (isStaticContains)
                {
                    source = WithoutSpanConversion(call.Arguments[0]);
                    item = call.Arguments[1];
                    return true;
                }

                var isCollectionContains = call.Object != null && call.Arguments.Count == 1
                    && call.Object.Type != typeof(string)
                    && typeof(IEnumerable).IsAssignableFrom(call.Object.Type);
                if (!isCollectionContains) return false;

                source = call.Object!;
                item = call.Arguments[0];
                return true;
            }

            // The array or collection under an implicit conversion to a span.
            private static Expression WithoutSpanConversion(Expression expression)
            {
                if (expression is MethodCallExpression conversion && conversion.Method.Name == "op_Implicit" && conversion.Arguments.Count == 1)
                    return conversion.Arguments[0];
                return StripConvert(expression);
            }

            private static bool IsStringMethod(MethodCallExpression call, string name) =>
                call.Method.DeclaringType == typeof(string) && call.Method.Name == name
                && call.Object == null && call.Arguments.Count == 1;

            // A member chain rooted at the model: x.Name, x.Address.City, x.Score.Value. A member of a
            // value .NET defines (x.Name.Length, x.Tags.Count, x.Date.Year) is not one the page binds.
            private bool IsField(Expression expression)
            {
                var current = ModelMember(expression);
                if (!(current is MemberExpression)) return false;
                while (current is MemberExpression member && member.Expression != null)
                {
                    if (IsValueMember(member.Member)) return false;
                    current = StripConvert(member.Expression);
                }
                return current == _model;
            }

            private static bool IsValueMember(MemberInfo member) =>
                member.DeclaringType?.Namespace?.StartsWith("System", StringComparison.Ordinal) == true;

            // x.Score.HasValue: the nullable member has a value.
            private bool IsHasValue(Expression expression, out Expression nullable)
            {
                nullable = null!;
                if (!(expression is MemberExpression member) || member.Expression == null) return false;
                var isHasValue = member.Member.Name == nameof(Nullable<int>.HasValue)
                    && Nullable.GetUnderlyingType(member.Expression.Type) != null;
                nullable = member.Expression;
                return isHasValue && IsField(nullable);
            }

            // x.Score.Value reads x.Score: the browser's value is the number or nothing.
            private static Expression ModelMember(Expression expression)
            {
                var current = StripConvert(expression);
                return current is MemberExpression member
                       && member.Expression != null
                       && member.Member.Name == nameof(Nullable<int>.Value)
                       && Nullable.GetUnderlyingType(member.Expression.Type) != null
                    ? StripConvert(member.Expression)
                    : current;
            }

            // An expression that reads nothing of the model: a constant, or a captured variable.
            private bool IsFixed(Expression expression) => !new ModelReads(_model).Find(expression);

            private ClientValidationFieldReference FieldOf(Expression member)
            {
                var chain = ModelMember(member);
                return ClientValidationFieldReference.Of(
                    ValidationFieldPath.Of(ExpressionPathHelper.ToMemberName(chain)),
                    Shape.FromClrType(chain.Type));
            }

            // The value a comparison fixes, as the member's own type: an enum compared through its number
            // is its member again, so the browser compares names, which is how MVC binds an enum.
            private static object? Literal(Expression member, Expression fixedSide) =>
                LiteralOf(member, Evaluate(fixedSide));

            private static object? LiteralOf(Expression member, object? value)
            {
                if (value == null) return null;

                var memberType = StripConvert(member).Type;
                var enumType = Nullable.GetUnderlyingType(memberType) ?? memberType;
                var asMemberType = enumType.IsEnum && !value.GetType().IsEnum ? Enum.ToObject(enumType, value) : value;
                var shape = Shape.FromClrType(asMemberType.GetType());
                return asMemberType.GetType().IsEnum
                    ? asMemberType.ToString()
                    : ValidationDateLiteral.From(asMemberType, shape);
            }

            private static object? Evaluate(Expression expression) =>
                Expression.Lambda(expression).Compile().DynamicInvoke();

            private static Expression StripConvert(Expression expression)
            {
                while (expression is UnaryExpression unary
                       && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                    expression = unary.Operand;
                return expression;
            }

            private static ExpressionType Mirrored(ExpressionType op) => op switch
            {
                ExpressionType.GreaterThan => ExpressionType.LessThan,
                ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
                ExpressionType.LessThan => ExpressionType.GreaterThan,
                ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
                _ => op,
            };

            private static CompareOperator OperatorOf(ExpressionType op) => op switch
            {
                ExpressionType.Equal => CompareOperator.Eq,
                ExpressionType.NotEqual => CompareOperator.Neq,
                ExpressionType.GreaterThan => CompareOperator.Gt,
                ExpressionType.GreaterThanOrEqual => CompareOperator.Gte,
                ExpressionType.LessThan => CompareOperator.Lt,
                _ => CompareOperator.Lte,
            };
        }

        // Whether an expression reads the model parameter anywhere.
        private sealed class ModelReads : ExpressionVisitor
        {
            private readonly ParameterExpression _model;
            private bool _found;

            internal ModelReads(ParameterExpression model)
            {
                _model = model;
            }

            internal bool Find(Expression expression)
            {
                Visit(expression);
                return _found;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == _model) _found = true;
                return node;
            }
        }
    }
}
