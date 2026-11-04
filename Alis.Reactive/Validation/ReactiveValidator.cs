using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Members;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>Validates a model: the service a controller asks for to validate what a page sent.</summary>
    /// <typeparam name="T">The model it validates.</typeparam>
    public interface IValidator<in T>
    {
        /// <summary>Checks every rule against the model.</summary>
        /// <param name="instance">The model.</param>
        /// <exception cref="InvalidOperationException">A rule waits (<c>MustAsync</c>): use <see cref="ValidateAsync(T, CancellationToken)"/>.</exception>
        ValidationResult Validate(T instance);

        /// <summary>Checks the rules of one set against the model; a set the validator does not declare is refused, naming those it declares.</summary>
        /// <param name="instance">The model.</param>
        /// <param name="ruleSet">The set, as <c>RuleSet</c> named it.</param>
        ValidationResult Validate(T instance, string ruleSet);

        /// <summary>Checks every rule against the model, waiting for the rules that ask a database or a service.</summary>
        /// <param name="instance">The model.</param>
        /// <param name="cancellation">Stops the rules that wait.</param>
        Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default);

        /// <summary>Checks the rules of one set against the model, waiting where a rule waits; a set the validator does not declare is refused.</summary>
        /// <param name="instance">The model.</param>
        /// <param name="ruleSet">The set, as <c>RuleSet</c> named it.</param>
        /// <param name="cancellation">Cancels the wait.</param>
        Task<ValidationResult> ValidateAsync(T instance, string ruleSet, CancellationToken cancellation = default);
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

        internal abstract bool IsAsync(RuleSetSelection selection);

        internal abstract IReadOnlyList<BrowserField> BrowserRules(RuleSetSelection selection);

        internal abstract IEnumerable<string> DeclaredRuleSets { get; }

        internal RuleSetSelection Select(string ruleSet)
        {
            if (string.IsNullOrWhiteSpace(ruleSet)) throw new ArgumentException("A rule set has a name people read.", nameof(ruleSet));
            var declared = DeclaredRuleSets.ToList();
            if (declared.Contains(ruleSet, StringComparer.Ordinal)) return RuleSetSelection.Of(ruleSet);
            var validator = CSharpTypeName.Of(GetType());
            throw new InvalidOperationException(declared.Count == 0
                ? $"{validator} declares no rule set, and '{ruleSet}' was asked for. Write the rules it holds under RuleSet(\"{ruleSet}\", () => ..)."
                : $"{validator} declares no rule set '{ruleSet}'. It declares: {string.Join(", ", declared)}.");
        }
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
    /// member, <c>InclusiveBetween</c>, <c>ExclusiveBetween</c>, <c>IsInEnum</c> (a <c>[Flags]</c> enum stays on the
    /// server) and <c>PrecisionScale</c> (the server alone past 21 digits before the point or 6 after); <c>SetValidator</c>, <c>ChildRules</c>,
    /// <c>RuleForEach</c> with a child validator and <c>Include</c> bring the child's rules along. <c>WithName</c>
    /// gives the name a rule's messages say for the member, on both sides; errors stay under the member's path.
    /// The browser shows one message per input, the first failing check's; the server answers with every
    /// failing check of a rule, or with the first alone under <c>Cascade(CascadeMode.Stop)</c> or the validator's
    /// <c>RuleLevelCascadeMode</c>. <c>DependentRules</c> after a rule's checks declares rules that apply only while
    /// that rule passes; the browser skips them while the rule fails when every check of it has a browser form,
    /// otherwise they are the server's. <c>RuleSet("Issue", () => ..)</c> groups rules a form or a <c>Validate</c>
    /// call names to run alone; the rules in no set run when none is named.
    /// </para>
    /// <para>
    /// Some rules only the server can check, and they stay there by what they are: <c>Must</c>, <c>MustAsync</c>,
    /// <c>Custom</c> and <c>CustomAsync</c> (C# code, a database lookup), a message built from the model, a pattern with .NET-only
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
    public abstract class ReactiveValidator<T> : ReactiveValidator, IValidator<T>, IRuleHost<T>
    {
        private readonly List<IRule<T>> _rules = new List<IRule<T>>();
        private readonly List<string> _declaredSets = new List<string>();
        private Conditions<T> _scope = Conditions<T>.None;
        private RuleSets _sets = RuleSets.None;

        /// <summary>Starts a validator with no rules; the derived class's constructor declares them.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0022:Constructor make protected",
            Justification = "An app derives from the typed validator; the untyped base stays closed and exposes no protected member.")]
        protected ReactiveValidator() { }

        /// <summary>
        /// What the server does after a check of a rule fails, for every rule of this validator that sets no
        /// <c>Cascade</c> of its own: <see cref="CascadeMode.Continue"/>, FluentValidation's default, answers with
        /// every failing check; <see cref="CascadeMode.Stop"/> answers with the first alone, as the browser shows
        /// one message per input. Read when the rules run, so it may be set anywhere in the constructor.
        /// </summary>
        protected CascadeMode RuleLevelCascadeMode { get; set; } = CascadeMode.Continue;

        /// <summary>Starts the rules of one member: <c>RuleFor(x =&gt; x.Address.City).NotEmpty()</c>.</summary>
        /// <param name="member">The member, as a path from the model.</param>
        /// <typeparam name="TProperty">The member's type.</typeparam>
        public IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return Declared(new MemberRule<T, TProperty>(member, _scope, _sets, this));
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
            return Declared(new CollectionRule<T, TElement>(collection, _scope, _sets, this));
        }

        /// <summary>Runs another validator's rules for the same model, as if written here.</summary>
        /// <param name="rules">The validator.</param>
        public void Include(IValidator<T> rules) => _rules.Add(new IncludeRule<T>(rules, _scope, _sets));

        /// <summary>
        /// Declares the rules inside <paramref name="rules"/> as members of the named sets: <c>RuleSet("Issue", ..)</c>,
        /// or <c>RuleSet("Issue, Renew", ..)</c> for a rule in both. <c>Validate(instance, "Issue")</c> and a request's
        /// <c>Validate&lt;TRules&gt;(formId, "Issue")</c> run the rules in that set alone; <c>Validate(instance)</c> and
        /// <c>Validate&lt;TRules&gt;(formId)</c> run the rules in no set. The set reaches a child validator through a rule
        /// of this set that holds it (<c>SetValidator</c>, <c>ChildRules</c>), and an <c>Include</c> written in no set,
        /// and then runs the child's rules of the same name. Names match as written. A <c>RuleSet</c> inside a
        /// <c>RuleSet</c> is refused.
        /// </summary>
        /// <param name="ruleSet">The set's name, or several separated by commas.</param>
        /// <param name="rules">Declares the rules.</param>
        public void RuleSet(string ruleSet, Action rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            var sets = RuleSets.Named(ruleSet);
            if (_sets.Any)
                throw new InvalidOperationException(
                    $"RuleSet '{sets}' is written inside RuleSet '{_sets}'; a set holds rules, not sets. " +
                    $"A rule in both is written once, under RuleSet(\"{_sets}, {sets}\", () => ..).");
            foreach (var name in sets.Names)
                if (!_declaredSets.Contains(name, StringComparer.Ordinal)) _declaredSets.Add(name);
            _sets = sets;
            try
            {
                rules();
            }
            finally
            {
                _sets = RuleSets.None;
            }
        }

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

        /// <summary>
        /// Applies the rules only when <paramref name="predicate"/> holds, asked while the rules run and waited
        /// for: a bed book, a registry, a service. A condition that waits is the server's alone, and a
        /// validator holding one is validated with <see cref="ValidateAsync(T, CancellationToken)"/>, as one
        /// with <c>MustAsync</c> is.
        /// </summary>
        /// <param name="predicate">The condition over the model, answered later.</param>
        /// <param name="rules">Declares the rules.</param>
        /// <returns>Its <c>Otherwise</c> declares the rules for when the condition does not hold.</returns>
        public IConditionBuilder WhenAsync(Func<T, CancellationToken, Task<bool>> predicate, Action rules) =>
            Branch(RuleCondition<T>.OnServerAsync(predicate), rules);

        /// <summary>
        /// Applies the rules only when <paramref name="predicate"/> does not hold, asked while the rules run
        /// and waited for; the server's alone, as <see cref="WhenAsync"/>.
        /// </summary>
        /// <param name="predicate">The condition over the model, answered later.</param>
        /// <param name="rules">Declares the rules.</param>
        /// <returns>Its <c>Otherwise</c> declares the rules for when the condition holds.</returns>
        public IConditionBuilder UnlessAsync(Func<T, CancellationToken, Task<bool>> predicate, Action rules) =>
            Branch(RuleCondition<T>.OnServerAsync(predicate).Negated(), rules);

        /// <inheritdoc />
        public ValidationResult Validate(T instance) => Validate(instance, RuleSetSelection.Default);

        /// <inheritdoc />
        public ValidationResult Validate(T instance, string ruleSet) => Validate(instance, Select(ruleSet));

        /// <inheritdoc />
        public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default) =>
            ValidateAsync(instance, RuleSetSelection.Default, cancellation);

        /// <inheritdoc />
        public Task<ValidationResult> ValidateAsync(T instance, string ruleSet, CancellationToken cancellation = default) =>
            ValidateAsync(instance, Select(ruleSet), cancellation);

        private ValidationResult Validate(T instance, RuleSetSelection selection)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (IsAsync(selection))
                throw new InvalidOperationException($"{CSharpTypeName.Of(GetType())} has a rule that waits (MustAsync, CustomAsync or WhenAsync); validate with ValidateAsync.");

            var run = new ValidationRun(synchronous: true, CancellationToken.None, selection);
            RunRules(instance, "", run).GetAwaiter().GetResult();
            return new ValidationResult(run.Failures.ToArray());
        }

        private async Task<ValidationResult> ValidateAsync(T instance, RuleSetSelection selection, CancellationToken cancellation)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var run = new ValidationRun(synchronous: false, cancellation, selection);
            await RunRules(instance, "", run).ConfigureAwait(false);
            return new ValidationResult(run.Failures.ToArray());
        }

        internal override bool IsAsync(RuleSetSelection selection) => _rules.Any(rule => rule.RunsUnder(selection) && rule.IsAsync(selection));

        internal override IEnumerable<string> DeclaredRuleSets =>
            _declaredSets.Concat(_rules.SelectMany(rule => rule.ReachableRuleSets)).Distinct(StringComparer.Ordinal);

        internal override IReadOnlyList<BrowserField> BrowserRules(RuleSetSelection selection)
        {
            var browser = new BrowserRuleSet();
            foreach (var rule in _rules)
                if (rule.RunsUnder(selection)) rule.DescribeFor(browser, selection);
            return browser.Fields;
        }

        internal async Task RunRules(T instance, string prefix, ValidationRun run)
        {
            foreach (var rule in _rules)
                if (rule.RunsUnder(run.Selection)) await rule.Validate(instance, prefix, run).ConfigureAwait(false);
        }

        private TRule Declared<TRule>(TRule rule) where TRule : IRule<T>
        {
            _rules.Add(rule);
            return rule;
        }

        CascadeMode IRuleHost<T>.RuleLevelCascadeMode => RuleLevelCascadeMode;

        void IRuleHost<T>.DeclareUnder(RuleCondition<T> condition, Action rules) => DeclareUnder(_scope.And(condition), rules);

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
