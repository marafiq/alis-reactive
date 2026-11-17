using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors("Care/Round/round-tally", "RoundTally")]
public sealed class RoundTally : Plugin
{
    public RoundTally()
        : base("round-tally")
    {
    }

    public MethodResult<int> AddOne() => Method<int>();
}
