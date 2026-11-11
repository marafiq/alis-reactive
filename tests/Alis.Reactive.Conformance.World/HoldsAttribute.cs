using System;

namespace Alis.Reactive.Conformance.World;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class HoldsAttribute(object gate) : Attribute
{
    public object Gate { get; } = gate;
}
