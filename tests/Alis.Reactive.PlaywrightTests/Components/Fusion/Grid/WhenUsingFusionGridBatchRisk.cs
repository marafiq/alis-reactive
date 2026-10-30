using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the director of nursing at the morning falls huddle, I want to raise the fall risk of each resident who fell
/// overnight in one step, change any other risk the team agrees on, and send the changes to care planning, so that
/// every care plan reflects the risk the huddle decided before the day shift starts.
/// </summary>
/// <remarks>
/// Aspen wing's risk board loads from the record: Amina Patel (room 201, Low), Henry Liu (204, Moderate), Grace
/// Bennett (208, Low), Walter Grant (212, Moderate) and Ruth Carter (215, Low). Henry and Ruth fell overnight; their
/// Raise to High buttons set their fall risk to High. Sending tells care planning each risk that differs from the
/// record, the risk still being chosen in its cell included: the Send button takes the focus without the grid saving
/// that cell (grid.js:5907-5919), so the send keeps it only through its own SaveCell.
/// Not exercised:
/// The pending changes' AddedRecords and DeletedRecords: the huddle adds and removes no residents, so both arrive
/// empty (batch-edit.js:347-372).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridBatchRisk : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchRiskReview";
    private const string BoardId = "falls-risk-board";

    // The board numbers its columns from 1.
    private const int FallRiskColumn = 4;

    private static readonly Regex Changed = new("e-updatedtd");

    private ILocator Rows => Page.Locator($"#{BoardId} .e-row");
    private ILocator FallRisk(string resident) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{FallRiskColumn}']");
    private ILocator FallRiskEditor => Page.Locator($"#{BoardId} select[name='fallRisk']");
    private ILocator RaiseHenry => Page.Locator("#raise-risk-6402");
    private ILocator RaiseRuth => Page.Locator("#raise-risk-6405");
    private ILocator SendToCarePlanning => Page.Locator("#send-to-care-planning");
    private ILocator Status => Page.Locator("#care-planning-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_board_opens_with_each_residents_fall_risk_on_record()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(FallRisk("Amina Patel")).ToHaveTextAsync("Low");
        await Expect(FallRisk("Henry Liu")).ToHaveTextAsync("Moderate");
        await Expect(FallRisk("Ruth Carter")).ToHaveTextAsync("Low");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task raising_a_resident_who_fell_marks_them_high_risk()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RaiseHenry);

        await Expect(FallRisk("Henry Liu")).ToHaveTextAsync("High", new() { Timeout = 10000 });
        await Expect(FallRisk("Henry Liu")).ToHaveClassAsync(Changed);
        await Expect(FallRisk("Amina Patel")).ToHaveTextAsync("Low");
        await Expect(FallRisk("Walter Grant")).ToHaveTextAsync("Moderate");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_tells_care_planning_each_risk_the_huddle_raised()
    {
        await NavigateAndBoot();
        await ClickWhenStable(RaiseHenry);
        await ClickWhenStable(RaiseRuth);
        await Expect(FallRisk("Ruth Carter")).ToHaveTextAsync("High", new() { Timeout = 10000 });

        await ClickWhenStable(SendToCarePlanning);

        await Expect(Status).ToHaveTextAsync(
            "Sent to care planning: Henry Liu, Moderate → High; Ruth Carter, Low → High.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The risk list opens on the resident's own risk; the cell is still open when the director sends.
    [Test]
    public async Task a_risk_still_being_chosen_in_its_cell_is_sent_too()
    {
        await NavigateAndBoot();
        await FallRisk("Grace Bennett").DblClickAsync();
        await Expect(FallRiskEditor).ToHaveValueAsync("Low", new() { Timeout = 10000 });
        await FallRiskEditor.SelectOptionAsync("Moderate");

        await ClickWhenStable(SendToCarePlanning);

        await Expect(Status).ToHaveTextAsync(
            "Sent to care planning: Grace Bennett, Low → Moderate.", new() { Timeout = 10000 });
        await Expect(FallRisk("Grace Bennett")).ToHaveTextAsync("Moderate");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_with_no_risk_changed_tells_the_huddle_nothing_went_to_care_planning()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SendToCarePlanning);

        await Expect(Status).ToHaveTextAsync(
            "No fall risk changed, so nothing went to care planning.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
