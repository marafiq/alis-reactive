using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// A declared property the browser reads: a source of its value wherever a value goes.
/// </summary>
/// <typeparam name="TValue">The type the object's API gives the property, nullable where the API's is.</typeparam>
public class ReadOnly<TValue> : TypedSource<TValue>
{
    private readonly bool _fromArguments;

    internal ReadOnly(JsObject owner, string member, bool optional)
        : this(owner, new MemberStep(member), optional, false)
    {
    }

    internal ReadOnly(JsObject owner, PathStep member, bool optional, bool fromArguments)
    {
        Owner = owner;
        Member = member;
        Optional = optional;
        _fromArguments = fromArguments;
    }

    private protected JsObject Owner { get; }

    private protected PathStep Member { get; }

    private protected bool Optional { get; }

    internal override Schema.Expression Expression
        => new GetExpression([Member], _fromArguments ? PathMembers.Required : Owner.Bound.Reads(Optional), _fromArguments ? Owner.Bound.Arguments : Owner.Bound.Target);

    internal override HeldForm Form => HeldForm.Live(Owner.Bound.Writer.Content.Json, Owner.Bound.Writer.Content.RendersUnder);

    /// <summary>Reads data past the property: a field of the value it holds, at any depth.</summary>
    /// <remarks>
    /// A field the value lacks reads as a new instance of its holder's type would hold it, and a field past an
    /// object that is null cannot be read.
    /// </remarks>
    /// <param name="path">The field to read, as a member path from the property's value.</param>
    /// <returns>A source of the field's value.</returns>
    public TypedSource<TResult> Read<TResult>(Expression<Func<TValue, TResult>> path)
        => DataReads.Past<TResult>(Expression, Form, path, Owner.Bound.Writer.Content.Json.LiveForm);
}

/// <summary>
/// A declared property the browser reads and sets.
/// </summary>
/// <typeparam name="TValue">The type the object's API gives the property, nullable where the API's is.</typeparam>
public sealed class Property<TValue> : ReadOnly<TValue>
{
    internal Property(JsObject owner, string member, bool optional)
        : base(owner, member, optional)
    {
    }

    /// <summary>Sets the property to a literal when the step runs.</summary>
    /// <param name="value">The value to set, written into the plan where the view renders.</param>
    public void Set(TValue value) => Written(Literals.IntoLive(value, Owner.Bound.Writer.Content.Json));

    /// <summary>Sets the property to what a source reads when the step runs.</summary>
    /// <param name="source">The source of the value to set.</param>
    public void Set(ITypedSource<TValue> source) => Written(Sources.FormOf(source).IntoLive<TValue>(Sources.Of(source)));

    internal void Written(Schema.Expression value)
        => Owner.Bound.Writer.Write(new SetStatement([Member], Optional ? SetMembers.Optional : SetMembers.Required, false, Owner.Bound.Target, value));
}

/// <summary>
/// A declared property the browser only sets.
/// </summary>
/// <typeparam name="TValue">The type the object's API gives the property, nullable where the API's is.</typeparam>
public sealed class WriteOnly<TValue>
{
    private readonly JsObject _owner;
    private readonly PathStep _member;
    private readonly bool _optional;

    internal WriteOnly(JsObject owner, string member, bool optional)
    {
        _owner = owner;
        _member = new MemberStep(member);
        _optional = optional;
    }

    /// <summary>Sets the property to a literal when the step runs.</summary>
    /// <param name="value">The value to set, written into the plan where the view renders.</param>
    public void Set(TValue value) => Written(Literals.IntoLive(value, _owner.Bound.Writer.Content.Json));

    /// <summary>Sets the property to what a source reads when the step runs.</summary>
    /// <param name="source">The source of the value to set.</param>
    public void Set(ITypedSource<TValue> source) => Written(Sources.FormOf(source).IntoLive<TValue>(Sources.Of(source)));

    internal void Written(Schema.Expression value)
        => _owner.Bound.Writer.Write(new SetStatement([_member], _optional ? SetMembers.Optional : SetMembers.Required, false, _owner.Bound.Target, value));
}

/// <summary>
/// Sets a property of a nullable value type from a source of the type itself, as a C# assignment widens it.
/// </summary>
public static class NullableWrites
{
    /// <summary>Sets a property of a nullable value type to what a source of the type itself reads.</summary>
    /// <param name="property">The property to set.</param>
    /// <param name="source">The source of the value to set.</param>
    public static void Set<TValue>(this Property<TValue?> property, ITypedSource<TValue> source)
        where TValue : struct
        => property.Written(Sources.FormOf(source).IntoLive<TValue>(Sources.Of(source)));

    /// <summary>Sets a write-only property of a nullable value type to what a source of the type itself reads.</summary>
    /// <param name="member">The property to set.</param>
    /// <param name="source">The source of the value to set.</param>
    public static void Set<TValue>(this WriteOnly<TValue?> member, ITypedSource<TValue> source)
        where TValue : struct
        => member.Written(Sources.FormOf(source).IntoLive<TValue>(Sources.Of(source)));
}

/// <summary>
/// A declared property whose value is a function the object calls for an answer.
/// </summary>
/// <typeparam name="TArgs">The declaration of the first argument the object calls the function with.</typeparam>
/// <typeparam name="TResult">The type of the answer the object expects.</typeparam>
public sealed class Callback<TArgs, TResult>
    where TArgs : JsObject, new()
{
    private readonly JsObject _owner;
    private readonly PathStep _member;
    private readonly bool _optional;

    internal Callback(JsObject owner, string member, bool optional)
    {
        _owner = owner;
        _member = new MemberStep(member);
        _optional = optional;
    }

    /// <summary>Sets the function to one that answers each call with an expression over the call's arguments.</summary>
    /// <remarks>The answer holds no step and waits for nothing; it is read again on every call.</remarks>
    /// <param name="answer">The answer, read from the arguments of each call.</param>
    public void Set(Func<TArgs, TypedSource<TResult>> answer)
    {
        var writer = _owner.Bound.Writer;
        var value = JsObject.WrittenAnswer(writer, answer);
        writer.Write(new SetStatement([_member], _optional ? SetMembers.Optional : SetMembers.Required, false, _owner.Bound.Target, value));
    }
}

/// <summary>
/// A declared event the object raises with a payload.
/// </summary>
/// <typeparam name="TArgs">The declaration of what the event's listener receives.</typeparam>
public class Event<TArgs>
    where TArgs : JsObject, new()
{
    internal Event(JsObject owner, string name, IReadOnlyList<PathStep> payload)
    {
        Owner = owner;
        Name = name;
        Payload = payload;
    }

    internal JsObject Owner { get; }

    internal string Name { get; }

    internal IReadOnlyList<PathStep> Payload { get; }
}

/// <summary>
/// A declared event the object raises with no payload.
/// </summary>
public sealed class Event : Event<NoPayload>
{
    internal Event(JsObject owner, string name)
        : base(owner, name, [])
    {
    }
}

/// <summary>
/// What the listener of an event with no payload receives: nothing a handler reads.
/// </summary>
public sealed class NoPayload : JsObject;
