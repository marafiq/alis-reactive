namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBuilderRoster : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BuilderRoster";

    [Test]
    public async Task builder_owned_data_source_renders_the_roster_without_a_fetch()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);

        // The roster rows come from the grid builder's own dataSource (no HTTP request).
        await Expect(Page.Locator("#builder-roster-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#builder-roster-grid")).ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#builder-roster-grid")).ToContainTextAsync("Grace Bennett", new() { Timeout = 10000 });
        await Expect(Page.Locator("#builder-roster-grid")).ToContainTextAsync("Memory Care", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
