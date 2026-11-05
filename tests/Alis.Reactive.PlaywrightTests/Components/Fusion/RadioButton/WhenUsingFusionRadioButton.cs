namespace Alis.Reactive.PlaywrightTests.Components.Fusion.RadioButton;

[TestFixture]
public class WhenUsingFusionRadioButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionRadioButton";
    private const string StudioId = "room-studio";
    private const string OneBedroomId = "room-one-bedroom";
    private const string CompanionId = "room-companion-suite";

    private FusionRadioButtonLocator Studio => new(Page, StudioId);
    private FusionRadioButtonLocator OneBedroom => new(Page, OneBedroomId);
    private FusionRadioButtonLocator CompanionSuite => new(Page, CompanionId);

    private ILocator ChosenRoom => Page.Locator("#chosen-room");
    private ILocator RoomDetail => Page.Locator("#room-detail");
    private ILocator CompanionAvailability => Page.Locator("#companion-availability");
    private ILocator DeskConfirmation => Page.Locator("#desk-confirmation");

    private ILocator ApplyLastAssessment => Page.Locator("#apply-last-assessment");
    private ILocator RecommendStudio => Page.Locator("#recommend-studio");
    private ILocator MarkCompanionFull => Page.Locator("#mark-companion-full");
    private ILocator ReopenCompanion => Page.Locator("#reopen-companion");
    private ILocator ConfirmMoveIn => Page.Locator("#confirm-move-in");

    private async Task OpenIntake()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Studio.Input).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_intake_opens_with_every_room_option_listed()
    {
        await OpenIntake();

        await Expect(Studio.Label).ToHaveTextAsync("Studio apartment");
        await Expect(OneBedroom.Label).ToHaveTextAsync("One-bedroom apartment");
        await Expect(CompanionSuite.Label).ToHaveTextAsync("Shared companion suite");
        await Expect(ChosenRoom).ToHaveTextAsync("No room chosen yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_companion_suite_shows_it_as_the_selected_room()
    {
        await OpenIntake();

        await CompanionSuite.Choose();

        await Expect(CompanionSuite.Input).ToBeCheckedAsync();
        await Expect(ChosenRoom).ToHaveTextAsync("Shared Companion Suite");
        await Expect(RoomDetail)
            .ToHaveTextAsync("A companion suite pairs two residents who prefer not to live alone.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_studio_shows_the_studio_as_the_selected_room()
    {
        await OpenIntake();

        await Studio.Choose();

        await Expect(Studio.Input).ToBeCheckedAsync();
        await Expect(ChosenRoom).ToHaveTextAsync("Studio Apartment");
        await Expect(RoomDetail)
            .ToHaveTextAsync("A studio suits a resident who wants a compact, easy-to-navigate space.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_her_last_assessment_selects_the_recommended_room()
    {
        await OpenIntake();

        await ApplyLastAssessment.ClickAsync();

        await Expect(CompanionSuite.Input).ToBeCheckedAsync(new() { Timeout = 5000 });
        await Expect(ChosenRoom).ToHaveTextAsync("Shared Companion Suite", new() { Timeout = 5000 });
        await Expect(RoomDetail)
            .ToHaveTextAsync("Her last assessment recommended the shared companion suite.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task marking_the_companion_suite_full_takes_it_off_the_list()
    {
        await OpenIntake();

        await MarkCompanionFull.ClickAsync();

        await Expect(CompanionSuite.Input).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(CompanionAvailability)
            .ToHaveTextAsync("The companion suite is full this month and can't be chosen.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reopening_the_companion_suite_puts_it_back_on_the_list()
    {
        await OpenIntake();

        await MarkCompanionFull.ClickAsync();
        await Expect(CompanionSuite.Input).ToBeDisabledAsync(new() { Timeout = 5000 });

        await ReopenCompanion.ClickAsync();

        await Expect(CompanionSuite.Input).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(CompanionAvailability).ToHaveTextAsync("The companion suite is open for move-in.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task taking_her_to_the_recommended_studio_selects_and_focuses_it()
    {
        await OpenIntake();

        await RecommendStudio.ClickAsync();

        await Expect(Studio.Input).ToBeCheckedAsync(new() { Timeout = 5000 });
        await Expect(ChosenRoom).ToHaveTextAsync("Studio Apartment", new() { Timeout = 5000 });
        await Expect(Studio.Input).ToBeFocusedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirming_an_available_room_shows_the_desk_confirmation()
    {
        await OpenIntake();

        await Studio.Choose();
        await ConfirmMoveIn.ClickAsync();

        await Expect(DeskConfirmation)
            .ToHaveTextAsync("Move-in confirmed: Studio Apartment.", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirming_the_companion_suite_posts_that_it_was_chosen()
    {
        await OpenIntake();

        await ApplyLastAssessment.ClickAsync();
        await Expect(CompanionSuite.Input).ToBeCheckedAsync(new() { Timeout = 5000 });

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/FusionRadioButton/ConfirmPlan") && request.Method == "POST",
            new() { Timeout = 10000 });

        await ConfirmMoveIn.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"Room\":\"Shared Companion Suite\""),
            "the gather pipeline must carry SelectedValue() under its declared key");
        Assert.That(body, Does.Contain("\"CompanionSuiteChosen\":true"),
            "the gather pipeline must carry the companion suite's Checked() source under its declared key");
        Assert.That(body, Does.Contain("\"CompanionSuiteUnavailable\":false"),
            "the gather pipeline must carry the companion suite's Disabled() source under its declared key");

        AssertNoConsoleErrors();
    }
}
