using System;
using System.Collections.Generic;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conditions;

/// <summary>
/// The tests a condition makes of whether a value is there, each read in the browser when its step runs.
/// </summary>
/// <remarks>
/// A member the data lacks reads the value System.Text.Json binds for it, so <c>IsNull</c> holds over a member whose
/// start is null. A value past a holder that holds null reads nothing, so neither <c>IsNull</c> nor <c>NotNull</c>
/// holds over it.
/// </remarks>
public static class ValueTests
{
    /// <summary>Tests that the source's value is null.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <returns>A source that is true when the value is null.</returns>
    public static TypedSource<bool> IsNull<TValue>(this TypedSource<TValue?> source)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is null.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <returns>A source that is true when the value is null.</returns>
    public static TypedSource<bool> IsNull<TValue>(this TypedSource<TValue> source)
        where TValue : class?
        => throw new NotImplementedException();

    /// <summary>Tests that the source holds a value, one that is not null.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <returns>A source that is true when the value is not null.</returns>
    public static TypedSource<bool> NotNull<TValue>(this TypedSource<TValue?> source)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Tests that the source holds a value, one that is not null.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <returns>A source that is true when the value is not null.</returns>
    public static TypedSource<bool> NotNull<TValue>(this TypedSource<TValue> source)
        where TValue : class?
        => throw new NotImplementedException();

    /// <summary>Tests that the text is null or empty, as <c>string.IsNullOrEmpty</c> reads it.</summary>
    /// <remarks>A text the page cannot read is empty too.</remarks>
    /// <param name="text">The text the test reads.</param>
    /// <returns>A source that is true when the text is null, empty or not there.</returns>
    public static TypedSource<bool> IsEmpty(this ITypedSource<string?> text) => throw new NotImplementedException();

    /// <summary>Tests that the list is null or holds no item.</summary>
    /// <remarks>A list the page cannot read is empty too.</remarks>
    /// <param name="list">The list the test reads.</param>
    /// <returns>A source that is true when the list is null, holds no item or is not there.</returns>
    public static TypedSource<bool> IsEmpty<TItem>(this ITypedSource<IReadOnlyList<TItem>?> list) => throw new NotImplementedException();

    /// <summary>Tests that the text holds at least one character.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <returns>A source that is true when the text is there and not empty.</returns>
    public static TypedSource<bool> NotEmpty(this ITypedSource<string?> text) => throw new NotImplementedException();

    /// <summary>Tests that the list holds at least one item.</summary>
    /// <param name="list">The list the test reads.</param>
    /// <returns>A source that is true when the list is there and holds an item.</returns>
    public static TypedSource<bool> NotEmpty<TItem>(this ITypedSource<IReadOnlyList<TItem>?> list) => throw new NotImplementedException();
}
