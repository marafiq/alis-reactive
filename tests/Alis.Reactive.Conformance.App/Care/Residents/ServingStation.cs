using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ServingStation : Plugin
{
    public ServingStation()
        : base("serving-station")
    {
    }

    public StationPlates Plates => LiveObject<StationPlates>();
}

public sealed class StationPlates : JsObject, IDataIterable<string>
{
    TypedSource<IReadOnlyList<string>> IDataIterable<string>.Items => DataIterator<string>();

    public ReadOnly<int> Total => ReadOnly<int>(name: "count");
}
