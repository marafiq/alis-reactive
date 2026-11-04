namespace Alis.Reactive.PlaywrightTests.Plugins.ArrayManager;

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

    [Test]
    public async Task only_the_residents_in_residence_are_counted_in_residence()
    {
        await OpenTheDesk();

        await Expect(InResidence).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task todays_care_hours_are_those_of_the_residents_in_residence()
    {
        await OpenTheDesk();

        await Expect(CareHours).ToHaveTextAsync("15");
        AssertNoConsoleErrors();
    }

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
