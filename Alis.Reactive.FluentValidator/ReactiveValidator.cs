using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>Validates a model: the service a controller asks for to validate what a page sent.</summary>
    /// <typeparam name="T">The model it validates.</typeparam>
    public interface IValidator<in T>
    {
        /// <summary>Checks every rule against the model.</summary>
        /// <param name="instance">The model.</param>
        /// <exception cref="InvalidOperationException">A rule waits (<c>MustAsync</c>): use <see cref="ValidateAsync"/>.</exception>
        ValidationResult Validate(T instance);

        /// <summary>Checks every rule against the model, waiting for the rules that ask a database or a service.</summary>
        /// <param name="instance">The model.</param>
        /// <param name="cancellation">Stops the rules that wait.</param>
        Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default);
    }

    /// <summary>
    /// What every validator of the framework is, whatever model it validates: what
    /// <c>AddReactiveFluentValidation</c> registers and a page's plan reads its browser rules from.
    /// Derive from <see cref="ReactiveValidator{T}"/>.
    /// </summary>
    public abstract class ReactiveValidator
    {
        private protected ReactiveValidator()
        {
        }

        /// <summary>Whether a rule waits for a database or a service.</summary>
        internal abstract bool IsAsync { get; }

        /// <summary>The rules the browser runs, with those of the child validators the rules hold.</summary>
        internal abstract IReadOnlyList<ClientValidationField> ClientRules();
    }

    /// <summary>
    /// A model's rules, written once in FluentValidation's grammar. The server checks every rule; the page's
    /// plan carries every rule the browser can check, so a field says what is wrong as the user leaves it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The browser runs <c>NotNull</c>, <c>NotEmpty</c>, <c>Empty</c>, <c>MinimumLength</c>,
    /// <c>MaximumLength</c>, <c>Length</c>, <c>EmailAddress</c>, <c>Url</c>, <c>CreditCard</c>,
    /// <c>Matches</c> (a pattern JavaScript reads the same way), the comparisons with a value or another
    /// member, <c>InclusiveBetween</c> and <c>ExclusiveBetween</c>; <c>SetValidator</c>, <c>ChildRules</c>,
    /// <c>RuleForEach</c> with a child validator and <c>Include</c> bring the child's rules along.
    /// </para>
    /// <para>
    /// Some rules only the server can check, and they stay there by what they are: <c>Must</c> and
    /// <c>MustAsync</c> (C# code, a database lookup), a message built from the model, a pattern with .NET-only
    /// syntax, and a rule under a condition declared the server's (<c>WhenOnServer</c>, <c>UnlessOnServer</c>).
    /// <c>.ServerOnly()</c> keeps any other rule on the server, for a member the page has no input for. A
    /// condition the browser cannot read that is not declared the server's is refused when a page describes
    /// the rules, so no rule changes sides in silence.
    /// </para>
    /// <para>
    /// A condition, <c>When</c> or <c>Unless</c> around a block of rules or after a rule's checks, runs in the
    /// browser when it compares model members with fixed values: <c>==</c>, <c>!=</c>, <c>&lt;</c> and the
    /// rest, <c>string.IsNullOrEmpty</c>, <c>Contains</c>, <c>StartsWith</c>, <c>EndsWith</c>,
    /// <c>Regex.IsMatch</c>, membership in a fixed set, joined by <c>&amp;&amp;</c>, <c>||</c> and <c>!</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public sealed class AdmissionValidator : ReactiveValidator&lt;Admission&gt;
    /// {
    ///     public AdmissionValidator(IResidentDirectory residents)
    ///     {
    ///         RuleFor(x =&gt; x.ResidentName).NotEmpty().MaximumLength(100);
    ///         RuleFor(x =&gt; x.DischargeDate).GreaterThan(x =&gt; x.AdmissionDate);
    ///         When(x =&gt; x.IsVeteran, () =&gt; RuleFor(x =&gt; x.ServiceNumber).NotEmpty());
    ///         RuleForEach(x =&gt; x.Contacts).SetValidator(new ContactValidator());
    ///         RuleFor(x =&gt; x.RoomCode).MustAsync(residents.IsRoomFreeAsync).WithMessage("That room is taken.");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T">The model it validates.</typeparam>
    public abstract class ReactiveValidator<T> : ReactiveValidator, IValidator<T>
    {
        private readonly List<IRule<T>> _rules = new List<IRule<T>>();
        private Conditions<T> _scope = Conditions<T>.None;

        /// <summary>Starts the rules of one member: <c>RuleFor(x =&gt; x.Address.City).NotEmpty()</c>.</summary>
        /// <param name="member">The member, as a path from the model.</param>
        /// <typeparam name="TProperty">The member's type.</typeparam>
        public IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return Declared(new MemberRule<T, TProperty>(member, _scope));
        }

        /// <summary>
        /// Starts the rules of every item of a list, named <c>Contacts[0]</c>, <c>Contacts[1]</c>:
        /// <c>RuleForEach(x =&gt; x.Contacts).SetValidator(new ContactValidator())</c>.
        /// </summary>
        /// <param name="collection">The list member.</param>
        /// <typeparam name="TElement">The item's type.</typeparam>
        public IRuleBuilder<T, TElement> RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            return Declared(new CollectionRule<T, TElement>(collection, _scope));
        }

        /// <summary>Runs another validator's rules for the same model, as if written here.</summary>
        /// <param name="rules">The validator.</param>
        public void Include(IValidator<T> rules) => _rules.Add(new IncludeRule<T>(rules, _scope));

        /// <summary>Applies the rules <paramref name="rules"/> declares only when <paramref name="predicate"/> holds.</summary>
        /// <param name="predicate">The condition over the model.</param>
        /// <param name="rules">Declares the rules.</param>
        /// <returns>Its <c>Otherwise</c> declares the rules for when the condition does not hold.</returns>
        public IConditionBuilder When(Expression<Func<T, bool>> predicate, Action rules) =>
            Branch(RuleCondition<T>.When(predicate), rules);

        /// <summary>Applies the rules <paramref name="rules"/> declares only when <paramref name="predicate"/> does not hold.</summary>
        /// <param name="predicate">The condition over the model.</param>
        /// <param name="rules">Declares the rules.</param>
        /// <returns>Its <c>Otherwise</c> declares the rules for when the condition holds.</returns>
        public IConditionBuilder Unless(Expression<Func<T, bool>> predicate, Action rules) =>
            Branch(RuleCondition<T>.Unless(predicate), rules);

        /// <summary>
        /// Applies the rules only when <paramref name="predicate"/> holds, decided by the server alone: for a
        /// test the browser cannot make (a method, a service, a query over a collection). The rules under it
        /// stay with the server.
        /// </summary>
        /// <param name="predicate">The condition over the model.</param>
        /// <param name="rules">Declares the rules.</param>
        /// <returns>Its <c>Otherwise</c> declares the rules for when the condition does not hold.</returns>
        public IConditionBuilder WhenOnServer(Expression<Func<T, bool>> predicate, Action rules) =>
            Branch(RuleCondition<T>.OnServer(predicate), rules);

        /// <summary>Applies the rules only when <paramref name="predicate"/> does not hold, decided by the server alone.</summary>
        /// <param name="predicate">The condition over the model.</param>
        /// <param name="rules">Declares the rules.</param>
        /// <returns>Its <c>Otherwise</c> declares the rules for when the condition holds.</returns>
        public IConditionBuilder UnlessOnServer(Expression<Func<T, bool>> predicate, Action rules) =>
            Branch(RuleCondition<T>.OnServer(predicate).Negated(), rules);

        /// <inheritdoc />
        public ValidationResult Validate(T instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (IsAsync)
                throw new InvalidOperationException($"{GetType().Name} has a rule that waits (MustAsync); validate with ValidateAsync.");

            var run = new ValidationRun(synchronous: true, CancellationToken.None);
            RunRules(instance, "", run).GetAwaiter().GetResult();
            return new ValidationResult(run.Failures.ToArray());
        }

        /// <inheritdoc />
        public async Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var run = new ValidationRun(synchronous: false, cancellation);
            await RunRules(instance, "", run).ConfigureAwait(false);
            return new ValidationResult(run.Failures.ToArray());
        }

        internal override bool IsAsync => _rules.Any(rule => rule.IsAsync);

        internal override IReadOnlyList<ClientValidationField> ClientRules()
        {
            var browser = new ClientRuleSet();
            foreach (var rule in _rules)
                rule.DescribeFor(browser);
            return browser.Fields;
        }

        // A synchronous run completes without waiting: no check it holds waits.
        internal async Task RunRules(T instance, string prefix, ValidationRun run)
        {
            foreach (var rule in _rules)
                await rule.Validate(instance, prefix, run).ConfigureAwait(false);
        }

        private TRule Declared<TRule>(TRule rule) where TRule : IRule<T>
        {
            _rules.Add(rule);
            return rule;
        }

        private IConditionBuilder Branch(RuleCondition<T> condition, Action rules)
        {
            var outer = _scope;
            DeclareUnder(outer.And(condition), rules);
            return new Otherwise(this, outer.And(condition.Negated()));
        }

        private void DeclareUnder(Conditions<T> scope, Action rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            var outer = _scope;
            _scope = scope;
            try
            {
                rules();
            }
            finally
            {
                _scope = outer;
            }
        }

        private sealed class Otherwise : IConditionBuilder
        {
            private readonly ReactiveValidator<T> _validator;
            private readonly Conditions<T> _scope;

            internal Otherwise(ReactiveValidator<T> validator, Conditions<T> scope)
            {
                _validator = validator;
                _scope = scope;
            }

            void IConditionBuilder.Otherwise(Action rules) => _validator.DeclareUnder(_scope, rules);
        }
    }

    /// <summary>
    /// A validator whose rules are written where it is made: the child of <c>ChildRules</c>, or a one-off
    /// validator in a test.
    /// </summary>
    /// <example><c>var rules = new InlineValidator&lt;Contact&gt;(); rules.RuleFor(x =&gt; x.Phone).NotEmpty();</c></example>
    /// <typeparam name="T">The model it validates.</typeparam>
    public sealed class InlineValidator<T> : ReactiveValidator<T>
    {
    }
}
