using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// The browser's form of a <c>When</c> condition. A C# condition has a browser form when every part of it
    /// is a test of a model member against a value the condition fixes, joined by <c>&amp;&amp;</c>,
    /// <c>||</c> and <c>!</c>.
    /// </summary>
    internal static class BrowserCondition
    {
        /// <summary>The condition's browser form, or null when part of it has none; the condition then says so where it is described.</summary>
        internal static Condition? From(LambdaExpression predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new Translator(predicate.Parameters[0]).Translate(predicate.Body);
        }

        /// <summary>Every condition of a nested <c>When</c>, outermost first.</summary>
        internal static Condition AllOf(IReadOnlyList<Condition> conditions) =>
            conditions.Count == 1 ? conditions[0] : Condition.All(conditions.ToArray());

        private sealed class Translator
        {
            private readonly ParameterExpression _model;

            internal Translator(ParameterExpression model)
            {
                _model = model;
            }

            internal Condition? Translate(Expression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.AndAlso:
                        return Join((BinaryExpression)expression, (l, r) => Condition.All(l, r));
                    case ExpressionType.OrElse:
                        return Join((BinaryExpression)expression, (l, r) => Condition.Any(l, r));
                    case ExpressionType.Not:
                        return Negate(((UnaryExpression)expression).Operand);
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        return Compared((BinaryExpression)expression);
                    case ExpressionType.Call:
                        return Call((MethodCallExpression)expression);
                    default:
                        if (IsHasValue(expression, out var nullable))
                            return Is(nullable, ValueTest.NotNull);
                        return IsField(expression) ? Is(expression, ValueTest.Truthy) : null;
                }
            }

            private Condition? Join(BinaryExpression expression, Func<Condition, Condition, Condition> join)
            {
                var left = Translate(expression.Left);
                var right = Translate(expression.Right);
                return left == null || right == null ? null : join(left, right);
            }

            // !x.Flag reads as falsy, !string.IsNullOrEmpty(x.Name) as not empty, and !set.Contains(x.Code)
            // as not in; any other negation keeps its operand's form.
            private Condition? Negate(Expression operand)
            {
                if (IsField(operand))
                    return Is(operand, ValueTest.Falsy);

                if (operand is MethodCallExpression call)
                {
                    if (IsStringMethod(call, nameof(string.IsNullOrEmpty)) && IsField(call.Arguments[0]))
                        return Is(call.Arguments[0], ValueTest.NotEmpty);

                    if (TryMembership(call, out var member, out var values))
                        return Condition.Not(In(member, values));
                }

                var translated = Translate(operand);
                return translated == null ? null : Condition.Not(translated);
            }

            private Condition? Compared(BinaryExpression expression)
            {
                var fieldOnLeft = IsField(expression.Left) && IsFixed(expression.Right);
                var fieldOnRight = IsField(expression.Right) && IsFixed(expression.Left);
                if (!fieldOnLeft && !fieldOnRight) return null;

                var member = fieldOnLeft ? expression.Left : expression.Right;
                var operand = fieldOnLeft ? expression.Right : expression.Left;
                var value = Expressions.Evaluate(operand);
                var op = fieldOnLeft ? expression.NodeType : Mirrored(expression.NodeType);

                if (value == null)
                {
                    if (op == ExpressionType.Equal) return Is(member, ValueTest.IsNull);
                    if (op == ExpressionType.NotEqual) return Is(member, ValueTest.NotNull);
                    return null;
                }

                return Condition.Compare(Lambda(member), ComparisonOf(op), operand);
            }

            private Condition? Call(MethodCallExpression call)
            {
                if (IsStringMethod(call, nameof(string.IsNullOrEmpty)) && IsField(call.Arguments[0]))
                    return Is(call.Arguments[0], ValueTest.IsEmpty);

                if (TryText(call, out var textField, out var test, out var text))
                    return Condition.Text(Lambda(textField), test, text);

                if (TryMembership(call, out var member, out var values))
                    return In(member, values);

                if (TryCollectionContains(call, out var collection, out var item))
                    return Condition.Holds(Lambda(collection), item);

                return null;
            }

            private Condition Is(Expression member, ValueTest test) =>
                Condition.Is(Lambda(member), test);

            private Condition In(Expression member, Expression values) =>
                Condition.In(Lambda(member), values);

            // The member as a path from the model: x.Score.Value reads x.Score, whose browser value is the number or nothing.
            private LambdaExpression Lambda(Expression member) => Expression.Lambda(ModelMember(member), _model);

            // x.Name.Contains("a"), x.Name.StartsWith("a"), x.Name.EndsWith("a"), Regex.IsMatch(x.Name, "a").
            private bool TryText(MethodCallExpression call, out Expression field, out TextTest test, out string operand)
            {
                field = null!;
                test = TextTest.Contains;
                operand = "";

                var isRegex = call.Method.DeclaringType == typeof(Regex) && call.Method.Name == nameof(Regex.IsMatch)
                    && call.Object == null && call.Arguments.Count == 2;
                if (isRegex && IsField(call.Arguments[0]) && IsFixed(call.Arguments[1]))
                {
                    field = call.Arguments[0];
                    test = TextTest.Matches;
                    operand = (string)Expressions.Evaluate(call.Arguments[1])!;
                    return true;
                }

                var isTextMethod = call.Method.DeclaringType == typeof(string) && call.Object != null
                    && call.Arguments.Count == 1 && call.Arguments[0].Type == typeof(string);
                if (!isTextMethod || !IsField(call.Object!) || !IsFixed(call.Arguments[0])) return false;

                switch (call.Method.Name)
                {
                    case nameof(string.Contains): test = TextTest.Contains; break;
                    case nameof(string.StartsWith): test = TextTest.StartsWith; break;
                    case nameof(string.EndsWith): test = TextTest.EndsWith; break;
                    default: return false;
                }

                field = call.Object!;
                operand = (string)Expressions.Evaluate(call.Arguments[0])!;
                return true;
            }

            // values.Contains(x.Code): the field is one of a fixed set of values.
            private bool TryMembership(MethodCallExpression call, out Expression field, out Expression values)
            {
                field = null!;
                values = null!;
                if (!Expressions.TryContains(call, out var source, out var candidate)) return false;
                if (!IsFixed(source) || !IsField(candidate)) return false;

                field = candidate;
                values = source;
                return true;
            }

            // x.Tags.Contains("fall-risk"): a collection field holds a fixed item.
            private bool TryCollectionContains(
                MethodCallExpression call,
                out Expression collection,
                out Expression item)
            {
                collection = null!;
                item = null!;
                if (!Expressions.TryContains(call, out var source, out var candidate)) return false;
                if (!IsField(source) || !IsFixed(candidate)) return false;

                collection = source;
                item = candidate;
                return true;
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
                    if (Expressions.IsValueMember(member.Member)) return false;
                    current = Expressions.StripConvert(member.Expression);
                }
                return current == _model;
            }

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
                var current = Expressions.StripConvert(expression);
                return current is MemberExpression member
                       && member.Expression != null
                       && member.Member.Name == nameof(Nullable<int>.Value)
                       && Nullable.GetUnderlyingType(member.Expression.Type) != null
                    ? Expressions.StripConvert(member.Expression)
                    : current;
            }

            // An expression that reads nothing of the model: a constant, or a captured variable.
            private bool IsFixed(Expression expression) => !Expressions.Reads(expression, _model);

            private static ExpressionType Mirrored(ExpressionType op) => op switch
            {
                ExpressionType.GreaterThan => ExpressionType.LessThan,
                ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
                ExpressionType.LessThan => ExpressionType.GreaterThan,
                ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
                _ => op,
            };

            private static Comparison ComparisonOf(ExpressionType op) => op switch
            {
                ExpressionType.Equal => Comparison.Equal,
                ExpressionType.NotEqual => Comparison.NotEqual,
                ExpressionType.GreaterThan => Comparison.GreaterThan,
                ExpressionType.GreaterThanOrEqual => Comparison.GreaterThanOrEqual,
                ExpressionType.LessThan => Comparison.LessThan,
                _ => Comparison.LessThanOrEqual,
            };
        }
    }
}
