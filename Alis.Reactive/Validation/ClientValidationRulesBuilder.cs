using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Builds one model's deterministic browser validation rules.
    /// </summary>
    public sealed class ClientValidationRulesBuilder<TModel>
        where TModel : class
    {
        private readonly ClientValidationRuleSet _rules = new ClientValidationRuleSet();
        private ClientRuleActivation _activeActivation = ClientRuleActivation.Always;

        internal ClientValidationRulesBuilder() { }

        public ClientValidationFieldRuleBuilder<TModel, TValue> Field<TValue>(
            Expression<Func<TModel, TValue>> field) =>
            Field(ClientValidationFieldToken<TModel, TValue>.For(field));

        public ClientValidationFieldRuleBuilder<TModel, TValue> Field<TValue>(
            ClientValidationFieldToken<TModel, TValue> field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            _rules.EnsureField(field.Reference);
            return new ClientValidationFieldRuleBuilder<TModel, TValue>(
                _rules,
                field,
                _activeActivation);
        }

        public void When(
            Func<ClientValidationConditionBuilder<TModel>, ClientValidationCondition<TModel>> condition,
            Action<ClientValidationRulesBuilder<TModel>> define)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (define == null) throw new ArgumentNullException(nameof(define));

            var activeCondition = condition(new ClientValidationConditionBuilder<TModel>());
            if (activeCondition == null)
                throw new ArgumentException("A client validation condition is required.", nameof(condition));

            _rules.EnsureFields(activeCondition.Fields);

            var previousActivation = _activeActivation;
            _activeActivation = previousActivation.Combine(ClientRuleActivation.When(activeCondition.Condition));
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
            _rules.ToFields();
    }
}
