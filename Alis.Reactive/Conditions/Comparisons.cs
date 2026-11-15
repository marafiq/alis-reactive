using System;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
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
    public static TypedSource<bool> Eq<TValue>(this TypedSource<TValue> left, TValue right) => Equal(Operator.Eq, left, right);

    /// <summary>Tests that two sources read equal values.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the two are equal.</returns>
    public static TypedSource<bool> Eq<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right) => Equal(Operator.Eq, left, right);

    /// <summary>Tests that the source's value differs from a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with, written in the form the source holds its value in.</param>
    /// <returns>A source that is true when the two differ.</returns>
    public static TypedSource<bool> NotEq<TValue>(this TypedSource<TValue> left, TValue right) => Equal(Operator.Neq, left, right);

    /// <summary>Tests that two sources read different values.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the two differ.</returns>
    public static TypedSource<bool> NotEq<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right) => Equal(Operator.Neq, left, right);

    /// <summary>Tests that the source's value is greater than a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the greater.</returns>
    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gt, left.Expression, left.Form.Beside(right), left.Form);

    /// <summary>Tests that the source's value is greater than another source's.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the greater.</returns>
    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gt, left.Expression, right.Expression, left.Form, right.Form);

    /// <summary>Tests that the source's value is greater than a literal; a source that holds null is not.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the greater.</returns>
    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gt, left.Expression, left.Form.Beside(right), left.Form);

    /// <summary>Tests that the source's value is greater than another source's; null is in no order with a value.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the greater.</returns>
    public static TypedSource<bool> Gt<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gt, left.Expression, right.Expression, left.Form, right.Form);

    /// <summary>Tests that the source's value is greater than or equal to a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the greater or the two are equal.</returns>
    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gte, left.Expression, left.Form.Beside(right), left.Form);

    /// <summary>Tests that the source's value is greater than or equal to another source's.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the greater or the two are equal.</returns>
    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gte, left.Expression, right.Expression, left.Form, right.Form);

    /// <summary>Tests that the source's value is greater than or equal to a literal; a source that holds null is not.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the greater or the two are equal.</returns>
    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gte, left.Expression, left.Form.Beside(right), left.Form);

    /// <summary>Tests that the source's value is greater than or equal to another source's; null is in no order with a value.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the greater or the two are equal.</returns>
    public static TypedSource<bool> Gte<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Gte, left.Expression, right.Expression, left.Form, right.Form);

    /// <summary>Tests that the source's value is less than a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the lesser.</returns>
    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lt, left.Expression, left.Form.Beside(right), left.Form);

    /// <summary>Tests that the source's value is less than another source's.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the lesser.</returns>
    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lt, left.Expression, right.Expression, left.Form, right.Form);

    /// <summary>Tests that the source's value is less than a literal; a source that holds null is not.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the lesser.</returns>
    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lt, left.Expression, left.Form.Beside(right), left.Form);

    /// <summary>Tests that the source's value is less than another source's; null is in no order with a value.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the lesser.</returns>
    public static TypedSource<bool> Lt<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lt, left.Expression, right.Expression, left.Form, right.Form);

    /// <summary>Tests that the source's value is less than or equal to a literal.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the lesser or the two are equal.</returns>
    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lte, left.Expression, left.Form.Beside(right), left.Form);

    /// <summary>Tests that the source's value is less than or equal to another source's.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the lesser or the two are equal.</returns>
    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue> left, TypedSource<TValue> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lte, left.Expression, right.Expression, left.Form, right.Form);

    /// <summary>Tests that the source's value is less than or equal to a literal; a source that holds null is not.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The value it is compared with.</param>
    /// <returns>A source that is true when the source's value is the lesser or the two are equal.</returns>
    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue?> left, TValue right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lte, left.Expression, left.Form.Beside(right), left.Form);

    /// <summary>Tests that the source's value is less than or equal to another source's; null is in no order with a value.</summary>
    /// <param name="left">The source the comparison tests.</param>
    /// <param name="right">The source it is compared with.</param>
    /// <returns>A source that is true when the first value is the lesser or the two are equal.</returns>
    public static TypedSource<bool> Lte<TValue>(this TypedSource<TValue?> left, TypedSource<TValue?> right)
        where TValue : struct, IComparable<TValue>
        => Ordered<TValue>(Operator.Lte, left.Expression, right.Expression, left.Form, right.Form);

    /// <summary>Tests that both conditions are true.</summary>
    /// <remarks>
    /// The second condition is read only when the first is not false, so a method it calls runs only then. The
    /// result is false when either is false, and is neither true nor false when one cannot be read and the other
    /// is not false.
    /// </remarks>
    /// <param name="left">The first condition.</param>
    /// <param name="right">The condition read when the first is not false.</param>
    /// <returns>A source that is true when both are true.</returns>
    public static TypedSource<bool> And(this TypedSource<bool> left, TypedSource<bool> right) => Joined(Operator.And, left, right);

    /// <summary>Tests that either condition is true.</summary>
    /// <remarks>
    /// The second condition is read only when the first is not true, so a method it calls runs only then. The
    /// result is false when both are false, and is neither true nor false when one cannot be read and the other
    /// is not true.
    /// </remarks>
    /// <param name="left">The first condition.</param>
    /// <param name="right">The condition read when the first is not true.</param>
    /// <returns>A source that is true when either is true.</returns>
    public static TypedSource<bool> Or(this TypedSource<bool> left, TypedSource<bool> right) => Joined(Operator.Or, left, right);

    /// <summary>Tests that the condition is false.</summary>
    /// <remarks>A condition that cannot be read stays neither true nor false, so its opposite does not hold either.</remarks>
    /// <returns>A source that is true when the condition is false.</returns>
    public static TypedSource<bool> Not(this TypedSource<bool> condition) => new Computed<bool>(new OpExpression(Operator.Not, [condition.Expression]), condition.Form.Computed);

    private static readonly Type[] Orderable =
    [
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal), typeof(char),
    ];

    private static TypedSource<bool> Equal<TValue>(Operator op, TypedSource<TValue> left, TValue right)
    {
        if (left.Form.Own is { } own) return EqualInItsOwnForm(op, left, right, own);
        var written = left.Form.Beside(right);
        var writtenAsAnObject = written.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        if (writtenAsAnObject) throw ComparesObjects();
        return Compared<TValue>(op, left, written, left.Form);
    }

    private static TypedSource<bool> Equal<TValue>(Operator op, TypedSource<TValue> left, TypedSource<TValue> right)
    {
        RefuseTwoValuesWhereOneIsInItsOwnForm(left.Form);
        RefuseTwoValuesWhereOneIsInItsOwnForm(right.Form);
        if (!left.Form.WritesAsAScalar<TValue>()) throw ComparesObjects();
        return Compared<TValue>(op, left, right.Expression, right.Form);
    }

    private static TypedSource<bool> EqualInItsOwnForm<TValue>(Operator op, TypedSource<TValue> left, TValue right, HeldForm.OwnWriting own)
    {
        if (!left.Form.Computed.WritesAsAScalar<TValue>())
            throw new InvalidOperationException($"A condition compares '{own.Read}', which {own.Cause} writes unlike {left.Form.BaseFormName}, and .NET compares the object it holds; compare it on the server.");
        if (Keys.IsDateOrTime<TValue>())
            throw new InvalidOperationException("A condition compares a date or a time its own converter writes, whose text the converter decides; compare it on the server.");
        var written = left.Form.Beside(right);
        var writtenAsText = written.Value.ValueKind == JsonValueKind.String;
        if (writtenAsText && ComparedTypes.IsBinaryFraction(typeof(TValue)))
            throw new InvalidOperationException("A condition compares a float or double the JSON writes as text, whose text does not equal as the number does, as -0 and 0 or NaN and NaN; compare it on the server.");
        var decimalText = writtenAsText && ComparedTypes.IsDecimal(typeof(TValue));
        if (decimalText && own.Member.HasItsOwnConverter)
            throw new InvalidOperationException("A condition compares a decimal its own converter writes as text, whose text the converter decides; compare it on the server.");
        if (!decimalText) return new Computed<bool>(new OpExpression(op, [left.Expression, written]), left.Form.Computed);
        var equal = new OpExpression(Operator.EqDecimal, [left.Expression, written]);
        return new Computed<bool>(op == Operator.Eq ? equal : new OpExpression(Operator.Not, [equal]), left.Form.Computed);
    }

    internal static void RefuseTwoValuesWhereOneIsInItsOwnForm(HeldForm form)
    {
        if (form.Own is not { } own) return;
        throw new InvalidOperationException(
            $"A condition compares two values, and {own.Cause} writes '{own.Read}' unlike {form.BaseFormName}, so the browser does not compare them as .NET does; compare them on the server.");
    }

    private static void RefuseAnOrderingInItsOwnForm(HeldForm form)
    {
        if (form.Own is not { } own) return;
        throw new InvalidOperationException(
            $"A condition orders '{own.Read}', which {own.Cause} writes unlike {form.BaseFormName}, so the browser does not order it as .NET does; order it on the server.");
    }

    private static TypedSource<bool> Compared<TValue>(Operator op, TypedSource<TValue> left, Expression right, HeldForm rightsForm)
        => new Computed<bool>(new OpExpression(op, [Keys.Of<TValue>(left.Expression, left.Form), Keys.Of<TValue>(right, rightsForm)]), left.Form.Computed);

    private static TypedSource<bool> Ordered<TOrdered>(Operator op, Expression left, Expression right, HeldForm form) => Ordered<TOrdered>(op, left, right, form, form);

    private static TypedSource<bool> Ordered<TOrdered>(Operator op, Expression left, Expression right, HeldForm form, HeldForm partnersForm)
    {
        RefuseAnOrderingInItsOwnForm(partnersForm);
        RefuseAnOrderingInItsOwnForm(form);
        var browserOrdersAsDotNet = Orderable.Contains(typeof(TOrdered)) || Keys.IsDateOrTime<TOrdered>();
        if (!browserOrdersAsDotNet)
            throw new InvalidOperationException("A condition orders values the browser cannot order as .NET does; order numbers, a char, or a date or a time, or order on the server.");
        return new Computed<bool>(new OpExpression(op, [Keys.Of<TOrdered>(left, form), Keys.Of<TOrdered>(right, partnersForm)]), form.Computed);
    }

    private static TypedSource<bool> Joined(Operator op, TypedSource<bool> left, TypedSource<bool> right)
        => new Computed<bool>(new OpExpression(op, [left.Expression, right.Expression]), left.Form.Computed);

    internal static InvalidOperationException ComparesObjects()
        => new("A condition compares objects, and the browser compares objects by reference; compare a member of each.");
}
