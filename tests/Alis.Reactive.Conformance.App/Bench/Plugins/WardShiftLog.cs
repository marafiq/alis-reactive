using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Plugins/ward-shift-log", "WardShiftLog")]
public sealed class WardShiftLog : Plugin
{
    public WardShiftLog()
        : base("shiftlog")
    {
    }

    public ReadOnly<int> Entries => ReadOnly<int>();
}
