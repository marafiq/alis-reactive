using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Schema;
using Alis.Reactive.ValuesAndSources;
using PlanExpression = Alis.Reactive.Schema.Expression;

namespace Alis.Reactive.ArrayLambdas;

/// <summary>
/// The operations of a list the browser holds as data: a list a response, an event's payload or a member carries.
/// </summary>
/// <remarks>
/// Each operation runs in the browser when its step runs and returns a source, so its result goes wherever a
/// value goes. A lambda reads its item's members and compares them; it is translated for the browser where the
/// view renders and never runs on the server. It reads a value the view holds, such as <c>Model.Ward</c> or a
/// local, as the value is when the view renders. An operation over a list that cannot be read cannot be read
/// either.
/// </remarks>
public static class DataLists
{
    /// <summary>Counts the list's items.</summary>
    /// <returns>A source of the number of items.</returns>
    public static TypedSource<int> Count<TItem>(this ITypedSource<IEnumerable<TItem>> list) => new Computed<int>(Length(Sources.Of(list)), Sources.FormOf(list).Computed);

    /// <summary>Counts the items a condition holds for.</summary>
    /// <param name="list">The list to count in.</param>
    /// <param name="predicate">The condition an item is counted by.</param>
    /// <returns>A source of the number of items the condition holds for.</returns>
    public static TypedSource<int> Count<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => new Computed<int>(Length(Called(Filter, list, ItemLambda.Of(predicate, Sources.FormOf(list)))), Sources.FormOf(list).Computed);

    /// <summary>Keeps the items a condition holds for.</summary>
    /// <param name="list">The list to keep items of.</param>
    /// <param name="keep">The condition an item is kept by.</param>
    /// <returns>A source of a new list, in the list's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> Where<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> keep)
        => new Computed<IReadOnlyList<TItem>>(Called(Filter, list, ItemLambda.Of(keep, Sources.FormOf(list))), Sources.FormOf(list));

    /// <summary>Reads one field of every item.</summary>
    /// <remarks>
    /// A field whose type holds null is null in the new list where an item lacks it. A field whose type holds no
    /// null stops the pipeline where it cannot be read, so the new list holds only values of its type.
    /// </remarks>
    /// <param name="list">The list to read.</param>
    /// <param name="field">The field read from each item.</param>
    /// <returns>A source of a new list of the field's values, in the list's order.</returns>
    public static TypedSource<IReadOnlyList<TResult>> Select<TItem, TResult>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TResult>> field)
        => new Computed<IReadOnlyList<TResult>>(Called(Map, list, ItemLambda.Selecting(field, Sources.FormOf(list), fieldHoldsNull: default(TResult) is null)), Sources.FormOf(list));

    /// <summary>Tests that the list holds an item.</summary>
    /// <returns>A source that is true when the list holds at least one item.</returns>
    public static TypedSource<bool> Any<TItem>(this ITypedSource<IEnumerable<TItem>> list) => new Computed<bool>(Called(Some, list, ItemLambda.EveryItem()), Sources.FormOf(list).Computed);

    /// <summary>Tests that a condition holds for an item of the list.</summary>
    /// <param name="list">The list to look in.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source that is true when the condition holds for at least one item.</returns>
    public static TypedSource<bool> Any<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => new Computed<bool>(Called(Some, list, ItemLambda.Of(predicate, Sources.FormOf(list))), Sources.FormOf(list).Computed);

    /// <summary>Tests that a condition holds for every item of the list.</summary>
    /// <param name="list">The list to look in.</param>
    /// <param name="predicate">The condition every item is held to.</param>
    /// <returns>A source that is true when the condition holds for every item.</returns>
    public static TypedSource<bool> All<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => new Computed<bool>(Called(Every, list, ItemLambda.Of(predicate, Sources.FormOf(list))), Sources.FormOf(list).Computed);

    /// <summary>Finds the first item a condition holds for.</summary>
    /// <param name="list">The list to look in.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source of the item, which cannot be read when no item matches.</returns>
    public static TypedSource<TItem> Find<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => new Computed<TItem>(Found(list, predicate), Sources.FormOf(list));

    /// <summary>Finds the first item a condition holds for and reads one field of it.</summary>
    /// <param name="list">The list to look in.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <param name="field">The field read from the item found.</param>
    /// <returns>A source of the field's value, which cannot be read when no item matches.</returns>
    public static TypedSource<TResult> Find<TItem, TResult>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate, Expression<Func<TItem, TResult>> field)
        => DataReads.Past<TResult>(Found(list, predicate), Sources.FormOf(list), field, Sources.FormOf(list).DataOptions);

    /// <summary>Orders the list's items by a key, the smallest first and null before every value.</summary>
    /// <remarks>
    /// The order is the one .NET gives the key's type, and items with equal keys keep their order. A text key is
    /// ordered by the culture the view renders under. The list itself is left as it is.
    /// </remarks>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey>> key)
        => Sorted(list, key, ListOrder.ComparisonOf<TKey>(key, Sources.FormOf(list)), descending: false);

    /// <summary>Orders the list's items by a key that holds null, the smallest first and null before every value.</summary>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey?>> key)
        where TKey : struct
        => Sorted(list, key, ListOrder.ComparisonOf<TKey>(key, Sources.FormOf(list)), descending: false);

    /// <summary>Orders the list's items by a text key, compared as the comparer compares.</summary>
    /// <remarks>
    /// <see cref="StringComparer.Ordinal"/> orders by UTF-16 code unit, and <see cref="StringComparer.CurrentCulture"/>
    /// by the culture the view renders under, as no comparer does. The build refuses any other comparer.
    /// </remarks>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The text read from each item.</param>
    /// <param name="comparer">How two texts are compared.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, string?>> key, StringComparer comparer)
        => Sorted(list, key, ListOrder.TextComparison(key, comparer, Sources.FormOf(list)), descending: false);

    /// <summary>Orders the list's items by a key, the greatest first and null after every value.</summary>
    /// <remarks>
    /// The order is the reverse of the one .NET gives the key's type, and items with equal keys keep their order.
    /// A text key is ordered by the culture the view renders under. The list itself is left as it is.
    /// </remarks>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey>> key)
        => Sorted(list, key, ListOrder.ComparisonOf<TKey>(key, Sources.FormOf(list)), descending: true);

    /// <summary>Orders the list's items by a key that holds null, the greatest first and null after every value.</summary>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey?>> key)
        where TKey : struct
        => Sorted(list, key, ListOrder.ComparisonOf<TKey>(key, Sources.FormOf(list)), descending: true);

    /// <summary>Orders the list's items by a text key, in the reverse of the comparer's order.</summary>
    /// <remarks>
    /// <see cref="StringComparer.Ordinal"/> orders by UTF-16 code unit, and <see cref="StringComparer.CurrentCulture"/>
    /// by the culture the view renders under, as no comparer does. The build refuses any other comparer.
    /// </remarks>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The text read from each item.</param>
    /// <param name="comparer">How two texts are compared.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, string?>> key, StringComparer comparer)
        => Sorted(list, key, ListOrder.TextComparison(key, comparer, Sources.FormOf(list)), descending: true);

    /// <summary>Adds a whole number read from every item.</summary>
    /// <remarks>A total past the range of <see cref="int"/> stops the pipeline, as .NET's checked sum refuses it.</remarks>
    /// <param name="list">The list to add over.</param>
    /// <param name="term">The number read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<int> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, int>> term) => Summed<TItem, int>(Operator.SumInt, list, term);

    /// <summary>Adds a number read from every item.</summary>
    /// <param name="list">The list to add over.</param>
    /// <param name="term">The number read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<double> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, double>> term) => Summed<TItem, double>(Operator.SumDouble, list, term);

    /// <summary>Adds a decimal read from every item, exactly, as .NET adds decimals.</summary>
    /// <param name="list">The list to add over.</param>
    /// <param name="term">The decimal read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<decimal> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, decimal>> term) => Summed<TItem, decimal>(Operator.SumDecimal, list, term);

    private const string Filter = "filter";
    private const string Map = "map";
    private const string Some = "some";
    private const string Every = "every";
    private const string FindItem = "find";
    private const string LengthOf = "length";
    private const string ToSorted = "toSorted";

    private static GetExpression Length(PlanExpression list) => new([new MemberStep(LengthOf)], PathMembers.RequiredIfHeld, list);

    private static CallExpression Called<TItem>(string method, ITypedSource<IEnumerable<TItem>> list, FnExpression fn)
        => new([new MemberStep(method)], PathMembers.RequiredIfHeld, Sources.Of(list), [fn]);

    private static CallExpression Found<TItem>(ITypedSource<IEnumerable<TItem>> list, LambdaExpression predicate)
        => Called(FindItem, list, ItemLambda.Of(predicate, Sources.FormOf(list)));

    private static Computed<IReadOnlyList<TItem>> Sorted<TItem>(ITypedSource<IEnumerable<TItem>> list, LambdaExpression key, Operator comparison, bool descending)
        => new(Called(ToSorted, list, ItemLambda.Comparing(key, Sources.FormOf(list), comparison, descending)), Sources.FormOf(list));

    private static Computed<TTotal> Summed<TItem, TTotal>(Operator sum, ITypedSource<IEnumerable<TItem>> list, LambdaExpression term)
        => new(new OpExpression(sum, [Called(Map, list, ItemLambda.Of(term, Sources.FormOf(list)))]), Sources.FormOf(list).Computed);
}
