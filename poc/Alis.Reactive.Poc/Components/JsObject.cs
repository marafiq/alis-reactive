using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

internal abstract class Arguments
{
    private protected Arguments()
    {
    }
}

internal sealed class NoArguments : Arguments
{
    private NoArguments()
    {
    }

    internal static NoArguments Instance { get; } = new();
}

internal sealed class ArgumentsAt : Arguments
{
    internal ArgumentsAt(Expression list)
    {
        List = list;
    }

    internal Expression List { get; }
}

internal sealed class Binding
{
    private readonly Expression _target;

    internal Binding(Expression target, PlanBuild build, ReadPresence members)
        : this(target, build, members, NoArguments.Instance)
    {
    }

    internal Binding(Expression target, PlanBuild build, ReadPresence members, Arguments arguments)
    {
        _target = target;
        Build = build;
        Members = members;
        Arguments = arguments;
    }

    internal Expression Target => Build.InScope(_target);

    internal PlanBuild Build { get; }

    internal ReadPresence Members { get; }

    internal Arguments Arguments { get; }

    internal ReadPresence Read(bool optional) => optional ? Members.MemberMayBeAbsent() : Members;

    internal ReadPresence Below(bool optional) => optional ? Members.HolderMayHoldNothing() : Members;
}

internal sealed class ListenerArgumentsOf : JsObject
{
}

public abstract class JsObject
{
    private Binding? _binding;

    internal Binding Binding => _binding ?? throw new InvalidOperationException(
        $"'{GetType().FullName}' is a declaration, not a live object. A pipeline reaches one: p.Component<T>(id), or the payload an event hands its pipeline.");

    internal void Bind(Binding binding) => _binding = binding;

    internal static TObject Bound<TObject>(Binding binding)
        where TObject : JsObject, new()
    {
        var bound = new TObject();
        bound.Bind(binding);
        return bound;
    }

    protected ReadOnly<TValue> ReadOnly<TValue>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => new(this, Member(name, declaredBy), optional);

    protected Property<TValue> Property<TValue>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => new(this, Member(name, declaredBy), optional);

    protected WriteOnly<TValue> WriteOnly<TValue>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => new(this, Member(name, declaredBy), optional);

    protected TObject LiveObject<TObject>(string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => Bound<TObject>(new Binding(new GetExpression(Binding.Target, [Member(name, declaredBy)], Binding.Read(optional)), Binding.Build, Binding.Below(optional || nullable)));

    protected LiveObjects<TItem> LiveObjects<TItem>(string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => new(new GetExpression(Binding.Target, [Member(name, declaredBy)], Binding.Read(optional)), Binding.Build, Binding.Below(optional || nullable));

    protected void Method(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => Method([], name, optional, declaredBy);

    protected void Method(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => Binding.Build.Write(new DoStatement(Call(arguments, name, optional, declaredBy), awaits: false));

    protected MethodResult<TResult> Method<TResult>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => Method<TResult>([], name, optional, declaredBy);

    protected MethodResult<TResult> Method<TResult>(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => new MethodResult<TResult>(Binding.Build.Result(Call(arguments, name, optional, declaredBy)), Binding.Build);

    protected void MethodAsync(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => MethodAsync([], name, optional, declaredBy);

    protected void MethodAsync(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => Binding.Build.Write(new DoStatement(Call(arguments, name, optional, declaredBy), awaits: true));

    protected MethodResult<TResult> MethodAsync<TResult>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => MethodAsync<TResult>([], name, optional, declaredBy);

    protected MethodResult<TResult> MethodAsync<TResult>(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        => new(Binding.Build.AwaitedResult(Call(arguments, name, optional, declaredBy)), Binding.Build);

    protected TObject ReturnedObject<TObject>(string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => ReturnedObject<TObject>([], name, optional, nullable, declaredBy);

    protected TObject ReturnedObject<TObject>(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => Bound<TObject>(new Binding(Binding.Build.Result(Call(arguments, name, optional, declaredBy)), Binding.Build, Returned(optional || nullable)));

    protected LiveObjects<TItem> ReturnedObjects<TItem>(string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => ReturnedObjects<TItem>([], name, optional, nullable, declaredBy);

    protected LiveObjects<TItem> ReturnedObjects<TItem>(IReadOnlyList<Arg> arguments, string? name = null, bool optional = false, bool nullable = false, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => new(Binding.Build.Result(Call(arguments, name, optional, declaredBy)), Binding.Build, Returned(optional || nullable));

    protected void CallItself()
        => CallItself([]);

    protected void CallItself(IReadOnlyList<Arg> arguments)
        => Binding.Build.Write(new DoStatement(Itself(arguments), awaits: false));

    protected MethodResult<TResult> CallItself<TResult>(IReadOnlyList<Arg> arguments)
        => new(Binding.Build.Result(Itself(arguments)), Binding.Build);

    protected ReadOnly<TValue> Argument<TValue>(int position)
        => new(ListenerArguments(), ArgumentStep(position), optional: false);

    protected void MethodAt(int position)
        => MethodAt(position, []);

    protected void MethodAt(int position, IReadOnlyList<Arg> arguments)
    {
        var listener = ListenerArguments().Binding;
        var call = new CallExpression(listener.Target, [ArgumentStep(position)], listener.Members, [.. arguments.Select(argument => argument.ExpressionIn(Binding.Build))]);
        Binding.Build.Write(new DoStatement(call, awaits: false));
    }

    protected Callback<TArgs, TResult> Callback<TArgs, TResult>(string? name = null, bool optional = false, [CallerMemberName] string declaredBy = "")
        where TArgs : JsObject, new()
        => new(this, Member(name, declaredBy), optional);

    protected static Arg Answer<TArgs, TResult>(Func<TArgs, TypedSource<TResult>> answer)
        where TArgs : JsObject, new()
        => new WrittenArg(build => Answers.Of(answer, build));

    protected static Arg Fixed<TValue>(TValue value)
        where TValue : notnull
        => (Arg<TValue>)value;

    protected static Arg Pass<TDeclared, TSource>(TypedSource<TSource> value)
        where TSource : TDeclared
        => (Arg<TSource>)value;

    protected Event Event(string? name = null, [CallerMemberName] string declaredBy = "")
        => new(JavaScriptName(name, declaredBy));

    protected Event<TArgs> Event<TArgs>(string? name = null, [CallerMemberName] string declaredBy = "")
        where TArgs : JsObject, new()
        => new(JavaScriptName(name, declaredBy));

    internal void MethodWhenHeld(Arg argument, string name)
    {
        var build = Binding.Build;
        var held = build.Bind(argument.ExpressionIn(build));
        var call = new CallExpression(Binding.Target, [PathStep.Member(name)], Binding.Members, [held]);
        var holdsAValue = new OperatorExpression(Operator.Neq, [held, Literal.Of<string?>(null, build)]);
        build.When(holdsAValue, () => build.Write(new DoStatement(call, awaits: false)));
    }

    private ListenerArgumentsOf ListenerArguments() => Binding.Arguments switch
    {
        ArgumentsAt arguments => Bound<ListenerArgumentsOf>(new Binding(arguments.List, Binding.Build, ReadPresence.Required)),
        _ => throw new InvalidOperationException(
            "An argument at a position is what a listener or a callback receives: declare it on an event's payload or a callback's arguments."),
    };

    private static PathStep ArgumentStep(int position)
    {
        if (position < 0) throw new ArgumentOutOfRangeException(nameof(position), position, "An argument's position is zero or more.");
        return PathStep.Index(position);
    }

    private CallExpression Itself(IReadOnlyList<Arg> arguments)
        => new(Binding.Target, [], Binding.Members, [.. arguments.Select(argument => argument.ExpressionIn(Binding.Build))]);

    private static ReadPresence Returned(bool mayHoldNothing) => mayHoldNothing ? ReadPresence.Required.HolderMayHoldNothing() : ReadPresence.Required;

    private CallExpression Call(IReadOnlyList<Arg> arguments, string? name, bool optional, string declaredBy)
        => new(Binding.Target, [Member(name, declaredBy)], Binding.Read(optional), [.. arguments.Select(argument => argument.ExpressionIn(Binding.Build))]);

    private static PathStep Member(string? name, string declaredBy) => PathStep.Member(JavaScriptName(name, declaredBy));

    private static string JavaScriptName(string? name, string declaredBy)
    {
        var chosen = name ?? JsonNamingPolicy.CamelCase.ConvertName(declaredBy);
        var reachesIntoAnObject = chosen.IndexOfAny(['.', '[']) >= 0;
        if (chosen.Length == 0 || reachesIntoAnObject)
        {
            throw new ArgumentException(
                $"'{declaredBy}' is named '{chosen}'. A member's name is one JavaScript name; declare an object a member holds as its own declaration.",
                nameof(name));
        }

        return chosen;
    }
}
