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
/// no other session; every desk is also told how many desks watch the week, on the same stream under
/// its own event name. The board also reads the week back off the visit planner: the wing's board
/// code and desk code (through the slugify plugin), the quiet, first weekend and first afternoon times
/// (arrays over the planner's slots), the weekend visits booked (an array over its change payload) and
/// what is left (its remaining function). Its activity list and spoken status line are plugins.
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
    private ILocator FirstAfternoon => Page.Locator("#board-first-afternoon");
    private ILocator DeskCode => _plan.TextBox(m => m.DeskCode).Input;
    private ILocator Desks => Page.Locator("#board-desks");
    private ILocator LogDesk => Page.Locator("#board-log-desk");
    private ILocator LogReleased => Page.Locator("#board-log-released");
    private ILocator LogLost => Page.Locator("#board-log-lost");
    private ILocator Greeter => Page.Locator("#board-greeter");
    private ILocator Announcer => Page.Locator("#board-announcer");
    private ILocator Activity => Page.Locator("#board-activity li");
    private ILocator ActivityCount => Page.Locator("#board-activity-count");

    private async Task OpenBoardForTheHales()
    {
        await NavigateToAndWaitForBoot(BoardFor + "hale");
        _plan = await PagePlan<VisitBoardModel>.FromPage(Page);
        await Expect(Planner.Times).ToHaveCountAsync(6);
    }

    // The Okafor family's desk, open in the same session as this test's page.
    private async Task<PlannerField> OpenTheOkaforDesk() => PlannerOn(await OpenTheOkaforDeskPage());

    private async Task<IPage> OpenTheOkaforDeskPage()
    {
        await WaitForTraceMessage("[alis:server-push] connected", 10000);
        var okaforDesk = await Context.NewPageAsync();
        await okaforDesk.GotoAsync(BaseUrl + BoardFor + "okafor");
        await Expect(PlannerOn(okaforDesk).Times).ToHaveCountAsync(6, new() { Timeout = 10000 });
        return okaforDesk;
    }

    private PlannerField PlannerOn(IPage desk) =>
        new(desk.Locator($"#{_plan.FindComponent(m => m.VisitSlots)!.ElementId}"));

    [Test]
    public async Task the_board_reads_the_wing_code_quiet_times_and_first_weekend_time_off_the_planner()
    {
        await OpenBoardForTheHales();

        await Expect(BoardCode).ToHaveTextAsync("garden-wing");
        await Expect(QuietTimes).ToHaveTextAsync("1");
        await Expect(FirstWeekend).ToHaveTextAsync("Sat 11:00 AM");
        AssertNoConsoleErrors();
    }

    // Find(label => label.EndsWith("PM")) over the slots' labels: the first label, "Mon 10:00 AM", is a
    // morning time, so a Find that ignored its predicate would name it instead of Tuesday's.
    [Test]
    public async Task the_board_names_the_first_afternoon_time_of_the_week()
    {
        await OpenBoardForTheHales();

        await Expect(FirstAfternoon).ToHaveTextAsync("Tue 3:00 PM");
        AssertNoConsoleErrors();
    }

    // The slugify plugin's result is written to a component's property: the desk code box, not text.
    [Test]
    public async Task the_desk_code_is_filled_in_from_the_wings_name()
    {
        await OpenBoardForTheHales();

        await Expect(DeskCode).ToHaveValueAsync("garden-wing");
        AssertNoConsoleErrors();
    }

    // The visitLog plugin's command records a fixed line, and its Entries member is read back into the
    // count; neither has any other way onto the page.
    [Test]
    public async Task opening_the_board_records_it_in_the_activity_list()
    {
        await OpenBoardForTheHales();

        await Expect(Activity).ToHaveCountAsync(1);
        await Expect(Activity.First).ToHaveTextAsync("Board opened for this week");
        await Expect(ActivityCount).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_counts_the_weekend_visits_and_says_what_is_left_until_the_week_is_full()
    {
        await OpenBoardForTheHales();

        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("1");
        await Expect(Capacity).ToHaveTextAsync("More than one visit left this week.");
        await Expect(Status).ToHaveTextAsync("Sat 11:00 AM is held for the Hale family: 1 of 1 booked visits fall on a weekend.");

        await Planner.Time("Sun 11:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("2");
        await Expect(Capacity).ToHaveTextAsync("One visit left this week.");

        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("2");
        await Expect(Capacity).ToHaveTextAsync("This week is full.");
        await Expect(Capacity).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\btext-warning\b"));
        AssertNoConsoleErrors();
    }

    // The take sends two array results: the weekend count in the request body and the booked count in the
    // X-Booked header, and the desk answers with both. Sat, then Sun, then Mon: 1 of 1, 2 of 2, 2 of 3.
    // An array result that did not reach the request would leave the desk saying 0.
    [Test]
    public async Task each_hold_says_how_many_of_the_familys_booked_visits_fall_on_a_weekend()
    {
        await OpenBoardForTheHales();

        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Sat 11:00 AM is held for the Hale family: 1 of 1 booked visits fall on a weekend.");

        await Planner.Time("Sun 11:00 AM").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Sun 11:00 AM is held for the Hale family: 2 of 2 booked visits fall on a weekend.");

        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Mon 10:00 AM is held for the Hale family: 2 of 3 booked visits fall on a weekend.");
        AssertNoConsoleErrors();
    }

    // The greeter note reads a group: the planner's IsBooked("sat-11") method, the slugify plugin over the
    // wing, and the family named in the URL. Releasing Saturday makes the group's first term false.
    [Test]
    public async Task the_weekend_greeter_is_asked_for_a_named_garden_wing_family_holding_saturday_eleven()
    {
        await OpenBoardForTheHales();

        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(Greeter).ToBeVisibleAsync();

        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Mon 10:00 AM is held for the Hale family: 1 of 2 booked visits fall on a weekend.");
        await Expect(Greeter).ToBeVisibleAsync();

        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("0");
        await Expect(Greeter).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // A desk whose link names no family: the group's URL term is empty, so Saturday at the Garden wing
    // asks for no greeter. The weekend count, written by the same handler, proves the handler ran.
    [Test]
    public async Task a_desk_whose_link_names_no_family_asks_for_no_greeter()
    {
        await NavigateToAndWaitForBoot(BoardFor);
        _plan = await PagePlan<VisitBoardModel>.FromPage(Page);
        await Expect(Planner.Times).ToHaveCountAsync(6);

        await Planner.Time("Sat 11:00 AM").ClickAsync();

        await Expect(WeekendBooked).ToHaveTextAsync("1");
        await Expect(Greeter).ToBeHiddenAsync();
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

    // The log's dispatch carries fixed values (false, 0) and a plugin result (the slug of the wing); a time
    // this family never booked is not one it lost.
    [Test]
    public async Task the_log_names_the_desk_and_that_this_family_lost_nothing_when_another_family_takes_an_open_time()
    {
        await OpenBoardForTheHales();
        var okaforDesk = await OpenTheOkaforDesk();

        await okaforDesk.Time("Tue 3:00 PM").ClickAsync();

        await Expect(Log).ToHaveTextAsync("Tue 3:00 PM was just taken by the Okafor family.");
        await Expect(LogDesk).ToHaveTextAsync("garden-wing");
        await Expect(LogReleased).ToHaveTextAsync("0");
        await Expect(LogLost).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The take is recorded with the visitLog plugin's command given the pushed message, its Entries are
    // read back, and the announce plugin (a function itself, called for its effect) reads the message out.
    [Test]
    public async Task a_time_another_desk_takes_is_recorded_in_the_activity_list_and_read_out()
    {
        await OpenBoardForTheHales();
        await Expect(ActivityCount).ToHaveTextAsync("1");
        var okaforDesk = await OpenTheOkaforDesk();

        await okaforDesk.Time("Tue 3:00 PM").ClickAsync();

        await Expect(Activity).ToHaveCountAsync(2);
        await Expect(Activity.First).ToHaveTextAsync("Board opened for this week");
        await Expect(Activity.Last).ToHaveTextAsync("Tue 3:00 PM was just taken by the Okafor family.");
        await Expect(ActivityCount).ToHaveTextAsync("2");
        await Expect(Announcer).ToHaveTextAsync("Tue 3:00 PM was just taken by the Okafor family.");
        AssertNoConsoleErrors();
    }

    // Each push runs the trigger again with its own payload: a second take replaces the first message and
    // marks a second time taken.
    [Test]
    public async Task every_time_another_desk_takes_shows_here_in_turn()
    {
        await OpenBoardForTheHales();
        var okaforDesk = await OpenTheOkaforDesk();

        await okaforDesk.Time("Tue 3:00 PM").ClickAsync();
        await Expect(Alert).ToHaveTextAsync("Tue 3:00 PM was just taken by the Okafor family.");
        await Expect(Log).ToHaveTextAsync("Tue 3:00 PM was just taken by the Okafor family.");

        await okaforDesk.Time("Mon 10:00 AM").ClickAsync();
        await Expect(Alert).ToHaveTextAsync("Mon 10:00 AM was just taken by the Okafor family.");
        await Expect(Log).ToHaveTextAsync("Mon 10:00 AM was just taken by the Okafor family.");
        await Expect(Planner.TakenUnder("Tue 3:00 PM")).ToHaveTextAsync("Taken by the Okafor family");
        await Expect(Planner.TakenUnder("Mon 10:00 AM")).ToHaveTextAsync("Taken by the Okafor family");
        await Expect(ActivityCount).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    // Both triggers on the board read one URL; the server counts each open stream as a desk. One
    // EventSource per page makes two desks; one per trigger would make four, and the Okafor desk, whose
    // streams open after the Hales' two, would never read two.
    [Test]
    public async Task the_board_says_how_many_desks_are_watching_the_week()
    {
        await OpenBoardForTheHales();
        await Expect(Desks).ToHaveTextAsync("1 desk is watching this week", new() { Timeout = 10000 });

        var okaforDesk = await OpenTheOkaforDeskPage();

        await Expect(okaforDesk.Locator("#board-desks")).ToHaveTextAsync("2 desks are watching this week", new() { Timeout = 10000 });
        await Expect(Desks).ToHaveTextAsync("2 desks are watching this week");
        AssertNoConsoleErrors();
    }

    // The stream sends two event types. The desks message must not run the SlotTaken pipeline (it would
    // write the alert, record a line and tell the log), and a slot-taken message must not run the desks
    // trigger (it would blank the count).
    [Test]
    public async Task a_desk_joining_leaves_the_taken_time_alert_alone_and_a_taken_time_leaves_the_desk_count()
    {
        await OpenBoardForTheHales();
        var okaforDesk = await OpenTheOkaforDeskPage();

        await Expect(Desks).ToHaveTextAsync("2 desks are watching this week", new() { Timeout = 10000 });
        await Expect(Alert).ToHaveTextAsync("No other desk has taken a time.");
        await Expect(ActivityCount).ToHaveTextAsync("1");
        await Expect(Log).ToHaveTextAsync("none");

        await PlannerOn(okaforDesk).Time("Tue 3:00 PM").ClickAsync();

        await Expect(Alert).ToHaveTextAsync("Tue 3:00 PM was just taken by the Okafor family.");
        await Expect(Desks).ToHaveTextAsync("2 desks are watching this week");
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

    // The log's dispatch from the branch that released the time carries true and 1, written into the plan.
    [Test]
    public async Task the_log_says_this_family_lost_a_booked_time_when_another_desk_takes_it()
    {
        await OpenBoardForTheHales();
        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(WeekendBooked).ToHaveTextAsync("1");
        var okaforDesk = await OpenTheOkaforDesk();

        await okaforDesk.Time("Sat 11:00 AM").ClickAsync();

        await Expect(Log).ToHaveTextAsync("Sat 11:00 AM was just taken by the Okafor family.");
        await Expect(LogReleased).ToHaveTextAsync("1");
        await Expect(LogLost).ToBeVisibleAsync();
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
