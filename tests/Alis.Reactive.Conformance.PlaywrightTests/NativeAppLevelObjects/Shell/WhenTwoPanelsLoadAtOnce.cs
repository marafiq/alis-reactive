using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoPanelsLoadAtOnce(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F2.M25_X3_0)]
    public async Task each_panel_shows_its_own_overlay_until_its_own_request_settles()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.WardPanels);
        var northOverlay = ward.Page.Locator("#alis-loader--north");
        var southOverlay = ward.Page.Locator("#alis-loader--south");
        var armedNorth = await ward.Arm(CompositionGate.Answer, CareEndpoints.NorthWardPanel);
        var armedSouth = await ward.Arm(CompositionGate.Answer, CareEndpoints.SouthWardPanel);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh north", Exact = true }).ClickAsync();
        var heldNorth = await armedNorth.Held();
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh south", Exact = true }).ClickAsync();
        var heldSouth = await armedSouth.Held();
        await Expect(northOverlay).ToBeVisibleAsync();
        await Expect(southOverlay).ToBeVisibleAsync();
        await heldSouth.Release();
        await Expect(ward.Page.Locator("#south")).ToHaveTextAsync("South: 9 residents");
        await Expect(southOverlay).ToHaveCountAsync(0);
        await Expect(northOverlay).ToBeVisibleAsync();
        await heldNorth.Release();
        await Expect(ward.Page.Locator("#north")).ToHaveTextAsync("North: 14 residents");
        await Expect(northOverlay).ToHaveCountAsync(0);
        await Expect(southOverlay).ToHaveCountAsync(0);
    }
}
