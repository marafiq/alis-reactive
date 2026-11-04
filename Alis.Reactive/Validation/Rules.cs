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
    internal sealed class RuleCondition<T>
    {
        private readonly Func<T, RuleSetSelection, bool> _holds;
        private readonly Func<Condition?> _browser;
        private readonly string? _unreadable;
        private readonly Func<T, RuleSetSelection, CancellationToken, Task<bool>>? _waits;

        private RuleCondition(Func<T, RuleSetSelection, bool> holds, Func<Condition?> browser, string? unreadable)
        {
            _holds = holds;
            _browser = browser;
            _unreadable = unreadable;
        }

        private RuleCondition(Func<T, RuleSetSelection, CancellationToken, Task<bool>> waits)
        {
            _holds = (_, _) => throw new InvalidOperationException("A condition that waits is asked through HoldsAsync.");
            _browser = () => null;
            _unreadable = null;
            _waits = waits;
        }

        internal bool Waits => _waits != null;

        internal Task<bool> HoldsAsync(T instance, RuleSetSelection selection, CancellationToken cancellation)
            => _waits != null ? _waits(instance, selection, cancellation) : Task.FromResult(_holds(instance, selection));

        internal static RuleCondition<T> OnServerAsync(Func<T, CancellationToken, Task<bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new RuleCondition<T>((instance, _, cancellation) => predicate(instance, cancellation));
        }

        internal static RuleCondition<T> Passing<TValue>(Rule<T, TValue> rule)
            => new RuleCondition<T>(rule.PassesOn, rule.BrowserPassing, null);

        internal static RuleCondition<T> When(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var browser = BrowserCondition.From(predicate);
            var holds = predicate.Compile();
            return new RuleCondition<T>((instance, _) => holds(instance), () => browser, browser == null ? predicate.Body.ToString() : null);
        }

        internal static RuleCondition<T> Unless(Expression<Func<T, bool>> predicate) => When(predicate).Negated();

        internal static RuleCondition<T> OnServer(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var holds = predicate.Compile();
            return new RuleCondition<T>((instance, _) => holds(instance), () => null, null);
        }

        internal static RuleCondition<T> ServerOnly { get; } = new RuleCondition<T>((_, _) => true, () => null, null);

        internal RuleCondition<T> Negated()
            => _waits is { } waits
                ? new RuleCondition<T>(async (instance, selection, cancellation) => !await waits(instance, selection, cancellation).ConfigureAwait(false))
                : new RuleCondition<T>((instance, selection) => !_holds(instance, selection), () => _browser() is Condition browser ? Condition.Not(browser) : null, _unreadable);

        internal bool TryForBrowser(out Condition browser)
        {
            if (_unreadable != null)
            {
                throw new InvalidOperationException(
                    $"The condition '{_unreadable}' has no browser form. Write it as tests of model members against fixed " +
                    "values (==, !=, <, <=, >, >=, string.IsNullOrEmpty, Contains, StartsWith, EndsWith, Regex.IsMatch, HasValue, " +
                    "joined by &&, || and !), or declare that the server alone decides it: WhenOnServer or UnlessOnServer.");
            }

            browser = _browser()!;
            return browser != null;
        }
    }

    internal sealed class Conditions<T>
    {
        private readonly IReadOnlyList<RuleCondition<T>> _all;

        private Conditions(IReadOnlyList<RuleCondition<T>> all)
        {
            _all = all;
        }

        internal static Conditions<T> None { get; } = new Conditions<T>(Array.Empty<RuleCondition<T>>());

        internal Conditions<T> And(RuleCondition<T> condition) => new Conditions<T>(_all.Concat(new[] { condition }).ToArray());

        internal bool Waits => _all.Any(condition => condition.Waits);

        internal async Task<bool> HoldAsync(T instance, ValidationRun run)
        {
            foreach (var condition in _all)
                if (!await condition.HoldsAsync(instance, run.Selection, run.Cancellation).ConfigureAwait(false)) return false;
            return true;
        }

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

    internal sealed class ValidationRun
    {
        private readonly List<ValidationFailure> _failures = new List<ValidationFailure>();

        internal ValidationRun(bool synchronous, CancellationToken cancellation, RuleSetSelection selection)
        {
            Synchronous = synchronous;
            Cancellation = cancellation;
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        internal RuleSetSelection Selection { get; }

        internal bool Synchronous { get; }

        internal CancellationToken Cancellation { get; }

        internal IReadOnlyList<ValidationFailure> Failures => _failures;

        internal void Fail(string propertyName, string message) => _failures.Add(new ValidationFailure(propertyName, message));
    }

    internal interface IRule<T>
    {
        bool RunsUnder(RuleSetSelection selection);

        bool IsAsync(RuleSetSelection selection);

        IEnumerable<string> ReachableRuleSets { get; }

        Task Validate(T instance, string prefix, ValidationRun run);

        void DescribeFor(BrowserRuleSet browser, RuleSetSelection selection);
    }

    internal interface IRuleHost<T>
    {
        CascadeMode RuleLevelCascadeMode { get; }

        void DeclareUnder(RuleCondition<T> condition, Action rules);
    }

    internal abstract class Rule<T, TValue> : IRule<T>, IRuleBuilderOptions<T, TValue>, IRuleTarget<T, TValue>
    {
        private readonly IRuleHost<T> _host;
        private CascadeMode? _cascade;

        protected Rule(ModelMember<T> member, Conditions<T> scope, RuleSets sets, IRuleHost<T> host)
        {
            _host = host;
            Sets = sets;
            FieldPath = member.Path ?? throw new ArgumentException(
                $"A rule names a member of the model, like x => x.Address.City; {member.Written} computes a value. " +
                "Write the rule on the member it reads, with Must for what C# decides.");
            FieldDisplayName = DisplayName.Of(FieldPath);
            Member = member;
            Scope = scope;
        }

        protected ModelMember<T> Member { get; }

        protected Conditions<T> Scope { get; }

        protected RuleSets Sets { get; }

        RuleSets IRuleTarget<T, TValue>.Sets => Sets;

        protected List<RulePart<T, TValue>> Parts { get; } = new List<RulePart<T, TValue>>();

        protected string FieldPath { get; }

        protected string FieldDisplayName { get; private set; }

        public bool RunsUnder(RuleSetSelection selection) => selection.Selects(Sets);

        public bool IsAsync(RuleSetSelection selection) => Scope.Waits || Parts.Any(part => part.IsAsync(selection));

        public IEnumerable<string> ReachableRuleSets => Enumerable.Empty<string>();

        protected bool StopsAtFirstFailure => (_cascade ?? _host.RuleLevelCascadeMode) == CascadeMode.Stop;

        public abstract Task Validate(T instance, string prefix, ValidationRun run);

        public abstract void DescribeFor(BrowserRuleSet browser, RuleSetSelection selection);

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
            return Add(new CheckPart<T, TValue>(check));
        }

        public IRuleBuilderOptions<T, TValue> Add(RulePart<T, TValue> part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            Parts.Add(part);
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
            var waits = Sets.Any ? Sets.Names.Any(name => IsAsync(RuleSetSelection.Of(name))) : IsAsync(RuleSetSelection.Default);
            if (waits)
            {
                throw new InvalidOperationException(
                    $"DependentRules follows a rule on '{FieldPath}' that waits (MustAsync, CustomAsync or WhenAsync), and a condition does not wait: " +
                    "write the dependent rules under WhenOnServer with the same test.");
            }

            _host.DeclareUnder(RuleCondition<T>.Passing(this), rules);
            return this;
        }

        internal bool PassesOn(T instance, RuleSetSelection selection)
        {
            var run = new ValidationRun(synchronous: true, CancellationToken.None, selection);
            Validate(instance, "", run).GetAwaiter().GetResult();
            return run.Failures.Count == 0;
        }

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

        private IRuleBuilderOptions<T, TValue> Conditioned(RuleCondition<T> condition)
        {
            foreach (var part in Parts)
                part.Conditions = part.Conditions.And(condition);
            return this;
        }

        private CheckPart<T, TValue> LastCheck(string option)
        {
            if (Parts.LastOrDefault() is CheckPart<T, TValue> check) return check;
            if (Parts.LastOrDefault() is CustomPart<T, TValue>)
                throw new InvalidOperationException($"{option} has nothing to say after Custom: a custom check names its messages where it adds a failure, context.AddFailure(..).");
            throw new InvalidOperationException($"{option} names the check written just before it; write a check first, like .NotEmpty().{option}(..).");
        }
    }

    internal sealed class MemberRule<T, TValue> : Rule<T, TValue>
    {
        internal MemberRule(Expression<Func<T, TValue>> member, Conditions<T> scope, RuleSets sets, IRuleHost<T> host)
            : base(ModelMember<T>.Of(member), scope, sets, host)
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
            if (!await Scope.HoldAsync(instance, run).ConfigureAwait(false)) return;
            var value = (TValue)Member.ValueIn(instance)!;
            foreach (var part in Parts)
            {
                var failures = run.Failures.Count;
                await part.Validate(instance, value, prefix, FieldPath, FieldDisplayName, run).ConfigureAwait(false);
                if (StopsAtFirstFailure && run.Failures.Count > failures) return;
            }
        }

        public override void DescribeFor(BrowserRuleSet browser, RuleSetSelection selection)
        {
            foreach (var part in Parts)
                if (TryBrowserCondition(part, out var when)) part.DescribeFor(browser, Member, FieldDisplayName, when, selection);
        }
    }

    internal sealed class CollectionRule<T, TElement> : Rule<T, TElement>
    {
        internal CollectionRule(Expression<Func<T, IEnumerable<TElement>>> collection, Conditions<T> scope, RuleSets sets, IRuleHost<T> host)
            : base(ModelMember<T>.Of(collection), scope, sets, host)
        {
        }

        internal override Condition? BrowserPassing()
        {
            var collection = FieldReference.Of(Member.Expression);
            var item = FieldReference.ItemOf(collection);
            var rules = new List<BrowserRule>();
            foreach (var part in Parts)
            {
                if (!(part is CheckPart<T, TElement> check) || !TryBrowserCondition(part, out var when) || !check.TryBrowserRules(FieldDisplayName, out var own))
                    return null;
                foreach (var rule in own)
                {
                    if (!rule.TryFor(item, out var resolved)) return null;
                    rules.Add(resolved.When(when));
                }
            }
            return new ItemsPassCondition(collection, rules);
        }

        public override async Task Validate(T instance, string prefix, ValidationRun run)
        {
            if (!await Scope.HoldAsync(instance, run).ConfigureAwait(false)) return;
            if (!(Member.ValueIn(instance) is IEnumerable<TElement> items)) return;

            var index = 0;
            foreach (var item in items)
            {
                var path = FieldPath + "[" + index++ + "]";
                foreach (var part in Parts)
                {
                    var failures = run.Failures.Count;
                    await part.Validate(instance, item, prefix, path, FieldDisplayName, run).ConfigureAwait(false);
                    if (StopsAtFirstFailure && run.Failures.Count > failures) return;
                }
            }
        }

        public override void DescribeFor(BrowserRuleSet browser, RuleSetSelection selection)
        {
            foreach (var part in Parts)
            {
                if (!TryBrowserCondition(part, out var when)) continue;
                if (part is ChildPart<T, TElement> child && child.TryItemFields(selection, out var itemFields))
                    browser.AddItems(Member.Expression, itemFields, when);
                if (part is CheckPart<T, TElement> check)
                    check.DescribeForItems(browser, Member, FieldDisplayName, when);
            }
        }
    }

    internal sealed class IncludeRule<T> : IRule<T>
    {
        private readonly IValidator<T> _included;
        private readonly Conditions<T> _scope;
        private readonly RuleSets _sets;

        internal IncludeRule(IValidator<T> included, Conditions<T> scope, RuleSets sets)
        {
            _included = included ?? throw new ArgumentNullException(nameof(included));
            _scope = scope;
            _sets = sets;
        }

        public bool RunsUnder(RuleSetSelection selection) => selection.Selects(_sets) || (selection.IsNamed && !_sets.Any);

        public bool IsAsync(RuleSetSelection selection) => _scope.Waits || (_included is ReactiveValidator rules && rules.IsAsync(selection));

        public IEnumerable<string> ReachableRuleSets
            => _included is ReactiveValidator rules && !_sets.Any ? rules.DeclaredRuleSets : Enumerable.Empty<string>();

        public async Task Validate(T instance, string prefix, ValidationRun run)
        {
            if (!await _scope.HoldAsync(instance, run).ConfigureAwait(false)) return;
            await Validators.Run(_included, instance, prefix, run).ConfigureAwait(false);
        }

        public void DescribeFor(BrowserRuleSet browser, RuleSetSelection selection)
        {
            if (!(_included is ReactiveValidator rules)) return;
            if (_scope.TryForBrowser(out var scope)) browser.Add(rules.BrowserRules(selection), scope);
        }
    }

    internal abstract class RulePart<T, TValue>
    {
        internal Conditions<T> Conditions { get; set; } = Conditions<T>.None;

        internal abstract bool IsAsync(RuleSetSelection selection);

        internal abstract Task Validate(T instance, TValue value, string prefix, string fieldPath, string displayName, ValidationRun run);

        internal abstract void DescribeFor(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when, RuleSetSelection selection);
    }

    internal sealed class CustomPart<T, TValue> : RulePart<T, TValue>
    {
        private readonly Func<TValue, CustomContext<T>, CancellationToken, Task> _check;
        private readonly bool _waits;

        internal CustomPart(Func<TValue, CustomContext<T>, CancellationToken, Task> check, bool waits)
        {
            _check = check ?? throw new ArgumentNullException(nameof(check));
            _waits = waits;
        }

        internal override bool IsAsync(RuleSetSelection selection) => _waits;

        internal override async Task Validate(T instance, TValue value, string prefix, string fieldPath, string displayName, ValidationRun run)
        {
            if (!await Conditions.HoldAsync(instance, run).ConfigureAwait(false)) return;
            var arguments = new MessageArguments(displayName).With("PropertyValue", value);
            await _check(value, new CustomContext<T>(instance, run, prefix, fieldPath, arguments), run.Cancellation).ConfigureAwait(false);
        }

        internal override void DescribeFor(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when, RuleSetSelection selection)
        {
        }
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

        internal override bool IsAsync(RuleSetSelection selection) => _check.IsAsync;

        internal override async Task Validate(T instance, TValue value, string prefix, string fieldPath, string displayName, ValidationRun run)
        {
            if (!await Conditions.HoldAsync(instance, run).ConfigureAwait(false)) return;
            var arguments = new MessageArguments(displayName).With("PropertyValue", value);
            _check.NameRenderArguments(arguments);
            var passes = run.Synchronous
                ? _check.Passes(instance, value, arguments)
                : await _check.PassesAsync(instance, value, arguments, run.Cancellation).ConfigureAwait(false);
            if (!passes) run.Fail(prefix + fieldPath, Message.For(instance, arguments));
        }

        internal override void DescribeFor(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when, RuleSetSelection selection)
        {
            foreach (var rule in BrowserRules(displayName))
                browser.Add(member.Expression, rule.When(when));
        }

        internal void DescribeForItems(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when)
        {
            var rules = BrowserRules(displayName).ToList();
            if (rules.Count > 0) browser.AddItemRules(member.Expression, rules, when);
        }

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

        internal override bool IsAsync(RuleSetSelection selection) => _child is ReactiveValidator rules && rules.IsAsync(selection);

        internal override async Task Validate(T instance, TValue value, string prefix, string fieldPath, string displayName, ValidationRun run)
        {
            if (!await Conditions.HoldAsync(instance, run).ConfigureAwait(false) || value == null) return;
            await Validators.Run(_child, value, prefix + fieldPath + ".", run).ConfigureAwait(false);
        }

        internal override void DescribeFor(BrowserRuleSet browser, ModelMember<T> member, string displayName, Condition when, RuleSetSelection selection)
        {
            if (TryItemFields(selection, out var fields)) browser.Add(member.Expression, fields, when);
        }

        internal bool TryItemFields(RuleSetSelection selection, out IReadOnlyList<BrowserField> fields)
        {
            fields = _child is ReactiveValidator rules ? rules.BrowserRules(selection) : Array.Empty<BrowserField>();
            return _child is ReactiveValidator;
        }
    }

    internal static class Validators
    {
        internal static async Task Run<TModel>(IValidator<TModel> validator, TModel instance, string prefix, ValidationRun run)
        {
            if (validator is ReactiveValidator<TModel> rules)
            {
                await rules.RunRules(instance, prefix, run).ConfigureAwait(false);
                return;
            }

            var result = run.Synchronous
                ? run.Selection.Validate(validator, instance)
                : await run.Selection.ValidateAsync(validator, instance, run.Cancellation).ConfigureAwait(false);
            foreach (var failure in result.Errors)
                run.Fail(prefix + failure.PropertyName, failure.ErrorMessage);
        }
    }
}
