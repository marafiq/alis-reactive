using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.FluentValidator.Validators;
using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator
{
    public sealed class ReactiveClientRuleBuilder<TModel, TValue>
        where TModel : class
    {
        private readonly IRuleBuilder<TModel, TValue> _serverRule;
        private readonly ClientValidationFieldRuleBuilder<TModel, TValue> _clientRule;

        internal ReactiveClientRuleBuilder(
            IRuleBuilder<TModel, TValue> serverRule,
            ClientValidationFieldRuleBuilder<TModel, TValue> clientRule)
        {
            _serverRule = serverRule ?? throw new ArgumentNullException(nameof(serverRule));
            _clientRule = clientRule ?? throw new ArgumentNullException(nameof(clientRule));
        }

        internal ReactiveClientRuleBuilder<TModel, TValue> Add(
            Action<IRuleBuilder<TModel, TValue>> serverRule,
            Action<ClientValidationFieldRuleBuilder<TModel, TValue>> clientRule)
        {
            serverRule(_serverRule);
            clientRule(_clientRule);
            return this;
        }
    }

    public sealed class ReactiveClientCollectionRuleBuilder<TModel, TItem>
        where TModel : class
        where TItem : class
    {
        private readonly IRuleBuilder<TModel, IEnumerable<TItem>> _collectionRule;
        private readonly IRuleBuilder<TModel, TItem> _itemRule;
        private readonly ClientValidationFieldRuleBuilder<TModel, IEnumerable<TItem>> _clientRule;
        private readonly ClientValidationRuleSet _clientRules;
        private readonly ClientValidationFieldReference _collection;
        private readonly ClientRuleActivation _activation;

        internal ReactiveClientCollectionRuleBuilder(
            IRuleBuilder<TModel, IEnumerable<TItem>> collectionRule,
            IRuleBuilder<TModel, TItem> itemRule,
            ClientValidationFieldRuleBuilder<TModel, IEnumerable<TItem>> clientRule,
            ClientValidationRuleSet clientRules,
            ClientValidationFieldReference collection,
            ClientRuleActivation activation)
        {
            _collectionRule = collectionRule ?? throw new ArgumentNullException(nameof(collectionRule));
            _itemRule = itemRule ?? throw new ArgumentNullException(nameof(itemRule));
            _clientRule = clientRule ?? throw new ArgumentNullException(nameof(clientRule));
            _clientRules = clientRules ?? throw new ArgumentNullException(nameof(clientRules));
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _activation = activation ?? throw new ArgumentNullException(nameof(activation));
        }

        public ReactiveClientCollectionRuleBuilder<TModel, TItem> AtLeastOne(string message)
        {
            _collectionRule.NotEmpty().WithMessage(message);
            _clientRule.AtLeastOne(message);
            return this;
        }

        public ReactiveClientCollectionRuleBuilder<TModel, TItem> SetValidator(
            ReactiveValidator<TItem> validator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            _itemRule.SetValidator(validator);
            _clientRules.AddItemFields(
                _collection,
                ((IClientValidationMetadataSource)validator).GetClientRules(),
                _activation);
            return this;
        }
    }

    public static class ReactiveClientRules
    {
        public static ReactiveClientRuleBuilder<TModel, TValue> Required<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            string message)
            where TModel : class =>
            Add(rule, server => server.NotEmpty().WithMessage(message), client => client.Required(message));

        public static ReactiveClientRuleBuilder<TModel, TValue> Empty<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            string message)
            where TModel : class =>
            Add(rule, server => server.IsEmpty().WithMessage(message), client => client.Empty(message));

        public static ReactiveClientRuleBuilder<TModel, string?> Email<TModel>(
            this ReactiveClientRuleBuilder<TModel, string?> rule,
            string message)
            where TModel : class =>
            Add(rule, server => server.EmailAddress().WithMessage(message), client => client.Email(message));

        public static ReactiveClientRuleBuilder<TModel, string?> Url<TModel>(
            this ReactiveClientRuleBuilder<TModel, string?> rule,
            string message)
            where TModel : class =>
            Add(rule, server => server.Must(IsEmptyOrHttpUrl).WithMessage(message), client => client.Url(message));

        public static ReactiveClientRuleBuilder<TModel, string?> CreditCard<TModel>(
            this ReactiveClientRuleBuilder<TModel, string?> rule,
            string message)
            where TModel : class =>
            Add(rule, server => server.CreditCard().WithMessage(message), client => client.CreditCard(message));

        public static ReactiveClientRuleBuilder<TModel, TValue> AtLeastOne<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            string message)
            where TModel : class =>
            Add(rule, server => server.NotEmpty().WithMessage(message), client => client.AtLeastOne(message));

        public static ReactiveClientRuleBuilder<TModel, string?> MinLength<TModel>(
            this ReactiveClientRuleBuilder<TModel, string?> rule,
            int length,
            string message)
            where TModel : class =>
            Add(rule, server => server.MinimumLength(length).WithMessage(message), client => client.MinLength(length, message));

        public static ReactiveClientRuleBuilder<TModel, string?> MaxLength<TModel>(
            this ReactiveClientRuleBuilder<TModel, string?> rule,
            int length,
            string message)
            where TModel : class =>
            Add(rule, server => server.MaximumLength(length).WithMessage(message), client => client.MaxLength(length, message));

        public static ReactiveClientRuleBuilder<TModel, string?> Regex<TModel>(
            this ReactiveClientRuleBuilder<TModel, string?> rule,
            string pattern,
            string message)
            where TModel : class =>
            Add(rule, server => server.Matches(pattern).WithMessage(message), client => client.Regex(pattern, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> Range<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue lowerBound,
            TValue upperBound,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.InclusiveBetween(lowerBound, upperBound).WithMessage(message), client => client.Range(lowerBound, upperBound, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> Range<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            TValue lowerBound,
            TValue upperBound,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.InclusiveBetween(lowerBound, upperBound).WithMessage(message), client => client.Range(lowerBound, upperBound, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> ExclusiveRange<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue lowerBound,
            TValue upperBound,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.IsExclusiveBetween(lowerBound, upperBound).WithMessage(message), client => client.ExclusiveRange(lowerBound, upperBound, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> ExclusiveRange<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            TValue lowerBound,
            TValue upperBound,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.ExclusiveBetween(lowerBound, upperBound).WithMessage(message), client => client.ExclusiveRange(lowerBound, upperBound, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> Min<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue minimum,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            rule.GreaterThanOrEqualTo(minimum, message);

        public static ReactiveClientRuleBuilder<TModel, TValue?> Min<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            TValue minimum,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            rule.GreaterThanOrEqualTo(minimum, message);

        public static ReactiveClientRuleBuilder<TModel, TValue> Max<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue maximum,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            rule.LessThanOrEqualTo(maximum, message);

        public static ReactiveClientRuleBuilder<TModel, TValue?> Max<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            TValue maximum,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            rule.LessThanOrEqualTo(maximum, message);

        public static ReactiveClientRuleBuilder<TModel, TValue> GreaterThanOrEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue minimum,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.GreaterThanOrEqualTo(minimum).WithMessage(message), client => client.GreaterThanOrEqualTo(minimum, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> GreaterThanOrEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            TValue minimum,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.GreaterThanOrEqualTo(minimum).WithMessage(message), client => client.GreaterThanOrEqualTo(minimum, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> LessThanOrEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue maximum,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.LessThanOrEqualTo(maximum).WithMessage(message), client => client.LessThanOrEqualTo(maximum, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> LessThanOrEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            TValue maximum,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.LessThanOrEqualTo(maximum).WithMessage(message), client => client.LessThanOrEqualTo(maximum, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> GreaterThan<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue value,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.GreaterThan(value).WithMessage(message), client => client.GreaterThan(value, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> GreaterThan<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            TValue value,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.GreaterThan(value).WithMessage(message), client => client.GreaterThan(value, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> LessThan<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue value,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.LessThan(value).WithMessage(message), client => client.LessThan(value, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> LessThan<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            TValue value,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.LessThan(value).WithMessage(message), client => client.LessThan(value, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> EqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue expected,
            string message)
            where TModel : class =>
            Add(rule, server => server.Equal(expected).WithMessage(message), client => client.EqualTo(expected, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> EqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            Expression<Func<TModel, TValue>> peerField,
            string message)
            where TModel : class =>
            Add(rule, server => server.Equal(peerField).WithMessage(message), client => client.EqualTo(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> NotEqual<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            TValue forbidden,
            string message)
            where TModel : class =>
            Add(rule, server => server.NotEqual(forbidden).WithMessage(message), client => client.NotEqual(forbidden, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> NotEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            Expression<Func<TModel, TValue>> peerField,
            string message)
            where TModel : class =>
            Add(rule, server => server.NotEqual(peerField).WithMessage(message), client => client.NotEqualTo(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> GreaterThan<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            Expression<Func<TModel, TValue>> peerField,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.GreaterThan(peerField).WithMessage(message), client => client.GreaterThan(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> GreaterThan<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            Expression<Func<TModel, TValue?>> peerField,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.GreaterThan(peerField).WithMessage(message), client => client.GreaterThan(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> GreaterThanOrEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            Expression<Func<TModel, TValue>> peerField,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.GreaterThanOrEqualTo(peerField).WithMessage(message), client => client.GreaterThanOrEqualTo(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> GreaterThanOrEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            Expression<Func<TModel, TValue?>> peerField,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.GreaterThanOrEqualTo(peerField).WithMessage(message), client => client.GreaterThanOrEqualTo(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> LessThan<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            Expression<Func<TModel, TValue>> peerField,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.LessThan(peerField).WithMessage(message), client => client.LessThan(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> LessThan<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            Expression<Func<TModel, TValue?>> peerField,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.LessThan(peerField).WithMessage(message), client => client.LessThan(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue> LessThanOrEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue> rule,
            Expression<Func<TModel, TValue>> peerField,
            string message)
            where TModel : class
            where TValue : IComparable<TValue>, IComparable =>
            Add(rule, server => server.LessThanOrEqualTo(peerField).WithMessage(message), client => client.LessThanOrEqualTo(peerField, message));

        public static ReactiveClientRuleBuilder<TModel, TValue?> LessThanOrEqualTo<TModel, TValue>(
            this ReactiveClientRuleBuilder<TModel, TValue?> rule,
            Expression<Func<TModel, TValue?>> peerField,
            string message)
            where TModel : class
            where TValue : struct, IComparable<TValue>, IComparable =>
            Add(rule, server => server.LessThanOrEqualTo(peerField).WithMessage(message), client => client.LessThanOrEqualTo(peerField, message));

        private static ReactiveClientRuleBuilder<TModel, TValue> Add<TModel, TValue>(
            ReactiveClientRuleBuilder<TModel, TValue> rule,
            Action<IRuleBuilder<TModel, TValue>> serverRule,
            Action<ClientValidationFieldRuleBuilder<TModel, TValue>> clientRule)
            where TModel : class
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            return rule.Add(serverRule, clientRule);
        }

        private static bool IsEmptyOrHttpUrl(string? value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return false;
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}
