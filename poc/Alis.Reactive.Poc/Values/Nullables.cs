namespace Alis.Reactive.Values;

public static class NullableValues
{
    public static TypedSource<TValue?> OrNull<TValue>(this TypedSource<TValue> source)
        where TValue : struct
        => new DataRead<TValue?>(source.Expression, source.Build);
}

public static class NullableReferences
{
    public static TypedSource<TValue?> OrNull<TValue>(this TypedSource<TValue> source)
        where TValue : class
        => new DataRead<TValue?>(source.Expression, source.Build);
}
