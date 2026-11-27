using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheRegisterPositionsItsDateCalendar(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1759_0)]
    public async Task the_calendar_can_follow_a_named_axis_and_keep_the_other_axis_fixed()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalCenteredHandover);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open handover calendar", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2700; left: 180px; top: 160px; position: fixed;");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalAlignedReturn);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open handover calendar", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2700; left: 368px; top: 116px; position: fixed;");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1760_0)]
    public async Task the_registration_calendar_remains_stationary_or_tracks_the_scrolled_field()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPinnedReference);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open register date", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 92px; position: fixed;");
        await resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read next arrival", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByText("Next arrival at the west reception desk", new PageGetByTextOptions { Exact = true })).ToBeFocusedAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to current admission", Exact = true })).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 92px; position: fixed;");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalFollowingBooking);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open register date", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 92px; position: fixed;");
        await referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read next arrival", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByText("Next arrival at the west reception desk", new PageGetByTextOptions { Exact = true })).ToBeFocusedAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to current admission", Exact = true })).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 52px; position: fixed;");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }
}
