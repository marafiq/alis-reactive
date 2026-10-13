namespace Alis.Reactive.PlaywrightTests.Components.Native;

// Shift-report payload carries ResidentAlert[]; p.From(payload.Get(x => x.Alerts))
// drives filter, aggregate, find, and guard behavior.
[TestFixture]
public class WhenOperatingOnCustomEventPayloadArray : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ShiftReport";

    private async Task NavigateAndGenerateReport()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Page.Locator("#load-report-btn").ClickAsync();
    }

    [Test]
    public async Task counts_every_alert_in_the_custom_payload_array()
    {
        await NavigateAndGenerateReport();
        await Expect(Page.Locator("#alert-total")).ToHaveTextAsync("5", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task counts_only_critical_alerts_by_element_member()
    {
        await NavigateAndGenerateReport();
        await Expect(Page.Locator("#critical-count")).ToHaveTextAsync("3", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sums_priority_of_critical_alerts()
    {
        await NavigateAndGenerateReport();
        await Expect(Page.Locator("#critical-priority-sum")).ToHaveTextAsync("20", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finds_the_highest_priority_critical_resident()
    {
        await NavigateAndGenerateReport();
        await Expect(Page.Locator("#top-critical")).ToHaveTextAsync("Maple", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task guards_on_an_unacknowledged_critical_alert()
    {
        await NavigateAndGenerateReport();
        await Expect(Page.Locator("#unack-warning")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The shift report has two unacknowledged critical alerts (Maple, Oak), open priorities 9 + 5 + 4 = 18,
    // and Maple (critical, priority 9) as the most urgent open alert; the clear report has none open.
    // Each line is a When over an array result: Count(..) Gte 2, Sum(..) Gt 10, Find(.., ..) Eq "critical".
    // A Count read as Gt (2 is not over 2), an unread Sum, or a Find that ignores its predicate (Maple is
    // still first in the clear report) each fail one of the two reports.
    [Test]
    public async Task an_open_critical_load_escalates_to_the_charge_nurse_float_nurse_and_physician_until_cleared()
    {
        await NavigateAndGenerateReport();

        await Expect(Page.Locator("#charge-nurse-escalation")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#float-nurse")).ToBeVisibleAsync();
        await Expect(Page.Locator("#page-physician")).ToBeVisibleAsync();

        await Page.Locator("#clear-report-btn").ClickAsync();

        await Expect(Page.Locator("#all-acknowledged")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#charge-nurse-escalation")).ToBeHiddenAsync();
        await Expect(Page.Locator("#float-nurse")).ToBeHiddenAsync();
        await Expect(Page.Locator("#page-physician")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // Any() with no predicate over the open urgent alerts: Aspen in the shift report, none in the clear one.
    // An Any() that answered for the array itself, empty or not, would keep the follow-up on the clear report.
    [Test]
    public async Task an_open_urgent_alert_needs_a_follow_up_and_none_is_asked_once_all_are_acknowledged()
    {
        await NavigateAndGenerateReport();
        await Expect(Page.Locator("#urgent-followup")).ToBeVisibleAsync(new() { Timeout = 10000 });

        await Page.Locator("#clear-report-btn").ClickAsync();

        await Expect(Page.Locator("#all-acknowledged")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#urgent-followup")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // All(a => a.Acknowledged): every alert in the clear report, not every alert in the shift report. An
    // All that answered like Any would also close the review for the shift report, where Birch and Cedar
    // are acknowledged.
    [Test]
    public async Task the_alert_review_closes_only_when_every_alert_is_acknowledged()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Page.Locator("#clear-report-btn").ClickAsync();
        await Expect(Page.Locator("#all-acknowledged")).ToBeVisibleAsync(new() { Timeout = 10000 });

        await Page.Locator("#load-report-btn").ClickAsync();

        await Expect(Page.Locator("#urgent-followup")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#all-acknowledged")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task guard_clears_when_all_critical_alerts_are_acknowledged()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Page.Locator("#clear-report-btn").ClickAsync();

        await Expect(Page.Locator("#unack-clear")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#unack-warning")).Not.ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
