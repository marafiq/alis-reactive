using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    public abstract class ReactiveValidator<T> :
        AbstractValidator<T>,
        IClientValidationMetadataSource
        where T : class
    {
        private readonly ClientValidationRuleSet _clientRules = new ClientValidationRuleSet();
        private readonly ClientConditionScope _scope = new ClientConditionScope();

        IReadOnlyList<ClientValidationField> IClientValidationMetadataSource.GetClientRules() =>
            _clientRules.ToFields();

        protected ReactiveClientRuleBuilder<T, TValue> ClientRule<TValue>(
            Expression<Func<T, TValue>> field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            var token = ClientValidationFieldToken<T, TValue>.For(field);
            var activation = _scope.ActiveClientRuleActivation(_clientRules);
            _clientRules.EnsureField(token.Reference);
            var clientRule = new ClientValidationFieldRuleBuilder<T, TValue>(
                _clientRules,
                token,
                activation);
            return new ReactiveClientRuleBuilder<T, TValue>(
                RuleFor(field),
                clientRule);
        }

        protected ReactiveClientCollectionRuleBuilder<T, TItem> ClientRuleEach<TItem>(
            Expression<Func<T, IEnumerable<TItem>>> field)
            where TItem : class
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            var token = ClientValidationFieldToken<T, IEnumerable<TItem>>.For(field);
            var activation = _scope.ActiveClientRuleActivation(_clientRules);
            _clientRules.EnsureField(token.Reference);
            var clientRule = new ClientValidationFieldRuleBuilder<T, IEnumerable<TItem>>(
                _clientRules,
                token,
                activation);
            return new ReactiveClientCollectionRuleBuilder<T, TItem>(
                RuleFor(field),
                RuleForEach(field),
                clientRule,
                _clientRules,
                token.Reference,
                activation);
        }

        protected void ClientRule<TChild>(
            Expression<Func<T, TChild>> field,
            ReactiveValidator<TChild> validator)
            where TChild : class
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            RuleFor(field).SetValidator(validator);

            var prefix = ValidationFieldPath.Of(ExpressionPathHelper.ToPropertyName(field));
            _clientRules.AddRulesFrom(
                ((IClientValidationMetadataSource)validator).GetClientRules(),
                prefix,
                _scope.ActiveClientRuleActivation(_clientRules));
        }

        protected void ClientRulesFrom(ReactiveValidator<T> validator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            Include(validator);

            _clientRules.AddRulesFrom(
                ((IClientValidationMetadataSource)validator).GetClientRules(),
                ValidationFieldPath.Empty,
                _scope.ActiveClientRuleActivation(_clientRules));
        }

        protected void ClientRulesFrom<TSource>(ReactiveValidator<TSource> validator)
            where TSource : class
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            _clientRules.AddRulesFrom(
                ((IClientValidationMetadataSource)validator).GetClientRules(),
                ValidationFieldPath.Empty,
                _scope.ActiveClientRuleActivation(_clientRules));
        }

        protected void WhenField(Expression<Func<T, bool>> field, Action defineRules) =>
            Apply(new FieldStart<T, bool>(field).Truthy(), defineRules);

        protected void WhenField<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).Eq(value), defineRules);

        protected void WhenFieldNot(Expression<Func<T, bool>> field, Action defineRules) =>
            Apply(new FieldStart<T, bool>(field).Falsy(), defineRules);

        protected void WhenFieldNot<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).Neq(value), defineRules);

        protected void WhenFieldGt<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).Gt(value), defineRules);

        protected void WhenFieldGte<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).Gte(value), defineRules);

        protected void WhenFieldLt<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).Lt(value), defineRules);

        protected void WhenFieldLte<TProp>(Expression<Func<T, TProp>> field, TProp value, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).Lte(value), defineRules);

        protected void WhenFieldNull<TProp>(Expression<Func<T, TProp>> field, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).IsNull(), defineRules);

        protected void WhenFieldNotNull<TProp>(Expression<Func<T, TProp>> field, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).NotNull(), defineRules);

        protected void WhenFieldEmpty(Expression<Func<T, string?>> field, Action defineRules) =>
            Apply(new FieldStart<T, string?>(field).IsEmpty(), defineRules);

        protected void WhenFieldNotEmpty(Expression<Func<T, string?>> field, Action defineRules) =>
            Apply(new FieldStart<T, string?>(field).NotEmpty(), defineRules);

        protected void WhenFieldIn<TProp>(Expression<Func<T, TProp>> field, TProp[] values, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).In(values), defineRules);

        protected void WhenFieldNotIn<TProp>(Expression<Func<T, TProp>> field, TProp[] values, Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).NotIn(values), defineRules);

        protected void WhenFieldBetween<TProp>(
            Expression<Func<T, TProp>> field,
            TProp low,
            TProp high,
            Action defineRules) =>
            Apply(new FieldStart<T, TProp>(field).Between(low, high), defineRules);

        protected void WhenFieldContains(Expression<Func<T, string?>> field, string substring, Action defineRules) =>
            Apply(new FieldStart<T, string?>(field).Contains(substring), defineRules);

        protected void WhenFieldStartsWith(Expression<Func<T, string?>> field, string prefix, Action defineRules) =>
            Apply(new FieldStart<T, string?>(field).StartsWith(prefix), defineRules);

        protected void WhenFieldEndsWith(Expression<Func<T, string?>> field, string suffix, Action defineRules) =>
            Apply(new FieldStart<T, string?>(field).EndsWith(suffix), defineRules);

        protected void WhenFieldMatches(Expression<Func<T, string?>> field, string pattern, Action defineRules) =>
            Apply(new FieldStart<T, string?>(field).Matches(pattern), defineRules);

        protected void WhenFieldMinLength(Expression<Func<T, string?>> field, int minLength, Action defineRules) =>
            Apply(new FieldStart<T, string?>(field).MinLength(minLength), defineRules);

        protected void WhenFieldArrayContains<TProp>(
            Expression<Func<T, IEnumerable<TProp>?>> field,
            TProp value,
            Action defineRules)
        {
            var selected = SelectedClientValidationField<T, IEnumerable<TProp>?>.From(field);
            Apply(
                selected.GuardAgainstCollectionItem(
                    CompareOperator.ArrayContains,
                    value,
                    candidate => FieldConditionPredicates.EnumerableContains(candidate, value)),
                defineRules);
        }

        protected void WhenFields(
            Func<FieldConditionBuilder<T>, FieldGuard<T>> buildCondition,
            Action defineRules)
        {
            if (buildCondition == null) throw new ArgumentNullException(nameof(buildCondition));
            Apply(buildCondition(new FieldConditionBuilder<T>()), defineRules);
        }

        public new IConditionBuilder When(Func<T, bool> predicate, Action action) =>
            ApplyServerOnlyCondition(predicate, action, guarded => base.When(predicate, guarded));

        public new IConditionBuilder When(Func<T, ValidationContext<T>, bool> predicate, Action action) =>
            ApplyServerOnlyCondition(predicate, action, guarded => base.When(predicate, guarded));

        public new IConditionBuilder Unless(Func<T, bool> predicate, Action action) =>
            ApplyServerOnlyCondition(predicate, action, guarded => base.Unless(predicate, guarded));

        public new IConditionBuilder Unless(Func<T, ValidationContext<T>, bool> predicate, Action action) =>
            ApplyServerOnlyCondition(predicate, action, guarded => base.Unless(predicate, guarded));

        public new IConditionBuilder WhenAsync(
            Func<T, CancellationToken, Task<bool>> predicate,
            Action action) =>
            ApplyServerOnlyCondition(predicate, action, guarded => base.WhenAsync(predicate, guarded));

        public new IConditionBuilder WhenAsync(
            Func<T, ValidationContext<T>, CancellationToken, Task<bool>> predicate,
            Action action) =>
            ApplyServerOnlyCondition(predicate, action, guarded => base.WhenAsync(predicate, guarded));

        public new IConditionBuilder UnlessAsync(
            Func<T, CancellationToken, Task<bool>> predicate,
            Action action) =>
            ApplyServerOnlyCondition(predicate, action, guarded => base.UnlessAsync(predicate, guarded));

        public new IConditionBuilder UnlessAsync(
            Func<T, ValidationContext<T>, CancellationToken, Task<bool>> predicate,
            Action action) =>
            ApplyServerOnlyCondition(predicate, action, guarded => base.UnlessAsync(predicate, guarded));

        private void Apply(FieldGuard<T> guard, Action defineRules)
        {
            if (guard == null) throw new ArgumentNullException(nameof(guard));
            if (defineRules == null) throw new ArgumentNullException(nameof(defineRules));

            using (_scope.Enter(guard))
            {
                base.When(guard.ServerPredicate, defineRules);
            }
        }

        private IConditionBuilder ApplyServerOnlyCondition<TPredicate>(
            TPredicate predicate,
            Action defineRules,
            Func<Action, IConditionBuilder> applyCondition)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (defineRules == null) throw new ArgumentNullException(nameof(defineRules));
            if (applyCondition == null) throw new ArgumentNullException(nameof(applyCondition));

            using (_scope.EnterServerOnlyCondition())
                return new ServerOnlyConditionBuilder(applyCondition(defineRules), _scope);
        }

        private sealed class ClientConditionScope
        {
            private readonly Stack<FieldGuard<T>> _clientConditions = new Stack<FieldGuard<T>>();
            private int _serverOnlyDepth;

            internal IDisposable Enter(FieldGuard<T> condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                _clientConditions.Push(condition);
                return new Exit(this, scope => scope.ExitClient());
            }

            internal IDisposable EnterServerOnlyCondition()
            {
                _serverOnlyDepth++;
                return new Exit(this, scope => scope.ExitServerOnlyCondition());
            }

            internal ClientRuleActivation ActiveClientRuleActivation(ClientValidationRuleSet rules)
            {
                if (rules == null) throw new ArgumentNullException(nameof(rules));

                if (_serverOnlyDepth > 0)
                {
                    throw new InvalidOperationException(
                        "ClientRule cannot be declared inside FluentValidation When, Unless, WhenAsync, or UnlessAsync. " +
                        "Use WhenField for a Reactive Plan field condition, or keep the rule server-only with RuleFor.");
                }

                if (_clientConditions.Count == 0)
                    return ClientRuleActivation.Always;

                var conditions = _clientConditions.Reverse().ToArray();
                var fieldCondition = ActiveClientCondition(conditions);
                var fields = ClientValidationGuardFields.From(conditions.SelectMany(condition => condition.Fields));
                rules.EnsureFields(fields);
                return ClientRuleActivation.When(fieldCondition);
            }

            private static FieldCondition ActiveClientCondition(IReadOnlyList<FieldGuard<T>> conditions) =>
                conditions.Count == 1
                    ? conditions[0].Condition
                    : FieldCondition.All(conditions.Select(condition => condition.Condition).ToArray());

            private void ExitClient()
            {
                if (_clientConditions.Count == 0)
                    throw new InvalidOperationException("Cannot exit a client validation condition scope that was not entered.");

                _clientConditions.Pop();
            }

            private void ExitServerOnlyCondition()
            {
                if (_serverOnlyDepth == 0)
                    throw new InvalidOperationException("Cannot exit a server-only validation condition scope that was not entered.");

                _serverOnlyDepth--;
            }

            private sealed class Exit : IDisposable
            {
                private ClientConditionScope? _scope;
                private readonly Action<ClientConditionScope> _exit;

                internal Exit(ClientConditionScope scope, Action<ClientConditionScope> exit)
                {
                    _scope = scope ?? throw new ArgumentNullException(nameof(scope));
                    _exit = exit ?? throw new ArgumentNullException(nameof(exit));
                }

                public void Dispose()
                {
                    var scope = _scope;
                    if (scope == null) return;

                    _scope = null;
                    _exit(scope);
                }
            }
        }

        private sealed class ServerOnlyConditionBuilder : IConditionBuilder
        {
            private readonly IConditionBuilder _inner;
            private readonly ClientConditionScope _scope;

            internal ServerOnlyConditionBuilder(IConditionBuilder inner, ClientConditionScope scope)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            }

            public void Otherwise(Action action)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));
                _inner.Otherwise(() =>
                {
                    using (_scope.EnterServerOnlyCondition())
                        action();
                });
            }
        }
    }
}
