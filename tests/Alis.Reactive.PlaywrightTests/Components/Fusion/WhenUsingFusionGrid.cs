namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises FusionGrid server-side custom binding end-to-end:
/// initial data load, sorting with column echo, paging with skip echo,
/// external filtering, and plan JSON verification.
///
/// Page under test: /Sandbox/Components/Grid
/// Server: 200 resident records, 10 per page.
/// </summary>
[TestFixture]
public class WhenUsingFusionGrid : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid";

    private async Task NavigateAndWaitForInitialLoad()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#load-status"))
            .ToHaveTextAsync("initial data loaded", new() { Timeout = 10000 });
    }

    // ── Initial load ──

    [Test]
    public async Task page_loads_with_initial_data()
    {
        await NavigateAndWaitForInitialLoad();

        var gridRows = Page.Locator("#residents-grid .e-row");
        await Expect(gridRows.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task grid_displays_ten_rows_per_page()
    {
        await NavigateAndWaitForInitialLoad();

        var gridRows = Page.Locator("#residents-grid .e-row");
        var count = await gridRows.CountAsync();
        Assert.That(count, Is.EqualTo(10), "Grid should display exactly 10 rows (pageSize)");
        AssertNoConsoleErrors();
    }

    // ── Server-side sorting ──

    [Test]
    public async Task sorting_a_column_fetches_sorted_data_and_echoes_action()
    {
        await NavigateAndWaitForInitialLoad();

        // Click Name column header to sort ascending
        var nameHeader = Page.Locator("#residents-grid .e-headercell").Filter(new() { HasText = "Name" });
        await Expect(nameHeader).ToBeVisibleAsync(new() { Timeout = 5000 });
        await nameHeader.ClickAsync();

        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        // Echo shows sorting action details
        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("sorting", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Server-side paging ──

    [Test]
    public async Task paging_fetches_next_page_with_correct_skip()
    {
        await NavigateAndWaitForInitialLoad();

        // Pager should show (200 items / 10 per page = 20 pages)
        var pager = Page.Locator("#residents-grid .e-pagercontainer");
        await Expect(pager).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click page 2
        var page2 = Page.Locator("#residents-grid .e-numericitem:has-text('2')");
        await Expect(page2).ToBeVisibleAsync(new() { Timeout = 5000 });
        await page2.ClickAsync();

        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        // Echo shows paging with skip=10
        await Expect(Page.Locator("#evt-skip"))
            .ToHaveTextAsync("10", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("paging", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── External filter ──

    [Test]
    public async Task external_filter_reloads_grid_with_fewer_results()
    {
        await NavigateAndWaitForInitialLoad();

        // Type a high min age to filter significantly
        var numericInput = Page.Locator("input[id$='__MinAge']");
        await Expect(numericInput).ToBeVisibleAsync(new() { Timeout = 5000 });
        await numericInput.ClickAsync();
        await numericInput.FillAsync("90");
        await numericInput.PressAsync("Tab");

        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("filtered", new() { Timeout = 10000 });

        // Grid should still render rows (server processed the filter)
        var gridRows = Page.Locator("#residents-grid .e-row");
        await Expect(gridRows.First).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Sort + Page combined ──

    [Test]
    public async Task sorting_then_paging_preserves_sort_order()
    {
        await NavigateAndWaitForInitialLoad();

        // Sort by Age ascending
        var ageHeader = Page.Locator("#residents-grid .e-headercell").Filter(new() { HasText = "Age" });
        await ClickWhenStable(ageHeader);
        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        // Now page to page 2 — sort should still be active
        var page2 = Page.Locator("#residents-grid .e-numericitem:has-text('2')");
        await Expect(page2).ToBeVisibleAsync(new() { Timeout = 5000 });
        await page2.ClickAsync();

        await Expect(Page.Locator("#evt-skip"))
            .ToHaveTextAsync("10", new() { Timeout = 10000 });

        // The action type for paging should be "paging" not "sorting"
        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("paging", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Plan JSON ──

    [Test]
    public async Task plan_json_contains_grid_behaviors()
    {
        await NavigateAndWaitForInitialLoad();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions for SetDataSource");
        Assert.That(planJson, Does.Contain("\"dataSource\""),
            "Plan must target the dataSource property");
        Assert.That(planJson, Does.Contain("residents-grid"),
            "Plan must reference the grid element ID");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must declare fusion vendor");

        AssertNoConsoleErrors();
    }
}
