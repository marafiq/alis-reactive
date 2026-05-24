using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;
using Alis.Reactive.FluentValidator.Validators;
using ValidationRule = Alis.Reactive.Validation.ValidationRule;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Extracts client-side validation rules from FluentValidation validators.
    /// Unconditional rules are extracted for client-side use.
    /// Conditional rules (.When()/.Unless()) are skipped for client projection unless
    /// paired with a ReactiveValidator WhenField() guard.
    /// ReactiveValidator WhenField() conditions are included with a When guard.
    /// </summary>
    public sealed partial class FluentValidationAdapter : IValidationExtractor
    {
        private readonly Func<Type, IValidator?> _factory;

        public FluentValidationAdapter(Func<Type, IValidator?> factory)
        {
            _factory = factory ?? throw new ArgumentException(
                "A validator factory is required. Pass a function that resolves " +
                "IValidator instances (e.g. from your DI container).", nameof(factory));
        }

        public ValidationExtractionReport Extract(ValidationExtractionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var validator = ResolveRootValidator(request.ValidatorType);

            var projection = new ClientValidationProjectionDraft();
            var clientConditions = ClientConditionCatalog.From(validator);
            var rootFrame = ValidatorExtractionFrame.Root(
                ValidationFieldPath.Empty,
                projection,
                _factory,
                clientConditions);

            ExtractFromValidator(validator, rootFrame);

            projection.EnsurePeerFields();
            return projection.ToReport(request.ValidationContainer);
        }

        private IValidator ResolveRootValidator(Type validatorType)
        {
            if (validatorType == null) throw new ArgumentNullException(nameof(validatorType));
            return ResolveValidator(
                _factory,
                validatorType,
                "validator",
                "Ensure it is registered in the validator factory passed to FluentValidationAdapter.");
        }

        private static void ExtractFromValidator(IValidator validator, ValidatorExtractionFrame frame)
        {
            if (!(validator is IEnumerable<IValidationRule> rules)) return;

            foreach (var rule in rules)
            {
                RuleExtractionScope
                    .For(rule, frame)
                    .Extract(rule, frame);
            }
        }

        /// <summary>
        /// Resolves whether a FluentValidation rule can be extracted for the browser.
        /// Server-only conditions become a no-op extraction scope.
        /// </summary>
        private static RuleExtractionScope ResolveRuleExtractionScope(
            IValidationRule rule,
            ValidatorExtractionFrame frame)
        {
            var ruleHasServerCondition = rule.HasCondition || rule.HasAsyncCondition;
            if (!ruleHasServerCondition)
                return RuleExtractionScope.Unconditional;

            return frame.ClientConditions
                .Find(rule)
                .ToRuleExtractionScope(frame);
        }

        /// <summary>
        /// Handles Include() rules (empty PropertyName) — recurses into the included validator.
        /// </summary>
        private static void ProcessIncludeRule(
            IValidationRule rule,
            ValidatorExtractionFrame frame)
        {
            foreach (IRuleComponent component in rule.Components)
            {
                if (component.Validator is IChildValidatorAdaptor adaptor)
                {
                    var nested = ResolveNestedValidator(frame.Factory, adaptor.ValidatorType);
                    ExtractFromValidator(nested, frame.ForNestedValidator(nested));
                }
            }
        }

        /// <summary>
        /// Iterates rule components, recursing into nested validators and mapping leaf validators.
        /// </summary>
        private static void ProcessComponents(
            IValidationRule rule,
            ValidationRuleTarget target,
            ValidatorExtractionFrame frame,
            ValidationRuleCondition ruleCondition)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            foreach (IRuleComponent component in rule.Components)
            {
                var componentHasComponentCondition = component.HasCondition || component.HasAsyncCondition;
                if (componentHasComponentCondition)
                {
                    frame.Projection.RecordSkippedRule(SkippedClientRuleFor(
                        target.FullPath,
                        component.Validator,
                        ClientRuleExtractionSkipReason.RuleComponentCondition));
                    continue;
                }

                if (component.Validator is IChildValidatorAdaptor adaptor)
                {
                    var nested = ResolveNestedValidator(frame.Factory, adaptor.ValidatorType);
                    ExtractFromValidator(nested, frame.ForNestedValidator(nested, target.FullPath, ruleCondition));
                    continue;
                }

                var effectiveCondition = frame.ParentCondition.Combine(ruleCondition);
                ProjectRuleComponentForBrowser(
                    RuleComponentMapping.For(
                        component,
                        target,
                        effectiveCondition),
                    frame.Projection);
            }
        }

        private static IValidator ResolveNestedValidator(Func<Type, IValidator?> factory, Type validatorType)
        {
            return ResolveValidator(
                factory,
                validatorType,
                "nested validator",
                "Ensure it is registered in the validator factory.");
        }

        private static IValidator ResolveValidator(
            Func<Type, IValidator?> factory,
            Type validatorType,
            string validatorRole,
            string fixGuidance)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (validatorType == null) throw new ArgumentNullException(nameof(validatorType));
            if (validatorRole == null) throw new ArgumentNullException(nameof(validatorRole));
            if (fixGuidance == null) throw new ArgumentNullException(nameof(fixGuidance));

            IValidator? validator;
            try
            {
                validator = factory(validatorType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create {validatorRole} '{validatorType.Name}'. " +
                    fixGuidance, ex);
            }

            if (validator == null)
            {
                throw new InvalidOperationException(
                    $"Validator factory returned null for {validatorRole} '{validatorType.Name}'. " +
                    fixGuidance);
            }

            return validator;
        }

        private static void ProjectRuleComponentForBrowser(
            RuleComponentMapping mapping,
            ClientValidationProjectionDraft projection)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            if (projection == null) throw new ArgumentNullException(nameof(projection));

            var projectedRules = new List<ProjectedClientValidationRule>();
            var validator = mapping.Validator;
            var displayName = mapping.DisplayName;

            if (ClientValidationRuleProjectionCatalog.TryFind(mapping.Component, out var explicitClientRule))
            {
                projectedRules.Add(new ProjectedClientValidationRule(
                    explicitClientRule.Name,
                    mapping.Message.OrDefault(explicitClientRule.MessageFor(displayName).Value),
                    explicitClientRule.DetailsFor(mapping.RuleCondition)));
                projection.AddProjectedRules(mapping.Target.FullPath, projectedRules);
                return;
            }

            switch (validator)
            {
                case INotEmptyValidator _:
                case INotNullValidator _:
                    projectedRules.Add(new ProjectedClientValidationRule(
                        ValidationRuleName.Required,
                        mapping.Message.OrDefault($"'{displayName}' is required."),
                        ValidationRuleDetails.NoOperand(mapping.RuleCondition)));
                    break;

                case IEmptyValidator _:
                    projectedRules.Add(new ProjectedClientValidationRule(
                        ValidationRuleName.Empty,
                        mapping.Message.OrDefault($"'{displayName}' must be empty."),
                        ValidationRuleDetails.NoOperand(mapping.RuleCondition)));
                    break;

                case ILengthValidator lv:
                    MapLengthValidator(lv, mapping, projectedRules);
                    break;

                case IEmailValidator _:
                    projectedRules.Add(new ProjectedClientValidationRule(
                        ValidationRuleName.Email,
                        mapping.Message.OrDefault($"'{displayName}' must be a valid email address."),
                        ValidationRuleDetails.NoOperand(mapping.RuleCondition)));
                    break;

                case IRegularExpressionValidator rv:
                    if (string.IsNullOrEmpty(rv.Expression))
                    {
                        projection.RecordSkippedRule(SkippedClientRuleFor(
                            mapping.Target.FullPath,
                            validator,
                            ClientRuleExtractionSkipReason.MissingRegexExpression));
                    }
                    else
                    {
                        projectedRules.Add(new ProjectedClientValidationRule(
                            ValidationRuleName.Regex,
                            mapping.Message.OrDefault($"'{displayName}' format is invalid."),
                            ValidationRuleDetails.WithConstraint(rv.Expression, mapping.RuleCondition, Shape.None)));
                    }
                    break;

                case FluentValidation.Validators.ICreditCardValidator _:
                    projectedRules.Add(new ProjectedClientValidationRule(
                        ValidationRuleName.CreditCard,
                        mapping.Message.OrDefault($"'{displayName}' must be a valid credit card number."),
                        ValidationRuleDetails.NoOperand(mapping.RuleCondition)));
                    break;

                case IExclusiveBetweenValidator ebv:
                {
                    var defaultMessage = mapping.Message.OrDefault(
                        $"'{displayName}' must be between {ebv.From} and {ebv.To} (exclusive).");
                    MapRangeValidator(
                        ValidationRuleName.ExclusiveRange,
                        ebv.From,
                        ebv.To,
                        defaultMessage,
                        mapping.RuleCondition).AddTo(projectedRules, mapping, projection);
                    break;
                }

                case IBetweenValidator bv:
                {
                    var defaultMessage = mapping.Message.OrDefault(
                        $"'{displayName}' must be between {bv.From} and {bv.To}.");
                    MapRangeValidator(
                        ValidationRuleName.Range,
                        bv.From,
                        bv.To,
                        defaultMessage,
                        mapping.RuleCondition).AddTo(projectedRules, mapping, projection);
                    break;
                }

                case IComparisonValidator cv:
                {
                    MapComparisonValidator(cv, mapping).AddTo(projectedRules, mapping, projection);
                    break;
                }

                default:
                    projection.RecordSkippedRule(SkippedClientRuleFor(
                        mapping.Target.FullPath,
                        validator,
                        ClientRuleExtractionSkipReason.UnsupportedValidator));
                    break;
            }

            if (projectedRules.Count > 0)
                projection.AddProjectedRules(mapping.Target.FullPath, projectedRules);
        }

        private static void MapLengthValidator(
            ILengthValidator lv,
            RuleComponentMapping mapping,
            List<ProjectedClientValidationRule> result)
        {
            if (lv == null) throw new ArgumentNullException(nameof(lv));
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            if (result == null) throw new ArgumentNullException(nameof(result));

            var displayName = mapping.DisplayName;
            if (lv.Min > 0)
            {
                result.Add(new ProjectedClientValidationRule(
                    ValidationRuleName.MinLength,
                    mapping.Message.OrDefault($"'{displayName}' must be at least {lv.Min} characters."),
                    ValidationRuleDetails.WithConstraint(lv.Min, mapping.RuleCondition, Shape.None)));
            }
            if (lv.Max > 0)
            {
                result.Add(new ProjectedClientValidationRule(
                    ValidationRuleName.MaxLength,
                    mapping.Message.OrDefault($"'{displayName}' must be at most {lv.Max} characters."),
                    ValidationRuleDetails.WithConstraint(lv.Max, mapping.RuleCondition, Shape.None)));
            }
        }

        private static ClientRuleProjection MapRangeValidator(
            ValidationRuleName ruleName,
            object? lowerBound,
            object? upperBound,
            ValidationMessage message,
            ValidationRuleCondition ruleCondition)
        {
            if (ruleName == null) throw new ArgumentNullException(nameof(ruleName));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (ruleCondition == null) throw new ArgumentNullException(nameof(ruleCondition));

            return RangeEndpointValues
                .From(lowerBound, upperBound)
                .BuildRule(ruleName, message, ruleCondition);
        }

        private static ClientRuleProjection MapComparisonValidator(
            IComparisonValidator cv,
            RuleComponentMapping mapping)
        {
            if (cv == null) throw new ArgumentNullException(nameof(cv));
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));

            return ComparisonRuleOperands
                .From(cv, mapping.Target)
                .PrefixedBy(mapping.SameObjectPeerPrefix)
                .BuildRule(cv.Comparison, mapping);
        }

        private sealed class ValidationRuleTarget
        {
            private ValidationRuleTarget(
                string propertyName,
                ValidationFieldPath fullPath,
                System.Reflection.MemberInfo? ruleMember)
            {
                PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
                FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
                DisplayName = Humanize(propertyName);
                PeerScope = ValidationPeerFieldScope.ForRuleMember(ruleMember);
            }

            internal string PropertyName { get; }
            internal ValidationFieldPath FullPath { get; }
            internal ValidationFieldPath SameObjectPeerPrefix => FullPath.Parent();
            internal string DisplayName { get; }
            private ValidationPeerFieldScope PeerScope { get; }

            internal ValidationPeerFieldProjection ClassifyPeerMember(System.Reflection.MemberInfo member)
            {
                if (member == null) throw new ArgumentNullException(nameof(member));
                return PeerScope.Classify(member);
            }

            internal static ValidationRuleTarget From(
                ValidationFieldPath prefix,
                IValidationRule rule)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                if (rule == null) throw new ArgumentNullException(nameof(rule));

                return From(prefix, rule.PropertyName, rule.Member);
            }

            private static ValidationRuleTarget From(
                ValidationFieldPath prefix,
                string propertyName,
                System.Reflection.MemberInfo? ruleMember)
            {
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

                return new ValidationRuleTarget(
                    propertyName,
                    prefix.Append(propertyName),
                    ruleMember);
            }
        }

        private sealed class ValidationPeerFieldScope
        {
            private readonly Type? _ownerType;

            private ValidationPeerFieldScope(Type? ownerType)
            {
                _ownerType = ownerType;
            }

            internal static ValidationPeerFieldScope ForRuleMember(System.Reflection.MemberInfo? ruleMember) =>
                new ValidationPeerFieldScope(ruleMember?.DeclaringType);

            internal ValidationPeerFieldProjection Classify(System.Reflection.MemberInfo peerMember)
            {
                if (peerMember == null) throw new ArgumentNullException(nameof(peerMember));
                if (_ownerType == null)
                    return ValidationPeerFieldProjection.SkipClientProjection(ClientRuleExtractionSkipReason.UnknownPeerFieldScope);

                var peerOwnerType = peerMember.DeclaringType;
                if (peerOwnerType == null)
                    return ValidationPeerFieldProjection.SkipClientProjection(ClientRuleExtractionSkipReason.UnknownPeerFieldScope);

                var sameObjectPeer = peerOwnerType.IsAssignableFrom(_ownerType);
                if (sameObjectPeer) return ValidationPeerFieldProjection.SameObjectPeer;

                return ValidationPeerFieldProjection.SkipClientProjection(ClientRuleExtractionSkipReason.CrossObjectPeerComparison);
            }
        }

        private abstract class ValidationPeerFieldProjection
        {
            internal static ValidationPeerFieldProjection SameObjectPeer { get; } =
                new SameObjectPeerFieldProjection();

            internal static ValidationPeerFieldProjection SkipClientProjection(ClientRuleExtractionSkipReason reason) =>
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
            private readonly ClientRuleExtractionSkipReason _reason;

            internal SkippedPeerFieldProjection(ClientRuleExtractionSkipReason reason)
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
                return ClientRuleProjection.SkipClientProjection(ClientRuleExtractionSkipReason.MissingRangeEndpoint);
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

        private abstract class ComparisonRuleOperands
        {
            private protected ComparisonRuleOperands() { }

            internal abstract ComparisonRuleOperands PrefixedBy(ValidationFieldPath prefix);

            internal abstract ClientRuleProjection BuildRule(
                Comparison comparison,
                RuleComponentMapping mapping);

            internal static ComparisonRuleOperands From(
                IComparisonValidator validator,
                ValidationRuleTarget target)
            {
                if (validator == null) throw new ArgumentNullException(nameof(validator));
                if (target == null) throw new ArgumentNullException(nameof(target));

                var peerMember = validator.MemberToCompare;
                if (peerMember == null)
                    return Literal(validator.ValueToCompare);

                return ComparisonPeerField.ToOperands(peerMember, target);
            }

            private static ComparisonRuleOperands Literal(object? value) =>
                new LiteralComparisonRuleOperands(value);
        }

        private abstract class ExtractableComparisonRuleOperands : ComparisonRuleOperands
        {
            internal abstract ValidationRuleName NotEqualRule { get; }

            internal abstract ValidationRuleDetails DetailsFor(ValidationRuleCondition condition);

            internal abstract string Message(
                string displayName,
                string fieldVerb,
                string constraintVerb);

            internal abstract string NotEqualMessage(string displayName);

            internal override ClientRuleProjection BuildRule(
                Comparison comparison,
                RuleComponentMapping mapping)
            {
                if (mapping == null) throw new ArgumentNullException(nameof(mapping));

                var displayName = mapping.DisplayName;
                return comparison switch
                {
                    Comparison.Equal => ClientRule(
                        ValidationRuleName.EqualTo,
                        Message(displayName, "must match", "must equal"),
                        mapping),
                    Comparison.NotEqual => ClientRule(
                        NotEqualRule,
                        NotEqualMessage(displayName),
                        mapping),
                    Comparison.GreaterThanOrEqual => ClientRule(
                        ValidationRuleName.Min,
                        Message(displayName, "must be at least", "must be at least"),
                        mapping),
                    Comparison.LessThanOrEqual => ClientRule(
                        ValidationRuleName.Max,
                        Message(displayName, "must be at most", "must be at most"),
                        mapping),
                    Comparison.GreaterThan => ClientRule(
                        ValidationRuleName.Gt,
                        Message(displayName, "must be greater than", "must be greater than"),
                        mapping),
                    Comparison.LessThan => ClientRule(
                        ValidationRuleName.Lt,
                        Message(displayName, "must be less than", "must be less than"),
                        mapping),
                    _ => ClientRuleProjection.SkipClientProjection(ClientRuleExtractionSkipReason.UnsupportedComparisonOperator)
                };
            }

            private ClientRuleProjection ClientRule(
                ValidationRuleName ruleName,
                string defaultMessage,
                RuleComponentMapping mapping)
            {
                return ClientRuleProjection.Project(new ProjectedClientValidationRule(
                    ruleName,
                    mapping.Message.OrDefault(defaultMessage),
                    DetailsFor(mapping.RuleCondition)));
            }
        }

        private sealed class SkippedComparisonRuleOperands : ComparisonRuleOperands
        {
            private readonly ClientRuleExtractionSkipReason _reason;

            internal SkippedComparisonRuleOperands(ClientRuleExtractionSkipReason reason)
            {
                _reason = reason;
            }

            internal override ComparisonRuleOperands PrefixedBy(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                return this;
            }

            internal override ClientRuleProjection BuildRule(
                Comparison comparison,
                RuleComponentMapping mapping)
            {
                if (mapping == null) throw new ArgumentNullException(nameof(mapping));
                return ClientRuleProjection.SkipClientProjection(_reason);
            }
        }

        private sealed class ComparisonPeerField
        {
            private ComparisonPeerField(ValidationFieldPath path, Shape shape)
            {
                Path = path ?? throw new ArgumentNullException(nameof(path));
                Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            }

            internal ValidationFieldPath Path { get; }
            internal Shape Shape { get; }

            internal static ComparisonPeerField Of(ValidationFieldPath path, Shape shape) =>
                new ComparisonPeerField(path, shape);

            internal static ComparisonRuleOperands ToOperands(
                System.Reflection.MemberInfo member,
                ValidationRuleTarget target)
            {
                if (member == null) throw new ArgumentNullException(nameof(member));
                if (target == null) throw new ArgumentNullException(nameof(target));
                return target.ClassifyPeerMember(member).ToOperands(member);
            }

            internal ComparisonPeerField PrefixedBy(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                return new ComparisonPeerField(prefix.Append(Path), Shape);
            }
        }

        private abstract class ComparisonPeerShape
        {
            private protected ComparisonPeerShape() { }

            internal static ComparisonPeerShape From(System.Reflection.MemberInfo member)
            {
                if (member == null) throw new ArgumentNullException(nameof(member));

                Shape shape;
                if (member is System.Reflection.PropertyInfo property)
                    shape = Shape.FromClrType(property.PropertyType);
                else if (member is System.Reflection.FieldInfo field)
                    shape = Shape.FromClrType(field.FieldType);
                else
                    return UnsupportedComparisonPeerShape.Instance;

                if (shape.IsAny)
                    return UnsupportedComparisonPeerShape.Instance;

                return new DeclaredComparisonPeerShape(shape);
            }

            internal abstract ComparisonRuleOperands ToOperands(ValidationFieldPath path);
        }

        private sealed class UnsupportedComparisonPeerShape : ComparisonPeerShape
        {
            internal static UnsupportedComparisonPeerShape Instance { get; } =
                new UnsupportedComparisonPeerShape();

            private UnsupportedComparisonPeerShape() { }

            internal override ComparisonRuleOperands ToOperands(ValidationFieldPath path)
            {
                if (path == null) throw new ArgumentNullException(nameof(path));
                return new SkippedComparisonRuleOperands(ClientRuleExtractionSkipReason.UnsupportedPeerShape);
            }
        }

        private sealed class DeclaredComparisonPeerShape : ComparisonPeerShape
        {
            private readonly Shape _shape;

            internal DeclaredComparisonPeerShape(Shape shape)
            {
                _shape = shape ?? throw new ArgumentNullException(nameof(shape));
            }

            internal override ComparisonRuleOperands ToOperands(ValidationFieldPath path)
            {
                if (path == null) throw new ArgumentNullException(nameof(path));
                return PeerComparisonRuleOperands.For(ComparisonPeerField.Of(path, _shape));
            }
        }

        private sealed class PeerComparisonRuleOperands : ExtractableComparisonRuleOperands
        {
            private readonly ComparisonPeerField _field;

            private PeerComparisonRuleOperands(ComparisonPeerField field)
            {
                _field = field ?? throw new ArgumentNullException(nameof(field));
            }

            internal static PeerComparisonRuleOperands For(ComparisonPeerField field) =>
                new PeerComparisonRuleOperands(field);

            internal override ValidationRuleName NotEqualRule => ValidationRuleName.NotEqualTo;

            internal override ComparisonRuleOperands PrefixedBy(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                return For(_field.PrefixedBy(prefix));
            }

            internal override ValidationRuleDetails DetailsFor(ValidationRuleCondition condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                return ValidationRuleDetails.WithPeerField(_field.Path, condition, _field.Shape);
            }

            internal override string Message(
                string displayName,
                string fieldVerb,
                string constraintVerb)
            {
                if (displayName == null) throw new ArgumentNullException(nameof(displayName));
                if (fieldVerb == null) throw new ArgumentNullException(nameof(fieldVerb));
                return $"'{displayName}' {fieldVerb} '{Humanize(_field.Path)}'.";
            }

            internal override string NotEqualMessage(string displayName)
            {
                if (displayName == null) throw new ArgumentNullException(nameof(displayName));
                return $"'{displayName}' must not match '{Humanize(_field.Path)}'.";
            }
        }

        private sealed class LiteralComparisonRuleOperands : ExtractableComparisonRuleOperands
        {
            private readonly object? _constraint;
            private readonly Shape _shape;

            internal LiteralComparisonRuleOperands(object? value)
            {
                var literal = ComparisonLiteralConstraint.From(value);
                _constraint = literal.Value;
                _shape = literal.Shape;
            }

            internal override ValidationRuleName NotEqualRule => ValidationRuleName.NotEqual;

            internal override ComparisonRuleOperands PrefixedBy(ValidationFieldPath prefix)
            {
                if (prefix == null) throw new ArgumentNullException(nameof(prefix));
                return this;
            }

            internal override ValidationRuleDetails DetailsFor(ValidationRuleCondition condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                return ValidationRuleDetails.WithConstraint(_constraint, condition, _shape);
            }

            internal override string Message(
                string displayName,
                string fieldVerb,
                string constraintVerb)
            {
                if (displayName == null) throw new ArgumentNullException(nameof(displayName));
                if (constraintVerb == null) throw new ArgumentNullException(nameof(constraintVerb));
                return $"'{displayName}' {constraintVerb} {_constraint}.";
            }

            internal override string NotEqualMessage(string displayName)
            {
                if (displayName == null) throw new ArgumentNullException(nameof(displayName));
                return $"'{displayName}' must not equal '{_constraint}'.";
            }
        }

        private sealed class ComparisonLiteralConstraint
        {
            private ComparisonLiteralConstraint(object? value, Shape shape)
            {
                Value = value;
                Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            }

            internal object? Value { get; }
            internal Shape Shape { get; }

            internal static ComparisonLiteralConstraint From(object? value)
            {
                if (value == null)
                    return new ComparisonLiteralConstraint(null, Shape.None);

                var shape = Shape.FromClrType(value.GetType());
                var serialized = SerializeForPlan(value, shape);
                return new ComparisonLiteralConstraint(serialized, shape);
            }

            private static object SerializeForPlan(object value, Shape shape)
            {
                var shouldSerializeDateLiteral = shape == Shape.Date;
                if (shouldSerializeDateLiteral)
                    return SerializeDateConstraint(value);

                return value;
            }
        }

        private static object SerializeDateConstraint(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return ValidationDateLiteral.From(value, Shape.Date);
        }

        private static SkippedClientRuleExtraction SkippedClientRuleFor(
            ValidationFieldPath fieldPath,
            IPropertyValidator validator,
            ClientRuleExtractionSkipReason reason)
        {
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            return SkippedClientRuleExtraction.For(fieldPath, validator.Name, reason);
        }

        private static SkippedClientRuleExtraction SkippedClientRuleFor(
            ValidationFieldPath fieldPath,
            IValidationRule rule,
            ClientRuleExtractionSkipReason reason)
        {
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            var validatorName = string.Join(
                ", ",
                rule.Components.Select(component => component.Validator.Name));
            var noComponentNamesWereAvailable = string.IsNullOrWhiteSpace(validatorName);
            if (noComponentNamesWereAvailable)
                validatorName = rule.GetType().Name;

            return SkippedClientRuleExtraction.For(fieldPath, validatorName, reason);
        }

        private static string Humanize(ValidationFieldPath fieldPath) => Humanize(fieldPath.Value);

        private static string Humanize(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return propertyName;
            var result = new StringBuilder();
            foreach (var c in propertyName)
            {
                var shouldInsertWordBoundary = char.IsUpper(c) && result.Length > 0;
                if (shouldInsertWordBoundary)
                    result.Append(' ');
                result.Append(result.Length == 0 ? char.ToUpper(c) : c);
            }
            return result.ToString();
        }

    }
}
