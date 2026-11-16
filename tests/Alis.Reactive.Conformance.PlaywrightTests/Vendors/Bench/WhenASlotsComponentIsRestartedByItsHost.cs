using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotsComponentIsRestartedByItsHost(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.lane_slot_hostDestroyed)]
    public async Task closing_the_card_calls_nothing_on_the_destroyed_component_and_opening_it_shows_a_new_one()
    {
        var restarted = await NewWorld();
        await restarted.Page.GotoAsync(BenchPages.LaneSlotHostDestroyed);
        await Expect(restarted.Page.GetByText("The card listens to its own component.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await restarted.Restart("card-component");
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close card", Exact = true }).ClickAsync();
        await Expect(restarted.Page.Locator("#card")).ToBeEmptyAsync();
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open card", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByText("The card listens to its own component.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(restarted.Page.Locator("#card").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-1");
        Assert.That(restarted.ConsoleErrors, Is.Empty);

        var kept = await NewWorld();
        await kept.Page.GotoAsync(BenchPages.LaneSlotHostDestroyed);
        await Expect(kept.Page.GetByText("The card listens to its own component.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close card", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#card")).ToBeEmptyAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open card", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByText("The card listens to its own component.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(kept.Page.Locator("#card").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-1");
        Assert.That(kept.ConsoleErrors, Is.Empty);
    }
}
