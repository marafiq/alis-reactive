using System;
using System.Runtime.CompilerServices;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    public static class ClientValidationRuleExtensions
    {
        public static IRuleBuilderOptions<TModel, TProperty> ProjectToClient<TModel, TProperty>(
            this IRuleBuilderOptions<TModel, TProperty> ruleBuilder,
            Func<ClientValidationRuleWriter<TModel, TProperty>, ClientValidationRule> writeRule)
        {
            if (ruleBuilder == null) throw new ArgumentNullException(nameof(ruleBuilder));
            if (writeRule == null) throw new ArgumentNullException(nameof(writeRule));

            var clientRule = writeRule(new ClientValidationRuleWriter<TModel, TProperty>());
            if (clientRule == null) throw new ArgumentException("A client validation rule projection is required.", nameof(writeRule));

            return ruleBuilder.Configure(rule =>
            {
                var component = rule.Current;
                if (component == null)
                {
                    throw new InvalidOperationException(
                        "ProjectToClient must be called after a FluentValidation property validator, " +
                        "for example RuleFor(x => x.Code).Must(...).ProjectToClient(rule => rule.Regex(...)).");
                }

                if (component.Validator is IAsyncPropertyValidator<TModel, TProperty>)
                    return;

                ClientValidationRuleProjectionCatalog.Register(component, clientRule);
            });
        }
    }

    public sealed class ClientValidationRuleWriter<TModel, TProperty>
    {
        internal ClientValidationRuleWriter() { }

        public ClientValidationRule Required() => NoOperand(ValidationRuleName.Required, name => $"'{name}' is required.");
        public ClientValidationRule Empty() => NoOperand(ValidationRuleName.Empty, name => $"'{name}' must be empty.");
        public ClientValidationRule Email() => NoOperand(ValidationRuleName.Email, name => $"'{name}' must be a valid email address.");
        public ClientValidationRule Url() => NoOperand(ValidationRuleName.Url, name => $"'{name}' must be a valid URL.");
        public ClientValidationRule CreditCard() => NoOperand(ValidationRuleName.CreditCard, name => $"'{name}' must be a valid credit card number.");
        public ClientValidationRule AtLeastOne() => NoOperand(ValidationRuleName.AtLeastOne, name => $"'{name}' must contain at least one value.");
        public ClientValidationRule Min(TProperty value) => Literal(ValidationRuleName.Min, value, name => $"'{name}' must be at least {value}.");
        public ClientValidationRule Max(TProperty value) => Literal(ValidationRuleName.Max, value, name => $"'{name}' must be at most {value}.");
        public ClientValidationRule GreaterThan(TProperty value) => Literal(ValidationRuleName.Gt, value, name => $"'{name}' must be greater than {value}.");
        public ClientValidationRule LessThan(TProperty value) => Literal(ValidationRuleName.Lt, value, name => $"'{name}' must be less than {value}.");
        public ClientValidationRule EqualTo(TProperty value) => Literal(ValidationRuleName.EqualTo, value, name => $"'{name}' must equal {value}.");
        public ClientValidationRule NotEqual(TProperty value) => Literal(ValidationRuleName.NotEqual, value, name => $"'{name}' must not equal '{value}'.");

        public ClientValidationRule MinLength(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Minimum length must not be negative.");
            return ClientValidationRule.Literal(ValidationRuleName.MinLength, length, Shape.None, name => $"'{name}' must be at least {length} characters.");
        }

        public ClientValidationRule MaxLength(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Maximum length must not be negative.");
            return ClientValidationRule.Literal(ValidationRuleName.MaxLength, length, Shape.None, name => $"'{name}' must be at most {length} characters.");
        }

        public ClientValidationRule Regex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("A regex pattern is required for client validation projection.", nameof(pattern));

            return ClientValidationRule.Literal(ValidationRuleName.Regex, pattern, Shape.None, name => $"'{name}' format is invalid.");
        }

        public ClientValidationRule Range(TProperty lowerBound, TProperty upperBound) =>
            Range(ValidationRuleName.Range, lowerBound, upperBound, name => $"'{name}' must be between {lowerBound} and {upperBound}.");

        public ClientValidationRule ExclusiveRange(TProperty lowerBound, TProperty upperBound) =>
            Range(ValidationRuleName.ExclusiveRange, lowerBound, upperBound, name => $"'{name}' must be between {lowerBound} and {upperBound} (exclusive).");

        public ClientValidationRule EqualTo(System.Linq.Expressions.Expression<Func<TModel, TProperty>> peerField) =>
            Peer(ValidationRuleName.EqualTo, peerField, name => $"'{name}' must match '{Humanize(peerField)}'.");

        public ClientValidationRule NotEqualTo(System.Linq.Expressions.Expression<Func<TModel, TProperty>> peerField) =>
            Peer(ValidationRuleName.NotEqualTo, peerField, name => $"'{name}' must not match '{Humanize(peerField)}'.");

        private static ClientValidationRule NoOperand(ValidationRuleName rule, Func<string, string> message) =>
            new ClientValidationRule(rule, message, condition => ValidationRuleDetails.NoOperand(condition));

        private static ClientValidationRule Literal(
            ValidationRuleName rule,
            TProperty value,
            Func<string, string> message)
        {
            var literal = ClientValidationProjectionLiteral.From(value);
            return ClientValidationRule.Literal(rule, literal.Value, literal.Shape, message);
        }

        private static ClientValidationRule Range(
            ValidationRuleName rule,
            TProperty lowerBound,
            TProperty upperBound,
            Func<string, string> message)
        {
            var bounds = ClientValidationProjectionRangeBounds.From(lowerBound, upperBound);
            return new ClientValidationRule(
                rule,
                message,
                condition => ValidationRuleDetails.WithConstraint(
                    bounds.ToValidationRangeBounds(),
                    condition,
                    bounds.EndpointShape));
        }

        private static ClientValidationRule Peer(
            ValidationRuleName rule,
            System.Linq.Expressions.Expression<Func<TModel, TProperty>> peerField,
            Func<string, string> message)
        {
            if (peerField == null) throw new ArgumentNullException(nameof(peerField));

            var field = ValidationFieldPath.Of(ExpressionPathHelper.ToPropertyName(peerField));
            var shape = Shape.FromClrType(typeof(TProperty));
            return new ClientValidationRule(
                rule,
                message,
                condition => ValidationRuleDetails.WithPeerField(field, condition, shape));
        }

        private static string Humanize(System.Linq.Expressions.Expression<Func<TModel, TProperty>> peerField)
        {
            if (peerField == null) throw new ArgumentNullException(nameof(peerField));
            return ExpressionPathHelper.ToPropertyName(peerField).Replace(".", " ");
        }
    }

    public sealed class ClientValidationRule
    {
        private readonly Func<string, string> _message;
        private readonly Func<ValidationRuleCondition, ValidationRuleDetails> _details;

        internal ClientValidationRule(
            ValidationRuleName name,
            Func<string, string> message,
            Func<ValidationRuleCondition, ValidationRuleDetails> details)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _details = details ?? throw new ArgumentNullException(nameof(details));
        }

        internal ValidationRuleName Name { get; }

        internal static ClientValidationRule Literal(
            ValidationRuleName rule,
            object? value,
            Shape shape,
            Func<string, string> message) =>
            new ClientValidationRule(
                rule,
                message,
                condition => ValidationRuleDetails.WithConstraint(value, condition, shape));

        internal ValidationMessage MessageFor(string displayName)
        {
            if (displayName == null) throw new ArgumentNullException(nameof(displayName));
            return ValidationMessage.Of(_message(displayName));
        }

        internal ValidationRuleDetails DetailsFor(ValidationRuleCondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            return _details(condition);
        }
    }

    internal static class ClientValidationRuleProjectionCatalog
    {
        private static readonly ConditionalWeakTable<IRuleComponent, ClientValidationRule> Rules =
            new ConditionalWeakTable<IRuleComponent, ClientValidationRule>();

        internal static void Register(IRuleComponent component, ClientValidationRule rule)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            Rules.Remove(component);
            Rules.Add(component, rule);
        }

        internal static bool TryFind(IRuleComponent component, out ClientValidationRule rule)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return Rules.TryGetValue(component, out rule!);
        }
    }
}
