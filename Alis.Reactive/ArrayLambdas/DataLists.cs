using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.ValuesAndSources;

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
    private const string TextIsNoList = "A text is not a list in the browser. A list's operations take a list, such as a List<T> or an array.";

    /// <summary>Counts the list's items.</summary>
    /// <returns>A source of the number of items.</returns>
    public static TypedSource<int> Count<TItem>(this ITypedSource<IEnumerable<TItem>> list) => throw new NotImplementedException();

    /// <summary>Counts the items a condition holds for.</summary>
    /// <param name="list">The list to count in.</param>
    /// <param name="predicate">The condition an item is counted by.</param>
    /// <returns>A source of the number of items the condition holds for.</returns>
    public static TypedSource<int> Count<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate) => throw new NotImplementedException();

    /// <summary>Keeps the items a condition holds for.</summary>
    /// <param name="list">The list to keep items of.</param>
    /// <param name="keep">The condition an item is kept by.</param>
    /// <returns>A source of a new list, in the list's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> Where<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> keep) => throw new NotImplementedException();

    /// <summary>Reads one field of every item.</summary>
    /// <remarks>
    /// A field whose type holds null is null in the new list where an item lacks it. A field whose type holds no
    /// null stops the pipeline where it cannot be read, so the new list holds only values of its type.
    /// </remarks>
    /// <param name="list">The list to read.</param>
    /// <param name="field">The field read from each item.</param>
    /// <returns>A source of a new list of the field's values, in the list's order.</returns>
    public static TypedSource<IReadOnlyList<TResult>> Select<TItem, TResult>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TResult>> field) => throw new NotImplementedException();

    /// <summary>Tests that the list holds an item.</summary>
    /// <returns>A source that is true when the list holds at least one item.</returns>
    public static TypedSource<bool> Any<TItem>(this ITypedSource<IEnumerable<TItem>> list) => throw new NotImplementedException();

    /// <summary>Tests that a condition holds for an item of the list.</summary>
    /// <param name="list">The list to look in.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source that is true when the condition holds for at least one item.</returns>
    public static TypedSource<bool> Any<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate) => throw new NotImplementedException();

    /// <summary>Tests that a condition holds for every item of the list.</summary>
    /// <param name="list">The list to look in.</param>
    /// <param name="predicate">The condition every item is held to.</param>
    /// <returns>A source that is true when the condition holds for every item.</returns>
    public static TypedSource<bool> All<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate) => throw new NotImplementedException();

    /// <summary>Finds the first item a condition holds for.</summary>
    /// <param name="list">The list to look in.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <returns>A source of the item, which cannot be read when no item matches.</returns>
    public static TypedSource<TItem> Find<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate) => throw new NotImplementedException();

    /// <summary>Finds the first item a condition holds for and reads one field of it.</summary>
    /// <param name="list">The list to look in.</param>
    /// <param name="predicate">The condition looked for.</param>
    /// <param name="field">The field read from the item found.</param>
    /// <returns>A source of the field's value, which cannot be read when no item matches.</returns>
    public static TypedSource<TResult> Find<TItem, TResult>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate, Expression<Func<TItem, TResult>> field) => throw new NotImplementedException();

    /// <summary>Orders the list's items by a key, the smallest first and null before every value.</summary>
    /// <remarks>
    /// The order is the one .NET gives the key's type, and items with equal keys keep their order. A text key is
    /// ordered by the culture the view renders under. The list itself is left as it is.
    /// </remarks>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey>> key) => throw new NotImplementedException();

    /// <summary>Orders the list's items by a key that holds null, the smallest first and null before every value.</summary>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey?>> key)
        where TKey : struct
        => throw new NotImplementedException();

    /// <summary>Orders the list's items by a text key, compared as the comparer compares.</summary>
    /// <remarks>
    /// <see cref="StringComparer.Ordinal"/> orders by UTF-16 code unit, and <see cref="StringComparer.CurrentCulture"/>
    /// by the culture the view renders under, as no comparer does. The build refuses any other comparer.
    /// </remarks>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The text read from each item.</param>
    /// <param name="comparer">How two texts are compared.</param>
    /// <returns>A source of a new list in the key's order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, string?>> key, StringComparer comparer) => throw new NotImplementedException();

    /// <summary>Orders the list's items by a key, the greatest first and null after every value.</summary>
    /// <remarks>
    /// The order is the reverse of the one .NET gives the key's type, and items with equal keys keep their order.
    /// A text key is ordered by the culture the view renders under. The list itself is left as it is.
    /// </remarks>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey>> key) => throw new NotImplementedException();

    /// <summary>Orders the list's items by a key that holds null, the greatest first and null after every value.</summary>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The key read from each item.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey?>> key)
        where TKey : struct
        => throw new NotImplementedException();

    /// <summary>Orders the list's items by a text key, in the reverse of the comparer's order.</summary>
    /// <remarks>
    /// <see cref="StringComparer.Ordinal"/> orders by UTF-16 code unit, and <see cref="StringComparer.CurrentCulture"/>
    /// by the culture the view renders under, as no comparer does. The build refuses any other comparer.
    /// </remarks>
    /// <param name="list">The list to order.</param>
    /// <param name="key">The text read from each item.</param>
    /// <param name="comparer">How two texts are compared.</param>
    /// <returns>A source of a new list in the key's reverse order.</returns>
    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, string?>> key, StringComparer comparer) => throw new NotImplementedException();

    /// <summary>Adds a whole number read from every item.</summary>
    /// <remarks>A total past the range of <see cref="int"/> stops the pipeline, as .NET's checked sum refuses it.</remarks>
    /// <param name="list">The list to add over.</param>
    /// <param name="term">The number read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<int> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, int>> term) => throw new NotImplementedException();

    /// <summary>Adds a number read from every item.</summary>
    /// <param name="list">The list to add over.</param>
    /// <param name="term">The number read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<double> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, double>> term) => throw new NotImplementedException();

    /// <summary>Adds a decimal read from every item, exactly, as .NET adds decimals.</summary>
    /// <param name="list">The list to add over.</param>
    /// <param name="term">The decimal read from each item.</param>
    /// <returns>A source of the total.</returns>
    public static TypedSource<decimal> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, decimal>> term) => throw new NotImplementedException();

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<int> Count(this ITypedSource<string> text) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<int> Count(this ITypedSource<string> text, Expression<Func<char, bool>> predicate) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<IReadOnlyList<char>> Where(this ITypedSource<string> text, Expression<Func<char, bool>> keep) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<IReadOnlyList<TResult>> Select<TResult>(this ITypedSource<string> text, Expression<Func<char, TResult>> field) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<bool> Any(this ITypedSource<string> text) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<bool> Any(this ITypedSource<string> text, Expression<Func<char, bool>> predicate) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<bool> All(this ITypedSource<string> text, Expression<Func<char, bool>> predicate) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<char> Find(this ITypedSource<string> text, Expression<Func<char, bool>> predicate) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<TResult> Find<TResult>(this ITypedSource<string> text, Expression<Func<char, bool>> predicate, Expression<Func<char, TResult>> field) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<int> Sum(this ITypedSource<string> text, Expression<Func<char, int>> term) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<double> Sum(this ITypedSource<string> text, Expression<Func<char, double>> term) => throw new NotSupportedException(TextIsNoList);

    /// <summary>Refuses a text read as a list of its characters: a text is not a list in the browser.</summary>
    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<decimal> Sum(this ITypedSource<string> text, Expression<Func<char, decimal>> term) => throw new NotSupportedException(TextIsNoList);
}
