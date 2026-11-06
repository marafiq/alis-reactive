using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

internal sealed class Binding
{
    internal Binding(Expression target, PlanBuild build)
    {
        Target = target;
        Build = build;
    }

    internal Expression Target { get; }

    internal PlanBuild Build { get; }
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
        => Bound<TObject>(new Binding(new GetExpression(Binding.Target, [Member(name, declaredBy)], ReadPresence.Required), Binding.Build));

    protected LiveObjects<TItem> LiveObjects<TItem>(string? name = null, [CallerMemberName] string declaredBy = "")
        where TItem : JsObject, new()
        => new(new GetExpression(Binding.Target, [Member(name, declaredBy)], ReadPresence.Required), Binding.Build);

    protected Event Event(string? name = null, [CallerMemberName] string declaredBy = "")
        => new(JavaScriptName(name, declaredBy));

    protected Event<TArgs> Event<TArgs>(string? name = null, [CallerMemberName] string declaredBy = "")
        where TArgs : JsObject, new()
        => new(JavaScriptName(name, declaredBy));

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
