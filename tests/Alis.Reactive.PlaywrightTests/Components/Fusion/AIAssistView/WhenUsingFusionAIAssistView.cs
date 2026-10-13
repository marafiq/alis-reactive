using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.AIAssistView;

[TestFixture]
public class WhenUsingFusionAIAssistView : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AIAssistView";

    // The whole assistant fits in the window, so an item is in view exactly when the assistant's
    // own scrolling shows it.
    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.ViewportSize = new() { Width = 1280, Height = 1400 };
        return options;
    }

    private async Task NavigateAndWaitForAssistView()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Page.Locator("#care-ai")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-ai .e-assist-textarea"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task builder_renders_initial_prompt_suggestions()
    {
        await NavigateAndWaitForAssistView();

        await Expect(Page.Locator("#care-ai"))
            .ToContainTextAsync("Care prompts", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-ai"))
            .ToContainTextAsync("Draft family update", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_prompt_fires_typed_prompt_changed_event()
    {
        await NavigateAndWaitForAssistView();

        await Page.Locator("#care-ai .e-assist-textarea").FillAsync("fall risk update");

        await Expect(Page.Locator("#prompt-changed-value"))
            .ToHaveTextAsync("fall risk update", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task execute_prompt_calls_js_method_and_prompt_request_adds_text_response()
    {
        await NavigateAndWaitForAssistView();

        await Page.Locator("#execute-prompt-btn").ClickAsync();

        await Expect(Page.Locator("#command-status"))
            .ToHaveTextAsync("executePrompt called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#request-status"))
            .ToHaveTextAsync("promptRequest fired", new() { Timeout = 5000 });
        await Expect(Page.Locator("#request-prompt"))
            .ToHaveTextAsync("Summarize the medication changes", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-ai"))
            .ToContainTextAsync("Here is what the care team recorded about: Summarize the medication changes.", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task object_prompt_response_appends_prompt_and_response()
    {
        await NavigateAndWaitForAssistView();

        await Page.Locator("#append-object-response-btn").ClickAsync();

        await Expect(Page.Locator("#object-response-status"))
            .ToHaveTextAsync("object response added", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-ai"))
            .ToContainTextAsync("Care plan follow up", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-ai"))
            .ToContainTextAsync(
                "Schedule hydration checks and confirm the updated medication list with the nurse.",
                new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_prompt_mutates_component_and_prompt_source_reads_current_value()
    {
        await NavigateAndWaitForAssistView();

        await Page.Locator("#set-prompt-btn").ClickAsync();

        await Expect(Page.Locator("#current-prompt"))
            .ToHaveTextAsync("Prepare discharge summary", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-ai .e-assist-textarea"))
            .ToContainTextAsync("Prepare discharge summary", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task active_view_source_reads_current_view_index()
    {
        await NavigateAndWaitForAssistView();

        await Page.Locator("#read-active-view-btn").ClickAsync();

        await Expect(Page.Locator("#active-view"))
            .ToHaveTextAsync("0", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task jumping_to_the_latest_answer_scrolls_the_conversation_to_its_end()
    {
        await NavigateAndWaitForAssistView();
        for (var answer = 0; answer < 5; answer++)
            await Page.Locator("#append-object-response-btn").ClickAsync();
        var firstQuestion = Page.Locator("#care-ai #e-prompt-item_0");
        var latestAnswer = Page.Locator("#care-ai #e-response-item_4");
        await Expect(latestAnswer).ToContainTextAsync("Schedule hydration checks", new() { Timeout = 5000 });

        // The coordinator scrolls back up to the first question.
        await firstQuestion.HoverAsync();
        await Page.Mouse.WheelAsync(0, -5000);
        await Expect(firstQuestion).ToBeInViewportAsync(new() { Timeout = 5000 });

        await Page.Locator("#scroll-bottom-btn").ClickAsync();

        await Expect(latestAnswer).ToBeInViewportAsync(new() { Timeout = 5000 });
        await Expect(firstQuestion).Not.ToBeInViewportAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task stopping_an_answer_notes_the_question_it_belonged_to()
    {
        // The care team's answer is held, so the assistant is still answering when Stop is clicked.
        var answerReleased = new SemaphoreSlim(0);
        await Page.RouteAsync("**/Sandbox/Components/AIAssistView/Reply*", async route =>
        {
            await answerReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        await NavigateAndWaitForAssistView();
        var prompt = Page.Locator("#care-ai .e-assist-textarea");

        await prompt.FillAsync("Summarize Margaret's day");
        await prompt.PressAsync("Enter");
        await Page.Locator("#care-ai .e-assist-stop").ClickAsync(new() { Timeout = 10000 });

        await Expect(Page.Locator("#assist-stopped"))
            .ToHaveTextAsync("Stopped the answer to \"Summarize Margaret's day\".", new() { Timeout = 5000 });

        // Let the held answer arrive before the page closes; the stopped assistant does not show it.
        await Page.RunAndWaitForResponseAsync(
            () => { answerReleased.Release(); return Task.CompletedTask; },
            "**/Sandbox/Components/AIAssistView/Reply*");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_declares_typed_runtime_members()
    {
        await NavigateAndWaitForAssistView();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("care-ai"));
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("executePrompt"));
        // Two C# members type two signatures of the JavaScript addPromptResponse method; both call
        // the one JavaScript member, each with its own argument shapes.
        Assert.That(planJson, Does.Contain("\"addPromptResponse\""));
        Assert.That(planJson, Does.Contain("scrollToBottom"));
        Assert.That(planJson, Does.Contain("prompt"));
        Assert.That(planJson, Does.Contain("activeView"));
        Assert.That(planJson, Does.Match(new Regex("promptChanged|promptRequest|stopRespondingClick")));

        AssertNoConsoleErrors();
    }
}
