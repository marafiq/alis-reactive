using System;

namespace Alis.Reactive.ValuesAndSources;

/// <summary>
/// One argument of a call the browser makes: a literal or a source.
/// </summary>
/// <remarks>
/// A declaration that takes its arguments as one list takes them as this type; each is written as an
/// <see cref="Arg{T}"/>, so it stays typed.
/// </remarks>
public abstract class Arg
{
    private protected Arg()
    {
    }
}

/// <summary>
/// One argument of a call the browser makes, typed as the parameter it fills.
/// </summary>
/// <remarks>
/// C# converts a literal of <typeparamref name="T"/> and a <see cref="TypedSource{T}"/> to it, so a call is written
/// with the values themselves. A source of the type's nullable form is passed with <c>OrNull()</c>.
/// </remarks>
/// <typeparam name="T">The type of the parameter the argument fills.</typeparam>
public sealed class Arg<T> : Arg
{
    private Arg()
    {
    }

    /// <summary>Passes a literal, written into the plan where the view renders.</summary>
    /// <param name="value">The value the call receives.</param>
    public static implicit operator Arg<T>(T value) => throw new NotImplementedException();

    /// <summary>Passes what a source reads when the call runs.</summary>
    /// <param name="source">The source the call's argument is read from.</param>
    public static implicit operator Arg<T>(TypedSource<T> source) => throw new NotImplementedException();
}
