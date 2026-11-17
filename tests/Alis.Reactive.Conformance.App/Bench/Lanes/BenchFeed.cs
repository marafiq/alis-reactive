using Alis.Reactive.Conformance.World;
using Alis.Reactive.Realtime;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class BenchFeed : ServerPushStream
{
    public static readonly ServerPushSource<BenchFeed> Live = At<BenchFeed>(BenchEndpoints.Stream);

    public ServerEvent<Welcome> Welcome => Message<Welcome>();

    public ServerEvent<Renamed> Renamed => Message<Renamed>();
}

public sealed class Welcome
{
    public int Accepted { get; set; }
}

public sealed class Renamed
{
    public string Name { get; set; } = "";
}
