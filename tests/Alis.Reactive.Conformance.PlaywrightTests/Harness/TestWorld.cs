using System.Collections.Concurrent;
using System.Net.Http.Json;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public sealed class TestWorld
{
    private readonly HttpClient _harness;
    private readonly IBrowserContext _context;
    private readonly ConcurrentQueue<string> _consoleErrors = new();

    private TestWorld(HttpClient harness, IBrowserContext context, IPage page)
    {
        _harness = harness;
        _context = context;
        Page = page;
        page.Console += (_, message) => Keep(message);
    }

    public IPage Page { get; }

    [BesideThePage]
    public IReadOnlyCollection<string> ConsoleErrors => _consoleErrors;

    public async Task Seed(Seed seed) => (await _harness.PostAsJsonAsync(WorldEndpoints.Seed, seed)).EnsureSuccessStatusCode();

    public Task<ArmedGate> Arm<TGate>(TGate gate)
        where TGate : struct, Enum
        => Arm(new GateOrder { Gate = GateName.Of(gate) });

    public Task<ArmedGate> Arm<TGate>(TGate gate, string endpoint)
        where TGate : struct, Enum
        => Arm(new GateOrder { Gate = GateName.Of(gate), At = endpoint });

    private async Task<ArmedGate> Arm(GateOrder order)
    {
        (await _harness.PostAsJsonAsync(WorldEndpoints.ArmGate, order)).EnsureSuccessStatusCode();
        return new ArmedGate(_harness, order);
    }

    private void Keep(IConsoleMessage message)
    {
        if (message.Type == "error") _consoleErrors.Enqueue(message.Text);
    }

    internal static async Task<TestWorld> Open(ConformanceHost host)
    {
        var key = Guid.NewGuid().ToString("N");
        var harness = new HttpClient { BaseAddress = host.Address };
        harness.DefaultRequestHeaders.Add("Cookie", $"{WorldKey.Cookie}={key}");
        var context = await NewContext(host);
        await context.AddCookiesAsync([new Cookie { Name = WorldKey.Cookie, Value = key, Url = host.Address.ToString() }]);
        return new TestWorld(harness, context, await context.NewPageAsync());
    }

    internal static async Task<IBrowserContext> NewContext(ConformanceHost host)
    {
        var context = await Run.Browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = host.Address.ToString(), Locale = "en-US", TimezoneId = "America/Los_Angeles" });
        await Waits.Open(context);
        return context;
    }

    internal async Task<IList<string>> End()
    {
        await Waits.Close(_context);
        using var answer = await _harness.PostAsync(WorldEndpoints.End, content: null);
        answer.EnsureSuccessStatusCode();
        var end = await answer.Content.ReadFromJsonAsync<WorldEnd>();
        _harness.Dispose();
        return end!.GatesStillParked;
    }
}
