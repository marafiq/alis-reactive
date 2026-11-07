using System;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Values;

namespace Alis.Reactive.Conditions;

public static class Choices
{
    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TValue fallback)
        where TValue : class
        => Coalesced<TValue>(source, Literal.Of(fallback, source.Form), source.Form);

    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TypedSource<TValue> fallback)
        where TValue : class
        => Coalesced<TValue>(source, fallback.Expression, OneForm(source, fallback));

    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TValue fallback)
        where TValue : struct
        => Coalesced<TValue>(source, Literal.Of(fallback, source.Form), source.Form);

    public static TypedSource<TValue> IfNull<TValue>(this TypedSource<TValue?> source, TypedSource<TValue> fallback)
        where TValue : struct
        => Coalesced<TValue>(source, fallback.Expression, OneForm(source, fallback));

    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TValue whenTrue, TValue whenFalse)
        => Chosen<TValue>(test, Literal.Of(whenTrue, test.Build), Literal.Of(whenFalse, test.Build), test.Build.OptionsForm);

    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TypedSource<TValue> whenTrue, TypedSource<TValue> whenFalse)
        => Chosen<TValue>(test, whenTrue.Expression, whenFalse.Expression, OneForm(whenTrue, whenFalse));

    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TValue whenTrue, TypedSource<TValue> whenFalse)
        => Chosen<TValue>(test, Literal.Of(whenTrue, whenFalse.Form), whenFalse.Expression, whenFalse.Form);

    public static TypedSource<TValue> Choose<TValue>(this TypedSource<bool> test, TypedSource<TValue> whenTrue, TValue whenFalse)
        => Chosen<TValue>(test, whenTrue.Expression, Literal.Of(whenFalse, whenTrue.Form), whenTrue.Form);

    private static HeldForm OneForm(IPlanSource first, IPlanSource second)
    {
        var own = first.Form.IsOwn ? first.Form : second.Form;
        if (!own.IsOwn) return first.Form;
        throw new InvalidOperationException(
            $"A choice joins '{own.Read}', which {own.Cause} writes unlike the app's options, with another value read from the page; choose between it and a value the view writes, or choose on the server.");
    }

    private static DataRead<TValue> Coalesced<TValue>(IPlanSource source, Expression fallback, HeldForm form)
        => new(new OperatorExpression(Operator.Coalesce, [source.Expression, fallback]), source.Build, form);

    private static DataRead<TValue> Chosen<TValue>(TypedSource<bool> test, Expression whenTrue, Expression whenFalse, HeldForm form)
        => new(new OperatorExpression(Operator.Cond, [test.Expression, whenTrue, whenFalse]), test.Build, form);
}
