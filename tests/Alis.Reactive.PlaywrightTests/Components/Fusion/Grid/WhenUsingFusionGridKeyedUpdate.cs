namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridKeyedUpdate : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/KeyedUpdate";

    private ILocator Cell(string text) =>
        Page.Locator("#keyed-update-grid .e-gridcontent").GetByText(text, new() { Exact = true });

    private async Task NavigateKeyed()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#keyed-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#keyed-update-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task setting_a_cell_value_and_row_data_by_primary_key_updates_the_visible_grid()
    {
        await NavigateKeyed();

        // SetCellValue (int) flags a resident's task count by primary key.
        await ClickWhenStable(Page.Locator("#keyed-set-tasks"));
        await Expect(Page.Locator("#keyed-status"))
            .ToHaveTextAsync("setCellValue int called", new() { Timeout = 10000 });
        await Expect(Cell("99").First).ToBeVisibleAsync(new() { Timeout = 10000 });

        // SetCellValue (string) quarantines a resident's risk by primary key.
        await ClickWhenStable(Page.Locator("#keyed-set-risk"));
        await Expect(Page.Locator("#keyed-status"))
            .ToHaveTextAsync("setCellValue string called", new() { Timeout = 10000 });
        await Expect(Cell("Quarantine").First).ToBeVisibleAsync(new() { Timeout = 10000 });

        // SetRowData replaces a whole resident row by primary key.
        await ClickWhenStable(Page.Locator("#keyed-set-row"));
        await Expect(Page.Locator("#keyed-status"))
            .ToHaveTextAsync("setRowData called", new() { Timeout = 10000 });
        await Expect(Cell("Keyed Row").First).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
