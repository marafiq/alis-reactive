namespace Alis.Reactive.PlaywrightTests.Events;

/// <summary>
/// Verifies trace output proves two-phase boot ordering.
/// The most important test here is trace_shows_phase_1_then_phase_2 — it catches
/// the subtle bug where someone accidentally removes two-phase boot, causing dispatch
/// chains to silently break (events fire before listeners exist).
/// </summary>
[TestFixture]
public class WhenTraceIsEnabled : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Events";

    [Test]
    public async Task boot_trace_appears_in_console()
    {
        // data-trace="trace" on the plan element enables full tracing.
        // If auto-boot fails to read data-trace, or trace.setLevel breaks,
        // no [alis:boot] messages appear.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var hasBootStart = _consoleMessages.Any(m => m.Contains("[alis:boot]") && m.Contains("booting"));
        var hasBootEnd = _consoleMessages.Any(m => m.Contains("[alis:boot]") && m.Contains("booted"));

        Assert.That(hasBootStart, Is.True, "Boot start trace must appear in console");
        Assert.That(hasBootEnd, Is.True, "Boot complete trace must appear in console");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trace_shows_phase_1_then_phase_2()
    {
        // THIS IS THE CRITICAL TWO-PHASE BOOT REGRESSION TEST.
        //
        // Two-phase boot guarantees: custom-event listeners wire BEFORE dom-ready executes.
        // Without this, dom-ready dispatches "test" but nobody is listening yet — chain breaks silently.
        //
        // Phase 1: trigger.ts logs "[alis:trigger] custom-event: listening" for each custom-event entry
        // Phase 2: dom-ready executes, which triggers "[alis:command] dispatch" for the first dispatch
        //
        // If someone refactors boot.ts to remove two-phase ordering, the dispatch trace would
        // appear BEFORE the listening traces — and this test catches it.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var messages = _consoleMessages;

        // Find the LAST custom-event listener wiring (phase 1 must complete fully)
        var lastWireIndex = -1;
        for (var i = 0; i < messages.Count; i++)
        {
            if (messages[i].Contains("[alis:trigger]") && messages[i].Contains("custom-event"))
                lastWireIndex = i;
        }

        // Find the FIRST dispatch command (phase 2 starts when dom-ready executes)
        var firstDispatchIndex = messages.FindIndex(m =>
            m.Contains("[alis:command]") && m.Contains("dispatch"));

        Assert.That(lastWireIndex, Is.GreaterThanOrEqualTo(0),
            "Custom-event listener wiring must be traced (phase 1)");
        Assert.That(firstDispatchIndex, Is.GreaterThanOrEqualTo(0),
            "Dispatch command must be traced (phase 2)");
        Assert.That(lastWireIndex, Is.LessThan(firstDispatchIndex),
            "All custom-event listeners must wire (phase 1) BEFORE dom-ready dispatches (phase 2). " +
            "If this fails, two-phase boot is broken — dispatch chains will silently fail.");
    }

    [Test]
    public async Task trace_captures_all_three_dispatch_events()
    {
        // Every dispatch in the chain must produce a trace entry.
        // If the runtime skips tracing for any dispatch, we lose observability.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        AssertTraceContains("command", "\"event\":\"test\"");
        AssertTraceContains("command", "\"event\":\"test-received\"");
        AssertTraceContains("command", "\"event\":\"final\"");

        AssertNoConsoleErrors();
    }
}
