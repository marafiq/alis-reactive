using System;
using System.Collections.Generic;

namespace Alis.Reactive.ArrayLambdas;

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

    private static readonly HashSet<Type> Fractions = [typeof(float), typeof(float?), typeof(double), typeof(double?), typeof(decimal), typeof(decimal?)];

    private static readonly HashSet<Type> BinaryFractions = [typeof(float), typeof(float?), typeof(double), typeof(double?)];

    private static readonly HashSet<Type> Decimals = [typeof(decimal), typeof(decimal?)];

    private static readonly HashSet<Type> Chars = [typeof(char), typeof(char?)];

    internal static bool IsNumber(Type type) => WholeNumbers.ContainsKey(type) || Fractions.Contains(type);

    internal static bool IsChar(Type type) => Chars.Contains(type);

    internal static bool IsDecimal(Type type) => Decimals.Contains(type);

    internal static bool IsBinaryFraction(Type type) => BinaryFractions.Contains(type);

    internal static bool Narrows(Type from, Type to)
    {
        if (!WholeNumbers.TryGetValue(to, out var whole)) return false;
        if (Fractions.Contains(from)) return true;
        if (!WholeNumbers.TryGetValue(from, out var cast)) return false;
        var holdsEvery = cast.Signed == whole.Signed ? whole.Bytes >= cast.Bytes : !cast.Signed && whole.Bytes > cast.Bytes;
        return !holdsEvery;
    }
}
