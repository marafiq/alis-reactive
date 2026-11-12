using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerWritesAComponentAnEventsPayloadHoldsAfterAnAwait(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_component_held_after)]
    public async Task the_write_reaches_the_payloads_peer_once_the_awaited_count_answers()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15ComponentHeldAfter);
        await Expect(bench.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Ratio", Exact = true })).ToHaveTextAsync("Ratio 25");
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Ratio", Exact = true })).ToHaveTextAsync("Ratio 25");
        await held.Release();

        await Expect(bench.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Ratio", Exact = true })).ToHaveTextAsync("Ratio 50");
    }
}
