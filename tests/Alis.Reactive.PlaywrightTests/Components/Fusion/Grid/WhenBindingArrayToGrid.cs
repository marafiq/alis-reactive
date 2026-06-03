namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// The array DSL routed into a component data source, end-to-end in the browser. A roster loads
/// once over HTTP, then a client-side ReactiveArray transform feeds the grid via
/// SetDataSource(TypedSource&lt;T[]&gt;) — the same value-routing law that already binds When/SetText/
/// gather, now reaching a component's dataSource member. Re-filtering reads the grid's own
/// dataSource member (the read counterpart) and rebinds, with no HTTP round-trip.
///
/// Roster: Ada(active), Bo(discharged), Cy(active), Di(critical), Ed(active) — 5 rows, 3 active.
/// Page under test: /Sandbox/Components/ArrayGrid. Isolated slice.
/// </summary>
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

        // OrderBy(x => x.Name) — the client-side transform routed into the grid: Ada is first.
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

        // Where(x => x.Status == "active") over the grid's own rows: 3 remain (Ada, Cy, Ed),
        // and no discharged/critical row survives.
        await Expect(Page.Locator("#roster-grid .e-row")).ToHaveCountAsync(3, new() { Timeout = 10000 });
        await Expect(Page.Locator("#roster-grid")).Not.ToContainTextAsync("discharged", new() { Timeout = 5000 });
        await Expect(Page.Locator("#roster-grid")).Not.ToContainTextAsync("critical", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
