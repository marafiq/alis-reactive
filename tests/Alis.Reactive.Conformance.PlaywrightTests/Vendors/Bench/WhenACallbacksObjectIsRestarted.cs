using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACallbacksObjectIsRestarted(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_52_0)]
    public async Task the_release_writes_nothing_to_either_object_and_the_card_opened_again_hears_the_new_one()
    {
        var restarted = await NewWorld();
        await restarted.Page.GotoAsync(BenchPages.VendorsRestartedCallbacks);
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await restarted.Restart("callbacks");
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(restarted.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own 2", "own 2 card" });
        Assert.That(restarted.ConsoleErrors, Is.Empty);

        var kept = await NewWorld();
        await kept.Page.GotoAsync(BenchPages.VendorsRestartedCallbacks);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own 1 card" });
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByText("The card listens to the room calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own 1 card", "own 1" });
    }
}
