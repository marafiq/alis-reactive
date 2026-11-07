using Alis.Reactive.Plan;
using Alis.Reactive.Values;

namespace Alis.Reactive.Conditions;

public static class Choices
{
    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TValue fallback)
        where TValue : class
        => Coalesced<TValue>(source, Literal.Of(fallback, source.Build));

    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TypedSource<TValue> fallback)
        where TValue : class
        => Coalesced<TValue>(source, fallback.Expression);

    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TValue fallback)
        where TValue : struct
        => Coalesced<TValue>(source, Literal.Of(fallback, source.Build));

    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TypedSource<TValue> fallback)
        where TValue : struct
        => Coalesced<TValue>(source, fallback.Expression);

    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TValue whenTrue, TValue whenFalse)
        => Chosen<TValue>(test, Literal.Of(whenTrue, test.Build), Literal.Of(whenFalse, test.Build));

    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TypedSource<TValue> whenTrue, TypedSource<TValue> whenFalse)
        => Chosen<TValue>(test, whenTrue.Expression, whenFalse.Expression);

    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TValue whenTrue, TypedSource<TValue> whenFalse)
        => Chosen<TValue>(test, Literal.Of(whenTrue, test.Build), whenFalse.Expression);

    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TypedSource<TValue> whenTrue, TValue whenFalse)
        => Chosen<TValue>(test, whenTrue.Expression, Literal.Of(whenFalse, test.Build));

    private static DataRead<TValue> Coalesced<TValue>(IPlanSource source, Expression fallback)
        => new(new OperatorExpression(Operator.Coalesce, [source.Expression, fallback]), source.Build);

    private static DataRead<TValue> Chosen<TValue>(TypedSource<bool> test, Expression whenTrue, Expression whenFalse)
        => new(new OperatorExpression(Operator.Cond, [test.Expression, whenTrue, whenFalse]), test.Build);
}
