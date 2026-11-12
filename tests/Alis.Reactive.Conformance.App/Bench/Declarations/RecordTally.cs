using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class RecordTally : Plugin
{
    public RecordTally()
        : base("page-tally")
    {
    }

    public MethodResult<int> AddOne() => Method<int>();
}
