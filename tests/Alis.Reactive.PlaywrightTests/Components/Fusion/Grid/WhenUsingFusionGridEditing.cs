using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridEditing : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Editing";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentGridEditingModel";
    private const string ResidentNameId = GeneratedTypeScope + "__ResidentName";
    private const string RiskLevelId = GeneratedTypeScope + "__RiskLevel";
    private const string OpenTasksId = GeneratedTypeScope + "__OpenTasks";

    private FusionTextBoxLocator ResidentName => new(Page, ResidentNameId);
    private DropDownListLocator RiskLevel => new(Page, RiskLevelId);
    private NumericTextBoxLocator OpenTasks => new(Page, OpenTasksId);

    private ILocator ErrorFor(string property) => Page.Locator($"#{GeneratedTypeScope}__{property}_error");

    private async Task NavigateEditing()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#editing-load-status"))
            .ToHaveTextAsync("loaded editing rows", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-inline-edit-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task inline_editing_methods_add_update_edit_and_delete_rows()
    {
        await NavigateEditing();

        await ClickWhenStable(Page.Locator("#inline-add-literal"));
        await Expect(Page.Locator("#resident-inline-edit-grid")).ToContainTextAsync("Zara Inline", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#inline-add-server"));
        await Expect(Page.Locator("#resident-inline-edit-grid")).ToContainTextAsync("Sofia Server", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-command-status")).ToHaveTextAsync(
            "Sofia Server loaded from the server",
            new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#inline-select-first"));
        await ClickWhenStable(Page.Locator("#inline-start-edit"));
        await Expect(Page.Locator("#inline-begin-row")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-inline-edit-grid .e-editedrow")).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#inline-close-edit"));
        await Expect(Page.Locator("#inline-command-status")).ToHaveTextAsync("closeEdit called", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#inline-update-row"));
        await Expect(Page.Locator("#resident-inline-edit-grid")).ToContainTextAsync("Amina Updated", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#inline-select-first"));
        await ClickWhenStable(Page.Locator("#inline-delete-selected"));
        await Expect(Page.Locator("#inline-command-status")).ToHaveTextAsync("deleteRecord called", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task action_begin_save_edit_reads_typed_current_previous_and_action_fields()
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

        await NavigateEditing();

        await ClickWhenStable(Page.Locator("#inline-select-first"));
        await ClickWhenStable(Page.Locator("#inline-start-edit"));
        await Expect(Page.Locator("#resident-inline-edit-grid .e-editedrow"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        var editInput = Page.Locator("#resident-inline-edit-grid input[name='residentName']");
        await Expect(editInput).ToBeVisibleAsync(new() { Timeout = 10000 });
        await editInput.FillAsync("Amina ActionBegin");

        await ClickWhenStable(Page.Locator("#inline-end-edit"));

        await Expect(Page.Locator("#inline-action-begin"))
            .ToHaveTextAsync("save", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-action-begin-action"))
            .ToHaveTextAsync("edit", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-action-begin-type"))
            .ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-action-begin-event"))
            .ToHaveTextAsync("actionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-action-begin-cancel"))
            .ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-action-begin-row"))
            .ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-action-begin-selected-row"))
            .ToHaveTextAsync("-1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-action-begin-resident"))
            .ToHaveTextAsync("Amina ActionBegin", new() { Timeout = 10000 });
        await Expect(Page.Locator("#inline-action-begin-previous-resident"))
            .ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-inline-edit-grid"))
            .ToContainTextAsync("Amina ActionBegin", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source()
    {
        Assert.That(
            typeof(FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>).GetProperty("Cell"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>).GetProperty("Column"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>).GetProperty("ColumnObject"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>).GetProperty("IsForeignKey"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>).GetProperty("Name"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>).GetProperty("Cancel"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>).GetProperty("Cell"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>).GetProperty("Column"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>).GetProperty("ColumnObject"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>).GetProperty("IsForeignKey"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>).GetProperty("Name"),
            Is.Null);
        Assert.That(
            typeof(FusionGridBeforeBatchSaveArgs<ResidentDirectoryGridItem>).GetProperty("Name"),
            Is.Null);
        Assert.That(
            typeof(FusionGridEditEventArgsExtensions).GetMethods()
                .Where(method => method.Name == nameof(FusionGridEditEventArgsExtensions.Cancel))
                .Any(method => method.GetParameters()[0].ParameterType.Name.StartsWith("FusionGridCellSavedArgs")),
            Is.False);

        await NavigateEditing();

        await ClickWhenStable(Page.Locator("#batch-edit-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid input.e-field").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Page.Locator("#resident-batch-edit-grid input.e-field").First.FillAsync("4");
        await ClickWhenStable(Page.Locator("#batch-save-cell"));

        await Expect(Page.Locator("#batch-cell-save-column")).ToHaveTextAsync("openTasks", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-value")).ToHaveTextAsync("4", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-previous")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-resident")).ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-cancel")).ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-saved-column")).ToHaveTextAsync("openTasks", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-saved-value")).ToHaveTextAsync("4", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-saved-previous")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-saved-resident")).ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#batch-update-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid")).ToContainTextAsync("6", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#batch-gather-changes"));
        await Expect(Page.Locator("#batch-summary")).ToHaveTextAsync(
            "batch added 0, changed 1, deleted 0",
            new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#batch-end-edit"));
        await Expect(Page.Locator("#batch-before-save-tasks")).ToHaveTextAsync("6", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-before-save-resident")).ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-before-save-cancel")).ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-action-complete")).Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#batch-edit-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid input.e-field").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Page.Locator("#resident-batch-edit-grid input.e-field").First.FillAsync("99");
        await ClickWhenStable(Page.Locator("#batch-save-cell"));
        await Expect(Page.Locator("#batch-cell-save-value")).ToHaveTextAsync("99", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-previous")).ToHaveTextAsync("6", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-cancelled")).ToHaveTextAsync("blocked 99", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-batch-edit-grid")).ToContainTextAsync("6", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-batch-edit-grid")).Not.ToContainTextAsync("99", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#batch-edit-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid input.e-field").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Page.Locator("#resident-batch-edit-grid input.e-field").First.FillAsync("8");
        await ClickWhenStable(Page.Locator("#batch-save-cell"));
        await Expect(Page.Locator("#batch-cell-save-value")).ToHaveTextAsync("8", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#batch-end-edit"));
        await Expect(Page.Locator("#batch-before-save-tasks")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-before-save-cancelled")).ToHaveTextAsync("blocked batch 8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-action-complete")).ToHaveTextAsync("waiting after cancelled batch", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-batch-edit-grid")).ToContainTextAsync("8", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#batch-gather-changes"));
        await Expect(Page.Locator("#batch-summary")).ToHaveTextAsync(
            "batch added 0, changed 1, deleted 0",
            new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-action-complete"))
            .ToHaveTextAsync("waiting after cancelled batch", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dialog_editing_renders_builder_template_fields_and_typed_begin_event()
    {
        await NavigateEditing();

        await ClickWhenStable(Page.Locator("#dialog-select-first"));
        await ClickWhenStable(Page.Locator("#dialog-start-edit"));

        await Expect(Page.Locator("input[name='residentName']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("select[name='riskLevel']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#dialog-begin-resident"))
            .Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });

        await Page.Keyboard.PressAsync("Escape");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_updates_grid_only_after_form_rules_pass()
    {
        await NavigateEditing();

        await ClickWhenStable(Page.Locator("#validation-save-grid-row"));
        await Expect(ErrorFor("ResidentName")).ToContainTextAsync("required", new() { Timeout = 10000 });
        await Expect(ErrorFor("RiskLevel")).ToContainTextAsync("required", new() { Timeout = 10000 });
        await Expect(ErrorFor("OpenTasks")).ToContainTextAsync("required", new() { Timeout = 10000 });

        await ResidentName.FillAndBlur("Li");
        await Expect(ErrorFor("ResidentName")).ToContainTextAsync("at least 3", new() { Timeout = 10000 });

        await ResidentName.FillAndBlur("Mara Validated");
        await RiskLevel.Select("High");
        await OpenTasks.FillAndBlur("9");
        await ClickWhenStable(Page.Locator("#validation-save-grid-row"));
        await Expect(ErrorFor("OpenTasks")).ToContainTextAsync("between 0 and 7", new() { Timeout = 10000 });

        await OpenTasks.FillAndBlur("4");
        await ClickWhenStable(Page.Locator("#validation-save-grid-row"));

        await Expect(Page.Locator("#validation-status")).ToContainTextAsync(
            "Mara Validated passed validation",
            new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-inline-edit-grid")).ToContainTextAsync("Mara Validated", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
