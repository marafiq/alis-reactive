using System.Net.Http.Json;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public sealed class HeldGate
{
    private readonly HttpClient _harness;
    private readonly GateOrder _order;

    internal HeldGate(HttpClient harness, GateOrder order)
    {
        _harness = harness;
        _order = order;
    }

    [Gesture]
    public async Task Release() => (await _harness.PostAsJsonAsync(WorldEndpoints.ReleaseGate, _order)).EnsureSuccessStatusCode();
}
