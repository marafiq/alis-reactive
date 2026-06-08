namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridLockedEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/LockedResidentEdit";

    private ILocator EditInputs => Page.Locator("#locked-edit-grid .e-gridcontent input");

    private async Task NavigateLocked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#lock-edit-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#locked-edit-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task canceling_begin_edit_blocks_the_locked_resident_but_allows_others()
    {
        await NavigateLocked();

        // Resident 6001 is locked: Cancel() in the beginEdit handler blocks the editor.
        await ClickWhenStable(Page.Locator("#edit-6001"));
        await Expect(Page.Locator("#lock-result"))
            .ToHaveTextAsync("edit blocked for 6001", new() { Timeout = 10000 });
        await Expect(EditInputs).ToHaveCountAsync(0, new() { Timeout = 10000 });

        // Resident 6000 is not locked: the editor opens.
        await ClickWhenStable(Page.Locator("#edit-6000"));
        await Expect(Page.Locator("#lock-edit-status"))
            .ToHaveTextAsync("editing 6000", new() { Timeout = 10000 });
        await Expect(EditInputs.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
