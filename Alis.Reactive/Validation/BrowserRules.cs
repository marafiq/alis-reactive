using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;
using LinqExpression = System.Linq.Expressions.Expression;
using PlanExpression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.Validation;

internal sealed class BrowserTest(FnExpression check, Action<MessageArguments> arguments, IReadOnlyList<string> unknown, bool fillsTheStart)
{
    internal FnExpression Check { get; } = check;
    internal Action<MessageArguments> Arguments { get; } = arguments;
    internal IReadOnlyList<string> Unknown { get; } = unknown;
    internal bool FillsTheStart { get; } = fillsTheStart;
}

internal static class BrowserTerms
{
    internal const string Instance = "instance";
    internal const string Value = "value";

    internal static readonly FnExpression HoldsAValue = new([Value], new OpExpression(Operator.Neq, [new RefExpression(Value), Null]));

    private static LitExpression Null => new(JsonSerializer.SerializeToElement<object?>(null));

    internal static PlanExpression NullOr(PlanExpression value, PlanExpression test) => new OpExpression(Operator.Or, [new OpExpression(Operator.Eq, [value, Null]), test]);

    internal static PlanExpression Matches(PlanExpression value, string pattern) => new OpExpression(Operator.Matches, [value, Literals.Text(pattern)]);

    internal static NotImplementedException NoOperatorFor(string check)
        => new($"{check} reads text by .NET's own test, which no operator of the plan writes: a rule that checks {check} has no browser form yet; mark the rule .ServerOnly().");
}

internal sealed class BrowserScalars(AppJson json)
{
    private const string Before = "The value '";
    private readonly List<BoundScalar> _named = [];
    private readonly HashSet<string> _paths = new(StringComparer.Ordinal);

    internal IReadOnlyList<BoundScalar> Named => _named;

    internal void Name<TValue>(IReadOnlyList<RegistrationStep> path, string clrPath)
    {
        var contract = json.LiveForm.GetTypeInfo(typeof(TValue));
        if (contract.Kind != System.Text.Json.Serialization.Metadata.JsonTypeInfoKind.None || !_paths.Add(clrPath)) return;
        var shape = InputShapes.Of(contract, JsonSerializer.SerializeToElement(default(TValue), contract));
        if (shape is StringShape) return;
        var member = clrPath.Substring(clrPath.LastIndexOf('.') + 1);
        _named.Add(new BoundScalar(path, shape, Before, "' is not valid for " + member + "."));
    }
}

internal sealed class BrowserScope<T>
{
    private readonly ValidationRun _selection;

    internal BrowserScope(ValidationRun selection, AppJson json, HeldForm form, BrowserScalars scalars, IReadOnlyList<RegistrationStep> path, string prefix, bool each, Func<T> start)
    {
        _selection = selection;
        Json = json;
        Form = form;
        Scalars = scalars;
        Path = path;
        Prefix = prefix;
        Each = each;
        Start = start;
    }

    internal AppJson Json { get; }
    internal HeldForm Form { get; }
    internal BrowserScalars Scalars { get; }
    internal IReadOnlyList<RegistrationStep> Path { get; }
    internal string Prefix { get; }
    internal bool Each { get; }
    internal Func<T> Start { get; }
    internal IReadOnlyCollection<Type> Flags => _selection.Flags;

    internal bool Selects(IReadOnlyList<string> sets, bool include) => _selection.Selects(sets, include);

    internal FnExpression When(IReadOnlyList<RuleGuard<T>> guards)
    {
        if (guards.Count == 0) return ItemLambda.EveryItem();
        var conditions = guards.Select(guard => (Read: Condition(guard.Condition.Browser!), guard.Negated)).ToList();
        var body = conditions.Skip(1).Aggregate(Term(conditions[0]), (all, next) => new OpExpression(Operator.And, [all, Term(next)]));
        return new FnExpression(conditions[0].Read.Params, body);
    }

    internal void NameScalar<TValue>(LambdaExpression member, string path)
    {
        if (BodyPathOf(member, Json) is { } body) Scalars.Name<TValue>([.. Path, .. body], Prefix + path);
    }

    private static IReadOnlyList<PathStep>? BodyPathOf(LambdaExpression member, AppJson json)
    {
        var read = member.Body;
        while (read is UnaryExpression { NodeType: ExpressionType.Convert } conversion) read = conversion.Operand;
        try
        {
            return MemberPath.TryOf(read, member.Parameters[0], member, json.App, out var members) ? members.BodyPath : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    internal BrowserMember<T, TValue> Member<TValue>(LambdaExpression member, string path, bool each, string displayName, Func<TValue> start)
        => new(this, member, path, each, displayName, start);

    internal BrowserScope<TChild> Child<TChild>(IReadOnlyList<RegistrationStep> path, string prefix, bool each, Func<TChild> start)
        => new(_selection, Json, Form, Scalars, path, prefix, each, start);

    private static PlanExpression Term((FnExpression Read, bool Negated) condition)
        => condition.Negated ? new OpExpression(Operator.Not, [condition.Read.Body]) : condition.Read.Body;

    private FnExpression Condition(Expression<Func<T, bool>> condition)
    {
        try
        {
            return ItemLambda.Of(condition, Form);
        }
        catch (InvalidOperationException refused)
        {
            throw new InvalidOperationException(
                $"The condition '{Printed.Of(condition)}' has no browser form. Write it as tests of model members against fixed values or other members, or declare that the server alone decides it: WhenOnServer or UnlessOnServer.",
                refused);
        }
    }
}

internal sealed class BrowserMember<T, TValue>
{
    private static readonly string[] SentValues = ["PropertyValue", "TotalLength", "Digits", "ActualScale"];
    private readonly BrowserScope<T> _scope;
    private readonly LambdaExpression _member;
    private readonly string _path;
    private readonly bool _each;
    private readonly string _displayName;
    private readonly Func<TValue> _start;
    private IReadOnlyList<RegistrationStep> _fieldPath = [];
    private int _written;

    internal BrowserMember(BrowserScope<T> scope, LambdaExpression member, string path, bool each, string displayName, Func<TValue> start)
    {
        _scope = scope;
        _member = member;
        _path = path;
        _each = each;
        _displayName = displayName;
        _start = start;
    }

    internal IReadOnlyCollection<Type> Flags => _scope.Flags;

    internal FnExpression When(IReadOnlyList<RuleGuard<T>> guards) => _scope.When(guards);

    internal BrowserTest Tests(LambdaExpression passes, Action<MessageArguments> arguments)
    {
        var read = Translated(passes);
        return new BrowserTest(new FnExpression([read.Params[0], BrowserTerms.Instance], read.Body), arguments, [], false);
    }

    internal BrowserTest TestsTheValue(Func<PlanExpression, PlanExpression> passes, Action<MessageArguments> arguments)
        => new(new FnExpression([BrowserTerms.Value, BrowserTerms.Instance], passes(new RefExpression(BrowserTerms.Value))), arguments, [], false);

    internal BrowserTest NotNull()
    {
        if (default(TValue) is not null) return TestsTheValue(_ => new LitExpression(JsonSerializer.SerializeToElement(true)), NoArguments);
        var value = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        return Tests(LinqExpression.Lambda(LinqExpression.NotEqual(value, LinqExpression.Constant(null, typeof(TValue))), value), NoArguments);
    }

    internal BrowserTest Emptiness(bool empty)
    {
        if (typeof(TValue) == typeof(string)) return Tests(empty ? Blank : NotBlank, NoArguments);
        var contract = _scope.Json.LiveForm.GetTypeInfo(typeof(TValue));
        if (contract.IsDictionary(out _, out _))
            throw new NotImplementedException($"'{_path}' holds a dictionary, which the browser holds as an object whose entries the plan's operators do not count: NotEmpty and Empty over a dictionary have no browser form yet; mark the rule .ServerOnly().");
        if (contract.IsList(out _))
            return TestsTheValue(value => empty ? new OpExpression(Operator.Empty, [value]) : new OpExpression(Operator.Not, [new OpExpression(Operator.Empty, [value])]), NoArguments);
        var held = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        var start = LinqExpression.Constant(default(TValue), typeof(TValue));
        var test = empty ? LinqExpression.Equal(held, start) : LinqExpression.NotEqual(held, start);
        return Tests(LinqExpression.Lambda(test, held), NoArguments);
    }

    internal BrowserTest Compared(Comparison comparison, object? other)
    {
        if (comparison.Orders && typeof(TValue) == typeof(string))
            throw new NotImplementedException($"'{_path}' is text, which .NET orders by the culture's comparer and the plan's ordering operators do not: an ordering of text has no browser form yet; mark the rule .ServerOnly().");
        var value = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        var test = Binary(comparison.Node, value, LinqExpression.Constant(other, typeof(TValue)));
        var passes = comparison.Orders && default(TValue) is null ? LinqExpression.OrElse(IsNull(value), test) : test;
        return Tests(LinqExpression.Lambda(passes, value), arguments => arguments.With("ComparisonValue", other).With("ComparisonProperty", ""));
    }

    internal BrowserTest ComparedWithMember<TPeer>(Comparison comparison, Expression<Func<T, TPeer>> peer, string peerName)
    {
        if (_each)
            throw new NotImplementedException($"'{_path}' compares each item with another member, and a rule's test reads the item and its holder, not the model above it: a comparison of an item with a member has no browser form yet; mark the rule .ServerOnly().");
        var instance = _member.Parameters[0];
        var left = Unconverted(_member.Body);
        var right = new ParameterSwap(peer.Parameters[0], instance).Visit(peer.Body);
        if (Nullable.GetUnderlyingType(left.Type) == right.Type) right = LinqExpression.Convert(right, left.Type);
        if (Nullable.GetUnderlyingType(right.Type) == left.Type) left = LinqExpression.Convert(left, right.Type);
        var test = Binary(comparison.Node, left, right);
        var otherHeld = default(TPeer) is null ? LinqExpression.AndAlso(LinqExpression.Not(IsNull(right)), test) : test;
        var passes = !comparison.Orders ? test : default(TValue) is null ? LinqExpression.OrElse(IsNull(left), otherHeld) : otherHeld;
        var read = Translated(LinqExpression.Lambda(passes, instance));
        return new BrowserTest(new FnExpression([BrowserTerms.Value, read.Params[0]], read.Body), arguments => arguments.With("ComparisonProperty", peerName), ["ComparisonValue"], false);
    }

    internal BrowserTest Between(object? from, object? to, bool inclusive, Action<MessageArguments> arguments)
    {
        var value = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        var low = Binary(inclusive ? ExpressionType.GreaterThanOrEqual : ExpressionType.GreaterThan, value, LinqExpression.Constant(from, typeof(TValue)));
        var high = Binary(inclusive ? ExpressionType.LessThanOrEqual : ExpressionType.LessThan, value, LinqExpression.Constant(to, typeof(TValue)));
        var inside = LinqExpression.AndAlso(low, high);
        var passes = default(TValue) is null ? LinqExpression.OrElse(IsNull(value), inside) : inside;
        return Tests(LinqExpression.Lambda(passes, value), arguments);
    }

    internal BrowserTest InEnum(IEnumerable<object> values)
    {
        var value = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        var named = values.Select(declared => (LinqExpression)LinqExpression.Equal(value, LinqExpression.Constant(declared, typeof(TValue)))).ToList();
        var declaredOne = named.Count == 0 ? LinqExpression.Constant(false) : named.Skip(1).Aggregate(named[0], LinqExpression.OrElse);
        var passes = default(TValue) is null ? LinqExpression.OrElse(IsNull(value), declaredOne) : declaredOne;
        var read = Translated(LinqExpression.Lambda(passes, value));
        return new BrowserTest(new FnExpression([read.Params[0], BrowserTerms.Instance], read.Body), NoArguments, [], true);
    }

    internal string Message(string text, BrowserTest test)
    {
        var unknown = test.Unknown.Concat(_each ? ["CollectionIndex", "PropertyPath"] : _scope.Each ? ["PropertyPath"] : []).ToList();
        var named = unknown.FirstOrDefault(name => text.Contains("{" + name));
        if (named is not null)
            throw new NotImplementedException($"The message '{text}' names {{{named}}}, which only the server knows of '{_path}' when the check runs: a browser copy of it has no form yet; write the message without it, or mark the rule .ServerOnly().");
        var dropped = test.FillsTheStart ? SentValues.Skip(1) : SentValues;
        var kept = Sentences(text).Where(sentence => !dropped.Any(name => sentence.Contains("{" + name))).ToList();
        if (kept.Count == 0)
            throw new NotImplementedException($"Every sentence of the message '{text}' names a value the request sends, which the browser's copy leaves out: a browser copy of it has no form yet; write the message without it, or mark the rule .ServerOnly().");
        var start = test.FillsTheStart ? _each ? default : _start() : default;
        var arguments = new MessageArguments(_displayName, _scope.Prefix + _path, start);
        test.Arguments(arguments);
        return arguments.Fill(string.Join(" ", kept));
    }

    internal void CloseField(List<RuleGroup> segments, List<RuleTest> tests)
    {
        if (tests.Count == 0) return;
        _written += tests.Count;
        segments.Add(new RuleGroup([], ItemLambda.EveryItem(), [new RuleField(FieldPath(), [.. tests])], []));
        tests.Clear();
    }

    internal RuleGroup Child(ReactiveValidator<TValue> reactive)
    {
        _written++;
        var path = FieldPath();
        var scope = _scope.Child([.. _scope.Path, .. path], _scope.Prefix + _path + ".", _each || _scope.Each, ChildStart);
        return new RuleGroup(path, BrowserTerms.HoldsAValue, [], reactive.BrowserGroups(scope));
    }

    internal void RequireTheServersOrder(CascadeMode cascade, int dependents)
    {
        if (_written == 0) return;
        if (dependents > 0)
            throw new NotImplementedException($"The rule of '{_path}' declares DependentRules, which run only after the rule passes, and a plan's rules run side by side: DependentRules have no browser form yet; mark the rule .ServerOnly().");
        if (cascade == CascadeMode.Stop && (_each || _written > 1))
            throw new NotImplementedException($"The rule of '{_path}' stops at its first failure, and a plan's tests of a member run side by side: CascadeMode.Stop over more than one browser test has no browser form yet; mark the rule .ServerOnly().");
    }

    private static Expression<Func<string?, bool>> Blank => value => string.IsNullOrWhiteSpace(value);

    private static Expression<Func<string?, bool>> NotBlank => value => !string.IsNullOrWhiteSpace(value);

    private static void NoArguments(MessageArguments arguments)
    {
    }

    private TValue ChildStart()
    {
        var start = _each ? default : _start();
        return start ?? JsonSerializer.Deserialize<TValue>("{}", Contracts.Binding(_scope.Json.App))!;
    }

    private IReadOnlyList<RegistrationStep> FieldPath()
    {
        if (_fieldPath.Count > 0) return _fieldPath;
        var body = Unconverted(_member.Body);
        if (!MemberPath.TryOf(body, _member.Parameters[0], _member, _scope.Json.App, out var members))
            throw new NotImplementedException($"The rule of '{_path}' reads no member path, and a plan's rule names the member it checks by its path: a rule over a list position or a computed value has no browser form yet; mark the rule .ServerOnly().");
        _fieldPath = _each ? [.. members.BodyPath, EachRegistrationStep.Instance] : [.. members.BodyPath];
        return _fieldPath;
    }

    private FnExpression Translated(LambdaExpression passes)
    {
        try
        {
            return ItemLambda.Of(passes, _scope.Form);
        }
        catch (InvalidOperationException refused)
        {
            throw new InvalidOperationException($"The rule of '{_path}' has no browser form: {refused.Message} Mark the rule .ServerOnly() to keep it on the server.", refused);
        }
    }

    private LinqExpression Binary(ExpressionType node, LinqExpression left, LinqExpression right)
    {
        try
        {
            return LinqExpression.MakeBinary(node, left, right);
        }
        catch (InvalidOperationException)
        {
            throw new NotImplementedException($"'{_path}' is compared by its type's own comparer, which declares no {node} operator the plan can write: the comparison has no browser form yet; mark the rule .ServerOnly().");
        }
    }

    private static LinqExpression IsNull(LinqExpression read) => LinqExpression.Equal(read, LinqExpression.Constant(null, read.Type));

    private static LinqExpression Unconverted(LinqExpression read)
    {
        while (read is UnaryExpression { NodeType: ExpressionType.Convert } conversion) read = conversion.Operand;
        return read;
    }

    private static IEnumerable<string> Sentences(string text)
    {
        var at = 0;
        while (at < text.Length)
        {
            var end = text.IndexOf(". ", at, StringComparison.Ordinal);
            if (end < 0)
            {
                yield return text.Substring(at);
                yield break;
            }

            yield return text.Substring(at, end + 1 - at);
            at = end + 2;
        }
    }

    private sealed class ParameterSwap(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override LinqExpression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}
