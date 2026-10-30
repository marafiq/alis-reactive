namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the evening charge nurse on Maple wing, I want to update each resident's open tasks and PRN doses at shift
/// change in one tally, have a count no shift could carry refused as I leave the cell, and have the tally wait
/// for the night supervisor when it leaves a resident with ten or more open tasks, so that the night nurse
/// starts from an accurate tally that a supervisor has seen when it is heavy.
/// </summary>
/// <remarks>
/// The tally starts from Amina Patel (2 open tasks, 0 PRN doses), Grace Bennett (3, 1), Henry Liu (6, 0) and
/// Irene Morgan (4, 2). Start tally opens Amina's open tasks; Review tally lists the pending changes, the cell
/// still open included; Send to the night shift sends them once nothing holds the tally. The eMAR sync brings
/// in the open tasks the evening medication pass left Grace with.
/// Not exercised:
/// CloseEdit, which would discard the tally: the page offers no discard; the Maple wing roster
/// (WhenUsingFusionGridEditing) proves CloseEdit.
/// ActionComplete after a sent tally: the roster pages read ActionComplete; this page reads what the server sent.
/// CellSave's and BeforeBatchSave's Cancel as a value read: the page writes it, and EJ2 reads it back
/// (batch-edit.js:340, 991).
/// The pending changes' AddedRecords and DeletedRecords: the tally adds and removes no residents, so both
/// arrive empty (batch-edit.js:347-372).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridShiftTaskTally : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ShiftTaskTally";
    private const string TallyId = "shift-tally-grid";

    // The tally numbers its columns from 1.
    private const int OpenTasksColumn = 4;
    private const int PrnDosesColumn = 5;

    private ILocator Rows => Page.Locator($"#{TallyId} .e-row");
    private ILocator Cell(string resident, int column) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{column}']");
    private ILocator Editor(string field) => Page.Locator($"#{TallyId}{field}");
    private ILocator StartTally => Page.Locator("#tally-start");
    private ILocator SyncFromEmar => Page.Locator("#tally-emar-sync");
    private ILocator ReviewTally => Page.Locator("#tally-review");
    private ILocator SendToNightShift => Page.Locator("#tally-send");
    private ILocator Note => Page.Locator("#tally-note");
    private ILocator Refused => Page.Locator("#tally-refused");
    private ILocator Held => Page.Locator("#tally-held");
    private ILocator EmarStatus => Page.Locator("#tally-emar");
    private ILocator Review => Page.Locator("#tally-review-summary");
    private ILocator Sent => Page.Locator("#tally-sent");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(4, new() { Timeout = 10000 });
    }

    // Starts the tally and writes over Amina Patel's open tasks, leaving her cell open.
    private async Task CountAminasOpenTasks(string count)
    {
        await ClickWhenStable(StartTally);
        await Expect(Editor("openTasks")).ToHaveValueAsync("2", new() { Timeout = 10000 });
        await Editor("openTasks").FillAsync(count);
    }

    [Test]
    public async Task starting_the_tally_opens_the_first_residents_open_tasks()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StartTally);

        await Expect(Cell("Amina Patel", OpenTasksColumn).Locator($"#{TallyId}openTasks"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Editor("openTasks")).ToHaveValueAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_count_still_in_its_cell_is_in_the_review()
    {
        await NavigateAndBoot();
        await CountAminasOpenTasks("4");

        await ClickWhenStable(ReviewTally);

        await Expect(Review).ToHaveTextAsync(
            "Changed for the night shift: Amina Patel, 4 open tasks and 0 PRN doses.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_saved_count_is_noted_with_what_it_was()
    {
        await NavigateAndBoot();
        await CountAminasOpenTasks("4");

        await ClickWhenStable(ReviewTally);

        await Expect(Note).ToHaveTextAsync("Last change: Amina Patel's open tasks from 2 to 4.", new() { Timeout = 10000 });
        await Expect(Cell("Amina Patel", OpenTasksColumn)).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_prn_dose_count_is_noted_as_prn_doses()
    {
        await NavigateAndBoot();
        await Cell("Amina Patel", PrnDosesColumn).DblClickAsync();
        await Expect(Editor("prnDoses")).ToHaveValueAsync("0", new() { Timeout = 10000 });
        await Editor("prnDoses").FillAsync("1");

        await ClickWhenStable(ReviewTally);

        await Expect(Note).ToHaveTextAsync("Last change: Amina Patel's PRN doses from 0 to 1.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task more_than_twelve_open_tasks_is_refused_and_the_tally_keeps_the_count()
    {
        await NavigateAndBoot();
        await CountAminasOpenTasks("15");

        await ClickWhenStable(ReviewTally);

        await Expect(Refused).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Refused).ToHaveTextAsync("Amina Patel cannot carry 15 open tasks into one shift; the tally keeps 2.");
        await Expect(Review).ToHaveTextAsync("No changes in the tally yet.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_emar_sync_brings_a_residents_med_pass_tasks_into_the_tally()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SyncFromEmar);

        await Expect(Cell("Grace Bennett", OpenTasksColumn)).ToHaveTextAsync("5", new() { Timeout = 10000 });
        await Expect(EmarStatus).ToHaveTextAsync("eMAR: Grace Bennett's evening med pass leaves 5 open tasks.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_the_tally_hands_the_changes_to_the_night_shift()
    {
        await NavigateAndBoot();
        await CountAminasOpenTasks("4");

        await ClickWhenStable(SendToNightShift);

        await Expect(Sent).ToHaveTextAsync(
            "Sent to the night shift: Amina Patel, 4 open tasks and 0 PRN doses.", new() { Timeout = 10000 });
        await Expect(Held).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The tally stays pending: a review after the hold still lists the count.
    [Test]
    public async Task a_tally_leaving_a_resident_ten_or_more_tasks_waits_for_the_night_supervisor()
    {
        await NavigateAndBoot();
        await CountAminasOpenTasks("11");

        await ClickWhenStable(SendToNightShift);

        await Expect(Held).ToHaveTextAsync(
            "Call the night supervisor before sending: Amina Patel has 11 open tasks.", new() { Timeout = 10000 });
        await Expect(Held).ToBeVisibleAsync();
        await Expect(Sent).ToHaveTextAsync("Not sent to the night shift yet.");
        await ClickWhenStable(ReviewTally);
        await Expect(Review).ToHaveTextAsync(
            "Changed for the night shift: Amina Patel, 11 open tasks and 0 PRN doses.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
