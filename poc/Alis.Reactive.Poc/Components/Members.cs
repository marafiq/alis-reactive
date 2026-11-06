using System;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

public class ReadOnly<TValue> : TypedSource<TValue>
{
    internal ReadOnly(JsObject owner, PathStep member)
    {
        Owner = owner;
        Member = member;
    }

    internal JsObject Owner { get; }

    internal PathStep Member { get; }

    internal override Expression Expression => new GetExpression(Owner.Binding.Target, [Member], Owner.Binding.Members);

    internal override PlanBuild Build => Owner.Binding.Build;

    public TypedSource<TResult> Read<TResult>(System.Linq.Expressions.Expression<Func<TValue, TResult>> path)
        => new DataRead<TResult>(new GetExpression(Expression, JsonPath.Of(path, Build.Values), ReadPresence.Optional), Build);
}

public sealed class Property<TValue> : ReadOnly<TValue>
{
    internal Property(JsObject owner, PathStep member)
        : base(owner, member)
    {
    }

    public void Set(TValue value) => Assign.Write(Owner, Member, Literal.Of(value, Owner.Binding.Build));

    public void Set(TypedSource<TValue> source) => Assign.Write(Owner, Member, source.Expression);
}

public sealed class WriteOnly<TValue>
{
    private readonly JsObject _owner;
    private readonly PathStep _member;

    internal WriteOnly(JsObject owner, PathStep member)
    {
        _owner = owner;
        _member = member;
    }

    public void Set(TValue value) => Assign.Write(_owner, _member, Literal.Of(value, _owner.Binding.Build));

    public void Set(TypedSource<TValue> source) => Assign.Write(_owner, _member, source.Expression);
}

internal static class Assign
{
    internal static void Write(JsObject owner, PathStep member, Expression value)
    {
        var binding = owner.Binding;
        binding.Build.Write(new SetStatement(binding.Target, [member], Presence.Required, value));
    }
}

public class Event<TArgs>
    where TArgs : JsObject, new()
{
    internal Event(string name)
    {
        Name = name;
    }

    internal string Name { get; }
}

public sealed class Event : Event<NoPayload>
{
    internal Event(string name)
        : base(name)
    {
    }
}

public sealed class NoPayload : JsObject
{
}
