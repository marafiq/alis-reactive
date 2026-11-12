using System;

namespace Alis.Reactive.ValuesAndSources;

/// <summary>
/// Reads a source of a value type as a source of its nullable type.
/// </summary>
public static class NullableValues
{
    /// <summary>
    /// Reads the same value as a source of its nullable type, for an argument or a comparison that asks for one.
    /// </summary>
    /// <remarks>
    /// A set needs no <c>OrNull()</c>: a property of a nullable type takes a source of the type itself.
    /// </remarks>
    /// <typeparam name="TValue">The value type the source reads.</typeparam>
    /// <returns>A source that reads the same value, typed as nullable.</returns>
    public static TypedSource<TValue?> OrNull<TValue>(this TypedSource<TValue> source)
        where TValue : struct
        => throw new NotImplementedException();
}

/// <summary>
/// Reads a source of a reference type as a source of its nullable type.
/// </summary>
public static class NullableReferences
{
    /// <summary>
    /// Reads the same value as a source of its nullable type, for an argument or a comparison that asks for one.
    /// </summary>
    /// <remarks>
    /// A set needs no <c>OrNull()</c>: a property of a nullable type takes a source of the type itself.
    /// </remarks>
    /// <typeparam name="TValue">The reference type the source reads.</typeparam>
    /// <returns>A source that reads the same value, typed as nullable.</returns>
    public static TypedSource<TValue?> OrNull<TValue>(this TypedSource<TValue> source)
        where TValue : class
        => throw new NotImplementedException();
}
