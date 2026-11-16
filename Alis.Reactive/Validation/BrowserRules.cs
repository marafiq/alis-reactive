using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

    internal BrowserScope(ValidationRun selection, AppJson json, HeldForm form, BrowserScalars scalars, IReadOnlyList<RegistrationStep> path, MemberPath holder, string prefix, bool each, Func<T> start)
    {
        _selection = selection;
        Json = json;
        Form = form;
        Scalars = scalars;
        Path = path;
        Holder = holder;
        Prefix = prefix;
        Each = each;
        Start = start;
    }

    internal AppJson Json { get; }
    internal HeldForm Form { get; }
    internal BrowserScalars Scalars { get; }
    internal IReadOnlyList<RegistrationStep> Path { get; }
    internal MemberPath Holder { get; }
    internal string Prefix { get; }
    internal bool Each { get; }
    internal Func<T> Start { get; }
    internal IReadOnlyCollection<Type> Flags => _selection.Flags;

    internal bool Selects(IReadOnlyList<string> sets, bool include) => _selection.Selects(sets, include);

    internal bool ChecksALeftOutMember(LambdaExpression read) => IsTheModel && LeftOutMembers.Checks(read, Json.App);

    internal TRead ReadingLeftOutMembersAsStarts<TRead>(TRead read)
        where TRead : LambdaExpression
        => IsTheModel ? LeftOutMembers.ReadAsStarts(read, Start(), Json.App) : read;

    private bool IsTheModel => Prefix.Length == 0;

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

    internal BrowserMember<T, TValue> Member<TValue>(LambdaExpression member, string path, bool each, bool stops, string displayName, Func<TValue> start)
        => new(this, member, path, each, stops, displayName, start);

    internal BrowserScope<TChild> Child<TChild>(IReadOnlyList<RegistrationStep> path, MemberPath holder, string prefix, bool each, Func<TChild> start)
        => new(_selection, Json, Form, Scalars, path, holder, prefix, each, start);

    private static PlanExpression Term((FnExpression Read, bool Negated) condition)
        => condition.Negated ? new OpExpression(Operator.Not, [condition.Read.Body]) : condition.Read.Body;

    private FnExpression Condition(Expression<Func<T, bool>> condition)
    {
        try
        {
            return ItemLambda.Of(ReadingLeftOutMembersAsStarts(condition), Form);
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
    private readonly bool _stops;
    private readonly string _displayName;
    private readonly Func<TValue> _start;
    private readonly List<(FnExpression Check, FnExpression When)> _earlier = [];
    private int _written;

    internal BrowserMember(BrowserScope<T> scope, LambdaExpression member, string path, bool each, bool stops, string displayName, Func<TValue> start)
    {
        _scope = scope;
        _member = member;
        _path = path;
        _each = each;
        _stops = stops;
        _displayName = displayName;
        _start = start;
    }

    internal IReadOnlyCollection<Type> Flags => _scope.Flags;

    internal string Path => _path;

    internal RuleTest Written(BrowserTest test, IReadOnlyList<RuleGuard<T>> guards, string text)
    {
        var when = _scope.When(guards);
        var message = Message(text, test);
        var reads = ReadingItsStartWhereLeftOut(test.Check);
        if (!_stops) return new RuleTest(reads, when, message);
        var check = PassingWhereAnEarlierFailed(reads);
        _earlier.Add((reads, when));
        return new RuleTest(check, when, message);
    }

    internal BrowserTest Tests(LambdaExpression passes, string check, Action<MessageArguments> arguments)
    {
        var read = ItemLambda.Checking(passes, FormItsTestReads(check), Term(check), DatesJudgedBy);
        return new BrowserTest(new FnExpression([read.Params[0], BrowserTerms.Instance], read.Body), arguments, [], false);
    }

    internal BrowserTest TestsTheValue(Func<PlanExpression, PlanExpression> passes, string check, Action<MessageArguments> arguments)
    {
        var form = FormItIsHeldIn();
        if (form.Own is { Member.HasItsOwnConverter: true } own) throw ItemLambda.WrittenUnlikeItsBase(Term(check), own.Member, form);
        return Whole(passes, arguments);
    }

    internal BrowserTest TestsTheEmailAddress() => TestsTheText(Operator.EmailAddress, nameof(RuleBuilderExtensions.EmailAddress));

    internal BrowserTest TestsTheCardNumber() => TestsTheText(Operator.CreditCard, nameof(RuleBuilderExtensions.CreditCard));

    internal BrowserTest TestsTheWebAddress()
        => TestsTheValue(value => new OpExpression(Operator.Or, [new OpExpression(Operator.NullOrEmpty, [value]), new OpExpression(Operator.Url, [value])]),
            nameof(RuleBuilderExtensions.Url), NoArguments);

    internal BrowserTest TestsTheDigits(int precision, int scale, Action<MessageArguments> arguments)
        => TestsTheValue(value => BrowserTerms.NullOr(value, new OpExpression(Operator.PrecisionScale, [value, Number(precision), Number(scale)])),
            nameof(RuleBuilderExtensions.PrecisionScale), arguments);

    internal BrowserTest NotNull()
    {
        if (default(TValue) is not null) return Whole(_ => new LitExpression(JsonSerializer.SerializeToElement(true)), NoArguments);
        var value = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        return Tests(LinqExpression.Lambda(LinqExpression.NotEqual(value, LinqExpression.Constant(null, typeof(TValue))), value), nameof(RuleBuilderExtensions.NotNull), NoArguments);
    }

    internal BrowserTest Emptiness(bool empty)
    {
        var check = empty ? nameof(RuleBuilderExtensions.Empty) : nameof(RuleBuilderExtensions.NotEmpty);
        if (typeof(TValue) == typeof(string)) return Tests(empty ? Blank : NotBlank, check, NoArguments);
        var contract = _scope.Json.LiveForm.GetTypeInfo(typeof(TValue));
        if (contract.IsDictionary(out _, out _))
            throw new InvalidOperationException($"The rule of '{_path}' checks whether a dictionary is empty, and the browser's rules count a list's items, not a dictionary's entries; mark the rule .ServerOnly().");
        if (contract.IsList(out _))
            return TestsTheValue(value => empty ? new OpExpression(Operator.Empty, [value]) : new OpExpression(Operator.Not, [new OpExpression(Operator.Empty, [value])]), check, NoArguments);
        var held = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        var start = LinqExpression.Constant(default(TValue), typeof(TValue));
        var test = empty ? LinqExpression.Equal(held, start) : LinqExpression.NotEqual(held, start);
        return Tests(LinqExpression.Lambda(test, held), check, NoArguments);
    }

    internal BrowserTest Compared(Comparison comparison, object? other)
    {
        if (comparison.Orders && typeof(TValue) == typeof(string)) throw OrdersText();
        var value = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        var test = Binary(comparison.Node, value, LinqExpression.Constant(other, typeof(TValue)));
        var passes = comparison.Orders && default(TValue) is null ? LinqExpression.OrElse(IsNull(value), test) : test;
        return Tests(LinqExpression.Lambda(passes, value), comparison.Name, arguments => arguments.With("ComparisonValue", other).With("ComparisonProperty", ""));
    }

    internal BrowserTest ComparedWithMember<TPeer>(Comparison comparison, Expression<Func<T, TPeer>> peer, string peerName)
    {
        if (!ChecksALeftOutMember)
        {
            _ = ItsMemberPath();
            RefuseAReadTheBrowserDoesNotHold(_member);
        }

        var peerAsBound = _scope.ReadingLeftOutMembersAsStarts(peer);
        RefuseAReadTheBrowserDoesNotHold(peerAsBound);
        void Arguments(MessageArguments arguments) => arguments.With("ComparisonProperty", peerName);
        if (_each)
        {
            var item = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
            var itemBesidePeer = PassesBeside<TPeer>(comparison, item, peerAsBound.Body);
            var test = ItemLambda.CheckingAnItemBesideItsHolder(itemBesidePeer, item, BrowserTerms.Value, peerAsBound, FormItIsHeldIn(), Term(comparison.Name), DatesJudgedBy);
            return new BrowserTest(test, Arguments, ["ComparisonValue"], false);
        }

        var memberAsBound = _scope.ReadingLeftOutMembersAsStarts(_member);
        var memberBesidePeer = PassesBeside<TPeer>(comparison, Unconverted(memberAsBound.Body), peerAsBound.Body);
        var read = ItemLambda.CheckingAMemberBesideItsPeer(memberBesidePeer, memberAsBound.Parameters[0], peerAsBound, _scope.Form, Term(comparison.Name), DatesJudgedBy);
        return new BrowserTest(new FnExpression([BrowserTerms.Value, read.Params[0]], read.Body), Arguments, ["ComparisonValue"], false);
    }

    internal BrowserTest Between(object? from, object? to, bool inclusive, Action<MessageArguments> arguments)
    {
        var value = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        var low = Binary(inclusive ? ExpressionType.GreaterThanOrEqual : ExpressionType.GreaterThan, value, LinqExpression.Constant(from, typeof(TValue)));
        var high = Binary(inclusive ? ExpressionType.LessThanOrEqual : ExpressionType.LessThan, value, LinqExpression.Constant(to, typeof(TValue)));
        var inside = LinqExpression.AndAlso(low, high);
        var passes = default(TValue) is null ? LinqExpression.OrElse(IsNull(value), inside) : inside;
        return Tests(LinqExpression.Lambda(passes, value), inclusive ? nameof(RuleBuilderExtensions.InclusiveBetween) : nameof(RuleBuilderExtensions.ExclusiveBetween), arguments);
    }

    internal BrowserTest InEnum(IEnumerable<object> values)
    {
        var value = LinqExpression.Parameter(typeof(TValue), BrowserTerms.Value);
        var named = values.Select(declared => (LinqExpression)LinqExpression.Equal(value, LinqExpression.Constant(declared, typeof(TValue)))).ToList();
        var declaredOne = named.Count == 0 ? LinqExpression.Constant(false) : named.Skip(1).Aggregate(named[0], LinqExpression.OrElse);
        var passes = default(TValue) is null ? LinqExpression.OrElse(IsNull(value), declaredOne) : declaredOne;
        var check = nameof(RuleBuilderExtensions.IsInEnum);
        var read = ItemLambda.Checking(LinqExpression.Lambda(passes, value), FormItsTestReads(check), Term(check), DatesJudgedBy);
        return new BrowserTest(new FnExpression([read.Params[0], BrowserTerms.Instance], read.Body), NoArguments, [], true);
    }

    internal BrowserTest InFlags(IEnumerable<Enum> flags)
    {
        var form = FormItsTestReads(nameof(RuleBuilderExtensions.IsInEnum));
        var rows = flags.Select(flag => new FlagValue(form.Beside(flag, typeof(TValue)).Value, flag.ToString(), decimal.ToInt64(Convert.ToDecimal(flag, CultureInfo.InvariantCulture))))
            .OrderByDescending(row => row.Number).ToArray();
        var value = new RefExpression(BrowserTerms.Value);
        var split = new OpExpression(Operator.InFlags, [value, new LitExpression(JsonSerializer.SerializeToElement(rows, PlanJson.Options))]);
        return new BrowserTest(new FnExpression([BrowserTerms.Value, BrowserTerms.Instance], BrowserTerms.NullOr(value, split)), NoArguments, [], true);
    }

    private string Message(string text, BrowserTest test)
    {
        var unknown = test.Unknown.Concat(_each ? ["CollectionIndex", "PropertyPath"] : _scope.Each ? ["PropertyPath"] : []).ToList();
        var named = unknown.FirstOrDefault(name => text.Contains("{" + name));
        if (named is not null)
            throw new InvalidOperationException($"The message '{text}' names {{{named}}}, which only the endpoint knows when the check runs, so the browser's copy cannot say it; write the message without it, or mark the rule .ServerOnly().");
        var dropped = test.FillsTheStart ? SentValues.Skip(1) : SentValues;
        var kept = Sentences(text).Where(sentence => !dropped.Any(name => sentence.Contains("{" + name))).ToList();
        if (kept.Count == 0)
            throw new InvalidOperationException($"Every sentence of the message '{text}' names a value the request sends, which the browser's copy leaves out, so no text is left for the browser to show; write a sentence that names none, or mark the rule .ServerOnly().");
        var start = test.FillsTheStart ? _each ? default : _start() : default;
        var arguments = new MessageArguments(_displayName, _scope.Prefix + _path, start);
        test.Arguments(arguments);
        return arguments.Fill(string.Join(" ", kept));
    }

    internal void CloseField(List<RuleGroup> segments, List<RuleTest> tests)
    {
        if (tests.Count == 0) return;
        _written += tests.Count;
        segments.Add(new RuleGroup([], ItemLambda.EveryItem(), [new RuleField(ChecksALeftOutMember ? [new MemberStep(_path)] : FieldPath(ItsMemberPath()), [.. tests])], []));
        tests.Clear();
    }

    internal RuleGroup Child(ReactiveValidator<TValue> reactive)
    {
        _written++;
        var members = ItsMemberPath();
        var path = FieldPath(members);
        var scope = _scope.Child([.. _scope.Path, .. path], _scope.Holder.Then(members), _scope.Prefix + _path + ".", _each || _scope.Each, ChildStart);
        var group = new RuleGroup(path, BrowserTerms.HoldsAValue, [], reactive.BrowserGroups(scope));
        if (!_stops || _earlier.Count == 0) return group;
        var instance = new RefExpression(BrowserTerms.Instance);
        var value = PlanSubstitution.Applied(ItemLambda.Of(_member, _scope.Form), instance);
        return new RuleGroup([], new FnExpression([BrowserTerms.Instance], new OpExpression(Operator.Not, [AnEarlierFailed(value, instance)])), [], [group]);
    }

    internal void RequireTheServersOrder(int dependents)
    {
        if (_written == 0) return;
        if (dependents > 0)
            throw new InvalidOperationException($"The rule of '{_path}' declares DependentRules, which the endpoint checks only while the rule passes, a condition no browser rule reads; write them under When with the rule's test as the condition, or mark the rule .ServerOnly().");
        if (_stops && _each)
            throw new InvalidOperationException($"The rule of '{_path}' stops at its first failing item (CascadeMode.Stop), and the browser checks every item, so it would show failures the endpoint never gives; mark the rule .ServerOnly().");
    }

    private bool ChecksALeftOutMember => !_each && _scope.ChecksALeftOutMember(_member);

    private FnExpression ReadingItsStartWhereLeftOut(FnExpression check)
        => ChecksALeftOutMember ? new FnExpression(check.Params, PlanSubstitution.Applied(check, _scope.Form.Beside(_start()), new RefExpression(check.Params[1]))) : check;

    private FnExpression PassingWhereAnEarlierFailed(FnExpression check)
    {
        if (_earlier.Count == 0) return check;
        var failed = AnEarlierFailed(new RefExpression(check.Params[0]), new RefExpression(check.Params[1]));
        return new FnExpression(check.Params, new OpExpression(Operator.Cond, [failed, Boolean(true), check.Body]));
    }

    private PlanExpression AnEarlierFailed(PlanExpression value, PlanExpression instance)
    {
        var failures = _earlier.Select(earlier => Failed(earlier.Check, earlier.When, value, instance)).ToList();
        return failures.Skip(1).Aggregate(failures[0], (any, next) => new OpExpression(Operator.Or, [any, next]));
    }

    private static PlanExpression Failed(FnExpression check, FnExpression when, PlanExpression value, PlanExpression instance)
    {
        var fails = new OpExpression(Operator.Cond, [PlanSubstitution.Applied(check, value, instance), Boolean(false), Boolean(true)]);
        return new OpExpression(Operator.Cond, [PlanSubstitution.Applied(when, instance), fails, Boolean(false)]);
    }

    private static LitExpression Boolean(bool held) => new(JsonSerializer.SerializeToElement(held));

    private static LitExpression Number(int held) => new(JsonSerializer.SerializeToElement(held));

    private BrowserTest TestsTheText(Operator check, string name)
        => TestsTheValue(value => BrowserTerms.NullOr(value, new OpExpression(check, [value])), name, NoArguments);

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

    private MemberPath ItsMemberPath()
    {
        if (!ReadsAsWritten(_member, _scope.Json.App, out var members))
            throw new InvalidOperationException($"The rule of '{Printed.Of(_member)}' checks no member of the model, and the browser's rule names the member it checks by its path; write the rule for a member, as RuleFor(x => x.Member) or RuleForEach(x => x.Items) does, or mark the rule .ServerOnly().");
        return members;
    }

    private IReadOnlyList<RegistrationStep> FieldPath(MemberPath members) => _each ? [.. members.BodyPath, EachRegistrationStep.Instance] : [.. members.BodyPath];

    private void RefuseAReadTheBrowserDoesNotHold(LambdaExpression read) => ReadsAsWritten(read, _scope.Form.DataOptions, out _);

    private bool ReadsAsWritten(LambdaExpression read, JsonSerializerOptions options, [NotNullWhen(true)] out MemberPath? members)
    {
        var body = Unconverted(read.Body);
        if (!MemberPath.TryOf(body, read.Parameters[0], read, options, out members)) return false;
        _scope.Holder.Then(members).RefuseAReadPastItsOwnConverter(Printed.Of(body));
        return true;
    }

    private HeldForm DatesJudgedBy => HeldForm.App(_scope.Json, _scope.Form.RendersUnder);

    private static BrowserTest Whole(Func<PlanExpression, PlanExpression> passes, Action<MessageArguments> arguments)
        => new(new FnExpression([BrowserTerms.Value, BrowserTerms.Instance], passes(new RefExpression(BrowserTerms.Value))), arguments, [], false);

    private string Term(string check)
        => (_each ? nameof(ReactiveValidator<T>.RuleForEach) : nameof(ReactiveValidator<T>.RuleFor)) + "(" + Printed.Of(_member) + ")." + check;

    private HeldForm FormItIsHeldIn()
    {
        if (ChecksALeftOutMember) return _scope.Form;
        var read = Unconverted(_member.Body);
        if (!MemberPath.TryOf(read, _member.Parameters[0], _member, _scope.Form.DataOptions, out var members) || !members.Member.HoldsItsOwnForm) return _scope.Form;
        return _scope.Form.WrittenBy(members.Member, Printed.Of(read));
    }

    private HeldForm FormItsTestReads(string check)
    {
        var form = FormItIsHeldIn();
        if (_each && form.Own is { Member.HasItsOwnConverter: true } listsOwn) throw ItemLambda.WrittenUnlikeItsBase(Term(check), listsOwn.Member, form);
        return form;
    }

    private LinqExpression PassesBeside<TPeer>(Comparison comparison, LinqExpression left, LinqExpression right)
    {
        if (Nullable.GetUnderlyingType(left.Type) == right.Type) right = LinqExpression.Convert(right, left.Type);
        if (Nullable.GetUnderlyingType(right.Type) == left.Type) left = LinqExpression.Convert(left, right.Type);
        var test = Binary(comparison.Node, left, right);
        var otherHeld = default(TPeer) is null ? LinqExpression.AndAlso(LinqExpression.Not(IsNull(right)), test) : test;
        return !comparison.Orders ? test : default(TValue) is null ? LinqExpression.OrElse(IsNull(left), otherHeld) : otherHeld;
    }

    private LinqExpression Binary(ExpressionType node, LinqExpression left, LinqExpression right)
    {
        try
        {
            return LinqExpression.MakeBinary(node, left, right);
        }
        catch (InvalidOperationException)
        {
            if (typeof(TValue) == typeof(string)) throw OrdersText();
            throw new InvalidOperationException($"The rule of '{_path}' compares a value by its type's own comparer, which the browser does not have; mark the rule .ServerOnly().");
        }
    }

    private InvalidOperationException OrdersText()
        => new($"The rule of '{_path}' orders text, which .NET orders by the culture's comparer and the browser cannot order as .NET does; mark the rule .ServerOnly().");

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
}
