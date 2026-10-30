using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.ArrayOps;

namespace Alis.Reactive.PlaywrightTests.Components.Native.ArrayOps;

/// <summary>
/// As the executive director running the morning stand-up, I want each wing's census read out in the
/// figures I report (who is in residence, who is at risk of falling, whose review is next, what the wing
/// costs and needs) and the programs running today counted as I tick them, so that the stand-up works
/// from this morning's records rather than yesterday's.
/// </summary>
/// <remarks>
/// The census opens on Maple wing; Birch wing is a click away. Every figure is worked out in the browser
/// from the wing's resident list, and Maple's list puts a resident exactly on each threshold the figures
/// use: Margaret Hale's Morse score is 45, Rose Delgado's 24, Evelyn Hart is 90 and her review is in 7 days. Walter Brooks, in hospital, has the wing's highest Morse score and its soonest review,
/// and Harold Kim, first on the list, is neither the highest fall risk nor the next review. So a wrong
/// operator, a dropped filter or order, or a find that ignores who is in residence each shows a different
/// figure. Birch wing has nobody in hospital. Every array call the page makes is exercised.
/// </remarks>
[TestFixture]
public class WhenReadingTheWingCensus : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ArrayOps";

    private PagePlan<WingCensusModel> _plan = null!;

    private ILocator Wing => Page.Locator("#census-wing");
    private ILocator Figure(string id) => Page.Locator($"#census-{id}");
    private ILocator InHospital => Page.Locator("#census-in-hospital");
    private ILocator NoneInHospital => Page.Locator("#census-none-in-hospital");
    private ILocator BirchWing => Page.Locator("#birch-wing-btn");
    private ILocator MapleWing => Page.Locator("#maple-wing-btn");
    private ILocator ProgramsToday => Page.Locator("#programs-today-count");
    private NativeCheckListLocator Programs => new(Page, _plan.FindComponent(m => m.ProgramsToday)!.ElementId);

    private async Task OpenTheStandUp()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<WingCensusModel>.FromPage(Page);
        await Expect(Figure("total")).ToHaveTextAsync("8", new() { Timeout = 10000 });
    }

    private async Task SwitchToBirchWing()
    {
        await ClickWhenStable(BirchWing);
        await Expect(Wing).ToHaveTextAsync("Birch wing", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_stand_up_opens_on_maple_wing_with_everyone_on_the_census_and_who_is_away()
    {
        await OpenTheStandUp();

        await Expect(Wing).ToHaveTextAsync("Maple wing");
        await Expect(Figure("in-residence")).ToHaveTextAsync("5");
        await Expect(Figure("away")).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    // Margaret Hale's score is exactly 45 and Walter Brooks (70) is in hospital: a > would leave Margaret
    // out (1), and a count that ignored who is in residence would add Walter and Agnes Moore (4).
    [Test]
    public async Task residents_in_residence_with_a_morse_score_of_45_or_more_count_as_high_fall_risk()
    {
        await OpenTheStandUp();

        await Expect(Figure("high-fall-risk")).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    // Rose Delgado's score is exactly 24: a < would leave her out (2).
    [Test]
    public async Task a_morse_score_of_24_or_less_counts_as_low_fall_risk()
    {
        await OpenTheStandUp();

        await Expect(Figure("low-fall-risk")).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    // Evelyn Hart is exactly 90 and is not older than 90: a >= would count her (4).
    [Test]
    public async Task the_census_counts_the_residents_older_than_ninety()
    {
        await OpenTheStandUp();

        await Expect(Figure("over-ninety")).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    // Evelyn Hart's review is in exactly 7 days, which is next week: a <= would count it (5).
    [Test]
    public async Task reviews_due_within_seven_days_count_as_due_this_week()
    {
        await OpenTheStandUp();

        await Expect(Figure("reviews-due")).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }

    // 7 + 6 + 5 + 4 + 3 doses for the five residents in residence; the three away would add 9 (34).
    [Test]
    public async Task the_morning_medication_pass_counts_the_doses_of_residents_in_residence()
    {
        await OpenTheStandUp();

        await Expect(Figure("morning-doses")).ToHaveTextAsync("25");
        AssertNoConsoleErrors();
    }

    // $5,025 + $5,200.25 + $6,100.50 + $4,450.75 + $3,875.25: a sum that dropped the cents would say 24650,
    // one that kept the residents away 37651.75.
    [Test]
    public async Task monthly_charges_for_residents_in_residence_are_totalled_to_the_cent()
    {
        await OpenTheStandUp();

        await Expect(Figure("charges")).ToHaveTextAsync("24651.75");
        AssertNoConsoleErrors();
    }

    // 8 + 10.5 + 14.25 + 6.75 + 2.75 hours: a sum that dropped the fractions would say 40, one that kept
    // the residents away 63.25.
    [Test]
    public async Task personal_care_hours_for_residents_in_residence_are_totalled_to_the_quarter_hour()
    {
        await OpenTheStandUp();

        await Expect(Figure("care-hours")).ToHaveTextAsync("42.25");
        AssertNoConsoleErrors();
    }

    // Walter Brooks (70, in hospital) and Agnes Moore (55, on leave) score higher; Harold Kim is first on the
    // list and Ida Bell lowest. Unordered the page would name Harold, unfiltered Walter, ascending Ida.
    [Test]
    public async Task the_highest_fall_risk_named_is_the_highest_scoring_resident_in_residence()
    {
        await OpenTheStandUp();

        await Expect(Figure("highest-fall-risk")).ToHaveTextAsync("Evelyn Hart");
        AssertNoConsoleErrors();
    }

    // Walter Brooks (1 day, in hospital) and Agnes Moore (2 days, on leave) are due sooner; unordered the
    // page would name Harold Kim, unfiltered Walter.
    [Test]
    public async Task the_next_care_review_named_is_the_soonest_for_a_resident_in_residence()
    {
        await OpenTheStandUp();

        await Expect(Figure("next-review")).ToHaveTextAsync("Margaret Hale");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_in_hospital_puts_the_family_liaison_call_on_the_stand_up()
    {
        await OpenTheStandUp();

        await Expect(InHospital).ToBeVisibleAsync();
        await Expect(NoneInHospital).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // Birch's list is its own: George Webb is exactly 90 and his Morse score 60; nobody is in hospital.
    [Test]
    public async Task birch_wing_shows_its_own_census_and_that_nobody_is_in_hospital()
    {
        await OpenTheStandUp();

        await SwitchToBirchWing();

        await Expect(Figure("total")).ToHaveTextAsync("5");
        await Expect(Figure("in-residence")).ToHaveTextAsync("4");
        await Expect(Figure("high-fall-risk")).ToHaveTextAsync("1");
        await Expect(Figure("over-ninety")).ToHaveTextAsync("1");
        await Expect(Figure("charges")).ToHaveTextAsync("19251.25");
        await Expect(Figure("highest-fall-risk")).ToHaveTextAsync("George Webb");
        await Expect(NoneInHospital).ToBeVisibleAsync();
        await Expect(InHospital).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task going_back_to_maple_wing_brings_back_the_family_liaison_call()
    {
        await OpenTheStandUp();
        await SwitchToBirchWing();

        await ClickWhenStable(MapleWing);

        await Expect(Wing).ToHaveTextAsync("Maple wing", new() { Timeout = 10000 });
        await Expect(Figure("total")).ToHaveTextAsync("8");
        await Expect(InHospital).ToBeVisibleAsync();
        await Expect(NoneInHospital).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ticking_programs_counts_the_programs_running_today()
    {
        await OpenTheStandUp();
        await Expect(ProgramsToday).ToHaveTextAsync("0");

        await Programs.Tick(StandUpPrograms.ChairYoga);
        await Programs.Tick(StandUpPrograms.MusicHour);

        await Expect(ProgramsToday).ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_a_program_takes_it_off_todays_count()
    {
        await OpenTheStandUp();
        await Programs.Tick(StandUpPrograms.ChairYoga);
        await Programs.Tick(StandUpPrograms.MusicHour);
        await Expect(ProgramsToday).ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Programs.Clear(StandUpPrograms.ChairYoga);

        await Expect(ProgramsToday).ToHaveTextAsync("1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
