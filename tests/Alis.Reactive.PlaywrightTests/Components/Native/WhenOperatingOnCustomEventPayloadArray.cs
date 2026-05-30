namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// The array DSL operating on a CUSTOM event payload, end-to-end in the browser. A button loads the
/// alert roster, then dispatches the custom event <c>shift-report</c> whose payload carries
/// ResidentAlert[]; the listener runs filter/aggregate/find/guard over the payload's element members
/// (p.From(payload, x =&gt; x.Alerts)). Proves a developer-defined event payload array is fully operable.
///
/// Alerts: Maple(critical,9,unack), Birch(stable,2), Cedar(critical,7), Aspen(urgent,5), Oak(critical,4,unack).
/// Page under test: /Sandbox/Components/ShiftReport. Isolated slice.
/// </summary>
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
        // Where(Severity == "critical").Sum(Priority) = 9 + 7 + 4
        await Expect(Page.Locator("#critical-priority-sum")).ToHaveTextAsync("20", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finds_the_highest_priority_critical_resident()
    {
        await NavigateAndGenerateReport();
        // Where(critical).OrderByDescending(Priority).Find(first).Resident => Maple (priority 9)
        await Expect(Page.Locator("#top-critical")).ToHaveTextAsync("Maple", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task guards_on_an_unacknowledged_critical_alert()
    {
        await NavigateAndGenerateReport();
        // Any(Severity == "critical" && !Acknowledged) => true (Maple, Oak)
        await Expect(Page.Locator("#unack-warning")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
