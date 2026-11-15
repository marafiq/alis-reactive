using System;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal interface IArgumentValue
{
    Expression Written(AppJson json);
}

/// <summary>
/// One argument of a call the browser makes: a literal or a source.
/// </summary>
/// <remarks>
/// A declaration that takes its arguments as one list takes them as this type; each is written as an
/// <see cref="Arg{T}"/>, so it stays typed.
/// </remarks>
public abstract class Arg
{
    private readonly Func<AppJson, Expression> _write;

    private protected Arg(Func<AppJson, Expression> write)
    {
        _write = write;
    }

    internal Expression Written(AppJson json) => _write(json);
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
    private Arg(Func<AppJson, Expression> write) : base(write)
    {
    }

    internal static Arg<T> FromSource<TSource>(TypedSource<TSource> source)
        where TSource : T
        => new(_ => Sources.FormOf(source).IntoLive<T>(Sources.Of(source)));

    /// <summary>Passes a literal, written into the plan where the view renders.</summary>
    /// <param name="value">The value the call receives.</param>
    public static implicit operator Arg<T>(T value)
        => new(json => value is IArgumentValue argument ? argument.Written(json) : Literals.IntoLive(value, json));

    /// <summary>Passes what a source reads when the call runs.</summary>
    /// <param name="source">The source the call's argument is read from.</param>
    public static implicit operator Arg<T>(TypedSource<T> source) => FromSource(source);
}

internal sealed class TheRunsAbortSignal : Arg
{
    internal static readonly TheRunsAbortSignal Instance = new();

    private TheRunsAbortSignal()
        : base(_ => SignalExpression.Instance)
    {
    }
}
