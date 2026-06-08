using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// One client validation rule a developer records. Lowers to a plan
    /// <see cref="ValidationRuleNode"/> at render time.
    /// </summary>
    internal sealed class ClientRule
    {
        private readonly RuleName _rule;
        private readonly ValidationMessage _message;
        private readonly RuleOperand _operand;
        private readonly ClientRuleActivation _activation;
        private readonly Shape _shape;

        public string Message => _message.Value;
        public Shape Shape => _shape;

        // Projections for a vendor emitter that translates this rule into a native
        // component validation shape (e.g. Syncfusion EJ2 column.validationRules).
        internal RuleName Name => _rule;
        internal bool IsUnconditional => _activation.IsAlways;
        internal object? LiteralOperand => _operand.LiteralValue;
        internal object[]? RangeOperand => _operand.RangeValues;

        internal ClientRule(
            RuleName rule,
            ValidationMessage message,
            RuleOperand operand,
            ClientRuleActivation activation,
            Shape shape)
        {
            _rule = rule;
            _message = message;
            _operand = operand;
            _activation = activation;
            _shape = shape;
        }

        internal ValidationRuleNode ToPlanRule(ValidationPlanBinding binding)
        {
            return new ValidationRuleNode(
                _rule,
                _message,
                _operand.ToPlanExecution(
                    _activation.ToPlanActivation(binding),
                    binding,
                    _shape));
        }

        internal ClientRule PrefixedBy(
            ValidationFieldPath prefix,
            ClientRuleActivation parentActivation)
        {
            return new ClientRule(
                _rule,
                _message,
                _operand.PrefixedBy(prefix),
                parentActivation.Combine(_activation.PrefixedBy(prefix)),
                _shape);
        }

        internal IEnumerable<ClientValidationFieldReference> PeerFieldReferences =>
            _operand.PeerFieldReferences;
    }
}
