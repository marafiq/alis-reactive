using System;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Components;

public class ReadOnly<TValue> : TypedSource<TValue>
{
    internal ReadOnly(JsObject owner, PathStep member, bool optional)
    {
        Owner = owner;
        Member = member;
        Optional = optional;
    }

    internal JsObject Owner { get; }

    internal PathStep Member { get; }

    internal bool Optional { get; }

    internal override Expression Expression => new GetExpression(Owner.Binding.Target, [Member], Owner.Binding.Read(Optional));

    internal override PlanBuild Build => Owner.Binding.Build;

    public TypedSource<TResult> Read<TResult>(System.Linq.Expressions.Expression<Func<TValue, TResult>> path)
    {
        var read = JsonPath.Of(path, Form, Build.Starts, nameof(path));
        return new DataRead<TResult>(new DataExpression(Expression, read.Steps), Build, read.Form);
    }
}

public sealed class Property<TValue> : ReadOnly<TValue>
{
    private readonly Writes _writes;

    internal Property(JsObject owner, PathStep member, bool optional, Writes writes)
        : base(owner, member, optional)
    {
        _writes = writes;
    }

    public void Set(TValue value) => Assign.Write(Owner, Member, Optional, Owner.Binding.Build.IntoLive<TValue>(Literal.Of(value, Owner.Binding.Build), Owner.Binding.Build.OptionsForm), _writes);

    public void Set(ITypedSource<TValue> source) => Assign.Write(Owner, Member, Optional, Assign.Live<TValue>(Owner.Binding.Build, Sources.Of(source)), _writes);

    internal void Widen<TSource>(ITypedSource<TSource> source) => Assign.Write(Owner, Member, Optional, Assign.Live<TValue>(Owner.Binding.Build, Sources.Of(source)), _writes);
}

internal sealed class Writes
{
    private Writes(bool content, string noun)
    {
        Content = content;
        Noun = noun;
    }

    internal static Writes Member { get; } = new(content: false, "member");

    internal bool Content { get; }

    internal string Noun { get; }

    internal static Writes ContentAs(string noun) => new(content: true, noun);
}

public sealed class WriteOnly<TValue>
{
    private readonly JsObject _owner;
    private readonly PathStep _member;
    private readonly bool _optional;

    internal WriteOnly(JsObject owner, PathStep member, bool optional)
    {
        _owner = owner;
        _member = member;
        _optional = optional;
    }

    public void Set(TValue value) => Assign.Write(_owner, _member, _optional, _owner.Binding.Build.IntoLive<TValue>(Literal.Of(value, _owner.Binding.Build), _owner.Binding.Build.OptionsForm));

    public void Set(ITypedSource<TValue> source) => Assign.Write(_owner, _member, _optional, Assign.Live<TValue>(_owner.Binding.Build, Sources.Of(source)));

    internal void Widen<TSource>(ITypedSource<TSource> source) => Assign.Write(_owner, _member, _optional, Assign.Live<TValue>(_owner.Binding.Build, Sources.Of(source)));
}

public static class NullableWrites
{
    public static void Set<TValue>(this Property<TValue?> property, ITypedSource<TValue> source)
        where TValue : struct
        => property.Widen(source);

    public static void Set<TValue>(this WriteOnly<TValue?> member, ITypedSource<TValue> source)
        where TValue : struct
        => member.Widen(source);
}

internal static class Assign
{
    private static void RequireTheVendorsTurn(PlanBuild build, string payload)
    {
        if (!build.IsOwnPayload(payload))
        {
            throw new InvalidOperationException(
                "A write to the payload of the event around a Listen reaches no vendor: the vendor read it when its listener returned, before the listened object raised. Write it in the event's own pipeline, outside the Listen.");
        }

        if (build.HandlerAwaited)
        {
            throw new InvalidOperationException(
                "A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.");
        }
    }

    internal static Expression Live<TValue>(PlanBuild build, IPlanSource source) => build.IntoLive<TValue>(source.Expression, source.Form);

    internal static void Write(JsObject owner, PathStep member, bool optional, Expression value)
        => Write(owner, member, optional, value, Writes.Member);

    internal static void Write(JsObject owner, PathStep member, bool optional, Expression value, Writes writes)
    {
        var binding = owner.Binding;
        var target = binding.Target;
        if (target is RefExpression reference && binding.Build.IsPayload(reference.Name)) RequireTheVendorsTurn(binding.Build, reference.Name);
        if (writes.Content && target is ObjectExpression element) binding.Build.ReplacesContentOf(element.Id, writes.Noun);
        binding.Build.Write(new SetStatement(target, [member], optional ? Presence.Optional : Presence.Required, value, writes.Content));
    }
}

public sealed class Callback<TArgs, TResult>
    where TArgs : JsObject, new()
{
    private readonly JsObject _owner;
    private readonly PathStep _member;
    private readonly bool _optional;

    internal Callback(JsObject owner, PathStep member, bool optional)
    {
        _owner = owner;
        _member = member;
        _optional = optional;
    }

    public void Set(Func<TArgs, TypedSource<TResult>> answer) => Assign.Write(_owner, _member, _optional, Answers.Of(answer, _owner.Binding.Build));
}

internal static class Answers
{
    internal static AnswerExpression Of<TArgs, TResult>(Func<TArgs, TypedSource<TResult>> answer, PlanBuild build)
        where TArgs : JsObject, new()
    {
        var list = new RefExpression(build.AnswerArgumentsName());
        var first = new GetExpression(list, [PathStep.Index(0)], ReadPresence.Required);
        var args = JsObject.Bound<TArgs>(new Binding(first, build, ReadPresence.Required, new ArgumentsAt(list)));
        var body = build.Answering(() => answer(args))
            ?? throw new ArgumentException("A callback's answer reads its arguments, such as tick => tick.Label.", nameof(answer));
        PlanBuild.RequireTheOptionsForm(body.Form);
        return new AnswerExpression(list.Name, body.Expression);
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
