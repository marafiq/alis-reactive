using System;
using System.Collections.Generic;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conditions;

internal static partial class Keys
{
    private static readonly Dictionary<Type, Operator> OfType = Written();

    private static readonly HashSet<Type> DateTimes = [typeof(DateTime), typeof(DateTime?)];

    internal static bool IsDateOrTime<TValue>() => IsDateOrTime(typeof(TValue));

    internal static bool IsDateOrTime(Type type) => OfType.ContainsKey(type);

    internal static bool KeyedAlike(Type one, Type other)
        => OfType.TryGetValue(one, out var key) && OfType.TryGetValue(other, out var otherKey) && ReferenceEquals(key, otherKey);

    internal static Expression Of<TValue>(Expression operand, HeldForm form) => Of(typeof(TValue), operand, form);

    internal static Expression Of(Type type, Expression operand, HeldForm form)
    {
        if (IsDateOrTime(type) && form.ReadTheAppsConverterWrites is { } read) throw ComparesADateTheAppsConverterWrites(read, type);
        if (DateTimes.Contains(type)) return new OpExpression(form.KeyOfADateTime, form.KeyArguments(operand));
        return OfType.TryGetValue(type, out var key) ? new OpExpression(key, form.KeyArguments(operand)) : operand;
    }

    internal static bool WrittenByAConverterOfTheApps(Type type, HeldForm form) => IsDateOrTime(type) && form.WritesByAConverterOfTheApps(type);

    internal static InvalidOperationException ComparesADateTheAppsConverterWrites(string term, Type type)
        => new($"'{term}' compares a {ClassNames.AsCSharpWritesIt(type)} the app's JSON options write by a converter of their own, whose text the browser cannot read as the value .NET holds; compare it on the server.");

    private static Dictionary<Type, Operator> Written()
    {
        var keys = new Dictionary<Type, Operator>
        {
            [typeof(DateTime)] = Operator.DateKey,
            [typeof(DateTime?)] = Operator.DateKey,
            [typeof(DateTimeOffset)] = Operator.InstantKey,
            [typeof(DateTimeOffset?)] = Operator.InstantKey,
            [typeof(TimeSpan)] = Operator.SpanKey,
            [typeof(TimeSpan?)] = Operator.SpanKey,
        };
        AddTheHostsOwn(keys);
        return keys;
    }

    static partial void AddTheHostsOwn(Dictionary<Type, Operator> keys);
}
