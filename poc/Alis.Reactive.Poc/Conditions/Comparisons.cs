using System;
using System.Text.Json;
using Alis.Reactive.Plan;
using Alis.Reactive.Values;

namespace Alis.Reactive.Conditions;

public static class Comparisons
{
    public static TypedSource<bool> Eq<TValue>(this TypedSource<TValue> left, TValue right) => Equal(Operator.Eq, left, right);

    public static TypedSource<bool> Eq<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right) => Equal(Operator.Eq, left, right);

    public static TypedSource<bool> NotEq<TValue>(this TypedSource<TValue> left, TValue right) => Equal(Operator.Neq, left, right);

    public static TypedSource<bool> NotEq<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right) => Equal(Operator.Neq, left, right);

    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gt, left, Literal.Of(right, left.Build));

    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gt, left, right.Expression);

    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gt, left, Literal.Of(right, left.Build));

    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gt, left, right.Expression);

    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gte, left, Literal.Of(right, left.Build));

    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gte, left, right.Expression);

    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gte, left, Literal.Of(right, left.Build));

    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gte, left, right.Expression);

    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lt, left, Literal.Of(right, left.Build));

    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lt, left, right.Expression);

    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lt, left, Literal.Of(right, left.Build));

    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lt, left, right.Expression);

    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lte, left, Literal.Of(right, left.Build));

    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lte, left, right.Expression);

    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lte, left, Literal.Of(right, left.Build));

    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lte, left, right.Expression);

    public static TypedSource<bool> And(this TypedSource<bool> left, TypedSource<bool> right) => Combined(Operator.And, left, right.Expression);

    public static TypedSource<bool> Or(this TypedSource<bool> left, TypedSource<bool> right) => Combined(Operator.Or, left, right.Expression);

    public static TypedSource<bool> Not(this TypedSource<bool> condition)
        => new DataRead<bool>(new OperatorExpression(Operator.Not, [condition.Expression]), condition.Build);

    private static TypedSource<bool> Equal<TValue>(Operator op, TypedSource<TValue> left, TValue right)
    {
        RefuseDatesAndTimes<TValue>();
        var written = Literal.Of(right, left.Build);
        var writtenAsAnObject = written.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        if (writtenAsAnObject) throw ComparesObjects();
        return Combined(op, left, written);
    }

    private static TypedSource<bool> Equal<TValue>(Operator op, TypedSource<TValue> left, TypedSource<TValue> right)
    {
        RefuseDatesAndTimes<TValue>();
        if (!ComparedTypes.IsScalar(typeof(TValue))) throw ComparesObjects();
        return Combined(op, left, right.Expression);
    }

    private static TypedSource<bool> Ordered<TOrdered>(Operator op, IPlanSource left, Expression right)
    {
        RefuseDatesAndTimes<TOrdered>();
        var orderable = ComparedTypes.IsNumber(typeof(TOrdered)) || ComparedTypes.IsChar(typeof(TOrdered));
        if (!orderable)
        {
            throw new InvalidOperationException(
                "A condition orders values the browser cannot order as .NET does: order numbers or a char, or order on the server.");
        }

        return new DataRead<bool>(new OperatorExpression(op, [left.Expression, right]), left.Build);
    }

    private static TypedSource<bool> Combined(Operator op, IPlanSource left, Expression right)
        => new DataRead<bool>(new OperatorExpression(op, [left.Expression, right]), left.Build);

    private static void RefuseDatesAndTimes<TValue>()
    {
        if (!ComparedTypes.IsDateOrTime(typeof(TValue))) return;
        throw new InvalidOperationException(
            "A condition compares dates or times, whose text neither orders nor equals as the instant does: compare them on the server.");
    }

    private static InvalidOperationException ComparesObjects()
        => new("A condition compares objects, and the browser compares objects by reference: compare a member of each.");
}
