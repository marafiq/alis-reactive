namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionGridDirectory : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Directory";

    private async Task NavigateDirectory()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#directory-status"))
            .ToHaveTextAsync("loaded first page", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-directory-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task components_index_links_to_grid_directory_and_editing_cards()
    {
        await NavigateTo("/Sandbox/Components");

        await Expect(Page.Locator("a").Filter(new() { HasText = "Grid Directory" }))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("a").Filter(new() { HasText = "Grid Editing" }))
            .ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task public_methods_drive_server_paging_sorting_search_filtering_and_grouping()
    {
        await NavigateDirectory();

        await ClickWhenStable(Page.Locator("#grid-page-2"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("paging", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-skip")).ToHaveTextAsync("8", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-sort-risk"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("sorting", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-column")).ToHaveTextAsync("riskLevel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-direction")).ToHaveTextAsync("Descending", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-search-memory"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("searching", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-clear-search"));
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-filter-north"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("filtering", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("60 residents matched", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-clear-filters"));
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-group-care"));
        await Expect(Page.Locator("#grid-action")).ToHaveTextAsync("grouping", new() { Timeout = 10000 });
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync(
            "240 residents grouped by care level",
            new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-clear-grouping"));
        await Expect(Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task row_events_and_method_return_sources_flow_into_http_requests()
    {
        await NavigateDirectory();

        await ClickWhenStable(Page.Locator("#grid-select-second"));
        await Expect(Page.Locator("#selected-row-index")).ToHaveTextAsync("1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#selected-summary")).ToContainTextAsync("open tasks", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-gather-selection"));
        await Expect(Page.Locator("#selection-indexes")).ToHaveTextAsync("selected row indexes: 1", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-gather-selected-records"));
        await Expect(Page.Locator("#selected-records")).ToContainTextAsync("selected records:", new() { Timeout = 10000 });

        await Page.Locator("#resident-directory-grid .e-row .e-rowcell").First.ClickAsync();
        await Expect(Page.Locator("#clicked-resident")).Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });
        await Expect(Page.Locator("#clicked-cell")).Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#grid-clear-selection"));
        await ClickWhenStable(Page.Locator("#grid-gather-selection"));
        await Expect(Page.Locator("#selection-indexes")).ToHaveTextAsync("no selected row indexes", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task virtual_scroll_uses_grid_data_state_to_fetch_later_blocks()
    {
        await NavigateDirectory();

        await Expect(Page.Locator("#virtual-status"))
            .ToHaveTextAsync("loaded first virtual block", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-virtual-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        var content = Page.Locator("#resident-virtual-grid .e-content").First;
        await content.EvaluateAsync("el => { el.scrollTop = 1200; el.dispatchEvent(new Event('scroll')); }");

        await Expect(Page.Locator("#virtual-status"))
            .ToHaveTextAsync("virtual block refreshed", new() { Timeout = 10000 });
        await Expect(Page.Locator("#virtual-skip"))
            .Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
