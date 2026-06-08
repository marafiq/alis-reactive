namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridInlineEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/InlineEdit";

    private ILocator EditInputs => Page.Locator("#inline-edit-grid .e-gridcontent input");

    private async Task NavigateInline()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#inline-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-edit-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task starting_canceling_and_committing_an_inline_edit_toggles_the_row_editor()
    {
        await NavigateInline();

        // StartEdit opens the inline row editor.
        await ClickWhenStable(Page.Locator("#inline-start-edit"));
        await Expect(Page.Locator("#inline-status"))
            .ToHaveTextAsync("startEdit called", new() { Timeout = 10000 });
        await Expect(EditInputs.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        // CloseEdit cancels and closes the editor.
        await ClickWhenStable(Page.Locator("#inline-close-edit"));
        await Expect(Page.Locator("#inline-status"))
            .ToHaveTextAsync("closeEdit called", new() { Timeout = 10000 });
        await Expect(EditInputs).ToHaveCountAsync(0, new() { Timeout = 10000 });

        // StartEdit again, then EndEdit commits and closes the editor.
        await ClickWhenStable(Page.Locator("#inline-start-edit"));
        await Expect(EditInputs.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#inline-end-edit"));
        await Expect(Page.Locator("#inline-status"))
            .ToHaveTextAsync("endEdit called", new() { Timeout = 10000 });
        await Expect(EditInputs).ToHaveCountAsync(0, new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
