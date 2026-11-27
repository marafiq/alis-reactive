using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheWardUsesItsAdmissionCalendar(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1751_0)]
    public async Task restoring_the_local_weekday_changes_the_new_year_roster()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalReportingWeek);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open local reporting week", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Columnheader).First).ToHaveTextAsync("S");
        await Expect(resident.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Row).Nth(1).GetByRole(AriaRole.Gridcell).First).ToHaveTextAsync("52");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalReportingWeek);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open dispatch reporting week", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Columnheader).First).ToHaveTextAsync("Tue");
        await Expect(referral.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Row).Nth(1).GetByRole(AriaRole.Gridcell).First).ToHaveTextAsync("1");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1752_0)]
    public async Task the_two_reading_stations_place_working_calendars_in_their_own_columns()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalLeftStation);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open reading station", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 3400; left: 64px; top: 92px; width: 340px; height: 320px;");
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveClassAsync("e-datepicker e-popup-wrapper e-lib e-popup e-control null e-popup-open");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Station instruction", Exact = true })).ToHaveTextAsync("Use the left column beside the admission register.");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalRightStation);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open reading station", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 3400; left: 420px; top: 116px; width: 24rem; height: 22rem;");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveClassAsync("e-datepicker e-popup-wrapper e-lib e-popup e-control null e-rtl e-popup-open");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Station instruction", Exact = true })).ToHaveTextAsync("Use the right column beside the resident’s handover.");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1753_0)]
    public async Task paging_the_register_dismisses_a_quick_calendar_and_retains_a_review_calendar()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalQuickRegister);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open register calendar", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Register guidance", Exact = true })).ToHaveTextAsync("Finish the date before paging onward");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open register calendar", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read next transfer round", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByText("Next transfer round", new PageGetByTextOptions { Exact = true })).ToBeFocusedAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to upcoming transfers", Exact = true })).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalRetainedRegister);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open register calendar", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read next transfer round", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByText("Next transfer round", new PageGetByTextOptions { Exact = true })).ToBeFocusedAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to upcoming transfers", Exact = true })).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(1);
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Register guidance", Exact = true })).ToHaveTextAsync("Keep reviewing the arrival date");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1754_0)]
    public async Task a_saved_assisted_calendar_opens_while_the_signed_profile_remains_protected()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalAssistedEntry);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review saved access", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await Expect(resident.Page.GetByText("Jul", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(resident.Page.GetByLabel("close", new PageGetByLabelOptions { Exact = true })).ToHaveCountAsync(1);
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Access instruction", Exact = true })).ToHaveTextAsync("Choose a month for the admission");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalSignedEntry);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review saved access", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("readonly", "");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(referral.Page.GetByLabel("close", new PageGetByLabelOptions { Exact = true })).ToHaveCountAsync(0);
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Access instruction", Exact = true })).ToHaveTextAsync("Consult the signed transfer");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1755_0)]
    public async Task a_transfer_desk_loads_its_shortcut_and_incoming_referral_pattern()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalIncomingTransfer);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("F2");
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowUp");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("23.07.2026");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-23");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalIncomingOrdinary);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("F2");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("23.07.2026");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1756_0)]
    public async Task the_saved_transfer_zone_changes_the_initial_booking_day()
    {
        var resident = await NewWorldInZone(Zones.Utc);
        await resident.Page.GotoAsync(CarePages.ArrivalSavedWest);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review transfer time", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-13");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transfer review", Exact = true })).ToHaveTextAsync("Western transfer schedule");

        var referral = await NewWorldInZone(Zones.Utc);
        await referral.Page.GotoAsync(CarePages.ArrivalSavedEast);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review transfer time", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transfer review", Exact = true })).ToHaveTextAsync("Eastern transfer schedule");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1757_0)]
    public async Task the_family_sees_the_reason_for_a_day_outside_the_admission_window()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalFamilyLimited);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(resident.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Row).Nth(2).GetByRole(AriaRole.Gridcell, new LocatorGetByRoleOptions { Name = "7", Exact = true })).ToHaveClassAsync("e-cell e-disabled e-overlay");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Family notice", Exact = true })).ToHaveTextAsync("Before the approved window");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalFamilyReleased);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Family notice", Exact = true })).ToHaveTextAsync("Within the admission window");
        await referral.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Row).Nth(2).GetByRole(AriaRole.Gridcell, new LocatorGetByRoleOptions { Name = "7", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-07");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1758_0)]
    public async Task the_privacy_review_can_prevent_opening_until_the_referral_is_cleared()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPrivacyHeld);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Privacy review", Exact = true })).ToHaveTextAsync("Finish privacy clearance first");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPrivacyReleased);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Privacy review", Exact = true })).ToHaveTextAsync("Cleared for scheduling");
    }
}
