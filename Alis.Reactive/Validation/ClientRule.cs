using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// One client validation rule a developer records. Lowers to a plan
    /// <see cref="ValidationRuleNode"/> at render time.
    /// </summary>
    /// <remarks>
    /// Read-only to code outside the framework: a component whose own JavaScript validates (a grid's
    /// cell editor, say) translates these rules into its native shape.
    /// </remarks>
    public sealed class ClientRule
    {
        private readonly RuleName _rule;
        private readonly ValidationMessage _message;
        private readonly RuleOperand _operand;
        private readonly ClientRuleActivation _activation;
        private readonly Shape _shape;

        /// <summary>The message shown when the rule fails.</summary>
        public string Message => _message.Value;

        internal Shape Shape => _shape;

        /// <summary>The rule's name in the plan, for example <c>"required"</c>, <c>"minLength"</c> or <c>"range"</c>.</summary>
        public string Name => _rule.Value;

        /// <summary>Whether the rule always applies, rather than only when a condition holds.</summary>
        public bool IsUnconditional => _activation.IsAlways;

        /// <summary>The rule's single operand, such as a length or a bound; <see langword="null"/> when it has none.</summary>
        public object? LiteralOperand => _operand.LiteralValue;

        /// <summary>The rule's two bounds for a range; <see langword="null"/> for any other rule.</summary>
        public object[]? RangeOperand => _operand.RangeValues;

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
