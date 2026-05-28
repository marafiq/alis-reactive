using System;
using System.Runtime.CompilerServices;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Alis.Reactive.Validation;
using Shape = Alis.Reactive.PlanModel.Shape;

namespace Alis.Reactive.FluentValidator
{
    public static class ClientValidationRuleExtensions
    {
        public static IRuleBuilderOptions<TModel, TProperty> ClientRule<TModel, TProperty>(
            this IRuleBuilderOptions<TModel, TProperty> ruleBuilder,
            Func<ClientValidationRuleWriter<TModel, TProperty>, ClientValidationRule> writeRule)
        {
            if (ruleBuilder == null) throw new ArgumentNullException(nameof(ruleBuilder));
            if (writeRule == null) throw new ArgumentNullException(nameof(writeRule));

            var clientRule = writeRule(new ClientValidationRuleWriter<TModel, TProperty>());
            if (clientRule == null) throw new ArgumentException("A client validation rule is required.", nameof(writeRule));

            return ruleBuilder.Configure(rule =>
            {
                var component = rule.Current;
                if (component == null)
                {
                    throw new InvalidOperationException(
                        "ClientRule must be called after a FluentValidation property validator, " +
                        "for example RuleFor(x => x.Code).Must(...).ClientRule(rule => rule.Regex(...)).");
                }

                if (component.Validator is IAsyncPropertyValidator<TModel, TProperty>)
                    return;

                ClientValidationRuleBridge.Register(component, clientRule);
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
                throw new ArgumentException("A regex pattern is required for a client validation rule.", nameof(pattern));

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

        public ClientValidationRule GreaterThan(System.Linq.Expressions.Expression<Func<TModel, TProperty>> peerField) =>
            Peer(ValidationRuleName.Gt, peerField, name => $"'{name}' must be greater than '{Humanize(peerField)}'.");

        public ClientValidationRule GreaterThanOrEqualTo(System.Linq.Expressions.Expression<Func<TModel, TProperty>> peerField) =>
            Peer(ValidationRuleName.Min, peerField, name => $"'{name}' must be at least '{Humanize(peerField)}'.");

        public ClientValidationRule LessThan(System.Linq.Expressions.Expression<Func<TModel, TProperty>> peerField) =>
            Peer(ValidationRuleName.Lt, peerField, name => $"'{name}' must be less than '{Humanize(peerField)}'.");

        public ClientValidationRule LessThanOrEqualTo(System.Linq.Expressions.Expression<Func<TModel, TProperty>> peerField) =>
            Peer(ValidationRuleName.Max, peerField, name => $"'{name}' must be at most '{Humanize(peerField)}'.");

        private static ClientValidationRule NoOperand(ValidationRuleName rule, Func<string, string> message) =>
            new ClientValidationRule(rule, message, _ => ValidationRuleOperand.None, Shape.None);

        private static ClientValidationRule Literal(
            ValidationRuleName rule,
            TProperty value,
            Func<string, string> message)
        {
            var literal = ClientValidationLiteral.From(value);
            return ClientValidationRule.Literal(rule, literal.Value, literal.Shape, message);
        }

        private static ClientValidationRule Range(
            ValidationRuleName rule,
            TProperty lowerBound,
            TProperty upperBound,
            Func<string, string> message)
        {
            var bounds = ValidationRangeBounds.FromClientLiteral(lowerBound, upperBound);
            return new ClientValidationRule(
                rule,
                message,
                _ => ValidationRuleOperand.Range(bounds),
                bounds.Shape);
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
                prefix => ValidationRuleOperand.PeerField(prefix.Append(field), shape),
                shape);
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
        private readonly Func<ValidationFieldPath, ValidationRuleOperand> _operand;
        private readonly Shape _shape;

        internal ClientValidationRule(
            ValidationRuleName name,
            Func<string, string> message,
            Func<ValidationFieldPath, ValidationRuleOperand> operand,
            Shape shape)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _operand = operand ?? throw new ArgumentNullException(nameof(operand));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
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
                _ => ValidationRuleOperand.Literal(value, shape),
                shape);

        internal ValidationMessage MessageFor(string displayName)
        {
            if (displayName == null) throw new ArgumentNullException(nameof(displayName));
            return ValidationMessage.Of(_message(displayName));
        }

        internal ValidationRule ToValidationRule(
            ValidationMessage message,
            ClientRuleActivation activation,
            ValidationFieldPath fieldPrefix)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (activation == null) throw new ArgumentNullException(nameof(activation));
            if (fieldPrefix == null) throw new ArgumentNullException(nameof(fieldPrefix));
            return new ValidationRule(
                Name,
                message,
                _operand(fieldPrefix),
                activation,
                _shape);
        }
    }

    internal static class ClientValidationRuleBridge
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
