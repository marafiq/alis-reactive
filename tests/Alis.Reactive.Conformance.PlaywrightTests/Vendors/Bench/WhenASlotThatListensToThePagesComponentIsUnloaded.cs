using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotThatListensToThePagesComponentIsUnloaded(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.lane_slot_unloadListener)]
    public async Task the_unloaded_slots_listener_is_gone()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.LaneSlotUnloadListener);
        await Expect(bench.Page.Locator("#card").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("The card listens to the page's component.");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close card", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#card")).ToBeEmptyAsync();
        await bench.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await bench.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-2");
        await Expect(bench.Page.Locator("#log")).ToHaveTextAsync("");

        var loaded = await NewWorld();
        await loaded.Page.GotoAsync(BenchPages.LaneSlotUnloadListener);
        await Expect(loaded.Page.Locator("#card").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("The card listens to the page's component.");
        await loaded.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await loaded.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(loaded.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-2");
        await Expect(loaded.Page.Locator("#log")).ToHaveTextAsync("a-2");
    }
}
