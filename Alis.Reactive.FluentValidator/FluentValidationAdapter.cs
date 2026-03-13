using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Extracts client-side validation rules from FluentValidation validators.
    /// Unconditional rules are extracted for client-side use.
    /// Conditional rules (.When()/.Unless()) are skipped (server-side only).
    /// ReactiveValidator WhenField() conditions are included with a When guard.
    /// </summary>
    public sealed class FluentValidationAdapter : IValidationExtractor
    {
        private readonly Func<Type, IValidator?> _factory;

        public FluentValidationAdapter(Func<Type, IValidator?> factory)
        {
            _factory = factory ?? throw new ArgumentException(
                "A validator factory is required. Pass a function that resolves " +
                "IValidator instances (e.g. from your DI container).", nameof(factory));
        }

        /// <summary>
        /// Extract client rules from the given validator type for a form.
        /// Returns null if no extractable rules are found.
        /// Fields carry only fieldName + rules. Runtime enriches component info from plan.components.
        /// </summary>
        public ValidationDescriptor? ExtractRules(Type validatorType, string formId)
        {
            var validator = _factory(validatorType);
            if (validator == null) return null;

            // Intermediate: property path → ordered list of (ruleType, message, constraint)
            var fieldRules = new Dictionary<string, List<ExtractedRule>>();

            // Read client conditions if validator extends ReactiveValidator<T>
            IReadOnlyDictionary<IValidationRule, ValidationCondition>? clientConditions = null;
            if (validator is IClientConditionSource source)
            {
                clientConditions = source.ClientConditions;
            }

            ExtractFromValidator(validator, "", fieldRules, _factory, clientConditions);

            // Build fields
            var fields = new List<ValidationField>();
            foreach (var kvp in fieldRules)
            {
                var propertyPath = kvp.Key;
                var rules = new List<ValidationRule>();
                foreach (var er in kvp.Value)
                {
                    rules.Add(new ValidationRule(er.Rule, er.Message, er.Constraint, er.When));
                }
                fields.Add(new ValidationField(propertyPath, rules));
            }

            if (fields.Count == 0) return null;

            return new ValidationDescriptor(formId, fields);
        }

        private static void ExtractFromValidator(
            IValidator validator,
            string prefix,
            Dictionary<string, List<ExtractedRule>> fieldRules,
            Func<Type, IValidator?> factory,
            IReadOnlyDictionary<IValidationRule, ValidationCondition>? clientConditions = null)
        {
            if (!(validator is IEnumerable<IValidationRule> rules)) return;

            foreach (var rule in rules)
            {
                var propertyName = rule.PropertyName;

                // Check if this conditional rule was registered via WhenField()
                ValidationCondition? ruleCondition = null;
                if (rule.HasCondition || rule.HasAsyncCondition)
                {
                    if (clientConditions != null && clientConditions.TryGetValue(rule, out var cc))
                    {
                        // Apply prefix to condition field for nested validators
                        var condField = string.IsNullOrEmpty(prefix)
                            ? cc.Field
                            : prefix + "." + cc.Field;
                        ruleCondition = new ValidationCondition(condField, cc.Op, cc.Value);

                        // Ensure condition source field is in the descriptor (runtime needs it for value reading)
                        var condSourcePath = condField;
                        if (!fieldRules.ContainsKey(condSourcePath))
                        {
                            fieldRules[condSourcePath] = new List<ExtractedRule>();
                        }
                    }
                    else
                    {
                        continue; // Server-only .When() — skip
                    }
                }

                // Include() rules have empty PropertyName — recurse with same prefix
                if (string.IsNullOrEmpty(propertyName))
                {
                    foreach (IRuleComponent component in rule.Components)
                    {
                        if (component.Validator is IChildValidatorAdaptor adaptor)
                        {
                            var nested = ResolveNestedValidator(factory, adaptor.ValidatorType);
                            var nestedConditions = (nested as IClientConditionSource)?.ClientConditions;
                            ExtractFromValidator(nested, prefix, fieldRules, factory, nestedConditions);
                        }
                    }
                    continue;
                }

                var fullPath = string.IsNullOrEmpty(prefix)
                    ? propertyName
                    : prefix + "." + propertyName;

                foreach (IRuleComponent component in rule.Components)
                {
                    // Skip conditional components
                    if (component.HasCondition || component.HasAsyncCondition) continue;

                    // Handle nested validators recursively
                    if (component.Validator is IChildValidatorAdaptor adaptor)
                    {
                        var nested = ResolveNestedValidator(factory, adaptor.ValidatorType);
                        var nestedConditions = (nested as IClientConditionSource)?.ClientConditions;
                        ExtractFromValidator(nested, fullPath, fieldRules, factory, nestedConditions);
                        continue;
                    }

                    var extracted = MapComponent(component, propertyName, ruleCondition);
                    if (extracted.Count > 0)
                    {
                        if (!fieldRules.TryGetValue(fullPath, out var list))
                        {
                            list = new List<ExtractedRule>();
                            fieldRules[fullPath] = list;
                        }
                        list.AddRange(extracted);
                    }
                }
            }
        }

        private static IValidator ResolveNestedValidator(Func<Type, IValidator?> factory, Type validatorType)
        {
            IValidator? nested;
            try
            {
                nested = factory(validatorType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create nested validator '{validatorType.Name}'. " +
                    $"Ensure it is registered in the validator factory.", ex);
            }

            if (nested == null)
            {
                throw new InvalidOperationException(
                    $"Validator factory returned null for nested validator '{validatorType.Name}'. " +
                    $"Ensure it is registered in the validator factory.");
            }

            return nested;
        }

        private static List<ExtractedRule> MapComponent(
            IRuleComponent component, string propertyName, ValidationCondition? ruleCondition = null)
        {
            var result = new List<ExtractedRule>();
            var validator = component.Validator;
            var displayName = Humanize(propertyName);
            // GetUnformattedErrorMessage() returns FV's template (e.g. "'{PropertyName}' must not be empty.")
            // even when no .WithMessage() was set. Only treat it as a custom message if it does NOT
            // contain FV placeholder tokens like {PropertyName}.
            var rawMsg = component.GetUnformattedErrorMessage();
            var customMsg = !string.IsNullOrEmpty(rawMsg) && !rawMsg.Contains("{")
                ? rawMsg
                : null;

            switch (validator)
            {
                case INotEmptyValidator _:
                case INotNullValidator _:
                    result.Add(new ExtractedRule(
                        "required",
                        customMsg ?? $"'{displayName}' is required.",
                        null, ruleCondition));
                    break;

                case ILengthValidator lv:
                {
                    if (lv.Min > 0)
                    {
                        result.Add(new ExtractedRule(
                            "minLength",
                            customMsg ?? $"'{displayName}' must be at least {lv.Min} characters.",
                            lv.Min, ruleCondition));
                    }
                    if (lv.Max > 0)
                    {
                        result.Add(new ExtractedRule(
                            "maxLength",
                            customMsg ?? $"'{displayName}' must be at most {lv.Max} characters.",
                            lv.Max, ruleCondition));
                    }
                    break;
                }

                case IEmailValidator _:
                    result.Add(new ExtractedRule(
                        "email",
                        customMsg ?? $"'{displayName}' must be a valid email address.",
                        null, ruleCondition));
                    break;

                case IRegularExpressionValidator rv:
                    if (!string.IsNullOrEmpty(rv.Expression))
                    {
                        result.Add(new ExtractedRule(
                            "regex",
                            customMsg ?? $"'{displayName}' format is invalid.",
                            rv.Expression, ruleCondition));
                    }
                    break;

                case IBetweenValidator bv:
                    result.Add(new ExtractedRule(
                        "range",
                        customMsg ?? $"'{displayName}' must be between {bv.From} and {bv.To}.",
                        new object[] { bv.From, bv.To }, ruleCondition));
                    break;

                case IComparisonValidator cv:
                    if (cv.Comparison == Comparison.Equal && cv.MemberToCompare != null)
                    {
                        result.Add(new ExtractedRule(
                            "equalTo",
                            customMsg ?? $"'{displayName}' must match '{Humanize(cv.MemberToCompare.Name)}'.",
                            cv.MemberToCompare.Name, ruleCondition));
                    }
                    else if (cv.Comparison == Comparison.GreaterThanOrEqual)
                    {
                        result.Add(new ExtractedRule(
                            "min",
                            customMsg ?? $"'{displayName}' must be at least {cv.ValueToCompare}.",
                            cv.ValueToCompare, ruleCondition));
                    }
                    else if (cv.Comparison == Comparison.LessThanOrEqual)
                    {
                        result.Add(new ExtractedRule(
                            "max",
                            customMsg ?? $"'{displayName}' must be at most {cv.ValueToCompare}.",
                            cv.ValueToCompare, ruleCondition));
                    }
                    break;
            }

            return result;
        }

        private static string Humanize(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return propertyName;
            var result = new StringBuilder();
            foreach (var c in propertyName)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result.Append(' ');
                result.Append(result.Length == 0 ? char.ToUpper(c) : c);
            }
            return result.ToString();
        }

        private sealed class ExtractedRule
        {
            public string Rule { get; }
            public string Message { get; }
            public object? Constraint { get; }
            public ValidationCondition? When { get; }

            public ExtractedRule(string rule, string message, object? constraint, ValidationCondition? when = null)
            {
                Rule = rule;
                Message = message;
                Constraint = constraint;
                When = when;
            }
        }
    }
}
