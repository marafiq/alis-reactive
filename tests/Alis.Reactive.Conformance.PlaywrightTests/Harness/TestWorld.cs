using System.Net.Http.Json;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public sealed class TestWorld
{
    private readonly HttpClient _harness;
    private readonly IBrowserContext _context;

    private TestWorld(HttpClient harness, IBrowserContext context, IPage page)
    {
        _harness = harness;
        _context = context;
        Page = page;
    }

    public IPage Page { get; }

    public async Task Seed(Seed seed) => (await _harness.PostAsJsonAsync(WorldEndpoints.Seed, seed)).EnsureSuccessStatusCode();

    public async Task<ArmedGate> Arm<TGate>(TGate gate)
        where TGate : struct, Enum
    {
        var order = new GateOrder { Gate = GateName.Of(gate) };
        (await _harness.PostAsJsonAsync(WorldEndpoints.ArmGate, order)).EnsureSuccessStatusCode();
        return new ArmedGate(_harness, order);
    }

    internal static async Task<TestWorld> Open(ConformanceHost host)
    {
        var key = Guid.NewGuid().ToString("N");
        var harness = new HttpClient { BaseAddress = host.Address };
        harness.DefaultRequestHeaders.Add("Cookie", $"{WorldKey.Cookie}={key}");
        var context = await Run.Browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = host.Address.ToString(), Locale = "en-US", TimezoneId = "America/Los_Angeles" });
        await context.AddCookiesAsync([new Cookie { Name = WorldKey.Cookie, Value = key, Url = host.Address.ToString() }]);
        return new TestWorld(harness, context, await context.NewPageAsync());
    }

    internal async Task<IList<string>> End()
    {
        await _context.CloseAsync();
        using var answer = await _harness.PostAsync(WorldEndpoints.End, content: null);
        answer.EnsureSuccessStatusCode();
        var end = await answer.Content.ReadFromJsonAsync<WorldEnd>();
        _harness.Dispose();
        return end!.GatesStillParked;
    }
}
