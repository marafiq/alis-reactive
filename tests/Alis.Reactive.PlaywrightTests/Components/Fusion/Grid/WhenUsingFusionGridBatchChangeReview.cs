using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the night charge nurse on Cedar wing, I want to correct the two-hourly safety checks the aides recorded, see
/// every change listed before it goes into the night log, reopen any change I got wrong straight from that list, and
/// then save them together, so that the night log holds only the checks I have looked at.
/// </summary>
/// <remarks>
/// Tonight's checks load as the aides recorded them: Amina Patel (room 301, 3 checks), Henry Liu (304, 2), Grace
/// Bennett (308, 3), Walter Grant (312, 1) and Edith Hale (315, 3). Review changes lists the changed residents under
/// Changes to save and keeps every change pending (the review cancels the grid's own save, batch-edit.js:339-342);
/// choosing one there reopens that resident's checks; Save to the night log sends the pending changes, the one still
/// being typed included, shows the checks as the log now holds them and empties Changes to save.
/// Not exercised:
/// The pending changes' AddedRecords and DeletedRecords: the checks add and remove no residents, so both arrive empty
/// (batch-edit.js:347-372).
/// RecordClick's RowIndex, CellIndex and Name: the review reopens a change by the resident's ID, which RowData carries.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridBatchChangeReview : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchChangeReview";
    private const string ChecksId = "night-checks-grid";
    private const string ReviewId = "night-checks-review";

    // Both grids number their columns from 1.
    private const int ResidentColumn = 2;
    private const int ChecksColumn = 4;

    private static readonly Regex Changed = new("e-updatedtd");

    private ILocator Rows => Page.Locator($"#{ChecksId} .e-row");
    private ILocator Checks(string resident) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{ChecksColumn}']");
    private ILocator ChecksEditor => Page.Locator($"#{ChecksId}checksDone");
    private ILocator ChangesToSave => Page.Locator($"#{ReviewId} .e-row");
    private ILocator ChangeToSave(string resident) => ChangesToSave.Filter(new() { HasText = resident });
    private ILocator ReviewChanges => Page.Locator("#night-checks-review-changes");
    private ILocator SaveToTheNightLog => Page.Locator("#night-checks-save");
    private ILocator Status => Page.Locator("#night-checks-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    // Opens a resident's checks and types over them, leaving the cell open. The grid shows the editor holding the
    // count a moment after the double-click; a fill before then is lost.
    private async Task TypeChecks(string resident, string current, string checks)
    {
        await Checks(resident).DblClickAsync();
        await Expect(ChecksEditor).ToHaveValueAsync(current, new() { Timeout = 10000 });
        await ChecksEditor.FillAsync(checks);
    }

    // Henry Liu's checks become 4 and Grace Bennett's 2, and the nurse reviews them.
    private async Task CorrectHenryAndGraceThenReview()
    {
        await TypeChecks("Henry Liu", "2", "4");
        await TypeChecks("Grace Bennett", "3", "2");
        await ClickWhenStable(ReviewChanges);
        await Expect(ChangesToSave).ToHaveCountAsync(2, new() { Timeout = 10000 });
    }

    // Chooses a resident in Changes to save and clicks into the checks it reopens. The focus stays in the review
    // list after the choice, so the box shows the count as 2.00 until the nurse clicks into it.
    private async Task ReopenFromTheReview(string resident, string pending)
    {
        await ClickWhenStable(ChangeToSave(resident).Locator($"td[aria-colindex='{ResidentColumn}']"));
        var reopened = Checks(resident).Locator($"#{ChecksId}checksDone");
        await Expect(reopened).ToBeVisibleAsync(new() { Timeout = 10000 });
        await reopened.ClickAsync();
        await Expect(reopened).ToHaveValueAsync(pending, new() { Timeout = 10000 });
    }

    [Test]
    public async Task tonights_checks_open_as_the_aides_recorded_them()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(Checks("Henry Liu")).ToHaveTextAsync("2");
        await Expect(Checks("Walter Grant")).ToHaveTextAsync("1");
        await Expect(ChangesToSave).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_lists_each_changed_resident_with_the_new_checks()
    {
        await NavigateAndBoot();
        await TypeChecks("Henry Liu", "2", "4");
        await TypeChecks("Grace Bennett", "3", "2");

        await ClickWhenStable(ReviewChanges);

        await Expect(ChangesToSave).ToHaveCountAsync(2, new() { Timeout = 10000 });
        await Expect(ChangeToSave("Henry Liu").Locator($"td[aria-colindex='{ChecksColumn}']")).ToHaveTextAsync("4");
        await Expect(ChangeToSave("Grace Bennett").Locator($"td[aria-colindex='{ChecksColumn}']")).ToHaveTextAsync("2");
        await Expect(Status).ToHaveTextAsync("Check the changes below, then save them to the night log.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_keeps_every_change_pending_until_it_is_saved()
    {
        await NavigateAndBoot();

        await CorrectHenryAndGraceThenReview();

        await Expect(Checks("Henry Liu")).ToHaveTextAsync("4");
        await Expect(Checks("Henry Liu")).ToHaveClassAsync(Changed);
        await Expect(Checks("Grace Bennett")).ToHaveClassAsync(Changed);
        await Expect(Checks("Amina Patel")).Not.ToHaveClassAsync(Changed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_change_in_the_review_reopens_that_residents_checks()
    {
        await NavigateAndBoot();
        await CorrectHenryAndGraceThenReview();

        await ClickWhenStable(ChangeToSave("Grace Bennett").Locator($"td[aria-colindex='{ResidentColumn}']"));

        var reopened = Checks("Grace Bennett").Locator($"#{ChecksId}checksDone");
        await Expect(reopened).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(ChecksEditor).ToHaveCountAsync(1);
        await reopened.ClickAsync();
        await Expect(reopened).ToHaveValueAsync("2", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_puts_the_reviewed_changes_into_the_night_log()
    {
        await NavigateAndBoot();
        await CorrectHenryAndGraceThenReview();

        await ClickWhenStable(SaveToTheNightLog);

        await Expect(Status).ToHaveTextAsync(
            "Saved to the night log: Henry Liu, 4 checks; Grace Bennett, 2 checks.", new() { Timeout = 10000 });
        await Expect(ChangesToSave).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{ChecksId} .e-updatedtd")).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Checks("Henry Liu")).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_correction_still_being_typed_is_saved_with_the_rest()
    {
        await NavigateAndBoot();
        await CorrectHenryAndGraceThenReview();
        await ReopenFromTheReview("Grace Bennett", "2");
        await ChecksEditor.FillAsync("1");

        await ClickWhenStable(SaveToTheNightLog);

        await Expect(Status).ToHaveTextAsync(
            "Saved to the night log: Henry Liu, 4 checks; Grace Bennett, 1 check.", new() { Timeout = 10000 });
        await Expect(Checks("Grace Bennett")).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_with_nothing_changed_says_there_is_nothing_to_save()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ReviewChanges);

        await Expect(Status).ToHaveTextAsync(
            "No checks changed tonight; there is nothing to save.", new() { Timeout = 10000 });
        await Expect(ChangesToSave).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }
}
