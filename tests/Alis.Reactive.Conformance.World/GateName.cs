using System;

namespace Alis.Reactive.Conformance.World;

public static class GateName
{
    public static string Of<TGate>(TGate gate)
        where TGate : struct, Enum
        => typeof(TGate).Name + "." + gate;
}
