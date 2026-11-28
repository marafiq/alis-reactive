using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.FusionShell))]
[Category(Hosts.FusionShellName)]
public sealed class WhenStaffOpenAndCloseTheWardMenu(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F30.M21_X406_0)]
    public async Task its_closing_event_reports_focus_and_its_opening_event_reports_none()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.FusionWardNavigation);
        await closed.Page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions { Name = "Ward", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions { Name = "North", Exact = true })).ToBeVisibleAsync();
        await closed.Page.Keyboard.PressAsync("Escape");
        await Expect(closed.Page.Locator("#focus-status")).ToHaveTextAsync("Focused");

        var opened = await NewWorld();
        await opened.Page.GotoAsync(CarePages.FusionWardNavigation);
        await opened.Page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions { Name = "Ward", Exact = true }).ClickAsync();
        await Expect(opened.Page.Locator("#opening-status")).ToHaveTextAsync("No focus");
    }
}
