using System;
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
    private protected ReadOnly()
    {
    }

    /// <summary>Reads data past the property: a field of the value it holds, at any depth.</summary>
    /// <remarks>
    /// A field the value lacks reads as a new instance of its holder's type would hold it, and a field past an
    /// object that is null cannot be read.
    /// </remarks>
    /// <param name="path">The field to read, as a member path from the property's value.</param>
    /// <returns>A source of the field's value.</returns>
    public TypedSource<TResult> Read<TResult>(Expression<Func<TValue, TResult>> path) => throw new NotImplementedException();
}

/// <summary>
/// A declared property the browser reads and sets.
/// </summary>
/// <typeparam name="TValue">The type the object's API gives the property, nullable where the API's is.</typeparam>
public sealed class Property<TValue> : ReadOnly<TValue>
{
    private readonly JsObject _owner;
    private readonly PathStep _member;
    private readonly bool _content;

    internal Property(JsObject owner, string member, bool content)
    {
        _owner = owner;
        _member = new MemberStep(member);
        _content = content;
    }

    /// <summary>Sets the property to a literal when the step runs.</summary>
    /// <param name="value">The value to set, written into the plan where the view renders.</param>
    public void Set(TValue value)
        => _owner.Bound.Writer.Write(new SetStatement([_member], SetMembers.Required, _content, _owner.Bound.Target, Literals.Of(value)));

    /// <summary>Sets the property to what a source reads when the step runs.</summary>
    /// <param name="source">The source of the value to set.</param>
    public void Set(ITypedSource<TValue> source) => throw new NotImplementedException();
}

/// <summary>
/// A declared property the browser only sets.
/// </summary>
/// <typeparam name="TValue">The type the object's API gives the property, nullable where the API's is.</typeparam>
public sealed class WriteOnly<TValue>
{
    internal WriteOnly()
    {
    }

    /// <summary>Sets the property to a literal when the step runs.</summary>
    /// <param name="value">The value to set, written into the plan where the view renders.</param>
    public void Set(TValue value) => throw new NotImplementedException();

    /// <summary>Sets the property to what a source reads when the step runs.</summary>
    /// <param name="source">The source of the value to set.</param>
    public void Set(ITypedSource<TValue> source) => throw new NotImplementedException();
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
        => throw new NotImplementedException();

    /// <summary>Sets a write-only property of a nullable value type to what a source of the type itself reads.</summary>
    /// <param name="member">The property to set.</param>
    /// <param name="source">The source of the value to set.</param>
    public static void Set<TValue>(this WriteOnly<TValue?> member, ITypedSource<TValue> source)
        where TValue : struct
        => throw new NotImplementedException();
}

/// <summary>
/// A declared property whose value is a function the object calls for an answer.
/// </summary>
/// <typeparam name="TArgs">The declaration of the first argument the object calls the function with.</typeparam>
/// <typeparam name="TResult">The type of the answer the object expects.</typeparam>
public sealed class Callback<TArgs, TResult>
    where TArgs : JsObject, new()
{
    internal Callback()
    {
    }

    /// <summary>Sets the function to one that answers each call with an expression over the call's arguments.</summary>
    /// <remarks>The answer holds no step and waits for nothing; it is read again on every call.</remarks>
    /// <param name="answer">The answer, read from the arguments of each call.</param>
    public void Set(Func<TArgs, TypedSource<TResult>> answer) => throw new NotImplementedException();
}

/// <summary>
/// A declared event the object raises with a payload.
/// </summary>
/// <typeparam name="TArgs">The declaration of what the event's listener receives.</typeparam>
public class Event<TArgs>
    where TArgs : JsObject, new()
{
    private protected Event()
    {
    }
}

/// <summary>
/// A declared event the object raises with no payload.
/// </summary>
public sealed class Event : Event<NoPayload>
{
    internal Event()
    {
    }
}

/// <summary>
/// What the listener of an event with no payload receives: nothing a handler reads.
/// </summary>
public sealed class NoPayload : JsObject;
