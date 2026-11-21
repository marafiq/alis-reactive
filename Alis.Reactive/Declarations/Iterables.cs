using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Declarations;

/// <summary>
/// An object whose iterator yields objects, each read through its declaration: the list operations take it as the
/// list of those objects.
/// </summary>
/// <typeparam name="TItem">The declaration of each object the iterator yields.</typeparam>
public interface ILiveIterable<TItem>
    where TItem : JsObject, new()
{
    /// <summary>Gets the items the iterator yields when an operation runs.</summary>
    LiveObjects<TItem> Items { get; }
}

/// <summary>
/// An object whose iterator yields data: the list operations take it as the list of the items it yields.
/// </summary>
/// <typeparam name="TItem">The type of each item the iterator yields.</typeparam>
public interface IDataIterable<TItem>
{
    /// <summary>Gets a source of the items the iterator yields when an operation runs.</summary>
    TypedSource<IReadOnlyList<TItem>> Items { get; }
}

/// <summary>
/// The list operations of an object that declares its iterator, run over the items it yields when the step runs.
/// </summary>
/// <remarks>
/// The object takes them beside its own members, so <c>Children.Count()</c> counts an element's children and
/// <c>Classes.Count(flag =&gt; ...)</c> counts its classes.
/// </remarks>
public static class Iterables
{
    /// <summary>Counts the list's items.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <returns>A source of the number of items.</returns>
    public static TypedSource<int> Count<TItem>(this ILiveIterable<TItem> iterable)
        where TItem : JsObject, new()
        => iterable.Items.Count();

    /// <summary>Keeps the items a condition holds for.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="keep">The condition an item is kept by.</param>
    /// <returns>The list of the items kept, in the list's order.</returns>
    public static LiveObjects<TItem> Where<TItem>(this ILiveIterable<TItem> iterable, Func<TItem, TypedSource<bool>> keep)
        where TItem : JsObject, new()
        => iterable.Items.Where(keep);

    /// <summary>Reads one member of every item.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="field">The member read from each item.</param>
    /// <returns>A source of a new list of the member's values, in the list's order.</returns>
    public static TypedSource<IReadOnlyList<TResult>> Select<TItem, TResult>(this ILiveIterable<TItem> iterable, Func<TItem, TypedSource<TResult>> field)
        where TItem : JsObject, new()
        => iterable.Items.Select(field);

    /// <summary>Counts the items a condition holds for.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition an item is counted by.</param>
    /// <returns>A source of the number of items the condition holds for.</returns>
    public static TypedSource<int> Count<TItem>(this ILiveIterable<TItem> iterable, Func<TItem, TypedSource<bool>> predicate)
        where TItem : JsObject, new()
        => iterable.Items.Count(predicate);

    /// <summary>Tests that the list holds an item.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <returns>A source that is true when the list holds at least one item.</returns>
    public static TypedSource<bool> Any<TItem>(this ILiveIterable<TItem> iterable)
        where TItem : JsObject, new()
        => iterable.Items.Any();

    /// <summary>Tests that a condition holds for an item of the list.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source that is true when the condition holds for at least one item.</returns>
    public static TypedSource<bool> Any<TItem>(this ILiveIterable<TItem> iterable, Func<TItem, TypedSource<bool>> predicate)
        where TItem : JsObject, new()
        => iterable.Items.Any(predicate);

    /// <summary>Tests that a condition holds for every item of the list.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition every item is held to.</param>
    /// <returns>A source that is true when the condition holds for every item.</returns>
    public static TypedSource<bool> All<TItem>(this ILiveIterable<TItem> iterable, Func<TItem, TypedSource<bool>> predicate)
        where TItem : JsObject, new()
        => iterable.Items.All(predicate);

    /// <summary>Finds the first item a condition holds for.</summary>
    /// <remarks>
    /// Reading, setting or calling a member of the item stops the pipeline when no item matched. To read one
    /// member of an item that may not be there, give the member to <c>Find</c> as its second argument.
    /// </remarks>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>The item, whose members are read, set and called through its declaration.</returns>
    public static TItem Find<TItem>(this ILiveIterable<TItem> iterable, Func<TItem, TypedSource<bool>> predicate)
        where TItem : JsObject, new()
        => iterable.Items.Find(predicate);

    /// <summary>Finds the first item a condition holds for and reads one member of it.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <param name="field">The member read from the item found.</param>
    /// <returns>A source of the member's value, which cannot be read when no item matched.</returns>
    public static TypedSource<TResult> Find<TItem, TResult>(this ILiveIterable<TItem> iterable, Func<TItem, TypedSource<bool>> predicate, Func<TItem, TypedSource<TResult>> field)
        where TItem : JsObject, new()
        => iterable.Items.Find(predicate, field);

    /// <summary>Counts the list's items.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <returns>A source of the number of items.</returns>
    public static TypedSource<int> Count<TItem>(this IDataIterable<TItem> iterable)
        => iterable.Items.Count();

    /// <summary>Counts the items a condition holds for.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition an item is counted by.</param>
    /// <returns>A source of the number of items the condition holds for.</returns>
    public static TypedSource<int> Count<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, bool>> predicate)
        => iterable.Items.Count(predicate);

    /// <summary>Keeps the items a condition holds for.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="keep">The condition an item is kept by.</param>
    /// <returns>A source of a new list, in the list's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> Where<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, bool>> keep)
        => iterable.Items.Where(keep);

    /// <summary>Reads one field of every item.</summary>
    /// <remarks>
    /// A field whose type holds null is null in the new list where an item lacks it. A field whose type holds no
    /// null stops the pipeline where it cannot be read, so the new list holds only values of its type.
    /// </remarks>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="field">The field read from each item.</param>
    /// <returns>A source of a new list of the field's values, in the list's order.</returns>
    public static TypedSource<IReadOnlyList<TResult>> Select<TItem, TResult>(this IDataIterable<TItem> iterable, Expression<Func<TItem, TResult>> field)
        => iterable.Items.Select(field);

    /// <summary>Tests that the list holds an item.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <returns>A source that is true when the list holds at least one item.</returns>
    public static TypedSource<bool> Any<TItem>(this IDataIterable<TItem> iterable)
        => iterable.Items.Any();

    /// <summary>Tests that a condition holds for an item of the list.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source that is true when the condition holds for at least one item.</returns>
    public static TypedSource<bool> Any<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, bool>> predicate)
        => iterable.Items.Any(predicate);

    /// <summary>Tests that a condition holds for every item of the list.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition every item is held to.</param>
    /// <returns>A source that is true when the condition holds for every item.</returns>
    public static TypedSource<bool> All<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, bool>> predicate)
        => iterable.Items.All(predicate);

    /// <summary>Finds the first item a condition holds for.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source of the item, which cannot be read when no item matches.</returns>
    public static TypedSource<TItem> Find<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, bool>> predicate)
        => iterable.Items.Find(predicate);

    /// <summary>Finds the first item a condition holds for and reads one field of it.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <param name="field">The field read from the item found.</param>
    /// <returns>A source of the field's value, which cannot be read when no item matches.</returns>
    public static TypedSource<TResult> Find<TItem, TResult>(this IDataIterable<TItem> iterable, Expression<Func<TItem, bool>> predicate, Expression<Func<TItem, TResult>> field)
        => iterable.Items.Find(predicate, field);

    /// <summary>Orders the list's items by a key, the smallest first and null before every value.</summary>
    /// <remarks>
    /// The order is the one .NET gives the key's type, and items with equal keys keep their order. A text key is
    /// ordered by the culture the view renders under. The list itself is left as it is.
    /// </remarks>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem, TKey>(this IDataIterable<TItem> iterable, Expression<Func<TItem, TKey>> key)
        => iterable.Items.OrderBy(key);

    /// <summary>Orders the list's items by a key that holds null, the smallest first and null before every value.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem, TKey>(this IDataIterable<TItem> iterable, Expression<Func<TItem, TKey?>> key)
        where TKey : struct
        => iterable.Items.OrderBy(key);

    /// <summary>Orders the list's items by a text key, compared as the comparer compares.</summary>
    /// <remarks>
    /// <see cref="StringComparer.Ordinal"/> orders by UTF-16 code unit, and <see cref="StringComparer.CurrentCulture"/>
    /// by the culture the view renders under, as no comparer does. The build refuses any other comparer.
    /// </remarks>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="key">The text read from each item.</param>
    /// <param name="comparer">How two texts are compared.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, string?>> key, StringComparer comparer)
        => iterable.Items.OrderBy(key, comparer);

    /// <summary>Orders the list's items by a key, the greatest first and null after every value.</summary>
    /// <remarks>
    /// The order is the reverse of the one .NET gives the key's type, and items with equal keys keep their order.
    /// A text key is ordered by the culture the view renders under. The list itself is left as it is.
    /// </remarks>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem, TKey>(this IDataIterable<TItem> iterable, Expression<Func<TItem, TKey>> key)
        => iterable.Items.OrderByDescending(key);

    /// <summary>Orders the list's items by a key that holds null, the greatest first and null after every value.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem, TKey>(this IDataIterable<TItem> iterable, Expression<Func<TItem, TKey?>> key)
        where TKey : struct
        => iterable.Items.OrderByDescending(key);

    /// <summary>Orders the list's items by a text key, in the reverse of the comparer's order.</summary>
    /// <remarks>
    /// <see cref="StringComparer.Ordinal"/> orders by UTF-16 code unit, and <see cref="StringComparer.CurrentCulture"/>
    /// by the culture the view renders under, as no comparer does. The build refuses any other comparer.
    /// </remarks>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="key">The text read from each item.</param>
    /// <param name="comparer">How two texts are compared.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, string?>> key, StringComparer comparer)
        => iterable.Items.OrderByDescending(key, comparer);

    /// <summary>Adds a whole number read from every item.</summary>
    /// <remarks>A total past the range of <see cref="int"/> stops the pipeline, as .NET's checked sum refuses it.</remarks>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="term">The number read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<int> Sum<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, int>> term)
        => iterable.Items.Sum(term);

    /// <summary>Adds a number read from every item.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="term">The number read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<double> Sum<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, double>> term)
        => iterable.Items.Sum(term);

    /// <summary>Adds a decimal read from every item, exactly, as .NET adds decimals.</summary>
    /// <param name="iterable">The object whose iterator yields the items.</param>
    /// <param name="term">The decimal read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<decimal> Sum<TItem>(this IDataIterable<TItem> iterable, Expression<Func<TItem, decimal>> term)
        => iterable.Items.Sum(term);
}
