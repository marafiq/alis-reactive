using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Alis.Reactive.FluentValidator.Validators;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;
using ClientValidationRuleModel = Alis.Reactive.Validation.ValidationRule;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Projects only deterministic browser validation rules from FluentValidation validators.
    /// FluentValidation remains the server authority; rules that require server code stay server-side.
    /// </summary>
    public sealed class FluentValidationAdapter : IClientValidationProjectionSource
    {
        private static readonly IReadOnlyDictionary<IValidationRule, ClientConditionProjection> NoClientConditions =
            new Dictionary<IValidationRule, ClientConditionProjection>();

        private readonly Func<Type, IValidator?> _factory;

        public FluentValidationAdapter(Func<Type, IValidator?> factory)
        {
            _factory = factory ?? throw new ArgumentException(
                "A validator factory is required. Pass a function that resolves IValidator instances.",
                nameof(factory));
        }

        public ClientValidationProjection Project(ClientValidationProjectionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var root = ResolveValidator(
                request.ValidationSourceType,
                "validator",
                "Ensure it is registered in the validator factory passed to FluentValidationAdapter.");
            var projection = new ClientValidationProjectionAccumulator(request.ValidationContainer);

            ProjectValidator(root, ValidationFieldPath.Empty, ValidationRuleCondition.Always, projection);

            return projection.ToProjection();
        }

        private void ProjectValidator(
            IValidator validator,
            ValidationFieldPath prefix,
            ValidationRuleCondition parentCondition,
            ClientValidationProjectionAccumulator projection)
        {
            if (!(validator is IEnumerable<IValidationRule> rules)) return;

            var clientConditions = ClientConditionsFrom(validator);
            foreach (var rule in rules)
                ProjectRule(rule, prefix, parentCondition, projection, clientConditions);
        }

        private void ProjectRule(
            IValidationRule rule,
            ValidationFieldPath prefix,
            ValidationRuleCondition parentCondition,
            ClientValidationProjectionAccumulator projection,
            IReadOnlyDictionary<IValidationRule, ClientConditionProjection> clientConditions)
        {
            if (!TryResolveRuleCondition(rule, prefix, projection, clientConditions, out var ruleCondition, out var skipReason))
            {
                if (!IsIncludeRule(rule))
                    projection.Skip(prefix.Append(rule.PropertyName), rule, skipReason);
                return;
            }

            var inheritedCondition = parentCondition.Combine(ruleCondition);
            if (IsIncludeRule(rule))
            {
                ProjectChildValidators(rule, prefix, inheritedCondition, projection);
                return;
            }

            var field = ProjectedField.From(prefix, rule);
            foreach (var component in rule.Components)
                ProjectComponent(component, prefix, field, inheritedCondition, projection);
        }

        private void ProjectComponent(
            IRuleComponent component,
            ValidationFieldPath prefix,
            ProjectedField field,
            ValidationRuleCondition condition,
            ClientValidationProjectionAccumulator projection)
        {
            if (component.HasCondition || component.HasAsyncCondition)
            {
                projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.RuleComponentCondition);
                return;
            }

            if (component.Validator is IChildValidatorAdaptor child)
            {
                var nested = ResolveNestedValidator(_factory, child.ValidatorType);
                ProjectValidator(nested, field.Path, condition, projection);
                return;
            }

            if (ClientValidationRuleProjectionCatalog.TryFind(component, out var explicitRule))
            {
                projection.Add(
                    field.Reference,
                    explicitRule.Name,
                    Message(component, explicitRule.MessageFor(field.DisplayName)),
                    explicitRule.DetailsFor(condition, prefix));
                return;
            }

            ProjectBuiltInRule(component, field, condition, projection);
        }

        private static void ProjectBuiltInRule(
            IRuleComponent component,
            ProjectedField field,
            ValidationRuleCondition condition,
            ClientValidationProjectionAccumulator projection)
        {
            var validator = component.Validator;
            var displayName = field.DisplayName;

            switch (validator)
            {
                case INotEmptyValidator _:
                case INotNullValidator _:
                    projection.Add(field.Reference, ValidationRuleName.Required, Message(component, $"'{displayName}' is required."), ValidationRuleDetails.NoOperand(condition));
                    return;

                case IEmptyValidator _:
                    projection.Add(field.Reference, ValidationRuleName.Empty, Message(component, $"'{displayName}' must be empty."), ValidationRuleDetails.NoOperand(condition));
                    return;

                case ILengthValidator length:
                    ProjectLength(component, length, field, condition, projection);
                    return;

                case IEmailValidator _:
                    projection.Add(field.Reference, ValidationRuleName.Email, Message(component, $"'{displayName}' must be a valid email address."), ValidationRuleDetails.NoOperand(condition));
                    return;

                case IRegularExpressionValidator regex:
                    if (string.IsNullOrEmpty(regex.Expression))
                        projection.Skip(field.Path, validator, ClientRuleProjectionSkipReason.MissingRegexExpression);
                    else
                        projection.Add(field.Reference, ValidationRuleName.Regex, Message(component, $"'{displayName}' format is invalid."), ValidationRuleDetails.WithConstraint(regex.Expression, condition, Shape.None));
                    return;

                case ICreditCardValidator _:
                    projection.Add(field.Reference, ValidationRuleName.CreditCard, Message(component, $"'{displayName}' must be a valid credit card number."), ValidationRuleDetails.NoOperand(condition));
                    return;

                case IExclusiveBetweenValidator range:
                    ProjectRange(component, ValidationRuleName.ExclusiveRange, range.From, range.To, $"'{displayName}' must be between {range.From} and {range.To} (exclusive).", field, condition, projection);
                    return;

                case IBetweenValidator range:
                    ProjectRange(component, ValidationRuleName.Range, range.From, range.To, $"'{displayName}' must be between {range.From} and {range.To}.", field, condition, projection);
                    return;

                case IComparisonValidator comparison:
                    ProjectComparison(component, comparison, field, condition, projection);
                    return;

                default:
                    projection.Skip(field.Path, validator, ClientRuleProjectionSkipReason.UnsupportedValidator);
                    return;
            }
        }

        private static void ProjectLength(
            IRuleComponent component,
            ILengthValidator length,
            ProjectedField field,
            ValidationRuleCondition condition,
            ClientValidationProjectionAccumulator projection)
        {
            if (length.Min > 0)
            {
                projection.Add(
                    field.Reference,
                    ValidationRuleName.MinLength,
                    Message(component, $"'{field.DisplayName}' must be at least {length.Min} characters."),
                    ValidationRuleDetails.WithConstraint(length.Min, condition, Shape.None));
            }

            if (length.Max > 0)
            {
                projection.Add(
                    field.Reference,
                    ValidationRuleName.MaxLength,
                    Message(component, $"'{field.DisplayName}' must be at most {length.Max} characters."),
                    ValidationRuleDetails.WithConstraint(length.Max, condition, Shape.None));
            }
        }

        private static void ProjectRange(
            IRuleComponent component,
            ValidationRuleName ruleName,
            object? lower,
            object? upper,
            string defaultMessage,
            ProjectedField field,
            ValidationRuleCondition condition,
            ClientValidationProjectionAccumulator projection)
        {
            if (lower == null || upper == null)
            {
                projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.MissingRangeEndpoint);
                return;
            }

            var lowerLiteral = ClientValidationProjectionLiteral.From(lower);
            var upperLiteral = ClientValidationProjectionLiteral.From(upper);
            if (!lowerLiteral.Shape.Equals(upperLiteral.Shape))
            {
                projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.MissingRangeEndpoint);
                return;
            }

            var bounds = ValidationRangeBounds.Between(
                lowerLiteral.Value!,
                upperLiteral.Value!,
                lowerLiteral.Shape);
            projection.Add(
                field.Reference,
                ruleName,
                Message(component, defaultMessage),
                ValidationRuleDetails.WithConstraint(ValidationConstraint.InclusiveRange(bounds), condition, bounds.Shape));
        }

        private static void ProjectComparison(
            IRuleComponent component,
            IComparisonValidator comparison,
            ProjectedField field,
            ValidationRuleCondition condition,
            ClientValidationProjectionAccumulator projection)
        {
            if (comparison.MemberToCompare != null)
            {
                projection.Skip(
                    field.Path,
                    component.Validator,
                    ClientRuleProjectionSkipReason.PeerComparisonRequiresExplicitProjection);
                return;
            }

            var ruleName = RuleNameFor(comparison.Comparison);
            if (ruleName == null)
            {
                projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.UnsupportedComparisonOperator);
                return;
            }

            var literal = ClientValidationProjectionLiteral.From(comparison.ValueToCompare);
            projection.Add(
                field.Reference,
                ruleName,
                Message(component, LiteralComparisonMessage(comparison.Comparison, field.DisplayName, literal.Value)),
                ValidationRuleDetails.WithConstraint(literal.Value, condition, literal.Shape));
        }

        private static bool TryResolveRuleCondition(
            IValidationRule rule,
            ValidationFieldPath prefix,
            ClientValidationProjectionAccumulator projection,
            IReadOnlyDictionary<IValidationRule, ClientConditionProjection> clientConditions,
            out ValidationRuleCondition condition,
            out ClientRuleProjectionSkipReason skipReason)
        {
            condition = ValidationRuleCondition.Always;
            skipReason = ClientRuleProjectionSkipReason.UnsupportedValidator;

            if (!rule.HasCondition && !rule.HasAsyncCondition)
                return true;

            if (!clientConditions.TryGetValue(rule, out var clientCondition))
            {
                skipReason = ClientRuleProjectionSkipReason.FluentValidationConditionWithoutClientGuard;
                return false;
            }

            if (!clientCondition.TryProject(out var fieldCondition, out var fields, out skipReason))
                return false;

            foreach (var field in fields)
                projection.Ensure(field.PrefixedBy(prefix));

            var binding = new FieldConditionPrefixBinding(prefix);
            condition = ValidationRuleCondition.When(fieldCondition.PrefixWith(binding));
            return true;
        }

        private void ProjectChildValidators(
            IValidationRule rule,
            ValidationFieldPath prefix,
            ValidationRuleCondition condition,
            ClientValidationProjectionAccumulator projection)
        {
            foreach (var component in rule.Components)
            {
                if (component.Validator is not IChildValidatorAdaptor child) continue;

                var nested = ResolveNestedValidator(_factory, child.ValidatorType);
                ProjectValidator(nested, prefix, condition, projection);
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

        private static IReadOnlyDictionary<IValidationRule, ClientConditionProjection> ClientConditionsFrom(IValidator validator) =>
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

        private static ValidationRuleName? RuleNameFor(Comparison comparison)
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

        private sealed class ClientValidationProjectionAccumulator
        {
            private readonly ValidationContainerId _container;
            private readonly ClientValidationProjectionDraft _draft = new ClientValidationProjectionDraft();
            private readonly List<SkippedClientRuleProjection> _skippedRules = new List<SkippedClientRuleProjection>();

            internal ClientValidationProjectionAccumulator(ValidationContainerId container)
            {
                _container = container;
            }

            internal void Ensure(ClientValidationFieldReference field) => _draft.EnsureField(field);

            internal void Add(
                ClientValidationFieldReference field,
                ValidationRuleName name,
                ValidationMessage message,
                ValidationRuleDetails details)
            {
                foreach (var peer in details.PeerFieldReferences)
                    Ensure(peer);

                _draft.AddRule(field, new ClientValidationRuleModel(name, message, details));
            }

            internal void Skip(
                ValidationFieldPath fieldPath,
                IPropertyValidator validator,
                ClientRuleProjectionSkipReason reason) =>
                _skippedRules.Add(SkippedClientRuleProjection.For(fieldPath, validator.Name, reason));

            internal void Skip(
                ValidationFieldPath fieldPath,
                IValidationRule rule,
                ClientRuleProjectionSkipReason reason)
            {
                var name = string.Join(", ", rule.Components.Select(component => component.Validator.Name));
                if (string.IsNullOrWhiteSpace(name))
                    name = rule.GetType().Name;

                _skippedRules.Add(SkippedClientRuleProjection.For(fieldPath, name, reason));
            }

            internal ClientValidationProjection ToProjection() =>
                new ClientValidationProjection(_container, _draft.ToFields(), _skippedRules);
        }

        private sealed class ProjectedField
        {
            private ProjectedField(
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

            internal static ProjectedField From(ValidationFieldPath prefix, IValidationRule rule)
            {
                var path = prefix.Append(rule.PropertyName);
                return new ProjectedField(
                    rule.PropertyName,
                    path,
                    ClientValidationFieldReference.Of(path, Shape.FromClrType(rule.TypeToValidate)));
            }
        }
    }
}
