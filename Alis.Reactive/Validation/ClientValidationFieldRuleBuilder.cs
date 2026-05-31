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
        private readonly ClientRuleActivation _activation;

        internal ClientValidationFieldRuleBuilder(
            ClientValidationRuleSet rules,
            ClientValidationFieldToken<TModel, TValue> field,
            ClientRuleActivation activation)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _activation = activation ?? throw new ArgumentNullException(nameof(activation));
        }

        public ClientValidationFieldRuleBuilder<TModel, TValue> Required(string message) =>
            AddNoOperand(RuleName.Required, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Empty(string message) =>
            AddNoOperand(RuleName.Empty, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Email(string message) =>
            AddNoOperand(RuleName.Email, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Url(string message) =>
            AddNoOperand(RuleName.Url, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> CreditCard(string message) =>
            AddNoOperand(RuleName.CreditCard, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> AtLeastOne(string message) =>
            AddNoOperand(RuleName.AtLeastOne, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> MinLength(int length, string message)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Minimum length must not be negative.");
            return AddLiteral(RuleName.MinLength, length, Shape.None, message);
        }

        public ClientValidationFieldRuleBuilder<TModel, TValue> MaxLength(int length, string message)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "Maximum length must not be negative.");
            return AddLiteral(RuleName.MaxLength, length, Shape.None, message);
        }

        public ClientValidationFieldRuleBuilder<TModel, TValue> Regex(string pattern, string message)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("A regex pattern is required for a client validation rule.", nameof(pattern));

            return AddLiteral(RuleName.Regex, pattern, Shape.None, message);
        }

        public ClientValidationFieldRuleBuilder<TModel, TValue> Range(
            TValue lowerBound,
            TValue upperBound,
            string message) =>
            AddRange(RuleName.Range, lowerBound, upperBound, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> ExclusiveRange(
            TValue lowerBound,
            TValue upperBound,
            string message) =>
            AddRange(RuleName.ExclusiveRange, lowerBound, upperBound, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Min(TValue minimum, string message) =>
            AddLiteralComparison(RuleName.Min, minimum, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> Max(TValue maximum, string message) =>
            AddLiteralComparison(RuleName.Max, maximum, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThanOrEqualTo(TValue minimum, string message) =>
            AddLiteralComparison(RuleName.Min, minimum, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThanOrEqualTo(TValue maximum, string message) =>
            AddLiteralComparison(RuleName.Max, maximum, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThan(TValue value, string message) =>
            AddLiteralComparison(RuleName.Gt, value, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThan(TValue value, string message) =>
            AddLiteralComparison(RuleName.Lt, value, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> EqualTo(TValue expected, string message) =>
            AddLiteralComparison(RuleName.EqualTo, expected, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> EqualTo(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            EqualTo(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> EqualTo(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(RuleName.EqualTo, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> NotEqual(TValue forbidden, string message) =>
            AddLiteralComparison(RuleName.NotEqual, forbidden, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> NotEqualTo(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            NotEqualTo(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> NotEqualTo(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(RuleName.NotEqualTo, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThan(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            GreaterThan(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThan(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(RuleName.Gt, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThanOrEqualTo(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            GreaterThanOrEqualTo(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> GreaterThanOrEqualTo(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(RuleName.Min, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThan(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            LessThan(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThan(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(RuleName.Lt, peerField, message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThanOrEqualTo(
            Expression<Func<TModel, TValue>> peerField,
            string message) =>
            LessThanOrEqualTo(ClientValidationFieldToken<TModel, TValue>.For(peerField), message);

        public ClientValidationFieldRuleBuilder<TModel, TValue> LessThanOrEqualTo(
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message) =>
            AddPeerComparison(RuleName.Max, peerField, message);

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddNoOperand(
            RuleName name,
            string message) =>
            AddRule(
                name,
                message,
                RuleOperand.None,
                Shape.None);

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddLiteralComparison(
            RuleName name,
            TValue value,
            string message)
        {
            var literal = ClientValidationLiteral.From(value);
            return AddLiteral(name, literal.Value, literal.Shape, message);
        }

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddLiteral(
            RuleName name,
            object? value,
            Shape shape,
            string message) =>
            AddRule(
                name,
                message,
                RuleOperand.Literal(value, shape),
                shape);

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddRange(
            RuleName name,
            TValue lowerBound,
            TValue upperBound,
            string message)
        {
            var bounds = ValidationRangeBounds.FromClientLiteral(lowerBound, upperBound);
            return AddRule(
                name,
                message,
                RuleOperand.Range(bounds),
                bounds.Shape);
        }

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddPeerComparison(
            RuleName name,
            ClientValidationFieldToken<TModel, TValue> peerField,
            string message)
        {
            if (peerField == null) throw new ArgumentNullException(nameof(peerField));

            _rules.EnsureField(peerField.Reference);
            return AddRule(
                name,
                message,
                RuleOperand.PeerField(
                    peerField.Reference.Path,
                    peerField.Reference.Shape),
                peerField.Reference.Shape);
        }

        private ClientValidationFieldRuleBuilder<TModel, TValue> AddRule(
            RuleName name,
            string message,
            RuleOperand operand,
            Shape shape)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (shape == null) throw new ArgumentNullException(nameof(shape));

            _rules.AddRule(
                _field.Reference,
                new ClientRule(name, ValidationMessage.Of(message), operand, _activation, shape));
            return this;
        }
    }
}
