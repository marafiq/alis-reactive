using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.PivotView;

[TestFixture]
public class WhenUsingFusionPivotView : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/PivotView?facilityId=evergreen";

    private async Task NavigateAndWaitForPivotView()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Page.Locator("#care-pivot.e-pivotview"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-pivot"))
            .ToContainTextAsync("Residents", new() { Timeout = 10000 });
    }

    [Test]
    public async Task builder_renders_initial_care_census_pivot()
    {
        await NavigateAndWaitForPivotView();

        var pivotText = await Page.Locator("#care-pivot").TextContentAsync();
        var compactPivotText = Regex.Replace(pivotText ?? "", @"[\s,]+", "");

        await Expect(Page.Locator("#care-pivot"))
            .ToContainTextAsync("North", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-pivot"))
            .ToContainTextAsync("Memory Care", new() { Timeout = 5000 });
        Assert.Multiple(() =>
        {
            Assert.That(pivotText, Does.Contain("Jan"));
            Assert.That(pivotText, Does.Contain("Feb"));
            Assert.That(compactPivotText, Does.Contain("126000"));
            Assert.That(compactPivotText, Does.Contain("140000"));
        });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typed_sources_read_current_view_and_persisted_layout()
    {
        await NavigateAndWaitForPivotView();

        await Page.Locator("#read-pivot-btn").ClickAsync();

        await Expect(Page.Locator("#current-view"))
            .ToHaveTextAsync("Table", new() { Timeout = 5000 });
        await Expect(Page.Locator("#layout-json"))
            .ToContainTextAsync("dataSourceSettings", new() { Timeout = 5000 });
        await Expect(Page.Locator("#layout-json"))
            .ToContainTextAsync("\"dataSource\":[]", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task http_response_replaces_nested_datasource_and_refreshes_pivot()
    {
        await NavigateAndWaitForPivotView();

        await Page.Locator("#load-march-btn").ClickAsync();

        await Expect(Page.Locator("#data-status"))
            .ToHaveTextAsync("March census loaded for evergreen", new() { Timeout = 10000 });
        await Expect(Page.Locator("#data-bound-status"))
            .ToHaveTextAsync("dataBound fired", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-pivot"))
            .ToContainTextAsync("Mar", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-pivot"))
            .ToContainTextAsync("East", new() { Timeout = 10000 });

        var pivotText = await Page.Locator("#care-pivot").TextContentAsync();
        var compactPivotText = Regex.Replace(pivotText ?? "", @"[\s,]+", "");
        Assert.Multiple(() =>
        {
            Assert.That(pivotText, Does.Contain("Independent"));
            Assert.That(pivotText, Does.Not.Contain("Jan"));
            Assert.That(pivotText, Does.Not.Contain("Feb"));
            Assert.That(compactPivotText, Does.Contain("14"));
            Assert.That(compactPivotText, Does.Contain("63000"));
        });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task refresh_button_calls_public_refresh_method_and_updates_visible_status()
    {
        await NavigateAndWaitForPivotView();

        await Page.Locator("#refresh-pivot-btn").ClickAsync();

        await Expect(Page.Locator("#refresh-status"))
            .ToHaveTextAsync("refresh called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#data-bound-status"))
            .ToHaveTextAsync("dataBound fired", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_uses_route_url_and_method_return_sources()
    {
        await NavigateAndWaitForPivotView();

        await Page.Locator("#audit-pivot-btn").ClickAsync();

        await Expect(Page.Locator("#audit-summary"))
            .ToHaveTextAsync("Table:evergreen:Table", new() { Timeout = 10000 });
        await Expect(Page.Locator("#layout-length"))
            .ToHaveTextAsync(new Regex("^[1-9][0-9]+$"), new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task persisted_layout_can_round_trip_through_server_and_reload()
    {
        await NavigateAndWaitForPivotView();

        await Page.Locator("#load-layout-btn").ClickAsync();

        await Expect(Page.Locator("#layout-status"))
            .ToHaveTextAsync(new Regex("^layout echoed:[1-9][0-9]+$"), new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cell_selecting_event_exposes_typed_cell_payload()
    {
        await NavigateAndWaitForPivotView();

        var valueCells = Page.Locator("#care-pivot .e-valuescontent");
        await Expect(valueCells.First).ToBeVisibleAsync(new() { Timeout = 10000 });
        await valueCells.First.ClickAsync();

        await Expect(Page.Locator("#cell-axis"))
            .ToHaveTextAsync("value", new() { Timeout = 10000 });
        await Expect(Page.Locator("#cell-text"))
            .Not.ToHaveTextAsync("-", new() { Timeout = 10000 });
        await Expect(Page.Locator("#cell-value"))
            .ToHaveTextAsync(new Regex("^[0-9,.]+$"), new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task public_dialog_methods_open_visible_syncfusion_dialogs()
    {
        await NavigateAndWaitForPivotView();

        await Page.Locator("#formatting-dialog-btn").ClickAsync();

        await Expect(Page.Locator("#dialog-status"))
            .ToHaveTextAsync("conditional formatting opened", new() { Timeout = 5000 });
        await Expect(Page.Locator(".e-dialog").Filter(new() { HasTextString = "Conditional Formatting" }))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        await Page.Locator(".e-dialog .e-dlg-closeicon-btn").First.ClickAsync();
        await Page.Locator("#calculated-dialog-btn").ClickAsync();

        await Expect(Page.Locator("#dialog-status"))
            .ToHaveTextAsync("calculated field opened", new() { Timeout = 5000 });
        await Expect(Page.Locator(".e-dialog").Filter(new() { HasTextString = "Calculated Field" }))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_index_links_to_pivotview_sandbox()
    {
        await NavigateTo("/Sandbox/Components");
        await Expect(Page.Locator("a[href='/Sandbox/Components/PivotView/Index']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        await Page.Locator("a[href='/Sandbox/Components/PivotView/Index']").ClickAsync();
        await Page.WaitForURLAsync("**/Sandbox/Components/PivotView/Index", new() { Timeout = 10000 });

        await Expect(Page.Locator("#care-pivot.e-pivotview"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-pivot"))
            .ToContainTextAsync("Residents", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_declares_typed_runtime_members()
    {
        await NavigateAndWaitForPivotView();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("care-pivot"));
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("currentView"));
        Assert.That(planJson, Does.Contain("dataSource"));
        Assert.That(planJson, Does.Contain("dataSourceSettings"));
        Assert.That(planJson, Does.Contain("refresh"));
        Assert.That(planJson, Does.Contain("getPersistData"));
        Assert.That(planJson, Does.Contain("loadPersistData"));
        Assert.That(planJson, Does.Contain("showConditionalFormattingDialog"));
        Assert.That(planJson, Does.Contain("createCalculatedFieldDialog"));
        Assert.That(planJson, Does.Match(new Regex("dataBound|cellSelecting")));

        AssertNoConsoleErrors();
    }
}
