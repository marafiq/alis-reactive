using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Builds one model's deterministic browser validation projection.
    /// </summary>
    public sealed class ClientValidationProjectionBuilder<TModel>
        where TModel : class
    {
        private readonly ClientValidationProjectionDraft _projection = new ClientValidationProjectionDraft();
        private ValidationRuleActivation _activeActivation = ValidationRuleActivation.Always;

        internal ClientValidationProjectionBuilder() { }

        public ClientValidationFieldRuleBuilder<TModel, TValue> Field<TValue>(
            Expression<Func<TModel, TValue>> field) =>
            Field(ClientValidationFieldToken<TModel, TValue>.For(field));

        public ClientValidationFieldRuleBuilder<TModel, TValue> Field<TValue>(
            ClientValidationFieldToken<TModel, TValue> field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            _projection.EnsureField(field.Reference);
            return new ClientValidationFieldRuleBuilder<TModel, TValue>(
                _projection,
                field,
                _activeActivation);
        }

        public void When(
            Func<ClientValidationConditionBuilder<TModel>, ClientValidationCondition<TModel>> condition,
            Action<ClientValidationProjectionBuilder<TModel>> define)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (define == null) throw new ArgumentNullException(nameof(define));

            var projectedCondition = condition(new ClientValidationConditionBuilder<TModel>());
            if (projectedCondition == null)
                throw new ArgumentException("A client validation condition is required.", nameof(condition));

            _projection.EnsureFields(projectedCondition.Fields);

            var previousActivation = _activeActivation;
            _activeActivation = previousActivation.Combine(ValidationRuleActivation.When(projectedCondition.Condition));
            try
            {
                define(this);
            }
            finally
            {
                _activeActivation = previousActivation;
            }
        }

        internal IReadOnlyList<ClientValidationField> ToFields() =>
            _projection.ToFields();
    }
}
