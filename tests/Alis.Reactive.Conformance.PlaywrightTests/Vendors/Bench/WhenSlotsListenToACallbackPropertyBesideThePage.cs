using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenSlotsListenToACallbackPropertyBesideThePage(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_49_0)]
    public async Task each_listener_hears_once_in_the_order_it_subscribed_until_its_slot_unloads_and_the_objects_throw_skips_none()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsCallbackListeners);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the inner card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The inner card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the notes", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The notes listen to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the inner card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The inner card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the notes", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The notes listen to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the inner card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The inner card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Make it throw", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own page-1 page-2 card inner notes", "own page-1 page-2 card notes", "own page-1 page-2 card", "own page-1 page-2 card inner", "own page-1 page-2 card inner" });
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Error", Exact = true })).ToHaveTextAsync("The callbacks' own function threw.");

        var unopened = await NewWorld();
        await unopened.Page.GotoAsync(BenchPages.VendorsCallbackListeners);
        await unopened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(unopened.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own page-1 page-2" });
    }
}
