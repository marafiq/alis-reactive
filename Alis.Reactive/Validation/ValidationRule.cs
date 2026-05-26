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
        public Shape Shape => _details.Shape;
        public ValidationRuleOperand Operand => _details.Operand;
        public ValidationRuleActivation Activation => _details.Activation;

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
                _details.ToPlanExecution(binding));
        }

        internal System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences =>
            _details.PeerFieldReferences;
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

    public enum ValidationRuleOperandKind
    {
        None,
        Literal,
        Range,
        PeerField
    }

    public abstract class ValidationRuleOperand
    {
        private protected ValidationRuleOperand() { }

        public abstract ValidationRuleOperandKind Kind { get; }
    }

    public sealed class NoValidationRuleOperand : ValidationRuleOperand
    {
        internal NoValidationRuleOperand() { }

        public override ValidationRuleOperandKind Kind => ValidationRuleOperandKind.None;
    }

    public sealed class LiteralValidationRuleOperand : ValidationRuleOperand
    {
        internal LiteralValidationRuleOperand(object? value, Shape shape)
        {
            Value = value;
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
        }

        public override ValidationRuleOperandKind Kind => ValidationRuleOperandKind.Literal;
        public object? Value { get; }
        public Shape Shape { get; }
    }

    public sealed class RangeValidationRuleOperand : ValidationRuleOperand
    {
        internal RangeValidationRuleOperand(ValidationRangeBounds bounds)
        {
            if (bounds == null) throw new System.ArgumentNullException(nameof(bounds));
            LowerBound = bounds.LowerBound;
            UpperBound = bounds.UpperBound;
            EndpointShape = bounds.Shape;
        }

        public override ValidationRuleOperandKind Kind => ValidationRuleOperandKind.Range;
        public object LowerBound { get; }
        public object UpperBound { get; }
        public Shape EndpointShape { get; }

        public object[] ToArray() =>
            new[] { LowerBound, UpperBound };
    }

    public sealed class PeerFieldValidationRuleOperand : ValidationRuleOperand
    {
        internal PeerFieldValidationRuleOperand(ClientValidationFieldReference field)
        {
            if (field == null) throw new System.ArgumentNullException(nameof(field));
            FieldName = field.Path.Value;
            Shape = field.Shape;
        }

        public override ValidationRuleOperandKind Kind => ValidationRuleOperandKind.PeerField;
        public string FieldName { get; }
        public Shape Shape { get; }
    }

    public enum ValidationRuleActivationKind
    {
        Always,
        When
    }

    public abstract class ValidationRuleActivation
    {
        private protected ValidationRuleActivation() { }

        public abstract ValidationRuleActivationKind Kind { get; }
    }

    public sealed class AlwaysValidationRuleActivation : ValidationRuleActivation
    {
        internal AlwaysValidationRuleActivation() { }

        public override ValidationRuleActivationKind Kind => ValidationRuleActivationKind.Always;
    }

    public sealed class ConditionalValidationRuleActivation : ValidationRuleActivation
    {
        internal ConditionalValidationRuleActivation(FieldCondition condition)
        {
            Condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
        }

        public override ValidationRuleActivationKind Kind => ValidationRuleActivationKind.When;
        public FieldCondition Condition { get; }
    }

    internal sealed class ValidationRuleDetails
    {
        private readonly ValidationRuleTarget _target;
        private readonly ValidationRuleCondition _condition;

        private ValidationRuleDetails(
            ValidationRuleTarget target,
            ValidationRuleCondition condition,
            Shape shape)
        {
            _target = target ?? throw new System.ArgumentNullException(nameof(target));
            _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
            Shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
        }

        internal System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences => _target.PeerFieldReferences;
        internal Shape Shape { get; }
        internal ValidationRuleOperand Operand => _target.ToPublicOperand(Shape);
        internal ValidationRuleActivation Activation => _condition.ToPublicActivation();

        internal ValidationRuleExecution ToPlanExecution(ValidationPlanBinding binding)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return _target.ToPlanExecution(_condition.ToPlanActivation(binding), binding, Shape);
        }

        internal ValidationRuleDetails PrefixPeerFieldWith(ValidationFieldPath prefix)
        {
            if (prefix == null) throw new System.ArgumentNullException(nameof(prefix));
            if (prefix.IsEmpty) return this;
            return new ValidationRuleDetails(
                _target.PrefixPeerFieldWith(prefix),
                _condition,
                Shape);
        }

        internal static ValidationRuleDetails NoOperand(ValidationRuleCondition condition) =>
            new ValidationRuleDetails(
                ValidationRuleTarget.None,
                condition,
                Shape.None);

        internal static ValidationRuleDetails WithConstraint(
            object? constraint,
            ValidationRuleCondition condition,
            Shape shape) =>
            new ValidationRuleDetails(
                ValidationRuleTarget.Literal(constraint),
                condition,
                shape);

        internal static ValidationRuleDetails WithConstraint(
            ValidationRuleTarget target,
            ValidationRuleCondition condition,
            Shape shape) =>
            new ValidationRuleDetails(
                target,
                condition,
                shape);

        internal static ValidationRuleDetails WithPeerField(
            ValidationFieldPath peerField,
            ValidationRuleCondition condition,
            Shape shape) =>
            new ValidationRuleDetails(
                ValidationRuleTarget.PeerField(peerField, shape),
                condition,
                shape);
    }

    internal abstract class ValidationRuleCondition
    {
        private ValidationRuleCondition() { }

        internal static ValidationRuleCondition Always { get; } =
            new AlwaysValidationRuleCondition();

        internal abstract Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding);
        internal abstract ValidationRuleActivation ToPublicActivation();

        internal static ValidationRuleCondition When(FieldCondition condition)
        {
            if (condition == null) throw new System.ArgumentNullException(nameof(condition));
            return new ConditionalValidationRuleCondition(condition);
        }

        internal abstract ValidationRuleCondition Combine(ValidationRuleCondition incoming);

        internal abstract ValidationRuleCondition AppendTo(FieldCondition existingCondition);

        private sealed class AlwaysValidationRuleCondition : ValidationRuleCondition
        {
            internal override Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                return Alis.Reactive.PlanModel.ValidationRuleActivation.Always;
            }

            internal override ValidationRuleActivation ToPublicActivation() =>
                new AlwaysValidationRuleActivation();

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

            internal override Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                return Alis.Reactive.PlanModel.ValidationRuleActivation.When(binding.ResolveActivationCondition(_condition));
            }

            internal override ValidationRuleActivation ToPublicActivation() =>
                new ConditionalValidationRuleActivation(_condition);

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

    internal abstract class ValidationRuleTarget
    {
        private ValidationRuleTarget() { }

        internal static ValidationRuleTarget None { get; } =
            new NoValidationRuleTarget();

        internal abstract System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences { get; }
        internal abstract ValidationRuleOperand ToPublicOperand(Shape shape);

        internal abstract ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape shape);

        internal abstract ValidationRuleTarget PrefixPeerFieldWith(ValidationFieldPath prefix);

        internal static ValidationRuleTarget Literal(object? value) =>
            new LiteralValidationRuleTarget(value);

        internal static ValidationRuleTarget Range(ValidationRangeBounds bounds) =>
            new RangeValidationRuleTarget(bounds);

        internal static ValidationRuleTarget PeerField(ValidationFieldPath fieldPath, Shape shape)
        {
            if (fieldPath == null) throw new System.ArgumentNullException(nameof(fieldPath));
            if (shape == null) throw new System.ArgumentNullException(nameof(shape));
            return new PeerValidationRuleTarget(ClientValidationFieldReference.Of(fieldPath, shape));
        }

        private sealed class NoValidationRuleTarget : ValidationRuleTarget
        {
            internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
            {
                get { yield break; }
            }

            internal override ValidationRuleOperand ToPublicOperand(Shape shape)
            {
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return new NoValidationRuleOperand();
            }

            internal override ValidationRuleExecution ToPlanExecution(
                Alis.Reactive.PlanModel.ValidationRuleActivation activation,
                ValidationPlanBinding binding,
                Shape shape)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                return ValidationRuleExecution.WithoutTarget(activation, shape);
            }

            internal override ValidationRuleTarget PrefixPeerFieldWith(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new System.ArgumentNullException(nameof(prefix));
                return this;
            }
        }

        private sealed class LiteralValidationRuleTarget : ValidationRuleTarget
        {
            private readonly object? _value;

            internal LiteralValidationRuleTarget(object? value)
            {
                _value = value;
            }

            internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
            {
                get { yield break; }
            }

            internal override ValidationRuleOperand ToPublicOperand(Shape shape)
            {
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return new LiteralValidationRuleOperand(_value, shape);
            }

            internal override ValidationRuleExecution ToPlanExecution(
                Alis.Reactive.PlanModel.ValidationRuleActivation activation,
                ValidationPlanBinding binding,
                Shape shape)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return ValidationRuleExecution.WithConstraint(
                    ValueProducer.LiteralRaw(_value, shape),
                    activation,
                    shape);
            }

            internal override ValidationRuleTarget PrefixPeerFieldWith(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new System.ArgumentNullException(nameof(prefix));
                return this;
            }
        }

        private sealed class RangeValidationRuleTarget : ValidationRuleTarget
        {
            private readonly ValidationRangeBounds _bounds;

            internal RangeValidationRuleTarget(ValidationRangeBounds bounds)
            {
                _bounds = bounds ?? throw new System.ArgumentNullException(nameof(bounds));
            }

            internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
            {
                get { yield break; }
            }

            internal override ValidationRuleOperand ToPublicOperand(Shape shape)
            {
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return new RangeValidationRuleOperand(_bounds);
            }

            internal override ValidationRuleExecution ToPlanExecution(
                Alis.Reactive.PlanModel.ValidationRuleActivation activation,
                ValidationPlanBinding binding,
                Shape shape)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return ValidationRuleExecution.WithConstraint(
                    ValueProducer.LiteralRaw(_bounds.ToDescriptorArray(), _bounds.DescriptorShape),
                    activation,
                    shape);
            }

            internal override ValidationRuleTarget PrefixPeerFieldWith(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new System.ArgumentNullException(nameof(prefix));
                return this;
            }
        }

        private sealed class PeerValidationRuleTarget : ValidationRuleTarget
        {
            private readonly ClientValidationFieldReference _field;

            internal PeerValidationRuleTarget(ClientValidationFieldReference field)
            {
                _field = field ?? throw new System.ArgumentNullException(nameof(field));
            }

            internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
            {
                get { yield return _field; }
            }

            internal override ValidationRuleOperand ToPublicOperand(Shape shape)
            {
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return new PeerFieldValidationRuleOperand(_field);
            }

            internal override ValidationRuleExecution ToPlanExecution(
                Alis.Reactive.PlanModel.ValidationRuleActivation activation,
                ValidationPlanBinding binding,
                Shape shape)
            {
                if (binding == null) throw new System.ArgumentNullException(nameof(binding));
                if (shape == null) throw new System.ArgumentNullException(nameof(shape));
                return ValidationRuleExecution.WithPeer(
                    binding.ResolvePeerValue(_field.Path),
                    activation,
                    shape);
            }

            internal override ValidationRuleTarget PrefixPeerFieldWith(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new System.ArgumentNullException(nameof(prefix));
                return PeerField(prefix.Append(_field.Path), _field.Shape);
            }
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
        internal object LowerBound => _lowerBound;
        internal object UpperBound => _upperBound;

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

        internal Condition ResolveActivationCondition(FieldCondition condition)
        {
            if (condition == null) throw new System.ArgumentNullException(nameof(condition));
            return condition.ToPlanCondition(_conditionBinding);
        }
    }
}
