using System;
using System.Collections.Generic;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conditions;

/// <summary>
/// The tests a condition makes of whether a value is one of several or lies between two, each read in the browser
/// when its step runs.
/// </summary>
/// <remarks>
/// A value is one of the values when it equals one of them as <c>Eq</c> compares the two, so a date or a time is
/// never compared as its text. A source the page cannot read is neither in nor out, so neither <c>In</c> nor
/// <c>NotIn</c> holds over it, and a value that is null lies between no bounds.
/// </remarks>
public static class MembershipTests
{
    /// <summary>Tests that the source's value equals one of the values.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <param name="values">The values, each written in the form the source holds its value in.</param>
    /// <returns>A source that is true when the value equals one of them.</returns>
    public static TypedSource<bool> In<TValue>(this TypedSource<TValue> source, params TValue[] values) => throw new NotImplementedException();

    /// <summary>Tests that the source's value equals one of the items a list holds when the condition runs, as <c>Any(item =&gt; item == value)</c> reads it.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <param name="values">The source of the list.</param>
    /// <returns>A source that is true when the value equals one of the list's items.</returns>
    public static TypedSource<bool> In<TValue>(this TypedSource<TValue> source, ITypedSource<IReadOnlyList<TValue>> values) => throw new NotImplementedException();

    /// <summary>Tests that the source's value equals none of the values.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <param name="values">The values, each written in the form the source holds its value in.</param>
    /// <returns>A source that is true when the value equals none of them.</returns>
    public static TypedSource<bool> NotIn<TValue>(this TypedSource<TValue> source, params TValue[] values) => throw new NotImplementedException();

    /// <summary>Tests that the source's value equals none of the items a list holds when the condition runs.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <param name="values">The source of the list.</param>
    /// <returns>A source that is true when the value equals none of the list's items.</returns>
    public static TypedSource<bool> NotIn<TValue>(this TypedSource<TValue> source, ITypedSource<IReadOnlyList<TValue>> values) => throw new NotImplementedException();

    /// <summary>Tests that the source's value lies between two literals, both included.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <param name="low">The lowest value that holds.</param>
    /// <param name="high">The highest value that holds.</param>
    /// <returns>A source that is true when the value is neither below the low nor above the high.</returns>
    public static TypedSource<bool> Between<TValue>(this TypedSource<TValue> source, TValue low, TValue high)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value lies between the values two other sources read, both included.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <param name="low">The source of the lowest value that holds.</param>
    /// <param name="high">The source of the highest value that holds.</param>
    /// <returns>A source that is true when the value is neither below the low nor above the high.</returns>
    public static TypedSource<bool> Between<TValue>(this TypedSource<TValue> source, TypedSource<TValue> low, TypedSource<TValue> high)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value lies between two literals, both included; a source that holds null does not.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <param name="low">The lowest value that holds.</param>
    /// <param name="high">The highest value that holds.</param>
    /// <returns>A source that is true when the value is neither below the low nor above the high.</returns>
    public static TypedSource<bool> Between<TValue>(this TypedSource<TValue?> source, TValue low, TValue high)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value lies between the values two other sources read, both included; a source that holds null does not.</summary>
    /// <param name="source">The source the test reads.</param>
    /// <param name="low">The source of the lowest value that holds.</param>
    /// <param name="high">The source of the highest value that holds.</param>
    /// <returns>A source that is true when the value is neither below the low nor above the high.</returns>
    public static TypedSource<bool> Between<TValue>(this TypedSource<TValue?> source, TypedSource<TValue?> low, TypedSource<TValue?> high)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();
}
