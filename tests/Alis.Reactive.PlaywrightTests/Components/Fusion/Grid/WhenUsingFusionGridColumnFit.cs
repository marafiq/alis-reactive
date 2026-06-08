namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridColumnFit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ColumnFit";

    // residentId is a hidden column whose headercell stays in the DOM (display:none),
    // so index against visible headercells only.
    private ILocator NameHeader => Page.Locator("#column-fit-grid .e-headercell:visible").Nth(0);
    private ILocator RiskHeader => Page.Locator("#column-fit-grid .e-headercell:visible").Nth(1);

    private static async Task<double> WidthAsync(ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();
        return box?.Width ?? 0;
    }

    private static async Task AssertWidthBelowAsync(ILocator locator, double max)
    {
        for (var i = 0; i < 30; i++)
        {
            if (await WidthAsync(locator) is var w && w > 0 && w < max) return;
            await Task.Delay(100);
        }
        Assert.That(await WidthAsync(locator), Is.LessThan(max));
    }

    private async Task NavigateFit()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#fit-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#column-fit-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task auto_fitting_one_column_then_all_columns_shrinks_them_to_content()
    {
        await NavigateFit();

        // Both columns start wide (400px).
        Assert.That(await WidthAsync(RiskHeader), Is.GreaterThan(300));
        Assert.That(await WidthAsync(NameHeader), Is.GreaterThan(300));

        // AutoFitColumn(risk): risk shrinks to content, name stays wide.
        await ClickWhenStable(Page.Locator("#fit-risk"));
        await Expect(Page.Locator("#fit-status"))
            .ToHaveTextAsync("autoFitColumn called", new() { Timeout = 10000 });
        await AssertWidthBelowAsync(RiskHeader, 300);
        Assert.That(await WidthAsync(NameHeader), Is.GreaterThan(300));

        // AutoFitColumns(): name shrinks too.
        await ClickWhenStable(Page.Locator("#fit-all"));
        await Expect(Page.Locator("#fit-status"))
            .ToHaveTextAsync("autoFitColumns called", new() { Timeout = 10000 });
        await AssertWidthBelowAsync(NameHeader, 300);

        AssertNoConsoleErrors();
    }
}
