using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.ArrayLambdas;

internal static class ListOrder
{
    internal static Operator ComparisonOf<TKey>(LambdaExpression key, HeldForm form)
    {
        if (typeof(TKey) == typeof(string)) return TextOrder(key, form.RendersUnder);
        if (Keys.IsDateOrTime<TKey>()) return Operator.CompareOrdinal;
        return form.Beside(default(TKey)).Value.ValueKind switch
        {
            JsonValueKind.Number => Operator.CompareNumbers,
            JsonValueKind.True or JsonValueKind.False => Operator.CompareBooleans,
            JsonValueKind.String when ComparedTypes.IsChar(typeof(TKey)) => Operator.CompareOrdinal,
            JsonValueKind.String => throw Refused(key, "orders a value the app's JSON writes as text, which does not order as the value does in .NET; order by a member the JSON writes as a number, or order on the server."),
            _ => throw Refused(key, "orders objects, which the browser does not order; order by a member, such as OrderBy(d => d.Carer.Name, StringComparer.Ordinal)."),
        };
    }

    internal static Operator TextComparison(LambdaExpression key, StringComparer comparer, HeldForm form)
    {
        if (ReferenceEquals(comparer, StringComparer.Ordinal)) return Operator.CompareOrdinal;
        var ordersByTheViewsCulture = comparer.Equals(StringComparer.Create(form.RendersUnder, ignoreCase: false));
        if (ordersByTheViewsCulture) return TextOrder(key, form.RendersUnder);
        throw Refused(key, "orders text by a comparer the browser does not have; pass StringComparer.Ordinal, or no comparer to order by the view's culture, or order on the server.");
    }

    private static Operator TextOrder(LambdaExpression key, CultureInfo culture)
    {
        var ordersByCodeUnit = culture.CompareInfo.Compare("a", "B", CompareOptions.None) > 0;
        if (ordersByCodeUnit) return Operator.CompareOrdinal;
        var sortsAlternately = !string.Equals(culture.CompareInfo.Name, culture.Name, StringComparison.Ordinal);
        if (sortsAlternately) throw Refused(key, $"orders text by the alternate sort '{culture.CompareInfo.Name}', which the browser has no tag for; pass StringComparer.Ordinal, or order on the server.");
        return Operator.CompareCulture;
    }

    private static InvalidOperationException Refused(LambdaExpression key, string reason) => new($"'{Printed.Of(key.Body)}' {reason}");
}
