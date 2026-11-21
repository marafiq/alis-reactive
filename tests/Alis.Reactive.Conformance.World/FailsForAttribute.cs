using System;

namespace Alis.Reactive.Conformance.World;

[AttributeUsage(AttributeTargets.Method)]
public sealed class FailsForAttribute(string promise) : Attribute
{
    public string Promise { get; } = promise;
}
