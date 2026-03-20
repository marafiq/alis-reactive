using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Alis.Reactive.Validation;
using Alis.Reactive.FluentValidator.Validators;

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

            // Ensure cross-property peer fields are in the descriptor (runtime needs them for value reading)
            var peerFields = new HashSet<string>();
            foreach (var kvp in fieldRules)
            {
                foreach (var er in kvp.Value)
                {
                    if (!string.IsNullOrEmpty(er.Field) && !fieldRules.ContainsKey(er.Field))
                        peerFields.Add(er.Field);
                }
            }
            foreach (var peerField in peerFields)
                fieldRules[peerField] = new List<ExtractedRule>();

            // Build fields
            var fields = new List<ValidationField>();
            foreach (var kvp in fieldRules)
            {
                var propertyPath = kvp.Key;
                var rules = new List<ValidationRule>();
                foreach (var er in kvp.Value)
                {
                    rules.Add(new ValidationRule(er.Rule, er.Message, er.Constraint, er.When, er.Field, er.CoerceAs));
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
            IReadOnlyDictionary<IValidationRule, ValidationCondition>? clientConditions = null,
            ValidationCondition? parentCondition = null)
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

                    // Handle nested validators recursively — propagate parent condition
                    if (component.Validator is IChildValidatorAdaptor adaptor)
                    {
                        var nested = ResolveNestedValidator(factory, adaptor.ValidatorType);
                        var nestedConditions = (nested as IClientConditionSource)?.ClientConditions;
                        ExtractFromValidator(nested, fullPath, fieldRules, factory, nestedConditions, ruleCondition);
                        continue;
                    }

                    var extracted = MapComponent(component, propertyName, ruleCondition ?? parentCondition);
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

                case IEmptyValidator _:
                    result.Add(new ExtractedRule(
                        "empty",
                        customMsg ?? $"'{displayName}' must be empty.",
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

                case FluentValidation.Validators.ICreditCardValidator _:
                    result.Add(new ExtractedRule(
                        "creditCard",
                        customMsg ?? $"'{displayName}' must be a valid credit card number.",
                        null, ruleCondition));
                    break;

                case IExclusiveBetweenValidator ebv:
                {
                    var coerceAs = InferCoerceAs(ebv.From?.GetType());
                    var from = coerceAs == "date" ? SerializeDateConstraint(ebv.From!) : ebv.From;
                    var to = coerceAs == "date" ? SerializeDateConstraint(ebv.To!) : ebv.To;
                    result.Add(new ExtractedRule(
                        "exclusiveRange",
                        customMsg ?? $"'{displayName}' must be between {ebv.From} and {ebv.To} (exclusive).",
                        new object[] { from!, to! }, ruleCondition, field: null, coerceAs: coerceAs));
                    break;
                }

                case IBetweenValidator bv:
                {
                    var coerceAs = InferCoerceAs(bv.From?.GetType());
                    var from = coerceAs == "date" ? SerializeDateConstraint(bv.From!) : bv.From;
                    var to = coerceAs == "date" ? SerializeDateConstraint(bv.To!) : bv.To;
                    result.Add(new ExtractedRule(
                        "range",
                        customMsg ?? $"'{displayName}' must be between {bv.From} and {bv.To}.",
                        new object[] { from!, to! }, ruleCondition, field: null, coerceAs: coerceAs));
                    break;
                }

                case IComparisonValidator cv:
                {
                    Type? propertyType = null;
                    string? field = null;
                    object? constraint = null;

                    if (cv.MemberToCompare != null)
                    {
                        field = cv.MemberToCompare.Name;
                        if (cv.MemberToCompare is System.Reflection.PropertyInfo pi)
                            propertyType = pi.PropertyType;
                        else if (cv.MemberToCompare is System.Reflection.FieldInfo fi)
                            propertyType = fi.FieldType;
                    }
                    else
                    {
                        constraint = cv.ValueToCompare;
                        propertyType = cv.ValueToCompare?.GetType();
                    }

                    var coerceAs = InferCoerceAs(propertyType);
                    if (coerceAs == "date" && constraint != null)
                        constraint = SerializeDateConstraint(constraint);

                    string ruleType;
                    string defaultMsg;

                    switch (cv.Comparison)
                    {
                        case Comparison.Equal:
                            ruleType = "equalTo";
                            defaultMsg = field != null
                                ? $"'{displayName}' must match '{Humanize(field)}'."
                                : $"'{displayName}' must equal {constraint}.";
                            break;
                        case Comparison.NotEqual:
                            if (field != null)
                            {
                                ruleType = "notEqualTo";
                                defaultMsg = $"'{displayName}' must not match '{Humanize(field)}'.";
                            }
                            else
                            {
                                ruleType = "notEqual";
                                defaultMsg = $"'{displayName}' must not equal '{constraint}'.";
                            }
                            break;
                        case Comparison.GreaterThanOrEqual:
                            ruleType = "min";
                            defaultMsg = field != null
                                ? $"'{displayName}' must be at least '{Humanize(field)}'."
                                : $"'{displayName}' must be at least {constraint}.";
                            break;
                        case Comparison.LessThanOrEqual:
                            ruleType = "max";
                            defaultMsg = field != null
                                ? $"'{displayName}' must be at most '{Humanize(field)}'."
                                : $"'{displayName}' must be at most {constraint}.";
                            break;
                        case Comparison.GreaterThan:
                            ruleType = "gt";
                            defaultMsg = field != null
                                ? $"'{displayName}' must be greater than '{Humanize(field)}'."
                                : $"'{displayName}' must be greater than {constraint}.";
                            break;
                        case Comparison.LessThan:
                            ruleType = "lt";
                            defaultMsg = field != null
                                ? $"'{displayName}' must be less than '{Humanize(field)}'."
                                : $"'{displayName}' must be less than {constraint}.";
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Unknown Comparison type '{cv.Comparison}' on property '{propertyName}'. " +
                                $"This FluentValidation comparison is not supported for client-side extraction.");
                    }

                    result.Add(new ExtractedRule(ruleType, customMsg ?? defaultMsg, constraint, ruleCondition, field, coerceAs));
                    break;
                }
            }

            return result;
        }

        private static string? InferCoerceAs(Type? propertyType)
        {
            if (propertyType == null) return null;
            var t = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (t == typeof(decimal) || t == typeof(int) || t == typeof(long) ||
                t == typeof(double) || t == typeof(float) || t == typeof(byte) || t == typeof(short) ||
                t == typeof(uint) || t == typeof(ushort) || t == typeof(ulong))
                return "number";
            if (t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(DateOnly))
                return "date";
            return null;
        }

        private static object SerializeDateConstraint(object value)
        {
            if (value is DateTime dt)
                return dt.TimeOfDay == TimeSpan.Zero
                    ? dt.ToString("yyyy-MM-dd")
                    : dt.ToString("s");
            if (value is DateTimeOffset dto)
                return dto.TimeOfDay == TimeSpan.Zero
                    ? dto.ToString("yyyy-MM-dd")
                    : dto.ToString("s");
            return value;
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
            public string? Field { get; }
            public string? CoerceAs { get; }
            public ValidationCondition? When { get; }

            public ExtractedRule(string rule, string message, object? constraint,
                ValidationCondition? when = null, string? field = null, string? coerceAs = null)
            {
                Rule = rule;
                Message = message;
                Constraint = constraint;
                When = when;
                Field = field;
                CoerceAs = coerceAs;
            }
        }
    }
}
