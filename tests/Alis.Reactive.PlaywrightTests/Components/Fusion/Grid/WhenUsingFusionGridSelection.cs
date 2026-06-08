namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridSelection : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/RosterSelection";

    private ILocator SelectedRows =>
        Page.Locator("#roster-selection-grid .e-row[aria-selected='true']");

    private async Task NavigateSelection()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#selection-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#roster-selection-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task selecting_a_range_clearing_and_selecting_a_single_row_updates_the_selection()
    {
        await NavigateSelection();

        // SelectRowsByRange selects three rows.
        await ClickWhenStable(Page.Locator("#select-range"));
        await Expect(Page.Locator("#selection-status"))
            .ToHaveTextAsync("selectRowsByRange called", new() { Timeout = 10000 });
        await Expect(SelectedRows).ToHaveCountAsync(3, new() { Timeout = 10000 });

        // ClearSelection removes all selection.
        await ClickWhenStable(Page.Locator("#clear-selection"));
        await Expect(Page.Locator("#selection-status"))
            .ToHaveTextAsync("clearSelection called", new() { Timeout = 10000 });
        await Expect(SelectedRows).ToHaveCountAsync(0, new() { Timeout = 10000 });

        // SelectRow selects a single row.
        await ClickWhenStable(Page.Locator("#select-first"));
        await Expect(Page.Locator("#selection-status"))
            .ToHaveTextAsync("selectRow called", new() { Timeout = 10000 });
        await Expect(SelectedRows).ToHaveCountAsync(1, new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
