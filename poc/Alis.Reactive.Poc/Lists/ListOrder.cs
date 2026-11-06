using System;
using System.Collections.Generic;
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

    internal static Operator ComparisonOf<TKey>(LambdaExpression key, TKey? sample, JsonSerializerOptions values)
    {
        if (DatesAndTimes.Contains(typeof(TKey))) throw Refused(key, "orders dates or times, whose text does not order as the instant does; order them on the server.", nameof(key));
        if (typeof(TKey) == typeof(string)) throw Refused(key, "orders text by the culture in .NET, an order net48 and net10 do not share; pass StringComparer.Ordinal, such as OrderBy(d => d.Prescriber, StringComparer.Ordinal), or order on the server.", nameof(key));
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

    internal static Operator OrdinalComparison(LambdaExpression key, StringComparer comparer)
    {
        if (!ReferenceEquals(comparer, StringComparer.Ordinal)) throw Refused(key, "orders text by a comparer the browser does not have; pass StringComparer.Ordinal, or order on the server.", nameof(comparer));
        return Operator.CompareOrdinal;
    }

    internal static Plan.Expression Sorted(Plan.Expression items, PlanBuild build, LambdaExpression key, Operator comparison, bool descending)
    {
        var left = build.ItemName();
        var right = build.ItemName();
        var leftKey = LambdaCondition.Of(key, new RefExpression(left), build.Values, nameof(key), build.ItemName);
        var rightKey = LambdaCondition.Of(key, new RefExpression(right), build.Values, nameof(key), build.ItemName);
        Plan.Expression[] keys = descending ? [rightKey, leftKey] : [leftKey, rightKey];
        return ListForms.Sorted(items, new FnExpression([left, right], new OperatorExpression(comparison, keys)));
    }

    private static ArgumentException OrdersObjects(LambdaExpression key)
        => Refused(key, "orders objects, which the browser does not order; order by a member, such as OrderBy(d => d.Carer.Name, StringComparer.Ordinal).", nameof(key));

    private static ArgumentException Refused(LambdaExpression key, string reason, string argument)
        => new($"'{ViewValues.Printed(key.Body)}' {reason}", argument);
}
