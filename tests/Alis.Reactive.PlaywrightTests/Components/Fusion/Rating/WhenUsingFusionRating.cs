using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Rating;

// Journey: a resident reviews and updates their Monthly Care Satisfaction Survey.
// The survey carries over last month's rating; the resident rates by clicking stars,
// can restore last month's score, can clear it, then submits.
[TestFixture]
public class WhenUsingFusionRating : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Rating";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_RatingModel";
    private const string SatisfactionScoreId = GeneratedTypeScope + "__SatisfactionScore";

    private FusionRatingLocator SatisfactionRating => new(Page, SatisfactionScoreId);
    private ILocator ScoreText => Page.Locator("#survey-rating");
    private ILocator SentimentText => Page.Locator("#survey-sentiment");
    private ILocator ChangeNoteText => Page.Locator("#survey-change-note");
    private ILocator ReadinessText => Page.Locator("#survey-readiness");
    private ILocator Confirmation => Page.Locator("#survey-confirmation");
    private ILocator RestoreButton => Page.Locator("#restore-rating");
    private ILocator ClearButton => Page.Locator("#clear-rating");
    private ILocator SubmitButton => Page.Locator("#survey-submit");

    private async Task OpenSurvey()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(SatisfactionRating.RatingList).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionRating builder renders the rating bound to the carried-over model value.
    [Test]
    public async Task survey_opens_showing_the_rating_carried_over_from_last_month()
    {
        await OpenSurvey();

        await Expect(SatisfactionRating.Star(1)).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(SatisfactionRating.RatingList).ToHaveAttributeAsync("aria-valuenow", "3");
        await Expect(ScoreText).ToHaveTextAsync("3");

        AssertNoConsoleErrors();
    }

    // INTERACTS — clicking a star fires ValueChanged through the .Reactive wiring; the
    // FusionRatingValueChangedArgs payload carries the new Value into the visible response.
    [Test]
    public async Task rating_their_care_shows_the_score_and_a_matching_message()
    {
        await OpenSurvey();

        await SatisfactionRating.RateStars(5);

        await Expect(SatisfactionRating.RatingList).ToHaveAttributeAsync("aria-valuenow", "5");
        await Expect(ScoreText).ToHaveTextAsync("5");
        await Expect(SentimentText).ToHaveTextAsync("We're so glad your care is meeting your expectations.");

        AssertNoConsoleErrors();
    }

    // Lowering the rating proves FusionRatingValueChangedArgs.PreviousValue carries the value
    // before the change, and the mid-score branch over args.Value routes the follow-up message.
    [Test]
    public async Task lowering_their_rating_records_what_it_changed_from()
    {
        await OpenSurvey();

        await SatisfactionRating.RateStars(2);

        await Expect(ScoreText).ToHaveTextAsync("2");
        await Expect(ChangeNoteText).ToHaveTextAsync("3");
        await Expect(SentimentText).ToHaveTextAsync("Thank you. A care manager will follow up to see how we can improve.");

        AssertNoConsoleErrors();
    }

    // Clearing proves Reset() returns the rating to unrated, and FusionRatingValueChangedArgs.IsInteracted
    // distinguishes a rating the resident chose (true) from a programmatic clear (false).
    [Test]
    public async Task clearing_a_rating_the_resident_chose_marks_it_unrated()
    {
        await OpenSurvey();

        await SatisfactionRating.RateStars(4);
        await Expect(ReadinessText)
            .ToHaveTextAsync("Thank you for rating your care yourself — your rating is ready to submit.");

        await ClearButton.ClickAsync();

        await Expect(SatisfactionRating.RatingList).ToHaveAttributeAsync("aria-valuenow", "0");
        await Expect(ScoreText).ToHaveTextAsync("0");
        await Expect(ReadinessText)
            .ToHaveTextAsync("Your rating was cleared. Please rate your care to continue.");

        AssertNoConsoleErrors();
    }

    // Restoring proves SetValue() writes the given value back onto the rating, and the Value() source
    // reads the restored value back out for display.
    [Test]
    public async Task restoring_brings_back_the_rating_submitted_last_month()
    {
        await OpenSurvey();

        await SatisfactionRating.RateStars(5);
        await Expect(SatisfactionRating.RatingList).ToHaveAttributeAsync("aria-valuenow", "5");

        await RestoreButton.ClickAsync();

        await Expect(SatisfactionRating.RatingList).ToHaveAttributeAsync("aria-valuenow", "3");
        await Expect(ScoreText).ToHaveTextAsync("3");

        AssertNoConsoleErrors();
    }

    // SUBMITS — the Value() source feeds the gather body; the server confirmation the resident sees
    // reflects the submitted score.
    [Test]
    public async Task submitting_sends_the_rating_and_confirms_it()
    {
        await OpenSurvey();

        await SatisfactionRating.RateStars(5);
        await SubmitButton.ClickAsync();

        await Expect(Confirmation)
            .ToHaveTextAsync("Thank you. We recorded your satisfaction rating of 5 of 5 stars.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries the Value() source into the POST body under the
    // declared key. (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task submitting_posts_the_rating_score_to_the_server()
    {
        await OpenSurvey();

        await SatisfactionRating.RateStars(5);

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/Rating/Echo") && request.Method == "POST",
            new() { Timeout = 10000 });

        await SubmitButton.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"satisfactionScore\":5"),
            "the gather pipeline must carry the rating Value() source under its declared key");

        AssertNoConsoleErrors();
    }
}
