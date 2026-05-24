using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Writes rules for one projected client validation field.
    /// </summary>
    public sealed class ClientValidationFieldRuleBuilder<TModel, TValue>
        where TModel : class
    {
        private readonly ClientValidationProjectionDraft _projection;
        private readonly ClientValidationFieldToken<TModel, TValue> _field;
        private readonly ValidationRuleCondition _condition;

        internal ClientValidationFieldRuleBuilder(
            ClientValidationProjectionDraft projection,
            ClientValidationFieldToken<TModel, TValue> field,
            ValidationRuleCondition condition)
        {
            _projection = projection ?? throw new ArgumentNullException(nameof(projection));
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public ClientValidationFieldRuleBuilder<TModel, TValue> Required(string message) =>
            AddNoOperand(ValidationRuleName.Required, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Empty(string message) =>
            AddNoOperand(ValidationRuleName.Empty, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Email(string message) =>
            AddNoOperand(ValidationRuleName.Email, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Url(string message) =>
            AddNoOperand(ValidationRuleName.Url, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> CreditCard(string message) =>
            AddNoOperand(ValidationRuleName.CreditCard, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> AtLeastOne(string message) =>
            AddNoOperand(ValidationRuleName.AtLeastOne, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> MinLength(int length, string message)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Minimum length must not be negative.");
            return AddLiteral(ValidationRuleName.MinLength, length, Shape.None, message);
        }

        public ClientValidationFieldRuleBuilder<TModel, TValue> MaxLength(int length, string message)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Maximum length must not be negative.");
            return AddLiteral(ValidationRuleName.MaxLength, length, Shape.None, message);
        }

        public ClientValidationFieldRuleBuilder<TModel, TValue> Regex(string pattern, string message)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("A regex pattern is required for client validation projection.", nameof(pattern));

            return AddLiteral(ValidationRuleName.Regex, pattern, Shape.None, message);
        }

        public ClientValidationFieldRuleBuilder<TModel, TValue> Range(
            TValue lowerBound,
            TValue upperBound,
            string message) =>
            AddRange(ValidationRuleName.Range, lowerBound, upperBound, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> ExclusiveRange(
            TValue lowerBound,
            TValue upperBound,
            string message) =>
            AddRange(ValidationRuleName.ExclusiveRange, lowerBound, upperBound, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Min(TValue minimum, string message) =>
            AddLiteralComparison(ValidationRuleName.Min, minimum, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Max(TValue maximum, string message) =>
            AddLiteralComparison(ValidationRuleName.Max, maximum, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThan(TValue value, string message) =>
            AddLiteralComparison(ValidationRuleName.Gt, value, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThan(TValue value, string message) =>
            AddLiteralComparison(ValidationRuleName.Lt, value, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> EqualTo(TValue expected, string message) =>
            AddLiteralComparison(ValidationRuleName.EqualTo, expected, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> EqualTo(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            EqualTo(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> EqualTo(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(ValidationRuleName.EqualTo, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> NotEqual(TValue forbidden, string message) =>
            AddLiteralComparison(ValidationRuleName.NotEqual, forbidden, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> NotEqualTo(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            NotEqualTo(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> NotEqualTo(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(ValidationRuleName.NotEqualTo, peerField, message);

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddNoOperand(
            ValidationRuleName name,
            string message) =>
            AddRule(
                name,
                message,
                ValidationRuleDetails.NoOperand(_condition));

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddLiteralComparison(
            ValidationRuleName name,
            TValue value,
            string message)
        {
            var literal = ClientValidationProjectionLiteral.From(value);
            return AddLiteral(name, literal.Value, literal.Shape, message);
        }

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddLiteral(
            ValidationRuleName name,
            object? value,
            Shape shape,
            string message) =>
            AddRule(
                name,
                message,
                ValidationRuleDetails.WithConstraint(value, _condition, shape));

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddRange(
            ValidationRuleName name,
            TValue lowerBound,
            TValue upperBound,
            string message)
        {
            var bounds = ClientValidationProjectionRangeBounds.From(lowerBound, upperBound);
            return AddRule(
                name,
                message,
                ValidationRuleDetails.WithConstraint(bounds.ToValidationRangeBounds(), _condition, bounds.EndpointShape));
        }

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddPeerComparison(
            ValidationRuleName name,
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message)
        {
            if (peerField == null) throw new ArgumentNullException(nameof(peerField));

            _projection.EnsureField(peerField.Reference);
            return AddRule(
                name,
                message,
                ValidationRuleDetails.WithPeerField(
                    peerField.Reference.Path,
                    _condition,
                    peerField.Reference.Shape));
        }

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddRule(
            ValidationRuleName name,
            string message,
            ValidationRuleDetails details)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (details == null) throw new ArgumentNullException(nameof(details));

            _projection.AddRule(
                _field.Reference,
                new ValidationRule(name, ValidationMessage.Of(message), details));
            return this;
        }
    }
}
