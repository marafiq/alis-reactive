using System;

namespace Alis.Reactive.Components;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MirrorsAttribute : Attribute
{
    public MirrorsAttribute(string module, string name)
    {
        Module = module;
        Name = name;
    }

    public string Module { get; }

    public string Name { get; }
}
