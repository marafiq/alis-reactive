namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises FusionGrid server-side custom binding end-to-end:
/// initial data load, sorting, paging, and external filtering.
///
/// Page under test: /Sandbox/Components/Grid
/// Server: 200 resident records, 10 per page.
/// </summary>
[TestFixture]
public class WhenUsingFusionGrid : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid";

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#load-status");
    }

    // ── Page loads with initial data ──

    [Test]
    public async Task page_loads_with_initial_data()
    {
        await NavigateAndBoot();
        var status = await Page.Locator("#load-status").TextContentAsync();
        Assert.That(status, Does.Contain("initial data loaded"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task grid_renders_rows()
    {
        await NavigateAndBoot();
        var rows = Page.Locator("#residents-grid .e-row");
        var count = await rows.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1), "Grid should display at least one row");
        AssertNoConsoleErrors();
    }

    // ── Server-side sorting ──

    [Test]
    public async Task clicking_column_header_sorts_data()
    {
        await NavigateAndBoot();

        // Click the Age column header to trigger sorting
        var ageHeader = Page.Locator("#residents-grid .e-headercell").Filter(new() { HasText = "Age" });
        await ClickWhenStable(ageHeader);

        // Wait for the action status to update (server round-trip)
        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        var actionType = await Page.Locator("#evt-action-type").TextContentAsync();
        Assert.That(actionType, Is.EqualTo("sorting"));
        AssertNoConsoleErrors();
    }

    // ── Server-side paging ──

    [Test]
    public async Task clicking_next_page_fetches_next_page()
    {
        await NavigateAndBoot();

        // Click the next-page button in the grid pager
        var nextPage = Page.Locator("#residents-grid .e-pagercontainer .e-nextpage");
        await ClickWhenStable(nextPage);

        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        var skip = await Page.Locator("#evt-skip").TextContentAsync();
        Assert.That(skip, Is.EqualTo("10"), "Second page should skip 10 records");
        AssertNoConsoleErrors();
    }

    // ── External filter ──

    [Test]
    public async Task external_filter_reloads_grid_data()
    {
        await NavigateAndBoot();

        // Type a high min age to filter
        var filterInput = Page.Locator("#residents-grid").Locator("..").Locator("..").Locator("..").Locator("input[aria-label='Min Age Filter']");

        // Use the SF NumericTextBox approach — find the input inside the component wrapper
        var numericInput = Page.Locator("input[id$='__MinAge']");
        await Expect(numericInput).ToBeVisibleAsync(new() { Timeout = 5000 });
        await numericInput.ClickAsync();
        await numericInput.FillAsync("90");
        // Tab out to trigger the Changed event
        await numericInput.PressAsync("Tab");

        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("filtered", new() { Timeout = 10000 });

        // After filtering, the grid should have fewer rows (residents age >= 90)
        AssertNoConsoleErrors();
    }

    // ── Plan JSON rendered ──

    [Test]
    public async Task plan_json_contains_grid_behaviors()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions for SetDataSource");
        Assert.That(planJson, Does.Contain("\"dataSource\""),
            "Plan must target the dataSource property");
        Assert.That(planJson, Does.Contain("residents-grid"),
            "Plan must reference the grid element ID");
        AssertNoConsoleErrors();
    }
}
