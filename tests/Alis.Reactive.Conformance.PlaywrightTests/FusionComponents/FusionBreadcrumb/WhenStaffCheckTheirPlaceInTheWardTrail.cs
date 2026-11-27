using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.FusionShell))]
[Category(Hosts.FusionShellName)]
public sealed class WhenStaffCheckTheirPlaceInTheWardTrail(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F6.M21_X1002_0)]
    public async Task the_trail_reports_the_pages_active_location()
    {
        var wards = await NewWorld();
        await wards.Page.GotoAsync(CarePages.FusionWardTrail);
        await wards.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Where am I", Exact = true }).ClickAsync();
        await Expect(wards.Page.Locator("#ward-location")).ToHaveTextAsync("/wards");

        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.FusionNorthWingTrail);
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Where am I", Exact = true }).ClickAsync();
        await Expect(north.Page.Locator("#ward-location")).ToHaveTextAsync("/wards/north");
    }
}
