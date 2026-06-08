using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    internal abstract class RuleOperand
    {
        private protected RuleOperand() { }

        internal static RuleOperand None { get; } = new NoRuleOperand();

        internal static RuleOperand Literal(object? value, Shape shape) =>
            new LiteralRuleOperand(value, shape);

        internal static RuleOperand Range(ValidationRangeBounds bounds) =>
            new RangeRuleOperand(bounds);

        internal static RuleOperand PeerField(ValidationFieldPath fieldPath, Shape shape)
        {
            return new PeerFieldRuleOperand(ClientValidationFieldReference.Of(fieldPath, shape));
        }

        // Emit projections for a native-component validation emitter. The default is
        // "no value"; operands that carry a literal or a range override these.
        internal virtual object? LiteralValue => null;
        internal virtual object[]? RangeValues => null;

        internal abstract IEnumerable<ClientValidationFieldReference> PeerFieldReferences { get; }

        internal abstract RuleOperand PrefixedBy(ValidationFieldPath prefix);

        internal abstract ValidationRuleExecution ToPlanExecution(
            ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape);
    }

    internal sealed class NoRuleOperand : RuleOperand
    {
        internal NoRuleOperand() { }

        internal override IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield break; }
        }

        internal override RuleOperand PrefixedBy(ValidationFieldPath prefix)
        {
            return this;
        }

        internal override ValidationRuleExecution ToPlanExecution(
            ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            return ValidationRuleExecution.WithoutTarget(activation, comparisonShape);
        }
    }

    internal sealed class LiteralRuleOperand : RuleOperand
    {
        private readonly object? _value;
        private readonly Shape _shape;

        internal LiteralRuleOperand(object? value, Shape shape)
        {
            _value = value;
            _shape = shape;
        }

        internal override object? LiteralValue => _value;

        internal override IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield break; }
        }

        internal override RuleOperand PrefixedBy(ValidationFieldPath prefix)
        {
            return this;
        }

        internal override ValidationRuleExecution ToPlanExecution(
            ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            return ValidationRuleExecution.WithConstraint(
                ValueExpression.LiteralRaw(_value, _shape),
                activation,
                comparisonShape);
        }
    }

    internal sealed class RangeRuleOperand : RuleOperand
    {
        private readonly ValidationRangeBounds _bounds;

        internal RangeRuleOperand(ValidationRangeBounds bounds)
        {
            _bounds = bounds;
        }

        internal override object[]? RangeValues => _bounds.ToDescriptorArray();

        internal override IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield break; }
        }

        internal override RuleOperand PrefixedBy(ValidationFieldPath prefix)
        {
            return this;
        }

        internal override ValidationRuleExecution ToPlanExecution(
            ValidationRuleActivation activation,
            ValidationPlanBinding binding,
            Shape comparisonShape)
        {
            return ValidationRuleExecution.WithConstraint(
                ValueExpression.LiteralRaw(_bounds.ToDescriptorArray(), _bounds.DescriptorShape),
                activation,
                comparisonShape);
        }
    }

    internal sealed class PeerFieldRuleOperand : RuleOperand
    {
        private readonly ClientValidationFieldReference _field;

        internal PeerFieldRuleOperand(ClientValidationFieldReference field)
        {
            _field = field;
        }

        internal override IEnumerable<ClientValidationFieldReference> PeerFieldReferences
        {
            get { yield return _field; }
        }

        internal override RuleOperand PrefixedBy(ValidationFieldPath prefix)
        {
            return PeerField(prefix.Append(_field.Path), _field.Shape);
        }

        internal override ValidationRuleExecution ToPlanExecution(
            ValidationRuleActivation activation,
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

        // True only for an unconditional rule. A native-component validation emitter
        // skips conditional rules because EJ2 column rules are always-on.
        internal abstract bool IsAlways { get; }

        internal abstract ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding);

        internal abstract ClientRuleActivation Combine(ClientRuleActivation incoming);

        internal abstract ClientRuleActivation AppendTo(FieldCondition existingCondition);

        internal abstract ClientRuleActivation PrefixedBy(ValidationFieldPath prefix);
    }

    internal sealed class AlwaysClientRuleActivation : ClientRuleActivation
    {
        internal AlwaysClientRuleActivation() { }

        internal override bool IsAlways => true;

        internal override ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
        {
            return ValidationRuleActivation.Always;
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

        internal override bool IsAlways => false;

        internal override ValidationRuleActivation ToPlanActivation(ValidationPlanBinding binding)
        {
            return ValidationRuleActivation.When(binding.ResolveActivationCondition(_condition));
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

        internal static ValidationRangeBounds FromClientLiteral<TValue>(TValue lowerBound, TValue upperBound)
        {
            if (lowerBound == null) throw new ArgumentNullException(nameof(lowerBound));
            if (upperBound == null) throw new ArgumentNullException(nameof(upperBound));

            var lowerLiteral = ClientValidationLiteral.From(lowerBound);
            var upperLiteral = ClientValidationLiteral.From(upperBound);
            if (lowerLiteral.Shape.Equals(upperLiteral.Shape) &&
                lowerLiteral.Value != null &&
                upperLiteral.Value != null)
            {
                return new ValidationRangeBounds(
                    lowerLiteral.Value,
                    upperLiteral.Value,
                    lowerLiteral.Shape);
            }

            throw new ArgumentException(
                "Client validation range bounds must have the same shape. " +
                $"Lower bound is '{lowerLiteral.Shape.Kind}', upper bound is '{upperLiteral.Shape.Kind}'.");
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
