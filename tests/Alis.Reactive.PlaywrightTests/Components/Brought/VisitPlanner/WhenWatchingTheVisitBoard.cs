using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.PlaywrightTests.Components.Brought.VisitPlanner;

/// <summary>
/// As a front desk coordinator, I want the visit board to show at once when a family at another desk
/// takes a time, so that I never promise a family a time someone else already holds.
/// </summary>
/// <remarks>
/// Each desk opens the board for the family it serves (?resident=margaret&amp;family=...). Desks in
/// one browser session are one community: a time taken at one desk is pushed to the others, and to
/// no other session. The board also reads the week back off the visit planner: the wing's board code
/// (through the slugify plugin), the quiet times and the first weekend time (arrays over the planner's
/// slots), the weekend visits booked (an array over its change payload) and what is left (its
/// remaining function).
/// </remarks>
[TestFixture]
public class WhenWatchingTheVisitBoard : PlaywrightTestBase
{
    private const string BoardFor = "/Sandbox/Components/VisitPlanner/Board?resident=margaret&family=";

    private PagePlan<VisitBoardModel> _plan = null!;

    private PlannerField Planner => new(Page.Locator($"#{_plan.FindComponent(m => m.VisitSlots)!.ElementId}"));
    private ILocator BoardCode => Page.Locator("#board-code");
    private ILocator QuietTimes => Page.Locator("#board-quiet");
    private ILocator FirstWeekend => Page.Locator("#board-first-weekend");
    private ILocator WeekendBooked => Page.Locator("#board-weekend");
    private ILocator Capacity => Page.Locator("#board-capacity");
    private ILocator Status => Page.Locator("#board-status");
    private ILocator Alert => Page.Locator("#board-alert");
    private ILocator Log => Page.Locator("#board-log");
    private ILocator LogBooked => Page.Locator("#board-log-booked");
    private ILocator LegendToggle => Page.Locator("#board-legend-toggle");
    private ILocator Legend => Page.Locator("#board-legend");

    private async Task OpenBoardForTheHales()
    {
        await NavigateToAndWaitForBoot(BoardFor + "hale");
        _plan = await PagePlan<VisitBoardModel>.FromPage(Page);
        await Expect(Planner.Times).ToHaveCountAsync(6);
    }

    // The Okafor family's desk, open in the same session as this test's page.
    private async Task<PlannerField> OpenTheOkaforDesk()
    {
        await WaitForTraceMessage("[alis:server-push] connected", 10000);
        var okaforDesk = await Context.NewPageAsync();
        await okaforDesk.GotoAsync(BaseUrl + BoardFor + "okafor");
        var planner = new PlannerField(okaforDesk.Locator($"#{_plan.FindComponent(m => m.VisitSlots)!.ElementId}"));
        await Expect(planner.Times).ToHaveCountAsync(6, new() { Timeout = 10000 });
        return planner;
    }

    [Test]
    public async Task the_board_reads_the_wing_code_quiet_times_and_first_weekend_time_off_the_planner()
    {
        await OpenBoardForTheHales();

        await Expect(BoardCode).ToHaveTextAsync("garden-wing");
        await Expect(QuietTimes).ToHaveTextAsync("1");
        await Expect(FirstWeekend).ToHaveTextAsync("Sat 11:00 AM");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_counts_the_weekend_visits_and_says_what_is_left_until_the_week_is_full()
    {
        await OpenBoardForTheHales();

        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("1");
        await Expect(Capacity).ToHaveTextAsync("More than one visit left this week.");
        await Expect(Status).ToHaveTextAsync("Sat 11:00 AM is held for the Hale family.");

        await Planner.Time("Sun 11:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("2");
        await Expect(Capacity).ToHaveTextAsync("One visit left this week.");

        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("2");
        await Expect(Capacity).ToHaveTextAsync("This week is full.");
        await Expect(Capacity).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\btext-warning\b"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_another_desk_takes_shows_as_taken_here_at_once()
    {
        await OpenBoardForTheHales();
        var okaforDesk = await OpenTheOkaforDesk();

        await okaforDesk.Time("Tue 3:00 PM").ClickAsync();

        await Expect(Planner.TakenUnder("Tue 3:00 PM")).ToHaveTextAsync("Taken by the Okafor family");
        await Expect(Planner.Time("Tue 3:00 PM")).ToBeDisabledAsync();
        await Expect(Alert).ToHaveTextAsync("Tue 3:00 PM was just taken by the Okafor family.");
        await Expect(Alert).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\btext-danger\b"));
        await Expect(Log).ToHaveTextAsync("Tue 3:00 PM was just taken by the Okafor family.");
        await Expect(okaforDesk.TakenUnder("Tue 3:00 PM")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_this_family_had_booked_is_released_when_another_desk_takes_it()
    {
        await OpenBoardForTheHales();
        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("1");
        var okaforDesk = await OpenTheOkaforDesk();

        await okaforDesk.Time("Sat 11:00 AM").ClickAsync();

        await Expect(Planner.Time("Sat 11:00 AM")).ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(Planner.TakenUnder("Sat 11:00 AM")).ToHaveTextAsync("Taken by the Okafor family");
        await Expect(Alert).ToHaveTextAsync("Sat 11:00 AM was just taken by the Okafor family.");
        await Expect(Alert).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\btext-danger\b"));
        await Expect(LogBooked).ToHaveTextAsync("1");
        await Expect(WeekendBooked).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_legend_explains_taken_and_quiet_times_and_hides_again()
    {
        await OpenBoardForTheHales();

        await LegendToggle.ClickAsync();
        await Expect(Legend).ToBeVisibleAsync();
        await Expect(Legend).ToContainTextAsync("Taken times belong to another desk's family");

        await LegendToggle.ClickAsync();
        await Expect(Legend).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    private sealed class PlannerField
    {
        private readonly ILocator _root;

        internal PlannerField(ILocator root) => _root = root;

        internal ILocator Times => _root.Locator("button.vp-slot");

        internal ILocator Time(string label) =>
            _root.GetByRole(AriaRole.Button, new() { Name = label, Exact = true });

        // A filter's inner locator is matched inside each item, so it starts from the page, not the root.
        internal ILocator TakenUnder(string label) =>
            _root.Locator("li.vp-item")
                .Filter(new() { Has = _root.Page.GetByRole(AriaRole.Button, new() { Name = label, Exact = true }) })
                .Locator(".vp-taken");
    }
}
