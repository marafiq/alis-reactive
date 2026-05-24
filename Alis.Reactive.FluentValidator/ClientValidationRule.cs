using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using FluentValidation;
using FluentValidation.Internal;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// FluentValidation rule extensions for declaring deterministic browser validation.
    /// </summary>
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

                ClientValidationRuleProjectionCatalog.Register(component, clientRule);
            });
        }
    }

    /// <summary>
    /// Writes the deterministic browser-side projection for a FluentValidation rule.
    /// </summary>
    public sealed class ClientValidationRuleWriter<TModel, TProperty>
    {
        internal ClientValidationRuleWriter() { }

        public ClientValidationRule Required() =>
            ClientValidationRule.NoOperand(
                ValidationRuleName.Required,
                displayName => $"'{displayName}' is required.");

        public ClientValidationRule Empty() =>
            ClientValidationRule.NoOperand(
                ValidationRuleName.Empty,
                displayName => $"'{displayName}' must be empty.");

        public ClientValidationRule MinLength(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Minimum length must not be negative.");
            return ClientValidationRule.Literal(
                ValidationRuleName.MinLength,
                length,
                Shape.None,
                displayName => $"'{displayName}' must be at least {length} characters.");
        }

        public ClientValidationRule MaxLength(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Maximum length must not be negative.");
            return ClientValidationRule.Literal(
                ValidationRuleName.MaxLength,
                length,
                Shape.None,
                displayName => $"'{displayName}' must be at most {length} characters.");
        }

        public ClientValidationRule Email() =>
            ClientValidationRule.NoOperand(
                ValidationRuleName.Email,
                displayName => $"'{displayName}' must be a valid email address.");

        public ClientValidationRule Regex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("A regex pattern is required for client validation projection.", nameof(pattern));

            return ClientValidationRule.Literal(
                ValidationRuleName.Regex,
                pattern,
                Shape.None,
                displayName => $"'{displayName}' format is invalid.");
        }

        public ClientValidationRule Url() =>
            ClientValidationRule.NoOperand(
                ValidationRuleName.Url,
                displayName => $"'{displayName}' must be a valid URL.");

        public ClientValidationRule CreditCard() =>
            ClientValidationRule.NoOperand(
                ValidationRuleName.CreditCard,
                displayName => $"'{displayName}' must be a valid credit card number.");

        public ClientValidationRule Range(TProperty lowerBound, TProperty upperBound) =>
            RangeRule(
                ValidationRuleName.Range,
                lowerBound,
                upperBound,
                displayName => $"'{displayName}' must be between {lowerBound} and {upperBound}.");

        public ClientValidationRule ExclusiveRange(TProperty lowerBound, TProperty upperBound) =>
            RangeRule(
                ValidationRuleName.ExclusiveRange,
                lowerBound,
                upperBound,
                displayName => $"'{displayName}' must be between {lowerBound} and {upperBound} (exclusive).");

        public ClientValidationRule Min(TProperty minimum) =>
            LiteralComparison(
                ValidationRuleName.Min,
                minimum,
                displayName => $"'{displayName}' must be at least {minimum}.");

        public ClientValidationRule Max(TProperty maximum) =>
            LiteralComparison(
                ValidationRuleName.Max,
                maximum,
                displayName => $"'{displayName}' must be at most {maximum}.");

        public ClientValidationRule GreaterThan(TProperty value) =>
            LiteralComparison(
                ValidationRuleName.Gt,
                value,
                displayName => $"'{displayName}' must be greater than {value}.");

        public ClientValidationRule LessThan(TProperty value) =>
            LiteralComparison(
                ValidationRuleName.Lt,
                value,
                displayName => $"'{displayName}' must be less than {value}.");

        public ClientValidationRule EqualTo(TProperty expected) =>
            LiteralComparison(
                ValidationRuleName.EqualTo,
                expected,
                displayName => $"'{displayName}' must equal {expected}.");

        public ClientValidationRule EqualTo(Expression<Func<TModel, TProperty>> peerField) =>
            PeerComparison(
                ValidationRuleName.EqualTo,
                peerField,
                displayName => $"'{displayName}' must match '{Humanize(peerField)}'.");

        public ClientValidationRule NotEqual(TProperty forbidden) =>
            LiteralComparison(
                ValidationRuleName.NotEqual,
                forbidden,
                displayName => $"'{displayName}' must not equal '{forbidden}'.");

        public ClientValidationRule NotEqualTo(Expression<Func<TModel, TProperty>> peerField) =>
            PeerComparison(
                ValidationRuleName.NotEqualTo,
                peerField,
                displayName => $"'{displayName}' must not match '{Humanize(peerField)}'.");

        public ClientValidationRule AtLeastOne() =>
            ClientValidationRule.NoOperand(
                ValidationRuleName.AtLeastOne,
                displayName => $"'{displayName}' must contain at least one value.");

        private static ClientValidationRule RangeRule(
            ValidationRuleName name,
            TProperty lowerBound,
            TProperty upperBound,
            Func<string, string> defaultMessage)
        {
            var bounds = ClientValidationRangeBounds.From(lowerBound, upperBound);
            return ClientValidationRule.Range(name, bounds, defaultMessage);
        }

        private static ClientValidationRule LiteralComparison(
            ValidationRuleName name,
            TProperty value,
            Func<string, string> defaultMessage)
        {
            var literal = ClientValidationLiteral.From(value);
            return ClientValidationRule.Literal(name, literal.Value, literal.Shape, defaultMessage);
        }

        private static ClientValidationRule PeerComparison(
            ValidationRuleName name,
            Expression<Func<TModel, TProperty>> peerField,
            Func<string, string> defaultMessage)
        {
            if (peerField == null) throw new ArgumentNullException(nameof(peerField));

            return ClientValidationRule.PeerField(
                name,
                ValidationFieldPath.Of(ExpressionPathHelper.ToPropertyName(peerField)),
                Shape.FromClrType(typeof(TProperty)),
                defaultMessage);
        }

        private static string Humanize(Expression<Func<TModel, TProperty>> peerField)
        {
            if (peerField == null) throw new ArgumentNullException(nameof(peerField));
            return ExpressionPathHelper.ToPropertyName(peerField).Replace(".", " ");
        }
    }

    public sealed class ClientValidationRule
    {
        private readonly ClientValidationRuleOperand _operand;
        private readonly Func<string, string> _defaultMessage;

        private ClientValidationRule(
            ValidationRuleName name,
            ClientValidationRuleOperand operand,
            Shape comparisonShape,
            Func<string, string> defaultMessage)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _operand = operand ?? throw new ArgumentNullException(nameof(operand));
            ComparisonShape = comparisonShape ?? throw new ArgumentNullException(nameof(comparisonShape));
            _defaultMessage = defaultMessage ?? throw new ArgumentNullException(nameof(defaultMessage));
        }

        internal ValidationRuleName Name { get; }
        internal Shape ComparisonShape { get; }

        internal static ClientValidationRule NoOperand(
            ValidationRuleName name,
            Func<string, string> defaultMessage) =>
            new ClientValidationRule(
                name,
                ClientValidationRuleOperand.None,
                Shape.None,
                defaultMessage);

        internal static ClientValidationRule Literal(
            ValidationRuleName name,
            object? value,
            Shape shape,
            Func<string, string> defaultMessage) =>
            new ClientValidationRule(
                name,
                ClientValidationRuleOperand.Literal(value),
                shape,
                defaultMessage);

        internal static ClientValidationRule Range(
            ValidationRuleName name,
            ClientValidationRangeBounds bounds,
            Func<string, string> defaultMessage)
        {
            if (bounds == null) throw new ArgumentNullException(nameof(bounds));
            return new ClientValidationRule(
                name,
                ClientValidationRuleOperand.Range(bounds),
                bounds.EndpointShape,
                defaultMessage);
        }

        internal static ClientValidationRule PeerField(
            ValidationRuleName name,
            ValidationFieldPath peerField,
            Shape shape,
            Func<string, string> defaultMessage) =>
            new ClientValidationRule(
                name,
                ClientValidationRuleOperand.PeerField(peerField),
                shape,
                defaultMessage);

        internal ValidationMessage MessageFor(string displayName)
        {
            if (displayName == null) throw new ArgumentNullException(nameof(displayName));
            return ValidationMessage.Of(_defaultMessage(displayName));
        }

        internal ValidationRuleDetails DetailsFor(ValidationRuleCondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            return _operand.ToDetails(condition, ComparisonShape);
        }
    }

    internal abstract class ClientValidationRuleOperand
    {
        private protected ClientValidationRuleOperand() { }

        internal static ClientValidationRuleOperand None { get; } =
            new NoClientValidationRuleOperand();

        internal static ClientValidationRuleOperand Literal(object? value) =>
            new LiteralClientValidationRuleOperand(value);

        internal static ClientValidationRuleOperand Range(ClientValidationRangeBounds bounds)
        {
            if (bounds == null) throw new ArgumentNullException(nameof(bounds));
            return new RangeClientValidationRuleOperand(bounds);
        }

        internal static ClientValidationRuleOperand PeerField(ValidationFieldPath fieldPath)
        {
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
            return new PeerFieldClientValidationRuleOperand(fieldPath);
        }

        internal abstract ValidationRuleDetails ToDetails(
            ValidationRuleCondition condition,
            Shape comparisonShape);

        private sealed class NoClientValidationRuleOperand : ClientValidationRuleOperand
        {
            internal override ValidationRuleDetails ToDetails(
                ValidationRuleCondition condition,
                Shape comparisonShape)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                if (comparisonShape == null) throw new ArgumentNullException(nameof(comparisonShape));
                return ValidationRuleDetails.NoOperand(condition);
            }
        }

        private sealed class LiteralClientValidationRuleOperand : ClientValidationRuleOperand
        {
            private readonly object? _value;

            internal LiteralClientValidationRuleOperand(object? value)
            {
                _value = value;
            }

            internal override ValidationRuleDetails ToDetails(
                ValidationRuleCondition condition,
                Shape comparisonShape)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                if (comparisonShape == null) throw new ArgumentNullException(nameof(comparisonShape));
                return ValidationRuleDetails.WithConstraint(_value, condition, comparisonShape);
            }
        }

        private sealed class RangeClientValidationRuleOperand : ClientValidationRuleOperand
        {
            private readonly ClientValidationRangeBounds _bounds;

            internal RangeClientValidationRuleOperand(ClientValidationRangeBounds bounds)
            {
                _bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
            }

            internal override ValidationRuleDetails ToDetails(
                ValidationRuleCondition condition,
                Shape comparisonShape)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                if (comparisonShape == null) throw new ArgumentNullException(nameof(comparisonShape));
                return ValidationRuleDetails.WithConstraint(_bounds.ToValidationRangeBounds(), condition, _bounds.EndpointShape);
            }
        }

        private sealed class PeerFieldClientValidationRuleOperand : ClientValidationRuleOperand
        {
            private readonly ValidationFieldPath _fieldPath;

            internal PeerFieldClientValidationRuleOperand(ValidationFieldPath fieldPath)
            {
                _fieldPath = fieldPath ?? throw new ArgumentNullException(nameof(fieldPath));
            }

            internal override ValidationRuleDetails ToDetails(
                ValidationRuleCondition condition,
                Shape comparisonShape)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                if (comparisonShape == null) throw new ArgumentNullException(nameof(comparisonShape));
                return ValidationRuleDetails.WithPeerField(_fieldPath, condition, comparisonShape);
            }
        }
    }

    internal sealed class ClientValidationLiteral
    {
        private ClientValidationLiteral(object? value, Shape shape)
        {
            Value = value;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal object? Value { get; }
        internal Shape Shape { get; }

        internal static ClientValidationLiteral From<TValue>(TValue value)
        {
            if (value == null)
                return new ClientValidationLiteral(null, Shape.None);

            var shape = Shape.FromClrType(value.GetType());
            return new ClientValidationLiteral(ValidationDateLiteral.From(value, shape), shape);
        }
    }

    internal sealed class ClientValidationRangeBounds
    {
        private ClientValidationRangeBounds(object lowerBound, object upperBound, Shape endpointShape)
        {
            LowerBound = lowerBound ?? throw new ArgumentNullException(nameof(lowerBound));
            UpperBound = upperBound ?? throw new ArgumentNullException(nameof(upperBound));
            EndpointShape = endpointShape ?? throw new ArgumentNullException(nameof(endpointShape));
        }

        internal object LowerBound { get; }
        internal object UpperBound { get; }
        internal Shape EndpointShape { get; }

        internal static ClientValidationRangeBounds From<TValue>(TValue lowerBound, TValue upperBound)
        {
            if (lowerBound == null) throw new ArgumentNullException(nameof(lowerBound));
            if (upperBound == null) throw new ArgumentNullException(nameof(upperBound));

            var lowerLiteral = ClientValidationLiteral.From(lowerBound);
            var upperLiteral = ClientValidationLiteral.From(upperBound);
            var endpointsHaveSameShape = lowerLiteral.Shape.Equals(upperLiteral.Shape);
            if (!endpointsHaveSameShape)
            {
                throw new ArgumentException(
                    "Client validation range bounds must have the same shape. " +
                    $"Lower bound is '{lowerLiteral.Shape.Kind}', upper bound is '{upperLiteral.Shape.Kind}'.");
            }

            return new ClientValidationRangeBounds(
                lowerLiteral.Value!,
                upperLiteral.Value!,
                lowerLiteral.Shape);
        }

        internal ValidationRangeBounds ToValidationRangeBounds() =>
            ValidationRangeBounds.Between(LowerBound, UpperBound, EndpointShape);
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
