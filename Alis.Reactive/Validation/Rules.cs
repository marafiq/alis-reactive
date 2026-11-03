using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Inputs;
using Alis.Reactive.PlanModel.Validation;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A rule's <c>When</c> or <c>Unless</c>: the server's test, and the browser's form of it when it has one. A
    /// condition the browser cannot read is either declared the server's (<c>WhenOnServer</c>) or refused
    /// where a page describes the rules, so no rule changes sides in silence.
    /// </summary>
    internal sealed class RuleCondition<T>
    {
        private readonly Func<T, bool> _holds;
        private readonly Func<Condition?> _browser;
        private readonly string? _unreadable;

        // The browser's form is read where a page describes the rules, so that a validator the server alone
        // runs is built whatever its conditions.
        private RuleCondition(Func<T, bool> holds, Func<Condition?> browser, string? unreadable)
        {
            _holds = holds;
            _browser = browser;
            _unreadable = unreadable;
        }

        /// <summary>The rule <paramref name="rule"/> passes: what the rules under <c>DependentRules</c> run under.</summary>
        internal static RuleCondition<T> Passing<TValue>(Rule<T, TValue> rule) =>
            new RuleCondition<T>(rule.PassesOn, rule.BrowserPassing, null);

        internal bool Holds(T instance) => _holds(instance);

        internal static RuleCondition<T> When(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var browser = BrowserCondition.From(predicate);
            return new RuleCondition<T>(predicate.Compile(), () => browser, browser == null ? predicate.Body.ToString() : null);
        }

        internal static RuleCondition<T> Unless(Expression<Func<T, bool>> predicate) => When(predicate).Negated();

        /// <summary>A condition the server alone decides, by the developer's word: <c>WhenOnServer</c>, <c>UnlessOnServer</c>.</summary>
        internal static RuleCondition<T> OnServer(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new RuleCondition<T>(predicate.Compile(), () => null, null);
        }

        /// <summary>Holds always, with no browser form: what <c>.ServerOnly()</c> writes after a rule's checks.</summary>
        internal static RuleCondition<T> ServerOnly { get; } = new RuleCondition<T>(_ => true, () => null, null);

        internal RuleCondition<T> Negated() =>
            new RuleCondition<T>(instance => !_holds(instance), () => _browser() is Condition browser ? Condition.Not(browser) : null, _unreadable);

        /// <summary>
        /// The browser's form, or false when the server alone decides. A condition the browser cannot read that
        /// was not declared the server's is refused here, where a page describes the rules.
        /// </summary>
        internal bool TryForBrowser(out Condition browser)
        {
            if (_unreadable != null)
                throw new InvalidOperationException(
                    $"The condition '{_unreadable}' has no browser form. Write it as tests of model members against fixed " +
                    "values (==, !=, <, <=, >, >=, string.IsNullOrEmpty, Contains, StartsWith, EndsWith, Regex.IsMatch, HasValue, " +
                    "joined by &&, || and !), or declare that the server alone decides it: WhenOnServer or UnlessOnServer.");
            browser = _browser()!;
            return browser != null;
        }
    }

    /// <summary>Every condition a rule is written under, outermost first.</summary>
    internal sealed class Conditions<T>
    {
        private readonly IReadOnlyList<RuleCondition<T>> _all;

        private Conditions(IReadOnlyList<RuleCondition<T>> all)
        {
            _all = all;
        }

        internal static Conditions<T> None { get; } = new Conditions<T>(Array.Empty<RuleCondition<T>>());

        internal Conditions<T> And(RuleCondition<T> condition) => new Conditions<T>(_all.Concat(new[] { condition }).ToArray());

        internal bool Hold(T instance) => _all.All(condition => condition.Holds(instance));

        /// <summary>The browser's form of all of them, or false when one is the server's alone.</summary>
        internal bool TryForBrowser(out Condition condition)
        {
            condition = Condition.Always;
            var forms = new List<Condition>();
            var onServer = false;
            foreach (var each in _all)
            {
                if (each.TryForBrowser(out var form)) forms.Add(form);
                else onServer = true;
            }

            if (onServer) return false;
            if (forms.Count > 0) condition = BrowserCondition.AllOf(forms);
            return true;
        }
    }

    /// <summary>One validation of one model: the failures found, and whether it may wait.</summary>
    internal sealed class ValidationRun
    {
        private readonly List<ValidationFailure> _failures = new List<ValidationFailure>();

        internal ValidationRun(bool synchronous, CancellationToken cancellation)
        {
            Synchronous = synchronous;
            Cancellation = cancellation;
        }

        internal bool Synchronous { get; }

        internal CancellationToken Cancellation { get; }

        internal IReadOnlyList<ValidationFailure> Failures => _failures;

        internal void Fail(string propertyName, string message) => _failures.Add(new ValidationFailure(propertyName, message));
    }

    /// <summary>A rule of a validator: it validates a model, and describes what of it the browser runs.</summary>
    internal interface IRule<T>
    {
        bool IsAsync { get; }

        Task Validate(T instance, string prefix, ValidationRun run);

        void DescribeFor(BrowserRuleSet browser);
    }

    /// <summary>The validator as its rules see it: the default they run under, and where dependent rules are declared.</summary>
    internal interface IRuleHost<T>
    {
        CascadeMode RuleLevelCascadeMode { get; }

        /// <summary>Declares the rules <paramref name="rules"/> writes under <paramref name="condition"/>, inside the current blocks.</summary>
        void DeclareUnder(RuleCondition<T> condition, Action rules);
    }

    /// <summary>The checks and child validators written after one <c>RuleFor</c> or <c>RuleForEach</c>.</summary>
    internal abstract class Rule<T, TValue> : IRule<T>, IRuleBuilderOptions<T, TValue>, IRuleTarget<T, TValue>
    {
        private readonly IRuleHost<T> _host;
        private CascadeMode? _cascade;

        // A rule names a member: its errors are reported under the member's path, and the page binds it.
        protected Rule(ModelMember<T> member, Conditions<T> scope, IRuleHost<T> host)
        {
            _host = host;
            FieldPath = member.Path ?? throw new ArgumentException(
                $"A rule names a member of the model, like x => x.Address.City; {member.Written} computes a value. " +
                "Write the rule on the member it reads, with Must for what C# decides.");
            FieldDisplayName = DisplayName.Of(FieldPath);
            Member = member;
            Scope = scope;
        }

        protected ModelMember<T> Member { get; }

        protected Conditions<T> Scope { get; }

        protected List<RulePart<T, TValue>> Parts { get; } = new List<RulePart<T, TValue>>();

        /// <summary>The member's path, as model binding names it and as errors are reported.</summary>
        protected string FieldPath { get; }

        /// <summary>The member as a message names it: <c>AdmissionDate</c> reads "Admission Date", or the name <c>WithName</c> gives.</summary>
        protected string FieldDisplayName { get; private set; }

        public bool IsAsync => Parts.Any(part => part.IsAsync);

        /// <summary>Whether the server answers with the rule's first failing check alone: the rule's own mode, else the validator's.</summary>
        protected bool StopsAtFirstFailure => (_cascade ?? _host.RuleLevelCascadeMode) == CascadeMode.Stop;

        public abstract Task Validate(T instance, string prefix, ValidationRun run);

        public abstract void DescribeFor(BrowserRuleSet browser);

        // The browser's condition for one check: the rule's enclosing blocks and the check's own conditions,
        // or false when one of them is the server's alone.
        protected bool TryBrowserCondition(RulePart<T, TValue> part, out Condition condition)
        {
            condition = Condition.Always;
            if (!Scope.TryForBrowser(out var scope) || !part.Conditions.TryForBrowser(out var own)) return false;
            condition = ReferenceEquals(scope, Condition.Always) ? own
                : ReferenceEquals(own, Condition.Always) ? scope
                : Condition.All(scope, own);
            return true;
        }

        public IRuleBuilderOptions<T, TValue> Add(Check<T, TValue> check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            Parts.Add(new CheckPart<T, TValue>(check));
            return this;
        }

        public IRuleBuilderOptions<T, TValue> SetValidator(IValidator<TValue> validator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            Parts.Add(new ChildPart<T, TValue>(validator));
            return this;
        }

        public IRuleBuilder<T, TValue> Cascade(CascadeMode mode)
        {
            _cascade = mode;
            return this;
        }

        public IRuleBuilderOptions<T, TValue> DependentRules(Action rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (IsAsync)
                throw new InvalidOperationException(
                    $"DependentRules follows a rule on '{FieldPath}' that waits (MustAsync), and a condition does not wait: " +
                    "write the dependent rules under WhenOnServer with the same test.");
            _host.DeclareUnder(RuleCondition<T>.Passing(this), rules);
            return this;
        }

        /// <summary>The server's test of <c>DependentRules</c>: the rule, run on its own, finds no failure.</summary>
        internal bool PassesOn(T instance)
        {
            var run = new ValidationRun(synchronous: true, CancellationToken.None);
            Validate(instance, "", run).GetAwaiter().GetResult();
            return run.Failures.Count == 0;
        }

        /// <summary>
        /// The browser's form of the same test: every check of the rule has one, under conditions the browser
        /// reads; or null, and the dependent rules are the server's. A rule over a list has none.
        /// </summary>
        internal virtual Condition? BrowserPassing() => null;

        public IRuleBuilderOptions<T, TValue> WithMessage(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            LastCheck(nameof(WithMessage)).Message = Message<T>.Template(message);
            return this;
        }

        public IRuleBuilderOptions<T, TValue> WithMessage(Func<T, string> message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            LastCheck(nameof(WithMessage)).Message = Message<T>.FromModel(message);
            return this;
        }

        public IRuleBuilderOptions<T, TValue> WithName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("A name people read has text.", nameof(displayName));
            FieldDisplayName = displayName;
            return this;
        }

        public IRuleBuilderOptions<T, TValue> When(Expression<Func<T, bool>> predicate) => Conditioned(RuleCondition<T>.When(predicate));

        public IRuleBuilderOptions<T, TValue> Unless(Expression<Func<T, bool>> predicate) => Conditioned(RuleCondition<T>.Unless(predicate));

        public IRuleBuilderOptions<T, TValue> WhenOnServer(Expression<Func<T, bool>> predicate) => Conditioned(RuleCondition<T>.OnServer(predicate));

        public IRuleBuilderOptions<T, TValue> UnlessOnServer(Expression<Func<T, bool>> predicate) => Conditioned(RuleCondition<T>.OnServer(predicate).Negated());

        public IRuleBuilderOptions<T, TValue> ServerOnly() => Conditioned(RuleCondition<T>.ServerOnly);

        // A condition written after checks applies to every check written before it, as in FluentValidation.
        private IRuleBuilderOptions<T, TValue> Conditioned(RuleCondition<T> condition)
        {
            foreach (var part in Parts)
                part.Conditions = part.Conditions.And(condition);
            return this;
        }

        private CheckPart<T, TValue> LastCheck(string option) =>
            Parts.LastOrDefault() as CheckPart<T, TValue>
            ?? throw new InvalidOperationException($"{option} names the check written just before it; write a check first, like .NotEmpty().{option}(..).");
    }

    /// <summary><c>RuleFor(x =&gt; x.Member)</c>: the checks of one member.</summary>
    internal sealed class MemberRule<T, TValue> : Rule<T, TValue>
    {
        internal MemberRule(Expression<Func<T, TValue>> member, Conditions<T> scope, IRuleHost<T> host)
            : base(ModelMember<T>.Of(member), scope, host)
        {
        }

        internal override Condition? BrowserPassing()
        {
            var rules = new List<BrowserRule>();
            foreach (var part in Parts)
            {
                if (!(part is CheckPart<T, TValue> check) || !TryBrowserCondition(part, out var when) || !check.TryBrowserRules(FieldDisplayName, out var own))
                    return null;
                rules.AddRange(own.Select(rule => rule.When(when)));
            }
            return new RulesPassCondition(FieldReference.Of(Member.Expression), rules);
        }

        public override async Task Validate(T instance, string prefix, ValidationRun run)
        {
            if (!Scope.Hold(instance)) return;
            var value = (TValue)Member.ValueIn(instance)!;
            foreach (var part in Parts)
            {
                var failures = run.Failures.Count;
                await part.Validate(instance, value, prefix + FieldPath, FieldDisplayName, run).ConfigureAwait(false);
                if (StopsAtFirstFailure && run.Failures.Count > failures) return;
            }
        }

        // A check on the member reports under the member's name, so the member is declared whether or not
        // the browser can make the check; a child validator alone reports under its own members.
        public override void DescribeFor(BrowserRuleSet browser)
        {
            foreach (var part in Parts)
            {
                if (TryBrowserCondition(part, out var when)) part.DescribeFor(browser, Member, FieldDisplayName, when);
                else if (part is ChildPart<T, TValue> child) child.DeclareFor(browser, Member);
            }
            if (Parts.Any(part => part is CheckPart<T, TValue>))
                browser.Declare(Member.Expression);
        }
    }

    /// <summary><c>RuleForEach(x =&gt; x.Items)</c>: the checks of every item of a list, named <c>Items[0]</c>, <c>Items[1]</c>.</summary>
    internal sealed class CollectionRule<T, TElement> : Rule<T, TElement>
    {
        internal CollectionRule(Expression<Func<T, IEnumerable<TElement>>> collection, Conditions<T> scope, IRuleHost<T> host)
            : base(ModelMember<T>.Of(collection), scope, host)
        {
        }

        public override async Task Validate(T instance, string prefix, ValidationRun run)
        {
            if (!Scope.Hold(instance)) return;
            if (!(Member.ValueIn(instance) is IEnumerable<TElement> items)) return;

            var index = 0;
            foreach (var item in items)
            {
                var path = prefix + FieldPath + "[" + index++ + "]";
                foreach (var part in Parts)
                {
                    var failures = run.Failures.Count;
                    await part.Validate(instance, item, path, FieldDisplayName, run).ConfigureAwait(false);
                    if (StopsAtFirstFailure && run.Failures.Count > failures) break;
                }
            }
        }

        // The browser validates an item through the item's own validator, or by the checks written on the
        // item itself, at every item on the page when the form checks.
        // Under a condition only the server decides, the items and the child's members are still named.
        public override void DescribeFor(BrowserRuleSet browser)
        {
            foreach (var part in Parts)
            {
                var inBrowser = TryBrowserCondition(part, out var when);
                if (part is ChildPart<T, TElement> child && child.TryItemFields(out var itemFields))
                {
                    if (inBrowser) browser.AddItems(Member.Expression, itemFields, when);
                    else browser.AddItems(Member.Expression, itemFields.Select(field => field.WithoutRules()), Condition.Always);
                }
                if (inBrowser && part is CheckPart<T, TElement> check)
                    check.DescribeForItems(browser, Member, FieldDisplayName, when);
            }
            if (Parts.Any(part => part is CheckPart<T, TElement>))
                browser.DeclareItems(Member.Expression);
        }
    }

    /// <summary><c>Include(validator)</c>: another validator's rules for the same model, run where they are written.</summary>
    internal sealed class IncludeRule<T> : IRule<T>
    {
        private readonly IValidator<T> _included;
        private readonly Conditions<T> _scope;

        internal IncludeRule(IValidator<T> included, Conditions<T> scope)
        {
            _included = included ?? throw new ArgumentNullException(nameof(included));
            _scope = scope;
        }

        public bool IsAsync => _included is ReactiveValidator rules && rules.IsAsync;

        public async Task Validate(T instance, string prefix, ValidationRun run)
        {
            if (!_scope.Hold(instance)) return;
            await Validators.Run(_included, instance, prefix, run).ConfigureAwait(false);
        }

        // Under a condition only the server decides, the included rules' members are still named.
        public void DescribeFor(BrowserRuleSet browser)
        {
            if (!(_included is ReactiveValidator rules)) return;
            if (_scope.TryForBrowser(out var scope)) browser.Add(rules.BrowserRules(), scope);
            else browser.Add(rules.BrowserRules().Select(field => field.WithoutRules()), Condition.Always);
        }
    }

    /// <summary>One check or child validator of a rule, with the conditions written after it.</summary>
    internal abstract class RulePart<T, TValue>
    {
        internal Conditions<T> Conditions { get; set; } = Conditions<T>.None;

        internal abstract bool IsAsync { get; }

        internal abstract Task Validate(T instance, TValue value, string path, string displayName, ValidationRun run);

        internal abstract void DescribeFor(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when);
    }

    internal sealed class CheckPart<T, TValue> : RulePart<T, TValue>
    {
        private readonly Check<T, TValue> _check;

        internal CheckPart(Check<T, TValue> check)
        {
            _check = check;
            Message = Message<T>.Template(check.Template);
        }

        internal Message<T> Message { get; set; }

        internal override bool IsAsync => _check.IsAsync;

        internal override async Task Validate(T instance, TValue value, string path, string displayName, ValidationRun run)
        {
            if (!Conditions.Hold(instance)) return;
            var arguments = new MessageArguments(displayName).With("PropertyValue", value);
            _check.NameRenderArguments(arguments);
            var passes = run.Synchronous
                ? _check.Passes(instance, value, arguments)
                : await _check.PassesAsync(instance, value, arguments, run.Cancellation).ConfigureAwait(false);
            if (!passes) run.Fail(path, Message.For(instance, arguments));
        }

        // A check reaches the browser when the vocabulary has its term and its message is known before any
        // model exists.
        internal override void DescribeFor(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when)
        {
            foreach (var rule in BrowserRules(displayName))
                browser.Add(member.Expression, rule.When(when));
        }

        // The same check on each item of a collection member.
        internal void DescribeForItems(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when)
        {
            var rules = BrowserRules(displayName).ToList();
            if (rules.Count > 0) browser.AddItemRules(member.Expression, rules, when);
        }

        /// <summary>The check's browser rules, when it has any; a check the server alone makes has none.</summary>
        internal bool TryBrowserRules(string displayName, out IReadOnlyList<BrowserRule> rules)
        {
            rules = BrowserRules(displayName).ToList();
            return rules.Count > 0;
        }

        private IEnumerable<BrowserRule> BrowserRules(string displayName)
        {
            if (!(_check is IBrowserCheck term)) return Array.Empty<BrowserRule>();
            var arguments = new MessageArguments(displayName);
            _check.NameRenderArguments(arguments);
            return Message.TryForBrowser(arguments, out var text) ? term.ForBrowser(text) : Array.Empty<BrowserRule>();
        }
    }

    internal sealed class ChildPart<T, TValue> : RulePart<T, TValue>
    {
        private readonly IValidator<TValue> _child;

        internal ChildPart(IValidator<TValue> child)
        {
            _child = child;
        }

        internal override bool IsAsync => _child is ReactiveValidator rules && rules.IsAsync;

        // A missing value has no members to validate; NotNull says it is missing.
        internal override async Task Validate(T instance, TValue value, string path, string displayName, ValidationRun run)
        {
            if (!Conditions.Hold(instance) || value == null) return;
            await Validators.Run(_child, value, path + ".", run).ConfigureAwait(false);
        }

        internal override void DescribeFor(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when)
        {
            if (TryItemFields(out var fields)) browser.Add(member.Expression, fields, when);
        }

        // Under a condition only the server decides, the child's members are still named, with no browser check.
        internal void DeclareFor(BrowserRuleSet browser, ModelMember<T> member)
        {
            if (TryItemFields(out var fields)) browser.Add(member.Expression, fields.Select(field => field.WithoutRules()), Condition.Always);
        }

        // A validator the app wrote another way than ReactiveValidator gives the browser nothing.
        internal bool TryItemFields(out IReadOnlyList<BrowserField> fields)
        {
            fields = _child is ReactiveValidator rules ? rules.BrowserRules() : Array.Empty<BrowserField>();
            return _child is ReactiveValidator;
        }
    }

    internal static class Validators
    {
        // A validator of the framework runs its rules into this run; another reports its failures, named under prefix.
        internal static async Task Run<TModel>(IValidator<TModel> validator, TModel instance, string prefix, ValidationRun run)
        {
            if (validator is ReactiveValidator<TModel> rules)
            {
                await rules.RunRules(instance, prefix, run).ConfigureAwait(false);
                return;
            }

            var result = run.Synchronous
                ? validator.Validate(instance)
                : await validator.ValidateAsync(instance, run.Cancellation).ConfigureAwait(false);
            foreach (var failure in result.Errors)
                run.Fail(prefix + failure.PropertyName, failure.ErrorMessage);
        }
    }
}
