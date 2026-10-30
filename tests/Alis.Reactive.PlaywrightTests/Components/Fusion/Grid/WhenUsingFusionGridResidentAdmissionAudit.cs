namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the admissions coordinator on move-in day, I want to move today's arrivals onto the wing's roster and
/// today's departures off it, with the last change named above the roster, so that the nurses work from a
/// roster that matches who is in the building, and nobody is added before their residency agreement is signed.
/// </summary>
/// <remarks>
/// The North wing roster starts with six residents; Henry Liu is the second. Zara Ahmed arrives with her
/// agreement signed; Walter Price's is not signed yet. Henry Liu leaves today; his row is found by his
/// resident number, wherever an arrival has pushed it.
/// Not exercised:
/// ActionBegin's and ActionComplete's Name and Type: EJ2 writes the event's own name on its payload
/// (ej2-base observer.js:92) and Type repeats it (normal-edit.js:651, 75-84); a handler is attached to one
/// event and has nothing to decide by either.
/// ActionBegin's Cancel read back: the page writes it to refuse a move-in and never needs to read it; the write
/// is the same JavaScript member.
/// </remarks>
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

    // An arrival moves in at the top and is selected, so Henry's row is found by his resident number.
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
