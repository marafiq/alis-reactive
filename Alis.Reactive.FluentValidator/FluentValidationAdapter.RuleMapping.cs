using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    public sealed partial class FluentValidationAdapter
    {
        private sealed class ValidationRuleTarget
        {
            private ValidationRuleTarget(
                string propertyName,
                ValidationFieldPath fullPath,
                ClientValidationFieldReference field,
                ValidationPeerFieldScope peerScope)
            {
                PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
                FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
                Field = field ?? throw new ArgumentNullException(nameof(field));
                DisplayName = Humanize(propertyName);
                PeerScope = peerScope ?? throw new ArgumentNullException(nameof(peerScope));
            }

            internal string PropertyName { get; }
            internal ValidationFieldPath FullPath { get; }
            private ClientValidationFieldReference Field { get; }
            internal ValidationFieldPath SameObjectPeerPrefix => FullPath.Parent();
            internal string DisplayName { get; }
            private ValidationPeerFieldScope PeerScope { get; }

            internal ValidationPeerFieldProjection ClassifyPeerMember(System.Reflection.MemberInfo member)
            {
                if (member == null) throw new ArgumentNullException(nameof(member));
                return PeerScope.Classify(member);
            }

            internal void AddProjectedRules(
                FluentValidationProjectionDraft projection,
                IEnumerable<ProjectedClientValidationRule> rules)
            {
                if (projection == null) throw new ArgumentNullException(nameof(projection));
                if (rules == null) throw new ArgumentNullException(nameof(rules));
                projection.AddProjectedRules(Field, rules);
            }

            internal static ValidationRuleTarget From(
                ValidationFieldPath prefix,
                IValidationRule rule)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                if (rule == null) throw new ArgumentNullException(nameof(rule));

                var fullPath = prefix.Append(rule.PropertyName);
                return new ValidationRuleTarget(
                    rule.PropertyName,
                    fullPath,
                    ClientValidationFieldReference.Of(fullPath, Shape.FromClrType(rule.TypeToValidate)),
                    ValidationPeerFieldScope.ForRule(rule));
            }
        }

        private sealed class ValidationPeerFieldScope
        {
            private readonly Type? _ownerType;

            private ValidationPeerFieldScope(Type? ownerType)
            {
                _ownerType = ownerType;
            }

            internal static ValidationPeerFieldScope ForRule(IValidationRule rule)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));

                return new ValidationPeerFieldScope(rule.Member?.DeclaringType);
            }

            internal ValidationPeerFieldProjection Classify(System.Reflection.MemberInfo peerMember)
            {
                if (peerMember == null) throw new ArgumentNullException(nameof(peerMember));
                if (_ownerType == null)
                    return ValidationPeerFieldProjection.SkipClientProjection(ClientRuleProjectionSkipReason.UnknownPeerFieldScope);

                var peerOwnerType = peerMember.DeclaringType;
                if (peerOwnerType == null)
                    return ValidationPeerFieldProjection.SkipClientProjection(ClientRuleProjectionSkipReason.UnknownPeerFieldScope);

                var sameObjectPeer = peerOwnerType.IsAssignableFrom(_ownerType);
                if (sameObjectPeer) return ValidationPeerFieldProjection.SameObjectPeer;

                return ValidationPeerFieldProjection.SkipClientProjection(ClientRuleProjectionSkipReason.CrossObjectPeerComparison);
            }
        }

        private abstract class ValidationPeerFieldProjection
        {
            internal static ValidationPeerFieldProjection SameObjectPeer { get; } =
                new SameObjectPeerFieldProjection();

            internal static ValidationPeerFieldProjection SkipClientProjection(ClientRuleProjectionSkipReason reason) =>
                new SkippedPeerFieldProjection(reason);

            internal abstract ComparisonRuleOperands ToOperands(System.Reflection.MemberInfo member);
        }

        private sealed class SameObjectPeerFieldProjection : ValidationPeerFieldProjection
        {
            internal override ComparisonRuleOperands ToOperands(System.Reflection.MemberInfo member)
            {
                if (member == null) throw new ArgumentNullException(nameof(member));

                return ComparisonPeerShape
                    .From(member)
                    .ToOperands(ValidationFieldPath.Of(member.Name));
            }
        }

        private sealed class SkippedPeerFieldProjection : ValidationPeerFieldProjection
        {
            private readonly ClientRuleProjectionSkipReason _reason;

            internal SkippedPeerFieldProjection(ClientRuleProjectionSkipReason reason)
            {
                _reason = reason;
            }

            internal override ComparisonRuleOperands ToOperands(System.Reflection.MemberInfo member)
            {
                if (member == null) throw new ArgumentNullException(nameof(member));
                return new SkippedComparisonRuleOperands(_reason);
            }
        }

        private sealed class RuleComponentMapping
        {
            private readonly ValidationRuleTarget _target;

            private RuleComponentMapping(
                IRuleComponent component,
                ValidationRuleTarget target,
                ValidationRuleCondition ruleCondition)
            {
                Component = component ?? throw new ArgumentNullException(nameof(component));
                _target = target ?? throw new ArgumentNullException(nameof(target));
                RuleCondition = ruleCondition ?? throw new ArgumentNullException(nameof(ruleCondition));
                Message = RuleMessage.From(component);
            }

            internal IRuleComponent Component { get; }
            internal ValidationRuleTarget Target => _target;
            internal string PropertyName => _target.PropertyName;
            internal ValidationFieldPath SameObjectPeerPrefix => _target.SameObjectPeerPrefix;
            internal ValidationRuleCondition RuleCondition { get; }
            internal string DisplayName => _target.DisplayName;
            internal RuleMessage Message { get; }
            internal IPropertyValidator Validator => Component.Validator;

            internal static RuleComponentMapping For(
                IRuleComponent component,
                ValidationRuleTarget target,
                ValidationRuleCondition ruleCondition) =>
                new RuleComponentMapping(component, target, ruleCondition);
        }

        private abstract class RuleMessage
        {
            private protected RuleMessage() { }

            internal static RuleMessage From(IRuleComponent component)
            {
                if (component == null) throw new ArgumentNullException(nameof(component));

                // GetUnformattedErrorMessage() returns FV's template even when no
                // .WithMessage() was set. Template placeholders mean "use our default".
                var rawMessage = component.GetUnformattedErrorMessage();
                var messageIsCustomText = !string.IsNullOrEmpty(rawMessage) && !rawMessage.Contains('{');
                if (messageIsCustomText)
                    return new CustomRuleMessage(rawMessage);

                return DefaultRuleMessage.Instance;
            }

            internal abstract ValidationMessage OrDefault(string defaultMessage);

            private sealed class DefaultRuleMessage : RuleMessage
            {
                internal static DefaultRuleMessage Instance { get; } = new DefaultRuleMessage();

                private DefaultRuleMessage() { }

                internal override ValidationMessage OrDefault(string defaultMessage)
                {
                    if (defaultMessage == null) throw new ArgumentNullException(nameof(defaultMessage));
                    return ValidationMessage.Of(defaultMessage);
                }
            }

            private sealed class CustomRuleMessage : RuleMessage
            {
                private readonly string _message;

                internal CustomRuleMessage(string message)
                {
                    _message = message ?? throw new ArgumentNullException(nameof(message));
                }

                internal override ValidationMessage OrDefault(string defaultMessage)
                {
                    if (defaultMessage == null) throw new ArgumentNullException(nameof(defaultMessage));
                    return ValidationMessage.Of(_message);
                }
            }
        }
    }
}
