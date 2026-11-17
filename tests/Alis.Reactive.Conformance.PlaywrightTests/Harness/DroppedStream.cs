using System.Net.Http.Json;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public sealed class DroppedStream
{
    private readonly HttpClient _harness;
    private readonly string _stream;

    internal DroppedStream(HttpClient harness, string stream)
    {
        _harness = harness;
        _stream = stream;
    }

    public async Task NextHeld() => (await _harness.PostAsJsonAsync(WorldEndpoints.StreamNextHeld, new StreamOrder { Stream = _stream })).EnsureSuccessStatusCode();

    [Gesture]
    public async Task Reopen() => (await _harness.PostAsJsonAsync(WorldEndpoints.StreamReopen, new StreamOrder { Stream = _stream })).EnsureSuccessStatusCode();
}
