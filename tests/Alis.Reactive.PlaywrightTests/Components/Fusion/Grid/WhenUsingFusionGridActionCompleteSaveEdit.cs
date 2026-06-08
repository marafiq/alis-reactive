using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridActionCompleteSaveEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ActionCompleteSaveEdit";

    private async Task NavigateProofView()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#ac-load-status"))
            .ToHaveTextAsync("loaded actionComplete rows", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-complete-save-edit .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task action_complete_save_edit_reads_typed_current_previous_and_action_fields()
    {
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("Row"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("Form"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("Target"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("ForeignKeyData"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("IsScroll"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("PrimaryKey"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("PrimaryKeyValue"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("RowData"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("Index"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditActionArgs<ResidentDirectoryGridItem>).GetProperty("Promise"),
            Is.Null);

        await NavigateProofView();

        await ClickWhenStable(Page.Locator("#ac-select-first"));
        await ClickWhenStable(Page.Locator("#ac-start-edit"));
        await Expect(Page.Locator("#grid-action-complete-save-edit .e-editedrow"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        var editInput = Page.Locator("#grid-action-complete-save-edit input[name='residentName']");
        await Expect(editInput).ToBeVisibleAsync(new() { Timeout = 10000 });
        await editInput.FillAsync("Amina ActionComplete");

        await ClickWhenStable(Page.Locator("#ac-end-edit"));

        await Expect(Page.Locator("#ac-request-type"))
            .ToHaveTextAsync("save", new() { Timeout = 10000 });
        await Expect(Page.Locator("#ac-action"))
            .ToHaveTextAsync("edit", new() { Timeout = 10000 });
        await Expect(Page.Locator("#ac-type"))
            .ToHaveTextAsync("actionComplete", new() { Timeout = 10000 });
        await Expect(Page.Locator("#ac-name"))
            .ToHaveTextAsync("actionComplete", new() { Timeout = 10000 });
        await Expect(Page.Locator("#ac-cancel"))
            .ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#ac-row-index"))
            .ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#ac-selected-row"))
            .ToHaveTextAsync("-1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#ac-current-resident"))
            .ToHaveTextAsync("Amina ActionComplete", new() { Timeout = 10000 });
        await Expect(Page.Locator("#ac-previous-resident"))
            .ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#grid-action-complete-save-edit"))
            .ToContainTextAsync("Amina ActionComplete", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
