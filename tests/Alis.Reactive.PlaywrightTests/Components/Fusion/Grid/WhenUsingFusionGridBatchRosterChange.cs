using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the Maple wing admissions coordinator, I want to record a suite's move-out and move-in on the roster as one
/// turnover and have the roster keep it unsaved until housekeeping has turned the suite over, so that the roster never
/// shows a resident moved into a suite that is not ready for them.
/// </summary>
/// <remarks>
/// The roster loads from the record: Amina Patel (suite 201), Henry Liu (203), Grace Bennett (204), Ruth Carter
/// (205) and Jonah Reed (207). Ruth moves out of suite 205 this morning and Walter Kim moves in this afternoon.
/// Recording the move-out takes Ruth's row off the roster; recording the move-in puts Walter at the top, marked as
/// new. Saving names who moves out and who moves in, holds the turnover while housekeeping has not signed off, and
/// otherwise sends it to the server, which says what it recorded.
/// Not exercised:
/// The pending changes' ChangedRecords: a turnover changes no resident's row, so it arrives empty (batch-edit.js:367-368).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridBatchRosterChange : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchRosterChange";
    private const string RosterId = "turnover-roster";

    // The roster numbers its columns from 1.
    private const int SuiteColumn = 3;

    private static readonly Regex Changed = new("e-updatedtd");

    private PagePlan<SuiteTurnoverViewModel> _plan = null!;

    private ILocator Rows => Page.Locator($"#{RosterId} .e-row");
    private ILocator Row(string resident) => Rows.Filter(new() { HasText = resident });
    private ILocator RecordTheMoveOut => Page.Locator("#turnover-record-move-out");
    private ILocator RecordTheMoveIn => Page.Locator("#turnover-record-move-in");
    private ILocator SaveTheTurnover => Page.Locator("#turnover-save");
    private ILocator MovingOut => Page.Locator("#turnover-moving-out");
    private ILocator MovingIn => Page.Locator("#turnover-moving-in");
    private ILocator Held => Page.Locator("#turnover-held");
    private ILocator Status => Page.Locator("#turnover-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<SuiteTurnoverViewModel>.FromPage(Page);
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    private async Task RecordBothMoves()
    {
        await ClickWhenStable(RecordTheMoveOut);
        await Expect(Row("Ruth Carter")).ToBeHiddenAsync(new() { Timeout = 10000 });
        await ClickWhenStable(RecordTheMoveIn);
        await Expect(Rows.First).ToContainTextAsync("Walter Kim", new() { Timeout = 10000 });
    }

    private async Task HousekeepingSignsOff() => await _plan.NativeCheckBox(m => m.HousekeepingSignedOff).Check();

    [Test]
    public async Task the_roster_opens_with_maple_wings_residents_on_record()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(Row("Ruth Carter").Locator($"td[aria-colindex='{SuiteColumn}']")).ToHaveTextAsync("205");
        await Expect(Rows.Last).ToContainTextAsync("Jonah Reed");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_the_move_out_takes_ruth_off_the_roster()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RecordTheMoveOut);

        await Expect(Row("Ruth Carter")).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Row("Grace Bennett")).ToBeVisibleAsync();
        await Expect(Row("Jonah Reed")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_the_move_in_puts_walter_at_the_top_of_the_roster_marked_new()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RecordTheMoveIn);

        await Expect(Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
        await Expect(Rows.First).ToContainTextAsync("Walter Kim");
        await Expect(Rows.First.Locator($"td[aria-colindex='{SuiteColumn}']")).ToHaveTextAsync("205");
        await Expect(Rows.First.Locator($"td[aria-colindex='{SuiteColumn}']")).ToHaveClassAsync(Changed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_before_housekeeping_signs_off_names_the_moves_and_holds_the_turnover()
    {
        await NavigateAndBoot();
        await RecordBothMoves();

        await ClickWhenStable(SaveTheTurnover);

        await Expect(Held).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(MovingOut).ToHaveTextAsync("Moving out: Ruth Carter");
        await Expect(MovingIn).ToHaveTextAsync("Moving in: Walter Kim");
        await Expect(Status).ToHaveTextAsync("The turnover has not been saved.");
        await Expect(Rows.First.Locator($"td[aria-colindex='{SuiteColumn}']")).ToHaveClassAsync(Changed);
        AssertNoConsoleErrors();
    }

    // The held turnover stayed pending: signing off and saving again sends both moves.
    [Test]
    public async Task a_held_turnover_is_saved_in_full_once_housekeeping_signs_off()
    {
        await NavigateAndBoot();
        await RecordBothMoves();
        await ClickWhenStable(SaveTheTurnover);
        await Expect(Held).ToBeVisibleAsync(new() { Timeout = 10000 });

        await HousekeepingSignsOff();
        await ClickWhenStable(SaveTheTurnover);

        await Expect(Status).ToHaveTextAsync(
            "Turnover saved: Ruth Carter moved out of suite 205 and Walter Kim moved into suite 205.", new() { Timeout = 10000 });
        await Expect(Held).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_housekeeping_has_signed_off_saving_records_the_turnover_on_the_roster()
    {
        await NavigateAndBoot();
        await HousekeepingSignsOff();
        await RecordBothMoves();

        await ClickWhenStable(SaveTheTurnover);

        await Expect(Status).ToHaveTextAsync(
            "Turnover saved: Ruth Carter moved out of suite 205 and Walter Kim moved into suite 205.", new() { Timeout = 10000 });
        await Expect(Held).ToBeHiddenAsync();
        await Expect(Page.Locator($"#{RosterId} .e-updatedtd")).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Row("Walter Kim")).ToBeVisibleAsync();
        await Expect(Row("Ruth Carter")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    // A move-out alone names no one moving in.
    [Test]
    public async Task a_move_out_alone_is_saved_without_a_move_in()
    {
        await NavigateAndBoot();
        await HousekeepingSignsOff();
        await ClickWhenStable(RecordTheMoveOut);
        await Expect(Row("Ruth Carter")).ToBeHiddenAsync(new() { Timeout = 10000 });

        await ClickWhenStable(SaveTheTurnover);

        await Expect(Status).ToHaveTextAsync("Turnover saved: Ruth Carter moved out of suite 205.", new() { Timeout = 10000 });
        await Expect(MovingOut).ToHaveTextAsync("Moving out: Ruth Carter");
        await Expect(MovingIn).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_with_no_move_recorded_says_there_is_nothing_to_save()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SaveTheTurnover);

        await Expect(Status).ToHaveTextAsync("No move is recorded yet, so there is nothing to save.", new() { Timeout = 10000 });
        await Expect(Held).ToBeHiddenAsync();
        await Expect(MovingOut).ToBeHiddenAsync();
        await Expect(MovingIn).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
