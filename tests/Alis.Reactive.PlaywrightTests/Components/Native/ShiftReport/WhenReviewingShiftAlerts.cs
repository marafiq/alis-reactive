namespace Alis.Reactive.PlaywrightTests.Components.Native.ShiftReport;

/// <summary>
/// As the charge nurse on Maple wing, I want a shift's alerts summed up and what is still open called out
/// (escalation, a float nurse, the physician, follow-ups) before I sign the shift report, so that no open
/// critical alert is handed over unseen.
/// </summary>
/// <remarks>
/// Opening a shift loads its alerts from the alert log and announces them to the review, which reads them by
/// each alert's members. The evening shift has five alerts: Evelyn Hart's open urgent alert (priority 5)
/// first, Walter Brooks's acknowledged critical one (7), Margaret Hale's open critical one (9), Rose Delgado's
/// stable one (2) and Robert Miles's open critical one (4). The day shift has four, every one acknowledged:
/// Agnes Moore's critical alert (6) first, Harold Kim's urgent one (3), Frank Ortiz's critical one (8) and
/// Ida Bell's stable one (1). Each call-out is shown by one shift and hidden by the other, so a call-out that
/// always or never shows fails one of them. Every array call the review makes is exercised.
/// </remarks>
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

    // 7 + 9 + 4 for the critical alerts; every alert's priority would make 27.
    [Test]
    public async Task the_critical_alerts_priorities_are_added_up()
    {
        await OpenTheReview();

        await OpenTheEveningShift();

        await Expect(CriticalPriority).ToHaveTextAsync("Critical alert priority: 20 points.");
        AssertNoConsoleErrors();
    }

    // Walter Brooks's critical alert comes first in the log and Robert Miles's has the lowest priority: an
    // unordered review would name Walter, an ascending one Robert.
    [Test]
    public async Task the_most_urgent_critical_alert_is_the_critical_one_with_the_highest_priority()
    {
        await OpenTheReview();

        await OpenTheEveningShift();

        await Expect(MostUrgentCritical).ToHaveTextAsync("Most urgent critical alert: Margaret Hale.");
        AssertNoConsoleErrors();
    }

    // Each opened shift runs the review again with its own alerts.
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

    // The day shift's critical alerts are all acknowledged: a check that ignored acknowledgement would
    // still call one open there.
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

    // Margaret Hale's and Robert Miles's: exactly two open critical alerts, which a "more than two" would miss.
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

    // The evening's open priorities add up to 5 + 9 + 4 = 18. The day shift's are all acknowledged; adding
    // every one of its priorities (18) would call a float nurse there too.
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

    // The evening's most urgent open alert is Margaret Hale's critical one; taking the first open alert in
    // the log instead (Evelyn Hart's urgent one) would not page. The day shift has no open alert, and its
    // most urgent alert, ignoring acknowledgement, is Frank Ortiz's critical one.
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

    // Evelyn Hart's urgent alert is open on the evening shift; Harold Kim's on the day shift is acknowledged.
    // Asking whether the shift has any alert at all would call for a follow-up on both.
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

    // Walter Brooks's and Rose Delgado's evening alerts are acknowledged: asking whether any alert is
    // acknowledged, rather than every one, would call the evening ready too.
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
