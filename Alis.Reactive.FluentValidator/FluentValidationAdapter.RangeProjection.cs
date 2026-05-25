using System;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    public sealed partial class FluentValidationAdapter
    {
        private abstract class RangeEndpointValues
        {
            private protected RangeEndpointValues() { }

            internal static RangeEndpointValues From(object? lowerBound, object? upperBound)
            {
                if (lowerBound == null || upperBound == null)
                    return MissingRangeEndpointValues.Instance;

                return new CompleteRangeEndpointValues(lowerBound, upperBound);
            }

            internal abstract ClientRuleProjection BuildRule(
                ValidationRuleName ruleName,
                ValidationMessage message,
                ValidationRuleCondition ruleCondition);
        }

        private sealed class MissingRangeEndpointValues : RangeEndpointValues
        {
            internal static MissingRangeEndpointValues Instance { get; } =
                new MissingRangeEndpointValues();

            private MissingRangeEndpointValues() { }

            internal override ClientRuleProjection BuildRule(
                ValidationRuleName ruleName,
                ValidationMessage message,
                ValidationRuleCondition ruleCondition)
            {
                if (ruleName == null) throw new ArgumentNullException(nameof(ruleName));
                if (message == null) throw new ArgumentNullException(nameof(message));
                if (ruleCondition == null) throw new ArgumentNullException(nameof(ruleCondition));
                return ClientRuleProjection.SkipClientProjection(ClientRuleProjectionSkipReason.MissingRangeEndpoint);
            }
        }

        private sealed class CompleteRangeEndpointValues : RangeEndpointValues
        {
            internal CompleteRangeEndpointValues(object lowerBound, object upperBound)
            {
                LowerBound = lowerBound ?? throw new ArgumentNullException(nameof(lowerBound));
                UpperBound = upperBound ?? throw new ArgumentNullException(nameof(upperBound));
            }

            internal object LowerBound { get; }
            internal object UpperBound { get; }

            internal ValidationRangeBounds ToValidationRangeBounds()
            {
                var shape = Shape.FromClrType(LowerBound.GetType());
                var lowerBound = SerializeEndpoint(LowerBound, shape);
                var upperBound = SerializeEndpoint(UpperBound, shape);
                return ValidationRangeBounds.Between(lowerBound, upperBound, shape);
            }

            internal override ClientRuleProjection BuildRule(
                ValidationRuleName ruleName,
                ValidationMessage message,
                ValidationRuleCondition ruleCondition)
            {
                if (ruleName == null) throw new ArgumentNullException(nameof(ruleName));
                if (message == null) throw new ArgumentNullException(nameof(message));
                if (ruleCondition == null) throw new ArgumentNullException(nameof(ruleCondition));

                var bounds = ToValidationRangeBounds();
                return ClientRuleProjection.Project(new ProjectedClientValidationRule(
                    ruleName,
                    message,
                    ValidationRuleDetails.WithConstraint(
                        ValidationConstraint.InclusiveRange(bounds),
                        ruleCondition,
                        bounds.Shape)));
            }

            private static object SerializeEndpoint(object endpoint, Shape shape)
            {
                if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
                if (shape == null) throw new ArgumentNullException(nameof(shape));

                var endpointUsesDateShape = shape == Shape.Date;
                if (endpointUsesDateShape)
                    return SerializeDateConstraint(endpoint);

                return endpoint;
            }
        }
    }
}
