using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheBedMenuClosesOnEscape(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F14.M21_X405_0)]
    public async Task its_closing_payload_holds_the_focus_and_its_opening_payload_none()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.BedActions);
        await closed.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Beds", Exact = true }).ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });
        await closed.Page.Keyboard.PressAsync("Escape");

        await Expect(closed.Page.Locator("#focus-status")).ToHaveTextAsync("Focused");

        var opened = await NewWorld();
        await opened.Page.GotoAsync(CarePages.BedActions);
        await opened.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Beds", Exact = true }).ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });

        await Expect(opened.Page.Locator("#opening-status")).ToHaveTextAsync("No focus");
    }
}
