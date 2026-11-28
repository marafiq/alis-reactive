using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Plugins/lantern-of-amber", "Lantern")]
public sealed class Lantern : Plugin
{
    public Lantern()
        : base("lantern")
    {
    }

    public ReadOnly<string> Colour => ReadOnly<string>();
}
