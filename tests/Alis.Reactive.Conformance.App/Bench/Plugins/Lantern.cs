using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class Lantern : Plugin
{
    public Lantern()
        : base("lantern")
    {
    }

    public ReadOnly<string> Colour => ReadOnly<string>();
}
