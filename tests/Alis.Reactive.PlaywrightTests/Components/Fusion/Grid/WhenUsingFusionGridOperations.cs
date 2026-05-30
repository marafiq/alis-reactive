using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridOperations : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Operations";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_GridOperationsModel";
    private const string PatchRiskLevelId = Scope + "__PatchRiskLevel";
    private const string PatchOpenTasksId = Scope + "__PatchOpenTasks";

    private DropDownListLocator PatchRiskLevel => new(Page, PatchRiskLevelId);
    private NumericTextBoxLocator PatchOpenTasks => new(Page, PatchOpenTasksId);

    private async Task NavigateOperations()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#operations-load-status"))
            .ToHaveTextAsync("loaded operations rows", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-operations-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task components_index_links_to_grid_operations_card()
    {
        await NavigateTo("/Sandbox/Components");

        await Expect(Page.Locator("a").Filter(new() { HasText = "Grid Operations" }))
            .ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task template_columns_and_template_actions_drive_real_workflows()
    {
        await NavigateOperations();

        await Expect(Page.Locator("#resident-operations-grid .grid-resident-template").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-operations-grid .grid-action-template button").First)
            .ToHaveTextAsync("Review", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#resident-operations-grid .grid-action-template button").First);
        await Expect(Page.Locator("#template-action-status"))
            .ToContainTextAsync("review started for", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task column_methods_hide_show_and_reorder_visible_columns()
    {
        await NavigateOperations();

        await ClickWhenStable(Page.Locator("#ops-hide-care-columns"));
        await Expect(Page.Locator("#column-command-status"))
            .ToHaveTextAsync("care columns hidden", new() { Timeout = 10000 });
        await Expect(VisibleHeader("Primary Nurse"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(VisibleHeader("Next Review"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#ops-show-care-columns"));
        await Expect(VisibleHeader("Primary Nurse"))
            .ToHaveCountAsync(1, new() { Timeout = 10000 });
        await Expect(VisibleHeader("Next Review"))
            .ToHaveCountAsync(1, new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#ops-reorder-risk"));
        await Expect(Page.Locator("#column-command-status"))
            .ToHaveTextAsync("risk column moved before resident", new() { Timeout = 10000 });
        var headers = await Page.Locator("#resident-operations-grid .e-headercell:visible .e-headertext")
            .AllInnerTextsAsync();
        var visibleHeaders = headers.ToList();

        Assert.That(visibleHeaders.IndexOf("Risk"), Is.LessThan(visibleHeaders.IndexOf("Resident")));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task current_view_key_and_selection_sources_feed_http_requests()
    {
        await NavigateOperations();

        await ClickWhenStable(Page.Locator("#ops-select-range"));
        await Expect(Page.Locator("#selection-range-status"))
            .ToContainTextAsync("selected records:", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#ops-current-view"));
        await Expect(Page.Locator("#current-view-status"))
            .ToContainTextAsync("current view has 12 residents", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#ops-row-index"));
        await Expect(Page.Locator("#row-index-status"))
            .ToHaveTextAsync("resident 6005 is visible at row index 5", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task keyed_cell_and_row_updates_change_visible_rows()
    {
        await NavigateOperations();

        await ClickWhenStable(Page.Locator("#ops-set-risk-cell"));
        await Expect(Page.Locator("#keyed-update-status"))
            .ToHaveTextAsync("resident 6002 risk flagged", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-operations-grid"))
            .ToContainTextAsync("Critical", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#ops-set-tasks-cell"));
        var row6003 = Page.Locator("#resident-operations-grid .e-row").Filter(new() { HasText = "Irene Morgan" }).First;
        await Expect(row6003).ToContainTextAsync("7", new() { Timeout = 10000 });

        await PatchRiskLevel.Select("Moderate");
        await PatchOpenTasks.FillAndBlur("6");
        await ClickWhenStable(Page.Locator("#ops-server-patch-row"));
        await Expect(Page.Locator("#keyed-update-status"))
            .ToHaveTextAsync("Lena Server Patch changed to Moderate risk with 6 tasks", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-operations-grid"))
            .ToContainTextAsync("Lena Server Patch", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-operations-grid"))
            .ToContainTextAsync("Clinical Review Team", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    private ILocator VisibleHeader(string text) =>
        Page.Locator("#resident-operations-grid .e-headercell:visible .e-headertext")
            .Filter(new() { HasText = text });
}
