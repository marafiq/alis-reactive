namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the resident services coordinator, I want the roster to come from the resident record and to bring today's
/// changes in the record onto it (an arrival, a new care level, an approved suite move), and to look at another
/// wing's roster, so that the roster the staff read always matches the record.
/// </summary>
/// <remarks>
/// The roster opens on Cedar wing as the record lists it, by suite: Amina Patel (101), Henry Liu (104), Grace
/// Bennett (108), Walter Grant (112) and Edith Hale (115). Today Rosa Diaz moves into suite 106, Henry Liu's care
/// level becomes Skilled Nursing after his hospital stay, and Grace Bennett's move to suite 118 is approved. Each
/// change comes from the server, which answers from the record; Birch wing lists Jonah Reed, Priya Shah, Ruth Carter
/// and Samuel Diaz.
/// UpdateRow's index only finds the row on screen: EJ2 saves the row by its primary key (normal-edit.js:250-269,
/// data.js:485), so the index is proven by the row changing at all, not by which row changes.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridServerRoster : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ServerRoster";
    private const string RosterId = "record-roster";

    // The roster numbers its columns from 1.
    private const int SuiteColumn = 3;
    private const int CareLevelColumn = 4;

    private ILocator Rows => Page.Locator($"#{RosterId} .e-row");
    private ILocator Cell(string resident, int column) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{column}']");
    private ILocator Wing => Page.Locator("#roster-wing");
    private ILocator Status => Page.Locator("#roster-record-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_roster_opens_on_cedar_wing_as_the_record_lists_it()
    {
        await NavigateAndBoot();

        await Expect(Status).ToHaveTextAsync("5 residents on the record.", new() { Timeout = 10000 });
        await Expect(Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(Cell("Henry Liu", SuiteColumn)).ToHaveTextAsync("104");
        await Expect(Rows.Last).ToContainTextAsync("Edith Hale");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task todays_arrival_takes_her_suites_place_on_the_roster()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Page.Locator("#roster-add-arrival"));

        await Expect(Status).ToHaveTextAsync("Rosa Diaz moves into suite 106 today.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
        await Expect(Rows.Nth(2)).ToContainTextAsync("Rosa Diaz");
        await Expect(Cell("Rosa Diaz", SuiteColumn)).ToHaveTextAsync("106");
        await Expect(Rows.Nth(1)).ToContainTextAsync("Henry Liu");
        await Expect(Rows.Nth(3)).ToContainTextAsync("Grace Bennett");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_care_level_changed_in_the_record_updates_henrys_row()
    {
        await NavigateAndBoot();
        await Expect(Cell("Henry Liu", CareLevelColumn)).ToHaveTextAsync("Assisted Living");

        await ClickWhenStable(Page.Locator("#roster-apply-care-level"));

        await Expect(Status).ToHaveTextAsync(
            "Henry Liu's care level is now Skilled Nursing in the record.", new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", CareLevelColumn)).ToHaveTextAsync("Skilled Nursing", new() { Timeout = 10000 });
        await Expect(Cell("Amina Patel", CareLevelColumn)).ToHaveTextAsync("Assisted Living");
        await Expect(Rows).ToHaveCountAsync(5);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_approved_suite_move_shows_grace_in_her_new_suite()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Page.Locator("#roster-show-suite-move"));

        await Expect(Status).ToHaveTextAsync("Grace Bennett moves from suite 108 to suite 118.", new() { Timeout = 10000 });
        await Expect(Cell("Grace Bennett", SuiteColumn)).ToHaveTextAsync("118", new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", SuiteColumn)).ToHaveTextAsync("104");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_birch_wing_tab_lists_birch_wings_residents()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Page.Locator("#roster-birch-wing"));

        await Expect(Wing).ToHaveTextAsync("Birch", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("4 residents on the record.");
        await Expect(Rows).ToHaveCountAsync(4, new() { Timeout = 10000 });
        await Expect(Rows.First).ToContainTextAsync("Jonah Reed");
        await Expect(Cell("Ruth Carter", SuiteColumn)).ToHaveTextAsync("205");
        await Expect(Page.Locator($"#{RosterId}")).Not.ToContainTextAsync("Amina Patel");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_cedar_wing_tab_brings_cedar_wing_back()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Page.Locator("#roster-birch-wing"));
        await Expect(Wing).ToHaveTextAsync("Birch", new() { Timeout = 10000 });
        await Expect(Rows.First).ToContainTextAsync("Jonah Reed", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#roster-cedar-wing"));

        await Expect(Wing).ToHaveTextAsync("Cedar", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("5 residents on the record.");
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
        await Expect(Rows.First).ToContainTextAsync("Amina Patel");
        AssertNoConsoleErrors();
    }
}
