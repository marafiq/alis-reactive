using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
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
        internal static ClientCondition? From(LambdaExpression predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new Translator(predicate.Parameters[0]).Translate(predicate.Body);
        }

        /// <summary>Every condition of a nested <c>When</c>, outermost first.</summary>
        internal static ClientCondition AllOf(IReadOnlyList<ClientCondition> conditions) =>
            conditions.Count == 1 ? conditions[0] : ClientCondition.All(conditions.ToArray());

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
                        return Join((BinaryExpression)expression, (l, r) => ClientCondition.All(l, r));
                    case ExpressionType.OrElse:
                        return Join((BinaryExpression)expression, (l, r) => ClientCondition.Any(l, r));
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
                            return Is(nullable, ClientValueTest.NotNull);
                        return IsField(expression) ? Is(expression, ClientValueTest.Truthy) : null;
                }
            }

            private ClientCondition? Join(BinaryExpression expression, Func<ClientCondition, ClientCondition, ClientCondition> join)
            {
                var left = Translate(expression.Left);
                var right = Translate(expression.Right);
                return left == null || right == null ? null : join(left, right);
            }

            // !x.Flag reads as falsy, !string.IsNullOrEmpty(x.Name) as not empty, and !set.Contains(x.Code)
            // as not in; any other negation keeps its operand's form.
            private ClientCondition? Negate(Expression operand)
            {
                if (IsField(operand))
                    return Is(operand, ClientValueTest.Falsy);

                if (operand is MethodCallExpression call)
                {
                    if (IsStringMethod(call, nameof(string.IsNullOrEmpty)) && IsField(call.Arguments[0]))
                        return Is(call.Arguments[0], ClientValueTest.NotEmpty);

                    if (TryMembership(call, out var member, out var values))
                        return ClientCondition.Not(In(member, values));
                }

                var translated = Translate(operand);
                return translated == null ? null : ClientCondition.Not(translated);
            }

            private ClientCondition? Comparison(BinaryExpression expression)
            {
                var fieldOnLeft = IsField(expression.Left) && IsFixed(expression.Right);
                var fieldOnRight = IsField(expression.Right) && IsFixed(expression.Left);
                if (!fieldOnLeft && !fieldOnRight) return null;

                var member = fieldOnLeft ? expression.Left : expression.Right;
                var operand = fieldOnLeft ? expression.Right : expression.Left;
                var value = Evaluate(operand);
                var op = fieldOnLeft ? expression.NodeType : Mirrored(expression.NodeType);

                if (value == null)
                {
                    if (op == ExpressionType.Equal) return Is(member, ClientValueTest.Null);
                    if (op == ExpressionType.NotEqual) return Is(member, ClientValueTest.NotNull);
                    return null;
                }

                return ClientCondition.Compare(Lambda(member), ComparisonOf(op), operand);
            }

            private ClientCondition? Call(MethodCallExpression call)
            {
                if (IsStringMethod(call, nameof(string.IsNullOrEmpty)) && IsField(call.Arguments[0]))
                    return Is(call.Arguments[0], ClientValueTest.Empty);

                if (TryText(call, out var textField, out var test, out var text))
                    return ClientCondition.Text(Lambda(textField), test, text);

                if (TryMembership(call, out var member, out var values))
                    return In(member, values);

                if (TryCollectionContains(call, out var collection, out var item))
                    return ClientCondition.Holds(Lambda(collection), item);

                return null;
            }

            private ClientCondition Is(Expression member, ClientValueTest test) =>
                ClientCondition.Is(Lambda(member), test);

            private ClientCondition In(Expression member, Expression values) =>
                ClientCondition.In(Lambda(member), values);

            // The member as a path from the model: x.Score.Value reads x.Score, whose browser value is the number or nothing.
            private LambdaExpression Lambda(Expression member) => Expression.Lambda(ModelMember(member), _model);

            // x.Name.Contains("a"), x.Name.StartsWith("a"), x.Name.EndsWith("a"), Regex.IsMatch(x.Name, "a").
            private bool TryText(MethodCallExpression call, out Expression field, out ClientTextTest test, out string operand)
            {
                field = null!;
                test = ClientTextTest.Contains;
                operand = "";

                var isRegex = call.Method.DeclaringType == typeof(Regex) && call.Method.Name == nameof(Regex.IsMatch)
                    && call.Object == null && call.Arguments.Count == 2;
                if (isRegex && IsField(call.Arguments[0]) && IsFixed(call.Arguments[1]))
                {
                    field = call.Arguments[0];
                    test = ClientTextTest.Matches;
                    operand = (string)Evaluate(call.Arguments[1])!;
                    return true;
                }

                var isTextMethod = call.Method.DeclaringType == typeof(string) && call.Object != null
                    && call.Arguments.Count == 1 && call.Arguments[0].Type == typeof(string);
                if (!isTextMethod || !IsField(call.Object!) || !IsFixed(call.Arguments[0])) return false;

                switch (call.Method.Name)
                {
                    case nameof(string.Contains): test = ClientTextTest.Contains; break;
                    case nameof(string.StartsWith): test = ClientTextTest.StartsWith; break;
                    case nameof(string.EndsWith): test = ClientTextTest.EndsWith; break;
                    default: return false;
                }

                field = call.Object!;
                operand = (string)Evaluate(call.Arguments[0])!;
                return true;
            }

            // values.Contains(x.Code): the field is one of a fixed set of values.
            private bool TryMembership(MethodCallExpression call, out Expression field, out Expression values)
            {
                field = null!;
                values = null!;
                if (!TryContains(call, out var source, out var candidate)) return false;
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
                if (!TryContains(call, out var source, out var candidate)) return false;
                if (!IsField(source) || !IsFixed(candidate)) return false;

                collection = source;
                item = candidate;
                return true;
            }

            // Enumerable.Contains(source, item), MemoryExtensions.Contains(span, item) or (span, item, null)
            // (C# 14 binds an array's Contains to a span, through the comparer overload for an item type that
            // is not IEquatable), or an instance Contains on a collection (List<T>, a set).
            private static bool TryContains(MethodCallExpression call, out Expression source, out Expression item)
            {
                source = null!;
                item = null!;
                if (call.Method.Name != "Contains") return false;

                var isStaticContains = call.Object == null
                    && (call.Arguments.Count == 2 || (call.Arguments.Count == 3 && IsNullComparer(call.Arguments[2])))
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

            // The default comparer, which C# passes as null when the overload takes one.
            private static bool IsNullComparer(Expression comparer) =>
                StripConvert(comparer) is ConstantExpression constant && constant.Value == null;

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

            private static ClientComparison ComparisonOf(ExpressionType op) => op switch
            {
                ExpressionType.Equal => ClientComparison.Equal,
                ExpressionType.NotEqual => ClientComparison.NotEqual,
                ExpressionType.GreaterThan => ClientComparison.GreaterThan,
                ExpressionType.GreaterThanOrEqual => ClientComparison.GreaterThanOrEqual,
                ExpressionType.LessThan => ClientComparison.LessThan,
                _ => ClientComparison.LessThanOrEqual,
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
