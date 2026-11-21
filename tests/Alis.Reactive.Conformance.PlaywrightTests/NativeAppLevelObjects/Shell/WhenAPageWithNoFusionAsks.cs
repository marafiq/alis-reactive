using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageWithNoFusionAsks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F4.X17_6_0)]
    public async Task the_native_confirm_asks_and_its_answer_decides()
    {
        var discharging = await NewWorld();
        await discharging.Page.GotoAsync(CarePages.WardDischarge);
        await discharging.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge Ada", Exact = true }).ClickAsync();
        var asking = discharging.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asking.GetByText("Discharge Ada from the ward?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(discharging.Page.Locator("#decision")).ToHaveTextAsync("Discharged");

        var keeping = await NewWorld();
        await keeping.Page.GotoAsync(CarePages.WardDischarge);
        await keeping.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge Ada", Exact = true }).ClickAsync();
        var refusing = keeping.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await refusing.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "No", Exact = true }).ClickAsync();
        await Expect(keeping.Page.Locator("#decision")).ToHaveTextAsync("Kept");
    }
}
