using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Base class for validators that need client-side conditional rules.
    /// Use WhenField() instead of FV's .When() to get both server + client validation.
    /// FV's .When() still works for server-only conditions (DB, service calls).
    /// </summary>
    public abstract class ReactiveValidator<T> : AbstractValidator<T>, IClientConditionSource
        where T : class
    {
        private readonly Dictionary<IValidationRule, FieldCondition> _clientConditions =
            new Dictionary<IValidationRule, FieldCondition>();

        IReadOnlyDictionary<IValidationRule, FieldCondition> IClientConditionSource.ClientConditions =>
            _clientConditions;

        // ── Existing operators (unchanged signatures) ──────────────────────────

        /// <summary>
        /// Applies a "truthy" condition to all rules defined in the block.
        /// Server: FV's When() runs the condition at validation time.
        /// Client: Adapter extracts rules with FieldCondition.Compare(field, "truthy").
        /// </summary>
        protected void WhenField(Expression<Func<T, bool>> conditionField, Action defineRules)
        {
            var fieldName = ExtractPropertyName(conditionField);
            var compiled = conditionField.Compile();
            var condition = FieldCondition.Compare(fieldName, "truthy");

            ApplyClientCondition(compiled, condition, defineRules);
        }

        /// <summary>
        /// Applies an "eq" condition to all rules defined in the block.
        /// Server: FV's When() checks field == value at validation time.
        /// Client: Adapter extracts rules with FieldCondition.Compare(field, "eq", value).
        /// </summary>
        protected void WhenField<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "eq", SerializeConditionValue(value));

            ApplyClientCondition(
                x => Equals(fieldFunc(x), value),
                condition,
                defineRules);
        }

        /// <summary>
        /// Applies a "falsy" condition to all rules defined in the block.
        /// Server: FV's When() runs !condition at validation time.
        /// Client: Adapter extracts rules with FieldCondition.Compare(field, "falsy").
        /// </summary>
        protected void WhenFieldNot(Expression<Func<T, bool>> conditionField, Action defineRules)
        {
            var fieldName = ExtractPropertyName(conditionField);
            var compiled = conditionField.Compile();
            var condition = FieldCondition.Compare(fieldName, "falsy");

            ApplyClientCondition(x => !compiled(x), condition, defineRules);
        }

        /// <summary>
        /// Applies a "neq" condition to all rules defined in the block.
        /// Server: FV's When() checks field != value at validation time.
        /// Client: Adapter extracts rules with FieldCondition.Compare(field, "neq", value).
        /// </summary>
        protected void WhenFieldNot<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "neq", SerializeConditionValue(value));

            ApplyClientCondition(
                x => !Equals(fieldFunc(x), value),
                condition,
                defineRules);
        }

        // ── Ordering operators ─────────────────────────────────────────────────

        /// <summary>Applies a "gt" (greater than) condition.</summary>
        protected void WhenFieldGt<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "gt", SerializeConditionValue(value));

            ApplyClientCondition(
                x => Comparer<TProp>.Default.Compare(fieldFunc(x), value) > 0,
                condition,
                defineRules);
        }

        /// <summary>Applies a "gte" (greater than or equal) condition.</summary>
        protected void WhenFieldGte<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "gte", SerializeConditionValue(value));

            ApplyClientCondition(
                x => Comparer<TProp>.Default.Compare(fieldFunc(x), value) >= 0,
                condition,
                defineRules);
        }

        /// <summary>Applies a "lt" (less than) condition.</summary>
        protected void WhenFieldLt<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "lt", SerializeConditionValue(value));

            ApplyClientCondition(
                x => Comparer<TProp>.Default.Compare(fieldFunc(x), value) < 0,
                condition,
                defineRules);
        }

        /// <summary>Applies a "lte" (less than or equal) condition.</summary>
        protected void WhenFieldLte<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "lte", SerializeConditionValue(value));

            ApplyClientCondition(
                x => Comparer<TProp>.Default.Compare(fieldFunc(x), value) <= 0,
                condition,
                defineRules);
        }

        // ── Presence operators ─────────────────────────────────────────────────

        /// <summary>Applies an "is-null" condition.</summary>
        protected void WhenFieldNull<TProp>(
            Expression<Func<T, TProp>> field, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "is-null");

            ApplyClientCondition(x => fieldFunc(x) == null, condition, defineRules);
        }

        /// <summary>Applies a "not-null" condition.</summary>
        protected void WhenFieldNotNull<TProp>(
            Expression<Func<T, TProp>> field, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "not-null");

            ApplyClientCondition(x => fieldFunc(x) != null, condition, defineRules);
        }

        /// <summary>Applies an "is-empty" condition (null or empty string).</summary>
        protected void WhenFieldEmpty(
            Expression<Func<T, string>> field, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "is-empty");

            ApplyClientCondition(x => string.IsNullOrEmpty(fieldFunc(x)), condition, defineRules);
        }

        /// <summary>Applies a "not-empty" condition (non-null and non-empty string).</summary>
        protected void WhenFieldNotEmpty(
            Expression<Func<T, string>> field, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "not-empty");

            ApplyClientCondition(x => !string.IsNullOrEmpty(fieldFunc(x)), condition, defineRules);
        }

        // ── Membership operators ───────────────────────────────────────────────

        /// <summary>Applies an "in" condition — field value is in the given set.</summary>
        protected void WhenFieldIn<TProp>(
            Expression<Func<T, TProp>> field, TProp[] values, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var serialized = values.Select(v => SerializeConditionValue(v)).ToArray();
            var condition = FieldCondition.Compare(fieldName, "in", serialized);

            var set = new HashSet<TProp>(values);
            ApplyClientCondition(x => set.Contains(fieldFunc(x)), condition, defineRules);
        }

        // ── Text operators ─────────────────────────────────────────────────────

        /// <summary>Applies a "contains" condition — string field contains substring.</summary>
        protected void WhenFieldContains(
            Expression<Func<T, string>> field, string substring, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "contains", substring);

            ApplyClientCondition(
                x => fieldFunc(x)?.Contains(substring) == true,
                condition,
                defineRules);
        }

        /// <summary>Applies a "starts-with" condition.</summary>
        protected void WhenFieldStartsWith(
            Expression<Func<T, string>> field, string prefix, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "starts-with", prefix);

            ApplyClientCondition(
                x => fieldFunc(x)?.StartsWith(prefix) == true,
                condition,
                defineRules);
        }

        /// <summary>Applies an "ends-with" condition.</summary>
        protected void WhenFieldEndsWith(
            Expression<Func<T, string>> field, string suffix, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = FieldCondition.Compare(fieldName, "ends-with", suffix);

            ApplyClientCondition(
                x => fieldFunc(x)?.EndsWith(suffix) == true,
                condition,
                defineRules);
        }

        // ── Composition ────────────────────────────────────────────────────────

        /// <summary>
        /// Applies a composed condition (And/Or/Not) to all rules defined in the block.
        /// Server: combined predicate runs at validation time.
        /// Client: adapter extracts the FieldCondition tree.
        /// </summary>
        protected void WhenFields(
            Func<FieldConditionBuilder<T>, FieldGuard<T>> buildCondition,
            Action defineRules)
        {
            var builder = new FieldConditionBuilder<T>();
            var guard = buildCondition(builder);

            ApplyClientCondition(guard.ServerPredicate, guard.Condition, defineRules);
        }

        // ── Internals ──────────────────────────────────────────────────────────

        private void ApplyClientCondition(
            Func<T, bool> serverPredicate,
            FieldCondition clientCondition,
            Action defineRules)
        {
            var rulesBefore = ((IEnumerable<IValidationRule>)this).ToList();

            // FV's When() — defines rules AND applies condition for server validation
            When(serverPredicate, defineRules);

            // Find new rules added by the block
            var rulesAfter = ((IEnumerable<IValidationRule>)this).ToList();
            for (int i = rulesBefore.Count; i < rulesAfter.Count; i++)
            {
                _clientConditions[rulesAfter[i]] = clientCondition;
            }
        }

        /// <summary>
        /// Serializes a condition value for plan JSON.
        /// DateTime/DateTimeOffset/DateOnly -> Unix ms (long) via ToUnixTimeMilliseconds.
        /// All other types pass through as-is.
        /// Developer controls timezone by passing DateTime with the intended Kind.
        /// TimeSpan.Zero forces UTC interpretation for DateTime without explicit Kind.
        /// </summary>
        internal static object? SerializeConditionValue<TProp>(TProp value) => value switch
        {
            DateTime dt => new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
            DateTimeOffset dto => dto.ToUnixTimeMilliseconds(),
            DateOnly d => new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeMilliseconds(),
            _ => value
        };

        internal static string ExtractPropertyName<TResult>(Expression<Func<T, TResult>> expression)
        {
            var body = expression.Body;
            if (body is UnaryExpression unary)
                body = unary.Operand;

            if (body is MemberExpression member && member.Member is PropertyInfo)
                return member.Member.Name;

            throw new ArgumentException(
                $"WhenField() requires a simple property access expression (e.g. x => x.IsEmployed). " +
                $"Got: {expression}");
        }
    }
}
