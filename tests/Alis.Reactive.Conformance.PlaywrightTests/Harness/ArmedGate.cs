using System.Net.Http.Json;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public sealed class ArmedGate
{
    private readonly HttpClient _harness;
    private readonly GateOrder _order;

    internal ArmedGate(HttpClient harness, GateOrder order)
    {
        _harness = harness;
        _order = order;
    }

    public async Task<HeldGate> Held()
    {
        (await _harness.PostAsJsonAsync(WorldEndpoints.HeldGate, _order)).EnsureSuccessStatusCode();
        return new HeldGate(_harness, _order);
    }
}
