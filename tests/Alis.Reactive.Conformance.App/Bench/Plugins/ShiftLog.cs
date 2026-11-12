using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class ShiftLog : Plugin
{
    public ShiftLog()
        : base("shiftlog")
    {
    }

    public Property<int> Entries => Property<int>();

    public void Add(Arg<string> note) => Method([note]);

    public MethodResult<string> Label(Arg<string> resident) => Method<string>([resident]);

    public Event Signed => Event();
}
