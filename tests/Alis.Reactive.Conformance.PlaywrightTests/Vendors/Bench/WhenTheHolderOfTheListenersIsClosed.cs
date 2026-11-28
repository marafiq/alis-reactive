using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheHolderOfTheListenersIsClosed(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_50_1)]
    public async Task the_pages_handlers_keep_hearing_and_the_objects_own_function_is_not_put_back()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsCallbackHolder);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the slot", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The holder listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the inner slot", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#inner-opened")).ToHaveTextAsync("1");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the slot", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The holder listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Holds", Exact = true })).ToHaveTextAsync("other");
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own page-1 page-2 outer inner", "own page-1 page-2" });

        var unopened = await NewWorld();
        await unopened.Page.GotoAsync(BenchPages.VendorsCallbackHolder);
        await unopened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(unopened.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own page-1 page-2" });
    }
}
