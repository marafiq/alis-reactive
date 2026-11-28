using System;
using System.Collections.Generic;

namespace Alis.Reactive.Types;

internal static class ComparedTypes
{
    private static readonly Dictionary<Type, (int Bytes, bool Signed)> WholeNumbers = new()
    {
        [typeof(sbyte)] = (1, true),
        [typeof(sbyte?)] = (1, true),
        [typeof(byte)] = (1, false),
        [typeof(byte?)] = (1, false),
        [typeof(short)] = (2, true),
        [typeof(short?)] = (2, true),
        [typeof(ushort)] = (2, false),
        [typeof(ushort?)] = (2, false),
        [typeof(int)] = (4, true),
        [typeof(int?)] = (4, true),
        [typeof(uint)] = (4, false),
        [typeof(uint?)] = (4, false),
        [typeof(long)] = (8, true),
        [typeof(long?)] = (8, true),
        [typeof(ulong)] = (8, false),
        [typeof(ulong?)] = (8, false),
    };

    private static readonly HashSet<Type> Fractions =
    [
        typeof(float), typeof(float?), typeof(double), typeof(double?), typeof(decimal), typeof(decimal?),
    ];

    private static readonly HashSet<Type> Chars = [typeof(char), typeof(char?)];

    private static readonly HashSet<Type> Decimals = [typeof(decimal), typeof(decimal?)];

    private static readonly HashSet<Type> OtherScalars = [typeof(string), typeof(bool), typeof(bool?), typeof(Guid), typeof(Guid?)];

    private static readonly HashSet<Type> Booleans = [typeof(bool), typeof(bool?)];

    private static readonly HashSet<Type> Texts =
    [
        typeof(string), typeof(char), typeof(char?), typeof(Guid), typeof(Guid?), typeof(TimeSpan), typeof(TimeSpan?),
#if NET
        typeof(DateOnly), typeof(DateOnly?), typeof(TimeOnly), typeof(TimeOnly?),
#endif
    ];

    private static readonly HashSet<Type> DatesAndTimes =
    [
        typeof(DateTime), typeof(DateTime?), typeof(DateTimeOffset), typeof(DateTimeOffset?), typeof(TimeSpan), typeof(TimeSpan?),
#if NET
        typeof(DateOnly), typeof(DateOnly?), typeof(TimeOnly), typeof(TimeOnly?),
#endif
    ];

    private static readonly HashSet<Type> ScalarsThatAdmitNothing =
    [
        typeof(string), typeof(sbyte?), typeof(byte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?),
        typeof(long?), typeof(ulong?), typeof(float?), typeof(double?), typeof(decimal?), typeof(bool?), typeof(char?),
        typeof(Guid?), typeof(TimeSpan?), typeof(DateTime?), typeof(DateTimeOffset?),
#if NET
        typeof(DateOnly?), typeof(TimeOnly?),
#endif
    ];

    internal static bool IsChar(Type type) => Chars.Contains(type);

    internal static bool AdmitsNothing(Type type) => ScalarsThatAdmitNothing.Contains(type);

    internal static bool IsBoolean(Type type) => Booleans.Contains(type);

    internal static bool IsText(Type type) => Texts.Contains(type);

    internal static bool IsDecimal(Type type) => Decimals.Contains(type);

    internal static bool IsDateOrTime(Type type) => DatesAndTimes.Contains(type);

    internal static bool Narrows(Type from, Type to)
    {
        if (!WholeNumbers.TryGetValue(to, out var whole)) return false;
        if (Fractions.Contains(from)) return true;
        if (!WholeNumbers.TryGetValue(from, out var cast)) return false;
        var holdsEvery = cast.Signed == whole.Signed ? whole.Bytes >= cast.Bytes : !cast.Signed && whole.Bytes > cast.Bytes;
        return !holdsEvery;
    }

    internal static bool IsNumber(Type type) => WholeNumbers.ContainsKey(type) || Fractions.Contains(type);

    internal static bool IsFraction(Type type) => Fractions.Contains(type);

    internal static bool IsScalar(Type type) => IsNumber(type) || Chars.Contains(type) || OtherScalars.Contains(type);
}
