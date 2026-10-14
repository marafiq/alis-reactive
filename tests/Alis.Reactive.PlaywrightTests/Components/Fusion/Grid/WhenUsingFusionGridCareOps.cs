using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the director of nursing, I want to narrow the care census by risk or care level, correct a resident's
/// care in the grid, reassign, flag or discharge the residents I tick, admit a resident and work the high-risk
/// watch list, so that every resident's risk, nurse and open care tasks are right on the board the care team
/// works from.
/// </summary>
/// <remarks>
/// The census holds 24 residents, ten to a page by name: Abigail Moore (Low risk, Nora Ellis, 1 open task)
/// first and Esther Cole last on page one, 62 open care tasks in all. Four residents are at critical risk,
/// seven in Memory Care, and five at high risk make the watch list, Amina Patel (Malik Stone, 6 open tasks)
/// first. Beatrice Lane's review, October 15, is the soonest; eight residents are at low risk, Esther Cole among
/// them. The chips choose one risk and one care level. Every request that answers with the board's rows reads the
/// text of the chip each list has chosen: a chip, a sort, a page, a watch-list refresh, and each save,
/// reassignment, flag, discharge and admission. A chip clicked again lets its choice go. The server answers every
/// change with the first page of the residents the chips chose and a sentence that says what it did; a flag or
/// discharge from the watch list reloads both the list and the census. The census lives in the browser's session,
/// so every test starts from the census as filed.
/// Not exercised:
/// DataStateChange's Name, RequiresCounts, Group, Where, Search and Action: the board pages and sorts on the
/// server; WhenUsingFusionGridCensus and WhenUsingFusionGridDirectory read the rest.
/// The chip lists' SelectedChipIndex and SelectedChipValue, and the selected chip's Index and Data: the board asks
/// for its residents by the chosen chip's text. The chips' click payload: the board reads the chips themselves;
/// WhenUsingFusionChipList reads its Text and Selected.
/// The care level and risk cell editors and the review day's date editor: the nurse's editor proves the
/// typed select editor here; WhenUsingFusionGridResidentEditForm and WhenUsingFusionGridInlineEdit prove a
/// select and a date editor saved.
/// The page renders the discharge link's own plan before its own, so the admit form's fields are found by the
/// ids their model members give them, not through PagePlan.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridCareOps : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/CareOps";
    private const string GridId = "careops-grid";
    private const string WholeCensus = "24 residents on the census: 62 open care tasks.";
    private const string CriticalResidents = "4 residents at Critical risk: 18 open care tasks.";
    private const string MemoryCareResidents = "7 Memory Care residents: 26 open care tasks.";
    private const string HighRiskResidents = "5 residents at High risk: 23 open care tasks.";
    private const string LowRiskResidents = "8 residents at Low risk: 6 open care tasks.";
    private const string SkilledNursingResidents = "6 Skilled Nursing residents: 24 open care tasks.";

    // The grid numbers its columns from 1, the tick box first.
    private const int RiskColumn = 6;
    private const int NurseColumn = 7;
    private const int OpenTasksColumn = 8;

    private FusionGridLocator Board => new(Page, GridId);
    private ILocator Summary => Page.Locator("#careops-summary");
    // The tick box the director sees; the grid hides the checkbox input itself behind it.
    private ILocator Tick(string resident) => Board.Row(resident).Locator(".e-checkbox-wrapper .e-frame");
    private ILocator NurseEditor => Page.Locator($"#{GridId} select[name='primaryNurse']");
    private ILocator OpenTasksEditor => Page.Locator($"#{GridId}openTasks");
    private ILocator SaveCareChanges => Page.Locator("#careops-save-all");
    private ILocator ReassignTicked => Page.Locator("#careops-reassign");
    private ILocator FlagTickedCritical => Page.Locator("#careops-flag-risk");
    private ILocator DischargeTicked => Page.Locator("#careops-discharge-selected");
    private ILocator WatchList => Page.Locator("#careops-action-rows li");
    private ILocator Watched(string resident) => WatchList.Filter(new() { HasText = resident });
    private ILocator QuickAction(string resident, string action) => Watched(resident).Locator("a").Filter(new() { HasText = action });
    private ILocator ConfirmDialog => Page.Locator("#alisConfirmDialog");
    private ILocator ConfirmOk => ConfirmDialog.Locator("button.e-primary");
    private ILocator ConfirmCancel => ConfirmDialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" });
    private ILocator AdmitResident => Page.Locator("#careops-admit-open");
    private ILocator Admit => Page.Locator("#admit-save");
    private ILocator CancelAdmission => Page.Locator("#admit-cancel");
    private ILocator AdmitDialog => Page.Locator("#admit-dialog");
    private FusionChipListLocator RiskChips => new(Page, "careops-risk-chips");
    private FusionChipListLocator CareChips => new(Page, "careops-care-chips");

    private FusionTextBoxLocator NewResidentName => new(Page, IdGenerator.For<CareOpsViewModel>(m => m.NewResidentName));
    private DropDownListLocator NewWing => new(Page, IdGenerator.For<CareOpsViewModel>(m => m.NewWing));
    private DropDownListLocator NewCareLevel => new(Page, IdGenerator.For<CareOpsViewModel>(m => m.NewCareLevel));
    private DropDownListLocator NewRiskLevel => new(Page, IdGenerator.For<CareOpsViewModel>(m => m.NewRiskLevel));
    private NumericTextBoxLocator NewOpenTasks => new(Page, IdGenerator.For<CareOpsViewModel>(m => m.NewOpenTasks));
    private ILocator ErrorFor(System.Linq.Expressions.Expression<Func<CareOpsViewModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}_error");

    // Every discharge the page asks the server for, from the board or the watch list.
    private ConcurrentQueue<string> _dischargesSent = null!;

    private async Task NavigateAndBoot()
    {
        _dischargesSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Components/Grid/CareOpsDischarge")) _dischargesSent.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        await Expect(Summary).ToHaveTextAsync(WholeCensus, new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(10, new() { Timeout = 10000 });
        await Expect(WatchList).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    // Opens a resident's open-tasks cell and types over the count. The grid focuses its numeric editor a moment
    // after showing it and rewrites "6.00" to "6"; a fill before then lands after the number.
    private async Task EditOpenTasks(string resident, string current, string value)
    {
        await Board.Cell(resident, OpenTasksColumn).DblClickAsync();
        await Expect(OpenTasksEditor).ToHaveValueAsync(current, new() { Timeout = 10000 });
        await OpenTasksEditor.FillAsync(value);
    }

    // Clicks a chip and waits for the board to answer with the residents the chips now choose.
    private async Task Choose(FusionChipListLocator chips, string chip, string board)
    {
        await ClickWhenStable(chips.Chip(chip));
        await Expect(Summary).ToHaveTextAsync(board, new() { Timeout = 10000 });
    }

    private async Task TickResident(string resident)
    {
        await ClickWhenStable(Tick(resident));
        await Expect(Board.SelectedRows.Filter(new() { HasText = resident })).ToHaveCountAsync(1, new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_board_opens_on_the_care_census_and_its_high_risk_watch_list()
    {
        await NavigateAndBoot();

        await Expect(Board.Rows.First).ToContainTextAsync("Abigail Moore");
        await Expect(Board.Rows.Last).ToContainTextAsync("Esther Cole");
        await Expect(Board.Cell("Beatrice Lane", RiskColumn)).ToHaveTextAsync("Critical");
        await Expect(WatchList.First).ToContainTextAsync("Amina Patel");
        await Expect(WatchList.Last).ToContainTextAsync("Priya Shah");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_critical_chip_shows_only_the_residents_at_critical_risk()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RiskChips.Chip("Critical"));

        await Expect(Summary).ToHaveTextAsync(CriticalResidents, new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(4);
        await Expect(Board.Rows.First).ToContainTextAsync("Beatrice Lane");
        await Expect(Board.Rows.Last).ToContainTextAsync("Mara Thompson");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_memory_care_chip_shows_only_the_memory_care_residents()
    {
        await NavigateAndBoot();

        await ClickWhenStable(CareChips.Chip("Memory Care"));

        await Expect(Summary).ToHaveTextAsync(MemoryCareResidents, new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(7);
        await Expect(Board.Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(Board.Rows.Last).ToContainTextAsync("Yara Ahmed");
        AssertNoConsoleErrors();
    }

    // Harold Chen is the one Memory Care resident at critical risk.
    [Test]
    public async Task the_critical_chip_keeps_to_the_memory_care_residents_already_chosen()
    {
        await NavigateAndBoot();
        await ClickWhenStable(CareChips.Chip("Memory Care"));
        await Expect(Summary).ToHaveTextAsync(MemoryCareResidents, new() { Timeout = 10000 });

        await ClickWhenStable(RiskChips.Chip("Critical"));

        await Expect(Summary).ToHaveTextAsync("1 Memory Care resident at Critical risk: 6 open care tasks.", new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(1);
        await Expect(Board.Rows.First).ToContainTextAsync("Harold Chen");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_all_risks_widens_the_board_back_to_the_memory_care_residents()
    {
        await NavigateAndBoot();
        await ClickWhenStable(CareChips.Chip("Memory Care"));
        await Expect(Summary).ToHaveTextAsync(MemoryCareResidents, new() { Timeout = 10000 });
        await ClickWhenStable(RiskChips.Chip("Critical"));
        await Expect(Board.Rows).ToHaveCountAsync(1, new() { Timeout = 10000 });

        await ClickWhenStable(RiskChips.Chip("All"));

        await Expect(Summary).ToHaveTextAsync(MemoryCareResidents, new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(7);
        await Expect(Board.Rows.First).ToContainTextAsync("Amina Patel");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_all_care_levels_brings_back_the_whole_census()
    {
        await NavigateAndBoot();
        await ClickWhenStable(CareChips.Chip("Memory Care"));
        await Expect(Summary).ToHaveTextAsync(MemoryCareResidents, new() { Timeout = 10000 });

        await ClickWhenStable(CareChips.Chip("All"));

        await Expect(Summary).ToHaveTextAsync(WholeCensus, new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(10);
        await Expect(Board.Rows.First).ToContainTextAsync("Abigail Moore");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_the_critical_chip_again_brings_back_every_risk()
    {
        await NavigateAndBoot();
        await ClickWhenStable(RiskChips.Chip("Critical"));
        await Expect(Summary).ToHaveTextAsync(CriticalResidents, new() { Timeout = 10000 });

        await ClickWhenStable(RiskChips.Chip("Critical"));

        await Expect(RiskChips.SelectedChips).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync(WholeCensus, new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(10);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_the_memory_care_chip_again_brings_back_every_care_level()
    {
        await NavigateAndBoot();
        await ClickWhenStable(CareChips.Chip("Memory Care"));
        await Expect(Summary).ToHaveTextAsync(MemoryCareResidents, new() { Timeout = 10000 });

        await ClickWhenStable(CareChips.Chip("Memory Care"));

        await Expect(CareChips.SelectedChips).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync(WholeCensus, new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(10);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sorting_by_next_review_lists_the_soonest_review_first()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Board.Header("Next Review"));

        await Expect(Board.Rows.First).ToContainTextAsync("Beatrice Lane", new() { Timeout = 10000 });
        await Expect(Board.Rows.Nth(1)).ToContainTextAsync("Amina Patel");
        await Expect(Board.Rows.Last).ToContainTextAsync("Dorothy Fleming");
        await Expect(Board.Rows).ToHaveCountAsync(10);
        AssertNoConsoleErrors();
    }

    // Sorting asks the server for the board again, and the chosen chip goes with it: by open tasks, Mara Thompson's
    // three come first and Harold Chen's six last.
    [Test]
    public async Task sorting_the_critical_residents_keeps_to_critical_risk()
    {
        await NavigateAndBoot();
        await ClickWhenStable(RiskChips.Chip("Critical"));
        await Expect(Summary).ToHaveTextAsync(CriticalResidents, new() { Timeout = 10000 });

        await ClickWhenStable(Board.Header("Open Tasks"));

        await Expect(Board.Rows.First).ToContainTextAsync("Mara Thompson", new() { Timeout = 10000 });
        await Expect(Board.Rows.Last).ToContainTextAsync("Harold Chen");
        await Expect(Board.Rows).ToHaveCountAsync(4);
        await Expect(Summary).ToHaveTextAsync(CriticalResidents);
        AssertNoConsoleErrors();
    }

    // By open tasks, Yara Ahmed's one comes first and Harold Chen's six last among the Memory Care residents.
    [Test]
    public async Task sorting_the_memory_care_residents_keeps_to_memory_care()
    {
        await NavigateAndBoot();
        await Choose(CareChips, "Memory Care", MemoryCareResidents);

        await ClickWhenStable(Board.Header("Open Tasks"));

        await Expect(Board.Rows.First).ToContainTextAsync("Yara Ahmed", new() { Timeout = 10000 });
        await Expect(Board.Rows.Last).ToContainTextAsync("Harold Chen");
        await Expect(Board.Rows).ToHaveCountAsync(7);
        await Expect(Summary).ToHaveTextAsync(MemoryCareResidents);
        AssertNoConsoleErrors();
    }

    // A list that opened on its first nurse would reassign the resident when the cell is left untouched.
    [Test]
    public async Task the_nurse_editor_opens_on_the_residents_own_nurse()
    {
        await NavigateAndBoot();

        await Board.Cell("Amina Patel", NurseColumn).DblClickAsync();

        await Expect(NurseEditor).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(NurseEditor).ToHaveValueAsync("Malik Stone");
        AssertNoConsoleErrors();
    }

    // The nurse's editor is still open when Save Care Changes is clicked: the page saves the open cell into the
    // pending changes before it sends them.
    [Test]
    public async Task a_nurse_chosen_in_the_cell_is_saved_while_the_cell_is_still_open()
    {
        await NavigateAndBoot();
        await Board.Cell("Abigail Moore", NurseColumn).DblClickAsync();
        await Expect(NurseEditor).ToHaveValueAsync("Nora Ellis", new() { Timeout = 10000 });
        await NurseEditor.SelectOptionAsync("Night Float Team");

        await ClickWhenStable(SaveCareChanges);

        await Expect(Summary).ToHaveTextAsync("Saved care-plan changes for Abigail Moore.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Abigail Moore", NurseColumn)).ToHaveTextAsync("Night Float Team");
        AssertNoConsoleErrors();
    }

    // The cell checks the range ResidentCareItemValidator gives the browser as it is left.
    [Test]
    public async Task more_than_seven_open_tasks_are_refused_in_the_cell()
    {
        await NavigateAndBoot();
        await EditOpenTasks("Abigail Moore", "1", "9");

        await Board.Cell("Amina Patel", OpenTasksColumn).DblClickAsync();

        await Expect(Page.GetByText("Open tasks run from 0 to 7.")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // Only a critical-risk resident must keep an open task. A grid column's rule holds for every row, so that rule
    // is not the cell's: Amina Patel, at high risk, can be left with none.
    [Test]
    public async Task a_resident_who_is_not_critical_can_be_left_with_no_open_tasks()
    {
        await NavigateAndBoot();
        await EditOpenTasks("Amina Patel", "6", "0");

        await ClickWhenStable(SaveCareChanges);

        await Expect(Summary).ToHaveTextAsync("Saved care-plan changes for Amina Patel.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Amina Patel", OpenTasksColumn)).ToHaveTextAsync("0");
        await Expect(Page.GetByText(ResidentCareItemValidator.CriticalNeedsAnOpenTask)).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    // Amina Patel, Bilal Khan and Priya Shah are the Memory Care residents at high risk.
    [Test]
    public async Task saved_care_changes_come_back_on_the_board_the_chips_chose()
    {
        await NavigateAndBoot();
        await Choose(RiskChips, "High", HighRiskResidents);
        await Choose(CareChips, "Memory Care", "3 Memory Care residents at High risk: 14 open care tasks.");
        await EditOpenTasks("Amina Patel", "6", "5");

        await ClickWhenStable(SaveCareChanges);

        await Expect(Summary).ToHaveTextAsync("Saved care-plan changes for Amina Patel.", new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(3);
        await Expect(Board.Rows.Last).ToContainTextAsync("Priya Shah");
        await Expect(Board.Cell("Amina Patel", OpenTasksColumn)).ToHaveTextAsync("5");
        AssertNoConsoleErrors();
    }

    // The cell lets the edit through; the server checks the critical-risk rule and keeps the census as filed.
    [Test]
    public async Task a_critical_resident_left_with_no_open_tasks_is_refused_when_the_changes_are_saved()
    {
        await NavigateAndBoot();
        await EditOpenTasks("Clara Hoffman", "4", "0");

        await ClickWhenStable(SaveCareChanges);

        await Expect(Summary).ToHaveTextAsync(
            $"Clara Hoffman: {ResidentCareItemValidator.CriticalNeedsAnOpenTask}", new() { Timeout = 10000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task ticked_residents_are_reassigned_to_the_night_float_team()
    {
        await NavigateAndBoot();
        await TickResident("Abigail Moore");
        await TickResident("Daniel Reyes");

        await ClickWhenStable(ReassignTicked);

        await Expect(Summary).ToHaveTextAsync("Reassigned Abigail Moore and Daniel Reyes to the Night Float Team.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Abigail Moore", NurseColumn)).ToHaveTextAsync("Night Float Team");
        await Expect(Board.Cell("Daniel Reyes", NurseColumn)).ToHaveTextAsync("Night Float Team");
        await Expect(Board.Cell("Arthur Bell", NurseColumn)).ToHaveTextAsync("Elena Ruiz");
        AssertNoConsoleErrors();
    }

    // Beatrice Lane, Clara Hoffman and Mara Thompson are the Skilled Nursing residents at critical risk.
    [Test]
    public async Task a_reassigned_resident_comes_back_on_the_board_the_chips_chose()
    {
        await NavigateAndBoot();
        await Choose(CareChips, "Skilled Nursing", SkilledNursingResidents);
        await Choose(RiskChips, "Critical", "3 Skilled Nursing residents at Critical risk: 12 open care tasks.");
        await TickResident("Beatrice Lane");

        await ClickWhenStable(ReassignTicked);

        await Expect(Summary).ToHaveTextAsync("Reassigned Beatrice Lane to the Night Float Team.", new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(3);
        await Expect(Board.Cell("Beatrice Lane", NurseColumn)).ToHaveTextAsync("Night Float Team");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reassigning_with_nobody_ticked_reassigns_no_one()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ReassignTicked);

        await Expect(Summary).ToHaveTextAsync("Tick the residents to reassign first.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Abigail Moore", NurseColumn)).ToHaveTextAsync("Nora Ellis");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_ticked_resident_flagged_critical_shows_critical_risk()
    {
        await NavigateAndBoot();
        await TickResident("Arthur Bell");

        await ClickWhenStable(FlagTickedCritical);

        await Expect(Summary).ToHaveTextAsync("Flagged Arthur Bell at Critical risk.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Arthur Bell", RiskColumn)).ToHaveTextAsync("Critical");
        AssertNoConsoleErrors();
    }

    // Esther Cole, Leo Simmons and Ruth Carter are the Assisted Living residents at low risk.
    [Test]
    public async Task a_resident_flagged_critical_leaves_a_board_kept_to_low_risk()
    {
        await NavigateAndBoot();
        await Choose(RiskChips, "Low", LowRiskResidents);
        await Choose(CareChips, "Assisted Living", "3 Assisted Living residents at Low risk: 3 open care tasks.");
        await TickResident("Esther Cole");

        await ClickWhenStable(FlagTickedCritical);

        await Expect(Summary).ToHaveTextAsync("Flagged Esther Cole at Critical risk.", new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(2);
        await Expect(Board.Row("Esther Cole")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_ticked_resident_discharged_after_confirming_leaves_the_census()
    {
        await NavigateAndBoot();
        await TickResident("Esther Cole");

        await ClickWhenStable(DischargeTicked);
        await Expect(ConfirmDialog).ToContainTextAsync("Discharge the ticked residents from the active census?", new() { Timeout = 10000 });
        await ClickWhenStable(ConfirmOk);

        await Expect(Summary).ToHaveTextAsync("Discharged Esther Cole.", new() { Timeout = 10000 });
        await Expect(Board.Row("Esther Cole")).ToHaveCountAsync(0);
        await Expect(Board.Rows.Last).ToContainTextAsync("Frank Delgado");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_discharged_resident_leaves_the_board_the_chips_chose()
    {
        await NavigateAndBoot();
        await Choose(CareChips, "Skilled Nursing", SkilledNursingResidents);
        await Choose(RiskChips, "Critical", "3 Skilled Nursing residents at Critical risk: 12 open care tasks.");
        await TickResident("Mara Thompson");

        await ClickWhenStable(DischargeTicked);
        await Expect(ConfirmDialog).ToContainTextAsync("Discharge the ticked residents from the active census?", new() { Timeout = 10000 });
        await ClickWhenStable(ConfirmOk);

        await Expect(Summary).ToHaveTextAsync("Discharged Mara Thompson.", new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(2);
        await Expect(Board.Rows.Last).ToContainTextAsync("Clara Hoffman");
        AssertNoConsoleErrors();
    }

    // The low-risk chip asks the server again after the answer: a discharge sent on Cancel would come first.
    [Test]
    public async Task cancelling_the_discharge_keeps_the_ticked_resident_on_the_census()
    {
        await NavigateAndBoot();
        await TickResident("Esther Cole");
        await ClickWhenStable(DischargeTicked);
        await Expect(ConfirmDialog).ToContainTextAsync("Discharge the ticked residents from the active census?", new() { Timeout = 10000 });

        await ClickWhenStable(ConfirmCancel);

        await ClickWhenStable(RiskChips.Chip("Low"));
        await Expect(Summary).ToHaveTextAsync(LowRiskResidents, new() { Timeout = 10000 });
        await Expect(Board.Row("Esther Cole")).ToHaveCountAsync(1);
        Assert.That(_dischargesSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    // The flag raises one refresh event, and the page has two listeners for it: one reloads the watch list,
    // the other the census.
    [Test]
    public async Task a_watched_resident_flagged_critical_leaves_the_watch_list_and_shows_critical_on_the_board()
    {
        await NavigateAndBoot();

        await ClickWhenStable(QuickAction("Amina Patel", "Flag Critical"));

        await Expect(Watched("Amina Patel")).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(WatchList).ToHaveCountAsync(4);
        await Expect(Board.Cell("Amina Patel", RiskColumn)).ToHaveTextAsync("Critical", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_watched_resident_discharged_after_confirming_leaves_the_watch_list_and_the_census()
    {
        await NavigateAndBoot();

        await ClickWhenStable(QuickAction("Bilal Khan", "Discharge"));
        await Expect(ConfirmDialog).ToContainTextAsync("Discharge Bilal Khan from the active census?", new() { Timeout = 10000 });
        await ClickWhenStable(ConfirmOk);

        await Expect(Watched("Bilal Khan")).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("23 residents on the census: 59 open care tasks.", new() { Timeout = 10000 });
        await Expect(Board.Row("Bilal Khan")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    // Flagging Amina Patel reloads the watch list after the answer: a discharge sent on Cancel would come first.
    [Test]
    public async Task cancelling_a_watched_residents_discharge_keeps_them_on_the_watch_list()
    {
        await NavigateAndBoot();
        await ClickWhenStable(QuickAction("Bilal Khan", "Discharge"));
        await Expect(ConfirmDialog).ToContainTextAsync("Discharge Bilal Khan from the active census?", new() { Timeout = 10000 });

        await ClickWhenStable(ConfirmCancel);

        await ClickWhenStable(QuickAction("Amina Patel", "Flag Critical"));
        await Expect(Watched("Amina Patel")).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(WatchList).ToHaveCountAsync(4);
        await Expect(Watched("Bilal Khan")).ToHaveCountAsync(1);
        Assert.That(_dischargesSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_empty_admission_is_refused_field_by_field()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AdmitResident);
        await Expect(AdmitDialog).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Admit);

        await Expect(ErrorFor(m => m.NewResidentName)).ToHaveTextAsync("Enter the resident's name.", new() { Timeout = 10000 });
        await Expect(ErrorFor(m => m.NewWing)).ToHaveTextAsync("Choose the resident's wing.");
        await Expect(ErrorFor(m => m.NewCareLevel)).ToHaveTextAsync("Choose the resident's care level.");
        await Expect(ErrorFor(m => m.NewRiskLevel)).ToHaveTextAsync("Choose the resident's risk.");
        await Expect(ErrorFor(m => m.NewOpenTasks)).ToHaveTextAsync("Enter the resident's open care tasks, or 0.");
        await Expect(Summary).ToHaveTextAsync(WholeCensus);
        AssertNoConsoleErrors();
    }

    // Agnes Whitlock files second by name, so she shows on the board's first page.
    [Test]
    public async Task an_admitted_resident_joins_the_census_with_the_night_float_team()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AdmitResident);
        await Expect(AdmitDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await NewResidentName.FillAndBlur("Agnes Whitlock");
        await NewWing.Select("West");
        await NewCareLevel.Select("Memory Care");
        await NewRiskLevel.Select("High");
        await NewOpenTasks.FillAndBlur("2");

        await ClickWhenStable(Admit);

        await Expect(Summary).ToHaveTextAsync("Admitted Agnes Whitlock to West wing, Memory Care, at High risk.", new() { Timeout = 10000 });
        await Expect(AdmitDialog).ToBeHiddenAsync();
        await Expect(Board.Rows.Nth(1)).ToContainTextAsync("Agnes Whitlock");
        await Expect(Board.Cell("Agnes Whitlock", NurseColumn)).ToHaveTextAsync("Night Float Team");
        await Expect(Board.Cell("Agnes Whitlock", OpenTasksColumn)).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    // Agnes Whitlock files first by name among the Memory Care residents at high risk.
    [Test]
    public async Task an_admitted_resident_joins_the_board_the_chips_chose()
    {
        await NavigateAndBoot();
        await Choose(CareChips, "Memory Care", MemoryCareResidents);
        await Choose(RiskChips, "High", "3 Memory Care residents at High risk: 14 open care tasks.");
        await ClickWhenStable(AdmitResident);
        await Expect(AdmitDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await NewResidentName.FillAndBlur("Agnes Whitlock");
        await NewWing.Select("West");
        await NewCareLevel.Select("Memory Care");
        await NewRiskLevel.Select("High");
        await NewOpenTasks.FillAndBlur("2");

        await ClickWhenStable(Admit);

        await Expect(Summary).ToHaveTextAsync("Admitted Agnes Whitlock to West wing, Memory Care, at High risk.", new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(4);
        await Expect(Board.Rows.First).ToContainTextAsync("Agnes Whitlock");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_an_admission_closes_the_form()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AdmitResident);
        await Expect(AdmitDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await NewResidentName.FillAndBlur("Agnes Whitlock");

        await ClickWhenStable(CancelAdmission);

        await Expect(AdmitDialog).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync(WholeCensus);
        AssertNoConsoleErrors();
    }
}
