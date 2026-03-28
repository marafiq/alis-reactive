using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.RealTime;

/// <summary>
/// SSE endpoint for facility alerts using .NET 10 TypedResults.ServerSentEvents API.
/// Replaces manual text/event-stream writing with first-class SseItem support.
/// </summary>
public static class FacilityAlertEndpoint
{
    public static IResult Stream(CancellationToken ct)
        => TypedResults.ServerSentEvents(StreamAlerts(ct));

    private static async IAsyncEnumerable<SseItem<string>> StreamAlerts(
        [EnumeratorCancellation] CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(
            new FacilityAlert { Message = "Facility check complete", Level = "info" },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        yield return new SseItem<string>(json, "facility-alert");

        // Keep connection open — browser EventSource stays connected
        try { await Task.Delay(Timeout.Infinite, ct); }
        catch (OperationCanceledException) { /* client disconnected */ }
    }
}
