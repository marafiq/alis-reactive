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

            ProjectValidator(root, ProjectionFrame.Root(_factory, projection, ClientConditionCatalog.From(root)));

            return projection.ToProjection();
        }

        private static void ProjectValidator(IValidator validator, ProjectionFrame frame)
        {
            if (!(validator is IEnumerable<IValidationRule> rules)) return;

            foreach (var rule in rules)
                ProjectRule(rule, frame);
        }

        private static void ProjectRule(IValidationRule rule, ProjectionFrame frame)
        {
            var ruleScope = ResolveRuleScope(rule, frame);
            if (ruleScope.SkipReason.HasValue)
            {
                if (!IsIncludeRule(rule))
                    frame.Projection.Skip(frame.FieldPath(rule), rule, ruleScope.SkipReason.Value);
                return;
            }

            var inheritedCondition = frame.ParentCondition.Combine(ruleScope.Condition);
            if (IsIncludeRule(rule))
            {
                ProjectChildValidators(rule, frame.WithParentCondition(inheritedCondition));
                return;
            }

            var field = ProjectedField.From(frame.Prefix, rule);
            foreach (var component in rule.Components)
                ProjectComponent(component, field, frame.WithParentCondition(inheritedCondition));
        }

        private static void ProjectComponent(
            IRuleComponent component,
            ProjectedField field,
            ProjectionFrame frame)
        {
            if (component.HasCondition || component.HasAsyncCondition)
            {
                frame.Projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.RuleComponentCondition);
                return;
            }

            if (component.Validator is IChildValidatorAdaptor child)
            {
                var nested = ResolveNestedValidator(frame.Factory, child.ValidatorType);
                ProjectValidator(nested, frame.ForNestedValidator(nested, field.Path));
                return;
            }

            if (IsAsyncValidator(component.Validator))
            {
                frame.Projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.AsyncValidator);
                return;
            }

            if (ClientValidationRuleProjectionCatalog.TryFind(component, out var explicitRule))
            {
                frame.Projection.Add(field.Reference, explicitRule, Message(component, explicitRule.MessageFor(field.DisplayName)), frame.ParentCondition);
                return;
            }

            ProjectBuiltInRule(component, field, frame);
        }

        private static void ProjectBuiltInRule(
            IRuleComponent component,
            ProjectedField field,
            ProjectionFrame frame)
        {
            var validator = component.Validator;
            var displayName = field.DisplayName;

            switch (validator)
            {
                case INotEmptyValidator _:
                case INotNullValidator _:
                    frame.Projection.Add(field.Reference, ValidationRuleName.Required, Message(component, $"'{displayName}' is required."), ValidationRuleDetails.NoOperand(frame.ParentCondition));
                    return;

                case Alis.Reactive.FluentValidator.Validators.IEmptyValidator _:
                    frame.Projection.Add(field.Reference, ValidationRuleName.Empty, Message(component, $"'{displayName}' must be empty."), ValidationRuleDetails.NoOperand(frame.ParentCondition));
                    return;

                case ILengthValidator length:
                    ProjectLength(component, length, field, frame);
                    return;

                case IEmailValidator _:
                    frame.Projection.Add(field.Reference, ValidationRuleName.Email, Message(component, $"'{displayName}' must be a valid email address."), ValidationRuleDetails.NoOperand(frame.ParentCondition));
                    return;

                case IRegularExpressionValidator regex:
                    if (string.IsNullOrEmpty(regex.Expression))
                        frame.Projection.Skip(field.Path, validator, ClientRuleProjectionSkipReason.MissingRegexExpression);
                    else
                        frame.Projection.Add(field.Reference, ValidationRuleName.Regex, Message(component, $"'{displayName}' format is invalid."), ValidationRuleDetails.WithConstraint(regex.Expression, frame.ParentCondition, Shape.None));
                    return;

                case ICreditCardValidator _:
                    frame.Projection.Add(field.Reference, ValidationRuleName.CreditCard, Message(component, $"'{displayName}' must be a valid credit card number."), ValidationRuleDetails.NoOperand(frame.ParentCondition));
                    return;

                case IExclusiveBetweenValidator range:
                    ProjectRange(component, ValidationRuleName.ExclusiveRange, range.From, range.To, $"'{displayName}' must be between {range.From} and {range.To} (exclusive).", field, frame);
                    return;

                case IBetweenValidator range:
                    ProjectRange(component, ValidationRuleName.Range, range.From, range.To, $"'{displayName}' must be between {range.From} and {range.To}.", field, frame);
                    return;

                case IComparisonValidator comparison:
                    ProjectComparison(component, comparison, field, frame);
                    return;

                default:
                    frame.Projection.Skip(field.Path, validator, ClientRuleProjectionSkipReason.UnsupportedValidator);
                    return;
            }
        }

        private static bool IsAsyncValidator(IPropertyValidator validator) =>
            validator
                .GetType()
                .GetInterfaces()
                .Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncPropertyValidator<,>));

        private static void ProjectLength(
            IRuleComponent component,
            ILengthValidator length,
            ProjectedField field,
            ProjectionFrame frame)
        {
            if (length.Min > 0)
            {
                frame.Projection.Add(
                    field.Reference,
                    ValidationRuleName.MinLength,
                    Message(component, $"'{field.DisplayName}' must be at least {length.Min} characters."),
                    ValidationRuleDetails.WithConstraint(length.Min, frame.ParentCondition, Shape.None));
            }

            if (length.Max > 0)
            {
                frame.Projection.Add(
                    field.Reference,
                    ValidationRuleName.MaxLength,
                    Message(component, $"'{field.DisplayName}' must be at most {length.Max} characters."),
                    ValidationRuleDetails.WithConstraint(length.Max, frame.ParentCondition, Shape.None));
            }
        }

        private static void ProjectRange(
            IRuleComponent component,
            ValidationRuleName ruleName,
            object? lower,
            object? upper,
            string defaultMessage,
            ProjectedField field,
            ProjectionFrame frame)
        {
            if (lower == null || upper == null)
            {
                frame.Projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.MissingRangeEndpoint);
                return;
            }

            var lowerLiteral = PlanLiteral.From(lower);
            var upperLiteral = PlanLiteral.From(upper);
            if (!lowerLiteral.Shape.Equals(upperLiteral.Shape))
            {
                frame.Projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.MissingRangeEndpoint);
                return;
            }

            var bounds = ValidationRangeBounds.Between(
                lowerLiteral.Value!,
                upperLiteral.Value!,
                lowerLiteral.Shape);
            frame.Projection.Add(
                field.Reference,
                ruleName,
                Message(component, defaultMessage),
                ValidationRuleDetails.WithConstraint(ValidationConstraint.InclusiveRange(bounds), frame.ParentCondition, bounds.Shape));
        }

        private static void ProjectComparison(
            IRuleComponent component,
            IComparisonValidator comparison,
            ProjectedField field,
            ProjectionFrame frame)
        {
            var ruleName = RuleNameFor(comparison.Comparison, comparison.MemberToCompare != null);
            if (ruleName == null)
            {
                frame.Projection.Skip(field.Path, component.Validator, ClientRuleProjectionSkipReason.UnsupportedComparisonOperator);
                return;
            }

            if (comparison.MemberToCompare != null)
            {
                ProjectPeerComparison(component, comparison, ruleName, field, frame);
                return;
            }

            var literal = PlanLiteral.From(comparison.ValueToCompare);
            frame.Projection.Add(
                field.Reference,
                ruleName,
                Message(component, LiteralComparisonMessage(comparison.Comparison, field.DisplayName, literal.Value)),
                ValidationRuleDetails.WithConstraint(literal.Value, frame.ParentCondition, literal.Shape));
        }

        private static void ProjectPeerComparison(
            IRuleComponent component,
            IComparisonValidator comparison,
            ValidationRuleName ruleName,
            ProjectedField field,
            ProjectionFrame frame)
        {
            var peer = field.PeerFieldFor(comparison.MemberToCompare!);
            if (peer.SkipReason.HasValue)
            {
                frame.Projection.Skip(field.Path, component.Validator, peer.SkipReason.Value);
                return;
            }

            frame.Projection.Ensure(peer.Reference);
            frame.Projection.Add(
                field.Reference,
                ruleName,
                Message(component, PeerComparisonMessage(comparison.Comparison, field.DisplayName, peer.Reference.Path)),
                ValidationRuleDetails.WithPeerField(peer.Reference.Path, frame.ParentCondition, peer.Reference.Shape));
        }

        private static RuleScope ResolveRuleScope(IValidationRule rule, ProjectionFrame frame)
        {
            if (!rule.HasCondition && !rule.HasAsyncCondition)
                return RuleScope.Project(ValidationRuleCondition.Always);

            return frame.ClientConditions.TryGet(rule, out var condition)
                ? condition.Match(
                    (fieldCondition, fields) =>
                    {
                        foreach (var field in fields)
                            frame.Projection.Ensure(field.PrefixedBy(frame.Prefix));

                        var binding = new FieldConditionPrefixBinding(frame.Prefix, _ => { });
                        return RuleScope.Project(ValidationRuleCondition.When(fieldCondition.PrefixWith(binding)));
                    },
                    RuleScope.Skip)
                : RuleScope.Skip(ClientRuleProjectionSkipReason.FluentValidationConditionWithoutClientGuard);
        }

        private static void ProjectChildValidators(IValidationRule rule, ProjectionFrame frame)
        {
            foreach (var component in rule.Components)
            {
                if (component.Validator is not IChildValidatorAdaptor child) continue;

                var nested = ResolveNestedValidator(frame.Factory, child.ValidatorType);
                ProjectValidator(nested, frame.ForIncludedValidator(nested));
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

        private static ValidationMessage Message(IRuleComponent component, string defaultMessage) =>
            Message(component, ValidationMessage.Of(defaultMessage));

        private static ValidationMessage Message(IRuleComponent component, ValidationMessage defaultMessage)
        {
            var raw = component.GetUnformattedErrorMessage();
            return !string.IsNullOrEmpty(raw) && !raw.Contains('{')
                ? ValidationMessage.Of(raw)
                : defaultMessage;
        }

        private static ValidationRuleName? RuleNameFor(Comparison comparison, bool peerComparison)
        {
            return comparison switch
            {
                Comparison.Equal => ValidationRuleName.EqualTo,
                Comparison.NotEqual => peerComparison ? ValidationRuleName.NotEqualTo : ValidationRuleName.NotEqual,
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

        private static string PeerComparisonMessage(Comparison comparison, string fieldName, ValidationFieldPath peer)
        {
            var peerName = Humanize(peer);
            return comparison switch
            {
                Comparison.Equal => $"'{fieldName}' must match '{peerName}'.",
                Comparison.NotEqual => $"'{fieldName}' must not match '{peerName}'.",
                Comparison.GreaterThanOrEqual => $"'{fieldName}' must be at least '{peerName}'.",
                Comparison.LessThanOrEqual => $"'{fieldName}' must be at most '{peerName}'.",
                Comparison.GreaterThan => $"'{fieldName}' must be greater than '{peerName}'.",
                Comparison.LessThan => $"'{fieldName}' must be less than '{peerName}'.",
                _ => $"'{fieldName}' is invalid."
            };
        }

        private static string Humanize(ValidationFieldPath fieldPath) => Humanize(fieldPath.Value);

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

        private sealed class ProjectionFrame
        {
            private ProjectionFrame(
                ValidationFieldPath prefix,
                ValidationRuleCondition parentCondition,
                Func<Type, IValidator?> factory,
                ClientValidationProjectionAccumulator projection,
                ClientConditionCatalog clientConditions)
            {
                Prefix = prefix;
                ParentCondition = parentCondition;
                Factory = factory;
                Projection = projection;
                ClientConditions = clientConditions;
            }

            internal ValidationFieldPath Prefix { get; }
            internal ValidationRuleCondition ParentCondition { get; }
            internal Func<Type, IValidator?> Factory { get; }
            internal ClientValidationProjectionAccumulator Projection { get; }
            internal ClientConditionCatalog ClientConditions { get; }

            internal static ProjectionFrame Root(
                Func<Type, IValidator?> factory,
                ClientValidationProjectionAccumulator projection,
                ClientConditionCatalog clientConditions) =>
                new ProjectionFrame(ValidationFieldPath.Empty, ValidationRuleCondition.Always, factory, projection, clientConditions);

            internal ProjectionFrame WithParentCondition(ValidationRuleCondition condition) =>
                new ProjectionFrame(Prefix, condition, Factory, Projection, ClientConditions);

            internal ProjectionFrame ForIncludedValidator(IValidator validator) =>
                new ProjectionFrame(Prefix, ParentCondition, Factory, Projection, ClientConditionCatalog.From(validator));

            internal ProjectionFrame ForNestedValidator(IValidator validator, ValidationFieldPath prefix) =>
                new ProjectionFrame(prefix, ParentCondition, Factory, Projection, ClientConditionCatalog.From(validator));

            internal ValidationFieldPath FieldPath(IValidationRule rule) => Prefix.Append(rule.PropertyName);
        }

        private sealed class ClientConditionCatalog
        {
            private readonly IReadOnlyDictionary<IValidationRule, ClientConditionProjection> _conditions;

            private ClientConditionCatalog(IReadOnlyDictionary<IValidationRule, ClientConditionProjection> conditions)
            {
                _conditions = conditions;
            }

            internal static ClientConditionCatalog From(IValidator validator) =>
                validator is IClientConditionSource source
                    ? new ClientConditionCatalog(source.ClientConditions)
                    : new ClientConditionCatalog(new Dictionary<IValidationRule, ClientConditionProjection>());

            internal bool TryGet(IValidationRule rule, out ClientConditionProjection condition) =>
                _conditions.TryGetValue(rule, out condition!);
        }

        private readonly struct RuleScope
        {
            private RuleScope(ValidationRuleCondition condition, ClientRuleProjectionSkipReason? skipReason)
            {
                Condition = condition;
                SkipReason = skipReason;
            }

            internal ValidationRuleCondition Condition { get; }
            internal ClientRuleProjectionSkipReason? SkipReason { get; }

            internal static RuleScope Project(ValidationRuleCondition condition) =>
                new RuleScope(condition, null);

            internal static RuleScope Skip(ClientRuleProjectionSkipReason reason) =>
                new RuleScope(ValidationRuleCondition.Always, reason);
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
                ClientValidationRule rule,
                ValidationMessage message,
                ValidationRuleCondition condition)
            {
                Add(field, rule.Name, message, rule.DetailsFor(condition));
            }

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
            private readonly Type? _ownerType;

            private ProjectedField(
                string propertyName,
                ValidationFieldPath path,
                ClientValidationFieldReference reference,
                Type? ownerType)
            {
                PropertyName = propertyName;
                Path = path;
                Reference = reference;
                DisplayName = Humanize(propertyName);
                _ownerType = ownerType;
            }

            internal string PropertyName { get; }
            internal ValidationFieldPath Path { get; }
            internal ClientValidationFieldReference Reference { get; }
            internal string DisplayName { get; }

            internal PeerFieldProjection PeerFieldFor(System.Reflection.MemberInfo member)
            {
                if (_ownerType == null || member.DeclaringType == null)
                    return PeerFieldProjection.Skip(ClientRuleProjectionSkipReason.UnknownPeerFieldScope);

                if (!member.DeclaringType.IsAssignableFrom(_ownerType))
                    return PeerFieldProjection.Skip(ClientRuleProjectionSkipReason.CrossObjectPeerComparison);

                var shape = ShapeFor(member);
                if (shape.IsAny)
                    return PeerFieldProjection.Skip(ClientRuleProjectionSkipReason.UnsupportedPeerShape);

                var peerPath = Path.Parent().Append(member.Name);
                return PeerFieldProjection.Project(ClientValidationFieldReference.Of(peerPath, shape));
            }

            internal static ProjectedField From(ValidationFieldPath prefix, IValidationRule rule)
            {
                var path = prefix.Append(rule.PropertyName);
                return new ProjectedField(
                    rule.PropertyName,
                    path,
                    ClientValidationFieldReference.Of(path, Shape.FromClrType(rule.TypeToValidate)),
                    rule.Member?.DeclaringType);
            }

            private static Shape ShapeFor(System.Reflection.MemberInfo member)
            {
                if (member is System.Reflection.PropertyInfo property)
                    return Shape.FromClrType(property.PropertyType);

                if (member is System.Reflection.FieldInfo field)
                    return Shape.FromClrType(field.FieldType);

                return Shape.Any;
            }
        }

        private readonly struct PeerFieldProjection
        {
            private PeerFieldProjection(ClientValidationFieldReference reference, ClientRuleProjectionSkipReason? skipReason)
            {
                Reference = reference;
                SkipReason = skipReason;
            }

            internal ClientValidationFieldReference Reference { get; }
            internal ClientRuleProjectionSkipReason? SkipReason { get; }

            internal static PeerFieldProjection Project(ClientValidationFieldReference reference) =>
                new PeerFieldProjection(reference, null);

            internal static PeerFieldProjection Skip(ClientRuleProjectionSkipReason reason) =>
                new PeerFieldProjection(null!, reason);
        }

        private sealed class PlanLiteral
        {
            private PlanLiteral(object? value, Shape shape)
            {
                Value = value;
                Shape = shape;
            }

            internal object? Value { get; }
            internal Shape Shape { get; }

            internal static PlanLiteral From(object? value)
            {
                if (value == null)
                    return new PlanLiteral(null, Shape.None);

                var shape = Shape.FromClrType(value.GetType());
                var serialized = shape == Shape.Date
                    ? ValidationDateLiteral.From(value, shape)
                    : value;

                return new PlanLiteral(serialized, shape);
            }
        }
    }
}
