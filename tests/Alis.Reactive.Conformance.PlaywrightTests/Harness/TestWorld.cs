using System.Collections.Concurrent;
using System.Net.Http.Json;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public sealed class TestWorld
{
    private static readonly DateTime ClockStart = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan ClockStep = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan RetryBudget = TimeSpan.FromSeconds(30);
    private readonly HttpClient _harness;
    private readonly IBrowserContext _context;
    private readonly ConcurrentQueue<string> _consoleErrors = new();
    private readonly ConcurrentQueue<Uri> _sent = new();

    private TestWorld(HttpClient harness, IBrowserContext context, IPage page)
    {
        _harness = harness;
        _context = context;
        Page = page;
        page.Console += (_, message) => Keep(message);
        page.Request += (_, request) => _sent.Enqueue(new Uri(request.Url));
    }

    public IPage Page { get; }

    [BesideThePage]
    public IReadOnlyCollection<string> ConsoleErrors => _consoleErrors;

    [BesideThePage]
    public int SentTo(string endpoint) => _sent.Count(address => address.AbsolutePath == endpoint);

    public async Task Seed(Seed seed) => (await _harness.PostAsJsonAsync(WorldEndpoints.Seed, seed)).EnsureSuccessStatusCode();

    public Task<ArmedGate> Arm<TGate>(TGate gate)
        where TGate : struct, Enum
        => Arm(new GateOrder { Gate = GateName.Of(gate) });

    public Task<ArmedGate> Arm<TGate>(TGate gate, string endpoint)
        where TGate : struct, Enum
        => Arm(new GateOrder { Gate = GateName.Of(gate), At = endpoint });

    public async Task Restart(string elementId)
    {
        using var answer = await _harness.PostAsJsonAsync(WorldEndpoints.HostRestart, new RestartOrder { ElementId = elementId });
        if (answer.StatusCode == System.Net.HttpStatusCode.Conflict)
            throw new InvalidOperationException("The host did not restart the component at '" + elementId + "': " + await answer.Content.ReadAsStringAsync());
        answer.EnsureSuccessStatusCode();
    }

    public Task CollectGarbage() => Page.RequestGCAsync();

    public Task InstallTheClock() => Page.Clock.InstallAsync(new ClockInstallOptions { TimeDate = ClockStart });

    public Task HoldTheClock() => Page.Clock.PauseAtAsync(ClockStart.AddHours(1));

    public async Task RunTheClockUntilItSends(string endpoint, int requests)
    {
        var run = TimeSpan.Zero;
        while (SentTo(endpoint) < requests)
        {
            if (run >= RetryBudget)
                throw new InvalidOperationException("The page sent " + SentTo(endpoint) + " of " + requests + " requests to " + endpoint + " in " + RetryBudget.TotalSeconds + " seconds of its clock.");
            await Page.Clock.RunForAsync((long)ClockStep.TotalMilliseconds);
            run += ClockStep;
        }
    }

    private async Task<ArmedGate> Arm(GateOrder order)
    {
        using var answer = await _harness.PostAsJsonAsync(WorldEndpoints.ArmGate, order);
        answer.EnsureSuccessStatusCode();
        var armed = await answer.Content.ReadFromJsonAsync<GateOrder>();
        return new ArmedGate(_harness, armed!);
    }

    private void Keep(IConsoleMessage message)
    {
        if (message.Type == "error") _consoleErrors.Enqueue(message.Text);
    }

    internal static async Task<TestWorld> Open(ConformanceHost host, string zone)
    {
        var key = Guid.NewGuid().ToString("N");
        var harness = new HttpClient { BaseAddress = host.Address };
        harness.DefaultRequestHeaders.Add("Cookie", $"{WorldKey.Cookie}={key}");
        var context = await NewContext(host, zone, javaScriptEnabled: true);
        await context.AddCookiesAsync([new Cookie { Name = WorldKey.Cookie, Value = key, Url = host.Address.ToString() }]);
        return new TestWorld(harness, context, await context.NewPageAsync());
    }

    internal static async Task<IBrowserContext> NewContext(ConformanceHost host, string zone, bool javaScriptEnabled)
    {
        var context = await Run.Browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = host.Address.ToString(), Locale = "en-US", TimezoneId = zone, JavaScriptEnabled = javaScriptEnabled });
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
