using Alis.Reactive;
using Alis.Reactive.PlanModel;
using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A single validation rule projected for browser execution.
    /// </summary>
    public sealed class ValidationRule
    {
        private readonly ValidationRuleName _rule;
        private readonly ValidationMessage _message;
        private readonly ValidationRuleOperand _operand;
        private readonly ValidationRuleActivation _activation;
        private readonly Shape _shape;

        public string Message => _message.Value;
        public Shape Shape => _shape;

        internal ValidationRule(
            ValidationRuleName rule,
            ValidationMessage message,
            ValidationRuleOperand operand,
            ValidationRuleActivation activation,
            Shape shape)
        {
            _rule = rule ?? throw new System.ArgumentNullException(nameof(rule));
            _message = message ?? throw new System.ArgumentNullException(nameof(message));
            _operand = operand ?? throw new System.ArgumentNullException(nameof(operand));
            _activation = activation ?? throw new System.ArgumentNullException(nameof(activation));
            _shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
        }

        internal Alis.Reactive.PlanModel.ValidationRule ToPlanRule(ValidationPlanBinding binding)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return new Alis.Reactive.PlanModel.ValidationRule(
                _rule,
                _message,
                _operand.ToPlanExecution(
                    _activation.ToPlanActivation(binding),
                    binding,
                    _shape));
        }

        internal System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences =>
            _operand.PeerFieldReferences;
    }

    internal abstract class ValidationRuleOperand
    {
        private protected ValidationRuleOperand() { }

        internal static ValidationRuleOperand None { get; } = new NoValidationRuleOperand();

        internal static ValidationRuleOperand Literal(object? value, Shape shape) =>
            new LiteralValidationRuleOperand(value, shape);

        internal static ValidationRuleOperand Range(ValidationRangeBounds bounds) =>
            new RangeValidationRuleOperand(bounds);

        internal static ValidationRuleOperand PeerField(ValidationFieldPath fieldPath, Shape shape)
        {
            if (fieldPath == null) throw new System.ArgumentNullException(nameof(fieldPath));
            if (shape == null) throw new System.ArgumentNullException(nameof(shape));
            return new PeerFieldValidationRuleOperand(ClientValidationFieldReference.Of(fieldPath, shape));
        }

        internal abstract System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences { get; }

        internal abstract ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape);
    }

    internal sealed class NoValidationRuleOperand : ValidationRuleOperand
    {
        internal NoValidationRuleOperand() { }

        internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield break; }
        }

        internal override ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return ValidationRuleExecution.WithoutTarget(activation, comparisonShape);
        }
    }

    internal sealed class LiteralValidationRuleOperand : ValidationRuleOperand
    {
        private readonly object? _value;
        private readonly Shape _shape;

        internal LiteralValidationRuleOperand(object? value, Shape shape)
        {
            _value = value;
            _shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
        }

        internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield break; }
        }

        internal override ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return ValidationRuleExecution.WithConstraint(
                ValueProducer.LiteralRaw(_value, _shape),
                activation,
                comparisonShape);
        }
    }

    internal sealed class RangeValidationRuleOperand : ValidationRuleOperand
    {
        private readonly ValidationRangeBounds _bounds;

        internal RangeValidationRuleOperand(ValidationRangeBounds bounds)
        {
            if (bounds == null) throw new System.ArgumentNullException(nameof(bounds));
            _bounds = bounds;
        }

        internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield break; }
        }

        internal override ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return ValidationRuleExecution.WithConstraint(
                ValueProducer.LiteralRaw(_bounds.ToDescriptorArray(), _bounds.DescriptorShape),
                activation,
                comparisonShape);
        }
    }

    internal sealed class PeerFieldValidationRuleOperand : ValidationRuleOperand
    {
        private readonly ClientValidationFieldReference _field;

        internal PeerFieldValidationRuleOperand(ClientValidationFieldReference field)
        {
            if (field == null) throw new System.ArgumentNullException(nameof(field));
            _field = field;
        }

        internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield return _field; }
        }

        internal override ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return ValidationRuleExecution.WithPeer(
                binding.ResolvePeerValue(_field.Path),
                activation,
                comparisonShape);
        }
    }

    internal abstract class ValidationRuleActivation
    {
        private protected ValidationRuleActivation() { }

        internal static ValidationRuleActivation Always { get; } =
            new AlwaysValidationRuleActivation();

        internal static ValidationRuleActivation When(FieldCondition condition)
        {
            if (condition == null) throw new System.ArgumentNullException(nameof(condition));
            return new ConditionalValidationRuleActivation(condition);
        }

        internal abstract Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding);

        internal abstract ValidationRuleActivation Combine(ValidationRuleActivation incoming);

        internal abstract ValidationRuleActivation AppendTo(FieldCondition existingCondition);
    }

    internal sealed class AlwaysValidationRuleActivation : ValidationRuleActivation
    {
        internal AlwaysValidationRuleActivation() { }

        internal override Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return Alis.Reactive.PlanModel.ValidationRuleActivation.Always;
        }

        internal override ValidationRuleActivation Combine(ValidationRuleActivation incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
            return incoming;
        }

        internal override ValidationRuleActivation AppendTo(FieldCondition existingCondition)
        {
            if (existingCondition == null) throw new System.ArgumentNullException(nameof(existingCondition));
            return When(existingCondition);
        }
    }

    internal sealed class ConditionalValidationRuleActivation : ValidationRuleActivation
    {
        private readonly FieldCondition _condition;

        internal ConditionalValidationRuleActivation(FieldCondition condition)
        {
            _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
        }

        internal override Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
        {
            if (binding == null) throw new System.ArgumentNullException(nameof(binding));
            return Alis.Reactive.PlanModel.ValidationRuleActivation.When(binding.ResolveActivationCondition(_condition));
        }

        internal override ValidationRuleActivation Combine(ValidationRuleActivation incoming)
        {
            if (incoming == null) throw new System.ArgumentNullException(nameof(incoming));
            return incoming.AppendTo(_condition);
        }

        internal override ValidationRuleActivation AppendTo(FieldCondition existingCondition)
        {
            if (existingCondition == null) throw new System.ArgumentNullException(nameof(existingCondition));
            return When(FieldCondition.All(existingCondition, _condition));
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

        internal Shape DescriptorShape => Shape.ArrayOf(Shape.IsNone ? Shape.Any : Shape);

        internal static ValidationRangeBounds Between(object lowerBound, object upperBound, Shape shape) =>
            new ValidationRangeBounds(lowerBound, upperBound, shape);

        internal static ValidationRangeBounds FromProjection<TValue>(TValue lowerBound, TValue upperBound)
        {
            if (lowerBound == null) throw new System.ArgumentNullException(nameof(lowerBound));
            if (upperBound == null) throw new System.ArgumentNullException(nameof(upperBound));
            if (TryFromProjection(lowerBound, upperBound, out var bounds)) return bounds;

            var lowerShape = ClientValidationProjectionLiteral.From(lowerBound).Shape;
            var upperShape = ClientValidationProjectionLiteral.From(upperBound).Shape;
            throw new System.ArgumentException(
                "Client validation range bounds must have the same shape. " +
                $"Lower bound is '{lowerShape.Kind}', upper bound is '{upperShape.Kind}'.");
        }

        internal static bool TryFromProjection(
            object? lowerBound,
            object? upperBound,
            [NotNullWhen(true)] out ValidationRangeBounds? bounds)
        {
            bounds = null;
            if (lowerBound == null || upperBound == null) return false;

            var lowerLiteral = ClientValidationProjectionLiteral.From(lowerBound);
            var upperLiteral = ClientValidationProjectionLiteral.From(upperBound);
            if (!lowerLiteral.Shape.Equals(upperLiteral.Shape)) return false;
            if (lowerLiteral.Value == null || upperLiteral.Value == null) return false;

            bounds = Between(
                lowerLiteral.Value,
                upperLiteral.Value,
                lowerLiteral.Shape);
            return true;
        }

        internal object[] ToDescriptorArray() =>
            new[] { _lowerBound, _upperBound };
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
