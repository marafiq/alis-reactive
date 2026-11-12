using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerWritesAComponentAnEventsPayloadHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_component_held_before)]
    public async Task the_payloads_peers_ratio_is_shown_and_the_handlers_write_reaches_the_peer()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15ComponentHeldBefore);
        await Expect(bench.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Ratio", Exact = true })).ToHaveTextAsync("Ratio 25");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("25");
        await Expect(bench.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Ratio", Exact = true })).ToHaveTextAsync("Ratio 50");
    }
}
