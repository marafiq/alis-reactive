using System.Globalization;
using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Schedule;

[TestFixture]
public class WhenUsingFusionSchedule : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Schedule";

    private static readonly Regex StaffedShift = new(@"\((CNA|RN|LPN)\)");

    // Shifts as the schedule shows them; the page's "Selected shift" line repeats a subject, so
    // these look only inside the schedule.
    private ILocator StaffedShifts => Page.Locator("#shift-schedule").GetByText(StaffedShift);
    private ILocator OpenShifts => Page.Locator("#shift-schedule").GetByText(new Regex("UNASSIGNED"));
    private ILocator Status => Page.Locator("#status");
    private ILocator ShownWeek => Page.Locator("#schedule-week");
    private ILocator SelectedShift => Page.Locator("#selected-shift");
    private ILocator LockedNote => Page.Locator("#schedule-locked-note");
    private ILocator NewAssignmentDialog => Page.Locator("#new-assignment-dialog");
    private ILocator NextWeek => Page.Locator("#shift-schedule .e-toolbar-item.e-next button");
    private ILocator OpenQuickInfo => Page.Locator(".e-quick-popup-wrapper.e-popup-open");

    private NativeCheckBoxLocator ScheduleLocked =>
        new(Page, IdGenerator.For<PointInTimeScheduleModel>(m => m.ScheduleLocked));
    private NativeCheckBoxLocator NeedsCoverageOnly =>
        new(Page, IdGenerator.For<PointInTimeScheduleModel>(m => m.NeedsCoverageOnly));
    private DropDownListLocator Facility =>
        new(Page, IdGenerator.For<PointInTimeScheduleModel>(m => m.SelectedFacilityId));

    // A work cell of the week's grid, by its start (data-date is the start's epoch milliseconds in the
    // browser's zone, the same zone the tests run in) and its shift column (0 = AM, 1 = PM, 2 = Night).
    private ILocator WorkCell(DateTime start, int shiftColumn) =>
        Page.Locator($"#shift-schedule td.e-work-cells[data-group-index='{shiftColumn}'][data-date='{new DateTimeOffset(start).ToUnixTimeMilliseconds()}']");

    // The AM shift runs 6:00 AM to 2:00 PM, so its column is open at 4:00 PM every day.
    private static DateTime OpenHourToday => DateTime.Today.AddHours(16);

    private async Task ClickOpenHourToday()
    {
        var cell = WorkCell(OpenHourToday, shiftColumn: 0);
        await cell.ScrollIntoViewIfNeededAsync();
        await cell.ClickAsync();
    }

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
        await Expect(Page.Locator("#status"))
            .Not.ToHaveTextAsync("Loading...", new() { Timeout = 10000 });
    }

    // Opens a shift the way a coordinator does: bring it into view, then click it. A bare click
    // on a shift outside the visible hours scrolls and clicks in the same frame, the scroll event
    // lands after the click, and Schedule closes QuickInfo on any content scroll, so an overnight
    // shift at the bottom of the day would never show its actions.
    private static async Task OpenShift(ILocator shift)
    {
        await shift.ScrollIntoViewIfNeededAsync();
        await shift.ClickAsync();
    }

    [Test]
    public async Task route_template_gather_reads_schedule_current_view_and_reuses_it_in_chained_request()
    {
        await NavigateAndWaitForSchedule();

        await Page.Locator("#route-template-schedule-btn").ClickAsync();

        await Expect(Page.Locator("#route-view"))
            .ToHaveTextAsync("Week", new() { Timeout = 10000 });
        await Expect(Page.Locator("#route-view-summary"))
            .ToHaveTextAsync("summary:Week", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_reads_schedule_method_return_value()
    {
        await NavigateAndWaitForSchedule();

        await Page.Locator("#audit-events-btn").ClickAsync();

        await Expect(Page.Locator("#schedule-events-count"))
            .ToHaveTextAsync(new Regex("^[1-9][0-9]*$"), new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task schedule_binds_the_weeks_shift_assignments_from_the_server()
    {
        await NavigateAndWaitForSchedule();

        // The DomReady GET returns the week's assignments; OnSuccess sets the data source
        // binds them and the Schedule renders each as an appointment labelled with
        // the staff member and role. If the data source or the load breaks, no
        // appointment renders and this fails.
        await Expect(Page.GetByText(new Regex(@"\((CNA|RN|LPN)\)")).First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        // The same success response carries UnassignedCount, read into the page
        // after databind. While the data source is unbound it shows "--"; a numeric
        // value proves the typed response body was consumed.
        await Expect(Page.Locator("#unassigned-count"))
            .ToHaveTextAsync(new Regex(@"^\d+$"), new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_an_assigned_shift_shows_staff_details_and_edit_actions()
    {
        await NavigateAndWaitForSchedule();

        // Click an assigned shift (one labelled with a staff role).
        await OpenShift(Page.GetByText(new Regex(@"\((CNA|RN|LPN)\)")).First);

        // EventClick opens the QuickInfo popup, whose custom template binds the
        // assignment: the staff phone in the content and Edit/Reassign actions in
        // the footer. None of this text exists in the DOM until the popup opens and
        // the template renders the clicked appointment's data, so each assertion
        // fails if the event wiring or the QuickInfo template breaks.
        await Expect(Page.GetByText("Reassign", new() { Exact = true }))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.GetByText(new Regex(@"\d{3}-\d{4}")).First)
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task editing_a_shift_opens_the_edit_drawer_with_the_assignment_form()
    {
        await NavigateAndWaitForSchedule();

        // Open the QuickInfo for an assigned shift, then choose Edit.
        await OpenShift(Page.GetByText(new Regex(@"\((CNA|RN|LPN)\)")).First);
        await Page.GetByText("Edit", new() { Exact = true }).ClickAsync();

        // schedule:edit loads the EditForm partial into the drawer and opens it.
        // "Save Assignment" and the "Assignment #N" label live only in that partial,
        // so they are absent until the dispatch loads the drawer content — each
        // fails if the QuickInfo action, the custom-event wiring, or the partial
        // load into the drawer breaks.
        await Expect(Page.GetByText("Save Assignment"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.GetByText(new Regex(@"Assignment #\d+")))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_staff_to_an_open_shift_reduces_open_shifts_and_persists_on_reload()
    {
        await NavigateAndWaitForSchedule();

        // The page shows how many shifts still need coverage.
        var openShifts = Page.Locator("#unassigned-count");
        await Expect(openShifts).ToHaveTextAsync(new Regex(@"^\d+$"), new() { Timeout = 10000 });
        var before = int.Parse(await openShifts.InnerTextAsync());
        Assert.That(before, Is.GreaterThan(0), "the scenario needs at least one open shift to cover");

        // Open an unassigned shift and choose to staff it.
        await OpenShift(Page.GetByText(new Regex("UNASSIGNED")).First);
        await Page.GetByText("Assign Staff", new() { Exact = true }).ClickAsync();

        // Wait for the drawer to finish sliding in and its form to render.
        await Expect(Page.Locator("#alis-drawer")).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 10000 });
        await Expect(Page.Locator("#assignment-form")).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Pick a staff member from the radio group (pre-visible options, no popup),
        // then save.
        await Page.Locator("#assignment-form").GetByText("Tom Hardy (LPN)").ClickAsync();
        await ClickWhenStable(Page.Locator("#btn-save-assignment"));

        // The save persists server-side (POST /api/schedule/assign); the schedule
        // reloads and one fewer shift needs coverage.
        await Expect(openShifts).ToHaveTextAsync((before - 1).ToString(), new() { Timeout = 15000 });

        // Reload the whole page: the assignment is still there — it persisted on the
        // server, not only in the client's component state.
        await Page.ReloadAsync();
        await WaitForTraceMessage("booted", 10000);
        await Expect(openShifts).ToHaveTextAsync((before - 1).ToString(), new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_an_unassigned_shift_offers_to_assign_staff()
    {
        await NavigateAndWaitForSchedule();

        await OpenShift(Page.GetByText(new Regex("UNASSIGNED")).First);

        // The QuickInfo template renders its unassigned branch: an Assign Staff
        // action (assigned shifts get Edit/Reassign instead). The action exists
        // only in that branch's footer, so it fails if the conditional template or
        // the event wiring breaks.
        await Expect(Page.GetByText("Assign Staff", new() { Exact = true }))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reassigning_a_shift_opens_the_assignment_drawer()
    {
        await NavigateAndWaitForSchedule();

        await OpenShift(Page.GetByText(new Regex(@"\((CNA|RN|LPN)\)")).First);
        await Page.GetByText("Reassign", new() { Exact = true }).ClickAsync();

        // schedule:reassign is a distinct custom-event handler from schedule:edit;
        // it loads the assignment form into the drawer. "Save Assignment" and the
        // "Assignment #N" label exist only in that partial, so this fails if the
        // reassign action or its event wiring breaks.
        await Expect(Page.GetByText("Save Assignment")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.GetByText(new Regex(@"Assignment #\d+"))).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_an_open_hour_offers_a_new_assignment_for_that_shift_and_time()
    {
        await NavigateAndWaitForSchedule();

        await ClickOpenHourToday();

        // The form loads with the clicked cell's shift and start, read from the cellClick payload.
        await Expect(NewAssignmentDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(NewAssignmentDialog).ToContainTextAsync("Shift: AM Shift (6:00 AM - 2:00 PM)", new() { Timeout = 10000 });
        await Expect(NewAssignmentDialog).ToContainTextAsync(
            "Start: " + OpenHourToday.ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture));

        // The empty cell's own QuickInfo is cancelled in popupOpen, so only the dialog shows.
        await Expect(OpenQuickInfo).ToHaveCountAsync(0);
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

    [Test]
    public async Task moving_to_next_week_loads_that_weeks_shifts_and_names_the_week()
    {
        await NavigateAndWaitForSchedule();
        await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(ShownWeek).ToHaveTextAsync(new Regex(@"^\d{4}-\d{2}-\d{2}"));
        var shownWeek = DateTime.Parse(await ShownWeek.TextContentAsync() ?? "", CultureInfo.InvariantCulture);

        await NextWeek.ClickAsync();

        // The week's grid starts empty: only the navigating handler's request fills it.
        await Expect(StaffedShifts.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(ShownWeek).ToHaveTextAsync(
            new Regex("^" + Regex.Escape(shownWeek.AddDays(7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))),
            new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_schedule_locked_for_payroll_stays_on_its_week()
    {
        await NavigateAndWaitForSchedule();
        await Expect(ShownWeek).ToHaveTextAsync(new Regex(@"^\d{4}-\d{2}-\d{2}"));
        var shownWeek = await ShownWeek.TextContentAsync() ?? "";
        await ScheduleLocked.Check();

        await NextWeek.ClickAsync();

        await Expect(LockedNote).ToHaveTextAsync("The schedule is locked for payroll; it stays on this week.", new() { Timeout = 5000 });
        await Expect(ShownWeek).ToHaveTextAsync(shownWeek);
        await Expect(StaffedShifts.First).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_schedule_locked_for_payroll_does_not_open_a_shift()
    {
        await NavigateAndWaitForSchedule();
        await ScheduleLocked.Check();

        await OpenShift(StaffedShifts.First);

        await Expect(LockedNote).ToHaveTextAsync("The schedule is locked for payroll; unlock it to open a shift.", new() { Timeout = 5000 });
        await Expect(Page.GetByText("Reassign", new() { Exact = true })).ToHaveCountAsync(0);

        await ScheduleLocked.Input.UncheckAsync();
        await OpenShift(StaffedShifts.First);

        await Expect(Page.GetByText("Reassign", new() { Exact = true })).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_a_shifts_details_clears_the_selected_shift()
    {
        await NavigateAndWaitForSchedule();

        await OpenShift(StaffedShifts.First);
        await Expect(SelectedShift).ToHaveTextAsync(StaffedShift, new() { Timeout = 10000 });

        await Page.Locator(".e-quick-popup-wrapper .e-close").ClickAsync();

        await Expect(SelectedShift).ToHaveTextAsync("No shift selected.", new() { Timeout = 5000 });
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
    public async Task changing_the_facility_reloads_the_schedule_until_it_is_ready()
    {
        await NavigateAndWaitForSchedule();

        await Facility.Select("Sunrise Gardens");

        // The change handler says "Loading..." and its response "Facility changed"; only the
        // schedule's dataBound, after the new shifts bind, says "Ready".
        await Expect(Status).ToHaveTextAsync("Ready", new() { Timeout = 10000 });
        await Expect(StaffedShifts.First).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task jumping_to_a_shift_brings_its_first_hour_into_view()
    {
        await NavigateAndWaitForSchedule();
        var amShiftStart = WorkCell(DateTime.Today.AddHours(6), shiftColumn: 0);
        var nightShiftStart = WorkCell(DateTime.Today.AddHours(22), shiftColumn: 2);

        await Page.Locator("#schedule-show-morning").ClickAsync();

        await Expect(amShiftStart).ToBeInViewportAsync(new() { Timeout = 5000 });
        await Expect(nightShiftStart).Not.ToBeInViewportAsync();

        await Page.Locator("#schedule-show-night").ClickAsync();

        await Expect(nightShiftStart).ToBeInViewportAsync(new() { Timeout = 5000 });
        await Expect(amShiftStart).Not.ToBeInViewportAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task double_clicking_a_shift_opens_the_assignment_drawer_instead_of_the_built_in_editor()
    {
        await NavigateAndWaitForSchedule();
        var shift = StaffedShifts.First;
        await shift.ScrollIntoViewIfNeededAsync();

        await shift.DblClickAsync();

        // popupOpen cancels Syncfusion's editor (type "Editor") and loads the assignment form instead.
        await Expect(Page.GetByText("Save Assignment")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator(".e-schedule-dialog.e-popup-open")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }
}
