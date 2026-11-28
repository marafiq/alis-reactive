using Alis.Reactive.Conformance.World;
using Alis.Reactive.Declarations;
using Alis.Reactive.Realtime;
using Alis.Reactive.ValuesAndSources;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class BenchLine : SignalRHub
{
    public static readonly SignalRSource<BenchLine> Live = At<BenchLine>(BenchEndpoints.Hub);

    public MethodResult<string> Echo(Arg<string?> text) => Invoke<string>([text]);
}

internal sealed class BenchLineHub : Hub
{
    public string Echo(string? text) => "heard " + text;
}

internal static class BenchLines
{
    internal static void MapBenchLine(this WebApplication app) => app.MapHub<BenchLineHub>(BenchEndpoints.Hub);
}
