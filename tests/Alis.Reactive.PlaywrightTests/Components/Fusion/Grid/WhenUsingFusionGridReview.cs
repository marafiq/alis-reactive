using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the director of nursing, I want to go through the wing's residents a page at a time, see how many on the
/// page are high risk, put the residents I pick on Thursday's care conference agenda, and start a resident's
/// review from their row, so that every resident due a review has one under way before the conference.
/// </summary>
/// <remarks>
/// East wing, twelve residents six to a page. Page one: Amina Patel, Grace Bennett, Henry Liu, Irene Morgan,
/// Jonah Reed and Katherine Ortiz, one of them high risk (Katherine Ortiz); page two starts with Leo Simmons and
/// carries three high-risk residents. Henry Liu's review started last week; every other review has not. The
/// high-risk count is worked out from the rows of the page the grid shows each time the grid finishes showing
/// a page; counting the whole wing would say four on either page. The agenda and the started review come from
/// the server, which names the residents the page sent.
/// Not exercised: ActionComplete's payload, which the count does not read
/// (WhenUsingFusionGridInlineEdit.a_review_moved_to_a_weekday_is_booked_by_the_care_team_calendar reads its
/// RequestType and Data); the values of GetSelectedRowIndexes beyond whether any row is picked
/// (WhenUsingFusionGridDirectory.next_resident_opens_the_card_of_the_resident_below proves them).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridReview : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/CareReview";

    // The grid numbers its columns from 1; the hidden ID is the first.
    private const int ResidentColumn = 2;
    private const int ReviewColumn = 6;

    private FusionGridLocator Reviews => new(Page, "wing-reviews");
    private ILocator HighRiskOnPage => Page.Locator("#high-risk-on-page");
    private ILocator PickThisPage => Page.Locator("#pick-this-page");
    private ILocator AddToConference => Page.Locator("#add-to-conference");
    private ILocator Status => Page.Locator("#review-status");
    private ILocator PickedResidents => Reviews.SelectedRows.Locator($"td[aria-colindex='{ResidentColumn}']");
    private ILocator StartReview(string resident) =>
        Reviews.Row(resident).GetByRole(AriaRole.Button, new() { Name = "Start review" });

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Reviews.Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
    }

    // Ctrl-click adds a resident to those already picked.
    private async Task Pick(string resident) =>
        await Reviews.Cell(resident, ResidentColumn).ClickAsync(new() { Modifiers = new[] { KeyboardModifier.Control } });

    [Test]
    public async Task the_first_page_lists_six_residents_and_counts_its_one_high_risk_resident()
    {
        await NavigateAndBoot();

        await Expect(Reviews.Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(Reviews.Rows.Last).ToContainTextAsync("Katherine Ortiz");
        await Expect(HighRiskOnPage).ToHaveTextAsync("1", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("Ctrl-click residents to pick them, or start a review from a resident's row.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task turning_to_page_two_counts_the_three_high_risk_residents_there()
    {
        await NavigateAndBoot();
        await Expect(HighRiskOnPage).ToHaveTextAsync("1", new() { Timeout = 10000 });

        await ClickWhenStable(Reviews.PagerPage(2));

        await Expect(Reviews.Rows.First).ToContainTextAsync("Leo Simmons", new() { Timeout = 10000 });
        await Expect(HighRiskOnPage).ToHaveTextAsync("3", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_everyone_on_the_page_highlights_all_six_residents()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PickThisPage);

        await Expect(PickedResidents).ToHaveTextAsync(
            new[] { "Amina Patel", "Grace Bennett", "Henry Liu", "Irene Morgan", "Jonah Reed", "Katherine Ortiz" },
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_the_two_picked_residents_puts_them_on_the_conference_agenda()
    {
        await NavigateAndBoot();
        await Pick("Amina Patel");
        await Pick("Irene Morgan");
        await Expect(PickedResidents).ToHaveCountAsync(2, new() { Timeout = 10000 });

        await ClickWhenStable(AddToConference);

        await Expect(Status).ToHaveTextAsync(
            "Amina Patel and Irene Morgan are on Thursday's care conference agenda.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_everyone_on_the_page_puts_all_six_on_the_conference_agenda()
    {
        await NavigateAndBoot();
        await ClickWhenStable(PickThisPage);
        await Expect(PickedResidents).ToHaveCountAsync(6, new() { Timeout = 10000 });

        await ClickWhenStable(AddToConference);

        await Expect(Status).ToHaveTextAsync(
            "Amina Patel, Grace Bennett, Henry Liu, Irene Morgan, Jonah Reed and Katherine Ortiz are on Thursday's care conference agenda.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_with_nobody_picked_asks_for_the_residents_first()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AddToConference);

        await Expect(Status).ToHaveTextAsync("Pick the residents for the care conference first.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_a_review_from_a_residents_row_shows_it_under_way_on_that_row()
    {
        await NavigateAndBoot();
        await Expect(Reviews.Cell("Grace Bennett", ReviewColumn)).ToHaveTextAsync("Not started");

        await ClickWhenStable(StartReview("Grace Bennett"));

        await Expect(Status).ToHaveTextAsync("Review started for Grace Bennett, suite E-104.", new() { Timeout = 10000 });
        await Expect(Reviews.Cell("Grace Bennett", ReviewColumn)).ToHaveTextAsync("Under way", new() { Timeout = 10000 });
        await Expect(Reviews.Cell("Amina Patel", ReviewColumn)).ToHaveTextAsync("Not started");
        await Expect(Reviews.Cell("Irene Morgan", ReviewColumn)).ToHaveTextAsync("Not started");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_review_already_under_way_is_not_started_again()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StartReview("Henry Liu"));

        await Expect(Status).ToHaveTextAsync("Henry Liu's review is already under way.", new() { Timeout = 10000 });
        await Expect(Reviews.Cell("Henry Liu", ReviewColumn)).ToHaveTextAsync("Under way");
        AssertNoConsoleErrorsExcept("400");
    }
}
