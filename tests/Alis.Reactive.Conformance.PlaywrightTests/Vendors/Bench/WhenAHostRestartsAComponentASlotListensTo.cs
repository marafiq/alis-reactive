using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHostRestartsAComponentASlotListensTo(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F2.X12_13_1)]
    public async Task the_listen_stays_with_the_destroyed_object_and_the_unload_calls_no_unsubscribe()
    {
        var restarted = await NewWorld();
        await restarted.Page.GotoAsync(BenchPages.VendorsRestartedComponent);
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the slot", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByText("The holder listens to the component's raises.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(restarted.Page.Locator("#heard")).ToHaveTextAsync("1");
        await restarted.Restart("conformance");
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(restarted.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await restarted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the slot", Exact = true }).ClickAsync();
        await Expect(restarted.Page.GetByText("The holder listens to the component's raises.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(restarted.Page.Locator("#heard")).ToHaveTextAsync("1");
        await Expect(restarted.Page.Locator("#off-calls")).ToHaveTextAsync("0");
        Assert.That(restarted.ConsoleErrors, Is.Empty);

        var kept = await NewWorld();
        await kept.Page.GotoAsync(BenchPages.VendorsRestartedComponent);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the slot", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByText("The holder listens to the component's raises.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#heard")).ToHaveTextAsync("1");
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the slot", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByText("The holder listens to the component's raises.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(kept.Page.Locator("#off-calls")).ToHaveTextAsync("1");
    }
}
