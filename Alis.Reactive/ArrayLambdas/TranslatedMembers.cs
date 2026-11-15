using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Alis.Reactive.ArrayLambdas;

[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "The owner's choice, 2026-11-06: the .NET members a list's lambda reads through are one closed table keyed by the definition the expression tree carries, its module and metadata token, built from typed lambdas and never listed or invoked (D6.5, D6.12, D18.1).")]
internal static class TranslatedMembers
{
    private static readonly HashSet<(Module Module, int Token)> ExactDecimalConversions =
    [
        Conversion((sbyte value) => (decimal)value),
        Conversion((byte value) => (decimal)value),
        Conversion((short value) => (decimal)value),
        Conversion((ushort value) => (decimal)value),
        Conversion((int value) => (decimal)value),
        Conversion((uint value) => (decimal)value),
        Conversion((long value) => (decimal)value),
        Conversion((ulong value) => (decimal)value),
        Conversion((char value) => (decimal)value),
    ];

    internal static bool IsAnExactDecimalConversion(UnaryExpression conversion) => ExactDecimalConversions.Contains(Key(conversion.Method!));

    private static (Module Module, int Token) Key(MemberInfo member) => (member.Module, member.MetadataToken);

    private static (Module Module, int Token) Conversion<T, TResult>(Expression<Func<T, TResult>> cast) => Key(((UnaryExpression)cast.Body).Method!);
}
