using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnInnerSlotIsLoadedAgain(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_49_2)]
    public async Task the_new_inner_listener_takes_the_old_ones_place_and_hears_once()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsCallbackHolder);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the slot", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The holder listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the inner slot", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#inner-opened")).ToHaveTextAsync("1");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the inner slot", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#inner-opened")).ToHaveTextAsync("2");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own page-1 page-2 outer inner", "own page-1 page-2 outer inner" });

        var holderAlone = await NewWorld();
        await holderAlone.Page.GotoAsync(BenchPages.VendorsCallbackHolder);
        await holderAlone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the slot", Exact = true }).ClickAsync();
        await Expect(holderAlone.Page.GetByText("The holder listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await holderAlone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(holderAlone.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own page-1 page-2 outer" });
    }
}
