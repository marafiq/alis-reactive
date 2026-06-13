using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Schedule;

[TestFixture]
public class WhenUsingFusionSchedule : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Schedule";

    private async Task NavigateAndWaitForSchedule()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#status"))
            .Not.ToHaveTextAsync("Loading...", new() { Timeout = 10000 });
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

        // The DomReady GET returns the week's assignments; OnSuccess SetDataSource
        // binds them and the Schedule renders each as an appointment labelled with
        // the staff member and role. If SetDataSource or the load breaks, no
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
        await Page.GetByText(new Regex(@"\((CNA|RN|LPN)\)")).First.ClickAsync();

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
        await Page.GetByText(new Regex(@"\((CNA|RN|LPN)\)")).First.ClickAsync();
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
        await Page.GetByText(new Regex("UNASSIGNED")).First.ClickAsync();
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
}
