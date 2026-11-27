using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffUseSavedArrivalProfiles(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1738_0)]
    public async Task a_written_referral_can_use_its_culture_resolved_date_pattern()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalLetterIntake);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept written referral", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("July 23, 2026");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-23");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalLetterIntake);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Accept ordinary referral", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("July 23, 2026");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1739_0)]
    public async Task the_handover_stamp_uses_all_the_segments_of_the_selected_template()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalStationStamp);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use transport words", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("service weekday, transfer month/transfer day/transfer year departure hour:departure minute:departure second");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Stamp guide", Exact = true })).ToHaveTextAsync("Transcribe the departure stamp");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalStationStamp);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use facility words", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("ward weekday, ward month/ward day/ward year arrival hour:arrival minute:arrival second");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Stamp guide", Exact = true })).ToHaveTextAsync("Transcribe the arrival stamp");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1740_0)]
    public async Task the_saved_nursing_and_transport_templates_keep_their_own_mask_words()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalNursingMask);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("nursing month / nursing day / nursing year");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalTransportMask);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("transfer month / transfer day / transfer year");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1741_0)]
    public async Task returning_to_the_default_preserves_the_already_installed_draft_handler()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalRecoveryDesk);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep a draft", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use the default preference", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Preference", Exact = true })).ToHaveTextAsync("Default requested");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open next admission", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalRecoveryDesk);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use the default preference", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Preference", Exact = true })).ToHaveTextAsync("Default requested");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open next admission", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1742_0)]
    public async Task the_calendar_review_records_keyboard_or_desk_origin()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalReviewHistory);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Opening history", Exact = true })).ToHaveTextAsync("Keyboard calendar opened");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowUp");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Closing history", Exact = true })).ToHaveTextAsync("Keyboard review finished");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalReviewHistory);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open from booking desk", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Opening history", Exact = true })).ToHaveTextAsync("Desk calendar opened");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close from booking desk", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Closing history", Exact = true })).ToHaveTextAsync("Desk review finished");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1743_0)]
    public async Task an_unopened_calendar_and_a_displayed_period_make_distinct_allocation_notes()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPeriodNote);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare unopened note", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Period note", Exact = true })).ToHaveTextAsync("Open the calendar before filing the note");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPeriodNote);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation period", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByText("December 2027", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Period note", Exact = true })).ToHaveTextAsync("File the monthly allocation note");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1744_0)]
    public async Task the_receiving_desk_prints_the_booked_date_with_its_dispatch_label()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalDispatchLanguage);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use French desk", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Dispatch label", Exact = true })).ToHaveTextAsync("Date d’admission");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalDispatchLanguage);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use English desk", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Dispatch label", Exact = true })).ToHaveTextAsync("Admission date");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1745_0)]
    public async Task the_chaplaincy_referral_gives_the_bed_desk_its_gregorian_day()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalChaplaincy);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Book Muharram referral", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Referral date", Exact = true })).ToHaveTextAsync("29 Muharram 1448");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalChaplaincy);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Book Safar referral", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Referral date", Exact = true })).ToHaveTextAsync("2 Safar 1448");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1746_0)]
    public async Task hijri_allocation_compares_months_inside_the_same_planning_year()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalChaplaincy);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review Muharram allocation", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Allocation", Exact = true })).ToHaveTextAsync("Current chaplaincy allocation");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalChaplaincy);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review Safar allocation", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Allocation", Exact = true })).ToHaveTextAsync("Next month in the current allocation year");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1747_0)]
    public async Task required_calendar_modules_determine_the_referral_entry_instructions()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalChaplaincy);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare guided referral", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Entry instruction", Exact = true })).ToHaveTextAsync("Enter the guided Hijri date");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalChaplaincy);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare plain referral", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Entry instruction", Exact = true })).ToHaveTextAsync("Enter the ordinary date");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1748_0)]
    public async Task releasing_a_seasonal_limit_restores_its_default_extreme()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalSeason);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release the lower limit", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("1880-01-01");
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("1900-01-01");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalSeason);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release the upper limit", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).FillAsync("2200-12-31");
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2099-12-31");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1749_0)]
    public async Task the_standard_and_transfer_picker_editions_file_their_bound_dates()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalTransferEdition);
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("14.07.2026");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("aria-description", "Use the transfer letter date");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File this admission", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Filing receipt", Exact = true })).ToHaveTextAsync("Transfer filed for 14 July 2026");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalOrdinaryEdition);
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("7/14/2026");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File this admission", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Filing receipt", Exact = true })).ToHaveTextAsync("Admission filed for 14 July 2026");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1750_0)]
    public async Task clearing_a_calendar_width_allows_the_paper_form_to_use_a_full_row()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPaperRow);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use a full row", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print instruction", Exact = true })).ToHaveTextAsync("Reserve the whole row");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPaperRow);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use a bedside row", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print instruction", Exact = true })).ToHaveTextAsync("Reserve the bedside column");
    }
}
