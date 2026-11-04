namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Carousel;

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

    [Test]
    public async Task review_opens_on_the_first_care_plan_section()
    {
        await OpenReview();

        await Expect(Review.ActiveSectionTitle).ToHaveTextAsync("Medications");
        await Expect(Position).ToHaveTextAsync("Section 1 of 3: Medications");

        AssertNoConsoleErrors();
    }

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

    [Test]
    public async Task stepping_back_a_section_says_it_came_from_the_later_section()
    {
        await OpenReview();

        await NextButton.ClickAsync();
        await Expect(Section).ToHaveTextAsync("Therapy Goals");
        await NextButton.ClickAsync();
        await Expect(Section).ToHaveTextAsync("Discharge Steps");

        await PreviousButton.ClickAsync();

        await Expect(Review.ActiveSectionTitle).ToHaveTextAsync("Therapy Goals");
        await Expect(CameFrom).ToHaveTextAsync("after the discharge steps");
        await Expect(Movement).ToHaveTextAsync("You went back a section.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reaching_a_section_with_the_buttons_is_recorded_as_a_button_move()
    {
        await OpenReview();

        await NextButton.ClickAsync();

        await Expect(Method).ToHaveTextAsync("Reached using the navigation buttons.");
        await Expect(ChartLine).ToContainTextAsync("using the buttons", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_medications_signoff_stays_locked_when_stepping_back_to_it()
    {
        await OpenReview();

        await NextButton.ClickAsync();
        await Expect(Section).ToHaveTextAsync("Therapy Goals");

        await PreviousButton.ClickAsync();

        await Expect(Review.ActiveSectionTitle).ToHaveTextAsync("Therapy Goals");
        await Expect(Position).ToHaveTextAsync("Section 2 of 3: Therapy Goals");
        await Expect(GateNotice)
            .ToHaveTextAsync("The medications sign-off is locked. Continue forward through the care plan.");
        await Expect(GateDirection).ToHaveTextAsync("You tried to go back to the medications section.");
        await Expect(GateFrom).ToHaveTextAsync("You are still on the therapy goals section.");
        await Expect(GateMethod).ToHaveTextAsync("That button press was not applied.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_a_section_posts_the_slide_change_payload_to_the_server()
    {
        await OpenReview();

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/Carousel/Record") && request.Method == "POST",
            new() { Timeout = 10000 });

        await NextButton.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"SectionIndex\":1"),
            "the gather pipeline must carry SlideChanged.CurrentIndex under its declared key");
        Assert.That(body, Does.Contain("\"CameFromIndex\":0"),
            "the gather pipeline must carry SlideChanged.PreviousIndex under its declared key");
        Assert.That(body, Does.Contain("\"Direction\":\"Next\""),
            "the gather pipeline must carry SlideChanged.SlideDirection under its declared key");
        Assert.That(body, Does.Contain("\"BySwipe\":false"),
            "the gather pipeline must carry SlideChanged.IsSwiped under its declared key");

        AssertNoConsoleErrors();
    }
}
