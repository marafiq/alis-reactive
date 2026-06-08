using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBeginEditNormal : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BeginEditNormal";

    private async Task NavigateProofView()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#begin-edit-load-status"))
            .ToHaveTextAsync("loaded beginEdit rows", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-begin-edit-normal .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task begin_edit_normal_reads_row_data_and_can_cancel_edit()
    {
        Assert.That(
            typeof(FusionGridBeginEditArgs<ResidentDirectoryGridItem>).GetProperty("Row"),
            Is.Null);
        Assert.That(
            typeof(FusionGridBeginEditArgs<ResidentDirectoryGridItem>).GetProperty("ForeignKeyData"),
            Is.Null);
        Assert.That(
            typeof(FusionGridBeginEditArgs<ResidentDirectoryGridItem>).GetProperty("IsScroll"),
            Is.Null);
        Assert.That(
            typeof(FusionGridBeginEditArgs<ResidentDirectoryGridItem>).GetProperty("Name"),
            Is.Null);
        Assert.That(
            typeof(FusionGridBeginEditArgs<ResidentDirectoryGridItem>).GetProperty("PrimaryKey"),
            Is.Null);
        Assert.That(
            typeof(FusionGridBeginEditArgs<ResidentDirectoryGridItem>).GetProperty("PrimaryKeyValue"),
            Is.Null);
        Assert.That(
            typeof(FusionGridBeginEditArgs<ResidentDirectoryGridItem>).GetProperty("RequestType"),
            Is.Null);
        Assert.That(
            typeof(FusionGridBeginEditArgs<ResidentDirectoryGridItem>).GetProperty("Target"),
            Is.Null);

        await NavigateProofView();

        await ClickWhenStable(Page.Locator("#begin-edit-select-normal"));
        await ClickWhenStable(Page.Locator("#begin-edit-start-normal"));

        await Expect(Page.Locator("#begin-edit-resident"))
            .ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-row"))
            .ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-type"))
            .ToHaveTextAsync("edit", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-cancel"))
            .ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-begin-edit-normal .e-editedrow"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#begin-edit-close"));
        await Expect(Page.Locator("#grid-begin-edit-normal .e-editedrow"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#begin-edit-select-locked"));
        await ClickWhenStable(Page.Locator("#begin-edit-start-locked"));

        await Expect(Page.Locator("#begin-edit-resident"))
            .ToHaveTextAsync("Grace Bennett", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-row"))
            .ToHaveTextAsync("1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-type"))
            .ToHaveTextAsync("edit", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-cancel"))
            .ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-cancelled"))
            .ToHaveTextAsync("edit cancelled", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-begin-edit-normal .e-editedrow"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task begin_edit_dialog_reads_match_the_normal_edit_variant()
    {
        await NavigateProofView();
        await Expect(Page.Locator("#grid-begin-edit-dialog .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#begin-edit-select-dialog"));
        await ClickWhenStable(Page.Locator("#begin-edit-start-dialog"));

        // Dialog editor opens (mode is Dialog, not the inline edited row).
        // The dialog wrapper id is derived deterministically from the grid id.
        await Expect(Page.Locator("#grid-begin-edit-dialog_dialogEdit_wrapper"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        // The typed beginEdit payload reads identically to the normal-mode variant.
        await Expect(Page.Locator("#begin-edit-dialog-resident"))
            .ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-dialog-row"))
            .ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#begin-edit-dialog-type"))
            .ToHaveTextAsync("edit", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
