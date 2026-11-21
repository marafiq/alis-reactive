using System.Net.Http.Json;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public sealed class RefusedStream
{
    private readonly HttpClient _harness;
    private readonly string _stream;

    internal RefusedStream(HttpClient harness, string stream)
    {
        _harness = harness;
        _stream = stream;
    }

    public async Task Refused(int connections) => (await _harness.PostAsJsonAsync(WorldEndpoints.StreamRefused, new StreamOrder { Stream = _stream, Count = connections })).EnsureSuccessStatusCode();

    [Gesture]
    public async Task Reopen() => (await _harness.PostAsJsonAsync(WorldEndpoints.StreamReopen, new StreamOrder { Stream = _stream })).EnsureSuccessStatusCode();
}
