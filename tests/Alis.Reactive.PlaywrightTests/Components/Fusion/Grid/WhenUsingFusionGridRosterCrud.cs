using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridRosterCrud : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/RosterCrud";
    private const string RosterId = "cedar-roster";

    private const int ResidentColumn = 2;
    private const int SuiteColumn = 3;

    private PagePlan<RosterCrudModel> _plan = null!;

    private ILocator Roster => Page.Locator($"#{RosterId} .e-gridcontent");
    private ILocator Rows => Page.Locator($"#{RosterId} .e-row");
    private ILocator Row(string resident) => Rows.Filter(new() { HasText = resident });
    private ILocator Cell(string resident, int column) => Row(resident).Locator($"td[aria-colindex='{column}']");
    private ILocator Status => Page.Locator("#desk-status");
    private ILocator MoveToSuite => Page.Locator("#move-to-suite");
    private ILocator Admit => Page.Locator("#admit-resident");
    private ILocator RecordDennisMoveOut => Page.Locator("#record-move-out-7204");
    private ILocator RecordFrankMoveOut => Page.Locator("#record-move-out-7206");
    private ILocator DennisMovingOut => Page.Locator("#moving-out-7204");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<RosterCrudModel>.FromPage(Page);
        await Expect(Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
    }

    private async Task PickInTheRoster(string resident)
    {
        await Cell(resident, ResidentColumn).ClickAsync();
        await Expect(Row(resident)).ToHaveAttributeAsync("aria-selected", "true", new() { Timeout = 10000 });
    }

    private async Task FillAdmission(string name, string suite)
    {
        await _plan.TextBox(m => m.AdmitName).FillAndBlur(name);
        await _plan.TextBox(m => m.AdmitSuite).FillAndBlur(suite);
    }

    [Test]
    public async Task the_roster_lists_the_wing_in_suite_order()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Agnes Whitfield");
        await Expect(Rows.Last).ToContainTextAsync("Frank Delgado");
        await Expect(Cell("Clara Jensen", SuiteColumn)).ToHaveTextAsync("108");
        await Expect(Status).ToHaveTextAsync("No changes at the desk yet today.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_admitted_resident_joins_the_roster_in_suite_order()
    {
        await NavigateAndBoot();
        await FillAdmission("Margaret Hale", "110");

        await ClickWhenStable(Admit);

        await Expect(Status).ToHaveTextAsync("Margaret Hale is admitted to suite 110.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(7, new() { Timeout = 10000 });
        await Expect(Rows.Nth(3)).ToContainTextAsync("Margaret Hale");
        await Expect(Rows.Nth(2)).ToContainTextAsync("Clara Jensen");
        await Expect(Rows.Nth(4)).ToContainTextAsync("Dennis Moreau");
        await Expect(Cell("Margaret Hale", SuiteColumn)).ToHaveTextAsync("110");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_admission_into_an_occupied_suite_is_refused()
    {
        await NavigateAndBoot();
        await FillAdmission("Margaret Hale", "108");

        await ClickWhenStable(Admit);

        await Expect(Status).ToHaveTextAsync("Suite 108 is Clara Jensen's. Choose another suite.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(6);
        await Expect(Roster).Not.ToContainTextAsync("Margaret Hale");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task moving_the_picked_resident_shows_their_new_suite_on_their_row()
    {
        await NavigateAndBoot();
        await PickInTheRoster("Bernard Okafor");
        await _plan.TextBox(m => m.NewSuite).FillAndBlur("118");

        await ClickWhenStable(MoveToSuite);

        await Expect(Status).ToHaveTextAsync("Bernard Okafor moves to suite 118.", new() { Timeout = 10000 });
        await Expect(Cell("Bernard Okafor", SuiteColumn)).ToHaveTextAsync("118", new() { Timeout = 10000 });
        await Expect(Cell("Agnes Whitfield", SuiteColumn)).ToHaveTextAsync("101");
        await Expect(Cell("Clara Jensen", SuiteColumn)).ToHaveTextAsync("108");
        await Expect(Rows).ToHaveCountAsync(6);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_suite_move_with_nobody_picked_is_refused()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.NewSuite).FillAndBlur("118");

        await ClickWhenStable(MoveToSuite);

        await Expect(Status).ToHaveTextAsync("Pick the resident who is moving in the roster.", new() { Timeout = 10000 });
        await Expect(Cell("Bernard Okafor", SuiteColumn)).ToHaveTextAsync("104");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task recording_a_move_out_takes_that_resident_off_the_roster()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RecordDennisMoveOut);

        await Expect(Status).ToHaveTextAsync("Dennis Moreau has moved out of suite 112.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
        await Expect(Roster).Not.ToContainTextAsync("Dennis Moreau");
        await Expect(Row("Clara Jensen")).ToHaveCountAsync(1);
        await Expect(Row("Eleanor Price")).ToHaveCountAsync(1);
        await Expect(DennisMovingOut).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_move_out_with_the_final_invoice_open_keeps_the_resident_on_the_roster()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RecordFrankMoveOut);

        await Expect(Status).ToHaveTextAsync(
            "Frank Delgado's final invoice is still open; the business office closes it before the move-out.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(6);
        await Expect(Row("Frank Delgado")).ToHaveCountAsync(1);
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }
}
