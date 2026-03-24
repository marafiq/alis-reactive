namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

/// <summary>
/// Verifies the three-hop dispatch chain: dom-ready -> "test" -> "test-received" -> "final".
/// If two-phase boot breaks, the chain silently stops. If dispatch breaks, DOM never updates.
/// If expression path resolution breaks, payload data never reaches the DOM.
/// These tests catch all three failure modes via DOM assertions.
/// </summary>
[TestFixture]
public class WhenEventsChainAcrossListeners : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CoreBehaviors/Events";

    [Test]
    public async Task three_hop_chain_completes_in_order()
    {
        // If any hop in the chain breaks (boot ordering, dispatch wiring, reaction execution),
        // one or more steps will still show "waiting..." text instead of completion text.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var step1 = Page.Locator("#step-1");
        var step2 = Page.Locator("#step-2");
        var step3 = Page.Locator("#step-3");

        // Each step's text proves the PREVIOUS dispatch arrived and the reaction executed
        await Expect(step1).ToContainTextAsync("dom-ready fired");
        await Expect(step2).ToContainTextAsync("\"test\" received");
        await Expect(step3).ToContainTextAsync("\"test-received\" received");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task payload_survives_dispatch_chain()
    {
        // Entry 3 dispatches "final" with payload { eventName: "done" }.
        // The trace must carry that payload — if expression path serialization or
        // dispatch payload propagation breaks, this field disappears from the trace.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var finalDispatch = _consoleMessages
            .FirstOrDefault(m => m.Contains("[alis:command]")
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
        // Entry 4 listens for "final" and mutates chain-status:
        //   RemoveClass("text-text-muted") + AddClass("text-green-600") + AddClass("font-semibold") + SetText(...)
        // If the final event never fires (broken chain) OR mutations fail, this element stays gray.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#chain-status");

        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("font-semibold"));
        await Expect(status).ToContainTextAsync("Chain complete");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_mutations_preserve_class_coherence()
    {
        // Entry 4 does RemoveClass("text-text-muted") then AddClass("text-green-600").
        // If RemoveClass fails silently, the element would have BOTH classes — conflicting styles.
        // If AddClass fails, it would have neither green class.
        // This test verifies the mutation pair works atomically.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#chain-status");
        var classAttr = await status.GetAttributeAsync("class") ?? "";

        Assert.That(classAttr, Does.Contain("text-green-600"),
            "AddClass('text-green-600') must have applied");
        Assert.That(classAttr, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have removed the initial muted class");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dispatches_occur_in_chronological_order_in_trace()
    {
        // The three dispatches must appear in trace in causal order:
        // test -> test-received -> final.
        // If boot executes dom-ready before wiring listeners, the chain may partially fire
        // or fire out of order. This test catches ordering regressions.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var dispatches = _consoleMessages
            .Where(m => m.Contains("[alis:command]") && m.Contains("dispatch"))
            .ToList();

        var testIdx = dispatches.FindIndex(m =>
            m.Contains("\"event\":\"test\"") && !m.Contains("test-received"));
        var receivedIdx = dispatches.FindIndex(m =>
            m.Contains("\"event\":\"test-received\""));
        var finalIdx = dispatches.FindIndex(m =>
            m.Contains("\"event\":\"final\""));

        Assert.That(testIdx, Is.GreaterThanOrEqualTo(0), "test dispatch must be traced");
        Assert.That(receivedIdx, Is.GreaterThan(testIdx),
            "test-received must dispatch after test");
        Assert.That(finalIdx, Is.GreaterThan(receivedIdx),
            "final must dispatch after test-received");
    }

    [Test]
    public async Task all_three_steps_have_green_class_after_chain_completes()
    {
        // Each hop in the chain calls AddClass("text-green-600") on its step element.
        // If any hop fails to execute its mutation (broken dispatch wiring, missing element,
        // or reaction abort), that step's element will NOT have text-green-600.
        // Verifying all three proves every hop executed its AddClass mutation.
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
        // Entry 4 (on "final") calls RemoveClass("text-text-muted") on #chain-status
        // before calling AddClass("text-green-600"). If RemoveClass fails silently,
        // #chain-status would have BOTH muted and green — conflicting visual states.
        // This test, combined with chain_status_turns_green_on_completion, proves
        // the full remove+add mutation pair executed correctly across the entire chain.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var chainStatus = Page.Locator("#chain-status");
        var classAttr = await chainStatus.GetAttributeAsync("class") ?? "";

        Assert.That(classAttr, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have executed on #chain-status — " +
            "proves the 'final' event arrived AND the mutation pipeline ran in order");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chain_status_has_semibold_and_green_and_no_muted_class()
    {
        // The chain-status element receives a 3-mutation sequence from Entry 4 (on "final"):
        //   1. RemoveClass("text-text-muted")  — strip initial gray
        //   2. AddClass("text-green-600")      — apply success color
        //   3. AddClass("font-semibold")       — apply emphasis
        //
        // This test verifies the COMPLETE final class state in a single assertion block.
        // A missed RemoveClass leaves stale "text-text-muted" (conflicting styles).
        // A missed AddClass leaves the element without visual success cues.
        // Wrong ordering (e.g., AddClass before RemoveClass) could theoretically work,
        // but if the runtime reorders or skips mutations, this catches it.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#chain-status");
        var classAttr = await status.GetAttributeAsync("class") ?? "";

        Assert.Multiple(() =>
        {
            Assert.That(classAttr, Does.Contain("text-green-600"),
                "AddClass('text-green-600') must have applied — success color after full 3-hop chain");
            Assert.That(classAttr, Does.Contain("font-semibold"),
                "AddClass('font-semibold') must have applied — emphasis after full 3-hop chain");
            Assert.That(classAttr, Does.Not.Contain("text-text-muted"),
                "RemoveClass('text-text-muted') must have stripped the initial muted class — " +
                "stale class would cause conflicting green+muted styles");
        });

        AssertNoConsoleErrors();
    }
}
