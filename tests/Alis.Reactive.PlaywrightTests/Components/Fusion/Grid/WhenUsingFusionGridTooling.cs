namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridTooling : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/GridTooling";

    private async Task NavigateTooling()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#tooling-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-tooling-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task going_to_a_page_and_opening_the_column_chooser_updates_the_grid_tooling()
    {
        await NavigateTooling();

        // GoToPage moves the pager to page 2.
        await ClickWhenStable(Page.Locator("#tooling-go-page"));
        await Expect(Page.Locator("#tooling-status"))
            .ToHaveTextAsync("goToPage called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-tooling-grid .e-pager .e-numericitem.e-active"))
            .ToHaveTextAsync("2", new() { Timeout = 10000 });

        // ShowColumnChooser opens the column chooser dialog.
        await ClickWhenStable(Page.Locator("#tooling-column-chooser"));
        await Expect(Page.Locator("#tooling-status"))
            .ToHaveTextAsync("showColumnChooser called", new() { Timeout = 10000 });
        await Expect(Page.Locator(".e-ccdlg").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
