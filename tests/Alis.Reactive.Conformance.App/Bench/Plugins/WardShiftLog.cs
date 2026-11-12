using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class WardShiftLog : Plugin
{
    public WardShiftLog()
        : base("shiftlog")
    {
    }

    public ReadOnly<int> Entries => ReadOnly<int>();
}
