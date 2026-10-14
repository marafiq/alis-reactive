namespace Alis.Reactive.PlaywrightTests.Plugins.ArrayManager;

/// <summary>
/// As the night supervisor at the ward desk, I want the ward's census read out (who is in residence, their
/// care hours, where the round starts, anyone critical, any bed open), tonight's staffing planned from that
/// census, and a visitor's pass printed, so that the night starts staffed for the residents actually on
/// the ward and a visitor reaches them.
/// </summary>
/// <remarks>
/// The census is read through the page's plugins (Scripts/sandbox-plugins.ts): the array plugin counts,
/// filters, sums, plucks and tests the ward's residents by field name, the community plugin holds the beds
/// per ward (5) and slugify makes the ward's code. Birch ward opens with the desk: five residents fill its
/// five beds; Edith Morales, first on its list, is in hospital and Samuel Price on leave, so three are in
/// residence and the round starts with Thomas Reed. The memory-care ward has three residents, one critical.
/// The staffing service answers from the census it is sent: the ward's count in the X-Ward-Census header and
/// the residents in residence in the query, so a wrong or missing value reads as a different plan.
/// Not exercised here: a plugin member called only for its effect, which this page no longer makes (its
/// analytics call changed nothing anyone sees); WhenWatchingTheVisitBoard's
/// opening_the_board_records_it_in_the_activity_list fails when such a call (visitLog.Record) breaks.
/// </remarks>
[TestFixture]
public class WhenWorkingTheWardDesk : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Plugins/ArrayManager";

    private ILocator WardName => Page.Locator("#ward-name");
    private ILocator WardCode => Page.Locator("#ward-code");
    private ILocator Census => Page.Locator("#ward-census");
    private ILocator InResidence => Page.Locator("#ward-in-residence");
    private ILocator CareHours => Page.Locator("#ward-care-hours");
    private ILocator RoundStartsWith => Page.Locator("#ward-first-round");
    private ILocator SomeoneCritical => Page.Locator("#ward-critical");
    private ILocator NobodyCritical => Page.Locator("#ward-none-critical");
    private ILocator WardFull => Page.Locator("#ward-full");
    private ILocator BedsOpen => Page.Locator("#ward-beds-open");
    private ILocator OpenTheMemoryCareWard => Page.Locator("#memory-ward-btn");
    private ILocator RequestStaffing => Page.Locator("#request-staffing-btn");
    private ILocator StaffingPlan => Page.Locator("#staffing-plan");
    private ILocator PrintDanielsPass => Page.Locator("#print-pass-btn");
    private ILocator VisitorPass => Page.Locator("#visitor-pass");
    private ILocator PassHeading => Page.Locator("#pass-heading");
    private ILocator PassCharges => Page.Locator("#pass-charges");

    private async Task OpenTheDesk()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Census).ToHaveTextAsync("5", new() { Timeout = 10000 });
    }

    private async Task SwitchToTheMemoryCareWard()
    {
        await ClickWhenStable(OpenTheMemoryCareWard);
        await Expect(WardName).ToHaveTextAsync("Memory-Care Ward", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_desk_opens_on_birch_ward_under_its_ward_code()
    {
        await OpenTheDesk();

        await Expect(WardName).ToHaveTextAsync("Birch Ward");
        await Expect(WardCode).ToHaveTextAsync("birch-ward");
        AssertNoConsoleErrors();
    }

    // The array plugin counts the residents the filter keeps: all five would read 5.
    [Test]
    public async Task only_the_residents_in_residence_are_counted_in_residence()
    {
        await OpenTheDesk();

        await Expect(InResidence).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    // 4 + 6 + 5 hours for the residents in residence; with Edith Morales and Samuel Price it would be 20.
    [Test]
    public async Task todays_care_hours_are_those_of_the_residents_in_residence()
    {
        await OpenTheDesk();

        await Expect(CareHours).ToHaveTextAsync("15");
        AssertNoConsoleErrors();
    }

    // Edith Morales is first on the list but in hospital.
    [Test]
    public async Task the_morning_round_starts_with_the_first_resident_in_residence()
    {
        await OpenTheDesk();

        await Expect(RoundStartsWith).ToHaveTextAsync("Thomas Reed");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_ward_with_nobody_critical_says_so_and_a_ward_with_a_critical_resident_calls_it_out()
    {
        await OpenTheDesk();
        await Expect(NobodyCritical).ToBeVisibleAsync();
        await Expect(SomeoneCritical).ToBeHiddenAsync();

        await SwitchToTheMemoryCareWard();

        await Expect(SomeoneCritical).ToBeVisibleAsync();
        await Expect(NobodyCritical).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The ward's count against the community plugin's beds per ward: Birch's five fill its beds, the
    // memory-care ward's three leave two open.
    [Test]
    public async Task a_ward_whose_residents_fill_every_bed_says_so_and_one_with_room_says_beds_are_open()
    {
        await OpenTheDesk();
        await Expect(WardFull).ToBeVisibleAsync();
        await Expect(BedsOpen).ToBeHiddenAsync();

        await SwitchToTheMemoryCareWard();

        await Expect(BedsOpen).ToBeVisibleAsync();
        await Expect(WardFull).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The ward code is made from the ward's name each time a census loads.
    [Test]
    public async Task opening_the_memory_care_ward_reads_out_its_own_census_and_code()
    {
        await OpenTheDesk();

        await SwitchToTheMemoryCareWard();

        await Expect(WardCode).ToHaveTextAsync("memory-care-ward");
        await Expect(Census).ToHaveTextAsync("3");
        await Expect(InResidence).ToHaveTextAsync("2");
        await Expect(CareHours).ToHaveTextAsync("14");
        await Expect(RoundStartsWith).ToHaveTextAsync("Edith Lane");
        AssertNoConsoleErrors();
    }

    // An aide for every two of the 3 in residence, and a bed held for each of the 5 on the census who is
    // away. A header carrying the 3 would hold 0 beds; a query carrying the 5 would ask for 3 aides.
    [Test]
    public async Task tonights_staffing_is_planned_from_the_census_and_the_residents_in_residence()
    {
        await OpenTheDesk();
        await Expect(StaffingPlan).ToHaveTextAsync("Not requested yet tonight.");

        await ClickWhenStable(RequestStaffing);

        await Expect(StaffingPlan).ToHaveTextAsync(
            "Tonight: 2 aides for 3 residents in residence; 2 beds held for residents away.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The desk issues the pass with details the page knows: each reaches the plugin as its declared kind, a
    // bool (escorted), a long past int's range (the pass number), a DateTime (a Date on the day of the visit),
    // a decimal (the parking fee) and a double (the hours). The plugin writes "?" for a detail of another kind.
    [Test]
    public async Task printing_daniel_reeds_pass_shows_it_escorted_numbered_dated_and_charged()
    {
        await OpenTheDesk();
        await Expect(VisitorPass).ToBeHiddenAsync();

        await ClickWhenStable(PrintDanielsPass);

        await Expect(VisitorPass).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(PassHeading).ToHaveTextAsync("Escorted visitor · pass 4000000001 · 2026-11-14");
        await Expect(PassCharges).ToHaveTextAsync("Parking $12.50 · 1.5 hours");
        AssertNoConsoleErrors();
    }
}
