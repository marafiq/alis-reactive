namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Schedule;

/// <summary>
/// As a training coordinator, I want a session's Details to open in the drawer with nothing of the calendar
/// left over it, and Agenda to list the sessions of the week I was looking at, so that I never miss a session.
/// </summary>
/// <remarks>
/// The calendar opens on Thursday, November 5, 2026. Week view shows November 1 to 7; Next shows November 8 to 14
/// and keeps Thursday the 12th selected, so Agenda would start on the 12th unless the page moves it.
/// </remarks>
[TestFixture]
public class WhenCoordinatorReviewsTrainingCalendar : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Schedule/TrainingCalendar";

    private ILocator MonthStatus => Page.Locator("#training-calendar-status");
    private ILocator FireSafetyDrill => Page.Locator(".e-appointment", new() { HasText = "Fire safety drill" });
    private ILocator SessionCard => Page.Locator(".e-quick-popup-wrapper");
    private ILocator SessionTooltip => Page.GetByRole(AriaRole.Tooltip);
    private ILocator DrawerContent => Page.Locator("#alis-drawer-content");

    private async Task NavigateAndLoadTheMonth()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(MonthStatus).ToHaveTextAsync("3 training sessions this month.");
    }

    private async Task OpenTheFireSafetyDrillDetails()
    {
        await FireSafetyDrill.ScrollIntoViewIfNeededAsync();
        await FireSafetyDrill.ClickAsync();
        await Expect(SessionCard.GetByText("Dana Whitfield", new() { Exact = true })).ToBeVisibleAsync();
        await ClickWhenStable(SessionCard.GetByRole(AriaRole.Button, new() { Name = "Details", Exact = true }));
        await Expect(DrawerContent.GetByText("Trainer: Dana Whitfield", new() { Exact = true })).ToBeVisibleAsync();
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
