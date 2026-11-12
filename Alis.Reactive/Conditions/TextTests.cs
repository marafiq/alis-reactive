using System;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conditions;

/// <summary>
/// The tests a condition makes of a text, each read in the browser when its step runs.
/// </summary>
/// <remarks>
/// A test of a text the page cannot read, or of one that holds null, is neither true nor false, so its branch does
/// not run. A value that is no text stops its pipeline.
/// </remarks>
public static class TextTests
{
    /// <summary>Tests that the text holds a literal, compared character by character as <c>StringComparison.Ordinal</c> compares.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <param name="value">The text it looks for.</param>
    /// <returns>A source that is true when the text holds the value.</returns>
    public static TypedSource<bool> Contains(this ITypedSource<string?> text, string value) => throw new NotImplementedException();

    /// <summary>Tests that the text holds the text another source reads, compared character by character as <c>StringComparison.Ordinal</c> compares.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <param name="value">The source of the text it looks for.</param>
    /// <returns>A source that is true when the text holds the value.</returns>
    public static TypedSource<bool> Contains(this ITypedSource<string?> text, ITypedSource<string?> value) => throw new NotImplementedException();

    /// <summary>Tests that the text starts with a literal, compared character by character as <c>StringComparison.Ordinal</c> compares.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <param name="value">The text it starts with.</param>
    /// <returns>A source that is true when the text starts with the value.</returns>
    public static TypedSource<bool> StartsWith(this ITypedSource<string?> text, string value) => throw new NotImplementedException();

    /// <summary>Tests that the text starts with the text another source reads, compared character by character as <c>StringComparison.Ordinal</c> compares.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <param name="value">The source of the text it starts with.</param>
    /// <returns>A source that is true when the text starts with the value.</returns>
    public static TypedSource<bool> StartsWith(this ITypedSource<string?> text, ITypedSource<string?> value) => throw new NotImplementedException();

    /// <summary>Tests that the text ends with a literal, compared character by character as <c>StringComparison.Ordinal</c> compares.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <param name="value">The text it ends with.</param>
    /// <returns>A source that is true when the text ends with the value.</returns>
    public static TypedSource<bool> EndsWith(this ITypedSource<string?> text, string value) => throw new NotImplementedException();

    /// <summary>Tests that the text ends with the text another source reads, compared character by character as <c>StringComparison.Ordinal</c> compares.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <param name="value">The source of the text it ends with.</param>
    /// <returns>A source that is true when the text ends with the value.</returns>
    public static TypedSource<bool> EndsWith(this ITypedSource<string?> text, ITypedSource<string?> value) => throw new NotImplementedException();

    /// <summary>Tests that the text matches a regular expression, as <c>Regex.IsMatch</c> reads it.</summary>
    /// <remarks>
    /// The pattern is a literal: it is read where the view renders, and one that holds a construct the browser
    /// reads differently from .NET is refused there, naming the construct and the spelling both read alike, such
    /// as <c>[0-9]</c> for <c>\d</c>. A source cannot be a pattern, since a pattern read in the browser cannot be
    /// checked.
    /// </remarks>
    /// <param name="text">The text the test reads.</param>
    /// <param name="pattern">The regular expression.</param>
    /// <returns>A source that is true when the text matches the pattern.</returns>
    public static TypedSource<bool> Matches(this ITypedSource<string?> text, string pattern) => throw new NotImplementedException();

    /// <summary>Tests that the text is at least a number of characters long, counted as <c>string.Length</c> counts.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <param name="length">The fewest characters the text holds; a number below zero is refused at build.</param>
    /// <returns>A source that is true when the text is that long or longer.</returns>
    public static TypedSource<bool> MinLength(this ITypedSource<string?> text, int length) => throw new NotImplementedException();

    /// <summary>Tests that the text is at least as many characters long as another source reads, counted as <c>string.Length</c> counts.</summary>
    /// <param name="text">The text the test reads.</param>
    /// <param name="length">The source of the fewest characters the text holds.</param>
    /// <returns>A source that is true when the text is that long or longer.</returns>
    public static TypedSource<bool> MinLength(this ITypedSource<string?> text, ITypedSource<int> length) => throw new NotImplementedException();
}
