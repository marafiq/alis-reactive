using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerShowsWhatAnAwaitedMethodOfAnEventsPayloadGives(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_methodAsyncValue_payload_before)]
    public async Task the_text_the_promise_gives_is_shown_once_it_settles()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15MethodAsyncValuePayloadBefore);
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("");
        await held.Release();

        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Settled text");

        var refused = await NewWorld();
        await refused.Seed(Seed.TheTextEndpointRefuses());
        await refused.Page.GotoAsync(BenchPages.Poc15MethodAsyncValuePayloadBefore);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Refused");
        await Expect(refused.Page.Locator("#shown")).ToHaveTextAsync("");
    }
}
