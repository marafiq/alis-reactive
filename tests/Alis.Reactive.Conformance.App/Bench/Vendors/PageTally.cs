using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Vendors/page-tally", "PageTally")]
public sealed class PageTally : Plugin
{
    public PageTally()
        : base("page-tally")
    {
    }

    public MethodResult<int> AddOne() => Method<int>();
}
