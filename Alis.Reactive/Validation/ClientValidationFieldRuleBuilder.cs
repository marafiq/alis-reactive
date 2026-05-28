using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Writes rules for one client validation field.
    /// </summary>
    public sealed class ClientValidationFieldRuleBuilder<TModel, TValue>
        where TModel : class
    {
        private readonly ClientValidationRuleSet _rules;
        private readonly ClientValidationFieldToken<TModel, TValue> _field;
        private readonly ValidationRuleActivation _activation;

        internal ClientValidationFieldRuleBuilder(
            ClientValidationRuleSet rules,
            ClientValidationFieldToken<TModel, TValue> field,
            ValidationRuleActivation activation)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _activation = activation ?? throw new ArgumentNullException(nameof(activation));
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
                throw new ArgumentException("A regex pattern is required for a client validation rule.", nameof(pattern));

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

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThanOrEqualTo(TValue minimum, string message) =>
            AddLiteralComparison(ValidationRuleName.Min, minimum, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThanOrEqualTo(TValue maximum, string message) =>
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

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThan(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            GreaterThan(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThan(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(ValidationRuleName.Gt, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThanOrEqualTo(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            GreaterThanOrEqualTo(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThanOrEqualTo(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(ValidationRuleName.Min, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThan(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            LessThan(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThan(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(ValidationRuleName.Lt, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThanOrEqualTo(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            LessThanOrEqualTo(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThanOrEqualTo(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(ValidationRuleName.Max, peerField, message);

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddNoOperand(
            ValidationRuleName name,
            string message) =>
            AddRule(
                name,
                message,
                ValidationRuleOperand.None,
                Shape.None);

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddLiteralComparison(
            ValidationRuleName name,
            TValue value,
            string message)
        {
            var literal = ClientValidationLiteral.From(value);
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
                ValidationRuleOperand.Literal(value, shape),
                shape);

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddRange(
            ValidationRuleName name,
            TValue lowerBound,
            TValue upperBound,
            string message)
        {
            var bounds = ValidationRangeBounds.FromClientLiteral(lowerBound, upperBound);
            return AddRule(
                name,
                message,
                ValidationRuleOperand.Range(bounds),
                bounds.Shape);
        }

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddPeerComparison(
            ValidationRuleName name,
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message)
        {
            if (peerField == null) throw new ArgumentNullException(nameof(peerField));

            _rules.EnsureField(peerField.Reference);
            return AddRule(
                name,
                message,
                ValidationRuleOperand.PeerField(
                    peerField.Reference.Path,
                    peerField.Reference.Shape),
                peerField.Reference.Shape);
        }

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddRule(
            ValidationRuleName name,
            string message,
            ValidationRuleOperand operand,
            Shape shape)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (shape == null) throw new ArgumentNullException(nameof(shape));

            _rules.AddRule(
                _field.Reference,
                new ValidationRule(name, ValidationMessage.Of(message), operand, _activation, shape));
            return this;
        }
    }
}
