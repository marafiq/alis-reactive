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
    /// Explicit conditional rules from IConditionalRuleProvider are included with a When guard.
    /// </summary>
    public sealed class FluentValidationAdapter : IValidationExtractor
    {
        /// <summary>
        /// Extract client rules from the given validator type for a form.
        /// Returns null if no extractable rules are found.
        /// </summary>
        public ValidationDescriptor? ExtractRules(Type validatorType, string formId)
        {
            var validator = Activator.CreateInstance(validatorType) as IValidator;
            if (validator == null) return null;

            // Intermediate: property path → ordered list of (ruleType, message, constraint)
            var fieldRules = new Dictionary<string, List<ExtractedRule>>();

            ExtractFromValidator(validator, "", fieldRules);

            // Build fields
            var fields = new List<ValidationField>();
            foreach (var kvp in fieldRules)
            {
                var propertyPath = kvp.Key;
                var elementId = propertyPath.Replace(".", "_");
                var errorId = "err_" + elementId;

                var rules = new List<ValidationRule>();
                foreach (var er in kvp.Value)
                {
                    rules.Add(new ValidationRule(er.Rule, er.Message, er.Constraint, er.When));
                }

                fields.Add(new ValidationField(
                    elementId,
                    propertyPath,
                    errorId,
                    "native",
                    null,
                    rules));
            }

            // Merge conditional rules from IConditionalRuleProvider
            if (validator is IConditionalRuleProvider provider)
            {
                foreach (var cr in provider.GetConditionalRules())
                {
                    var field = FindOrCreateField(fields, cr.PropertyName);
                    field.Rules.Add(new ValidationRule(cr.Rule, cr.Message, cr.Constraint, cr.When));
                }
            }

            if (fields.Count == 0) return null;

            return new ValidationDescriptor(formId, fields);
        }

        private static ValidationField FindOrCreateField(List<ValidationField> fields, string propertyName)
        {
            foreach (var f in fields)
            {
                if (f.FieldName == propertyName) return f;
            }

            var elementId = propertyName.Replace(".", "_");
            var field = new ValidationField(
                elementId,
                propertyName,
                "err_" + elementId,
                "native",
                null,
                new List<ValidationRule>());
            fields.Add(field);
            return field;
        }

        private static void ExtractFromValidator(
            IValidator validator,
            string prefix,
            Dictionary<string, List<ExtractedRule>> fieldRules)
        {
            if (!(validator is IEnumerable<IValidationRule> rules)) return;

            foreach (var rule in rules)
            {
                var propertyName = rule.PropertyName;
                if (string.IsNullOrEmpty(propertyName)) continue;

                // Skip rules with conditions — these are server-side only
                if (rule.HasCondition || rule.HasAsyncCondition) continue;

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
                        try
                        {
                            var nested = Activator.CreateInstance(adaptor.ValidatorType) as IValidator;
                            if (nested != null)
                            {
                                ExtractFromValidator(nested, fullPath, fieldRules);
                            }
                        }
                        catch
                        {
                            // Skip if nested validator can't be instantiated
                        }
                        continue;
                    }

                    var extracted = MapComponent(component, propertyName);
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

        private static List<ExtractedRule> MapComponent(IRuleComponent component, string propertyName)
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
                        null));
                    break;

                case ILengthValidator lv:
                {
                    // Length(min, max) produces a single validator with both values.
                    // MinimumLength(n) → Min=n, Max=-1. MaximumLength(n) → Min=0, Max=n.
                    if (lv.Min > 0)
                    {
                        result.Add(new ExtractedRule(
                            "minLength",
                            customMsg ?? $"'{displayName}' must be at least {lv.Min} characters.",
                            lv.Min));
                    }
                    if (lv.Max > 0)
                    {
                        result.Add(new ExtractedRule(
                            "maxLength",
                            customMsg ?? $"'{displayName}' must be at most {lv.Max} characters.",
                            lv.Max));
                    }
                    break;
                }

                case IEmailValidator _:
                    result.Add(new ExtractedRule(
                        "email",
                        customMsg ?? $"'{displayName}' must be a valid email address.",
                        null));
                    break;

                case IRegularExpressionValidator rv:
                    if (!string.IsNullOrEmpty(rv.Expression))
                    {
                        result.Add(new ExtractedRule(
                            "regex",
                            customMsg ?? $"'{displayName}' format is invalid.",
                            rv.Expression));
                    }
                    break;

                case IBetweenValidator bv:
                    result.Add(new ExtractedRule(
                        "range",
                        customMsg ?? $"'{displayName}' must be between {bv.From} and {bv.To}.",
                        new object[] { bv.From, bv.To }));
                    break;

                case IComparisonValidator cv:
                    if (cv.Comparison == Comparison.GreaterThanOrEqual)
                    {
                        result.Add(new ExtractedRule(
                            "min",
                            customMsg ?? $"'{displayName}' must be at least {cv.ValueToCompare}.",
                            cv.ValueToCompare));
                    }
                    else if (cv.Comparison == Comparison.LessThanOrEqual)
                    {
                        result.Add(new ExtractedRule(
                            "max",
                            customMsg ?? $"'{displayName}' must be at most {cv.ValueToCompare}.",
                            cv.ValueToCompare));
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
