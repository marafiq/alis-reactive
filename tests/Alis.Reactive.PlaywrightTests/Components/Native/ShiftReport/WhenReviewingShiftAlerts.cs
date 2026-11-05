namespace Alis.Reactive.PlaywrightTests.Components.Native.ShiftReport;

[TestFixture]
public class WhenReviewingShiftAlerts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ShiftReport";

    private ILocator DayShift => Page.Locator("#day-shift-btn");
    private ILocator EveningShift => Page.Locator("#evening-shift-btn");
    private ILocator ChooseAShift => Page.Locator("#shift-choose");
    private ILocator Review => Page.Locator("#shift-review");
    private ILocator ShiftName => Page.Locator("#shift-name");
    private ILocator AlertSummary => Page.Locator("#alert-summary");
    private ILocator CriticalPriority => Page.Locator("#critical-priority");
    private ILocator MostUrgentCritical => Page.Locator("#most-urgent-critical");
    private ILocator CriticalStillOpen => Page.Locator("#unack-warning");
    private ILocator CriticalAllAcknowledged => Page.Locator("#unack-clear");
    private ILocator EscalateToChargeNurse => Page.Locator("#charge-nurse-escalation");
    private ILocator CallAFloatNurse => Page.Locator("#float-nurse");
    private ILocator PageThePhysician => Page.Locator("#page-physician");
    private ILocator FollowUpUrgent => Page.Locator("#urgent-followup");
    private ILocator ReadyToSign => Page.Locator("#all-acknowledged");

    private async Task OpenTheReview() => await NavigateToAndWaitForBoot(Path);

    private async Task Open(ILocator shift, string name)
    {
        await ClickWhenStable(shift);
        await Expect(ShiftName).ToHaveTextAsync(name, new() { Timeout = 10000 });
    }

    private Task OpenTheEveningShift() => Open(EveningShift, "Evening shift");

    private Task OpenTheDayShift() => Open(DayShift, "Day shift");

    [Test]
    public async Task the_review_waits_for_a_shift_to_be_chosen()
    {
        await OpenTheReview();

        await Expect(ChooseAShift).ToBeVisibleAsync();
        await Expect(Review).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_a_shift_counts_its_alerts_and_how_many_are_critical()
    {
        await OpenTheReview();

        await OpenTheEveningShift();

        await Expect(Review).ToBeVisibleAsync();
        await Expect(ChooseAShift).ToBeHiddenAsync();
        await Expect(AlertSummary).ToHaveTextAsync("5 alerts this shift, 3 of them critical.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_critical_alerts_priorities_are_added_up()
    {
        await OpenTheReview();

        await OpenTheEveningShift();

        await Expect(CriticalPriority).ToHaveTextAsync("Critical alert priority: 20 points.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_most_urgent_critical_alert_is_the_critical_one_with_the_highest_priority()
    {
        await OpenTheReview();

        await OpenTheEveningShift();

        await Expect(MostUrgentCritical).ToHaveTextAsync("Most urgent critical alert: Margaret Hale.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_the_day_shift_after_the_evening_shift_shows_the_day_shifts_figures()
    {
        await OpenTheReview();
        await OpenTheEveningShift();

        await OpenTheDayShift();

        await Expect(AlertSummary).ToHaveTextAsync("4 alerts this shift, 2 of them critical.");
        await Expect(CriticalPriority).ToHaveTextAsync("Critical alert priority: 14 points.");
        await Expect(MostUrgentCritical).ToHaveTextAsync("Most urgent critical alert: Frank Ortiz.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_open_critical_alert_is_called_out_until_every_critical_alert_is_acknowledged()
    {
        await OpenTheReview();
        await OpenTheEveningShift();
        await Expect(CriticalStillOpen).ToBeVisibleAsync();
        await Expect(CriticalAllAcknowledged).ToBeHiddenAsync();

        await OpenTheDayShift();

        await Expect(CriticalAllAcknowledged).ToBeVisibleAsync();
        await Expect(CriticalStillOpen).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task two_open_critical_alerts_are_escalated_to_the_charge_nurse()
    {
        await OpenTheReview();
        await OpenTheEveningShift();
        await Expect(EscalateToChargeNurse).ToBeVisibleAsync();

        await OpenTheDayShift();

        await Expect(EscalateToChargeNurse).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_open_alert_load_over_ten_points_calls_in_a_float_nurse()
    {
        await OpenTheReview();
        await OpenTheEveningShift();
        await Expect(CallAFloatNurse).ToBeVisibleAsync();

        await OpenTheDayShift();

        await Expect(CallAFloatNurse).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_physician_is_paged_when_the_most_urgent_open_alert_is_critical()
    {
        await OpenTheReview();
        await OpenTheEveningShift();
        await Expect(PageThePhysician).ToBeVisibleAsync();

        await OpenTheDayShift();

        await Expect(PageThePhysician).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_open_urgent_alert_asks_for_a_follow_up_this_shift()
    {
        await OpenTheReview();
        await OpenTheEveningShift();
        await Expect(FollowUpUrgent).ToBeVisibleAsync();

        await OpenTheDayShift();

        await Expect(FollowUpUrgent).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_shift_report_is_ready_to_sign_only_when_every_alert_is_acknowledged()
    {
        await OpenTheReview();
        await OpenTheDayShift();
        await Expect(ReadyToSign).ToBeVisibleAsync();

        await OpenTheEveningShift();

        await Expect(ReadyToSign).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
