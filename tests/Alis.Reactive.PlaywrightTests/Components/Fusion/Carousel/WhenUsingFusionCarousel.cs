using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Carousel;

// Journey: a nurse walks a resident through their Guided Care-Plan Review.
// The review opens on the Medications section, the nurse moves forward and back
// through the sections with the navigation buttons, each section reached is
// recorded to the chart, and the medications sign-off stays locked once the
// review has moved past it.
[TestFixture]
public class WhenUsingFusionCarousel : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Carousel";

    private FusionCarouselLocator Review => new(Page, "care-plan-review");
    private ILocator Position => Page.Locator("#review-position");
    private ILocator Section => Page.Locator("#review-section");
    private ILocator CameFrom => Page.Locator("#review-came-from");
    private ILocator Movement => Page.Locator("#review-movement");
    private ILocator Method => Page.Locator("#review-method");
    private ILocator ChartLine => Page.Locator("#chart-line");
    private ILocator GateNotice => Page.Locator("#gate-notice");
    private ILocator GateDirection => Page.Locator("#gate-direction");
    private ILocator GateFrom => Page.Locator("#gate-from");
    private ILocator GateMethod => Page.Locator("#gate-method");
    private ILocator NextButton => Page.Locator("#review-next");
    private ILocator PreviousButton => Page.Locator("#review-previous");

    private async Task OpenReview()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Review.ActiveSlide).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionCarousel builder renders the review on the first section,
    // and SelectedIndex() read on load reports which section the resident is on.
    [Test]
    public async Task review_opens_on_the_first_care_plan_section()
    {
        await OpenReview();

        await Expect(Review.ActiveSectionTitle).ToHaveTextAsync("Medications");
        await Expect(Position).ToHaveTextAsync("Section 1 of 3: Medications");

        AssertNoConsoleErrors();
    }

    // INTERACTS — Next() advances the carousel; SlideChanged fires through the .Reactive
    // wiring; SlideChanged.CurrentIndex names the section reached; SelectedIndex() re-read
    // reports the new position; and the gathered move is recorded to the chart.
    [Test]
    public async Task advancing_to_the_next_section_shows_it_and_records_it_to_the_chart()
    {
        await OpenReview();

        await NextButton.ClickAsync();

        await Expect(Review.ActiveSectionTitle).ToHaveTextAsync("Therapy Goals");
        await Expect(Section).ToHaveTextAsync("Therapy Goals");
        await Expect(Position).ToHaveTextAsync("Section 2 of 3: Therapy Goals");
        await Expect(ChartLine)
            .ToHaveTextAsync("Recorded: moved forward to Therapy Goals (from Medications), using the buttons.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // SlideChanged.PreviousIndex carries the section the resident came from, and
    // SlideChanged.SlideDirection reports a Previous move when the nurse steps back.
    [Test]
    public async Task stepping_back_a_section_says_it_came_from_the_later_section()
    {
        await OpenReview();

        await NextButton.ClickAsync();                 // Medications -> Therapy Goals
        await Expect(Section).ToHaveTextAsync("Therapy Goals");
        await NextButton.ClickAsync();                 // Therapy Goals -> Discharge Steps
        await Expect(Section).ToHaveTextAsync("Discharge Steps");

        await PreviousButton.ClickAsync();             // Discharge Steps -> Therapy Goals

        await Expect(Review.ActiveSectionTitle).ToHaveTextAsync("Therapy Goals");
        await Expect(CameFrom).ToHaveTextAsync("after the discharge steps");
        await Expect(Movement).ToHaveTextAsync("You went back a section.");

        AssertNoConsoleErrors();
    }

    // SlideChanged.IsSwiped distinguishes a button move (false) from a swipe; reaching a
    // section with the navigation buttons reports a button move on screen and in the chart.
    [Test]
    public async Task reaching_a_section_with_the_buttons_is_recorded_as_a_button_move()
    {
        await OpenReview();

        await NextButton.ClickAsync();

        await Expect(Method).ToHaveTextAsync("Reached using the navigation buttons.");
        await Expect(ChartLine).ToContainTextAsync("using the buttons", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // SlideChanging guards the move before it happens: a step back onto the medications
    // section (NextIndex 0) is cancelled by PreventTransition (the payload Cancel write),
    // so the carousel stays put; SlideChanging.SlideDirection and IsSwiped explain the block.
    [Test]
    public async Task the_medications_signoff_stays_locked_when_stepping_back_to_it()
    {
        await OpenReview();

        await NextButton.ClickAsync();                 // Medications -> Therapy Goals
        await Expect(Section).ToHaveTextAsync("Therapy Goals");

        await PreviousButton.ClickAsync();             // Therapy Goals -> (would be) Medications: blocked

        // PreventTransition cancelled the move: the carousel never left Therapy Goals.
        await Expect(Review.ActiveSectionTitle).ToHaveTextAsync("Therapy Goals");
        await Expect(Position).ToHaveTextAsync("Section 2 of 3: Therapy Goals");
        // The nurse is told the medications sign-off is locked, and why the move was rejected.
        await Expect(GateNotice)
            .ToHaveTextAsync("The medications sign-off is locked. Continue forward through the care plan.");
        await Expect(GateDirection).ToHaveTextAsync("You tried to go back to the medications section.");
        await Expect(GateFrom).ToHaveTextAsync("You are still on the therapy goals section.");
        await Expect(GateMethod).ToHaveTextAsync("That button press was not applied.");

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries the SlideChanged payload members into
    // the POST body under their declared keys. (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task recording_a_section_posts_the_slide_change_payload_to_the_server()
    {
        await OpenReview();

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/Carousel/Record") && request.Method == "POST",
            new() { Timeout = 10000 });

        await NextButton.ClickAsync();                 // Medications (0) -> Therapy Goals (1)

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"sectionIndex\":1"),
            "the gather pipeline must carry SlideChanged.CurrentIndex under its declared key");
        Assert.That(body, Does.Contain("\"cameFromIndex\":0"),
            "the gather pipeline must carry SlideChanged.PreviousIndex under its declared key");
        Assert.That(body, Does.Contain("\"direction\":\"Next\""),
            "the gather pipeline must carry SlideChanged.SlideDirection under its declared key");
        Assert.That(body, Does.Contain("\"bySwipe\":false"),
            "the gather pipeline must carry SlideChanged.IsSwiped under its declared key");

        AssertNoConsoleErrors();
    }
}
