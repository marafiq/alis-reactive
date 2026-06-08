namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridServerRoster : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ServerRoster";

    private async Task NavigateRoster()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        // SetDataSource [response path]: the Result array bound out of the {Result, Count} body.
        await Expect(Page.Locator("#server-load-status"))
            .ToHaveTextAsync("loaded via response path", new() { Timeout = 10000 });
        await Expect(Page.Locator("#server-roster-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#server-roster-grid"))
            .ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#server-keyed-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task admitting_a_server_resident_reads_the_row_from_the_response()
    {
        await NavigateRoster();

        await ClickWhenStable(Page.Locator("#server-admit"));

        await Expect(Page.Locator("#server-command")).ToHaveTextAsync("server resident admitted", new() { Timeout = 10000 });
        await Expect(Page.Locator("#server-roster-grid")).ToContainTextAsync("Sofia Server", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task updating_row_zero_reads_the_row_from_the_response()
    {
        await NavigateRoster();

        await ClickWhenStable(Page.Locator("#server-update"));

        await Expect(Page.Locator("#server-command")).ToHaveTextAsync("server row updated", new() { Timeout = 10000 });
        // Keyed grid row 0 (Amina Patel) is updated in place from the server row.
        await Expect(Page.Locator("#server-keyed-grid .e-row").First).ToContainTextAsync("Amina Server Updated", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task patching_a_keyed_resident_reads_the_row_from_the_response()
    {
        await NavigateRoster();

        await ClickWhenStable(Page.Locator("#server-patch"));

        await Expect(Page.Locator("#server-command")).ToHaveTextAsync("server row patched", new() { Timeout = 10000 });
        await Expect(Page.Locator("#server-keyed-grid")).ToContainTextAsync("Lena Server Patch", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task loading_a_nested_page_binds_from_the_nested_data_source_path()
    {
        await NavigateRoster();

        // SetDataSource [nested data-source property path]: bind from Page.Result of the envelope.
        await ClickWhenStable(Page.Locator("#server-nested"));

        await Expect(Page.Locator("#server-command")).ToHaveTextAsync("loaded nested path", new() { Timeout = 10000 });
        await Expect(Page.Locator("#server-keyed-grid")).ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#server-keyed-grid")).ToContainTextAsync("Henry Liu", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
