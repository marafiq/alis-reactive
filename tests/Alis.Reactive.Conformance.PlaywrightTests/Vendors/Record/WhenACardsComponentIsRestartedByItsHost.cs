using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardsComponentIsRestartedByItsHost(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F2.X12_13_0)]
    public async Task its_handler_stays_with_the_destroyed_object_and_the_cards_release_calls_no_unsubscribe()
    {
        var restarted = await NewWorld();
        await restarted.Page.GotoAsync(CarePages.BedMonitor);
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed monitor", Exact = true })).ToBeVisibleAsync();
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(restarted.Page.Locator("#raised-heard")).ToHaveTextAsync("1");
        await restarted.Restart("bed-monitor");
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(restarted.Page.Locator("#bed-monitor").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed monitor", Exact = true })).ToBeHiddenAsync();
        await Expect(restarted.Page.Locator("#raised-heard")).ToHaveTextAsync("1");
        await Expect(restarted.Page.Locator("#off-calls")).ToHaveTextAsync("0");
        Assert.That(restarted.ConsoleErrors, Is.Empty);

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.BedMonitor);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed monitor", Exact = true })).ToBeVisibleAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#raised-heard")).ToHaveTextAsync("1");
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#raised-heard")).ToHaveTextAsync("2");
    }
}
