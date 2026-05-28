using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Alis.Reactive.FluentValidator.Validators;
using Alis.Reactive.Validation;
using Shape = Alis.Reactive.PlanModel.Shape;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Extracts only deterministic browser validation rules from FluentValidation validators.
    /// FluentValidation still executes normally; rules that cannot be represented in the
    /// browser plan are omitted from the client rule set.
    /// </summary>
    public sealed class FluentValidationAdapter : IClientValidationRuleSource
    {
        private static readonly IReadOnlyDictionary<IValidationRule, ClientRuleCondition> NoClientConditions =
            new Dictionary<IValidationRule, ClientRuleCondition>();

        private readonly Func<Type, IValidator?> _factory;

        public FluentValidationAdapter(Func<Type, IValidator?> factory)
        {
            _factory = factory ?? throw new ArgumentException(
                "A validator factory is required. Pass a function that resolves IValidator instances.",
                nameof(factory));
        }

        public IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType)
        {
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));

            var root = ResolveValidator(
                validationSourceType,
                "validator",
                "Ensure it is registered in the validator factory passed to FluentValidationAdapter.");
            var rules = new ClientValidationRuleSet();

            ExtractValidatorRules(root, ValidationFieldPath.Empty, ClientRuleActivation.Always, rules);

            return rules.ToFields();
        }

        private void ExtractValidatorRules(
            IValidator validator,
            ValidationFieldPath prefix,
            ClientRuleActivation parentCondition,
            ClientValidationRuleSet rules)
        {
            if (!(validator is IEnumerable<IValidationRule> validatorRules)) return;

            var clientConditions = ClientConditionsFrom(validator);
            foreach (var rule in validatorRules)
                ExtractRule(rule, prefix, parentCondition, rules, clientConditions);
        }

        private void ExtractRule(
            IValidationRule rule,
            ValidationFieldPath prefix,
            ClientRuleActivation parentCondition,
            ClientValidationRuleSet rules,
            IReadOnlyDictionary<IValidationRule, ClientRuleCondition> clientConditions)
        {
            if (!TryResolveClientRuleCondition(rule, prefix, rules, clientConditions, out var ruleCondition))
                return;

            var inheritedCondition = parentCondition.Combine(ruleCondition);
            if (IsIncludeRule(rule))
            {
                ExtractChildValidatorRules(rule, prefix, inheritedCondition, rules);
                return;
            }

            var field = ClientRuleField.From(prefix, rule);
            foreach (var component in rule.Components)
                ExtractComponentRule(component, prefix, field, inheritedCondition, rules);
        }

        private void ExtractComponentRule(
            IRuleComponent component,
            ValidationFieldPath prefix,
            ClientRuleField field,
            ClientRuleActivation condition,
            ClientValidationRuleSet rules)
        {
            if (component.HasCondition || component.HasAsyncCondition)
            {
                return;
            }

            if (component.Validator is IChildValidatorAdaptor child)
            {
                var nested = ResolveNestedValidator(_factory, child.ValidatorType);
                ExtractValidatorRules(nested, field.Path, condition, rules);
                return;
            }

            if (ClientValidationRuleBridge.TryFind(component, out var explicitRule))
            {
                rules.AddRule(
                    field.Reference,
                    explicitRule.ToValidationRule(
                        Message(component, explicitRule.MessageFor(field.DisplayName)),
                        condition,
                        prefix));
                return;
            }

            ExtractBuiltInRule(component, prefix, field, condition, rules);
        }

        private static void ExtractBuiltInRule(
            IRuleComponent component,
            ValidationFieldPath prefix,
            ClientRuleField field,
            ClientRuleActivation condition,
            ClientValidationRuleSet rules)
        {
            var validator = component.Validator;
            var displayName = field.DisplayName;

            switch (validator)
            {
                case INotEmptyValidator _:
                case INotNullValidator _:
                    rules.AddRule(field.Reference, ValidationRuleName.Required, Message(component, $"'{displayName}' is required."), ValidationRuleOperand.None, condition, Shape.None);
                    return;

                case IEmptyValidator _:
                    rules.AddRule(field.Reference, ValidationRuleName.Empty, Message(component, $"'{displayName}' must be empty."), ValidationRuleOperand.None, condition, Shape.None);
                    return;

                case ILengthValidator length:
                    ExtractLengthRule(component, length, field, condition, rules);
                    return;

                case IEmailValidator _:
                    rules.AddRule(field.Reference, ValidationRuleName.Email, Message(component, $"'{displayName}' must be a valid email address."), ValidationRuleOperand.None, condition, Shape.None);
                    return;

                case IRegularExpressionValidator regex:
                    if (string.IsNullOrEmpty(regex.Expression))
                        return;
                    else
                        rules.AddRule(field.Reference, ValidationRuleName.Regex, Message(component, $"'{displayName}' format is invalid."), ValidationRuleOperand.Literal(regex.Expression, Shape.None), condition, Shape.None);
                    return;

                case ICreditCardValidator _:
                    rules.AddRule(field.Reference, ValidationRuleName.CreditCard, Message(component, $"'{displayName}' must be a valid credit card number."), ValidationRuleOperand.None, condition, Shape.None);
                    return;

                case IExclusiveBetweenValidator range:
                    ExtractRangeRule(component, ValidationRuleName.ExclusiveRange, range.From, range.To, $"'{displayName}' must be between {range.From} and {range.To} (exclusive).", field, condition, rules);
                    return;

                case IBetweenValidator range:
                    ExtractRangeRule(component, ValidationRuleName.Range, range.From, range.To, $"'{displayName}' must be between {range.From} and {range.To}.", field, condition, rules);
                    return;

                case IComparisonValidator comparison:
                    ExtractComparisonRule(component, comparison, prefix, field, condition, rules);
                    return;

                default:
                    return;
            }
        }

        private static void ExtractLengthRule(
            IRuleComponent component,
            ILengthValidator length,
            ClientRuleField field,
            ClientRuleActivation condition,
            ClientValidationRuleSet rules)
        {
            if (length.Min > 0)
            {
                rules.AddRule(
                    field.Reference,
                    ValidationRuleName.MinLength,
                    Message(component, $"'{field.DisplayName}' must be at least {length.Min} characters."),
                    ValidationRuleOperand.Literal(length.Min, Shape.None),
                    condition,
                    Shape.None);
            }

            if (length.Max > 0)
            {
                rules.AddRule(
                    field.Reference,
                    ValidationRuleName.MaxLength,
                    Message(component, $"'{field.DisplayName}' must be at most {length.Max} characters."),
                    ValidationRuleOperand.Literal(length.Max, Shape.None),
                    condition,
                    Shape.None);
            }
        }

        private static void ExtractRangeRule(
            IRuleComponent component,
            ValidationRuleName ruleName,
            object? lower,
            object? upper,
            string defaultMessage,
            ClientRuleField field,
            ClientRuleActivation condition,
            ClientValidationRuleSet rules)
        {
            if (!ValidationRangeBounds.TryFromClientLiteral(lower, upper, out var bounds))
            {
                return;
            }

            rules.AddRule(
                field.Reference,
                ruleName,
                Message(component, defaultMessage),
                ValidationRuleOperand.Range(bounds),
                condition,
                bounds.Shape);
        }

        private static void ExtractComparisonRule(
            IRuleComponent component,
            IComparisonValidator comparison,
            ValidationFieldPath prefix,
            ClientRuleField field,
            ClientRuleActivation condition,
            ClientValidationRuleSet rules)
        {
            var peerField = comparison.MemberToCompare;
            if (peerField != null)
            {
                ExtractPeerComparisonRule(component, comparison, prefix, peerField.Name, field, condition, rules);
                return;
            }

            var ruleName = LiteralRuleNameFor(comparison.Comparison);
            if (ruleName == null)
            {
                return;
            }

            var literal = ClientValidationLiteral.From(comparison.ValueToCompare);
            rules.AddRule(
                field.Reference,
                ruleName,
                Message(component, LiteralComparisonMessage(comparison.Comparison, field.DisplayName, literal.Value)),
                ValidationRuleOperand.Literal(literal.Value, literal.Shape),
                condition,
                literal.Shape);
        }

        private static void ExtractPeerComparisonRule(
            IRuleComponent component,
            IComparisonValidator comparison,
            ValidationFieldPath prefix,
            string peerFieldName,
            ClientRuleField field,
            ClientRuleActivation condition,
            ClientValidationRuleSet rules)
        {
            var ruleName = PeerRuleNameFor(comparison.Comparison);
            if (ruleName == null)
            {
                return;
            }

            var peerPath = prefix.Append(peerFieldName);
            rules.AddRule(
                field.Reference,
                ruleName,
                Message(component, PeerComparisonMessage(comparison.Comparison, field.DisplayName, peerFieldName)),
                ValidationRuleOperand.PeerField(peerPath, field.Reference.Shape),
                condition,
                field.Reference.Shape);
        }

        private static bool TryResolveClientRuleCondition(
            IValidationRule rule,
            ValidationFieldPath prefix,
            ClientValidationRuleSet rules,
            IReadOnlyDictionary<IValidationRule, ClientRuleCondition> clientConditions,
            out ClientRuleActivation condition)
        {
            condition = ClientRuleActivation.Always;

            if (!rule.HasCondition && !rule.HasAsyncCondition)
                return true;

            if (!clientConditions.TryGetValue(rule, out var clientCondition))
                return false;

            if (!clientCondition.TryUseOnClient(out var fieldCondition, out var fields))
                return false;

            foreach (var field in fields)
                rules.EnsureField(field.PrefixedBy(prefix));

            var binding = new FieldConditionPrefixBinding(prefix);
            condition = ClientRuleActivation.When(fieldCondition.PrefixWith(binding));
            return true;
        }

        private void ExtractChildValidatorRules(
            IValidationRule rule,
            ValidationFieldPath prefix,
            ClientRuleActivation condition,
            ClientValidationRuleSet rules)
        {
            foreach (var component in rule.Components)
            {
                if (component.Validator is not IChildValidatorAdaptor child) continue;

                var nested = ResolveNestedValidator(_factory, child.ValidatorType);
                ExtractValidatorRules(nested, prefix, condition, rules);
            }
        }

        private IValidator ResolveValidator(Type validatorType, string role, string guidance) =>
            ResolveValidator(_factory, validatorType, role, guidance);

        private static IValidator ResolveNestedValidator(Func<Type, IValidator?> factory, Type validatorType) =>
            ResolveValidator(factory, validatorType, "nested validator", "Ensure it is registered in the validator factory.");

        private static IValidator ResolveValidator(
            Func<Type, IValidator?> factory,
            Type validatorType,
            string role,
            string guidance)
        {
            try
            {
                var validator = factory(validatorType);
                if (validator != null) return validator;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create {role} '{validatorType.Name}'. {guidance}", ex);
            }

            throw new InvalidOperationException($"Validator factory returned null for {role} '{validatorType.Name}'. {guidance}");
        }

        private static bool IsIncludeRule(IValidationRule rule) =>
            string.IsNullOrEmpty(rule.PropertyName);

        private static IReadOnlyDictionary<IValidationRule, ClientRuleCondition> ClientConditionsFrom(IValidator validator) =>
            validator is IClientConditionSource source
                ? source.ClientConditions
                : NoClientConditions;

        private static ValidationMessage Message(IRuleComponent component, string defaultMessage) =>
            Message(component, ValidationMessage.Of(defaultMessage));

        private static ValidationMessage Message(IRuleComponent component, ValidationMessage defaultMessage)
        {
            var raw = component.GetUnformattedErrorMessage();
            return !string.IsNullOrEmpty(raw) && !raw.Contains('{')
                ? ValidationMessage.Of(raw)
                : defaultMessage;
        }

        private static ValidationRuleName? LiteralRuleNameFor(Comparison comparison)
        {
            return comparison switch
            {
                Comparison.Equal => ValidationRuleName.EqualTo,
                Comparison.NotEqual => ValidationRuleName.NotEqual,
                Comparison.GreaterThanOrEqual => ValidationRuleName.Min,
                Comparison.LessThanOrEqual => ValidationRuleName.Max,
                Comparison.GreaterThan => ValidationRuleName.Gt,
                Comparison.LessThan => ValidationRuleName.Lt,
                _ => null
            };
        }

        private static ValidationRuleName? PeerRuleNameFor(Comparison comparison) =>
            comparison == Comparison.NotEqual
                ? ValidationRuleName.NotEqualTo
                : LiteralRuleNameFor(comparison);

        private static string LiteralComparisonMessage(Comparison comparison, string fieldName, object? value)
        {
            return comparison switch
            {
                Comparison.Equal => $"'{fieldName}' must equal {value}.",
                Comparison.NotEqual => $"'{fieldName}' must not equal '{value}'.",
                Comparison.GreaterThanOrEqual => $"'{fieldName}' must be at least {value}.",
                Comparison.LessThanOrEqual => $"'{fieldName}' must be at most {value}.",
                Comparison.GreaterThan => $"'{fieldName}' must be greater than {value}.",
                Comparison.LessThan => $"'{fieldName}' must be less than {value}.",
                _ => $"'{fieldName}' is invalid."
            };
        }

        private static string PeerComparisonMessage(Comparison comparison, string fieldName, string peerFieldName)
        {
            var peer = Humanize(peerFieldName);
            return comparison switch
            {
                Comparison.Equal => $"'{fieldName}' must equal '{peer}'.",
                Comparison.NotEqual => $"'{fieldName}' must not equal '{peer}'.",
                Comparison.GreaterThanOrEqual => $"'{fieldName}' must be at least '{peer}'.",
                Comparison.LessThanOrEqual => $"'{fieldName}' must be at most '{peer}'.",
                Comparison.GreaterThan => $"'{fieldName}' must be greater than '{peer}'.",
                Comparison.LessThan => $"'{fieldName}' must be less than '{peer}'.",
                _ => $"'{fieldName}' is invalid."
            };
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

        private sealed class ClientRuleField
        {
            private ClientRuleField(
                string displayName,
                ValidationFieldPath path,
                ClientValidationFieldReference reference)
            {
                Path = path;
                Reference = reference;
                DisplayName = Humanize(displayName);
            }

            internal ValidationFieldPath Path { get; }
            internal ClientValidationFieldReference Reference { get; }
            internal string DisplayName { get; }

            internal static ClientRuleField From(ValidationFieldPath prefix, IValidationRule rule)
            {
                var path = prefix.Append(rule.PropertyName);
                return new ClientRuleField(
                    rule.PropertyName,
                    path,
                    ClientValidationFieldReference.Of(path, Shape.FromClrType(rule.TypeToValidate)));
            }
        }
    }
}
