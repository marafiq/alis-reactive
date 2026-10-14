using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Rating;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Rating;

/// <summary>
/// As a resident answering the monthly care satisfaction survey, I want to rate October's care in stars, say what
/// made the month harder when I lower my rating, and send it, so that the community hears how its care is going and
/// I see how my month compares with everyone else's.
/// </summary>
/// <remarks>
/// Margaret Hale rated September 3 stars. The survey starts unrated; four or five stars is thanked, three or fewer
/// brings a call from the care manager, and lowering the rating asks what made October harder. The community
/// answers a sent rating with October's average over its other 23 residents' ratings (95 stars) and hers.
/// Not exercised: <c>DataBind</c>, because a rating written by the page is readable at once (the property setter
/// stores it, ej2-base notify-property-change.js:70-77) and EJ2 raises no <c>valueChanged</c> for it
/// (rating.js:499-506, 709-714), so no later step of the same pipeline can tell whether it was applied; only the
/// stars repaint a tick later.
/// </remarks>
[TestFixture]
public class WhenUsingFusionRating : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Rating";
    private const string Unrated = "Choose the stars that match October's care.";
    private const string ChooseAStar = "Choose at least one star to rate October's care.";

    private PagePlan<SatisfactionSurveyModel> _plan = null!;

    private FusionRatingLocator Rating => new(Page, _plan.FindComponent(m => m.SatisfactionScore)!.ElementId);
    private ILocator Reply => Page.Locator("#survey-reply");
    private ILocator WhatChanged => Page.Locator("#what-changed");
    private ILocator Recorded => Page.Locator("#survey-recorded");
    private ILocator SameAsSeptember => Page.Locator("#same-as-last-month-btn");
    private ILocator ClearMyRating => Page.Locator("#clear-rating-btn");
    private ILocator SendMyRating => Page.Locator("#send-survey-btn");

    private async Task OpenTheSurvey()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<SatisfactionSurveyModel>.FromPage(Page);
        await Expect(Rating.Star(5)).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task Rate(int stars)
    {
        await Rating.RateStars(stars);
        await Expect(Rating.RatingList).ToHaveAttributeAsync("aria-valuenow", stars.ToString(), new() { Timeout = 5000 });
    }

    [Test]
    public async Task the_survey_opens_unrated()
    {
        await OpenTheSurvey();

        await Expect(Rating.RatingList).ToHaveAttributeAsync("aria-valuenow", "0");
        await Expect(Reply).ToHaveTextAsync(Unrated);
        await Expect(WhatChanged).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task rating_october_five_stars_says_the_community_is_glad()
    {
        await OpenTheSurvey();

        await Rate(5);

        await Expect(Reply).ToHaveTextAsync("We're glad October's care went well.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task rating_october_two_stars_brings_a_call_from_the_care_manager()
    {
        await OpenTheSurvey();

        await Rate(2);

        await Expect(Reply).ToHaveTextAsync(
            "Thank you for telling us. Maple wing's care manager will call you this week.", new() { Timeout = 5000 });
        await Expect(WhatChanged).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lowering_the_rating_asks_what_made_october_harder()
    {
        await OpenTheSurvey();
        await Rate(4);
        await Expect(WhatChanged).ToBeHiddenAsync();

        await Rate(2);

        await Expect(Reply).ToHaveTextAsync("You lowered your rating. Tell us what made October harder.", new() { Timeout = 5000 });
        await Expect(WhatChanged).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_rating_says_it_is_cleared_and_asks_for_a_star_again()
    {
        await OpenTheSurvey();
        await Rate(4);
        await Rate(2);
        await Expect(WhatChanged).ToBeVisibleAsync();

        await ClickWhenStable(ClearMyRating);

        await Expect(Rating.RatingList).ToHaveAttributeAsync("aria-valuenow", "0", new() { Timeout = 5000 });
        await Expect(Reply).ToHaveTextAsync("Your rating is cleared. Choose the stars that match October's care.");
        await Expect(WhatChanged).ToBeHiddenAsync();
        await Expect(_plan.ErrorFor(m => m.SatisfactionScore)).ToHaveTextAsync(ChooseAStar);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task rating_the_same_as_september_gives_three_stars()
    {
        await OpenTheSurvey();

        await ClickWhenStable(SameAsSeptember);

        await Expect(Rating.RatingList).ToHaveAttributeAsync("aria-valuenow", "3", new() { Timeout = 5000 });
        await Expect(Reply).ToHaveTextAsync("The same as September.");

        await ClickWhenStable(SendMyRating);
        await Expect(Recorded).ToHaveTextAsync(
            "Thank you, Margaret. We recorded 3 stars for October. Maple Grove's October average is now 4.1 stars from 24 residents.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_an_unrated_survey_asks_for_a_star_and_choosing_one_clears_the_request()
    {
        await OpenTheSurvey();

        await ClickWhenStable(SendMyRating);
        await Expect(_plan.ErrorFor(m => m.SatisfactionScore)).ToHaveTextAsync(ChooseAStar, new() { Timeout = 5000 });

        await Rate(4);

        await Expect(_plan.ErrorFor(m => m.SatisfactionScore)).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_five_stars_records_them_and_answers_with_octobers_average()
    {
        await OpenTheSurvey();
        await Rate(5);

        await ClickWhenStable(SendMyRating);

        await Expect(Recorded).ToHaveTextAsync(
            "Thank you, Margaret. We recorded 5 stars for October. Maple Grove's October average is now 4.2 stars from 24 residents.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_a_lowered_rating_passes_what_changed_to_the_care_manager()
    {
        await OpenTheSurvey();
        await Rate(4);
        await Rate(2);
        await Page.Locator($"#{_plan.FindComponent(m => m.WhatChanged)!.ElementId}").FillAsync("Call bells took a long time at night.");

        await ClickWhenStable(SendMyRating);

        await Expect(Recorded).ToHaveTextAsync(
            "Thank you, Margaret. We recorded 2 stars for October. Maple Grove's October average is now 4.0 stars from 24 residents. Your note goes to Maple wing's care manager.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
