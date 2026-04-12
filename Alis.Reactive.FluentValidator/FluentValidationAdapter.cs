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
        /// Returns an empty list if no extractable rules are found.
        /// Fields carry only fieldName + rules. Runtime enriches component info from plan.components.
        /// </summary>
        public List<ValidationField> ExtractRules(Type validatorType, string formId)
        {
            var validator = _factory(validatorType);
            if (validator == null) return new List<ValidationField>();

            // Intermediate: property path → ordered list of (ruleType, message, constraint)
            var fieldRules = new Dictionary<string, List<ExtractedRule>>();

            // Read client conditions if validator extends ReactiveValidator<T>
            IReadOnlyDictionary<IValidationRule, FieldCondition>? clientConditions = null;
            if (validator is IClientConditionSource source)
            {
                clientConditions = source.ClientConditions;
            }

            ExtractFromValidator(validator, "", fieldRules, _factory, clientConditions);

            // Ensure cross-property peer fields are present in the extracted form contract for value reads.
            var peerFields = fieldRules
                .SelectMany(kvp => kvp.Value)
                .Where(er => !string.IsNullOrEmpty(er.Field) && !fieldRules.ContainsKey(er.Field!))
                .Select(er => er.Field!)
                .ToHashSet();
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
                    rules.Add(new ValidationRule(er.Rule, er.Message, er.Constraint, er.When, er.Field, er.Shape));
                }
                fields.Add(new ValidationField(propertyPath, rules));
            }

            return fields;
        }

        private static void ExtractFromValidator(
            IValidator validator,
            string prefix,
            Dictionary<string, List<ExtractedRule>> fieldRules,
            Func<Type, IValidator?> factory,
            IReadOnlyDictionary<IValidationRule, FieldCondition>? clientConditions = null,
            FieldCondition? parentCondition = null)
        {
            if (!(validator is IEnumerable<IValidationRule> rules)) return;

            foreach (var rule in rules)
            {
                var (ruleCondition, skip) = TryResolveCondition(rule, prefix, fieldRules, clientConditions);
                if (skip) continue;

                if (string.IsNullOrEmpty(rule.PropertyName))
                {
                    ProcessIncludeRule(rule, prefix, fieldRules, factory, ruleCondition ?? parentCondition);
                    continue;
                }

                var fullPath = string.IsNullOrEmpty(prefix)
                    ? rule.PropertyName
                    : prefix + "." + rule.PropertyName;

                ProcessComponents(rule, fullPath, rule.PropertyName, prefix, fieldRules, factory, ruleCondition, parentCondition);
            }
        }

        /// <summary>
        /// Checks if a rule has a client-side WhenField() condition. Returns the condition
        /// and whether the rule should be skipped (server-only .When()).
        /// </summary>
        private static (FieldCondition? condition, bool skip) TryResolveCondition(
            IValidationRule rule,
            string prefix,
            Dictionary<string, List<ExtractedRule>> fieldRules,
            IReadOnlyDictionary<IValidationRule, FieldCondition>? clientConditions)
        {
            if (!rule.HasCondition && !rule.HasAsyncCondition)
                return (null, false);

            if (clientConditions == null || !clientConditions.TryGetValue(rule, out var cc))
                return (null, true); // Server-only .When() — skip

            // Apply prefix to all field references in the condition tree
            // and ensure peer fields exist in fieldRules.
            var resolved = ApplyPrefix(cc, prefix, fieldRules);
            return (resolved, false);
        }

        /// <summary>
        /// Recursively applies a prefix to all FieldCompare.Field values in the tree
        /// and ensures each referenced field is present in fieldRules.
        /// </summary>
        private static FieldCondition ApplyPrefix(
            FieldCondition fc, string prefix,
            Dictionary<string, List<ExtractedRule>> fieldRules)
        {
            switch (fc)
            {
                case FieldCompare cmp:
                {
                    var fullField = string.IsNullOrEmpty(prefix)
                        ? cmp.Field
                        : prefix + "." + cmp.Field;
                    if (!fieldRules.ContainsKey(fullField))
                        fieldRules[fullField] = new List<ExtractedRule>();
                    return FieldCondition.Compare(fullField, cmp.Op, cmp.Value);
                }
                case FieldAll all:
                {
                    var terms = new FieldCondition[all.Terms.Count];
                    for (int i = 0; i < all.Terms.Count; i++)
                        terms[i] = ApplyPrefix(all.Terms[i], prefix, fieldRules);
                    return FieldCondition.All(terms);
                }
                case FieldAny any:
                {
                    var terms = new FieldCondition[any.Terms.Count];
                    for (int i = 0; i < any.Terms.Count; i++)
                        terms[i] = ApplyPrefix(any.Terms[i], prefix, fieldRules);
                    return FieldCondition.Any(terms);
                }
                case FieldNot not:
                    return FieldCondition.Not(ApplyPrefix(not.Term, prefix, fieldRules));
                default:
                    throw new InvalidOperationException(
                        $"Unknown FieldCondition type: {fc.GetType().Name}");
            }
        }

        /// <summary>
        /// Handles Include() rules (empty PropertyName) — recurses into the included validator.
        /// </summary>
        private static void ProcessIncludeRule(
            IValidationRule rule,
            string prefix,
            Dictionary<string, List<ExtractedRule>> fieldRules,
            Func<Type, IValidator?> factory,
            FieldCondition? parentCondition = null)
        {
            foreach (IRuleComponent component in rule.Components)
            {
                if (component.Validator is IChildValidatorAdaptor adaptor)
                {
                    var nested = ResolveNestedValidator(factory, adaptor.ValidatorType);
                    var nestedConditions = (nested as IClientConditionSource)?.ClientConditions;
                    ExtractFromValidator(nested, prefix, fieldRules, factory, nestedConditions, parentCondition);
                }
            }
        }

        /// <summary>
        /// Iterates rule components, recursing into nested validators and mapping leaf validators.
        /// </summary>
        private static void ProcessComponents(
            IValidationRule rule,
            string fullPath,
            string propertyName,
            string prefix,
            Dictionary<string, List<ExtractedRule>> fieldRules,
            Func<Type, IValidator?> factory,
            FieldCondition? ruleCondition,
            FieldCondition? parentCondition)
        {
            foreach (IRuleComponent component in rule.Components)
            {
                if (component.HasCondition || component.HasAsyncCondition) continue;

                if (component.Validator is IChildValidatorAdaptor adaptor)
                {
                    var nested = ResolveNestedValidator(factory, adaptor.ValidatorType);
                    var nestedConditions = (nested as IClientConditionSource)?.ClientConditions;
                    ExtractFromValidator(nested, fullPath, fieldRules, factory, nestedConditions, ruleCondition);
                    continue;
                }

                // Compose parent + rule conditions: if both exist, both must be true (All).
                FieldCondition? effectiveCondition;
                if (ruleCondition != null && parentCondition != null)
                    effectiveCondition = FieldCondition.All(parentCondition, ruleCondition);
                else
                    effectiveCondition = ruleCondition ?? parentCondition;
                var extracted = MapComponent(component, propertyName, prefix, effectiveCondition);
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
            IRuleComponent component, string propertyName, string prefix, FieldCondition? ruleCondition = null)
        {
            var result = new List<ExtractedRule>();
            var validator = component.Validator;
            var displayName = Humanize(propertyName);
            // GetUnformattedErrorMessage() returns FV's template (e.g. "'{PropertyName}' must not be empty.")
            // even when no .WithMessage() was set. Only treat it as a custom message if it does NOT
            // contain FV placeholder tokens like {PropertyName}.
            var rawMsg = component.GetUnformattedErrorMessage();
            var customMsg = !string.IsNullOrEmpty(rawMsg) && !rawMsg.Contains('{')
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
                    MapLengthValidator(lv, displayName, customMsg, ruleCondition, result);
                    break;

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
                    result.Add(MapRangeValidator("exclusiveRange", ebv.From, ebv.To,
                        customMsg ?? $"'{displayName}' must be between {ebv.From} and {ebv.To} (exclusive).",
                        ruleCondition));
                    break;

                case IBetweenValidator bv:
                    result.Add(MapRangeValidator("range", bv.From, bv.To,
                        customMsg ?? $"'{displayName}' must be between {bv.From} and {bv.To}.",
                        ruleCondition));
                    break;

                case IComparisonValidator cv:
                {
                    var comparisonRule = MapComparisonValidator(cv, propertyName, prefix, displayName, customMsg, ruleCondition);
                    result.Add(comparisonRule);
                    break;
                }
            }

            return result;
        }

        private static void MapLengthValidator(
            ILengthValidator lv, string displayName, string? customMsg,
            FieldCondition? ruleCondition, List<ExtractedRule> result)
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
        }

        private static ExtractedRule MapRangeValidator(
            string ruleType, object? from, object? to,
            string message, FieldCondition? ruleCondition)
        {
            var shape = Shape.FromClrType(from?.GetType());
            var isDate = shape == Shape.Date;
            var serializedFrom = isDate && from != null ? SerializeDateConstraint(from) : from;
            var serializedTo = isDate && to != null ? SerializeDateConstraint(to) : to;
            return new ExtractedRule(ruleType, message,
                new object[] { serializedFrom!, serializedTo! }, ruleCondition, field: null, shape: shape);
        }

        private static ExtractedRule MapComparisonValidator(
            IComparisonValidator cv, string propertyName, string prefix, string displayName,
            string? customMsg, FieldCondition? ruleCondition)
        {
            var (field, constraint, propertyType) = ResolveComparisonOperands(cv);
            var isNestedPeerField = field != null && !string.IsNullOrEmpty(prefix);
            if (isNestedPeerField)
                field = prefix + "." + field;

            var shape = Shape.FromClrType(propertyType);
            var isDateConstraint = shape == Shape.Date && constraint != null;
            if (isDateConstraint)
                constraint = SerializeDateConstraint(constraint);

            var (ruleType, defaultMsg) = cv.Comparison switch
            {
                Comparison.Equal => ("equalTo",
                    ComparisonMessage(displayName, field, constraint, "must match", "must equal")),
                Comparison.NotEqual when field != null => ("notEqualTo",
                    $"'{displayName}' must not match '{Humanize(field)}'."),
                Comparison.NotEqual => ("notEqual",
                    $"'{displayName}' must not equal '{constraint}'."),
                Comparison.GreaterThanOrEqual => ("min",
                    ComparisonMessage(displayName, field, constraint, "must be at least", "must be at least")),
                Comparison.LessThanOrEqual => ("max",
                    ComparisonMessage(displayName, field, constraint, "must be at most", "must be at most")),
                Comparison.GreaterThan => ("gt",
                    ComparisonMessage(displayName, field, constraint, "must be greater than", "must be greater than")),
                Comparison.LessThan => ("lt",
                    ComparisonMessage(displayName, field, constraint, "must be less than", "must be less than")),
                _ => throw new InvalidOperationException(
                    $"Unknown Comparison type '{cv.Comparison}' on property '{propertyName}'. " +
                    $"This FluentValidation comparison is not supported for client-side extraction.")
            };

            return new ExtractedRule(ruleType, customMsg ?? defaultMsg, constraint, ruleCondition, field, shape);
        }

        private static string ComparisonMessage(
            string displayName, string? field, object? constraint,
            string fieldVerb, string constraintVerb)
            => field != null
                ? $"'{displayName}' {fieldVerb} '{Humanize(field)}'."
                : $"'{displayName}' {constraintVerb} {constraint}.";

        private static (string? field, object? constraint, Type? propertyType) ResolveComparisonOperands(
            IComparisonValidator cv)
        {
            if (cv.MemberToCompare != null)
            {
                var field = cv.MemberToCompare.Name;
                Type? propertyType = cv.MemberToCompare switch
                {
                    System.Reflection.PropertyInfo pi => pi.PropertyType,
                    System.Reflection.FieldInfo fi => fi.FieldType,
                    _ => null
                };
                return (field, null, propertyType);
            }

            return (null, cv.ValueToCompare, cv.ValueToCompare?.GetType());
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
#if NET6_0_OR_GREATER
            if (value is DateOnly d)
                return d.ToString("yyyy-MM-dd");
#endif
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
            public Shape Shape { get; }
            public FieldCondition? When { get; }

            public ExtractedRule(string rule, string message, object? constraint,
                FieldCondition? when = null, string? field = null, Shape? shape = null)
            {
                Rule = rule;
                Message = message;
                Constraint = constraint;
                When = when;
                Field = field;
                Shape = shape ?? Shape.None;
            }
        }
    }
}
