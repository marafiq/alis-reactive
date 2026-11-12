using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerAwaitsAMethodDeepInAnEventsPayload(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_deepAsync)]
    public async Task the_steps_after_the_call_wait_until_its_promise_settles()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.EventDeepAsync);
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#flagged")).ToHaveTextAsync("");
        await held.Release();

        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Item settled");
        await Expect(bench.Page.Locator("#flagged")).ToHaveTextAsync("Flagged");

        var refused = await NewWorld();
        await refused.Seed(Seed.TheItemEndpointRefuses());
        await refused.Page.GotoAsync(BenchPages.EventDeepAsync);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Refused");
        await Expect(refused.Page.Locator("#shown")).ToHaveTextAsync("");
        await Expect(refused.Page.Locator("#flagged")).ToHaveTextAsync("");
    }
}
