namespace Alis.Reactive.PlaywrightTests.Components.Fusion.AIAssistView;

/// <summary>
/// As a care coordinator, I want to ask the community's assistant about a resident's care-plan follow-ups
/// and read the care team's answer in the conversation, so that I can plan her week without paging through
/// her chart.
/// </summary>
/// <remarks>
/// The page is Margaret Hale's assistant. The server answers each question from the care team's records and
/// ends by naming the common questions not asked. The coordinator can stop an answer (the question goes back
/// in the box and the page says which question stopped), bring the night shift's handoff into the
/// conversation, send the typed question to the charge nurse instead, and open the notes the care team saved
/// in place of the conversation. Her chart links here with a question, which is asked on arrival.
/// </remarks>
[TestFixture]
public class WhenUsingFusionAIAssistView : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AIAssistView";
    private const string AnswerUrl = "**/Sandbox/Components/AIAssistView/Answer";

    private const string FollowUps = "What follow-ups are due this week?";
    private const string FallRisk = "List fall-risk interventions";
    private const string FollowUpsAnswer =
        "Margaret's follow-ups this week: a hydration check-in on Wednesday, her physical therapy re-evaluation on Thursday and a podiatry visit on Friday.";
    private const string FallRiskAnswer =
        "Margaret's fall-risk interventions: a bed alarm at night, non-slip socks and her walker within reach.";
    private const string PodiatryQuestion = "When is the podiatry visit?";
    private const string PodiatryAnswer = "Margaret's podiatry visit is Friday at 2 p.m. with Dr. Lin.";
    private const string HandoffQuestion = "What did the night shift hand off?";
    private const string HandoffAnswer =
        "Priya Shah, night nurse: Margaret woke at 2 a.m. asking for Ellen and settled after warm milk. She drank two glasses of water overnight.";

    private FusionAIAssistViewLocator Assistant => new(Page, "care-ai");
    private ILocator NotesToggle => Page.Locator("#notes-toggle-btn");
    private ILocator SavedNotes => Page.Locator("#saved-notes");
    private ILocator AddHandoff => Page.Locator("#add-handoff-btn");
    private ILocator AskNurse => Page.Locator("#ask-nurse-btn");
    private ILocator Cleared => Page.Locator("#question-cleared");
    private ILocator Status => Page.Locator("#assistant-status");

    // The whole assistant fits in the window, so an item is in view exactly when the assistant's own
    // scrolling shows it.
    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.ViewportSize = new() { Width = 1280, Height = 1400 };
        return options;
    }

    private async Task NavigateAndBoot(string path = Path)
    {
        await NavigateToAndWaitForBoot(path);
        await Expect(Assistant.PromptBox).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task Ask(string question)
    {
        await Assistant.PromptBox.FillAsync(question);
        await Assistant.PromptBox.PressAsync("Enter");
    }

    private async Task AskTheFollowUpsFromTheSuggestions()
    {
        await ClickWhenStable(Assistant.Suggestion(FollowUps));
        await Expect(Assistant.Responses.First).ToContainTextAsync(FollowUpsAnswer, new() { Timeout = 10000 });
    }

    [Test]
    public async Task a_typed_question_sent_with_enter_is_answered_from_the_care_teams_records()
    {
        await NavigateAndBoot();

        await Ask(PodiatryQuestion);

        await Expect(Assistant.Prompts).ToHaveTextAsync(new[] { PodiatryQuestion }, new() { Timeout = 10000 });
        await Expect(Assistant.Responses.First).ToHaveTextAsync(
            PodiatryAnswer + " You can also ask “What follow-ups are due this week?”, “Draft a family update” or “List fall-risk interventions”.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_question_the_records_do_not_cover_is_answered_as_such()
    {
        await NavigateAndBoot();

        await Ask("Does Margaret like jazz on Sundays?");

        await Expect(Assistant.Responses.First).ToContainTextAsync(
            "The care team has no notes on “Does Margaret like jazz on Sundays?” yet.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_common_question_is_asked_with_one_click_and_its_answer_names_the_others()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Assistant.Suggestion(FallRisk));

        await Expect(Assistant.Prompts).ToHaveTextAsync(new[] { FallRisk }, new() { Timeout = 10000 });
        await Expect(Assistant.Responses.First).ToHaveTextAsync(
            FallRiskAnswer + " You can also ask “What follow-ups are due this week?” or “Draft a family update”.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_an_answer_arrives_the_next_question_can_be_asked()
    {
        await NavigateAndBoot();
        await AskTheFollowUpsFromTheSuggestions();
        await Expect(Assistant.StopResponding).ToHaveCountAsync(0);

        await Ask(PodiatryQuestion);

        await Expect(Assistant.Prompts).ToHaveTextAsync(new[] { FollowUps, PodiatryQuestion }, new() { Timeout = 10000 });
        await Expect(Assistant.Responses.Nth(1)).ToContainTextAsync(PodiatryAnswer, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task arriving_from_margarets_chart_asks_its_question_straight_away()
    {
        await NavigateAndBoot($"{Path}?ask={Uri.EscapeDataString(FollowUps)}");

        await Expect(Assistant.Prompts).ToHaveTextAsync(new[] { FollowUps }, new() { Timeout = 10000 });
        await Expect(Assistant.Responses.First).ToContainTextAsync(FollowUpsAnswer, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_a_typed_question_names_it_until_another_is_typed()
    {
        await NavigateAndBoot();
        await Assistant.PromptBox.FillAsync(FollowUps);
        await Expect(Cleared).ToBeHiddenAsync();

        await Assistant.PromptBox.FillAsync("");

        await Expect(Cleared).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Cleared).ToHaveTextAsync("Cleared “What follow-ups are due this week?”.");

        await Assistant.PromptBox.FillAsync("Draft a family update");
        await Expect(Cleared).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_the_night_handoff_puts_its_question_and_answer_in_the_conversation()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AddHandoff);

        await Expect(Assistant.Prompts).ToHaveTextAsync(new[] { HandoffQuestion }, new() { Timeout = 5000 });
        await Expect(Assistant.Responses).ToHaveTextAsync(new[] { HandoffAnswer });
        await Expect(AddHandoff).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_question_about_the_night_handoff_is_answered_once_it_is_in_the_conversation()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AddHandoff);
        await Expect(Assistant.Responses).ToHaveTextAsync(new[] { HandoffAnswer }, new() { Timeout = 5000 });

        await Ask("Did Margaret drink enough overnight?");

        await Expect(Assistant.Prompts).ToHaveTextAsync(
            new[] { HandoffQuestion, "Did Margaret drink enough overnight?" }, new() { Timeout = 10000 });
        await Expect(Assistant.Responses.Nth(1)).ToContainTextAsync(
            "Margaret drank two glasses of water overnight; the night nurse asks that fluids be offered again at breakfast.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_night_handoff_added_after_a_long_conversation_is_scrolled_into_view()
    {
        await NavigateAndBoot();
        await AskTheFollowUpsFromTheSuggestions();
        await Ask(PodiatryQuestion);
        await Expect(Assistant.Responses.Nth(1)).ToContainTextAsync(PodiatryAnswer, new() { Timeout = 10000 });
        var firstQuestion = Assistant.Prompts.First;

        // The coordinator scrolls the conversation back up to the first question.
        await Assistant.Responses.Nth(1).HoverAsync();
        await Page.Mouse.WheelAsync(0, -5000);
        await Expect(firstQuestion).ToBeInViewportAsync(new() { Timeout = 5000 });

        await ClickWhenStable(AddHandoff);

        await Expect(Assistant.Responses.Last).ToHaveTextAsync(HandoffAnswer, new() { Timeout = 5000 });
        await Expect(Assistant.Responses.Last).ToBeInViewportAsync(new() { Timeout = 5000 });
        await Expect(firstQuestion).Not.ToBeInViewportAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_saved_notes_shows_the_care_teams_notes_in_place_of_the_conversation()
    {
        await NavigateAndBoot();

        await ClickWhenStable(NotesToggle);

        await Expect(SavedNotes).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SavedNotes.Locator("li")).ToHaveTextAsync(new[]
        {
            "Offer fluids every two hours while Margaret is awake.",
            "Physical therapy is Tuesday and Thursday at 10 a.m.",
            "Call her daughter Ellen after any fall, day or night."
        });
        await Expect(Assistant.PromptBox).ToBeHiddenAsync();
        await Expect(NotesToggle).ToHaveTextAsync("Back to the conversation");
        await Expect(AskNurse).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task going_back_from_the_notes_shows_the_conversation_as_it_was()
    {
        await NavigateAndBoot();
        await AskTheFollowUpsFromTheSuggestions();
        await ClickWhenStable(NotesToggle);
        await Expect(SavedNotes).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(NotesToggle);

        await Expect(SavedNotes).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Assistant.Responses.First).ToContainTextAsync(FollowUpsAnswer);
        await Expect(Assistant.PromptBox).ToBeVisibleAsync();
        await Expect(NotesToggle).ToHaveTextAsync("Open saved notes");
        await Expect(AskNurse).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task stopping_an_answer_puts_the_question_back_in_the_box_and_says_which_question_stopped()
    {
        await NavigateAndBoot();
        await AskTheFollowUpsFromTheSuggestions();
        // The care team's answer to the next question is held, so the assistant is still answering when
        // Stop is clicked.
        var answerReleased = new SemaphoreSlim(0);
        await Page.RouteAsync(AnswerUrl, async route =>
        {
            await answerReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        await Ask("Is the walker check done?");
        await Expect(Assistant.Prompts.Nth(1)).ToHaveTextAsync("Is the walker check done?", new() { Timeout = 5000 });

        await ClickWhenStable(Assistant.StopResponding);

        await Expect(Status).ToHaveTextAsync(
            "Stopped the answer to question 2. Edit it below and send it again.", new() { Timeout = 5000 });
        await Expect(Assistant.PromptBox).ToHaveTextAsync("Is the walker check done?");

        // Let the held answer arrive before the page closes.
        await Page.RunAndWaitForResponseAsync(
            () => { answerReleased.Release(); return Task.CompletedTask; },
            AnswerUrl);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_typed_question_goes_to_the_charge_nurse_instead_and_leaves_the_box()
    {
        await NavigateAndBoot();
        await Assistant.PromptBox.FillAsync("Can Margaret skip physical therapy on Thursday?");

        await ClickWhenStable(AskNurse);

        await Expect(Status).ToHaveTextAsync(
            "Sent to Dana Ruiz, charge nurse on Maple: “Can Margaret skip physical therapy on Thursday?”",
            new() { Timeout = 5000 });
        await Expect(Assistant.PromptBox).ToHaveTextAsync("");
        await Expect(Assistant.Prompts).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_charge_nurse_is_not_sent_an_empty_question()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AskNurse);

        await Expect(Status).ToHaveTextAsync("Type the question for the charge nurse first.", new() { Timeout = 5000 });
        // The browser logs the refused request's 400; nothing else may fail.
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
