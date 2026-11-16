using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors("Care/Residents/roster-rounds", "RosterRounds")]
public sealed class RosterRounds : Plugin
{
    public RosterRounds()
        : base("roster-rounds")
    {
    }

    public MethodResult<int> Next() => Method<int>();
}
