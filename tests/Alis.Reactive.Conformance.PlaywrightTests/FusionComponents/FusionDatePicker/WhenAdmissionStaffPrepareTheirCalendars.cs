using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAdmissionStaffPrepareTheirCalendars(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1714_0)]
    public async Task allocation_planners_can_jump_to_the_required_year_or_decade()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPlanning);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open 2028 allocation", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByText("2028", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Planning scope", Exact = true })).ToHaveTextAsync("Annual allocation 2028");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPlanning);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open 2030s allocation", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByText("2030 - 2039", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Planning scope", Exact = true })).ToHaveTextAsync("Decade allocation 2030");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1715_0)]
    public async Task the_cleaning_day_is_released_only_when_beds_are_ready()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPlanningHeld);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true })).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Day notice", Exact = true })).ToHaveTextAsync("Closed for cleaning");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPlanning);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true })).Not.ToHaveAttributeAsync("aria-disabled", "true");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Day notice", Exact = true })).ToHaveTextAsync("Beds released");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1716_0)]
    public async Task a_transfer_ledger_and_a_resident_letter_print_the_correct_date()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalFormats);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print ledger date", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("14.07.2026");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalFormats);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print resident letter", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("July 14, 2026");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1717_0)]
    public async Task a_culture_pattern_can_be_applied_and_cleared_on_the_letter()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalFormats);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use full correspondence date", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("Tuesday, July 14, 2026");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalFormats);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use full correspondence date", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Restore local date", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("7/14/2026");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1718_0)]
    public async Task only_an_imported_referral_accepts_its_contracted_date_notation()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalFormats);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept European referral", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("23.07.2026");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-23");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalFormats);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use ordinary referral", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("23.07.2026");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1719_0)]
    public async Task the_clear_control_withdraws_an_optional_arrival()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalWithdrawal);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Permit date withdrawal", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await resident.Page.GetByLabel("close", new PageGetByLabelOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Withdrawal receipt", Exact = true })).ToHaveTextAsync("Arrival withdrawn by staff");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalWithdrawal);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Protect transfer date", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(referral.Page.GetByLabel("close", new PageGetByLabelOptions { Exact = true })).ToHaveCountAsync(0);
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1720_0)]
    public async Task the_printed_arrival_instruction_uses_the_chart_or_the_nurse()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPrompt);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Follow chart instructions", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).Not.ToHaveAttributeAsync("placeholder", "");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Printed instruction", Exact = true })).ToHaveTextAsync("Follow the resident chart");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPrompt);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Suppress printed hint", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).Not.ToHaveAttributeAsync("placeholder", "");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Printed instruction", Exact = true })).ToHaveTextAsync("Ask the admission nurse");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1721_0)]
    public async Task large_print_and_compact_forms_show_the_arrival_cue_at_the_right_height()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPrompt);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use large-print date cue", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByText("Arrival day", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-top");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Card instructions", Exact = true })).ToHaveTextAsync("Large-print date entry");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPrompt);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use compact date cue", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByText("Arrival day", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-bottom");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(referral.Page.GetByText("Arrival day", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-top");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Card instructions", Exact = true })).ToHaveTextAsync("Compact date entry");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1722_0)]
    public async Task nursing_and_transport_rotas_use_their_respective_weekday_headings()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalRota);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open nursing rota", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Columnheader).First).ToHaveTextAsync("Monday");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalRota);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open transport rota", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Columnheader).First).ToHaveTextAsync("Su");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1723_0)]
    public async Task the_first_roster_week_obeys_the_selected_reporting_convention()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalRota);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open ISO reporting weeks", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Row).Nth(1).GetByRole(AriaRole.Gridcell).First).ToHaveTextAsync("53");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalRota);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open local reporting weeks", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Row).Nth(1).GetByRole(AriaRole.Gridcell).First).ToHaveTextAsync("1");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1724_0)]
    public async Task a_live_intake_keeps_the_today_and_week_number_aids()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalRota);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open same-day intake", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Today", Exact = true })).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Row).Nth(1).GetByRole(AriaRole.Gridcell)).ToHaveCountAsync(8);

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalRota);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open historical transfer", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Row).Nth(1).GetByRole(AriaRole.Gridcell)).ToHaveCountAsync(7);
        await Expect(referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Today", Exact = true })).ToHaveCountAsync(0);
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1725_0)]
    public async Task a_guided_arrival_uses_the_units_date_prompts()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalMask);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare guided date", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("arrival month / arrival day / admission year");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalMask);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare free-entry date", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1726_0)]
    public async Task the_calendar_fits_the_bedside_or_full_admission_sheet()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalSheet);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open bedside calendar", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Layout slip", Exact = true })).ToHaveTextAsync("Narrow bedside calendar");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalSheet);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open full admission calendar", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Layout slip", Exact = true })).ToHaveTextAsync("Full admission calendar");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1727_0)]
    public async Task only_a_remembering_workstation_restores_the_changed_arrival_date()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalDraft);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remember this arrival", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open next visit", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalDraft);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use a fresh visit", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open next visit", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1728_0)]
    public async Task the_recovery_archive_records_a_booked_or_released_arrival()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalDraft);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Archive booked referral", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Archive receipt", Exact = true })).ToHaveTextAsync("Archived arrival 14 July 2026");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalDraft);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Archive released referral", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Archive receipt", Exact = true })).ToHaveTextAsync("No scheduled date in the archive");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1729_0)]
    public async Task staff_can_close_the_arrival_editor_or_keep_it_in_service()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalDraft);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close arrival editor", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Editor record", Exact = true })).ToHaveTextAsync("Arrival editor closed");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalDraft);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue arrival editor", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Editor record", Exact = true })).ToHaveTextAsync("Arrival editor in service");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1730_0)]
    public async Task the_transport_and_return_slips_identify_their_own_native_date_fields()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalSlips);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview transport slip", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Slip preview", Exact = true })).ToHaveTextAsync("Transport arrival booked for 14 July");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalSlips);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview return slip", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Slip preview", Exact = true })).ToHaveTextAsync("Return transfer booked for 20 July");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1731_0)]
    public async Task the_protected_calendar_opens_at_its_reserved_overlay_layer()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalSheet);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open protected transfer", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calendar layer", Exact = true })).ToHaveTextAsync("Priority overlay");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalSheet);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open ordinary desk", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calendar layer", Exact = true })).ToHaveTextAsync("Standard desk calendar");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1732_0)]
    public async Task a_handover_can_prevent_dismissal_until_staff_finish_confirming()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalHandoverHeld);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowUp");
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Handover review", Exact = true })).ToHaveTextAsync("Handover confirmation required");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalHandover);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowUp");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Handover review", Exact = true })).ToHaveTextAsync("Calendar released");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1733_0)]
    public async Task the_allocation_note_distinguishes_a_clerks_navigation_from_a_planning_jump()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPlanning);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "next month", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Allocation note", Exact = true })).ToHaveTextAsync("Staff opened August 2026");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPlanning);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open December allocation", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByText("December 2027", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Allocation note", Exact = true })).ToHaveTextAsync("Desk opened December 2027");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1734_0)]
    public async Task a_saved_edition_opens_with_its_saved_date_format_and_prompt()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalSheet);
        await resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved ledger edition", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("14.07.2026");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("placeholder", "Transfer ledger date");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review saved edition", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Edition instructions", Exact = true })).ToHaveTextAsync("Use the transfer ledger");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalSheet);
        await referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved resident edition", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("July 14, 2026");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("placeholder", "Resident arrival date");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review saved edition", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Edition instructions", Exact = true })).ToHaveTextAsync("Use the resident letter");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1735_0)]
    public async Task the_submitting_units_offset_is_applied_when_the_transfer_editor_rebuilds()
    {
        var resident = await NewWorldInZone(Zones.Utc);
        await resident.Page.GotoAsync(CarePages.ArrivalZone);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply western transfer", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-13");

        var referral = await NewWorldInZone(Zones.Utc);
        await referral.Page.GotoAsync(CarePages.ArrivalZone);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply eastern transfer", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1736_0)]
    public async Task the_keyboard_profile_installs_the_shared_desks_open_and_close_shortcuts()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalKeys);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Install shared keyboard", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("F2");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("F3");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "false");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalKeys);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Install standard keyboard", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("F2");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "false");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1737_0)]
    public async Task the_chaplaincy_and_bed_desk_review_the_same_day_in_their_own_calendar()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalCalendarSystem);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use chaplaincy calendar", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("1448");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Schedule edition", Exact = true })).ToHaveTextAsync("Chaplaincy calendar");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalCalendarSystem);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use bed desk calendar", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Schedule edition", Exact = true })).ToHaveTextAsync("Gregorian calendar");
    }
}
