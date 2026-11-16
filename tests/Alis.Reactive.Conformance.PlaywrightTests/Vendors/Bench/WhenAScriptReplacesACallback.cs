using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAScriptReplacesACallback(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_50_0)]
    public async Task the_framework_puts_back_only_its_own_function_and_keeps_what_the_script_assigned()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsCallbackScript);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Holds", Exact = true })).ToHaveTextAsync("other");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Holds", Exact = true })).ToHaveTextAsync("own");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace the callback", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Holds", Exact = true })).ToHaveTextAsync("script");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own card", "own", "script", "script card", "script" });

        var unopened = await NewWorld();
        await unopened.Page.GotoAsync(BenchPages.VendorsCallbackScript);
        await unopened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(unopened.Page.Locator("#callbacks").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Holds", Exact = true })).ToHaveTextAsync("own");
        await unopened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(unopened.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own" });
    }
}
