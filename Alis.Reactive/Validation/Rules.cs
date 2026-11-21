using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Validation;

internal sealed class ValidationRun(bool synchronous, CancellationToken cancellation, string selection, IReadOnlyList<ValidationFailure> binding, IReadOnlyCollection<Type> flags)
{
    internal bool Synchronous { get; } = synchronous;
    internal CancellationToken Cancellation { get; } = cancellation;
    internal string Selection { get; } = selection;
    internal IReadOnlyCollection<Type> Flags { get; } = flags;
    internal List<ValidationFailure> Failures { get; } = [.. binding];

    internal bool Refused(string path) => binding.Any(failure => string.Equals(failure.PropertyName, path, StringComparison.OrdinalIgnoreCase));

    internal bool Selects(IReadOnlyList<string> sets, bool include = false)
    {
        if (Selection == "*") return true;
        if (sets.Count == 0) return include || string.Equals(Selection, "default", StringComparison.OrdinalIgnoreCase);
        return sets.Contains(Selection, StringComparer.OrdinalIgnoreCase);
    }

    internal void RequireAsync()
    {
        if (Synchronous) throw new InvalidOperationException("A rule that waits (MustAsync, CustomAsync or WhenAsync) runs only in ValidateAsync.");
    }

    internal async Task Child<T>(IValidator<T> validator, T instance, string prefix)
    {
        if (validator is ReactiveValidator<T> reactive)
        {
            await reactive.RunRules(new ValidationContext<T>(instance, prefix, this)).ConfigureAwait(false);
            return;
        }

        var result = Synchronous
            ? validator.Validate(instance, Selection)
            : await validator.ValidateAsync(instance, Selection, Cancellation).ConfigureAwait(false);
        foreach (var failure in result.Errors) Failures.Add(new ValidationFailure(prefix + failure.PropertyName, failure.ErrorMessage));
    }
}

internal static class RuleSets
{
    internal static string[] Named(string names) => names.Split([',', ';']).Select(name => name.Trim()).ToArray();
}

internal sealed class ValidationContext<T>(T model, string prefix, ValidationRun run)
{
    private readonly Dictionary<RuleCondition<T>, bool> _conditions = [];
    internal T Model { get; } = model;
    internal string Prefix { get; } = prefix;
    internal ValidationRun Run { get; } = run;

    internal async Task<bool> Holds(RuleCondition<T> condition)
    {
        if (!condition.Shared) return await condition.Evaluate(Model, Run).ConfigureAwait(false);
        if (_conditions.TryGetValue(condition, out var held)) return held;
        var answer = await condition.Evaluate(Model, Run).ConfigureAwait(false);
        _conditions.Add(condition, answer);
        return answer;
    }
}

internal sealed class RuleCondition<T>(Func<T, ValidationRun, Task<bool>> evaluate, bool shared, Expression<Func<T, bool>>? browser)
{
    internal bool Shared { get; } = shared;
    internal Expression<Func<T, bool>>? Browser { get; } = browser;
    internal Task<bool> Evaluate(T model, ValidationRun run) => evaluate(model, run);

    internal static RuleCondition<T> Of(Expression<Func<T, bool>> expression, bool shared) => Read(expression, shared, expression);

    internal static RuleCondition<T> OnServer(Expression<Func<T, bool>> expression, bool shared) => Read(expression, shared, null);

    internal static RuleCondition<T> Awaited(Func<T, CancellationToken, Task<bool>> predicate)
        => new((model, run) =>
        {
            run.RequireAsync();
            return predicate(model, run.Cancellation);
        }, true, null);

    private static RuleCondition<T> Read(Expression<Func<T, bool>> expression, bool shared, Expression<Func<T, bool>>? browser)
    {
        var predicate = ServerExpressions.Read(expression);
        return new RuleCondition<T>((model, _) => Task.FromResult(predicate(model)), shared, browser);
    }
}

internal sealed class RuleGuard<T>(RuleCondition<T> condition, bool negated)
{
    internal RuleCondition<T> Condition { get; } = condition;
    internal bool Negated { get; } = negated;
    internal bool DecidedOnTheServer => Condition.Browser is null;
    internal async Task<bool> Holds(ValidationContext<T> context) => await context.Holds(Condition).ConfigureAwait(false) != Negated;
}

internal abstract class ValidationRule<T>(IReadOnlyList<string> sets)
{
    internal IReadOnlyList<string> Sets { get; } = sets;
    internal List<RuleGuard<T>> Guards { get; } = [];

    internal async Task Validate(ValidationContext<T> context)
    {
        context.Run.Cancellation.ThrowIfCancellationRequested();
        if (!context.Run.Selects(Sets, this is IncludeRule<T>) || Refused(context)) return;
        foreach (var guard in Guards)
            if (!await guard.Holds(context).ConfigureAwait(false)) return;
        await Check(context).ConfigureAwait(false);
    }

    internal IReadOnlyList<RuleGroup> BrowserGroups(BrowserScope<T> scope)
    {
        NameScalars(scope);
        var decidedOnTheServer = Guards.Any(guard => guard.DecidedOnTheServer);
        if (!scope.Selects(Sets, this is IncludeRule<T>) || decidedOnTheServer) return [];
        var rules = BrowserRules(scope);
        if (rules.Count == 0 || Guards.Count == 0) return rules;
        return [new RuleGroup([], scope.When(Guards), [], rules)];
    }

    protected virtual bool Refused(ValidationContext<T> context) => false;
    protected abstract Task Check(ValidationContext<T> context);

    private protected virtual void NameScalars(BrowserScope<T> scope)
    {
    }

    private protected abstract IReadOnlyList<RuleGroup> BrowserRules(BrowserScope<T> scope);
}

internal sealed class IncludeRule<T>(IValidator<T> validator, IReadOnlyList<string> sets) : ValidationRule<T>(sets)
{
    protected override Task Check(ValidationContext<T> context) => context.Run.Child(validator, context.Model, context.Prefix);

    private protected override IReadOnlyList<RuleGroup> BrowserRules(BrowserScope<T> scope)
        => validator is ReactiveValidator<T> reactive ? reactive.BrowserGroups(scope) : [];
}

internal sealed class MemberRule<T, TValue> : ValidationRule<T>, IRuleBuilderOptions<T, TValue>
{
    private readonly ReactiveValidator<T> _owner;
    private readonly LambdaExpression _member;
    private readonly Func<T, IEnumerable<TValue>> _values;
    private readonly bool _each;
    private readonly string _path;
    private readonly List<RulePart<T, TValue>> _parts = [];
    private readonly List<ValidationRule<T>> _dependents = [];
    private Func<CascadeMode> _cascade;
    private string _displayName;
    private bool _serverOnly;

    internal MemberRule(ReactiveValidator<T> owner, LambdaExpression member, string path, Func<T, IEnumerable<TValue>> values, bool each, IReadOnlyList<string> sets, Func<CascadeMode> cascade) : base(sets)
    {
        _owner = owner;
        _member = member;
        _path = path;
        _displayName = DisplayName.Of(path);
        _values = values;
        _each = each;
        _cascade = cascade;
    }

    internal IRuleBuilderOptions<T, TValue> Add(RulePart<T, TValue> part)
    {
        _parts.Add(part);
        return this;
    }

    internal static MemberRule<T, TValue> Of(IRuleBuilder<T, TValue> rule)
        => rule as MemberRule<T, TValue> ?? throw new ArgumentException("Write a check on a rule a ReactiveValidator declares: RuleFor(x => x.Member).", nameof(rule));

    public IRuleBuilderOptions<T, TValue> SetValidator(IValidator<TValue> validator) => Add(new ChildPart<T, TValue>(validator));

    public IRuleBuilder<T, TValue> Cascade(CascadeMode mode)
    {
        _cascade = () => mode;
        return this;
    }

    public IRuleBuilderOptions<T, TValue> WithName(string displayName)
    {
        if (displayName.Length == 0) throw new ArgumentException("The value cannot be an empty string", "overridePropertyName");
        _displayName = displayName;
        return this;
    }

    public IRuleBuilderOptions<T, TValue> WithMessage(string message)
    {
        if (message.Length == 0) throw new ArgumentException("The value cannot be an empty string", "errorMessage");
        _parts[_parts.Count - 1].Written(message);
        return this;
    }

    public IRuleBuilderOptions<T, TValue> WithMessage(Func<T, string> message)
    {
        _parts[_parts.Count - 1].BuiltFromTheModel(message);
        return this;
    }

    public IRuleBuilderOptions<T, TValue> DependentRules(Action rules)
    {
        _dependents.AddRange(_owner.Capture(rules, Sets));
        return this;
    }

    public IRuleBuilderOptions<T, TValue> When(Expression<Func<T, bool>> predicate) => Conditioned(RuleCondition<T>.Of(predicate, false), false);
    public IRuleBuilderOptions<T, TValue> Unless(Expression<Func<T, bool>> predicate) => Conditioned(RuleCondition<T>.Of(predicate, false), true);
    public IRuleBuilderOptions<T, TValue> WhenOnServer(Expression<Func<T, bool>> predicate) => Conditioned(RuleCondition<T>.OnServer(predicate, false), false);
    public IRuleBuilderOptions<T, TValue> UnlessOnServer(Expression<Func<T, bool>> predicate) => Conditioned(RuleCondition<T>.OnServer(predicate, false), true);

    public IRuleBuilderOptions<T, TValue> ServerOnly()
    {
        _serverOnly = true;
        return this;
    }

    private IRuleBuilderOptions<T, TValue> Conditioned(RuleCondition<T> condition, bool negated)
    {
        var guard = new RuleGuard<T>(condition, negated);
        foreach (var part in _parts) part.Guards.Insert(0, guard);
        foreach (var dependent in _dependents) dependent.Guards.Insert(0, guard);
        return this;
    }

    protected override bool Refused(ValidationContext<T> context) => context.Run.Refused(context.Prefix + _path);

    private protected override void NameScalars(BrowserScope<T> scope)
    {
        if (!_each) scope.NameScalar<TValue>(_member, _path);
    }

    private protected override IReadOnlyList<RuleGroup> BrowserRules(BrowserScope<T> scope)
    {
        if (_serverOnly) return [];
        var stops = _cascade() == CascadeMode.Stop;
        var member = scope.Member<TValue>(_member, _path, _each, stops, _displayName, () => _values(scope.Start()).First());
        var segments = new List<RuleGroup>();
        var tests = new List<RuleTest>();
        foreach (var part in _parts)
        {
            var decidedOnTheServer = part.Guards.Any(guard => guard.DecidedOnTheServer);
            if (!decidedOnTheServer && part.Test(member) is { } test)
            {
                tests.Add(test);
                continue;
            }

            if (!decidedOnTheServer && part.Group(member) is { } group)
            {
                member.CloseField(segments, tests);
                segments.Add(part.Guards.Count == 0 ? group : new RuleGroup([], scope.When(part.Guards), [], [group]));
            }

            if (stops) break;
        }

        member.CloseField(segments, tests);
        member.RequireTheServersOrder(_dependents.Count);
        return segments;
    }

    protected override async Task Check(ValidationContext<T> context)
    {
        var before = context.Run.Failures.Count;
        if (_each && !await CheckItems(context, before).ConfigureAwait(false)) return;
        if (!_each) await CheckMember(context, before).ConfigureAwait(false);
        if (context.Run.Failures.Count != before) return;
        foreach (var dependent in _dependents) await dependent.Validate(context).ConfigureAwait(false);
    }

    private async Task CheckMember(ValidationContext<T> context, int before)
    {
        var read = false;
        var value = default(TValue)!;
        foreach (var part in _parts)
        {
            if (!await part.Applies(context).ConfigureAwait(false)) continue;
            if (!read)
            {
                value = _values(context.Model).First();
                read = true;
            }
            var arguments = new MessageArguments(_displayName, context.Prefix + _path, value);
            await part.Validate(context, value, _path, arguments).ConfigureAwait(false);
            if (_cascade() == CascadeMode.Stop && context.Run.Failures.Count > before) return;
        }
    }

    private async Task<bool> CheckItems(ValidationContext<T> context, int before)
    {
        var active = new List<RulePart<T, TValue>>();
        foreach (var part in _parts)
            if (await part.Applies(context).ConfigureAwait(false)) active.Add(part);
        if (active.Count == 0) return false;
        var index = 0;
        foreach (var value in _values(context.Model))
        {
            var fieldPath = _path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
            if (context.Run.Refused(context.Prefix + fieldPath))
            {
                index++;
                continue;
            }
            foreach (var part in active)
            {
                var arguments = new MessageArguments(_displayName, context.Prefix + fieldPath, value).With("CollectionIndex", index);
                await part.Validate(context, value, fieldPath, arguments).ConfigureAwait(false);
                if (_cascade() == CascadeMode.Stop && context.Run.Failures.Count > before) return true;
            }
            index++;
        }
        return true;
    }
}

internal abstract class RulePart<T, TValue>
{
    internal List<RuleGuard<T>> Guards { get; } = [];
    internal Func<T, string> Message { get; private set; } = _ => "";
    private protected string Text { get; private set; } = "";
    private protected bool FromTheModel { get; private set; }

    internal void Written(string message)
    {
        Message = _ => message;
        Text = message;
        FromTheModel = false;
    }

    internal void BuiltFromTheModel(Func<T, string> message)
    {
        Message = message;
        FromTheModel = true;
    }

    internal virtual RuleTest? Test(BrowserMember<T, TValue> member) => null;

    internal virtual RuleGroup? Group(BrowserMember<T, TValue> member) => null;

    internal async Task<bool> Applies(ValidationContext<T> context)
    {
        foreach (var guard in Guards)
            if (!await guard.Holds(context).ConfigureAwait(false)) return false;
        return true;
    }

    internal abstract Task Validate(ValidationContext<T> context, TValue value, string fieldPath, MessageArguments arguments);
}

internal sealed class CheckPart<T, TValue> : RulePart<T, TValue>
{
    private readonly Check<T, TValue> _check;

    internal CheckPart(Check<T, TValue> check)
    {
        _check = check;
        Written(check.Template);
    }

    internal override RuleTest? Test(BrowserMember<T, TValue> member)
    {
        if (FromTheModel) return null;
        return _check.Browser(member) is { } test ? member.Written(test, Guards, Text) : null;
    }

    internal override async Task Validate(ValidationContext<T> context, TValue value, string fieldPath, MessageArguments arguments)
    {
        var passes = await _check.Passes(context.Model, value, arguments, context.Run).ConfigureAwait(false);
        if (!passes) context.Run.Failures.Add(new ValidationFailure(context.Prefix + fieldPath, arguments.Fill(Message(context.Model))));
    }
}

internal sealed class ChildPart<T, TValue>(IValidator<TValue> validator) : RulePart<T, TValue>
{
    internal override RuleGroup? Group(BrowserMember<T, TValue> member)
        => validator is ReactiveValidator<TValue> reactive ? member.Child(reactive) : null;

    internal override Task Validate(ValidationContext<T> context, TValue value, string fieldPath, MessageArguments arguments)
        => value is null ? Task.CompletedTask : context.Run.Child(validator, value, context.Prefix + fieldPath + ".");
}

internal sealed class CustomPart<T, TValue>(Func<TValue, CustomContext<T>, CancellationToken, Task> check, bool waits) : RulePart<T, TValue>
{
    internal override Task Validate(ValidationContext<T> context, TValue value, string fieldPath, MessageArguments arguments)
    {
        if (waits) context.Run.RequireAsync();
        return check(value, new CustomContext<T>(context.Model, context.Run.Failures, context.Prefix, fieldPath), context.Run.Cancellation);
    }
}
