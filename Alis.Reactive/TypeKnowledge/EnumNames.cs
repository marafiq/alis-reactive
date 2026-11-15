using System;

namespace Alis.Reactive.TypeKnowledge;

internal static class EnumNames
{
    internal static bool NamesFlags<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var named = value.ToString();
        return named.Length > 0 && named[0] != '-' && !char.IsDigit(named[0]);
    }
}
