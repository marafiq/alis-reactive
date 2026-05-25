using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A single validation rule projected for browser execution.
    /// </summary>
    public sealed class ValidationRule
    {
        private readonly ValidationRuleName _rule;
        private readonly ValidationMessage _message;
        private readonly ValidationRuleDetails _details;

        public ValidationRuleKind Kind => _rule.ToPublicKind();
        public string Message => _message.Value;
        public ValidationRuleExecutionIntent Execution => _details.ExecutionIntent;
        public Shape Shape => _details.Shape;

        internal ValidationRule(
            ValidationRuleName rule,
            ValidationMessage message,
            ValidationRuleDetails details)
        {
            _rule = rule ?? throw new System.ArgumentNullException(nameof(rule));
            _message = message ?? throw new System.ArgumentNullException(nameof(message));
            _details = details ?? throw new System.ArgumentNullException(nameof(details));
        }

        internal Alis.Reactive.PlanModel.ValidationRule ToPlanRule(ValidationPlanBinding binding)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return new Alis.Reactive.PlanModel.ValidationRule(
                _rule,
                _message,
                _details.ToPlanExecution(_rule, binding));
        }
    }

    public enum ValidationRuleKind
    {
        Required,
        Empty,
        MinLength,
        MaxLength,
        Email,
        Regex,
        Url,
        CreditCard,
        Range,
        ExclusiveRange,
        Min,
        Max,
        GreaterThan,
        LessThan,
        EqualTo,
        NotEqual,
        NotEqualTo,
        AtLeastOne
    }

    internal sealed class ValidationRuleDetails
    {
        private readonly ValidationConstraint _constraint;
        private readonly ValidationRuleCondition _condition;
        private readonly ValidationPeerField _peerField;

        private ValidationRuleDetails(
            ValidationConstraint constraint,
            ValidationRuleCondition condition,
            ValidationPeerField peerField,
            Shape shape)
        {
            _constraint = constraint ?? throw new System.ArgumentNullException(nameof(constraint));
            _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
            _peerField = peerField ?? throw new System.ArgumentNullException(nameof(peerField));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
        }

        internal System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences => _peerField.FieldReferences;
        internal Shape Shape { get; }
        internal ValidationRuleExecutionIntent ExecutionIntent =>
            ValidationRuleExecutionIntent.From(
                _constraint.ToIntent(),
                _peerField.ToIntent(),
                _condition.ToIntent(),
                Shape);

        internal ValidationRuleExecution ToPlanExecution(ValidationRuleName rule, ValidationPlanBinding binding)
        {
            if (rule == null) throw new System.ArgumentNullException(nameof(rule));
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return ValidationRuleExecution.ForRule(
                rule,
                _constraint.ToPlanOperand(Shape),
                _peerField.ToPlanOperand(binding),
                _condition.ToPlanActivation(binding),
                Shape);
        }

        internal ValidationRuleDetails PrefixPeerFieldWith(ValidationFieldPath prefix)
        {
            if (prefix == null) throw new System.ArgumentNullException(nameof(prefix));
            if (prefix.IsEmpty) return this;
            return new ValidationRuleDetails(
                _constraint,
                _condition,
                _peerField.PrefixWith(prefix),
                Shape);
        }

        internal static ValidationRuleDetails NoOperand(ValidationRuleCondition condition) =>
            new ValidationRuleDetails(
                ValidationConstraint.Missing,
                condition,
                ValidationPeerField.None,
                Shape.None);

        internal static ValidationRuleDetails WithConstraint(
            object? constraint,
            ValidationRuleCondition condition,
            Shape shape) =>
            new ValidationRuleDetails(
                ValidationConstraint.Literal(constraint),
                condition,
                ValidationPeerField.None,
                shape);

        internal static ValidationRuleDetails WithConstraint(
            ValidationConstraint constraint,
            ValidationRuleCondition condition,
            Shape shape) =>
            new ValidationRuleDetails(
                constraint,
                condition,
                ValidationPeerField.None,
                shape);

        internal static ValidationRuleDetails WithPeerField(
            ValidationFieldPath peerField,
            ValidationRuleCondition condition,
            Shape shape) =>
            new ValidationRuleDetails(
                ValidationConstraint.Missing,
                condition,
                ValidationPeerField.Of(peerField, shape),
                shape);
    }

    internal abstract class ValidationRuleCondition
    {
        private ValidationRuleCondition() { }

        internal static ValidationRuleCondition Always { get; } =
            new AlwaysValidationRuleCondition();

        internal abstract ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding);
        internal abstract ValidationRuleActivationIntent ToIntent();

        internal static ValidationRuleCondition When(FieldCondition condition)
        {
            if (condition == null) throw new System.ArgumentNullException(nameof(condition));
            return new ConditionalValidationRuleCondition(condition);
        }

        internal abstract ValidationRuleCondition Combine(ValidationRuleCondition incoming);

        internal abstract ValidationRuleCondition AppendTo(FieldCondition existingCondition);

        private sealed class AlwaysValidationRuleCondition : ValidationRuleCondition
        {
            internal override ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                return ValidationRuleActivation.Always;
            }

            internal override ValidationRuleActivationIntent ToIntent() =>
                ValidationRuleActivationIntent.Always;

            internal override ValidationRuleCondition Combine(ValidationRuleCondition incoming)
            {
                if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
                return incoming;
            }

            internal override ValidationRuleCondition AppendTo(FieldCondition existingCondition)
            {
                if (existingCondition == null) throw new System.ArgumentNullException(nameof(existingCondition));
                return When(existingCondition);
            }
        }

        private sealed class ConditionalValidationRuleCondition : ValidationRuleCondition
        {
            private readonly FieldCondition _condition;

            internal ConditionalValidationRuleCondition(FieldCondition condition)
            {
                _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
            }

            internal override ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                return ValidationRuleActivation.When(binding.ResolveActivationCondition(_condition));
            }

            internal override ValidationRuleActivationIntent ToIntent() =>
                ValidationRuleActivationIntent.When(_condition);

            internal override ValidationRuleCondition Combine(ValidationRuleCondition incoming)
            {
                if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
                return incoming.AppendTo(_condition);
            }

            internal override ValidationRuleCondition AppendTo(FieldCondition existingCondition)
            {
                if (existingCondition == null) throw new System.ArgumentNullException(nameof(existingCondition));
                return When(FieldCondition.All(existingCondition, _condition));
            }
        }
    }

    internal abstract class ValidationConstraint
    {
        private ValidationConstraint() { }

        internal static ValidationConstraint Missing { get; } =
            new MissingValidationConstraint();

        internal abstract ValidationRuleOperand ToPlanOperand(Shape shape);
        internal abstract ValidationRuleOperandIntent ToIntent();

        internal static ValidationConstraint Literal(object? value) =>
            new LiteralValidationConstraint(value);

        internal static ValidationConstraint InclusiveRange(ValidationRangeBounds bounds) =>
            new RangeValidationConstraint(bounds);

        private sealed class MissingValidationConstraint : ValidationConstraint
        {
            internal override ValidationRuleOperand ToPlanOperand(Shape shape)
            {
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return ValidationRuleOperand.None;
            }

            internal override ValidationRuleOperandIntent ToIntent() =>
                ValidationRuleOperandIntent.None;
        }

        private sealed class LiteralValidationConstraint : ValidationConstraint
        {
            private readonly object? _value;

            internal LiteralValidationConstraint(object? value)
            {
                _value = value;
            }

            internal override ValidationRuleOperand ToPlanOperand(Shape shape)
            {
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return ValidationRuleOperand.Constraint(ValueProducer.LiteralRaw(_value, shape));
            }

            internal override ValidationRuleOperandIntent ToIntent() =>
                ValidationRuleOperandIntent.Literal(_value);
        }

        private sealed class RangeValidationConstraint : ValidationConstraint
        {
            private readonly ValidationRangeBounds _bounds;

            internal RangeValidationConstraint(ValidationRangeBounds bounds)
            {
                _bounds = bounds ?? throw new System.ArgumentNullException(nameof(bounds));
            }

            internal override ValidationRuleOperand ToPlanOperand(Shape shape)
            {
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return ValidationRuleOperand.Constraint(
                    ValueProducer.LiteralRaw(_bounds.ToDescriptorArray(), _bounds.DescriptorShape));
            }

            internal override ValidationRuleOperandIntent ToIntent() =>
                ValidationRuleOperandIntent.Range(_bounds);
        }
    }

    internal sealed class ValidationRangeBounds
    {
        private readonly object _lowerBound;
        private readonly object _upperBound;

        private ValidationRangeBounds(object lowerBound, object upperBound, Shape shape)
        {
            _lowerBound = lowerBound ?? throw new System.ArgumentNullException(nameof(lowerBound));
            _upperBound = upperBound ?? throw new System.ArgumentNullException(nameof(upperBound));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
        }

        internal Shape Shape { get; }

        internal Shape DescriptorShape => ValidationRangeDescriptorShape.FromEndpointShape(Shape);

        internal static ValidationRangeBounds Between(object lowerBound, object upperBound, Shape shape) =>
            new ValidationRangeBounds(lowerBound, upperBound, shape);

        internal object[] ToDescriptorArray() =>
            new[] { _lowerBound, _upperBound };
    }

    internal static class ValidationRangeDescriptorShape
    {
        internal static Shape FromEndpointShape(Shape endpointShape)
        {
            if (endpointShape == null) throw new System.ArgumentNullException(nameof(endpointShape));
            if (endpointShape.IsNone) return Shape.ArrayOf(Shape.Any);

            return Shape.ArrayOf(endpointShape);
        }
    }

    internal abstract class ValidationPeerField
    {
        private ValidationPeerField() { }

        internal static ValidationPeerField None { get; } =
            new NoValidationPeerField();

        internal abstract System.Collections.Generic.IEnumerable<ClientValidationFieldReference> FieldReferences { get; }

        internal abstract ValidationRuleOperand ToPlanOperand(ValidationPlanBinding binding);
        internal abstract ValidationRuleOperandIntent ToIntent();
        internal abstract ValidationPeerField PrefixWith(ValidationFieldPath prefix);

        internal static ValidationPeerField Of(ValidationFieldPath fieldPath, Shape shape)
        {
            if (fieldPath == null) throw new System.ArgumentNullException(nameof(fieldPath));
            if (shape == null) throw new System.ArgumentNullException(nameof(shape));
            return new NamedValidationPeerField(ClientValidationFieldReference.Of(fieldPath, shape));
        }

        private sealed class NoValidationPeerField : ValidationPeerField
        {
            internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> FieldReferences
            {
                get { yield break; }
            }

            internal override ValidationRuleOperand ToPlanOperand(ValidationPlanBinding binding)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                return ValidationRuleOperand.None;
            }

            internal override ValidationRuleOperandIntent ToIntent() =>
                ValidationRuleOperandIntent.None;

            internal override ValidationPeerField PrefixWith(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new System.ArgumentNullException(nameof(prefix));
                return this;
            }
        }

        private sealed class NamedValidationPeerField : ValidationPeerField
        {
            private readonly ClientValidationFieldReference _field;

            internal NamedValidationPeerField(ClientValidationFieldReference field)
            {
                _field = field ?? throw new System.ArgumentNullException(nameof(field));
            }

            internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> FieldReferences
            {
                get { yield return _field; }
            }

            internal override ValidationRuleOperand ToPlanOperand(ValidationPlanBinding binding)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                return ValidationRuleOperand.Peer(binding.ResolvePeerValue(_field.Path));
            }

            internal override ValidationRuleOperandIntent ToIntent() =>
                ValidationRuleOperandIntent.PeerField(_field.Path);

            internal override ValidationPeerField PrefixWith(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new System.ArgumentNullException(nameof(prefix));
                return Of(prefix.Append(_field.Path), _field.Shape);
            }
        }
    }

    public sealed class ValidationRuleExecutionIntent
    {
        private readonly ValidationRuleOperandIntent _constraint;
        private readonly ValidationRuleOperandIntent _otherValue;
        private readonly ValidationRuleActivationIntent _activation;

        private ValidationRuleExecutionIntent(
            ValidationRuleOperandIntent constraint,
            ValidationRuleOperandIntent otherValue,
            ValidationRuleActivationIntent activation,
            Shape comparisonShape)
        {
            _constraint = constraint ?? throw new System.ArgumentNullException(nameof(constraint));
            _otherValue = otherValue ?? throw new System.ArgumentNullException(nameof(otherValue));
            _activation = activation ?? throw new System.ArgumentNullException(nameof(activation));
            ComparisonShape = comparisonShape ?? throw new System.ArgumentNullException(nameof(comparisonShape));
        }

        public ValidationRuleOperandIntent Constraint => _constraint;
        public ValidationRuleOperandIntent OtherValue => _otherValue;
        public ValidationRuleActivationIntent Activation => _activation;
        public Shape ComparisonShape { get; }

        internal static ValidationRuleExecutionIntent From(
            ValidationRuleOperandIntent constraint,
            ValidationRuleOperandIntent otherValue,
            ValidationRuleActivationIntent activation,
            Shape comparisonShape) =>
            new ValidationRuleExecutionIntent(
                constraint,
                otherValue,
                activation,
                comparisonShape);
    }

    public abstract class ValidationRuleOperandIntent
    {
        private protected ValidationRuleOperandIntent() { }

        public abstract ValidationRuleOperandKind Kind { get; }

        internal static ValidationRuleOperandIntent None { get; } =
            new NoValidationRuleOperandIntent();

        internal static ValidationRuleOperandIntent Literal(object? value) =>
            new LiteralValidationRuleOperandIntent(value);

        internal static ValidationRuleOperandIntent Range(ValidationRangeBounds bounds)
        {
            if (bounds == null) throw new System.ArgumentNullException(nameof(bounds));
            return new RangeValidationRuleOperandIntent(bounds);
        }

        internal static ValidationRuleOperandIntent PeerField(ValidationFieldPath fieldPath)
        {
            if (fieldPath == null) throw new System.ArgumentNullException(nameof(fieldPath));
            return new PeerFieldValidationRuleOperandIntent(fieldPath);
        }
    }

    public sealed class NoValidationRuleOperandIntent : ValidationRuleOperandIntent
    {
        internal NoValidationRuleOperandIntent() { }

        public override ValidationRuleOperandKind Kind => ValidationRuleOperandKind.None;
    }

    public sealed class LiteralValidationRuleOperandIntent : ValidationRuleOperandIntent
    {
        internal LiteralValidationRuleOperandIntent(object? value)
        {
            Value = value;
        }

        public override ValidationRuleOperandKind Kind => ValidationRuleOperandKind.Literal;
        public object? Value { get; }
        public bool IsLiteralNull => Value == null;
    }

    public sealed class RangeValidationRuleOperandIntent : ValidationRuleOperandIntent
    {
        private readonly object[] _bounds;

        internal RangeValidationRuleOperandIntent(ValidationRangeBounds bounds)
        {
            if (bounds == null) throw new System.ArgumentNullException(nameof(bounds));
            _bounds = bounds.ToDescriptorArray();
        }

        public override ValidationRuleOperandKind Kind => ValidationRuleOperandKind.Range;
        public System.Collections.Generic.IReadOnlyList<object> Bounds => _bounds;
    }

    public sealed class PeerFieldValidationRuleOperandIntent : ValidationRuleOperandIntent
    {
        private readonly ValidationFieldPath _fieldPath;

        internal PeerFieldValidationRuleOperandIntent(ValidationFieldPath fieldPath)
        {
            _fieldPath = fieldPath ?? throw new System.ArgumentNullException(nameof(fieldPath));
        }

        public override ValidationRuleOperandKind Kind => ValidationRuleOperandKind.PeerField;
        public string Field => _fieldPath.Value;
    }

    public enum ValidationRuleOperandKind
    {
        None,
        Literal,
        Range,
        PeerField
    }

    public abstract class ValidationRuleActivationIntent
    {
        private protected ValidationRuleActivationIntent() { }

        public abstract ValidationRuleActivationKind Kind { get; }

        internal static ValidationRuleActivationIntent Always { get; } =
            new AlwaysValidationRuleActivationIntent();

        internal static ValidationRuleActivationIntent When(FieldCondition condition)
        {
            if (condition == null) throw new System.ArgumentNullException(nameof(condition));
            return new ConditionalValidationRuleActivationIntent(condition);
        }
    }

    public sealed class AlwaysValidationRuleActivationIntent : ValidationRuleActivationIntent
    {
        internal AlwaysValidationRuleActivationIntent() { }

        public override ValidationRuleActivationKind Kind => ValidationRuleActivationKind.Always;
    }

    public sealed class ConditionalValidationRuleActivationIntent : ValidationRuleActivationIntent
    {
        private readonly FieldCondition _condition;

        internal ConditionalValidationRuleActivationIntent(FieldCondition condition)
        {
            _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
        }

        public override ValidationRuleActivationKind Kind => ValidationRuleActivationKind.When;
        public FieldCondition Condition => _condition;
    }

    public enum ValidationRuleActivationKind
    {
        Always,
        When
    }

    internal sealed class ValidationPlanBinding
    {
        private readonly ValidationFieldBindingCatalog _fieldBindings;
        private readonly FieldConditionPlanBinding _conditionBinding;

        private ValidationPlanBinding(ValidationFieldBindingCatalog fieldBindings)
        {
            _fieldBindings = fieldBindings ?? throw new System.ArgumentNullException(nameof(fieldBindings));
            _conditionBinding = FieldConditionPlanBinding.For(_fieldBindings);
        }

        internal static ValidationPlanBinding For(ValidationFieldBindingCatalog fieldBindings) =>
            new ValidationPlanBinding(fieldBindings);

        internal ValueProducer ResolvePeerValue(ValidationFieldPath fieldPath)
        {
            if (fieldPath == null) throw new System.ArgumentNullException(nameof(fieldPath));
            return _fieldBindings.Resolve(fieldPath).ReadValue();
        }

        internal ValidationCondition ResolveActivationCondition(FieldCondition condition)
        {
            if (condition == null) throw new System.ArgumentNullException(nameof(condition));
            return condition.ToValidationCondition(_conditionBinding);
        }
    }
}
