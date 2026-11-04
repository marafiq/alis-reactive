namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridResidentAdmissionAudit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ResidentAdmissionAudit";

    private FusionGridLocator Roster => new(Page, "move-in-roster");
    private ILocator LastChange => Page.Locator("#roster-change");
    private ILocator Refused => Page.Locator("#move-in-refused");
    private ILocator MoveInZara => Page.Locator("#move-in-9101");
    private ILocator MoveInWalter => Page.Locator("#move-in-9102");
    private ILocator MoveOutHenry => Page.Locator("#move-out-6002");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Roster.Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
    }

    [Test]
    public async Task moving_zara_in_adds_her_to_the_roster_and_names_the_change()
    {
        await NavigateAndBoot();

        await ClickWhenStable(MoveInZara);

        await Expect(Roster.Rows).ToHaveCountAsync(7, new() { Timeout = 10000 });
        await Expect(Roster.Rows.First).ToContainTextAsync("Zara Ahmed");
        await Expect(LastChange).ToHaveTextAsync("Last change: Zara Ahmed moved in.", new() { Timeout = 10000 });
        await Expect(Refused).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task walter_cannot_move_in_before_his_residency_agreement_is_signed()
    {
        await NavigateAndBoot();

        await ClickWhenStable(MoveInWalter);

        await Expect(Refused).ToHaveTextAsync(
            "Walter Price cannot move in until the residency agreement is signed.", new() { Timeout = 10000 });
        await Expect(Roster.Row("Walter Price")).ToHaveCountAsync(0);
        await Expect(Roster.Rows).ToHaveCountAsync(6);
        await Expect(LastChange).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_refusal_goes_once_a_signed_arrival_moves_in()
    {
        await NavigateAndBoot();
        await ClickWhenStable(MoveInWalter);
        await Expect(Refused).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(MoveInZara);

        await Expect(LastChange).ToHaveTextAsync("Last change: Zara Ahmed moved in.", new() { Timeout = 10000 });
        await Expect(Refused).ToBeHiddenAsync();
        await Expect(Roster.Row("Walter Price")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_henry_out_after_an_arrival_takes_only_him_off_the_roster()
    {
        await NavigateAndBoot();
        await ClickWhenStable(MoveInZara);
        await Expect(LastChange).ToHaveTextAsync("Last change: Zara Ahmed moved in.", new() { Timeout = 10000 });

        await ClickWhenStable(MoveOutHenry);

        await Expect(LastChange).ToHaveTextAsync("Last change: Henry Liu moved out.", new() { Timeout = 10000 });
        await Expect(Roster.Row("Henry Liu")).ToHaveCountAsync(0);
        await Expect(Roster.Row("Zara Ahmed")).ToHaveCountAsync(1);
        await Expect(Roster.Rows).ToHaveCountAsync(6);
        AssertNoConsoleErrors();
    }
}
