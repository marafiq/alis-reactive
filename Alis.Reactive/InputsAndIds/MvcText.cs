using System;
using System.Linq.Expressions;

namespace Alis.Reactive.InputsAndIds;

internal static partial class MvcText
{
    internal static partial string Of<TModel, TValue>(Expression<Func<TModel, TValue>> member);

    internal static partial string IdFromName(string name);
}
