using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the charge nurse on the South wing, I want the updates the pharmacy and the therapy team send, and what
/// the clinical review team decides for a resident, to show on that resident's row of the wing's board as soon
/// as the server answers, with the changed row picked out, so that the aides read a current board all shift.
/// </summary>
/// <remarks>
/// The board: Dorothy Hale S-101 (Independent, Low, 2 tasks), Grace Bennett S-104 (Assisted Living, Moderate,
/// 3), Harold Brooks S-107, Irene Morgan S-110 (Skilled Nursing, Low, 3), Leo Simmons S-112 (Assisted Living,
/// Low, 2, next review 2026-06-09) and Katherine Ortiz S-115 (Memory Care, High, 4). Each resident's cell shows
/// the suite and care level under the name, and the fall risk is a badge, red when High. The pharmacy added two
/// tasks for Irene Morgan and therapy raised Grace Bennett's fall risk to High; applying either changes that one
/// cell by the resident's key (SetCellValue: a number, then a text the badge draws) and picks the row out by
/// its key (GetRowIndexByPrimaryKey). A clinical review replaces the picked resident's row with the one the
/// server answers (SetRowData); a review books the next one three months on.
/// Every grid member the page uses is exercised.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridOperations : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Operations";

    // The grid numbers its columns from 1; the hidden ID is the first.
    private const int ResidentColumn = 2;
    private const int FallRiskColumn = 3;
    private const int OpenTasksColumn = 4;
    private const int PrimaryNurseColumn = 5;
    private const int NextReviewColumn = 6;

    private PagePlan<ClinicalUpdatesViewModel> _plan = null!;

    private FusionGridLocator Board => new(Page, "clinical-updates-board");
    private ILocator Badge(string resident) => Board.Cell(resident, FallRiskColumn).Locator("span");
    private ILocator PickedResidents => Board.SelectedRows.Locator($"td[aria-colindex='{ResidentColumn}']");
    private ILocator ApplyPharmacyUpdate => Page.Locator("#apply-pharmacy-update");
    private ILocator ApplyTherapyUpdate => Page.Locator("#apply-therapy-update");
    private ILocator RecordReview => Page.Locator("#record-review");
    private ILocator Status => Page.Locator("#board-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<ClinicalUpdatesViewModel>.FromPage(Page);
        await Expect(Board.Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_board_shows_each_residents_suite_and_care_level_under_their_name()
    {
        await NavigateAndBoot();

        var dorothy = Board.Cell("Dorothy Hale", ResidentColumn);
        await Expect(dorothy.GetByText("Dorothy Hale", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(dorothy.GetByText("Suite S-101 · Independent", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Board.Cell("Irene Morgan", ResidentColumn).GetByText("Suite S-110 · Skilled Nursing", new() { Exact = true }))
            .ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("The board is as the night shift left it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_high_risk_resident_carries_a_red_fall_risk_badge()
    {
        await NavigateAndBoot();

        await Expect(Badge("Katherine Ortiz")).ToHaveTextAsync("High");
        await Expect(Badge("Katherine Ortiz")).ToHaveClassAsync(new Regex("bg-red-100"));
        await Expect(Badge("Dorothy Hale")).ToHaveTextAsync("Low");
        await Expect(Badge("Dorothy Hale")).Not.ToHaveClassAsync(new Regex("bg-red-100"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_pharmacy_update_adds_two_tasks_to_irenes_row_and_picks_it_out()
    {
        await NavigateAndBoot();
        await Expect(Board.Cell("Irene Morgan", OpenTasksColumn)).ToHaveTextAsync("3");

        await ClickWhenStable(ApplyPharmacyUpdate);

        await Expect(Status).ToHaveTextAsync("Pharmacy update applied: Irene Morgan has 5 open tasks.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Irene Morgan", OpenTasksColumn)).ToHaveTextAsync("5", new() { Timeout = 10000 });
        await Expect(Board.Cell("Dorothy Hale", OpenTasksColumn)).ToHaveTextAsync("2");
        await Expect(PickedResidents).ToHaveCountAsync(1, new() { Timeout = 10000 });
        await Expect(PickedResidents.GetByText("Irene Morgan", new() { Exact = true })).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_therapy_update_turns_graces_fall_risk_badge_red_and_picks_her_out()
    {
        await NavigateAndBoot();
        await Expect(Badge("Grace Bennett")).ToHaveTextAsync("Moderate");

        await ClickWhenStable(ApplyTherapyUpdate);

        await Expect(Status).ToHaveTextAsync("Therapy update applied: Grace Bennett's fall risk is High.", new() { Timeout = 10000 });
        await Expect(Badge("Grace Bennett")).ToHaveTextAsync("High", new() { Timeout = 10000 });
        await Expect(Badge("Grace Bennett")).ToHaveClassAsync(new Regex("bg-red-100"));
        await Expect(Badge("Irene Morgan")).ToHaveTextAsync("Low");
        await Expect(PickedResidents).ToHaveCountAsync(1, new() { Timeout = 10000 });
        await Expect(PickedResidents.GetByText("Grace Bennett", new() { Exact = true })).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_a_clinical_review_updates_the_picked_residents_row()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Board.Cell("Leo Simmons", PrimaryNurseColumn));
        await Expect(PickedResidents).ToHaveCountAsync(1, new() { Timeout = 10000 });
        await _plan.DropDownList(m => m.ReviewRisk).Select("Moderate");
        await _plan.NumericTextBox(m => m.ReviewOpenTasks).FillAndBlur("6");

        await ClickWhenStable(RecordReview);

        await Expect(Status).ToHaveTextAsync(
            "Clinical review recorded for Leo Simmons: Moderate fall risk, 6 open tasks, next review September 9.",
            new() { Timeout = 10000 });
        await Expect(Badge("Leo Simmons")).ToHaveTextAsync("Moderate", new() { Timeout = 10000 });
        await Expect(Board.Cell("Leo Simmons", OpenTasksColumn)).ToHaveTextAsync("6");
        await Expect(Board.Cell("Leo Simmons", NextReviewColumn)).ToHaveTextAsync("2026-09-09");
        await Expect(Board.Cell("Leo Simmons", ResidentColumn).GetByText("Suite S-112 · Assisted Living", new() { Exact = true }))
            .ToBeVisibleAsync();
        await Expect(Board.Cell("Dorothy Hale", NextReviewColumn)).ToHaveTextAsync("2026-06-02");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_a_review_with_nobody_picked_asks_for_the_resident()
    {
        await NavigateAndBoot();
        await _plan.DropDownList(m => m.ReviewRisk).Select("High");
        await _plan.NumericTextBox(m => m.ReviewOpenTasks).FillAndBlur("4");

        await ClickWhenStable(RecordReview);

        await Expect(Status).ToHaveTextAsync("Pick the resident the review team saw.", new() { Timeout = 10000 });
        await Expect(Badge("Katherine Ortiz")).ToHaveTextAsync("High");
        await Expect(Board.Cell("Leo Simmons", OpenTasksColumn)).ToHaveTextAsync("2");
        AssertNoConsoleErrorsExcept("400");
    }
}
