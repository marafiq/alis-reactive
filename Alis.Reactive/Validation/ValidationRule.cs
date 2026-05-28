using Alis.Reactive;
using Alis.Reactive.PlanModel;
using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A single validation rule for browser execution.
    /// </summary>
    public sealed class ValidationRule
    {
        private readonly ValidationRuleName _rule;
        private readonly ValidationMessage _message;
        private readonly ValidationRuleOperand _operand;
        private readonly ClientRuleActivation _activation;
        private readonly Shape _shape;

        public string Message => _message.Value;
        public Shape Shape => _shape;

        internal ValidationRule(
            ValidationRuleName rule,
            ValidationMessage message,
            ValidationRuleOperand operand,
            ClientRuleActivation activation,
            Shape shape)
        {
            _rule = rule;
            _message = message;
            _operand = operand;
            _activation = activation;
            _shape = shape;
        }

        internal Alis.Reactive.PlanModel.ValidationRule ToPlanRule(ValidationPlanBinding binding)
        {
            return new Alis.Reactive.PlanModel.ValidationRule(
                _rule,
                _message,
                _operand.ToPlanExecution(
                    _activation.ToPlanActivation(binding),
                    binding,
                    _shape));
        }

        internal ValidationRule PrefixedBy(
            ValidationFieldPath prefix,
            ClientRuleActivation parentActivation)
        {
            return new ValidationRule(
                _rule,
                _message,
                _operand.PrefixedBy(prefix),
                parentActivation.Combine(_activation.PrefixedBy(prefix)),
                _shape);
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
            return new PeerFieldValidationRuleOperand(ClientValidationFieldReference.Of(fieldPath, shape));
        }

        internal abstract System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences { get; }

        internal abstract ValidationRuleOperand PrefixedBy(ValidationFieldPath prefix);

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

        internal override ValidationRuleOperand PrefixedBy(ValidationFieldPath prefix)
        {
            return this;
        }

        internal override ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
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
            _shape = shape;
        }

        internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield break; }
        }

        internal override ValidationRuleOperand PrefixedBy(ValidationFieldPath prefix)
        {
            return this;
        }

        internal override ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            return ValidationRuleExecution.WithConstraint(
                ValueExpression.LiteralRaw(_value, _shape),
                activation,
                comparisonShape);
        }
    }

    internal sealed class RangeValidationRuleOperand : ValidationRuleOperand
    {
        private readonly ValidationRangeBounds _bounds;

        internal RangeValidationRuleOperand(ValidationRangeBounds bounds)
        {
            _bounds = bounds;
        }

        internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield break; }
        }

        internal override ValidationRuleOperand PrefixedBy(ValidationFieldPath prefix)
        {
            return this;
        }

        internal override ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            return ValidationRuleExecution.WithConstraint(
                ValueExpression.LiteralRaw(_bounds.ToDescriptorArray(), _bounds.DescriptorShape),
                activation,
                comparisonShape);
        }
    }

    internal sealed class PeerFieldValidationRuleOperand : ValidationRuleOperand
    {
        private readonly ClientValidationFieldReference _field;

        internal PeerFieldValidationRuleOperand(ClientValidationFieldReference field)
        {
            _field = field;
        }

        internal override System.Collections.Generic.IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield return _field; }
        }

        internal override ValidationRuleOperand PrefixedBy(ValidationFieldPath prefix)
        {
            return PeerField(prefix.Append(_field.Path), _field.Shape);
        }

        internal override ValidationRuleExecution ToPlanExecution(
            Alis.Reactive.PlanModel.ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            return ValidationRuleExecution.WithPeer(
                binding.ResolvePeerValue(_field.Path),
                activation,
                comparisonShape);
        }
    }

    internal abstract class ClientRuleActivation
    {
        private protected ClientRuleActivation() { }

        internal static ClientRuleActivation Always { get; } =
            new AlwaysClientRuleActivation();

        internal static ClientRuleActivation When(FieldCondition condition)
        {
            return new ConditionalClientRuleActivation(condition);
        }

        internal abstract Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding);

        internal abstract ClientRuleActivation Combine(ClientRuleActivation incoming);

        internal abstract ClientRuleActivation AppendTo(FieldCondition existingCondition);

        internal abstract ClientRuleActivation PrefixedBy(ValidationFieldPath prefix);
    }

    internal sealed class AlwaysClientRuleActivation : ClientRuleActivation
    {
        internal AlwaysClientRuleActivation() { }

        internal override Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
        {
            return Alis.Reactive.PlanModel.ValidationRuleActivation.Always;
        }

        internal override ClientRuleActivation Combine(ClientRuleActivation incoming)
        {
            return incoming;
        }

        internal override ClientRuleActivation AppendTo(FieldCondition existingCondition)
        {
            return When(existingCondition);
        }

        internal override ClientRuleActivation PrefixedBy(ValidationFieldPath prefix)
        {
            return this;
        }
    }

    internal sealed class ConditionalClientRuleActivation : ClientRuleActivation
    {
        private readonly FieldCondition _condition;

        internal ConditionalClientRuleActivation(FieldCondition condition)
        {
            _condition = condition;
        }

        internal override Alis.Reactive.PlanModel.ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
        {
            return Alis.Reactive.PlanModel.ValidationRuleActivation.When(binding.ResolveActivationCondition(_condition));
        }

        internal override ClientRuleActivation Combine(ClientRuleActivation incoming)
        {
            return incoming.AppendTo(_condition);
        }

        internal override ClientRuleActivation AppendTo(FieldCondition existingCondition)
        {
            return When(FieldCondition.All(existingCondition, _condition));
        }

        internal override ClientRuleActivation PrefixedBy(ValidationFieldPath prefix)
        {
            return When(_condition.PrefixWith(new FieldConditionPrefixBinding(prefix)));
        }
    }

    internal sealed class ValidationRangeBounds
    {
        private readonly object _lowerBound;
        private readonly object _upperBound;

        private ValidationRangeBounds(object lowerBound, object upperBound, Shape shape)
        {
            _lowerBound = lowerBound;
            _upperBound = upperBound;
            Shape = shape;
        }

        internal Shape Shape { get; }

        internal Shape DescriptorShape => Shape.ArrayOf(Shape.IsNone ? Shape.Any : Shape);

        internal static ValidationRangeBounds Between(object lowerBound, object upperBound, Shape shape) =>
            new ValidationRangeBounds(lowerBound, upperBound, shape);

        internal static ValidationRangeBounds FromClientLiteral<TValue>(TValue lowerBound, TValue upperBound)
        {
            if (lowerBound == null) throw new System.ArgumentNullException(nameof(lowerBound));
            if (upperBound == null) throw new System.ArgumentNullException(nameof(upperBound));
            if (TryFromClientLiteral(lowerBound, upperBound, out var bounds)) return bounds;

            var lowerShape = ClientValidationLiteral.From(lowerBound).Shape;
            var upperShape = ClientValidationLiteral.From(upperBound).Shape;
            throw new System.ArgumentException(
                "Client validation range bounds must have the same shape. " +
                $"Lower bound is '{lowerShape.Kind}', upper bound is '{upperShape.Kind}'.");
        }

        internal static bool TryFromClientLiteral(
            object? lowerBound,
            object? upperBound,
            [NotNullWhen(true)] out ValidationRangeBounds? bounds)
        {
            bounds = null;
            if (lowerBound == null || upperBound == null) return false;

            var lowerLiteral = ClientValidationLiteral.From(lowerBound);
            var upperLiteral = ClientValidationLiteral.From(upperBound);
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
        private readonly ClientValidationFieldBinder _fieldBindings;
        private readonly FieldConditionPlanBinding _conditionBinding;

        private ValidationPlanBinding(ClientValidationFieldBinder fieldBindings)
        {
            _fieldBindings = fieldBindings;
            _conditionBinding = FieldConditionPlanBinding.For(_fieldBindings);
        }

        internal static ValidationPlanBinding For(ClientValidationFieldBinder fieldBindings) =>
            new ValidationPlanBinding(fieldBindings);

        internal ValueExpression ResolvePeerValue(ValidationFieldPath fieldPath)
        {
            return _fieldBindings.Resolve(fieldPath).ReadValue();
        }

        internal ConditionGraph ResolveActivationCondition(FieldCondition condition)
        {
            return condition.ToPlanCondition(_conditionBinding);
        }
    }
}
