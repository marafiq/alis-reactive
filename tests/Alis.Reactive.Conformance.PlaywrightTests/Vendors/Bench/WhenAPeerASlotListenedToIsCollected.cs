using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPeerASlotListenedToIsCollected(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X12_18_0)]
    public async Task the_page_reads_it_collected_and_the_slots_unload_calls_nothing_on_it()
    {
        var dropped = await NewWorld();
        await dropped.Page.GotoAsync(BenchPages.VendorsCollectedPeer);
        await dropped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the slot", Exact = true }).ClickAsync();
        await Expect(dropped.Page.GetByText("The holder listens to the peer each raise hands.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await dropped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await dropped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Drop the peer", Exact = true }).ClickAsync();
        await Expect(dropped.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true })).ToBeHiddenAsync();
        await dropped.CollectGarbage();
        await dropped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the first peer", Exact = true }).ClickAsync();
        await Expect(dropped.Page.Locator("#peer-state")).ToHaveTextAsync("collected");
        await dropped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the slot", Exact = true }).ClickAsync();
        await Expect(dropped.Page.Locator("#slot-state")).ToHaveTextAsync("closed");
        Assert.That(dropped.ConsoleErrors, Is.Empty);

        var kept = await NewWorld();
        await kept.Page.GotoAsync(BenchPages.VendorsCollectedPeer);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the slot", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByText("The holder listens to the peer each raise hands.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await kept.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#heard")).ToHaveTextAsync("1");
        await kept.CollectGarbage();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the first peer", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#peer-state")).ToHaveTextAsync("alive");
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the slot", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#slot-state")).ToHaveTextAsync("closed");
        await kept.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#heard")).ToHaveTextAsync("1");
    }
}
