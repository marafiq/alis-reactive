using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.AIAssistView;

[TestFixture]
public class WhenUsingFusionAIAssistView : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AIAssistView";

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
            .ToContainTextAsync("Reactive response added from promptRequest.", new() { Timeout = 5000 });

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
    public async Task scroll_to_bottom_calls_js_method()
    {
        await NavigateAndWaitForAssistView();

        await Page.Locator("#append-object-response-btn").ClickAsync();
        await Page.Locator("#scroll-bottom-btn").ClickAsync();

        await Expect(Page.Locator("#scroll-status"))
            .ToHaveTextAsync("scrollToBottom called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#care-ai"))
            .ToContainTextAsync("Care plan follow up", new() { Timeout = 5000 });

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
        Assert.That(planJson, Does.Contain("addPromptResponseText"));
        Assert.That(planJson, Does.Contain("addPromptResponseObject"));
        Assert.That(planJson, Does.Contain("scrollToBottom"));
        Assert.That(planJson, Does.Contain("prompt"));
        Assert.That(planJson, Does.Contain("activeView"));
        Assert.That(planJson, Does.Match(new Regex("promptChanged|promptRequest|stopRespondingClick")));

        AssertNoConsoleErrors();
    }
}
