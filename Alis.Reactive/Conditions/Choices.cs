using System;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conditions;

/// <summary>
/// A value chosen in the browser from two others: a fallback for a source that holds null, and one of two values
/// by a condition.
/// </summary>
/// <remarks>
/// The value chosen is a source, so it goes wherever a value goes: an element's text, a property, a request's
/// member, another comparison.
/// </remarks>
public static class Choices
{
    /// <summary>Reads the source's value, or a literal when the source holds null or cannot be read.</summary>
    /// <param name="source">The source read first.</param>
    /// <param name="fallback">The value used in its place, written in the form the source holds its value in.</param>
    /// <returns>A source of the type without null.</returns>
    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TValue fallback)
        where TValue : class
        => throw new NotImplementedException();

    /// <summary>Reads the source's value, or another source's when the first holds null or cannot be read.</summary>
    /// <remarks>The fallback is read only when it is needed, so a method it calls runs only then.</remarks>
    /// <param name="source">The source read first.</param>
    /// <param name="fallback">The source read in its place.</param>
    /// <returns>A source of the type without null.</returns>
    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TypedSource<TValue> fallback)
        where TValue : class
        => throw new NotImplementedException();

    /// <summary>Reads the source's value, or a literal when the source holds null or cannot be read.</summary>
    /// <param name="source">The source read first.</param>
    /// <param name="fallback">The value used in its place, written in the form the source holds its value in.</param>
    /// <returns>A source of the type without null.</returns>
    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TValue fallback)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Reads the source's value, or another source's when the first holds null or cannot be read.</summary>
    /// <remarks>The fallback is read only when it is needed, so a method it calls runs only then.</remarks>
    /// <param name="source">The source read first.</param>
    /// <param name="fallback">The source read in its place.</param>
    /// <returns>A source of the type without null.</returns>
    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TypedSource<TValue> fallback)
        where TValue : struct
        => throw new NotImplementedException();

    /// <summary>Reads one literal when the condition is true and the other when it is false or cannot be read.</summary>
    /// <param name="test">The condition that chooses.</param>
    /// <param name="whenTrue">The value read when the condition is true.</param>
    /// <param name="whenFalse">The value read otherwise.</param>
    /// <returns>A source of the value chosen.</returns>
    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TValue whenTrue, TValue whenFalse) => throw new NotImplementedException();

    /// <summary>Reads one source when the condition is true and the other when it is false or cannot be read.</summary>
    /// <remarks>Only the source chosen is read, so a method the other calls does not run.</remarks>
    /// <param name="test">The condition that chooses.</param>
    /// <param name="whenTrue">The source read when the condition is true.</param>
    /// <param name="whenFalse">The source read otherwise.</param>
    /// <returns>A source of the value chosen.</returns>
    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TypedSource<TValue> whenTrue, TypedSource<TValue> whenFalse) => throw new NotImplementedException();

    /// <summary>Reads a literal when the condition is true and a source when it is false or cannot be read.</summary>
    /// <param name="test">The condition that chooses.</param>
    /// <param name="whenTrue">The value read when the condition is true, written in the form the source holds its value in.</param>
    /// <param name="whenFalse">The source read otherwise.</param>
    /// <returns>A source of the value chosen.</returns>
    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TValue whenTrue, TypedSource<TValue> whenFalse) => throw new NotImplementedException();

    /// <summary>Reads a source when the condition is true and a literal when it is false or cannot be read.</summary>
    /// <param name="test">The condition that chooses.</param>
    /// <param name="whenTrue">The source read when the condition is true.</param>
    /// <param name="whenFalse">The value read otherwise, written in the form the source holds its value in.</param>
    /// <returns>A source of the value chosen.</returns>
    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TypedSource<TValue> whenTrue, TValue whenFalse) => throw new NotImplementedException();
}
