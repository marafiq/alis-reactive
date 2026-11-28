using System;
using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.TypeKnowledge;

internal static class EnumNames
{
    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's exception, 2026-11-26: an enum is flags to the framework when its own [Flags] marks it, the attribute FluentValidation's IsInEnum and System.Text.Json read, so the core reads it here once and the app names no flags enum at startup (D18.1, N16.4).")]
    internal static bool NamesFlags(Type enumeration) => enumeration.IsDefined(typeof(FlagsAttribute), inherit: false);

    internal static bool Names<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        if (!NamesFlags(typeof(TEnum))) return Enum.IsDefined(typeof(TEnum), value);
        var named = value.ToString();
        return named.Length > 0 && named[0] != '-' && !char.IsDigit(named[0]);
    }
}
