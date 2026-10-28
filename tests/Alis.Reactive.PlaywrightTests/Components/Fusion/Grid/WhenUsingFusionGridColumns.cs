using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the staffing scheduler, I want to choose which columns the morning huddle sheet carries, lead it with
/// each resident's risk when the huddle reviews falls, and head the wing column with the community's own word
/// for a wing, so that the printed sheet carries only what the huddle reads, in the order it reads it.
/// </summary>
/// <remarks>
/// The sheet lists five residents, Ada Lawson first, under Resident, Primary nurse, Next review, Risk, Wing and
/// Care level. A column button leaves its column off the sheet when the sheet shows it (the grid's own column
/// says whether it shows) and puts it back when it does not. Lead with risk moves the risk column into the
/// resident's place at the front, and clearing the box moves it back into the place after the next review; the
/// two columns are apart, so a move from the wrong column to the wrong place shows. The wing heading is written
/// into the wing column and the headers are drawn again.
/// Not exercised: writing a column's Visible (the page only reads it; hiding and showing go through HideColumn
/// and ShowColumn), and moving headers by dragging, which is EJ2's own gesture and runs no plan.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridColumns : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/CareStaffColumns";

    private static readonly string[] EveryColumn = { "Resident", "Primary nurse", "Next review", "Risk", "Wing", "Care level" };

    private PagePlan<HuddleSheetViewModel> _plan = null!;

    private FusionGridLocator Sheet => new(Page, "huddle-sheet");
    private ILocator PrimaryNurseColumn => Page.Locator("#primary-nurse-column");
    private ILocator NextReviewColumn => Page.Locator("#next-review-column");
    private ILocator LeadWithRisk => _plan.NativeCheckBox(m => m.LeadWithRisk).Input;
    private FusionTextBoxLocator WingHeading => new(Page, _plan.FindComponent(m => m.WingHeading)!.ElementId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<HuddleSheetViewModel>.FromPage(Page);
        await Expect(Sheet.Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_sheet_opens_with_every_column_in_huddle_order()
    {
        await NavigateAndBoot();

        await Expect(Sheet.VisibleHeaders).ToHaveTextAsync(EveryColumn);
        await Expect(Sheet.Rows.First.Locator("td:visible")).ToHaveTextAsync(
            new[] { "Ada Lawson", "Nora Ellis", "2026-06-02", "Low", "North", "Independent" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_primary_nurse_off_takes_its_column_off_the_sheet()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PrimaryNurseColumn);

        await Expect(Sheet.VisibleHeaders).ToHaveTextAsync(
            new[] { "Resident", "Next review", "Risk", "Wing", "Care level" }, new() { Timeout = 10000 });
        await Expect(Sheet.Rows.First.Locator("td:visible")).ToHaveTextAsync(
            new[] { "Ada Lawson", "2026-06-02", "Low", "North", "Independent" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task putting_the_primary_nurse_back_returns_its_column_to_its_place()
    {
        await NavigateAndBoot();
        await ClickWhenStable(PrimaryNurseColumn);
        await Expect(Sheet.VisibleHeaders).ToHaveCountAsync(5, new() { Timeout = 10000 });

        await ClickWhenStable(PrimaryNurseColumn);

        await Expect(Sheet.VisibleHeaders).ToHaveTextAsync(EveryColumn, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_next_review_off_keeps_the_primary_nurse_on_the_sheet()
    {
        await NavigateAndBoot();

        await ClickWhenStable(NextReviewColumn);

        await Expect(Sheet.VisibleHeaders).ToHaveTextAsync(
            new[] { "Resident", "Primary nurse", "Risk", "Wing", "Care level" }, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leading_with_risk_puts_the_risk_column_before_the_resident()
    {
        await NavigateAndBoot();

        await LeadWithRisk.CheckAsync();

        await Expect(Sheet.VisibleHeaders).ToHaveTextAsync(
            new[] { "Risk", "Resident", "Primary nurse", "Next review", "Wing", "Care level" }, new() { Timeout = 10000 });
        await Expect(Sheet.Rows.First.Locator("td:visible").First).ToHaveTextAsync("Low");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_longer_leading_with_risk_puts_risk_back_after_the_next_review()
    {
        await NavigateAndBoot();
        await LeadWithRisk.CheckAsync();
        await Expect(Sheet.VisibleHeaders.First).ToHaveTextAsync("Risk", new() { Timeout = 10000 });

        await LeadWithRisk.UncheckAsync();

        await Expect(Sheet.VisibleHeaders).ToHaveTextAsync(EveryColumn, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task heading_the_wing_column_neighborhood_renames_it_on_the_sheet()
    {
        await NavigateAndBoot();

        await WingHeading.FillAndBlur("Neighborhood");

        await Expect(Sheet.VisibleHeaders).ToHaveTextAsync(
            new[] { "Resident", "Primary nurse", "Next review", "Risk", "Neighborhood", "Care level" }, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
