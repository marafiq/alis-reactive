using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CardTally : Plugin
{
    public CardTally()
        : base("card-tally")
    {
    }

    public MethodResult<int> AddOne() => Method<int>();

    public ReadOnly<int> Count => ReadOnly<int>();
}
