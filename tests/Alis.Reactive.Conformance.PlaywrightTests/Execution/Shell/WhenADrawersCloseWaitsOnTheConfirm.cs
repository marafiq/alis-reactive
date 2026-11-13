using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADrawersCloseWaitsOnTheConfirm(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_2_0)]
    public async Task the_drawer_stays_open_while_the_dialog_asks_and_closes_after_yes()
    {
        var discarding = await NewWorld();
        await discarding.Page.GotoAsync(CarePages.CarePlanDrawer);
        await discarding.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit care plan", Exact = true }).ClickAsync();
        var drawer = discarding.Page.GetByRole(AriaRole.Complementary, new PageGetByRoleOptions { Name = "Care plan", Exact = true });
        await Expect(drawer.GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Goals", Exact = true })).ToBeVisibleAsync();
        await drawer.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        var asking = discarding.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asking.GetByText("Discard the unsaved care plan?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(drawer).ToBeVisibleAsync();
        await asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(drawer).ToBeHiddenAsync();

        var keeping = await NewWorld();
        await keeping.Page.GotoAsync(CarePages.CarePlanDrawer);
        await keeping.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit care plan", Exact = true }).ClickAsync();
        var kept = keeping.Page.GetByRole(AriaRole.Complementary, new PageGetByRoleOptions { Name = "Care plan", Exact = true });
        await kept.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        var refusing = keeping.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await refusing.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "No", Exact = true }).ClickAsync();
        await Expect(refusing).ToBeHiddenAsync();
        await Expect(kept).ToBeVisibleAsync();
    }
}
