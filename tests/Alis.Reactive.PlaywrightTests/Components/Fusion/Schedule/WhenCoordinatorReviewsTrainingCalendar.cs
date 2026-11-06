namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Schedule;

/// <summary>
/// As a training coordinator, I want each session's card to tell me whether everyone completed it, to open its
/// details with nothing of the calendar left over them, and to remind the staff who have not, and I want Agenda
/// to list the sessions of the week I was looking at, so that I never miss a session.
/// </summary>
/// <remarks>
/// The calendar opens on Thursday, November 5, 2026. Week view shows November 1 to 7: the Hand hygiene audit on
/// Tuesday (3 of 8 staff still to complete) and the Fire safety drill on Thursday (all 12 complete). Next shows
/// November 8 to 14 and keeps Thursday the 12th selected, so Agenda would start on the 12th unless the page moves it.
/// </remarks>
[TestFixture]
public class WhenCoordinatorReviewsTrainingCalendar : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Schedule/TrainingCalendar";

    private ILocator MonthStatus => Page.Locator("#training-calendar-status");
    private ILocator FireSafetyDrill => Page.Locator(".e-appointment", new() { HasText = "Fire safety drill" });
    private ILocator HandHygieneAudit => Page.Locator(".e-appointment", new() { HasText = "Hand hygiene audit" });
    private ILocator SessionCard => Page.Locator(".e-quick-popup-wrapper");
    private ILocator SessionTooltip => Page.GetByRole(AriaRole.Tooltip);
    private ILocator DrawerContent => Page.Locator("#alis-drawer-content");
    private ILocator Toast => Page.Locator("#alisFusionToast");

    private async Task NavigateAndLoadTheMonth()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(MonthStatus).ToHaveTextAsync("4 training sessions this month.");
    }

    private async Task OpenTheCard(ILocator session, string trainer)
    {
        await session.ScrollIntoViewIfNeededAsync();
        await session.ClickAsync();
        await Expect(SessionCard.GetByText(trainer, new() { Exact = true })).ToBeVisibleAsync();
    }

    private async Task OpenTheFireSafetyDrillDetails()
    {
        await OpenTheCard(FireSafetyDrill, "Dana Whitfield");
        await ClickWhenStable(SessionCard.GetByRole(AriaRole.Button, new() { Name = "Details", Exact = true }));
        await Expect(DrawerContent.GetByText("Trainer: Dana Whitfield", new() { Exact = true })).ToBeVisibleAsync();
    }

    private async Task RemindTheHandHygieneAuditStaff()
    {
        await OpenTheCard(HandHygieneAudit, "Grace Kim");
        await ClickWhenStable(SessionCard.GetByRole(AriaRole.Button, new() { Name = "Send reminder", Exact = true }));
    }

    [Test]
    public async Task a_session_everyone_completed_is_marked_all_complete_on_its_card()
    {
        await NavigateAndLoadTheMonth();

        await OpenTheCard(FireSafetyDrill, "Dana Whitfield");

        await Expect(SessionCard.GetByText("All complete", new() { Exact = true })).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_session_with_staff_still_to_complete_is_not_marked_all_complete()
    {
        await NavigateAndLoadTheMonth();

        await OpenTheCard(HandHygieneAudit, "Grace Kim");

        await Expect(SessionCard.GetByText("All complete", new() { Exact = true })).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_session_with_staff_still_to_complete_asks_for_follow_up_on_its_card()
    {
        await NavigateAndLoadTheMonth();

        await OpenTheCard(HandHygieneAudit, "Grace Kim");

        await Expect(SessionCard.GetByText("Follow-up needed", new() { Exact = true })).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_session_everyone_completed_asks_for_no_follow_up()
    {
        await NavigateAndLoadTheMonth();

        await OpenTheCard(FireSafetyDrill, "Dana Whitfield");

        await Expect(SessionCard.GetByText("Follow-up needed", new() { Exact = true })).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_sessions_details_close_its_card()
    {
        await NavigateAndLoadTheMonth();

        await OpenTheFireSafetyDrillDetails();

        await Expect(SessionCard).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hovering_a_session_shows_its_tooltip()
    {
        await NavigateAndLoadTheMonth();

        await FireSafetyDrill.ScrollIntoViewIfNeededAsync();
        await FireSafetyDrill.HoverAsync();

        await Expect(SessionTooltip.GetByText("Fire safety drill", new() { Exact = true })).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_sessions_details_leave_no_tooltip_over_the_drawer()
    {
        await NavigateAndLoadTheMonth();

        await OpenTheFireSafetyDrillDetails();

        await Expect(SessionTooltip).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_reminder_toast_is_titled_by_the_server()
    {
        await NavigateAndLoadTheMonth();

        await RemindTheHandHygieneAuditStaff();

        await Expect(Toast.Locator(".e-toast-title")).ToHaveTextAsync("Reminder sent");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_reminder_toast_says_the_servers_message()
    {
        await NavigateAndLoadTheMonth();

        await RemindTheHandHygieneAuditStaff();

        await Expect(Toast.Locator(".e-toast-content")).ToHaveTextAsync("Reminded 3 staff to complete Hand hygiene audit.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task agenda_after_next_week_starts_on_that_weeks_first_day()
    {
        await NavigateAndLoadTheMonth();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next", Exact = true }).ClickAsync();
        await Expect(Page.GetByText("November 08 - 14, 2026", new() { Exact = true })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Agenda", Exact = true }).ClickAsync();

        await Expect(Page.GetByText("Nov 08 - Dec 07, 2026", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Medication safety refresher", new() { Exact = true })).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }
}
