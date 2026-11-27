using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheAdmissionDeskSchedulesArrivals(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1700_0)]
    public async Task the_file_keeps_a_confirmed_day_or_releases_the_resident_to_the_waiting_list()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalBooking);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply confirmed referral", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File arrival", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Arrival receipt", Exact = true })).ToHaveTextAsync("Arrival booked for 14 July 2026");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalBooking);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release referral", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File arrival", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Arrival receipt", Exact = true })).ToHaveTextAsync("Waiting list without an arrival day");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1701_0)]
    public async Task the_transport_booking_carries_the_offset_for_its_scheduled_day()
    {
        var resident = await NewWorldInZone(Zones.NewYork);
        await resident.Page.GotoAsync(CarePages.ArrivalTransport);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use summer transport", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File transport booking", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transport receipt", Exact = true })).ToHaveTextAsync("Transport booked for 14 July 2026 00:00 -04:00");

        var referral = await NewWorldInZone(Zones.NewYork);
        await referral.Page.GotoAsync(CarePages.ArrivalTransport);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use winter transport", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File transport booking", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transport receipt", Exact = true })).ToHaveTextAsync("Transport booked for 14 January 2026 00:00 -05:00");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1702_0)]
    public async Task placement_history_distinguishes_a_resident_choice_from_the_referral()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalBooking);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Placement history", Exact = true })).ToHaveTextAsync("Resident chose 16 July");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalBooking);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply transfer referral", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Placement history", Exact = true })).ToHaveTextAsync("Referral assigned 16 July");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1703_0)]
    public async Task the_desk_opens_the_calendar_and_returns_to_the_intake_form()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalCalendarReview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open arrival calendar", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(resident.Page.GetByText("July 2026", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalCalendarReview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open arrival calendar", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Return to intake", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1704_0)]
    public async Task a_signed_admission_keeps_its_calendar_closed()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalCalendarSigned);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open arrival calendar", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calendar access", Exact = true })).ToHaveTextAsync("Signed arrival locked");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalCalendarReview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open arrival calendar", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calendar access", Exact = true })).ToHaveTextAsync("Arrival calendar available");
        await Expect(referral.Page.GetByText("July 2026", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1705_0)]
    public async Task an_active_date_review_can_hold_the_calendar_open()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalCalendarHeld);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowUp");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Date review", Exact = true })).ToHaveTextAsync("Continue reviewing dates");
        await Expect(resident.Page.GetByText("July 2026", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalCalendarReview);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowUp");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Date review", Exact = true })).ToHaveTextAsync("Date review finished");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1706_0)]
    public async Task the_interview_moves_keyboard_focus_into_and_out_of_the_arrival_date()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalInterview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Begin arrival interview", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Interview", Exact = true })).ToHaveTextAsync("Editing the arrival date");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalInterview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Begin arrival interview", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Finish arrival interview", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).Not.ToBeFocusedAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Interview", Exact = true })).ToHaveTextAsync("Arrival editing finished");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1707_0)]
    public async Task calendar_only_entry_keeps_the_date_list_available()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalInterview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use calendar choices", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("readonly", "");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Entry instruction", Exact = true })).ToHaveTextAsync("Choose an available day");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalInterview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use keyboard entry", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).Not.ToHaveAttributeAsync("readonly", "");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Entry instruction", Exact = true })).ToHaveTextAsync("Enter a referral date");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1708_0)]
    public async Task intake_availability_controls_whether_the_date_editor_opens()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalInterview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hold intake", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Booking status", Exact = true })).ToHaveTextAsync("Intake on hold");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalInterview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release intake", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-disabled", "false");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Booking status", Exact = true })).ToHaveTextAsync("Ready to schedule");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1709_0)]
    public async Task signing_the_arrival_protects_the_current_day()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalInterview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Protect signed arrival", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("readonly", "");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signature status", Exact = true })).ToHaveTextAsync("Signed arrival");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalInterview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reopen arrival", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).Not.ToHaveAttributeAsync("readonly", "");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Signature status", Exact = true })).ToHaveTextAsync("Arrival reopened");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1710_0)]
    public async Task calendar_assistance_is_applied_when_the_field_gains_focus()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalInterview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use assisted entry", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalInterview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use direct entry", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1711_0)]
    public async Task the_admission_window_corrects_dates_at_its_two_bounds()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalWindow);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply approved window", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("2026-07-05");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-10");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalWindow);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply approved window", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("2026-07-28");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-20");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1712_0)]
    public async Task a_request_outside_the_window_can_be_retained_for_review()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalWindow);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review exceptional date", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("7/25/2026");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-25");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalWindow);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Enforce arrival window", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("7/25/2026");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-20");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1713_0)]
    public async Task the_admission_forecast_offers_months_while_a_transfer_offers_days()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalCalendarReview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Plan by month", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByText("Jul", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calendar scope", Exact = true })).ToHaveTextAsync("Choose an admission month");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalCalendarReview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Plan by day", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calendar scope", Exact = true })).ToHaveTextAsync("Choose an arrival day");
    }
}
