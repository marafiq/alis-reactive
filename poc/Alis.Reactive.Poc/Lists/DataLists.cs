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
        => Over<TItem, int>(list, (items, _, _) => ListForms.Length(items));

    public static TypedSource<int> Count<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => Over<TItem, int>(list, (items, build, form) => ListForms.Length(ListForms.Filter(items, ItemRead(build, predicate, nameof(predicate), LambdaUse.Computes, form))));

    public static TypedSource<IReadOnlyList<TItem>> Where<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> keep)
        => OverItems<TItem, IReadOnlyList<TItem>>(list, (items, build, form) => ListForms.Filter(items, ItemRead(build, keep, nameof(keep), LambdaUse.Computes, form)));

    public static TypedSource<IReadOnlyList<TResult>> Select<TItem, TResult>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, TResult>> field)
    {
        var source = Sources.Of(list);
        var build = source.Build;
        var item = build.ItemName();
        var taken = LambdaCondition.Taken(field, new RefExpression(item), source.Form, build.Starts, nameof(field), build.ItemName, LambdaUse.Takes);
        return new DataRead<IReadOnlyList<TResult>>(ListForms.Map(source.Expression, new FnExpression([item], taken.Body)), build, taken.Form);
    }

    public static TypedSource<bool> Any<TItem>(this ITypedSource<IEnumerable<TItem>> list)
        => Over<TItem, bool>(list, (items, build, form) => ListForms.Some(items, ListForms.EveryItem(build.ItemName())));

    public static TypedSource<bool> Any<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => Over<TItem, bool>(list, (items, build, form) => ListForms.Some(items, ItemRead(build, predicate, nameof(predicate), LambdaUse.Computes, form)));

    public static TypedSource<bool> All<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => Over<TItem, bool>(list, (items, build, form) => ListForms.Every(items, ItemRead(build, predicate, nameof(predicate), LambdaUse.Computes, form)));

    public static TypedSource<TItem> Find<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate)
        => OverItems<TItem, TItem>(list, (items, build, form) => ListForms.Find(items, ItemRead(build, predicate, nameof(predicate), LambdaUse.Computes, form)));

    public static TypedSource<TResult> Find<TItem, TResult>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, bool>> predicate, Expression<Func<TItem, TResult>> field)
    {
        var source = Sources.Of(list);
        var build = source.Build;
        var found = ListForms.Find(source.Expression, ItemRead(build, predicate, nameof(predicate), LambdaUse.Computes, source.Form));
        var read = JsonPath.Of(field, source.Form, build.Starts, nameof(field));
        return new DataRead<TResult>(new DataExpression(found, read.Steps), build, read.Form);
    }

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
        => Over<TItem, int>(list, (items, build, form) => ListForms.Sum(Operator.SumInt, items, ItemRead(build, term, nameof(term), LambdaUse.Computes, form)));

    public static TypedSource<double> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, double>> term)
        => Over<TItem, double>(list, (items, build, form) => ListForms.Sum(Operator.SumDouble, items, ItemRead(build, term, nameof(term), LambdaUse.Computes, form)));

    public static TypedSource<decimal> Sum<TItem>(this ITypedSource<IEnumerable<TItem>> list, Expression<Func<TItem, decimal>> term)
        => Over<TItem, decimal>(list, (items, build, form) => ListForms.Sum(Operator.SumDecimal, items, ItemRead(build, term, nameof(term), LambdaUse.SumsDecimal, form)));

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

    private static TypedSource<TResult> Over<TItem, TResult>(ITypedSource<IEnumerable<TItem>> list, Func<Plan.Expression, PlanBuild, HeldForm, Plan.Expression> operation)
    {
        var source = Sources.Of(list);
        return new DataRead<TResult>(operation(source.Expression, source.Build, source.Form), source.Build);
    }

    private static TypedSource<TResult> OverItems<TItem, TResult>(ITypedSource<IEnumerable<TItem>> list, Func<Plan.Expression, PlanBuild, HeldForm, Plan.Expression> operation)
    {
        var source = Sources.Of(list);
        return new DataRead<TResult>(operation(source.Expression, source.Build, source.Form), source.Build, source.Form);
    }

    private static TypedSource<IReadOnlyList<TItem>> Ordered<TItem, TKey>(ITypedSource<IEnumerable<TItem>> list, LambdaExpression key, TKey? sample, bool descending)
        => OverItems<TItem, IReadOnlyList<TItem>>(list, (items, build, form) => ListOrder.Sorted(items, form, build, key, ListOrder.ComparisonOf(key, sample, build.Values, build.Culture), descending));

    private static TypedSource<IReadOnlyList<TItem>> OrderedText<TItem>(ITypedSource<IEnumerable<TItem>> list, LambdaExpression key, StringComparer comparer, bool descending)
        => OverItems<TItem, IReadOnlyList<TItem>>(list, (items, build, form) => ListOrder.Sorted(items, form, build, key, ListOrder.TextComparison(key, comparer, build.Culture), descending));

    private static FnExpression ItemRead(PlanBuild build, LambdaExpression read, string argument, LambdaUse use, HeldForm itemForm)
    {
        var item = build.ItemName();
        return new FnExpression([item], LambdaCondition.Of(read, new RefExpression(item), itemForm, build.Starts, argument, build.ItemName, use));
    }
}
