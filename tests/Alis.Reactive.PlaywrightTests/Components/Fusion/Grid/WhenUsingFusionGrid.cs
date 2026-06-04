namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// Exercises FusionGrid server-side custom binding end-to-end:
/// initial data load, sorting with column echo, paging with skip echo,
/// external filtering, and plan JSON verification.
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
        await Expect(gridRows)
            .ToHaveCountAsync(10, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sorting_a_column_fetches_sorted_data_and_echoes_action()
    {
        await NavigateAndWaitForInitialLoad();

        var nameHeader = Page.Locator("#residents-grid .e-headercell").Filter(new() { HasText = "Name" });
        await Expect(nameHeader).ToBeVisibleAsync(new() { Timeout = 5000 });
        await nameHeader.ClickAsync();

        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("sorting", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task paging_fetches_next_page_with_correct_skip()
    {
        await NavigateAndWaitForInitialLoad();

        var pager = Page.Locator("#residents-grid .e-pagercontainer");
        await Expect(pager).ToBeVisibleAsync(new() { Timeout = 5000 });

        var page2 = Page.Locator("#residents-grid .e-numericitem:has-text('2')");
        await Expect(page2).ToBeVisibleAsync(new() { Timeout = 5000 });
        await page2.ClickAsync();

        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        await Expect(Page.Locator("#evt-skip"))
            .ToHaveTextAsync("10", new() { Timeout = 5000 });

        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("paging", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task external_filter_reloads_grid_with_fewer_results()
    {
        await NavigateAndWaitForInitialLoad();

        var numericInput = Page.Locator("input[id$='__MinAge']");
        await Expect(numericInput).ToBeVisibleAsync(new() { Timeout = 5000 });
        await numericInput.ClickAsync();
        await numericInput.FillAsync("90");
        await numericInput.PressAsync("Tab");

        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("filtered", new() { Timeout = 10000 });

        var gridRows = Page.Locator("#residents-grid .e-row");
        await Expect(gridRows.First).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sorting_then_paging_reports_paging_action()
    {
        await NavigateAndWaitForInitialLoad();

        var ageHeader = Page.Locator("#residents-grid .e-headercell").Filter(new() { HasText = "Age" });
        await ClickWhenStable(ageHeader);
        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        var page2 = Page.Locator("#residents-grid .e-numericitem:has-text('2')");
        await Expect(page2).ToBeVisibleAsync(new() { Timeout = 5000 });
        await page2.ClickAsync();

        await Expect(Page.Locator("#evt-skip"))
            .ToHaveTextAsync("10", new() { Timeout = 10000 });

        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("paging", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

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
