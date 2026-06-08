namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridRosterCrud : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/RosterCrud";

    private ILocator GridCell(string text) =>
        Page.Locator("#roster-crud-grid .e-gridcontent").GetByText(text, new() { Exact = true });

    private async Task NavigateCrud()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#crud-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#roster-crud-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task adding_updating_and_deleting_a_resident_row_changes_the_visible_roster()
    {
        await NavigateCrud();

        // AddRecord inserts a new resident at the top.
        await ClickWhenStable(Page.Locator("#crud-add"));
        await Expect(Page.Locator("#crud-status"))
            .ToHaveTextAsync("addRecord called", new() { Timeout = 10000 });
        await Expect(GridCell("Zara Inline")).ToBeVisibleAsync(new() { Timeout = 10000 });

        // UpdateRow replaces the top row's data.
        await ClickWhenStable(Page.Locator("#crud-update"));
        await Expect(Page.Locator("#crud-status"))
            .ToHaveTextAsync("updateRow called", new() { Timeout = 10000 });
        await Expect(GridCell("Amina Updated")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(GridCell("Zara Inline")).ToHaveCountAsync(0, new() { Timeout = 10000 });

        // DeleteSelectedRecord removes the selected top row.
        await ClickWhenStable(Page.Locator("#crud-delete"));
        await Expect(Page.Locator("#crud-status"))
            .ToHaveTextAsync("deleteRecord called", new() { Timeout = 10000 });
        await Expect(GridCell("Amina Updated")).ToHaveCountAsync(0, new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
