using System;

namespace Alis.Reactive.ValuesAndSources;

/// <summary>
/// A value the browser reads when a step runs, read as <typeparamref name="T"/> or as a type it converts to.
/// </summary>
/// <remarks>
/// A set, an element's text and a class name take their value through this interface, so a source of a narrower
/// type fits where a wider one is asked for. Only the framework makes a source.
/// </remarks>
/// <typeparam name="T">The type of the value the source reads.</typeparam>
public interface ITypedSource<out T>;

/// <summary>
/// A value the browser reads when a step runs: a declared member, a method's result, a field of an event's payload
/// or of a response, a member of the page's URL, the result of a list's operation, or a value chosen from others.
/// </summary>
/// <remarks>
/// Every place a step takes a value takes a literal of its type or a source of it, so what one step reads another
/// can set, send, pass or compare. A view gets a source from the door that reads the value and never makes one.
/// </remarks>
/// <typeparam name="T">The type of the value the source reads.</typeparam>
public abstract class TypedSource<T> : ITypedSource<T>
{
    private protected TypedSource()
    {
    }

    /// <summary>Joins what the source reads when the step runs, shown as an element's text shows it.</summary>
    /// <param name="source">The source of the part; one that cannot be read or holds null adds nothing.</param>
    public static implicit operator TextPart(TypedSource<T> source) => throw new NotImplementedException();
}
