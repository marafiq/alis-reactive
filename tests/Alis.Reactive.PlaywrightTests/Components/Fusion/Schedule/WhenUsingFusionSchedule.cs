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
}
