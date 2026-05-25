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

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Projects client-side validation rules from FluentValidation validators.
    /// Unconditional rules are projected for client-side use.
    /// Conditional rules (.When()/.Unless()) are skipped for client projection unless
    /// paired with a ReactiveValidator WhenField() guard.
    /// ReactiveValidator WhenField() conditions are included with a When guard.
    /// </summary>
    public sealed partial class FluentValidationAdapter : IClientValidationProjectionSource
    {
        private readonly Func<Type, IValidator?> _factory;

        public FluentValidationAdapter(Func<Type, IValidator?> factory)
        {
            _factory = factory ?? throw new ArgumentException(
                "A validator factory is required. Pass a function that resolves " +
                "IValidator instances (e.g. from your DI container).", nameof(factory));
        }

        public ClientValidationProjection Project(ClientValidationProjectionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var validator = ResolveRootValidator(request.ValidationSourceType);

            var projection = new FluentValidationProjectionDraft();
            var clientConditions = ClientConditionCatalog.From(validator);
            var rootFrame = ValidatorProjectionFrame.Root(
                ValidationFieldPath.Empty,
                projection,
                _factory,
                clientConditions);

            ProjectFromValidator(validator, rootFrame);

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

        private static void ProjectFromValidator(IValidator validator, ValidatorProjectionFrame frame)
        {
            if (!(validator is IEnumerable<IValidationRule> rules)) return;

            foreach (var rule in rules)
            {
                RuleProjectionScope
                    .For(rule, frame)
                    .Project(rule, frame);
            }
        }

        /// <summary>
        /// Resolves whether a FluentValidation rule can be projected for the browser.
        /// Server-only conditions become a no-op projection scope.
        /// </summary>
        private static RuleProjectionScope ResolveRuleProjectionScope(
            IValidationRule rule,
            ValidatorProjectionFrame frame)
        {
            var ruleHasServerCondition = rule.HasCondition || rule.HasAsyncCondition;
            if (!ruleHasServerCondition)
                return RuleProjectionScope.Unconditional;

            return frame.ClientConditions
                .Find(rule)
                .ToRuleProjectionScope(frame);
        }

        /// <summary>
        /// Handles Include() rules (empty PropertyName) — recurses into the included validator.
        /// </summary>
        private static void ProcessIncludeRule(
            IValidationRule rule,
            ValidatorProjectionFrame frame)
        {
            foreach (IRuleComponent component in rule.Components)
            {
                if (component.Validator is IChildValidatorAdaptor adaptor)
                {
                    var nested = ResolveNestedValidator(frame.Factory, adaptor.ValidatorType);
                    ProjectFromValidator(nested, frame.ForNestedValidator(nested));
                }
            }
        }

        /// <summary>
        /// Iterates rule components, recursing into nested validators and mapping leaf validators.
        /// </summary>
        private static void ProcessComponents(
            IValidationRule rule,
            ValidationRuleTarget target,
            ValidatorProjectionFrame frame,
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
                        ClientRuleProjectionSkipReason.RuleComponentCondition));
                    continue;
                }

                if (component.Validator is IChildValidatorAdaptor adaptor)
                {
                    var nested = ResolveNestedValidator(frame.Factory, adaptor.ValidatorType);
                    ProjectFromValidator(nested, frame.ForNestedValidator(nested, target.FullPath, ruleCondition));
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
            FluentValidationProjectionDraft projection)
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
                mapping.Target.AddProjectedRules(projection, projectedRules);
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
                            ClientRuleProjectionSkipReason.MissingRegexExpression));
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
                        ClientRuleProjectionSkipReason.UnsupportedValidator));
                    break;
            }

            if (projectedRules.Count > 0)
                mapping.Target.AddProjectedRules(projection, projectedRules);
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

        private static object SerializeDateConstraint(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return ValidationDateLiteral.From(value, Shape.Date);
        }

        private static SkippedClientRuleProjection SkippedClientRuleFor(
            ValidationFieldPath fieldPath,
            IPropertyValidator validator,
            ClientRuleProjectionSkipReason reason)
        {
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            return SkippedClientRuleProjection.For(fieldPath, validator.Name, reason);
        }

        private static SkippedClientRuleProjection SkippedClientRuleFor(
            ValidationFieldPath fieldPath,
            IValidationRule rule,
            ClientRuleProjectionSkipReason reason)
        {
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            var validatorName = string.Join(
                ", ",
                rule.Components.Select(component => component.Validator.Name));
            var noComponentNamesWereAvailable = string.IsNullOrWhiteSpace(validatorName);
            if (noComponentNamesWereAvailable)
                validatorName = rule.GetType().Name;

            return SkippedClientRuleProjection.For(fieldPath, validatorName, reason);
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
