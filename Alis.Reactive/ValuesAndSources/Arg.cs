using System;
using Alis.Reactive.Execution;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal interface IArgumentValue
{
    Expression Written(AppJson json);
}

/// <summary>
/// One argument of a call the browser makes: a literal, a source or a callback's answer.
/// </summary>
/// <remarks>
/// A declaration that takes its arguments as one list takes them as this type; a value or source is written as an
/// <see cref="Arg{T}"/>, so it stays typed.
/// </remarks>
public abstract class Arg
{
    private readonly Func<BlockWriter, string, Expression> _write;

    private protected Arg(Func<BlockWriter, string, Expression> write)
    {
        _write = write;
    }

    internal Expression Written(BlockWriter writer, string argument) => _write(writer, argument);
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
    private Arg(Func<AppJson, string, Expression> write) : base((writer, argument) => write(writer.Content.Json, argument))
    {
    }

    internal static Arg<T> FromSource<TSource>(TypedSource<TSource> source)
        where TSource : T
        => new((_, _) => Sources.FormOf(source).IntoLive<T>(Sources.Of(source)));

    /// <summary>Passes a literal, written into the plan where the view renders.</summary>
    /// <param name="value">The value the call receives.</param>
    public static implicit operator Arg<T>(T value)
        => new((json, argument) => value is IArgumentValue held ? held.Written(json) : Literals.IntoLive(value, json, argument));

    /// <summary>Passes what a source reads when the call runs.</summary>
    /// <param name="source">The source the call's argument is read from.</param>
    public static implicit operator Arg<T>(TypedSource<T> source) => FromSource(source);
}

internal sealed class TheRunsAbortSignal : Arg
{
    internal static readonly TheRunsAbortSignal Instance = new();

    private TheRunsAbortSignal()
        : base((_, _) => SignalExpression.Instance)
    {
    }
}
