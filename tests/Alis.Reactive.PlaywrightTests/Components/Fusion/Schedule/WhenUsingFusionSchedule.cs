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
}
