namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridReview : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/CareReview";

    private async Task NavigateReview()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#review-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-review-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task reviewing_current_view_row_index_and_selected_residents_gathers_typed_sources()
    {
        await NavigateReview();

        // CurrentViewRecords source gathered to the server.
        await ClickWhenStable(Page.Locator("#review-current-view"));
        await Expect(Page.Locator("#view-summary"))
            .ToContainTextAsync("current view has 12 residents", new() { Timeout = 10000 });

        // RowIndexByPrimaryKey source gathered to the server.
        await ClickWhenStable(Page.Locator("#review-row-index"));
        await Expect(Page.Locator("#index-summary"))
            .ToContainTextAsync("row index 5", new() { Timeout = 10000 });

        // SelectedRecords source gathered to the server after selecting a range.
        await ClickWhenStable(Page.Locator("#review-selected"));
        await Expect(Page.Locator("#selected-summary"))
            .ToContainTextAsync("selected records:", new() { Timeout = 10000 });

        // SelectedRowIndexes source gathered to the server.
        await ClickWhenStable(Page.Locator("#review-indexes"));
        await Expect(Page.Locator("#indexes-summary"))
            .ToContainTextAsync("selected row indexes: 0, 1", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
