using System;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conditions;

/// <summary>
/// The comparisons and the logic a condition is written with, each read in the browser when its step runs.
/// </summary>
/// <remarks>
/// A condition holds only when it is true. A comparison with a value the page cannot read, such as a member past
/// an object that is null, is neither true nor false, so neither <c>Eq</c> nor <c>NotEq</c> holds for it. Null is a
/// value: it equals null, differs from every other value, and is neither before nor after one.
/// </remarks>
public static class Comparisons
{
    /// <summary>Tests that the source's value equals a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with, written in the form the source holds its value in.</param>
    /// <returns>A source that is true when the two are equal.</returns>
    public static TypedSource<bool> Eq<TValue>(this TypedSource<TValue> left, TValue right) => throw new NotImplementedException();

    /// <summary>Tests that two sources read equal values.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the two are equal.</returns>
    public static TypedSource<bool> Eq<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right) => throw new NotImplementedException();

    /// <summary>Tests that the source's value differs from a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with, written in the form the source holds its value in.</param>
    /// <returns>A source that is true when the two differ.</returns>
    public static TypedSource<bool> NotEq<TValue>(this TypedSource<TValue> left, TValue right) => throw new NotImplementedException();

    /// <summary>Tests that two sources read different values.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the two differ.</returns>
    public static TypedSource<bool> NotEq<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right) => throw new NotImplementedException();

    /// <summary>Tests that the source's value is greater than a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the greater.</returns>
    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is greater than another source's.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the greater.</returns>
    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is greater than a literal; a source that holds null is not.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the greater.</returns>
    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is greater than another source's; null is in no order with a value.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the greater.</returns>
    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is greater than or equal to a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the greater or the two are equal.</returns>
    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is greater than or equal to another source's.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the greater or the two are equal.</returns>
    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is greater than or equal to a literal; a source that holds null is not.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the greater or the two are equal.</returns>
    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is greater than or equal to another source's; null is in no order with a value.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the greater or the two are equal.</returns>
    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is less than a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the lesser.</returns>
    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is less than another source's.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the lesser.</returns>
    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is less than a literal; a source that holds null is not.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the lesser.</returns>
    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is less than another source's; null is in no order with a value.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the lesser.</returns>
    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is less than or equal to a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the lesser or the two are equal.</returns>
    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is less than or equal to another source's.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the lesser or the two are equal.</returns>
    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is less than or equal to a literal; a source that holds null is not.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the lesser or the two are equal.</returns>
    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that the source's value is less than or equal to another source's; null is in no order with a value.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the lesser or the two are equal.</returns>
    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => throw new NotImplementedException();

    /// <summary>Tests that both conditions are true.</summary>
    /// <remarks>
    /// The second condition is read only when the first is not false, so a method it calls runs only then. The
    /// result is false when either is false, and is neither true nor false when one cannot be read and the other
    /// is not false.
    /// </remarks>
    /// <param name="left">The first condition.</param>
    /// <param name="right">The condition read when the first is not false.</param>
    /// <returns>A source that is true when both are true.</returns>
    public static TypedSource<bool> And(this TypedSource<bool> left, TypedSource<bool> right) => throw new NotImplementedException();

    /// <summary>Tests that either condition is true.</summary>
    /// <remarks>
    /// The second condition is read only when the first is not true, so a method it calls runs only then. The
    /// result is false when both are false, and is neither true nor false when one cannot be read and the other
    /// is not true.
    /// </remarks>
    /// <param name="left">The first condition.</param>
    /// <param name="right">The condition read when the first is not true.</param>
    /// <returns>A source that is true when either is true.</returns>
    public static TypedSource<bool> Or(this TypedSource<bool> left, TypedSource<bool> right) => throw new NotImplementedException();

    /// <summary>Tests that the condition is false.</summary>
    /// <remarks>A condition that cannot be read stays neither true nor false, so its opposite does not hold either.</remarks>
    /// <returns>A source that is true when the condition is false.</returns>
    public static TypedSource<bool> Not(this TypedSource<bool> condition) => throw new NotImplementedException();
}
