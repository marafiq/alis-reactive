namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises array DSL operations over a developer-defined custom event payload.
/// </summary>
/// <remarks>
/// The <c>shift-report</c> payload carries <c>ResidentAlert[]</c>, and
/// <c>p.From(payload, x =&gt; x.Alerts)</c> drives filter, aggregate, find, and guard behavior.
/// </remarks>
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
