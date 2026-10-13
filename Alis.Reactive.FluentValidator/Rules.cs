using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>A <c>When</c> or <c>Unless</c>: the server's test, and the browser's form of it when it has one.</summary>
    internal sealed class Condition<T>
    {
        private readonly Func<T, bool> _holds;

        private Condition(Func<T, bool> holds, ClientCondition? browser)
        {
            _holds = holds;
            Browser = browser;
        }

        /// <summary>The browser's form; null when part of the condition is a test only C# can make.</summary>
        internal ClientCondition? Browser { get; }

        internal bool Holds(T instance) => _holds(instance);

        internal static Condition<T> When(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return new Condition<T>(predicate.Compile(), BrowserCondition.From(predicate));
        }

        internal static Condition<T> Unless(Expression<Func<T, bool>> predicate) => When(predicate).Negated();

        /// <summary>Holds always, with no browser form: what <c>.ServerOnly()</c> writes after a rule's checks.</summary>
        internal static Condition<T> ServerOnly { get; } = new Condition<T>(_ => true, null);

        internal Condition<T> Negated() =>
            new Condition<T>(instance => !_holds(instance), Browser == null ? null : ClientCondition.Not(Browser));
    }

    /// <summary>Every condition a rule is written under, outermost first.</summary>
    internal sealed class Conditions<T>
    {
        private readonly IReadOnlyList<Condition<T>> _all;

        private Conditions(IReadOnlyList<Condition<T>> all)
        {
            _all = all;
        }

        internal static Conditions<T> None { get; } = new Conditions<T>(Array.Empty<Condition<T>>());

        internal Conditions<T> And(Condition<T> condition) => new Conditions<T>(_all.Concat(new[] { condition }).ToArray());

        internal bool Hold(T instance) => _all.All(condition => condition.Holds(instance));

        /// <summary>The browser's form of all of them, or false when one is the server's alone.</summary>
        internal bool TryForBrowser(out ClientCondition condition)
        {
            condition = ClientCondition.Always;
            if (_all.Any(each => each.Browser == null)) return false;
            if (_all.Count > 0) condition = BrowserCondition.AllOf(_all.Select(each => each.Browser!).ToArray());
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

        void DescribeFor(ClientRuleSet browser);
    }

    /// <summary>The checks and child validators written after one <c>RuleFor</c> or <c>RuleForEach</c>.</summary>
    internal abstract class Rule<T, TValue> : IRule<T>, IRuleBuilderOptions<T, TValue>, IRuleTarget<T, TValue>
    {
        // A rule names a member: its errors are reported under the member's path, and the page binds it.
        protected Rule(ModelMember<T> member, Conditions<T> scope)
        {
            FieldPath = member.Path ?? throw new ArgumentException(
                $"A rule names a member of the model, like x => x.Address.City; {member.Written} computes a value. " +
                "Write the rule on the member it reads, with Must for what C# decides.");
            FieldDisplayName = DisplayName.Of(FieldPath);
            Member = member;
            Scope = scope;
        }

        protected ModelMember<T> Member { get; }

        protected Conditions<T> Scope { get; }

        protected List<Component<T, TValue>> Components { get; } = new List<Component<T, TValue>>();

        /// <summary>The member's path, as model binding names it and as errors are reported.</summary>
        protected string FieldPath { get; }

        /// <summary>The member as a message names it: <c>AdmissionDate</c> reads "Admission Date".</summary>
        protected string FieldDisplayName { get; }

        public bool IsAsync => Components.Any(component => component.IsAsync);

        public abstract Task Validate(T instance, string prefix, ValidationRun run);

        public abstract void DescribeFor(ClientRuleSet browser);

        // The browser's condition for one check: the rule's enclosing blocks and the check's own conditions,
        // or false when one of them is the server's alone.
        protected bool TryBrowserCondition(Component<T, TValue> component, out ClientCondition condition)
        {
            condition = ClientCondition.Always;
            if (!Scope.TryForBrowser(out var scope) || !component.Conditions.TryForBrowser(out var own)) return false;
            condition = ReferenceEquals(scope, ClientCondition.Always) ? own
                : ReferenceEquals(own, ClientCondition.Always) ? scope
                : ClientCondition.All(scope, own);
            return true;
        }

        public IRuleBuilderOptions<T, TValue> Add(Check<T, TValue> check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            Components.Add(new CheckComponent<T, TValue>(check));
            return this;
        }

        public IRuleBuilderOptions<T, TValue> SetValidator(IValidator<TValue> validator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            Components.Add(new ChildComponent<T, TValue>(validator));
            return this;
        }

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

        public IRuleBuilderOptions<T, TValue> When(Expression<Func<T, bool>> predicate) => Conditioned(Condition<T>.When(predicate));

        public IRuleBuilderOptions<T, TValue> Unless(Expression<Func<T, bool>> predicate) => Conditioned(Condition<T>.Unless(predicate));

        public IRuleBuilderOptions<T, TValue> ServerOnly() => Conditioned(Condition<T>.ServerOnly);

        // A condition written after checks applies to every check written before it, as in FluentValidation.
        private IRuleBuilderOptions<T, TValue> Conditioned(Condition<T> condition)
        {
            foreach (var component in Components)
                component.Conditions = component.Conditions.And(condition);
            return this;
        }

        private CheckComponent<T, TValue> LastCheck(string option) =>
            Components.LastOrDefault() as CheckComponent<T, TValue>
            ?? throw new InvalidOperationException($"{option} names the check written just before it; write a check first, like .NotEmpty().{option}(..).");
    }

    /// <summary><c>RuleFor(x =&gt; x.Member)</c>: the checks of one member.</summary>
    internal sealed class MemberRule<T, TValue> : Rule<T, TValue>
    {
        internal MemberRule(Expression<Func<T, TValue>> member, Conditions<T> scope)
            : base(ModelMember<T>.Of(member), scope)
        {
        }

        public override async Task Validate(T instance, string prefix, ValidationRun run)
        {
            if (!Scope.Hold(instance)) return;
            var value = (TValue)Member.ValueIn(instance)!;
            foreach (var component in Components)
                await component.Validate(instance, value, prefix + FieldPath, FieldDisplayName, run).ConfigureAwait(false);
        }

        public override void DescribeFor(ClientRuleSet browser)
        {
            foreach (var component in Components)
                if (TryBrowserCondition(component, out var when))
                    component.DescribeFor(browser, FieldPath, Member.Type, FieldDisplayName, when);
        }
    }

    /// <summary><c>RuleForEach(x =&gt; x.Items)</c>: the checks of every item of a list, named <c>Items[0]</c>, <c>Items[1]</c>.</summary>
    internal sealed class CollectionRule<T, TElement> : Rule<T, TElement>
    {
        internal CollectionRule(Expression<Func<T, IEnumerable<TElement>>> collection, Conditions<T> scope)
            : base(ModelMember<T>.Of(collection), scope)
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
                foreach (var component in Components)
                    await component.Validate(instance, item, path, FieldDisplayName, run).ConfigureAwait(false);
            }
        }

        // The browser validates an item through the item's own validator, at each item the page renders.
        public override void DescribeFor(ClientRuleSet browser)
        {
            foreach (var component in Components.OfType<ChildComponent<T, TElement>>())
                if (TryBrowserCondition(component, out var when) && component.TryItemFields(out var itemFields))
                    browser.Add(FieldPath, Member.Type, itemFields, when);
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

        public void DescribeFor(ClientRuleSet browser)
        {
            if (!_scope.TryForBrowser(out var scope) || !(_included is ReactiveValidator rules)) return;
            browser.Add(rules.ClientRules(), "", scope);
        }
    }

    /// <summary>One check or child validator of a rule, with the conditions written after it.</summary>
    internal abstract class Component<T, TValue>
    {
        internal Conditions<T> Conditions { get; set; } = Conditions<T>.None;

        internal abstract bool IsAsync { get; }

        internal abstract Task Validate(T instance, TValue value, string path, string displayName, ValidationRun run);

        internal abstract void DescribeFor(ClientRuleSet browser, string path, Type type, string displayName, ClientCondition when);
    }

    internal sealed class CheckComponent<T, TValue> : Component<T, TValue>
    {
        private readonly Check<T, TValue> _check;

        internal CheckComponent(Check<T, TValue> check)
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
        internal override void DescribeFor(ClientRuleSet browser, string path, Type type, string displayName, ClientCondition when)
        {
            if (!(_check is IBrowserCheck term)) return;
            var arguments = new MessageArguments(displayName);
            _check.NameRenderArguments(arguments);
            if (!Message.TryForBrowser(arguments, out var text)) return;
            foreach (var rule in term.ForBrowser(text))
                browser.Add(path, type, rule.When(when));
        }
    }

    internal sealed class ChildComponent<T, TValue> : Component<T, TValue>
    {
        private readonly IValidator<TValue> _child;

        internal ChildComponent(IValidator<TValue> child)
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

        internal override void DescribeFor(ClientRuleSet browser, string path, Type type, string displayName, ClientCondition when)
        {
            if (TryItemFields(out var fields)) browser.Add(fields, path, when);
        }

        // A validator the app wrote another way than ReactiveValidator gives the browser nothing.
        internal bool TryItemFields(out IReadOnlyList<ClientValidationField> fields)
        {
            fields = _child is ReactiveValidator rules ? rules.ClientRules() : Array.Empty<ClientValidationField>();
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
