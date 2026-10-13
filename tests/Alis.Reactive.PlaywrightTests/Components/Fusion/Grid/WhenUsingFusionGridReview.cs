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

    // The first twelve directory residents carry four High risk levels: Katherine Ortiz, Noah Walsh,
    // Ruth Carter and Samuel Diaz (GridController.GenerateResidentDirectory).
    [Test]
    public async Task counting_high_risk_residents_counts_those_on_the_current_page()
    {
        await NavigateReview();

        await ClickWhenStable(Page.Locator("#count-high-risk"));

        await Expect(Page.Locator("#high-risk-line"))
            .ToHaveTextAsync("High-risk residents on this page: 4", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flagging_with_no_resident_selected_asks_for_a_selection()
    {
        await NavigateReview();

        await ClickWhenStable(Page.Locator("#flag-selected"));

        await Expect(Page.Locator("#flag-prompt"))
            .ToHaveTextAsync("Select residents in the roster to flag them for care review.", new() { Timeout = 10000 });
        await Expect(Page.Locator("#flag-prompt")).ToBeVisibleAsync();
        await Expect(Page.Locator("#flagged-line")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flagging_the_selected_resident_counts_them_for_care_review()
    {
        await NavigateReview();

        await Page.Locator("#care-review-grid .e-rowcell").Filter(new() { HasText = "Amina Patel" }).ClickAsync();
        await ClickWhenStable(Page.Locator("#flag-selected"));

        await Expect(Page.Locator("#flagged-line"))
            .ToHaveTextAsync("Residents flagged for care review: 1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#flag-prompt")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
