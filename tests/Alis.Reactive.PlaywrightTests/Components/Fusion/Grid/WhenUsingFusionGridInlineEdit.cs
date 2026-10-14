namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As a Birch wing nurse, I want to move a resident's next care-plan review in the review list and have the
/// care team's calendar confirm the new day, so that every review lands on a day the care team meets.
/// </summary>
/// <remarks>
/// The list is Amina Patel (due Thursday, October 15), Henry Liu (due Friday, October 16), Grace Bennett,
/// Walter Grant and Rosa Diaz. The reviews due this week open from the buttons above the list; Save sends
/// the moved review to the calendar, which books a weekday and refuses a weekend, and a refused review goes
/// back to the day on file.
/// Not exercised:
/// ActionComplete's Name, Type, Action, PreviousData, RowIndex, SelectedRow and Cancel: the page sends the
/// saved row, told apart by RequestType. Name and Type are the event's own name (ej2-base observer.js:92,
/// normal-edit.js:76); Cancel acts only in ActionBegin; WhenUsingFusionGridEditing reads Action and
/// PreviousData and cancels an ActionBegin.
/// A review saved with its day cleared is refused as a weekend is; a nurse clears the day only by mistake.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridInlineEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/InlineEdit";
    private const string GridId = "care-plan-reviews";

    // The grid numbers its columns from 1, counting the hidden ID column.
    private const int NextReviewColumn = 5;

    private ILocator Rows => Page.Locator($"#{GridId} .e-row");
    private ILocator NextReview(string resident) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{NextReviewColumn}']");
    private ILocator RowBeingEdited => Page.Locator($"#{GridId} .e-editedrow");
    private ILocator ResidentBeingEdited => Page.Locator($"#{GridId}residentName");
    private ILocator DayEditor => RowBeingEdited.Locator("input[name='nextReview']");
    private ILocator HenryDueThisWeek => Page.Locator("#review-due-6302");
    private ILocator Save => Page.Locator("#review-save");
    private ILocator Cancel => Page.Locator("#review-cancel");
    private ILocator Status => Page.Locator("#review-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    private async Task OpenHenrysReview()
    {
        await ClickWhenStable(HenryDueThisWeek);
        await Expect(DayEditor).ToHaveValueAsync("2026-10-16", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_review_list_shows_each_residents_next_review()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(NextReview("Henry Liu")).ToHaveTextAsync("2026-10-16");
        await Expect(NextReview("Rosa Diaz")).ToHaveTextAsync("2027-01-14");
        AssertNoConsoleErrors();
    }

    // Henry is the second row: his button finds his row by his resident number, selects it and opens it.
    [Test]
    public async Task choosing_a_review_due_this_week_opens_that_residents_review_day()
    {
        await NavigateAndBoot();

        await ClickWhenStable(HenryDueThisWeek);

        await Expect(ResidentBeingEdited).ToHaveValueAsync("Henry Liu", new() { Timeout = 10000 });
        await Expect(DayEditor).ToHaveValueAsync("2026-10-16");
        await Expect(RowBeingEdited).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_review_moved_to_a_weekday_is_booked_by_the_care_team_calendar()
    {
        await NavigateAndBoot();
        await OpenHenrysReview();
        await DayEditor.FillAsync("2026-11-03");

        await ClickWhenStable(Save);

        await Expect(Status).ToHaveTextAsync(
            "Henry Liu's care-plan review is booked for Tuesday, November 3.", new() { Timeout = 10000 });
        await Expect(NextReview("Henry Liu")).ToHaveTextAsync("2026-11-03");
        await Expect(RowBeingEdited).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_review_moved_to_a_weekend_is_refused_and_goes_back_to_its_booked_day()
    {
        await NavigateAndBoot();
        await OpenHenrysReview();
        await DayEditor.FillAsync("2026-11-07");

        await ClickWhenStable(Save);

        await Expect(Status).ToHaveTextAsync(
            "The care team does not meet on Saturdays. Henry Liu's review stays on Friday, October 16.",
            new() { Timeout = 10000 });
        await Expect(NextReview("Henry Liu")).ToHaveTextAsync("2026-10-16");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task cancelling_a_move_keeps_the_booked_day_and_books_nothing()
    {
        await NavigateAndBoot();
        await OpenHenrysReview();
        await DayEditor.FillAsync("2026-11-03");

        await ClickWhenStable(Cancel);

        await Expect(RowBeingEdited).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(NextReview("Henry Liu")).ToHaveTextAsync("2026-10-16");
        await Expect(Status).ToHaveTextAsync("No reviews moved yet.");
        AssertNoConsoleErrors();
    }
}
