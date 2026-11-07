using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.Plan;
using Alis.Reactive.Writing;

namespace Alis.Reactive.Lists;

internal static class ListOrder
{
    private static readonly HashSet<Type> DatesAndTimes =
    [
        typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
#if NET
        typeof(DateOnly), typeof(TimeOnly),
#endif
    ];

    internal static Operator ComparisonOf<TKey>(LambdaExpression key, TKey? sample, JsonSerializerOptions values, CultureInfo culture)
    {
        if (DatesAndTimes.Contains(typeof(TKey))) throw Refused(key, "orders dates or times, whose text does not order as the instant does; order them on the server.", nameof(key));
        if (typeof(TKey) == typeof(string)) return TextOrder(key, culture, nameof(key));
        var written = JsonSerializer.SerializeToElement(sample, values.GetTypeInfo(typeof(TKey)));
        return written.ValueKind switch
        {
            JsonValueKind.Number => Operator.CompareNumbers,
            JsonValueKind.True or JsonValueKind.False => Operator.CompareBooleans,
            JsonValueKind.String when typeof(TKey) == typeof(char) => Operator.CompareOrdinal,
            JsonValueKind.String => throw Refused(key, "orders a value the app's JSON writes as text, which does not order as the value does in .NET; order by a member the JSON writes as a number, or order on the server.", nameof(key)),
            _ => throw OrdersObjects(key),
        };
    }

    internal static Operator TextComparison(LambdaExpression key, StringComparer comparer, CultureInfo culture)
    {
        if (ReferenceEquals(comparer, StringComparer.Ordinal)) return Operator.CompareOrdinal;
        var ordersByTheViewsCulture = comparer.Equals(StringComparer.Create(culture, ignoreCase: false));
        if (ordersByTheViewsCulture) return TextOrder(key, culture, nameof(comparer));
        throw Refused(key, "orders text by a comparer the browser does not have; pass StringComparer.Ordinal, or no comparer to order by the view's culture, or order on the server.", nameof(comparer));
    }

    private static Operator TextOrder(LambdaExpression key, CultureInfo culture, string argument)
    {
        var ordersByCodeUnit = culture.CompareInfo.Compare("a", "B", CompareOptions.None) > 0;
        if (ordersByCodeUnit) return Operator.CompareOrdinal;
        var sortsAlternately = !string.Equals(culture.CompareInfo.Name, culture.Name, StringComparison.Ordinal);
        if (sortsAlternately) throw Refused(key, $"orders text by the alternate sort '{culture.CompareInfo.Name}', which the browser has no tag for; pass StringComparer.Ordinal, or order on the server.", argument);
        return Operator.CompareCulture;
    }

    internal static Plan.Expression Sorted(Plan.Expression items, PlanBuild build, LambdaExpression key, Operator comparison, bool descending)
    {
        var left = build.ItemName();
        var right = build.ItemName();
        var leftKey = LambdaCondition.Of(key, new RefExpression(left), build.Starts, nameof(key), build.ItemName, LambdaUse.Computes);
        var rightKey = LambdaCondition.Of(key, new RefExpression(right), build.Starts, nameof(key), build.ItemName, LambdaUse.Computes);
        Plan.Expression[] keys = descending ? [rightKey, leftKey] : [leftKey, rightKey];
        return ListForms.Sorted(items, new FnExpression([left, right], new OperatorExpression(comparison, keys)));
    }

    private static ArgumentException OrdersObjects(LambdaExpression key)
        => Refused(key, "orders objects, which the browser does not order; order by a member, such as OrderBy(d => d.Carer.Name, StringComparer.Ordinal).", nameof(key));

    private static ArgumentException Refused(LambdaExpression key, string reason, string argument)
        => new($"'{ViewValues.Printed(key.Body)}' {reason}", argument);
}
