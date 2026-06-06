namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

// Dispatch chain under test: dom-ready -> "test" -> "test-received" -> "final".
[TestFixture]
public class WhenEventsChainAcrossListeners : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CoreBehaviors/Events";

    [Test]
    public async Task three_hop_chain_completes_in_order()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var step1 = Page.Locator("#step-1");
        var step2 = Page.Locator("#step-2");
        var step3 = Page.Locator("#step-3");

        await Expect(step1).ToContainTextAsync("dom-ready fired");
        await Expect(step2).ToContainTextAsync("\"test\" received");
        await Expect(step3).ToContainTextAsync("\"test-received\" received");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task final_dispatch_trace_contains_chained_literal_payload()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var finalDispatch = _consoleMessages
            .FirstOrDefault(m => m.Contains("[alis:execute]")
                                 && m.Contains("dispatch")
                                 && m.Contains("\"event\":\"final\""));

        Assert.That(finalDispatch, Is.Not.Null,
            "final dispatch must be traced");
        Assert.That(finalDispatch, Does.Contain("eventName"),
            "payload.eventName must survive the dispatch chain");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chain_status_turns_green_on_completion()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#chain-status");

        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("font-semibold"));
        await Expect(status).ToContainTextAsync("Chain complete");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_updates_preserve_class_coherence()
    {
        // Text assertions do not expose stale classes; this catches remove/add drift.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#chain-status");
        var statusClasses = await status.GetAttributeAsync("class") ?? "";

        Assert.That(statusClasses, Does.Contain("text-green-600"),
            "AddClass('text-green-600') must have applied");
        Assert.That(statusClasses, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have removed the initial muted class");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dispatches_occur_in_chronological_order_in_trace()
    {
        // If boot executes dom-ready before wiring listeners, the chain may partially fire
        // or fire out of order.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var dispatches = _consoleMessages
            .Where(m => m.Contains("[alis:execute]") && m.Contains("dispatch"))
            .ToList();

        var testDispatchIndex = dispatches.FindIndex(m =>
            m.Contains("\"event\":\"test\"") && !m.Contains("test-received"));
        var receivedDispatchIndex = dispatches.FindIndex(m =>
            m.Contains("\"event\":\"test-received\""));
        var finalDispatchIndex = dispatches.FindIndex(m =>
            m.Contains("\"event\":\"final\""));

        Assert.That(testDispatchIndex, Is.GreaterThanOrEqualTo(0), "test dispatch must be traced");
        Assert.That(receivedDispatchIndex, Is.GreaterThan(testDispatchIndex),
            "test-received must dispatch after test");
        Assert.That(finalDispatchIndex, Is.GreaterThan(receivedDispatchIndex),
            "final must dispatch after test-received");
    }

    [Test]
    public async Task all_three_steps_have_green_class_after_chain_completes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var step1 = Page.Locator("#step-1");
        var step2 = Page.Locator("#step-2");
        var step3 = Page.Locator("#step-3");

        await Expect(step1).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(step2).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(step3).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chain_status_has_no_muted_class_after_chain_completes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var chainStatus = Page.Locator("#chain-status");
        var statusClasses = await chainStatus.GetAttributeAsync("class") ?? "";

        Assert.That(statusClasses, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have executed on #chain-status — " +
            "proves the 'final' event arrived AND the element update pipeline ran in order");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chain_status_has_semibold_and_green_and_no_muted_class()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#chain-status");
        var statusClasses = await status.GetAttributeAsync("class") ?? "";

        Assert.Multiple(() =>
        {
            Assert.That(statusClasses, Does.Contain("text-green-600"),
                "AddClass('text-green-600') must have applied — success color after full 3-hop chain");
            Assert.That(statusClasses, Does.Contain("font-semibold"),
                "AddClass('font-semibold') must have applied — emphasis after full 3-hop chain");
            Assert.That(statusClasses, Does.Not.Contain("text-text-muted"),
                "RemoveClass('text-text-muted') must have stripped the initial muted class — " +
                "stale class would cause conflicting green+muted styles");
        });

        AssertNoConsoleErrors();
    }
}
