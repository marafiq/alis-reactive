using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Alis.Reactive.Validation;

internal static class ServerExpressions
{
    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Gate 1 C13 permits compiling validation's member reads, conditions and fixed operands in one file (D18.1).")]
    internal static Func<T, TValue> Read<T, TValue>(Expression<Func<T, TValue>> expression) => expression.Compile();
}
