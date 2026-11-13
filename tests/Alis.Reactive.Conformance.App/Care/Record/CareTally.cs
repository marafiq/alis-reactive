using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareTally : Plugin
{
    public CareTally()
        : base("page-tally")
    {
    }

    public MethodResult<int> AddOne() => Method<int>();
}
