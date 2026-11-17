using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAScheduleIsHandedItsEventsWithTheView(string host) : FusionFixture(host)
{
    private const string AdasShift = "Ada Brooks Begin From Monday, November 9, 2026 at 9:00:00 AM GMT-08:00 Ends At Monday, November 9, 2026 at 5:00:00 PM GMT-08:00";

    private const string LenasShift = "Lena Ortiz Begin From Tuesday, November 10, 2026 at 9:00:00 AM GMT-08:00 Ends At Tuesday, November 10, 2026 at 5:00:00 PM GMT-08:00";

    [Test]
    [FailsFor(Promises.M21_F39.M21_X1042_0)]
    public async Task the_schedule_shows_each_event_and_the_views_read_finds_it_by_its_json_name()
    {
        var ada = await NewWorld();
        await ada.Page.GotoAsync(CarePages.RotaCarers);
        await Expect(ada.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = LenasShift, Exact = true })).ToBeVisibleAsync();
        await ada.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = AdasShift, Exact = true }).ClickAsync();

        await Expect(ada.Page.Locator("#shift-picked")).ToHaveTextAsync("Ada Brooks");

        var lena = await NewWorld();
        await lena.Page.GotoAsync(CarePages.RotaCarers);
        await lena.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = LenasShift, Exact = true }).ClickAsync();

        await Expect(lena.Page.Locator("#shift-picked")).ToHaveTextAsync("Lena Ortiz");
    }
}
