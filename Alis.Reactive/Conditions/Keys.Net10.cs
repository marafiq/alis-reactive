#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Conditions;

internal static partial class Keys
{
    static partial void AddTheHostsOwn(Dictionary<Type, Operator> keys)
    {
        keys[typeof(DateOnly)] = Operator.DayKey;
        keys[typeof(DateOnly?)] = Operator.DayKey;
        keys[typeof(TimeOnly)] = Operator.TimeKey;
        keys[typeof(TimeOnly?)] = Operator.TimeKey;
    }
}
#endif
