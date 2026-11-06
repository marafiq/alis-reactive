using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Conditions;
using Alis.Reactive.Plan;
using Alis.Reactive.Types;
using Alis.Reactive.Values;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Lists;

public static class DataLists
{
    private const string TextIsNoList = "A text is not a list in the browser. A list's operations take a list, such as a List<T> or an array.";

    public static TypedSource<int> Count<TItem>(this ITypedSource<IEnumerable<TItem>> list)
        => Over<TItem, int>(list, (items, _) => ListForms.Length(items));

    public static TypedSource<int> Count<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => Over<TItem, int>(list, (items, build) => ListForms.Length(ListForms.Filter(items, ItemRead(build, predicate, nameof(predicate)))));

    public static TypedSource<IReadOnlyList<TItem>> Where<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> keep)
        => Over<TItem, IReadOnlyList<TItem>>(list, (items, build) => ListForms.Filter(items, ItemRead(build, keep, nameof(keep))));

    public static TypedSource<IReadOnlyList<TResult>> Select<TItem, TResult>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TResult>> field)
        => Over<TItem, IReadOnlyList<TResult>>(list, (items, build) => ListForms.Map(items, ItemRead(build, field, nameof(field))));

    public static TypedSource<bool> Any<TItem>(this ITypedSource<IEnumerable<TItem>> list)
        => Over<TItem, bool>(list, (items, build) => ListForms.Some(items, ListForms.EveryItem(build.ItemName())));

    public static TypedSource<bool> Any<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => Over<TItem, bool>(list, (items, build) => ListForms.Some(items, ItemRead(build, predicate, nameof(predicate))));

    public static TypedSource<bool> All<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => Over<TItem, bool>(list, (items, build) => ListForms.Every(items, ItemRead(build, predicate, nameof(predicate))));

    public static TypedSource<TItem> Find<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => Over<TItem, TItem>(list, (items, build) => ListForms.Find(items, ItemRead(build, predicate, nameof(predicate))));

    public static TypedSource<TResult> Find<TItem, TResult>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate, Expression<Func<TItem, TResult>> field)
        => Over<TItem, TResult>(list, (items, build) => new GetExpression(ListForms.Find(items, ItemRead(build, predicate, nameof(predicate))), JsonPath.Of(field, build.Values), ReadPresence.Optional));

    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey>> key)
        => Ordered(list, key, default(TKey), descending: false);

    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey?>> key)
        where TKey : struct
        => Ordered(list, key, default(TKey), descending: false);

    public static TypedSource<IReadOnlyList<TItem>> OrderBy<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, string?>> key, StringComparer comparer)
        => OrderedText(list, key, comparer, descending: false);

    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey>> key)
        => Ordered(list, key, default(TKey), descending: true);

    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem, TKey>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TKey?>> key)
        where TKey : struct
        => Ordered(list, key, default(TKey), descending: true);

    public static TypedSource<IReadOnlyList<TItem>> OrderByDescending<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, string?>> key, StringComparer comparer)
        => OrderedText(list, key, comparer, descending: true);

    public static TypedSource<int> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, int>> term)
        => Over<TItem, int>(list, (items, build) => ListForms.Sum(Operator.SumInt, items, ItemRead(build, term, nameof(term))));

    public static TypedSource<double> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, double>> term)
        => Over<TItem, double>(list, (items, build) => ListForms.Sum(Operator.SumDouble, items, ItemRead(build, term, nameof(term))));

    public static TypedSource<decimal> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, decimal>> term)
        => Over<TItem, decimal>(list, (items, build) => ListForms.Sum(Operator.SumDecimal, items, ItemRead(build, term, nameof(term))));

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<int> Count(this ITypedSource<string> text) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<int> Count(this ITypedSource<string> text, Expression<Func<char, bool>> predicate) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<IReadOnlyList<char>> Where(this ITypedSource<string> text, Expression<Func<char, bool>> keep) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<IReadOnlyList<TResult>> Select<TResult>(this ITypedSource<string> text, Expression<Func<char, TResult>> field) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<bool> Any(this ITypedSource<string> text) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<bool> Any(this ITypedSource<string> text, Expression<Func<char, bool>> predicate) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<bool> All(this ITypedSource<string> text, Expression<Func<char, bool>> predicate) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<char> Find(this ITypedSource<string> text, Expression<Func<char, bool>> predicate) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<TResult> Find<TResult>(this ITypedSource<string> text, Expression<Func<char, bool>> predicate, Expression<Func<char, TResult>> field) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<int> Sum(this ITypedSource<string> text, Expression<Func<char, int>> term) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<double> Sum(this ITypedSource<string> text, Expression<Func<char, double>> term) => throw new NotSupportedException(TextIsNoList);

    [Obsolete(TextIsNoList, error: true)]
    public static TypedSource<decimal> Sum(this ITypedSource<string> text, Expression<Func<char, decimal>> term) => throw new NotSupportedException(TextIsNoList);

    private static TypedSource<TResult> Over<TItem, TResult>(ITypedSource<IEnumerable<TItem>> list, Func<Plan.Expression, PlanBuild, Plan.Expression> operation)
    {
        var source = Sources.Of(list);
        return new DataRead<TResult>(operation(source.Expression, source.Build), source.Build);
    }

    private static TypedSource<IReadOnlyList<TItem>> Ordered<TItem, TKey>(ITypedSource<IEnumerable<TItem>> list, LambdaExpression key, TKey? sample, bool descending)
        => Over<TItem, IReadOnlyList<TItem>>(list, (items, build) => ListOrder.Sorted(items, build, key, ListOrder.ComparisonOf(key, sample, build.Values), descending));

    private static TypedSource<IReadOnlyList<TItem>> OrderedText<TItem>(ITypedSource<IEnumerable<TItem>> list, LambdaExpression key, StringComparer comparer, bool descending)
        => Over<TItem, IReadOnlyList<TItem>>(list, (items, build) => ListOrder.Sorted(items, build, key, ListOrder.OrdinalComparison(key, comparer), descending));

    private static FnExpression ItemRead(PlanBuild build, LambdaExpression read, string argument)
    {
        var item = build.ItemName();
        return new FnExpression([item], LambdaCondition.Of(read, new RefExpression(item), build.Values, argument, build.ItemName));
    }
}
