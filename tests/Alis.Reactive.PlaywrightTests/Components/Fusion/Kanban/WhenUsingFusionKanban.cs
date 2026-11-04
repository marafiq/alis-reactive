using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Kanban;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Kanban;

[TestFixture]
public class WhenUsingFusionKanban : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Kanban";
    private const string CommitUrl = "**/Sandbox/Components/Kanban/Commit";
    private const string WorkListsUrl = "**/Sandbox/Components/Kanban/WorkLists";
    private const string FinishedWorkIsClosed = "Finished work is closed. Open a new work order if the problem comes back.";

    private static readonly Regex Spinning = new(@"\be-spin-show\b");
    private static readonly Regex NotSpinning = new(@"\be-spin-hide\b");
    private static readonly Regex Folded = new(@"\be-collapsed\b");

    private PagePlan<MaintenanceBoardModel> _plan = null!;

    private FusionKanbanLocator Board => new(Page, "maintenance-board");
    private ILocator Status => Page.Locator("#board-status");
    private ILocator Saving => Page.Locator("#board-saving");
    private ILocator DragHint => Page.Locator("#drag-hint");
    private ILocator UrgentCount => Page.Locator("#urgent-count");
    private ILocator PartsNotice => Page.Locator("#parts-notice");
    private ILocator NewWorkOrder => Page.Locator("#new-work-order-btn");
    private ILocator FrontDeskCallsButton => Page.Locator("#front-desk-calls-btn");
    private ILocator SendWorkLists => Page.Locator("#send-work-lists-btn");
    private ILocator FileAwayFinishedWork => Page.Locator("#file-away-finished-work-btn");
    private ILocator PartArrived => Page.Locator("#part-arrived-btn");
    private ILocator ShowFinishedWork => _plan.NativeCheckBox(m => m.ShowFinishedWork).Input;

    private ILocator Cell(string column, string wing) => Board.Cell(column, wing);

    private ILocator DialogInput<TValue>(Expression<Func<WorkOrder, TValue>> member) =>
        Board.DialogField(Field.Of(member).Name);

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.ViewportSize = new() { Width = 1440, Height = 1400 };
        return options;
    }

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<MaintenanceBoardModel>.FromPage(Page);
        await Expect(Board.Card("Wheelchair brakes slipping")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task<Func<Task>> HoldTheAnswer(string url)
    {
        var released = new SemaphoreSlim(0);
        await Page.RouteAsync(url, async route =>
        {
            await released.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        return () => Page.RunAndWaitForResponseAsync(() =>
        {
            released.Release();
            return Task.CompletedTask;
        }, url);
    }

    private async Task OpenTheCardDialog(string workOrder)
    {
        await Board.Card(workOrder).DblClickAsync();
        await Expect(Board.Dialog).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task StartTheWheelchairRepair()
    {
        await Board.DragCard(Board.Card("Wheelchair brakes slipping"), Cell(WorkOrderStatus.InProgress, MaintenanceWing.Maple));
        await Expect(Status).ToHaveTextAsync(
            "The brake cable for Maple 118's wheelchair is back-ordered until Friday, so its work order waits for the part.",
            new() { Timeout = 10000 });
        await Expect(Cell(WorkOrderStatus.Blocked, MaintenanceWing.Maple)).ToContainTextAsync("Wheelchair brakes slipping", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_board_opens_with_each_wings_work_orders_in_their_columns_in_line()
    {
        await NavigateAndBoot();

        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Open, MaintenanceWing.Maple)))
            .ToContainTextAsync(new[] { "Wheelchair brakes slipping", "Call light not working" });
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Open, MaintenanceWing.Maple))).ToHaveCountAsync(2);
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Maple))).ToHaveCountAsync(0);
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Done, MaintenanceWing.Maple))).ToContainTextAsync(new[] { "Window sticks when opened" });
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)))
            .ToContainTextAsync(new[] { "Faucet drips in the kitchenette", "Door closer squeaks", "Shower drain backed up" });
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch))).ToHaveCountAsync(3);
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Birch))).ToContainTextAsync(new[] { "Replace bathroom grab bar" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_maintenance_board_opens_from_the_component_index()
    {
        await NavigateTo("/Sandbox/Components");

        await ClickWhenStable(Page.Locator("a[href='/Sandbox/Components/Kanban/Index']"));

        await Expect(Page).ToHaveURLAsync(new Regex("/Sandbox/Components/Kanban"), new() { Timeout = 10000 });
        await Expect(Board.Card("Wheelchair brakes slipping")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_board_counts_its_urgent_work_orders()
    {
        await NavigateAndBoot();

        await Expect(UrgentCount).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task work_finished_before_today_stays_off_the_board()
    {
        await NavigateAndBoot();

        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Done, MaintenanceWing.Birch))).ToHaveCountAsync(1);
        await Expect(Cell(WorkOrderStatus.Done, MaintenanceWing.Birch)).ToContainTextAsync("Hang new curtains");
        await Expect(Board.Card("Replace TV remote batteries")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_a_work_order_shows_its_details_and_history()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Board.Card("Wheelchair brakes slipping"));

        await Expect(Page.Locator("#work-order-title")).ToHaveTextAsync("Wheelchair brakes slipping", new() { Timeout = 5000 });
        await Expect(Page.Locator("#work-order-room")).ToHaveTextAsync("Maple 118");
        await Expect(Page.Locator("#work-order-technician")).ToHaveTextAsync("Dev Patel");
        await Expect(Page.Locator("#work-order-history"))
            .ToHaveTextAsync("Walter Finch's daughter reported it Monday; Dev Patel took it Monday afternoon.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_work_order_says_what_is_moving_while_it_is_dragged()
    {
        await NavigateAndBoot();

        await Board.PickUpAndCarry(Board.Card("Door closer squeaks"), Board.Card("Replace bathroom grab bar"), 0.75f);

        await Expect(DragHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(DragHint).ToHaveTextAsync("Moving Door closer squeaks: drop it in its new column.");
        await Board.LetGo();
        await Expect(DragHint).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_move_shows_on_the_board_once_the_work_order_system_has_it()
    {
        await NavigateAndBoot();
        var answer = await HoldTheAnswer(CommitUrl);

        await Board.DragCard(Board.Card("Door closer squeaks"), Board.Card("Replace bathroom grab bar"), 0.75f);

        await Expect(Saving).ToHaveTextAsync("Saving Door closer squeaks with the work-order system…", new() { Timeout = 5000 });
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).ToContainTextAsync("Door closer squeaks");
        await Expect(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Birch)).Not.ToContainTextAsync("Door closer squeaks");

        await answer();

        await Expect(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Birch)).ToContainTextAsync("Door closer squeaks", new() { Timeout = 10000 });
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).Not.ToContainTextAsync("Door closer squeaks");
        await Expect(Status).ToHaveTextAsync("Moved work order 108, Door closer squeaks, to In Progress.");
        await Expect(Saving).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_move_the_work_order_system_refuses_leaves_the_work_order_where_it_was()
    {
        await NavigateAndBoot();

        await Board.DragCard(Board.Card("Faucet drips in the kitchenette"), Board.Card("Replace bathroom grab bar"), 0.75f);

        await Expect(Status).ToHaveTextAsync(
            "The family in Birch 106 withdrew the faucet request this morning, so the work order stays where it was.",
            new() { Timeout = 10000 });
        await Expect(Board.Spinner).ToHaveClassAsync(NotSpinning);
        await Expect(Saving).ToBeHiddenAsync();
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).ToContainTextAsync("Faucet drips in the kitchenette");
        await Expect(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Birch)).Not.ToContainTextAsync("Faucet drips in the kitchenette");
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }

    [Test]
    public async Task unassigned_work_cannot_be_marked_done()
    {
        await NavigateAndBoot();

        await Board.DragCard(Board.Card("Door closer squeaks"), Board.Card("Hang new curtains"), 0.75f);

        await Expect(Status).ToHaveTextAsync("Assign a technician before marking the work done.", new() { Timeout = 5000 });
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).ToContainTextAsync("Door closer squeaks");
        await Expect(Cell(WorkOrderStatus.Done, MaintenanceWing.Birch)).Not.ToContainTextAsync("Door closer squeaks");
        await Expect(Saving).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task urgent_work_cannot_be_dropped_below_other_work_in_progress()
    {
        await NavigateAndBoot();

        await Board.DragCard(Board.Card("Shower drain backed up"), Board.Card("Replace bathroom grab bar"), 0.8f);

        await Expect(Status).ToHaveTextAsync("Urgent work goes to the top of In Progress.", new() { Timeout = 5000 });
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).ToContainTextAsync("Shower drain backed up");
        await Expect(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Birch)).Not.ToContainTextAsync("Shower drain backed up");
        await Expect(Saving).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finished_work_cannot_be_dragged_back()
    {
        await NavigateAndBoot();

        await Board.DragCard(Board.Card("Window sticks when opened"), Cell(WorkOrderStatus.InProgress, MaintenanceWing.Maple));

        await Expect(Status).ToHaveTextAsync(FinishedWorkIsClosed, new() { Timeout = 5000 });
        await Expect(Cell(WorkOrderStatus.Done, MaintenanceWing.Maple)).ToContainTextAsync("Window sticks when opened");
        await Expect(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Maple)).Not.ToContainTextAsync("Window sticks when opened");
        await Expect(DragHint).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_the_wheelchair_repair_before_its_part_arrives_parks_it_in_a_folded_waiting_on_parts_column()
    {
        await NavigateAndBoot();

        await StartTheWheelchairRepair();

        await Expect(Board.ColumnHeader("Waiting on parts")).ToHaveClassAsync(Folded, new() { Timeout = 5000 });
        await ClickWhenStable(Board.ColumnToggle("Waiting on parts"));
        await Expect(Board.ColumnHeader("Waiting on parts")).Not.ToHaveClassAsync(Folded, new() { Timeout = 5000 });
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Blocked, MaintenanceWing.Maple))).ToContainTextAsync(new[] { "Wheelchair brakes slipping" });
        await Expect(Board.Card("Wheelchair brakes slipping")).ToBeVisibleAsync();
        await Expect(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Maple)).Not.ToContainTextAsync("Wheelchair brakes slipping");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_parts_notice_counts_the_work_waiting_on_parts()
    {
        await NavigateAndBoot();

        await StartTheWheelchairRepair();

        await Expect(PartsNotice).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#parts-column")).ToHaveTextAsync("Waiting on parts");
        await Expect(Page.Locator("#parts-count")).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_the_part_arrives_the_wheelchair_repair_resumes_and_the_waiting_column_goes_away()
    {
        await NavigateAndBoot();
        await StartTheWheelchairRepair();
        await Expect(PartsNotice).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(PartArrived);

        await Expect(Status).ToHaveTextAsync("Moved work order 101, Wheelchair brakes slipping, to In Progress.", new() { Timeout = 10000 });
        await Expect(Cell(WorkOrderStatus.InProgress, MaintenanceWing.Maple)).ToContainTextAsync("Wheelchair brakes slipping");
        await Expect(Board.ColumnHeader("Waiting on parts")).ToHaveCountAsync(0);
        await Expect(PartsNotice).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_front_desks_urgent_call_goes_to_the_top_of_open_once_the_work_order_system_has_it()
    {
        await NavigateAndBoot();
        var answer = await HoldTheAnswer(CommitUrl);

        await ClickWhenStable(FrontDeskCallsButton);

        await Expect(Saving).ToHaveTextAsync("Saving Toilet running nonstop with the work-order system…", new() { Timeout = 5000 });
        await Expect(Board.Card("Toilet running nonstop")).ToHaveCountAsync(0);

        await answer();

        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).First)
            .ToContainTextAsync("Toilet running nonstop", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("Added work order 110, Toilet running nonstop, to Open.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_front_desk_has_no_more_calls_once_its_urgent_call_is_on_the_board()
    {
        await NavigateAndBoot();
        await ClickWhenStable(FrontDeskCallsButton);
        await Expect(Status).ToHaveTextAsync("Added work order 110, Toilet running nonstop, to Open.", new() { Timeout = 10000 });

        await ClickWhenStable(FrontDeskCallsButton);

        await Expect(Status).ToHaveTextAsync("No new calls from the front desk.", new() { Timeout = 5000 });
        await Expect(Board.Card("Toilet running nonstop")).ToHaveCountAsync(1);
        AssertNoConsoleErrorsExcept("the server responded with a status of 404");
    }

    [Test]
    public async Task a_new_work_order_described_in_the_card_dialog_lands_in_open()
    {
        await NavigateAndBoot();

        await ClickWhenStable(NewWorkOrder);

        await Expect(Board.Dialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(DialogInput(w => w.Number)).ToHaveValueAsync("110");
        await DialogInput(w => w.Room).FillAsync("Maple 220");
        await DialogInput(w => w.Summary).FillAsync("Door sticks at the threshold");
        await ClickWhenStable(Board.DialogButton("Save"));
        await Expect(Board.Dialog).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Maple)).ToContainTextAsync("Door sticks at the threshold", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("Added work order 110, Door sticks at the threshold, to Open.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task editing_a_work_orders_problem_in_the_card_dialog_saves_it()
    {
        await NavigateAndBoot();
        await OpenTheCardDialog("Door closer squeaks");

        await DialogInput(w => w.Summary).FillAsync("Door closer squeaks and slams shut");
        await ClickWhenStable(Board.DialogButton("Save"));

        await Expect(Status).ToHaveTextAsync("Saved work order 108: Door closer squeaks and slams shut.", new() { Timeout = 10000 });
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).ToContainTextAsync("Door closer squeaks and slams shut");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_the_card_dialog_leaves_the_work_order_as_it_was()
    {
        await NavigateAndBoot();
        await OpenTheCardDialog("Door closer squeaks");

        await DialogInput(w => w.Summary).FillAsync("Door closer replaced");
        await ClickWhenStable(Board.DialogButton("Cancel"));

        await Expect(Board.Dialog).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Board.Card("Door closer replaced")).ToHaveCountAsync(0);
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).ToContainTextAsync("Door closer squeaks");
        await Expect(Status).ToHaveTextAsync("The board is up to date.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_card_dialog_does_not_save_a_work_order_without_its_room()
    {
        await NavigateAndBoot();
        await OpenTheCardDialog("Door closer squeaks");

        await DialogInput(w => w.Room).FillAsync("");
        await ClickWhenStable(Board.DialogButton("Save"));

        await Expect(Status).ToHaveTextAsync("A work order keeps its room. Name the room before saving.", new() { Timeout = 5000 });
        await Expect(Board.Dialog).ToBeVisibleAsync();
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).ToContainTextAsync("Birch 104");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task work_orders_are_not_deleted_from_the_card_dialog()
    {
        await NavigateAndBoot();
        await OpenTheCardDialog("Door closer squeaks");

        await ClickWhenStable(Board.DialogButton("Delete"));

        await Expect(Status).ToHaveTextAsync("Work orders leave the board when finished work is filed away.", new() { Timeout = 5000 });
        await Expect(Board.Dialog).ToHaveCountAsync(0);
        await Expect(Cell(WorkOrderStatus.Open, MaintenanceWing.Birch)).ToContainTextAsync("Door closer squeaks");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_work_order_another_technician_is_updating_closes_its_dialog_and_says_who_has_it()
    {
        await NavigateAndBoot();

        await Board.Card("Call light not working").DblClickAsync();

        await Expect(Status).ToHaveTextAsync("Ana Ruiz is updating this work order; try again when she is done.", new() { Timeout = 10000 });
        await Expect(Board.Dialog).ToHaveCountAsync(0, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_finished_work_order_does_not_open_for_editing()
    {
        await NavigateAndBoot();

        await Board.Card("Window sticks when opened").DblClickAsync();

        await Expect(Status).ToHaveTextAsync(FinishedWorkIsClosed, new() { Timeout = 5000 });
        await Expect(Board.Dialog).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hiding_finished_work_takes_the_done_column_off_the_board()
    {
        await NavigateAndBoot();

        await ShowFinishedWork.UncheckAsync();

        await Expect(Board.ColumnHeader("Done")).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Board.Card("Hang new curtains")).ToHaveCountAsync(0);
        await Expect(Board.Card("Door closer squeaks")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showing_finished_work_again_brings_the_done_column_back()
    {
        await NavigateAndBoot();
        await ShowFinishedWork.UncheckAsync();
        await Expect(Board.ColumnHeader("Done")).ToHaveCountAsync(0, new() { Timeout = 5000 });

        await ShowFinishedWork.CheckAsync();

        await Expect(Board.ColumnHeader("Done")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Cell(WorkOrderStatus.Done, MaintenanceWing.Birch)).ToContainTextAsync("Hang new curtains");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filing_away_finished_work_clears_the_done_column()
    {
        await NavigateAndBoot();

        await ClickWhenStable(FileAwayFinishedWork);

        await Expect(Status).ToHaveTextAsync("Filed away 3 work orders of finished work.", new() { Timeout = 10000 });
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Done, MaintenanceWing.Maple))).ToHaveCountAsync(0);
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Done, MaintenanceWing.Birch))).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filing_away_with_no_finished_work_on_the_board_says_so()
    {
        await NavigateAndBoot();
        await ClickWhenStable(FileAwayFinishedWork);
        await Expect(Status).ToHaveTextAsync("Filed away 3 work orders of finished work.", new() { Timeout = 10000 });
        await Expect(Board.CardsIn(Cell(WorkOrderStatus.Done, MaintenanceWing.Birch))).ToHaveCountAsync(0, new() { Timeout = 10000 });

        await ClickWhenStable(FileAwayFinishedWork);

        await Expect(Status).ToHaveTextAsync("There is no finished work to file away.", new() { Timeout = 5000 });
        await Expect(Saving).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finished_work_the_resident_has_not_signed_off_is_not_filed_away()
    {
        await NavigateAndBoot();
        await Board.DragCard(Board.Card("Replace bathroom grab bar"), Board.Card("Hang new curtains"), 0.75f);
        await Expect(Status).ToHaveTextAsync("Moved work order 103, Replace bathroom grab bar, to Done.", new() { Timeout = 10000 });
        await Expect(Cell(WorkOrderStatus.Done, MaintenanceWing.Birch)).ToContainTextAsync("Replace bathroom grab bar", new() { Timeout = 10000 });

        await ClickWhenStable(FileAwayFinishedWork);

        await Expect(Status).ToHaveTextAsync("Finished work is filed away only after the resident signs off.", new() { Timeout = 5000 });
        await Expect(Cell(WorkOrderStatus.Done, MaintenanceWing.Birch)).ToContainTextAsync("Replace bathroom grab bar");
        await Expect(Cell(WorkOrderStatus.Done, MaintenanceWing.Maple)).ToContainTextAsync("Window sticks when opened");
        await Expect(Saving).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_the_work_lists_shows_the_board_waiting_until_they_are_sent()
    {
        await NavigateAndBoot();
        await Expect(Board.Spinner).ToHaveClassAsync(NotSpinning);
        var answer = await HoldTheAnswer(WorkListsUrl);

        await ClickWhenStable(SendWorkLists);

        await Expect(Board.Spinner).ToHaveClassAsync(Spinning, new() { Timeout = 5000 });

        await answer();

        await Expect(Status).ToHaveTextAsync(
            "Sent the night technician 1 work order in progress and the Maple wing's list of 3.", new() { Timeout = 10000 });
        await Expect(Board.Spinner).ToHaveClassAsync(NotSpinning);
        AssertNoConsoleErrors();
    }
}
