using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the resident services coordinator, I want to correct, admit and move out residents on the Maple wing
/// roster, and have the roster refuse a move into memory care that only the transfer team books, so that the
/// roster the nurses work from matches who lives on the wing.
/// </summary>
/// <remarks>
/// The roster is Amina Patel, Grace Bennett, Henry Liu, Irene Morgan (already approved for memory care and
/// moving to Willow wing), Jonah Reed and Katherine Ortiz. Henry and Irene are due for review today; choosing
/// one opens their row for editing. The handoff correction applies a resident's risk and open tasks from the
/// night nurse's note to their row.
/// Not exercised:
/// Refresh: UpdateRow redraws the roster itself (normal-edit.js:269-280), so a Refresh after it would break
/// unseen; WhenUsingFusionGridBilling's refresh test fails when Refresh breaks.
/// ActionBegin's and ActionComplete's Name and Type: EJ2 sets both to the event's own name (observer.js:92;
/// normal-edit.js:299, 421), the same on every save, so they carry nothing a page could show; the page tells a
/// save from a cancel by RequestType.
/// SelectedRow: an inline save reports -1, as the roster clears its selection when a row opens for editing
/// (normal-edit.js:227-229, 300), and an added row reports 0 (normal-edit.js:651).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridEditing : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Editing";
    private const string RosterId = "maple-roster";

    // The roster numbers its columns from 1.
    private const int ResidentColumn = 2;
    private const int CareLevelColumn = 3;
    private const int SuiteColumn = 4;
    private const int RiskColumn = 5;
    private const int OpenTasksColumn = 6;

    private PagePlan<ResidentGridEditingModel> _plan = null!;

    private ILocator Roster => Page.Locator($"#{RosterId}");
    private ILocator Rows => Page.Locator($"#{RosterId} .e-row");
    private ILocator Cell(string resident, int column) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{column}']");
    private ILocator Editor(string field) => Page.Locator($"#{RosterId}{field}");
    private ILocator RowBeingEdited => Page.Locator($"#{RosterId} .e-editedrow");
    private ILocator Status => Page.Locator("#roster-status");
    private ILocator TransferWarning => Page.Locator("#transfer-warning");
    private ILocator ReviewHenry => Page.Locator("#review-henry-liu");
    private ILocator ReviewIrene => Page.Locator("#review-irene-morgan");
    private ILocator Edit => Page.Locator("#roster-edit");
    private ILocator Save => Page.Locator("#roster-save");
    private ILocator Cancel => Page.Locator("#roster-cancel");
    private ILocator MoveOut => Page.Locator("#roster-move-out");
    private ILocator AddRespiteStay => Page.Locator("#roster-add-respite");
    private ILocator AdmitFromWaitingList => Page.Locator("#roster-admit");
    private ILocator ApplyCorrection => Page.Locator("#apply-correction");
    private ILocator CorrectionStatus => Page.Locator("#correction-status");

    private FusionTextBoxLocator CorrectedResident => new(Page, _plan.FindComponent(m => m.ResidentName)!.ElementId);
    private DropDownListLocator CorrectedRisk => _plan.DropDownList(m => m.RiskLevel);
    private NumericTextBoxLocator CorrectedOpenTasks => _plan.NumericTextBox(m => m.OpenTasks);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<ResidentGridEditingModel>.FromPage(Page);
        await Expect(Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
    }

    // Opens Henry Liu's row from today's review list.
    private async Task ReviewHenrysRow()
    {
        await ClickWhenStable(ReviewHenry);
        await Expect(Editor("residentName")).ToHaveValueAsync("Henry Liu", new() { Timeout = 10000 });
    }

    private async Task FillCorrection(string resident, string risk, string openTasks)
    {
        await CorrectedResident.FillAndBlur(resident);
        await CorrectedRisk.Select(risk);
        await CorrectedOpenTasks.FillAndBlur(openTasks);
    }

    [Test]
    public async Task the_roster_lists_the_wings_residents_when_it_opens()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(Cell("Irene Morgan", CareLevelColumn)).ToHaveTextAsync("Memory Care");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_resident_due_for_review_opens_their_row_for_editing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ReviewHenry);

        await Expect(Editor("residentName")).ToHaveValueAsync("Henry Liu", new() { Timeout = 10000 });
        await Expect(Editor("suite")).ToHaveValueAsync("108");
        await Expect(Status).ToHaveTextAsync("Editing Henry Liu.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_corrected_name_is_saved_to_the_roster()
    {
        await NavigateAndBoot();
        await Cell("Grace Bennett", ResidentColumn).ClickAsync();
        await ClickWhenStable(Edit);
        await Expect(Editor("residentName")).ToHaveValueAsync("Grace Bennett", new() { Timeout = 10000 });
        await Editor("residentName").FillAsync("Grace Bennett-Holt");

        await ClickWhenStable(Save);

        await Expect(Cell("Grace Bennett-Holt", ResidentColumn)).ToHaveTextAsync("Grace Bennett-Holt", new() { Timeout = 10000 });
        await Expect(RowBeingEdited).ToHaveCountAsync(0);
        await Expect(Status).ToHaveTextAsync("Saved Grace Bennett-Holt.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_an_edit_keeps_the_residents_details()
    {
        await NavigateAndBoot();
        await ReviewHenrysRow();
        await Editor("residentName").FillAsync("Henry Lu");

        await ClickWhenStable(Cancel);

        await Expect(RowBeingEdited).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", ResidentColumn)).ToHaveTextAsync("Henry Liu");
        await Expect(Status).ToHaveTextAsync("Discarded the changes to Henry Liu.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_move_into_memory_care_is_refused_as_a_roster_edit()
    {
        await NavigateAndBoot();
        await ReviewHenrysRow();
        await Expect(Editor("careLevel")).ToHaveValueAsync("Assisted Living");
        await Editor("careLevel").FillAsync("Memory Care");

        await ClickWhenStable(Save);

        await Expect(TransferWarning).ToHaveTextAsync(
            "Henry Liu moves to memory care through a transfer to Willow wing, not a roster edit.", new() { Timeout = 10000 });
        await Expect(TransferWarning).ToBeVisibleAsync();
        await Expect(RowBeingEdited).ToHaveCountAsync(1);
        await Expect(Status).ToHaveTextAsync("Editing Henry Liu.");
        AssertNoConsoleErrors();
    }

    // Irene's move to memory care is already booked, so correcting her row is no move into memory care.
    [Test]
    public async Task a_resident_already_in_memory_care_can_still_be_corrected()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ReviewIrene);
        await Expect(Editor("suite")).ToHaveValueAsync("112", new() { Timeout = 10000 });
        await Editor("suite").FillAsync("130");

        await ClickWhenStable(Save);

        await Expect(Cell("Irene Morgan", SuiteColumn)).ToHaveTextAsync("130", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("Saved Irene Morgan.");
        await Expect(TransferWarning).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task this_weeks_respite_guest_is_added_at_the_top_of_the_roster()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AddRespiteStay);

        await Expect(Rows).ToHaveCountAsync(7, new() { Timeout = 10000 });
        await Expect(Rows.First).ToContainTextAsync("Walter Kim");
        await Expect(Cell("Walter Kim", CareLevelColumn)).ToHaveTextAsync("Respite");
        await Expect(Status).ToHaveTextAsync("Added Walter Kim to the roster.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admitting_from_the_waiting_list_adds_the_next_resident_at_the_top()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AdmitFromWaitingList);

        await Expect(Rows).ToHaveCountAsync(7, new() { Timeout = 10000 });
        await Expect(Rows.First).ToContainTextAsync("Rosa Diaz");
        await Expect(Cell("Rosa Diaz", SuiteColumn)).ToHaveTextAsync("122");
        await Expect(Status).ToHaveTextAsync("Added Rosa Diaz to the roster.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_out_the_selected_resident_takes_them_off_the_roster()
    {
        await NavigateAndBoot();
        await Cell("Grace Bennett", ResidentColumn).ClickAsync();

        await ClickWhenStable(MoveOut);

        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
        await Expect(Roster).Not.ToContainTextAsync("Grace Bennett");
        await Expect(Status).ToHaveTextAsync("Moved out Grace Bennett.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_correction_without_resident_risk_or_tasks_says_what_is_missing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ApplyCorrection);

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Name the resident to correct.", new() { Timeout = 10000 });
        await Expect(_plan.ErrorFor(m => m.RiskLevel)).ToHaveTextAsync("Choose the resident's risk level.");
        await Expect(_plan.ErrorFor(m => m.OpenTasks)).ToHaveTextAsync("Enter the open tasks from the handoff note.");
        await Expect(CorrectionStatus).ToHaveTextAsync("No corrections applied yet.");
        AssertNoConsoleErrors();
    }

    // Once a correction has been tried, a field checks itself again as the coordinator leaves it.
    [Test]
    public async Task a_name_too_short_to_find_a_resident_is_refused_as_the_coordinator_leaves_the_field()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ApplyCorrection);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Name the resident to correct.", new() { Timeout = 10000 });

        await CorrectedResident.FillAndBlur("Li");

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Write the resident's full name.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task more_open_tasks_than_the_night_shift_carries_are_refused()
    {
        await NavigateAndBoot();
        await FillCorrection("Henry Liu", "High", "9");

        await ClickWhenStable(ApplyCorrection);

        await Expect(_plan.ErrorFor(m => m.OpenTasks)).ToHaveTextAsync(
            "The night shift carries 0 to 7 open tasks per resident.", new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", OpenTasksColumn)).ToHaveTextAsync("6");
        AssertNoConsoleErrors();
    }

    // Rosa Diaz is on the waiting list, not on the wing; only the server knows who lives here.
    [Test]
    public async Task a_correction_for_someone_not_on_the_wing_is_refused_by_the_server()
    {
        await NavigateAndBoot();
        await FillCorrection("Rosa Diaz", "High", "4");

        await ClickWhenStable(ApplyCorrection);

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync(
            "No resident named Rosa Diaz lives on Maple wing.", new() { Timeout = 10000 });
        await Expect(CorrectionStatus).ToHaveTextAsync("No corrections applied yet.");
        await Expect(Rows).ToHaveCountAsync(6);
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task a_handoff_correction_updates_the_residents_row()
    {
        await NavigateAndBoot();
        await FillCorrection("Henry Liu", "High", "4");

        await ClickWhenStable(ApplyCorrection);

        await Expect(CorrectionStatus).ToHaveTextAsync(
            "Henry Liu's row now shows High risk and 4 open tasks.", new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", RiskColumn)).ToHaveTextAsync("High", new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", OpenTasksColumn)).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }
}
