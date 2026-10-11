
namespace Alis.Reactive.PlanModel
{
    internal abstract class ValidationRuleExecution
    {
        private readonly ValidationRuleActivation _activation;

        private protected ValidationRuleExecution(
            ValidationRuleActivation activation,
            Shape comparisonShape)
        {
            _activation = activation ?? throw new System.ArgumentNullException(nameof(activation));
            ComparisonShape = comparisonShape ?? throw new System.ArgumentNullException(nameof(comparisonShape));
        }

        public ValidationRuleActivation Activation => _activation;
        public Shape ComparisonShape { get; }



        /// <summary>The literal the rule compares with, for a constraint rule.</summary>
        internal virtual bool TryGetConstraint(out LiteralExpression constraint)
        {
            constraint = null!;
            return false;
        }

        /// <summary>The other field's value the rule compares with, for a peer rule.</summary>
        internal virtual bool TryGetPeer(out ValueExpression peer)
        {
            peer = null!;
            return false;
        }

        internal static ValidationRuleExecution WithoutTarget(
            ValidationRuleActivation activation,
            Shape comparisonShape) =>
            new NoOperandValidationRuleExecution(
                activation,
                comparisonShape);

        internal static ValidationRuleExecution WithConstraint(
            ValueExpression constraint,
            ValidationRuleActivation activation,
            Shape comparisonShape)
        {
            if (constraint is not LiteralExpression literal)
                throw new System.ArgumentException("Validation rule constraints must be literal values.", nameof(constraint));

            return new ConstraintValidationRuleExecution(
                literal,
                activation,
                comparisonShape);
        }

        internal static ValidationRuleExecution WithPeer(
            ValueExpression peer,
            ValidationRuleActivation activation,
            Shape comparisonShape)
        {
            if (peer is not ReadExpression read)
                throw new System.ArgumentException("Validation rule peer values must read another field value.", nameof(peer));

            return new PeerValidationRuleExecution(
                read,
                activation,
                comparisonShape);
        }

        private sealed class NoOperandValidationRuleExecution : ValidationRuleExecution
        {

            internal NoOperandValidationRuleExecution(
                ValidationRuleActivation activation,
                Shape comparisonShape)
                : base(activation, comparisonShape)
            {
            }

        }

        private sealed class ConstraintValidationRuleExecution : ValidationRuleExecution
        {
            private readonly LiteralExpression _value;

            internal ConstraintValidationRuleExecution(
                LiteralExpression value,
                ValidationRuleActivation activation,
                Shape comparisonShape)
                : base(activation, comparisonShape)
            {
                _value = value ?? throw new System.ArgumentNullException(nameof(value));
            }

            public LiteralExpression Value => _value;

            internal override bool TryGetConstraint(out LiteralExpression constraint)
            {
                constraint = _value;
                return true;
            }

        }

        private sealed class PeerValidationRuleExecution : ValidationRuleExecution
        {
            private readonly ReadExpression _value;

            internal PeerValidationRuleExecution(
                ReadExpression value,
                ValidationRuleActivation activation,
                Shape comparisonShape)
                : base(activation, comparisonShape)
            {
                _value = value ?? throw new System.ArgumentNullException(nameof(value));
            }

            public ReadExpression Value => _value;

            internal override bool TryGetPeer(out ValueExpression peer)
            {
                peer = _value;
                return true;
            }

        }
    }
}
