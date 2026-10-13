using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// The base of <see cref="ReactiveValidator{T}"/>: it records the conditions each rule is declared
    /// under. FluentValidation's own condition methods live here, so a condition the browser cannot
    /// apply (a method group, a predicate over the validation context, an asynchronous predicate) keeps
    /// the rules it encloses on the server, nested or not.
    /// </summary>
    /// <remarks>
    /// A lambda over the model binds to <see cref="ReactiveValidator{T}.When"/> instead, because C#
    /// prefers the most derived applicable method; only what that overload cannot take reaches these.
    /// </remarks>
    /// <typeparam name="T">The validated model.</typeparam>
    public abstract class RecordingValidator<T> : AbstractValidator<T>
        where T : class
    {
        private readonly Stack<ClientCondition?> _conditions = new Stack<ClientCondition?>();
        private readonly Dictionary<IValidationRule, ClientScope> _scopes = new Dictionary<IValidationRule, ClientScope>();

        private protected RecordingValidator()
        {
        }

        /// <summary>A FluentValidation condition the browser cannot apply: its rules stay on the server.</summary>
        public new IConditionBuilder When(Func<T, bool> predicate, Action action) =>
            RecordedWhen(null, predicate, action);

        /// <summary>A FluentValidation condition the browser cannot apply: its rules stay on the server.</summary>
        public new IConditionBuilder When(Func<T, ValidationContext<T>, bool> predicate, Action action) =>
            Conditioned(null, action, scoped => base.When(predicate, scoped));

        /// <summary>A FluentValidation condition the browser cannot apply: its rules stay on the server.</summary>
        public new IConditionBuilder Unless(Func<T, bool> predicate, Action action) =>
            RecordedUnless(null, predicate, action);

        /// <summary>A FluentValidation condition the browser cannot apply: its rules stay on the server.</summary>
        public new IConditionBuilder Unless(Func<T, ValidationContext<T>, bool> predicate, Action action) =>
            Conditioned(null, action, scoped => base.Unless(predicate, scoped));

        /// <summary>An asynchronous condition: its rules stay on the server.</summary>
        public new IConditionBuilder WhenAsync(Func<T, CancellationToken, Task<bool>> predicate, Action action) =>
            Conditioned(null, action, scoped => base.WhenAsync(predicate, scoped));

        /// <summary>An asynchronous condition: its rules stay on the server.</summary>
        public new IConditionBuilder WhenAsync(Func<T, ValidationContext<T>, CancellationToken, Task<bool>> predicate, Action action) =>
            Conditioned(null, action, scoped => base.WhenAsync(predicate, scoped));

        /// <summary>An asynchronous condition: its rules stay on the server.</summary>
        public new IConditionBuilder UnlessAsync(Func<T, CancellationToken, Task<bool>> predicate, Action action) =>
            Conditioned(null, action, scoped => base.UnlessAsync(predicate, scoped));

        /// <summary>An asynchronous condition: its rules stay on the server.</summary>
        public new IConditionBuilder UnlessAsync(Func<T, ValidationContext<T>, CancellationToken, Task<bool>> predicate, Action action) =>
            Conditioned(null, action, scoped => base.UnlessAsync(predicate, scoped));

        /// <inheritdoc />
        protected override void OnRuleAdded(IValidationRule<T> rule)
        {
            _scopes[rule] = ClientScope.Of(_conditions);
            base.OnRuleAdded(rule);
        }

        private protected ClientScope ScopeOf(IValidationRule rule) =>
            _scopes.TryGetValue(rule, out var scope) ? scope : ClientScope.Unconditional;

        /// <summary>FluentValidation's When, with the rules it encloses recorded under <paramref name="condition"/>.</summary>
        private protected IConditionBuilder RecordedWhen(ClientCondition? condition, Func<T, bool> predicate, Action action) =>
            Conditioned(condition, action, scoped => base.When(predicate, scoped));

        /// <summary>FluentValidation's Unless, with the rules it encloses recorded under <paramref name="condition"/>.</summary>
        private protected IConditionBuilder RecordedUnless(ClientCondition? condition, Func<T, bool> predicate, Action action) =>
            Conditioned(condition, action, scoped => base.Unless(predicate, scoped));

        // Declares the rules of a condition under its browser form (null: none), and records its
        // Otherwise as the negation.
        private IConditionBuilder Conditioned(ClientCondition? condition, Action action, Func<Action, IConditionBuilder> apply)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var otherwise = apply(() => Within(condition, action));
            return new RecordedOtherwise(otherwise, condition == null ? null : ClientCondition.Not(condition), Within);
        }

        private void Within(ClientCondition? condition, Action action)
        {
            _conditions.Push(condition);
            try
            {
                action();
            }
            finally
            {
                _conditions.Pop();
            }
        }

        private sealed class RecordedOtherwise : IConditionBuilder
        {
            private readonly IConditionBuilder _inner;
            private readonly ClientCondition? _negation;
            private readonly Action<ClientCondition?, Action> _within;

            internal RecordedOtherwise(IConditionBuilder inner, ClientCondition? negation, Action<ClientCondition?, Action> within)
            {
                _inner = inner;
                _negation = negation;
                _within = within;
            }

            public void Otherwise(Action action)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));
                _inner.Otherwise(() => _within(_negation, action));
            }
        }
    }
}
