namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionKanban : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Kanban";

    private async Task ResetKanbanBoard()
    {
        using var client = new System.Net.Http.HttpClient();
        var response = await client.PostAsync($"{BaseUrl}/api/kanban/reset", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task NavigateAndWaitForKanban(
        bool reset = true,
        int cardCount = 2,
        string expectedCardText = "Assess fall risk")
    {
        if (reset)
            await ResetKanbanBoard();

        await NavigateToAndWaitForBoot(Path);
        await Expect(Page.Locator("#care-kanban.e-kanban"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban .e-card"))
            .ToHaveCountAsync(cardCount, new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync(expectedCardText, new() { Timeout = 10000 });
    }

    private async Task DragCardToColumn(int cardId, string columnKey)
    {
        var card = Page.Locator($"#care-kanban .e-card[data-id='{cardId}']").First;
        var column = Page.Locator($"#care-kanban .e-content-cells[data-key='{columnKey}']")
            .First;

        await Expect(card).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(column).ToBeVisibleAsync(new() { Timeout = 10000 });
        await card.ScrollIntoViewIfNeededAsync();
        await column.ScrollIntoViewIfNeededAsync();

        var sourceBox = await card.BoundingBoxAsync();
        var targetBox = await column.BoundingBoxAsync();
        Assert.That(sourceBox, Is.Not.Null);
        Assert.That(targetBox, Is.Not.Null);

        var sourceX = sourceBox!.X + sourceBox.Width / 2;
        var sourceY = sourceBox.Y + sourceBox.Height / 2;
        var targetX = targetBox!.X + targetBox.Width / 2;
        var targetY = targetBox.Y + Math.Min(72, targetBox.Height / 2);

        await Page.Mouse.MoveAsync(
            sourceX,
            sourceY);
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync(sourceX + 24, sourceY + 12, new() { Steps = 8 });
        await Page.Mouse.MoveAsync(
            targetX,
            targetY,
            new() { Steps = 30 });
        await Page.Mouse.UpAsync();
    }

    [Test]
    public async Task remote_data_renders_with_typed_card_template()
    {
        await NavigateAndWaitForKanban();

        await Expect(Page.Locator("#kanban-status"))
            .ToHaveTextAsync("loaded:AL:2", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-open-count"))
            .ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#kanban-al-count"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#kanban-data-binding-status"))
            .ToHaveTextAsync("Assess fall risk", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-data-bound-status"))
            .ToHaveTextAsync("bound", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-query-cell-status"))
            .ToContainTextAsync("Row", new() { Timeout = 10000 });

        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync("Resident: Eleanor Reed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync("ESCALATED", new() { Timeout = 5000 });

        var renderedSummary = await Page.Locator("#kanban-rendered-summary").TextContentAsync();
        Assert.That(renderedSummary, Is.Not.EqualTo("none"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_reads_board_column_and_swimlane_sources()
    {
        await NavigateAndWaitForKanban();

        await Page.Locator("#kanban-audit-btn").ClickAsync();

        await Expect(Page.Locator("#kanban-audit-summary"))
            .ToHaveTextAsync("audit:2:1:2", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-audit-total"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#kanban-audit-open"))
            .ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#kanban-audit-al"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task card_click_event_exposes_card_payload_and_route_gather()
    {
        await NavigateAndWaitForKanban();

        await Page.Locator("#care-kanban .e-card").First.ClickAsync();

        await Expect(Page.Locator("#kanban-clicked-summary"))
            .ToHaveTextAsync("Assess fall risk", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-route-summary"))
            .ToHaveTextAsync("card:101:summary", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task card_double_click_event_exposes_card_payload()
    {
        await NavigateAndWaitForKanban();

        await Page.Locator("#care-kanban .e-card").First.DblClickAsync();

        await Expect(Page.Locator("#kanban-double-clicked-summary"))
            .ToHaveTextAsync("Assess fall risk", new() { Timeout = 10000 });

        if (await Page.Locator("#care-kanban_dialog_wrapper.e-popup-open").IsVisibleAsync())
            await Page.Keyboard.PressAsync("Escape");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task add_update_and_delete_card_methods_commit_through_datasource_changed_event()
    {
        await NavigateAndWaitForKanban();

        await Page.Locator("#kanban-add-card-btn").ClickAsync();

        await Expect(Page.Locator("#kanban-command-status"))
            .ToHaveTextAsync("post:900", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-action-begin"))
            .ToHaveTextAsync("cardCreate", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-action-request"))
            .ToHaveTextAsync("cardCreated", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-commit-status"))
            .ToHaveTextAsync("commit:cardCreated:1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban .e-card"))
            .ToHaveCountAsync(3, new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync("Remote wound consult", new() { Timeout = 5000 });

        await Page.Locator("#kanban-update-card-btn").ClickAsync();

        await Expect(Page.Locator("#kanban-command-status"))
            .ToHaveTextAsync("put:102:Review", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-action-begin"))
            .ToHaveTextAsync("cardChange", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-action-request"))
            .ToHaveTextAsync("cardChanged", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-commit-status"))
            .ToHaveTextAsync("commit:cardChanged:1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync("Medication reconciliation updated", new() { Timeout = 5000 });

        await Page.Locator("#kanban-delete-card-btn").ClickAsync();

        await Expect(Page.Locator("#kanban-command-status"))
            .ToHaveTextAsync("delete:101", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-action-begin"))
            .ToHaveTextAsync("cardRemove", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-action-request"))
            .ToHaveTextAsync("cardRemoved", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-commit-status"))
            .ToHaveTextAsync("commit:cardRemoved:1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban"))
            .Not.ToContainTextAsync("Assess fall risk", new() { Timeout = 10000 });

        await NavigateAndWaitForKanban(
            reset: false,
            cardCount: 2,
            expectedCardText: "Remote wound consult");
        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync("Medication reconciliation updated", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban"))
            .Not.ToContainTextAsync("Assess fall risk", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task drag_stop_persists_move_through_reactive_http_put()
    {
        await NavigateAndWaitForKanban();

        await DragCardToColumn(101, "Review");

        await Expect(Page.Locator("#kanban-drag-start-summary"))
            .ToHaveTextAsync("Assess fall risk", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-drag-status"))
            .ToHaveTextAsync("Assess fall risk", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-move-status"))
            .ToContainTextAsync("move:101:Review", new() { Timeout = 10000 });

        await Page.Locator("#kanban-audit-btn").ClickAsync();
        await Expect(Page.Locator("#kanban-audit-summary"))
            .ToHaveTextAsync("audit:2:0:2", new() { Timeout = 10000 });

        await NavigateAndWaitForKanban(reset: false);
        await Page.Locator("#kanban-audit-btn").ClickAsync();
        await Expect(Page.Locator("#kanban-audit-summary"))
            .ToHaveTextAsync("audit:2:0:2", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task column_spinner_and_dialog_methods_execute_against_kanban_object()
    {
        await NavigateAndWaitForKanban();

        await Page.Locator("#kanban-hide-done-btn").ClickAsync();
        await Expect(Page.Locator("#kanban-column-status"))
            .ToHaveTextAsync("done hidden", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-kanban .e-header-text").Filter(new() { HasTextString = "Done" }))
            .ToHaveCountAsync(0, new() { Timeout = 5000 });

        await Page.Locator("#kanban-show-done-btn").ClickAsync();
        await Expect(Page.Locator("#kanban-column-status"))
            .ToHaveTextAsync("done shown", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-kanban .e-header-text").Filter(new() { HasTextString = "Done" }))
            .ToHaveCountAsync(1, new() { Timeout = 5000 });

        await Page.Locator("#kanban-add-column-btn").ClickAsync();
        await Expect(Page.Locator("#kanban-column-status"))
            .ToHaveTextAsync("blocked added", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-kanban .e-header-text").Filter(new() { HasTextString = "Blocked" }))
            .ToHaveCountAsync(1, new() { Timeout = 5000 });

        await Page.Locator("#kanban-delete-column-btn").ClickAsync();
        await Expect(Page.Locator("#kanban-column-status"))
            .ToHaveTextAsync("blocked deleted", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-kanban .e-header-text").Filter(new() { HasTextString = "Blocked" }))
            .ToHaveCountAsync(0, new() { Timeout = 5000 });

        await Page.Locator("#kanban-spinner-btn").ClickAsync();
        await Expect(Page.Locator("#kanban-spinner-status"))
            .ToHaveTextAsync("spinner toggled", new() { Timeout = 5000 });

        await Page.Locator("#kanban-open-close-dialog-btn").ClickAsync();
        await Expect(Page.Locator("#kanban-dialog-status"))
            .ToHaveTextAsync("open close requested", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-kanban_dialog_wrapper.e-popup-open"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dialog_save_closes_and_persists_through_datasource_changed_event()
    {
        await NavigateAndWaitForKanban();

        await Page.Locator("#kanban-open-dialog-btn").ClickAsync();

        await Expect(Page.Locator("#kanban-dialog-status"))
            .ToHaveTextAsync("Add", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban_dialog_wrapper.e-dialog.e-popup-open"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        await Page.Locator("#care-kanban_dialog_wrapper .e-footer-content .e-primary").ClickAsync();

        await Expect(Page.Locator("#care-kanban_dialog_wrapper.e-popup-open"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-dialog-close-status"))
            .ToHaveTextAsync("Add", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-action-request"))
            .ToHaveTextAsync("cardCreated", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-commit-status"))
            .ToHaveTextAsync("commit:cardCreated:1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban .e-card"))
            .ToHaveCountAsync(3, new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync("Remote wound consult", new() { Timeout = 10000 });

        await NavigateAndWaitForKanban(
            reset: false,
            cardCount: 3,
            expectedCardText: "Remote wound consult");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task edit_dialog_save_closes_and_persists_updated_card()
    {
        await NavigateAndWaitForKanban();

        await Page.Locator("#kanban-open-edit-dialog-btn").ClickAsync();

        await Expect(Page.Locator("#kanban-dialog-status"))
            .ToHaveTextAsync("Edit", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban_dialog_wrapper.e-dialog.e-popup-open"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        await Page.Locator("#care-kanban_dialog_wrapper .e-footer-content .e-primary").ClickAsync();

        await Expect(Page.Locator("#care-kanban_dialog_wrapper.e-popup-open"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-dialog-close-status"))
            .ToHaveTextAsync("Edit", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-action-request"))
            .ToHaveTextAsync("cardChanged", new() { Timeout = 10000 });
        await Expect(Page.Locator("#kanban-commit-status"))
            .ToHaveTextAsync("commit:cardChanged:1", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync("Medication reconciliation updated", new() { Timeout = 10000 });

        await NavigateAndWaitForKanban(reset: false);
        await Expect(Page.Locator("#care-kanban"))
            .ToContainTextAsync("Medication reconciliation updated", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_index_links_to_kanban_sandbox()
    {
        await NavigateTo("/Sandbox/Components");
        await Expect(Page.Locator("a[href='/Sandbox/Components/Kanban/Index']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_declares_typed_kanban_runtime_members()
    {
        await NavigateAndWaitForKanban();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("care-kanban"));
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("dataSource"));
        Assert.That(planJson, Does.Contain("dataBind"));
        Assert.That(planJson, Does.Contain("getColumnData"));
        Assert.That(planJson, Does.Contain("getSwimlaneData"));
        Assert.That(planJson, Does.Contain("addCard"));
        Assert.That(planJson, Does.Contain("updateCard"));
        Assert.That(planJson, Does.Contain("deleteCard"));
        Assert.That(planJson, Does.Contain("addColumn"));
        Assert.That(planJson, Does.Contain("deleteColumn"));
        Assert.That(planJson, Does.Contain("showColumn"));
        Assert.That(planJson, Does.Contain("hideColumn"));
        Assert.That(planJson, Does.Contain("showSpinner"));
        Assert.That(planJson, Does.Contain("hideSpinner"));
        Assert.That(planJson, Does.Contain("openDialog"));
        Assert.That(planJson, Does.Contain("closeDialog"));
        Assert.That(planJson, Does.Contain("dataBinding"));
        Assert.That(planJson, Does.Contain("dataBound"));
        Assert.That(planJson, Does.Contain("cardClick"));
        Assert.That(planJson, Does.Contain("cardDoubleClick"));
        Assert.That(planJson, Does.Contain("queryCellInfo"));
        Assert.That(planJson, Does.Contain("cardRendered"));
        Assert.That(planJson, Does.Contain("actionBegin"));
        Assert.That(planJson, Does.Contain("actionComplete"));
        Assert.That(planJson, Does.Contain("dataSourceChanged"));
        Assert.That(planJson, Does.Contain("dragStart"));
        Assert.That(planJson, Does.Contain("drag"));
        Assert.That(planJson, Does.Contain("dragStop"));
        Assert.That(planJson, Does.Contain("dialogOpen"));
        Assert.That(planJson, Does.Contain("dialogClose"));

        AssertNoConsoleErrors();
    }
}
