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
    /// <summary>
    /// Base class for validators that need client-side conditional rules.
    /// Use WhenField() instead of FV's .When() when a condition should also be
    /// projected into browser validation. FV's .When() still runs on the server
    /// and is intentionally skipped by client projection.
    /// </summary>
    public abstract class ReactiveValidator<T> : AbstractValidator<T>, IClientConditionSource
        where T : class
    {
        private readonly Dictionary<IValidationRule, ClientConditionProjection> _clientConditions =
            new Dictionary<IValidationRule, ClientConditionProjection>();
        private readonly ClientConditionScope _clientConditionScope = new ClientConditionScope();

        IReadOnlyDictionary<IValidationRule, ClientConditionProjection> IClientConditionSource.ClientConditions =>
            _clientConditions;

        protected override void OnRuleAdded(IValidationRule<T> rule)
        {
            base.OnRuleAdded(rule);
            _clientConditionScope.Register(rule, _clientConditions);
        }

        // ── Existing operators (unchanged signatures) ──────────────────────────

        /// <summary>
        /// Applies a "truthy" condition to all rules defined in the block.
        /// Server: FV's When() runs the condition at validation time.
        /// Client: Adapter extracts rules with the truthy comparison operator.
        /// </summary>
        /// <remarks>
        /// For bool fields. Server predicate evaluates the bool expression directly.
        /// For generic-typed truthy conditions via the composition API, use
        /// <c>WhenFields</c> with <c>FieldStart.Truthy()</c> which checks all falsy
        /// values (null, false, 0, empty string).
        /// </remarks>
        protected void WhenField(Expression<Func<T, bool>> conditionField, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, bool>(conditionField).Truthy(), defineRules);
        }

        /// <summary>
        /// Applies an "eq" condition to all rules defined in the block.
        /// Server: FV's When() checks field == value at validation time.
        /// Client: Adapter extracts rules with the equality comparison operator.
        /// </summary>
        protected void WhenField<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).Eq(value), defineRules);
        }

        /// <summary>
        /// Applies a "falsy" condition to all rules defined in the block.
        /// Server: FV's When() runs !condition at validation time.
        /// Client: Adapter extracts rules with the falsy comparison operator.
        /// </summary>
        protected void WhenFieldNot(Expression<Func<T, bool>> conditionField, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, bool>(conditionField).Falsy(), defineRules);
        }

        /// <summary>
        /// Applies a "neq" condition to all rules defined in the block.
        /// Server: FV's When() checks field != value at validation time.
        /// Client: Adapter extracts rules with the not-equal comparison operator.
        /// </summary>
        protected void WhenFieldNot<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).Neq(value), defineRules);
        }

        // ── Ordering operators ─────────────────────────────────────────────────

        /// <summary>Applies a "gt" (greater than) condition.</summary>
        protected void WhenFieldGt<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).Gt(value), defineRules);
        }

        /// <summary>Applies a "gte" (greater than or equal) condition.</summary>
        protected void WhenFieldGte<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).Gte(value), defineRules);
        }

        /// <summary>Applies a "lt" (less than) condition.</summary>
        protected void WhenFieldLt<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).Lt(value), defineRules);
        }

        /// <summary>Applies a "lte" (less than or equal) condition.</summary>
        protected void WhenFieldLte<TProp>(
            Expression<Func<T, TProp>> field, TProp value, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).Lte(value), defineRules);
        }

        // ── Presence operators ─────────────────────────────────────────────────

        /// <summary>Applies an "is-null" condition.</summary>
        protected void WhenFieldNull<TProp>(
            Expression<Func<T, TProp>> field, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).IsNull(), defineRules);
        }

        /// <summary>Applies a "not-null" condition.</summary>
        protected void WhenFieldNotNull<TProp>(
            Expression<Func<T, TProp>> field, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).NotNull(), defineRules);
        }

        /// <summary>Applies an "is-empty" condition (null or empty string).</summary>
        protected void WhenFieldEmpty(
            Expression<Func<T, string?>> field, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, string?>(field).IsEmpty(), defineRules);
        }

        /// <summary>Applies a "not-empty" condition (non-null and non-empty string).</summary>
        protected void WhenFieldNotEmpty(
            Expression<Func<T, string?>> field, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, string?>(field).NotEmpty(), defineRules);
        }

        // ── Membership operators ───────────────────────────────────────────────

        /// <summary>Applies an "in" condition — field value is in the given set.</summary>
        protected void WhenFieldIn<TProp>(
            Expression<Func<T, TProp>> field, TProp[] values, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).In(values), defineRules);
        }

        // ── Text operators ─────────────────────────────────────────────────────

        /// <summary>Applies a "contains" condition — string field contains substring.</summary>
        protected void WhenFieldContains(
            Expression<Func<T, string?>> field, string substring, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, string?>(field).Contains(substring), defineRules);
        }

        /// <summary>Applies a "starts-with" condition.</summary>
        protected void WhenFieldStartsWith(
            Expression<Func<T, string?>> field, string prefix, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, string?>(field).StartsWith(prefix), defineRules);
        }

        /// <summary>Applies an "ends-with" condition.</summary>
        protected void WhenFieldEndsWith(
            Expression<Func<T, string?>> field, string suffix, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, string?>(field).EndsWith(suffix), defineRules);
        }

        /// <summary>Applies a "not-in" condition — field value is NOT in the given set.</summary>
        protected void WhenFieldNotIn<TProp>(
            Expression<Func<T, TProp>> field, TProp[] values, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).NotIn(values), defineRules);
        }

        /// <summary>Applies a "between" condition — field value is between low and high (inclusive).</summary>
        protected void WhenFieldBetween<TProp>(
            Expression<Func<T, TProp>> field, TProp low, TProp high, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, TProp>(field).Between(low, high), defineRules);
        }

        /// <summary>Applies a "matches" condition — string field matches regex pattern.</summary>
        protected void WhenFieldMatches(
            Expression<Func<T, string?>> field, string pattern, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, string?>(field).Matches(pattern), defineRules);
        }

        /// <summary>Applies a "min-length" condition — string field length >= minimum.</summary>
        protected void WhenFieldMinLength(
            Expression<Func<T, string?>> field, int minLength, Action defineRules)
        {
            ApplyClientCondition(new FieldStart<T, string?>(field).MinLength(minLength), defineRules);
        }

        /// <summary>Applies an "array-contains" condition — array field contains the given element.</summary>
        protected void WhenFieldArrayContains<TProp>(
            Expression<Func<T, IEnumerable<TProp>?>> field, TProp value, Action defineRules)
        {
            var selectedField = SelectedValidationField<T, IEnumerable<TProp>?>.From(field);
            var guard = selectedField.GuardAgainstCollectionItem(
                CompareOperator.ArrayContains,
                value,
                candidate => FieldConditionPredicates.EnumerableContains(candidate, value));

            ApplyClientCondition(guard, defineRules);
        }

        // ── Composition ────────────────────────────────────────────────────────

        /// <summary>
        /// Applies a composed condition (And/Or/Not) to all rules defined in the block.
        /// Server: combined predicate runs at validation time.
        /// Client: adapter extracts the FieldCondition tree.
        /// </summary>
        protected void WhenFields(
            Func<FieldConditionBuilder<T>, FieldGuard<T>> buildCondition,
            Action defineRules)
        {
            var builder = new FieldConditionBuilder<T>();
            var guard = buildCondition(builder);

            ApplyClientCondition(guard, defineRules);
        }

        public new IConditionBuilder When(Func<T, bool> predicate, Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ApplyServerOnlyCondition(action, guardedAction => base.When(predicate, guardedAction));
        }

        public new IConditionBuilder When(Func<T, ValidationContext<T>, bool> predicate, Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ApplyServerOnlyCondition(action, guardedAction => base.When(predicate, guardedAction));
        }

        public new IConditionBuilder Unless(Func<T, bool> predicate, Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ApplyServerOnlyCondition(action, guardedAction => base.Unless(predicate, guardedAction));
        }

        public new IConditionBuilder Unless(Func<T, ValidationContext<T>, bool> predicate, Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ApplyServerOnlyCondition(action, guardedAction => base.Unless(predicate, guardedAction));
        }

        public new IConditionBuilder WhenAsync(
            Func<T, CancellationToken, Task<bool>> predicate,
            Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ApplyServerOnlyCondition(action, guardedAction => base.WhenAsync(predicate, guardedAction));
        }

        public new IConditionBuilder WhenAsync(
            Func<T, ValidationContext<T>, CancellationToken, Task<bool>> predicate,
            Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ApplyServerOnlyCondition(action, guardedAction => base.WhenAsync(predicate, guardedAction));
        }

        public new IConditionBuilder UnlessAsync(
            Func<T, CancellationToken, Task<bool>> predicate,
            Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ApplyServerOnlyCondition(action, guardedAction => base.UnlessAsync(predicate, guardedAction));
        }

        public new IConditionBuilder UnlessAsync(
            Func<T, ValidationContext<T>, CancellationToken, Task<bool>> predicate,
            Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ApplyServerOnlyCondition(action, guardedAction => base.UnlessAsync(predicate, guardedAction));
        }

        // ── Internals ──────────────────────────────────────────────────────────

        private IConditionBuilder ApplyServerOnlyCondition(
            Action defineRules,
            Func<Action, IConditionBuilder> applyCondition)
        {
            if (defineRules == null) throw new ArgumentNullException(nameof(defineRules));
            if (applyCondition == null) throw new ArgumentNullException(nameof(applyCondition));

            IConditionBuilder conditionBuilder;
            using (_clientConditionScope.EnterServerOnlyCondition())
            {
                conditionBuilder = applyCondition(defineRules);
            }

            return new ServerOnlyConditionBuilder(conditionBuilder, _clientConditionScope);
        }

        private void ApplyClientCondition(
            Func<T, bool> serverPredicate,
            FieldCondition clientCondition,
            Action defineRules)
        {
            if (serverPredicate == null) throw new ArgumentNullException(nameof(serverPredicate));
            if (clientCondition == null) throw new ArgumentNullException(nameof(clientCondition));
            if (defineRules == null) throw new ArgumentNullException(nameof(defineRules));

            using (_clientConditionScope.Enter(clientCondition))
            {
                // FV's When() defines the rules and keeps the predicate in server validation.
                base.When(serverPredicate, defineRules);
            }
        }

        private void ApplyClientCondition(FieldGuard<T> guard, Action defineRules)
        {
            if (guard == null) throw new ArgumentNullException(nameof(guard));
            ApplyClientCondition(guard.ServerPredicate, guard.Condition, defineRules);
        }

        private sealed class ClientConditionScope
        {
            private readonly Stack<FieldCondition> _activeConditions = new Stack<FieldCondition>();
            private int _serverOnlyConditionDepth;

            internal IDisposable Enter(FieldCondition condition)
            {
                if (condition == null) throw new ArgumentNullException(nameof(condition));
                _activeConditions.Push(condition);
                return new ScopeExit(this);
            }

            internal IDisposable EnterServerOnlyCondition()
            {
                _serverOnlyConditionDepth++;
                return new ServerOnlyScopeExit(this);
            }

            internal void Register(
                IValidationRule rule,
                Dictionary<IValidationRule, ClientConditionProjection> clientConditions)
            {
                if (rule == null) throw new ArgumentNullException(nameof(rule));
                if (clientConditions == null) throw new ArgumentNullException(nameof(clientConditions));

                var noClientConditionIsActive = _activeConditions.Count == 0;
                if (noClientConditionIsActive) return;

                clientConditions[rule] = ActiveProjection();
            }

            private ClientConditionProjection ActiveProjection()
            {
                var serverOnlyConditionIsActive = _serverOnlyConditionDepth > 0;
                if (serverOnlyConditionIsActive)
                    return ClientConditionProjection.Skip(
                        ClientRuleExtractionSkipReason.FluentValidationConditionWithoutClientGuard);

                return ClientConditionProjection.Project(ActiveProjectionGuard());
            }

            private FieldCondition ActiveProjectionGuard()
            {
                var singleActiveCondition = _activeConditions.Count == 1;
                if (singleActiveCondition) return _activeConditions.Peek();

                var conditionsInServerNestingOrder = _activeConditions.Reverse().ToArray();
                return FieldCondition.All(conditionsInServerNestingOrder);
            }

            private void Exit()
            {
                var noClientConditionIsActive = _activeConditions.Count == 0;
                if (noClientConditionIsActive)
                    throw new InvalidOperationException("Cannot exit a client validation condition scope that was not entered.");

                _activeConditions.Pop();
            }

            private void ExitServerOnlyCondition()
            {
                var noServerOnlyConditionIsActive = _serverOnlyConditionDepth == 0;
                if (noServerOnlyConditionIsActive)
                    throw new InvalidOperationException("Cannot exit a server-only validation condition scope that was not entered.");

                _serverOnlyConditionDepth--;
            }

            private sealed class ScopeExit : IDisposable
            {
                private ClientConditionScope? _scope;

                internal ScopeExit(ClientConditionScope scope)
                {
                    _scope = scope ?? throw new ArgumentNullException(nameof(scope));
                }

                public void Dispose()
                {
                    var scope = _scope;
                    if (scope == null) return;

                    _scope = null;
                    scope.Exit();
                }
            }

            private sealed class ServerOnlyScopeExit : IDisposable
            {
                private ClientConditionScope? _scope;

                internal ServerOnlyScopeExit(ClientConditionScope scope)
                {
                    _scope = scope ?? throw new ArgumentNullException(nameof(scope));
                }

                public void Dispose()
                {
                    var scope = _scope;
                    if (scope == null) return;

                    _scope = null;
                    scope.ExitServerOnlyCondition();
                }
            }
        }

        private sealed class ServerOnlyConditionBuilder : IConditionBuilder
        {
            private readonly IConditionBuilder _inner;
            private readonly ClientConditionScope _clientConditionScope;

            internal ServerOnlyConditionBuilder(
                IConditionBuilder inner,
                ClientConditionScope clientConditionScope)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _clientConditionScope = clientConditionScope ?? throw new ArgumentNullException(nameof(clientConditionScope));
            }

            public void Otherwise(Action action)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                _inner.Otherwise(() =>
                {
                    using (_clientConditionScope.EnterServerOnlyCondition())
                    {
                        action();
                    }
                });
            }
        }
    }
}
