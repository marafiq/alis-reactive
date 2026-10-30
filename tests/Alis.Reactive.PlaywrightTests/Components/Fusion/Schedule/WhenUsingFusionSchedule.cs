using System.Globalization;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Schedule;

/// <summary>
/// As a staffing coordinator, I keep the week's AM, PM and night shifts covered where the schedule shows them:
/// I staff, add and cancel shifts in the assignment drawer and the new-assignment dialog, ask the agency for
/// the open ones, and change or delete an in-service in the schedule's own editor unless someone else has it
/// open, so that every shift has someone on it, every change is saved on the server, and no change is made
/// twice at once.
/// </summary>
/// <remarks>
/// <para>
/// Each test opens a new browser, whose schedule starts from the seeded weeks: shifts staffed or left open the
/// same way on every run (Oak Ridge's roster is full, so none of its shifts is open), and two in-services a week
/// for the AM staff, Falls prevention on Tuesday and Safe resident transfers on Thursday, which the staff
/// educator, Dana Whitfield, has open. The server answers every load with a summary of what it answered for:
/// the facility, how many shifts need coverage, and the dates.
/// </para>
/// <para>
/// Not exercised. <c>DataBind</c>: Syncfusion applies <c>eventSettings.dataSource</c> on the next tick without
/// it (ej2-base base.js:147) and reads the events through a promise either way (crud.js:41), so no later step of
/// the same pipeline sees a difference; the page calls it after each load. <c>Navigating</c>'s
/// <c>PreviousDate</c>, <c>PreviousView</c>, <c>ViewIndex</c> and <c>Cancel</c>, and <c>EventRendered</c>'s
/// <c>Type</c>: no step of the journey needs them; the payroll lock stops a move in <c>ActionBegin</c>, which
/// Syncfusion raises before <c>Navigating</c> (schedule.js:671-676). <c>CellClick</c>'s all-day row: it has no
/// height while no all-day event is on the week, so <c>IsAllDay</c> is clicked in a month's day.
/// <c>ActionBegin</c>'s <c>AddedRecords</c>: the page reads no added event; a new shift is saved on the server
/// before <c>AddEvent</c> shows it, and the editor never opens on an empty hour. Moving an in-service to other
/// hours in the editor: the server keeps it at the hour after the AM shift and its answer says so; no test
/// moves one.
/// </para>
/// <para>
/// An in-service saved or deleted in the schedule's editor reaches <c>ActionBegin</c> with its record in
/// <c>ChangedRecords</c> (crud.js:314-317, from the editor's save, event-window.js:1434 and 1725) or
/// <c>DeletedRecords</c> (crud.js:655-657, from the delete alert, quick-popups.js:1008-1009). The page cancels
/// the schedule's own change, posts the record to the server and reloads the week with what it saved. A
/// shift the drawer saves or cancels passes <c>ActionBegin</c> too (<c>SaveEvent</c>, crud.js:722;
/// <c>DeleteEvent</c>, crud.js:657), after the server already has it, and is not sent again.
/// </para>
/// <para>
/// The new-assignment dialog's staff list is located by <c>IdGenerator</c>: the dialog's partial plan lands
/// before the page's own plan in the document, so <c>PagePlan.FromPage</c> would read the partial's plan once it
/// loads. The page's own inputs are read from its plan at boot.
/// </para>
/// </remarks>
[TestFixture]
public class WhenUsingFusionSchedule : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Schedule";

    private static readonly Regex StaffedShift = new(@"\((CNA|RN|LPN)\)");

    private PagePlan<PointInTimeScheduleModel> _plan = null!;

    private FusionScheduleLocator Schedule => new(Page, "shift-schedule");
    private ILocator StaffedShifts => Schedule.Root.GetByText(StaffedShift);
    private ILocator OpenShifts => Schedule.Root.GetByText(new Regex("UNASSIGNED"));
    private ILocator Summary => Page.Locator("#schedule-summary");
    private ILocator Loading => Page.Locator("#schedule-loading");
    private ILocator Selection => Page.Locator("#schedule-selection");
    private ILocator AgencyStatus => Page.Locator("#agency-status");
    private ILocator AddShiftHint => Page.Locator("#add-shift-hint");
    private ILocator AllCoveredNote => Page.Locator("#all-covered-note");
    private ILocator InServiceNote => Page.Locator("#in-service-note");
    private ILocator LockedNote => Page.Locator("#schedule-locked-note");
    private ILocator NewAssignmentDialog => Page.Locator("#new-assignment-dialog");
    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator AssignmentForm => Page.Locator("#assignment-form");

    private DropDownListLocator Facility => _plan.DropDownList(m => m.SelectedFacilityId);
    private NativeCheckBoxLocator ScheduleLocked => _plan.NativeCheckBox(m => m.ScheduleLocked);
    private NativeCheckBoxLocator NeedsCoverageOnly => _plan.NativeCheckBox(m => m.NeedsCoverageOnly);
    private DropDownListLocator NewShiftStaff =>
        new(Page, IdGenerator.For<NewAssignmentModel>(m => m.StaffId));

    // The AM shift runs 6:00 AM to 2:00 PM, so its column is open at 4:00 PM every day.
    private static DateTime OpenHourToday => DateTime.Today.AddHours(16);

    // The whole schedule fits in the window, so a cell is in view exactly when the schedule's own
    // scrolling shows it.
    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.ViewportSize = new() { Width = 1280, Height = 1400 };
        return options;
    }

    private async Task NavigateAndWaitForSchedule()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<PointInTimeScheduleModel>.FromPage(Page);
        await Expect(Summary).ToHaveTextAsync(WeekSummary("Maple Grove", DateTime.Today), new() { Timeout = 10000 });
    }

    // The server's summary of a week: Sunday to Saturday, three shifts a day.
    private static Regex WeekSummary(string facility, DateTime day)
    {
        var sunday = day.Date.AddDays(-(int)day.DayOfWeek);
        return SummaryOf(facility, 21, sunday, sunday.AddDays(6));
    }

    // The server's summary of a month as the month view shows it: whole weeks, from the Sunday on or before the
    // first to the Saturday on or after the last.
    private static Regex MonthSummary(string facility, DateTime day)
    {
        var first = new DateTime(day.Year, day.Month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var from = first.AddDays(-(int)first.DayOfWeek);
        var to = last.AddDays(6 - (int)last.DayOfWeek);
        return SummaryOf(facility, ((to - from).Days + 1) * 3, from, to);
    }

    // "Maple Grove: 6 of 21 shifts need coverage from October 11 to October 17, 2026."
    private static Regex SummaryOf(string facility, int shifts, DateTime from, DateTime to) =>
        new("^" + Regex.Escape($"{facility}: ") + @"\d+" +
            Regex.Escape($" of {shifts} shifts need coverage from {from.ToString("MMMM d", CultureInfo.InvariantCulture)} " +
                         $"to {to.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)}.") + "$");

    private async Task<int> ShiftsNeedingCoverage() =>
        int.Parse(Regex.Match(await Summary.InnerTextAsync(), @": (\d+) of").Groups[1].Value, CultureInfo.InvariantCulture);

    private async Task ClickOpenHourToday()
    {
        var cell = Schedule.Cell(OpenHourToday, groupIndex: 0);
        await cell.ScrollIntoViewIfNeededAsync();
        await cell.ClickAsync();
    }

    // The list opens from its own button; a click outside the modal dialog would close the dialog.
    private async Task ChooseNewShiftStaff(string staff)
    {
        await NewShiftStaff.Open();
        await NewShiftStaff.Popup.GetByText(staff, new() { Exact = true }).ClickAsync();
        await Expect(NewShiftStaff.Popup).ToBeHiddenAsync();
    }

    private async Task AddShiftAtOpenHourToday(string staff)
    {
        await ClickOpenHourToday();
        await Expect(NewAssignmentDialog).ToContainTextAsync("Staff Member", new() { Timeout = 10000 });
        await ChooseNewShiftStaff(staff);
        await ClickWhenStable(Page.Locator("#btn-create-assignment"));
    }

    private ILocator StaffChoice(string staff) =>
        AssignmentForm.GetByRole(AriaRole.Radio, new() { Name = staff, Exact = true });

    // Opens the first open shift's assignment in the drawer and chooses Tom Harding; returns where the shift is.
    private async Task<(DateTime Start, int Group)> ChooseTomForTheFirstOpenShift()
    {
        var openShift = Schedule.Appointments(new Regex("UNASSIGNED")).First;
        var start = await Schedule.StartOf(openShift);
        var group = await Schedule.GroupOf(openShift);
        await Schedule.Open(openShift);
        await Schedule.Details.GetByText("Assign Staff", new() { Exact = true }).ClickAsync();
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 10000 });
        await Expect(AssignmentForm).ToBeVisibleAsync(new() { Timeout = 10000 });
        await AssignmentForm.GetByText("Tom Harding (LPN)").ClickAsync();
        return (start, group);
    }

    private const string RenamedFallsPreventionTitle = "In-service: Falls prevention and hip protectors";

    // Falls prevention is the AM staff's in-service on this week's Tuesday at 2:30 PM, after their shift.
    private static DateTime FallsPreventionStart => DateTime.Today.AddDays(2 - (int)DateTime.Today.DayOfWeek).AddHours(14.5);

    private ILocator FallsPrevention =>
        Schedule.Appointment("In-service: Falls prevention", FallsPreventionStart, groupIndex: 0);

    private ILocator RenamedFallsPrevention =>
        Schedule.Appointment(RenamedFallsPreventionTitle, FallsPreventionStart, groupIndex: 0);

    // The server answers that Falls prevention is the coordinator's to change, and the editor stays open on it.
    private async Task OpenFallsPreventionInTheEditor()
    {
        await Schedule.OpenToChange(FallsPrevention);
        await Expect(InServiceNote).ToHaveTextAsync(
            "The Falls prevention in-service is yours to change until you close it.", new() { Timeout = 10000 });
    }

    // Holds the page's requests to the URL until the test releases them; Asked completes once the page sent one.
    private async Task<(Task Asked, SemaphoreSlim Released)> Hold(string url)
    {
        var asked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var released = new SemaphoreSlim(0);
        await Page.RouteAsync(url, async route =>
        {
            asked.TrySetResult();
            await released.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        return (asked.Task, released);
    }

    [Test]
    public async Task the_week_opens_with_its_shifts_and_how_many_need_coverage()
    {
        await NavigateAndWaitForSchedule();

        await Expect(Summary).ToHaveTextAsync(WeekSummary("Maple Grove", DateTime.Today));
        await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(OpenShifts.First).ToBeVisibleAsync();
        await Expect(Schedule.Appointments("In-service: Falls prevention")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_facility_reloads_the_schedule_until_it_is_ready()
    {
        await NavigateAndWaitForSchedule();
        await Expect(Loading).ToBeHiddenAsync(new() { Timeout = 10000 });
        var shiftsReleased = new SemaphoreSlim(0);
        await Page.RouteAsync("**/Schedule/Shifts/**", async route =>
        {
            await shiftsReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });

        await Facility.Select("Sunrise Gardens");

        // The schedule says it is loading while Sunrise Gardens' shifts are asked for; only the schedule's
        // dataBound, after they bind, takes that away.
        await Expect(Loading).ToBeVisibleAsync(new() { Timeout = 10000 });
        shiftsReleased.Release();
        await Expect(Summary).ToHaveTextAsync(WeekSummary("Sunrise Gardens", DateTime.Today), new() { Timeout = 10000 });
        await Expect(Loading).ToBeHiddenAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_facility_keeps_the_week_the_schedule_shows()
    {
        await NavigateAndWaitForSchedule();
        var nextWeek = DateTime.Today.AddDays(7);
        await Schedule.Next.ClickAsync();
        await Expect(Summary).ToHaveTextAsync(WeekSummary("Maple Grove", nextWeek), new() { Timeout = 10000 });

        await Facility.Select("Sunrise Gardens");

        await Expect(Summary).ToHaveTextAsync(WeekSummary("Sunrise Gardens", nextWeek), new() { Timeout = 10000 });
        await Expect(StaffedShifts.First).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_to_next_week_loads_that_weeks_shifts_and_names_the_week()
    {
        await NavigateAndWaitForSchedule();

        await Schedule.Next.ClickAsync();

        // The week's grid starts empty: only the navigating handler's request fills it.
        await Expect(Summary).ToHaveTextAsync(WeekSummary("Maple Grove", DateTime.Today.AddDays(7)), new() { Timeout = 10000 });
        await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task switching_to_the_month_loads_every_shift_the_month_shows()
    {
        await NavigateAndWaitForSchedule();

        await Schedule.ViewButton("Month").ClickAsync();

        await Expect(Summary).ToHaveTextAsync(MonthSummary("Maple Grove", DateTime.Today), new() { Timeout = 10000 });
        await Expect(StaffedShifts.First).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_to_the_next_month_loads_that_months_shifts()
    {
        await NavigateAndWaitForSchedule();
        await Schedule.ViewButton("Month").ClickAsync();
        await Expect(Summary).ToHaveTextAsync(MonthSummary("Maple Grove", DateTime.Today), new() { Timeout = 10000 });

        await Schedule.Next.ClickAsync();

        await Expect(Summary).ToHaveTextAsync(MonthSummary("Maple Grove", DateTime.Today.AddMonths(1)), new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // A PM cell holds one shift a day, so its lower corner is empty in every month.
    [Test]
    public async Task clicking_a_day_of_the_month_asks_for_the_hour_the_shift_starts()
    {
        await NavigateAndWaitForSchedule();
        await Schedule.ViewButton("Month").ClickAsync();
        await Expect(Summary).ToHaveTextAsync(MonthSummary("Maple Grove", DateTime.Today), new() { Timeout = 10000 });

        await Expect(AddShiftHint).ToBeHiddenAsync();

        await Schedule.ClickEmptyPartOf(Schedule.Cell(DateTime.Today, groupIndex: 1));

        await Expect(AddShiftHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(AddShiftHint).ToHaveTextAsync("A shift starts at an hour: open a day or a week and click the hour it starts.");
        await Expect(NewAssignmentDialog).ToBeHiddenAsync();
        await Expect(Schedule.Details).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_an_hour_after_the_hint_offers_the_shift_and_takes_the_hint_away()
    {
        await NavigateAndWaitForSchedule();
        await Schedule.ViewButton("Month").ClickAsync();
        await Expect(Summary).ToHaveTextAsync(MonthSummary("Maple Grove", DateTime.Today), new() { Timeout = 10000 });
        await Schedule.ClickEmptyPartOf(Schedule.Cell(DateTime.Today, groupIndex: 1));
        await Expect(AddShiftHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Schedule.ViewButton("Week").ClickAsync();
        await Expect(Summary).ToHaveTextAsync(WeekSummary("Maple Grove", DateTime.Today), new() { Timeout = 10000 });

        await ClickOpenHourToday();

        await Expect(NewAssignmentDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(AddShiftHint).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_an_open_hour_offers_a_new_assignment_for_that_shift_and_time()
    {
        await NavigateAndWaitForSchedule();

        await ClickOpenHourToday();

        // The form loads with the clicked cell's shift, start and end, read from the cellClick payload.
        await Expect(NewAssignmentDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(NewAssignmentDialog).ToContainTextAsync("Shift: AM Shift (6:00 AM - 2:00 PM)", new() { Timeout = 10000 });
        await Expect(NewAssignmentDialog).ToContainTextAsync(
            "Start: " + OpenHourToday.ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture));
        await Expect(NewAssignmentDialog).ToContainTextAsync(
            "End: " + OpenHourToday.AddHours(1).ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture));

        // The empty cell's own details are cancelled in popupOpen, so only the dialog shows.
        await Expect(Schedule.Details).ToHaveCountAsync(0);
        await Expect(AddShiftHint).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_staff_to_an_open_hour_adds_the_shift_to_the_week()
    {
        await NavigateAndWaitForSchedule();
        var weekLoads = 0;
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Schedule/Shifts/")) Interlocked.Increment(ref weekLoads);
        };
        var janetsShifts = await Schedule.Appointments("Janet Brooks (CNA)").CountAsync();

        await AddShiftAtOpenHourToday("Janet Brooks (CNA)");

        await Expect(NewAssignmentDialog).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Schedule.Appointments("Janet Brooks (CNA)")).ToHaveCountAsync(janetsShifts + 1, new() { Timeout = 10000 });
        await Expect(Schedule.Appointment("Janet Brooks (CNA)", OpenHourToday, groupIndex: 0)).ToBeVisibleAsync();
        Assert.That(weekLoads, Is.EqualTo(0), "The new shift joins the week the schedule shows; the week is not reloaded.");

        // The shift was saved in this coordinator's schedule, so it is there after a reload.
        await Page.ReloadAsync();
        await WaitForTraceMessage("booted", 10000);
        await Expect(Schedule.Appointment("Janet Brooks (CNA)", OpenHourToday, groupIndex: 0)).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_a_new_assignment_closes_its_dialog()
    {
        await NavigateAndWaitForSchedule();
        await ClickOpenHourToday();
        await Expect(NewAssignmentDialog).ToContainTextAsync("Staff Member", new() { Timeout = 10000 });

        await Page.Locator("#btn-cancel-new-assignment").ClickAsync();

        await Expect(NewAssignmentDialog).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_outside_the_new_assignment_dialog_closes_it()
    {
        await NavigateAndWaitForSchedule();
        await ClickOpenHourToday();
        await Expect(NewAssignmentDialog).ToBeVisibleAsync(new() { Timeout = 10000 });

        // The modal overlay sits beside the dialog; a click in its corner is a click outside the dialog.
        await Page.Locator("#new-assignment-dialog + .e-dlg-overlay")
            .ClickAsync(new() { Position = new() { X = 5, Y = 5 } });

        await Expect(NewAssignmentDialog).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_a_shift_takes_it_off_the_week()
    {
        await NavigateAndWaitForSchedule();
        await AddShiftAtOpenHourToday("Pat Nguyen (CNA)");
        var patsShift = Schedule.Appointment("Pat Nguyen (CNA)", OpenHourToday, groupIndex: 0);
        await Expect(patsShift).ToBeVisibleAsync(new() { Timeout = 10000 });

        await Schedule.Open(patsShift);
        await Schedule.Details.GetByText("Edit", new() { Exact = true }).ClickAsync();
        await Expect(AssignmentForm).ToBeVisibleAsync(new() { Timeout = 10000 });
        await ClickWhenStable(Page.Locator("#btn-cancel-shift"));

        await Expect(patsShift).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Page.ReloadAsync();
        await WaitForTraceMessage("booted", 10000);
        await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(patsShift).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_an_assigned_shift_shows_staff_details_and_edit_actions()
    {
        await NavigateAndWaitForSchedule();

        await Schedule.Open(StaffedShifts.First);

        // The details' template binds the clicked shift: its staff phone in the content, Edit and Reassign in
        // the footer. None of this exists until the popup renders the shift.
        await Expect(Schedule.Details.GetByText("Reassign", new() { Exact = true })).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Schedule.Details.GetByText(new Regex(@"\d{3}-\d{4}"))).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_an_unassigned_shift_offers_to_assign_staff()
    {
        await NavigateAndWaitForSchedule();

        await Schedule.Open(OpenShifts.First);

        await Expect(Schedule.Details.GetByText("Assign Staff", new() { Exact = true })).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Schedule.Details.GetByText("Reassign", new() { Exact = true })).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task editing_a_shift_opens_the_edit_drawer_with_the_assignment_form()
    {
        await NavigateAndWaitForSchedule();
        var shift = StaffedShifts.First;
        var staff = await shift.InnerTextAsync();
        await Schedule.Open(shift);

        await Schedule.Details.GetByText("Edit", new() { Exact = true }).ClickAsync();

        // The drawer loads the clicked shift's assignment: its staff member is the one chosen.
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 10000 });
        await Expect(Page.Locator("#alis-drawer-title")).ToHaveTextAsync("Edit Shift Assignment");
        await Expect(StaffChoice(staff)).ToBeCheckedAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reassigning_a_shift_opens_the_assignment_drawer()
    {
        await NavigateAndWaitForSchedule();
        var shift = StaffedShifts.First;
        var staff = await shift.InnerTextAsync();
        await Schedule.Open(shift);

        await Schedule.Details.GetByText("Reassign", new() { Exact = true }).ClickAsync();

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 10000 });
        await Expect(Page.Locator("#alis-drawer-title")).ToHaveTextAsync("Reassign Shift");
        await Expect(StaffChoice(staff)).ToBeCheckedAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_staff_to_an_open_shift_reduces_open_shifts_and_persists_on_reload()
    {
        await NavigateAndWaitForSchedule();
        var before = await ShiftsNeedingCoverage();
        Assert.That(before, Is.GreaterThan(0), "the scenario needs at least one open shift to cover");
        var (start, group) = await ChooseTomForTheFirstOpenShift();

        await ClickWhenStable(Page.Locator("#btn-save-assignment"));

        // The shift shows Tom where it is, and the week the server answers with has one fewer open shift.
        await Expect(Schedule.Appointment("Tom Harding (LPN)", start, group).First).ToBeVisibleAsync(new() { Timeout = 15000 });
        await Expect(Summary).ToHaveTextAsync(new Regex($@": {before - 1} of 21 shifts"), new() { Timeout = 15000 });

        // It was saved on the server, not only in the schedule.
        await Page.ReloadAsync();
        await WaitForTraceMessage("booted", 10000);
        await Expect(Summary).ToHaveTextAsync(new Regex($@": {before - 1} of 21 shifts"), new() { Timeout = 10000 });
        await Expect(Schedule.Appointment("Tom Harding (LPN)", start, group).First).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    // The drawer's save hands the saved shift to the schedule, which raises the same ActionBegin an in-service
    // saved in the editor raises; the server already has the shift.
    [Test]
    public async Task a_shift_staffed_in_the_drawer_is_not_sent_again_as_an_in_service()
    {
        await NavigateAndWaitForSchedule();
        var before = await ShiftsNeedingCoverage();
        Assert.That(before, Is.GreaterThan(0), "the scenario needs at least one open shift to cover");
        var inServiceSaves = 0;
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Schedule/SaveInService")) Interlocked.Increment(ref inServiceSaves);
        };
        var (start, group) = await ChooseTomForTheFirstOpenShift();

        await ClickWhenStable(Page.Locator("#btn-save-assignment"));

        await Expect(Schedule.Appointment("Tom Harding (LPN)", start, group).First).ToBeVisibleAsync(new() { Timeout = 15000 });
        await Expect(Summary).ToHaveTextAsync(new Regex($@": {before - 1} of 21 shifts"), new() { Timeout = 15000 });
        Assert.That(inServiceSaves, Is.EqualTo(0), "A shift is no in-service; the server is not asked to save it as one.");
        AssertNoConsoleErrors();
    }

    // The schedule's editor opens on the first shift no one covers, and the page's PopupOpen hands it to
    // the drawer: an open shift has no staff member chosen, where an assigned one would.
    [Test]
    public async Task covering_the_next_open_shift_opens_its_assignment_form_with_no_one_chosen()
    {
        await NavigateAndWaitForSchedule();
        await Expect(OpenShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#cover-next-open-shift"));

        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 10000 });
        await Expect(AssignmentForm).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(AssignmentForm.GetByRole(AriaRole.Radio).First).ToBeVisibleAsync();
        await Expect(AssignmentForm.GetByRole(AriaRole.Radio, new() { Checked = true })).ToHaveCountAsync(0);
        await Expect(Schedule.Editor).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // Oak Ridge's roster is full: the schedule holds no open shift for the editor to open on.
    [Test]
    public async Task covering_the_next_open_shift_of_a_full_roster_says_every_shift_is_covered()
    {
        await NavigateAndWaitForSchedule();
        await Facility.Select("Oak Ridge Senior Living");
        await Expect(Summary).ToHaveTextAsync(new Regex("^Oak Ridge Senior Living: 0 of 21 shifts need coverage"), new() { Timeout = 10000 });
        await Expect(AllCoveredNote).ToBeHiddenAsync();

        await ClickWhenStable(Page.Locator("#cover-next-open-shift"));

        await Expect(AllCoveredNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(AllCoveredNote).ToHaveTextAsync("Every shift on the schedule is covered.");
        await Expect(AssignmentForm).ToHaveCountAsync(0);
        await Expect(Schedule.Editor).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task double_clicking_a_shift_opens_the_assignment_drawer_instead_of_the_built_in_editor()
    {
        await NavigateAndWaitForSchedule();
        var shift = StaffedShifts.First;
        var staff = await shift.InnerTextAsync();

        await Schedule.OpenToChange(shift);

        // popupOpen cancels Syncfusion's editor (type "Editor") and loads the shift's assignment instead.
        await Expect(StaffChoice(staff)).ToBeCheckedAsync(new() { Timeout = 10000 });
        await Expect(Schedule.Editor).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_a_shifts_details_clears_the_selected_shift()
    {
        await NavigateAndWaitForSchedule();
        var shift = StaffedShifts.First;
        var staff = await shift.InnerTextAsync();

        await Schedule.Open(shift);
        await Expect(Selection).ToHaveTextAsync(staff, new() { Timeout = 10000 });

        await Schedule.DetailsClose.ClickAsync();

        await Expect(Selection).ToHaveTextAsync("Nothing selected.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_schedule_locked_for_payroll_stays_on_its_week()
    {
        await NavigateAndWaitForSchedule();
        var shownWeek = await Schedule.DateRange.InnerTextAsync();
        await ScheduleLocked.Check();
        await Expect(LockedNote).ToBeHiddenAsync();

        await Schedule.Next.ClickAsync();

        await Expect(LockedNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(LockedNote).ToHaveTextAsync("The schedule is locked for payroll; it stays on this week.");
        await Expect(Schedule.DateRange).ToHaveTextAsync(shownWeek);
        await Expect(Summary).ToHaveTextAsync(WeekSummary("Maple Grove", DateTime.Today));
        await Expect(StaffedShifts.First).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_schedule_locked_for_payroll_does_not_open_a_shift()
    {
        await NavigateAndWaitForSchedule();
        await ScheduleLocked.Check();
        await Expect(LockedNote).ToBeHiddenAsync();

        await Schedule.Open(StaffedShifts.First);

        await Expect(LockedNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(LockedNote).ToHaveTextAsync("The schedule is locked for payroll; unlock it to open a shift.");
        await Expect(Schedule.Details).ToHaveCountAsync(0);

        await ScheduleLocked.Input.UncheckAsync();
        await Schedule.Open(StaffedShifts.First);

        await Expect(Schedule.Details.GetByText("Reassign", new() { Exact = true })).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showing_only_shifts_that_need_coverage_hides_the_staffed_shifts()
    {
        await NavigateAndWaitForSchedule();
        await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(OpenShifts.First).ToBeVisibleAsync();

        await NeedsCoverageOnly.Check();

        await Expect(StaffedShifts).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(OpenShifts.First).ToBeVisibleAsync();

        await NeedsCoverageOnly.Input.UncheckAsync();

        await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task jumping_to_a_shift_brings_its_first_hour_into_view()
    {
        await NavigateAndWaitForSchedule();
        var amShiftStart = Schedule.Cell(DateTime.Today.AddHours(6), groupIndex: 0);
        var nightShiftStart = Schedule.Cell(DateTime.Today.AddHours(22), groupIndex: 2);

        // The schedule opens at the start of the work hours (6 AM), so the night shift comes first.
        await Page.Locator("#schedule-show-night").ClickAsync();

        await Expect(nightShiftStart).ToBeInViewportAsync(new() { Timeout = 5000 });
        await Expect(amShiftStart).Not.ToBeInViewportAsync();

        await Page.Locator("#schedule-show-morning").ClickAsync();

        await Expect(amShiftStart).ToBeInViewportAsync(new() { Timeout = 5000 });
        await Expect(nightShiftStart).Not.ToBeInViewportAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task printing_the_schedule_opens_the_print_view_with_the_weeks_shifts()
    {
        await NavigateAndWaitForSchedule();
        await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        var printView = await Page.RunAndWaitForPopupAsync(async () =>
        {
            await ClickWhenStable(Page.Locator("#print-btn"));
        });

        await printView.WaitForLoadStateAsync();
        await Expect(printView.Locator("body")).ToContainTextAsync(StaffedShift, new() { Timeout = 10000 });
        await Expect(printView.Locator("body")).ToContainTextAsync("UNASSIGNED", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The schedule's own events go to the agency; it counts the open ones among them, which are the ones the
    // server's summary counts.
    [Test]
    public async Task asking_the_agency_sends_every_open_shift_the_schedule_holds()
    {
        await NavigateAndWaitForSchedule();
        var open = await ShiftsNeedingCoverage();
        Assert.That(open, Is.GreaterThan(0), "the scenario needs at least one open shift to send");

        await ClickWhenStable(Page.Locator("#ask-agency-btn"));

        await Expect(AgencyStatus).ToHaveTextAsync(
            $"Asked Staffing Plus to cover {open} open {(open == 1 ? "shift" : "shifts")}.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task double_clicking_an_in_service_opens_it_in_the_schedules_own_editor()
    {
        await NavigateAndWaitForSchedule();

        await Schedule.OpenToChange(Schedule.Appointments("In-service: Falls prevention"));

        await Expect(Schedule.Editor).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Schedule.EditorField("subject")).ToHaveValueAsync("In-service: Falls prevention");
        await Expect(InServiceNote).ToHaveTextAsync("The Falls prevention in-service is yours to change until you close it.", new() { Timeout = 10000 });
        // The server's answer that it is free leaves the editor open.
        await Expect(Schedule.Editor).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_an_in_service_unsaved_takes_away_the_note_that_it_is_yours()
    {
        await NavigateAndWaitForSchedule();
        await Schedule.OpenToChange(Schedule.Appointments("In-service: Falls prevention"));
        await Expect(InServiceNote).ToBeVisibleAsync(new() { Timeout = 10000 });

        await Schedule.EditorCancel.ClickAsync();

        await Expect(Schedule.Editor).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(InServiceNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The editor is open while the server is asked; nothing the coordinator does closes it. The server's
    // answer that Dana Whitfield has the in-service open is what closes it.
    [Test]
    public async Task an_in_service_someone_else_is_changing_closes_and_says_who_has_it()
    {
        await NavigateAndWaitForSchedule();
        var answerReleased = new SemaphoreSlim(0);
        await Page.RouteAsync("**/Schedule/InServiceClaim", async route =>
        {
            await answerReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });

        await Schedule.OpenToChange(Schedule.Appointments("In-service: Safe resident transfers"));
        await Expect(Schedule.Editor).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Schedule.EditorField("subject")).ToHaveValueAsync("In-service: Safe resident transfers");
        await Expect(InServiceNote).ToBeHiddenAsync();

        answerReleased.Release();

        await Expect(InServiceNote).ToHaveTextAsync(
            "Dana Whitfield is changing the Safe resident transfers in-service right now, so it was closed here. Try again in a few minutes.",
            new() { Timeout = 10000 });
        await Expect(Schedule.Editor).ToBeHiddenAsync(new() { Timeout = 5000 });
        // Closing the editor hides the note; the refusal shows its note after the close.
        await Expect(InServiceNote).ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }

    [Test]
    public async Task changing_an_in_services_title_in_the_editor_keeps_it_after_the_week_reloads()
    {
        await NavigateAndWaitForSchedule();
        await OpenFallsPreventionInTheEditor();
        await Schedule.EditorField("subject").FillAsync(RenamedFallsPreventionTitle);

        await Schedule.EditorSave.ClickAsync();

        // The server says what it saved, and the week it reloads shows the new title.
        await Expect(InServiceNote).ToHaveTextAsync(
            "Saved the Falls prevention and hip protectors in-service, on Tuesday at 2:30 PM after the AM shift.",
            new() { Timeout = 10000 });
        await Expect(RenamedFallsPrevention).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(FallsPrevention).ToHaveCountAsync(0);

        // It was saved on the server, not only in the schedule.
        await Page.ReloadAsync();
        await WaitForTraceMessage("booted", 10000);
        await Expect(RenamedFallsPrevention).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(FallsPrevention).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    // The schedule's own save of the change is cancelled. Were it not, the schedule would show the new title before
    // the server had saved anything, and the reload the page makes after any save the schedule makes would put the
    // old one back; the week's loads are held, so that unsaved title would stay on screen.
    [Test]
    public async Task a_changed_in_service_shows_its_new_title_only_once_the_server_saved_it()
    {
        await NavigateAndWaitForSchedule();
        var (saveAsked, saveReleased) = await Hold("**/Schedule/SaveInService");
        var (_, weekReleased) = await Hold("**/Schedule/Shifts/**");
        await OpenFallsPreventionInTheEditor();
        await Schedule.EditorField("subject").FillAsync(RenamedFallsPreventionTitle);

        await Schedule.EditorSave.ClickAsync();

        await saveAsked.WaitAsync(TimeSpan.FromSeconds(10));
        await Expect(Schedule.Editor).ToBeHiddenAsync();
        await Expect(FallsPrevention).ToBeVisibleAsync();
        await Expect(RenamedFallsPrevention).ToHaveCountAsync(0);

        saveReleased.Release();
        weekReleased.Release();

        await Expect(RenamedFallsPrevention).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(FallsPrevention).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task deleting_an_in_service_in_the_editor_takes_it_off_the_week()
    {
        await NavigateAndWaitForSchedule();
        await OpenFallsPreventionInTheEditor();
        await Schedule.EditorDelete.ClickAsync();

        await Schedule.ConfirmDelete.ClickAsync();

        await Expect(InServiceNote).ToHaveTextAsync(
            "Took the Falls prevention in-service off Tuesday's schedule.", new() { Timeout = 10000 });
        await Expect(FallsPrevention).ToHaveCountAsync(0, new() { Timeout = 10000 });

        // It was taken off on the server: the week comes back with Thursday's in-service and without Tuesday's.
        await Page.ReloadAsync();
        await WaitForTraceMessage("booted", 10000);
        await Expect(Schedule.Appointments("In-service: Safe resident transfers")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(FallsPrevention).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    // The schedule's own delete is cancelled: until the server has taken the in-service off, the week keeps it.
    [Test]
    public async Task a_deleted_in_service_stays_on_the_week_until_the_server_took_it_off()
    {
        await NavigateAndWaitForSchedule();
        var (removalAsked, removalReleased) = await Hold("**/Schedule/RemoveInService");
        await OpenFallsPreventionInTheEditor();
        await Schedule.EditorDelete.ClickAsync();

        await Schedule.ConfirmDelete.ClickAsync();

        await removalAsked.WaitAsync(TimeSpan.FromSeconds(10));
        await Expect(Schedule.Alert).ToBeHiddenAsync();
        await Expect(FallsPrevention).ToBeVisibleAsync();

        removalReleased.Release();

        await Expect(FallsPrevention).ToHaveCountAsync(0, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
