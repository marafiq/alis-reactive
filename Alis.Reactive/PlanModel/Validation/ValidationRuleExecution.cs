using System.Text.Json;

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
        public abstract string Kind { get; }

        internal void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", Kind);
            WriteOperand(writer, options);
            PlanJsonWriter.WriteProperty(writer, options, "activation", Activation);
            PlanJsonWriter.WriteProperty(writer, options, "comparisonShape", ComparisonShape);
            writer.WriteEndObject();
        }

        internal abstract void WriteOperand(Utf8JsonWriter writer, JsonSerializerOptions options);

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
            public override string Kind => "none";

            internal NoOperandValidationRuleExecution(
                ValidationRuleActivation activation,
                Shape comparisonShape)
                : base(activation, comparisonShape)
            {
            }

            internal override void WriteOperand(Utf8JsonWriter writer, JsonSerializerOptions options)
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

            public override string Kind => "constraint";
            public LiteralExpression Value => _value;

            internal override void WriteOperand(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                PlanJsonWriter.WriteProperty(writer, options, "value", _value);
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

            public override string Kind => "peer";
            public ReadExpression Value => _value;

            internal override void WriteOperand(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                PlanJsonWriter.WriteProperty(writer, options, "value", _value);
        }
    }
}
