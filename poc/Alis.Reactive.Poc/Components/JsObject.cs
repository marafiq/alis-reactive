using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

internal sealed class Binding
{
    internal Binding(Expression target, PlanBuild build, ReadPresence members)
    {
        Target = target;
        Build = build;
        Members = members;
    }

    internal Expression Target { get; }

    internal PlanBuild Build { get; }

    internal ReadPresence Members { get; }
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

    protected ReadOnly<TValue> ReadOnly<TValue>(string? name = null, [CallerMemberName] string declaredBy = "")
        => new(this, Member(name, declaredBy));

    protected Property<TValue> Property<TValue>(string? name = null, [CallerMemberName] string declaredBy = "")
        => new(this, Member(name, declaredBy));

    protected WriteOnly<TValue> WriteOnly<TValue>(string? name = null, [CallerMemberName] string declaredBy = "")
        => new(this, Member(name, declaredBy));

    protected TObject LiveObject<TObject>(string? name = null, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => Bound<TObject>(new Binding(new GetExpression(Binding.Target, [Member(name, declaredBy)], Binding.Members), Binding.Build, Binding.Members));

    protected LiveObjects<TItem> LiveObjects<TItem>(string? name = null, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => new(new GetExpression(Binding.Target, [Member(name, declaredBy)], Binding.Members), Binding.Build, Binding.Members);

    protected void Method(string? name = null, [CallerMemberName] string declaredBy = "")
        => Method([], name, declaredBy);

    protected void Method(IReadOnlyList<Arg> arguments, string? name = null, [CallerMemberName] string declaredBy = "")
        => Binding.Build.Write(new DoStatement(Call(arguments, name, declaredBy)));

    protected MethodResult<TResult> Method<TResult>(string? name = null, [CallerMemberName] string declaredBy = "")
        => Method<TResult>([], name, declaredBy);

    protected MethodResult<TResult> Method<TResult>(IReadOnlyList<Arg> arguments, string? name = null, [CallerMemberName] string declaredBy = "")
        => new MethodResult<TResult>(Binding.Build.Result(Call(arguments, name, declaredBy)), Binding.Build);

    protected TObject ReturnedObject<TObject>(string? name = null, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => ReturnedObject<TObject>([], name, declaredBy);

    protected TObject ReturnedObject<TObject>(IReadOnlyList<Arg> arguments, string? name = null, [CallerMemberName] string declaredBy = "")
        where TObject : JsObject, new()
        => Bound<TObject>(new Binding(Binding.Build.Result(Call(arguments, name, declaredBy)), Binding.Build, ReadPresence.Required));

    protected LiveObjects<TItem> ReturnedObjects<TItem>(string? name = null, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => ReturnedObjects<TItem>([], name, declaredBy);

    protected LiveObjects<TItem> ReturnedObjects<TItem>(IReadOnlyList<Arg> arguments, string? name = null, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => new(Binding.Build.Result(Call(arguments, name, declaredBy)), Binding.Build, ReadPresence.Required);

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

    private CallExpression Call(IReadOnlyList<Arg> arguments, string? name, string declaredBy)
        => new(Binding.Target, [Member(name, declaredBy)], Binding.Members, [.. arguments.Select(argument => argument.ExpressionIn(Binding.Build))]);

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
