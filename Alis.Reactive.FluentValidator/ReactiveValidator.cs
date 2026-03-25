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
        private readonly Dictionary<IValidationRule, ValidationCondition> _clientConditions =
            new Dictionary<IValidationRule, ValidationCondition>();

        IReadOnlyDictionary<IValidationRule, ValidationCondition> IClientConditionSource.ClientConditions =>
            _clientConditions;

        /// <summary>
        /// Applies a "truthy" condition to all rules defined in the block.
        /// Server: FV's When() runs the condition at validation time.
        /// Client: Adapter extracts rules with ValidationCondition(field, "truthy").
        /// </summary>
        protected void WhenField(Expression<Func<T, bool>> conditionField, Action defineRules)
        {
            var fieldName = ExtractPropertyName(conditionField);
            var compiled = conditionField.Compile();
            var condition = new ValidationCondition(fieldName, "truthy");

            ApplyClientCondition(compiled, condition, defineRules);
        }

        /// <summary>
        /// Applies an "eq" condition to all rules defined in the block.
        /// Server: FV's When() checks field == value at validation time.
        /// Client: Adapter extracts rules with ValidationCondition(field, "eq", value).
        /// </summary>
        protected void WhenField<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = new ValidationCondition(fieldName, "eq", SerializeConditionValue(value));

            ApplyClientCondition(
                x => Equals(fieldFunc(x), value),
                condition,
                defineRules);
        }

        /// <summary>
        /// Applies a "falsy" condition to all rules defined in the block.
        /// Server: FV's When() runs !condition at validation time.
        /// Client: Adapter extracts rules with ValidationCondition(field, "falsy").
        /// </summary>
        protected void WhenFieldNot(Expression<Func<T, bool>> conditionField, Action defineRules)
        {
            var fieldName = ExtractPropertyName(conditionField);
            var compiled = conditionField.Compile();
            var condition = new ValidationCondition(fieldName, "falsy");

            ApplyClientCondition(x => !compiled(x), condition, defineRules);
        }

        /// <summary>
        /// Applies a "neq" condition to all rules defined in the block.
        /// Server: FV's When() checks field != value at validation time.
        /// Client: Adapter extracts rules with ValidationCondition(field, "neq", value).
        /// </summary>
        protected void WhenFieldNot<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            var fieldName = ExtractPropertyName(field);
            var fieldFunc = field.Compile();
            var condition = new ValidationCondition(fieldName, "neq", SerializeConditionValue(value));

            ApplyClientCondition(
                x => !Equals(fieldFunc(x), value),
                condition,
                defineRules);
        }

        private void ApplyClientCondition(
            Func<T, bool> serverPredicate,
            ValidationCondition clientCondition,
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
        /// DateTime/DateTimeOffset/DateOnly → Unix ms (long) via ToUnixTimeMilliseconds.
        /// All other types pass through as-is.
        /// Developer controls timezone by passing DateTime with the intended Kind.
        /// TimeSpan.Zero forces UTC interpretation for DateTime without explicit Kind.
        /// </summary>
        private static object? SerializeConditionValue<TProp>(TProp value) => value switch
        {
            DateTime dt => new DateTimeOffset(dt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
            DateTimeOffset dto => dto.ToUnixTimeMilliseconds(),
            DateOnly d => new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeMilliseconds(),
            _ => value
        };

        private static string ExtractPropertyName<TResult>(Expression<Func<T, TResult>> expression)
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
