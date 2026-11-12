using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPayloadsObjectIsHandedOverAgain(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X12_9_0)]
    public async Task its_event_runs_the_listening_pipeline_once_for_each_raise()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsPeerChanges);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await bench.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#changed-count")).ToHaveTextAsync("1");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 2");
        await bench.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#changed-count")).ToHaveTextAsync("2");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise with a new peer", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 3");
        await bench.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).Last.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#changed-count")).ToHaveTextAsync("3");
    }
}
