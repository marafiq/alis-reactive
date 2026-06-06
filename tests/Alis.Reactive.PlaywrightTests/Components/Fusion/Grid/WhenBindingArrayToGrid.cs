namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

// Roster loads once over HTTP, then client-side transforms feed SetDataSource.
// Re-filtering reads the grid's own dataSource member with no HTTP round trip.
[TestFixture]
public class WhenBindingArrayToGrid : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ArrayGrid";

    private async Task NavigateAndWaitForRoster()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#grid-status"))
            .ToHaveTextAsync("roster loaded", new() { Timeout = 10000 });
    }

    [Test]
    public async Task roster_loads_into_grid_sorted_by_name()
    {
        await NavigateAndWaitForRoster();

        var rows = Page.Locator("#roster-grid .e-row");
        await Expect(rows).ToHaveCountAsync(5, new() { Timeout = 10000 });

        await Expect(rows.First).ToContainTextAsync("Ada", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showing_active_only_filters_the_grid_client_side()
    {
        await NavigateAndWaitForRoster();
        await Expect(Page.Locator("#roster-grid .e-row")).ToHaveCountAsync(5, new() { Timeout = 10000 });

        await Page.Locator("#show-active-btn").ClickAsync();

        await Expect(Page.Locator("#grid-status"))
            .ToHaveTextAsync("active only", new() { Timeout = 5000 });

        await Expect(Page.Locator("#roster-grid .e-row")).ToHaveCountAsync(3, new() { Timeout = 10000 });
        await Expect(Page.Locator("#roster-grid")).Not.ToContainTextAsync("discharged", new() { Timeout = 5000 });
        await Expect(Page.Locator("#roster-grid")).Not.ToContainTextAsync("critical", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
