namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

// Trace coverage protects two-phase boot: listeners must wire before dom-ready dispatches.
[TestFixture]
public class WhenTraceReportsExecution : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CoreBehaviors/Events";

    [Test]
    public async Task boot_trace_appears_in_console()
    {
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
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var messages = _consoleMessages;

        var lastListenerTraceIndex = -1;
        for (var messageIndex = 0; messageIndex < messages.Count; messageIndex++)
        {
            if (messages[messageIndex].Contains("[alis:trigger]") && messages[messageIndex].Contains("document-event"))
                lastListenerTraceIndex = messageIndex;
        }

        var firstDispatchTraceIndex = messages.FindIndex(m =>
            m.Contains("[alis:execute] dispatch {"));

        Assert.That(lastListenerTraceIndex, Is.GreaterThanOrEqualTo(0),
            "Document-event listener wiring must be traced (phase 1)");
        Assert.That(firstDispatchTraceIndex, Is.GreaterThanOrEqualTo(0),
            "Dispatch reaction must be traced (phase 2)");
        Assert.That(lastListenerTraceIndex, Is.LessThan(firstDispatchTraceIndex),
            "All document-event listeners must wire (phase 1) BEFORE dom-ready dispatches (phase 2). " +
            "If this fails, two-phase boot is broken — dispatch chains will silently fail.");
    }

    [Test]
    public async Task trace_captures_all_three_dispatch_events()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        AssertTraceContains("execute", "\"event\":\"test\"");
        AssertTraceContains("execute", "\"event\":\"test-received\"");
        AssertTraceContains("execute", "\"event\":\"final\"");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trace_captures_dispatch_event_names()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var dispatchMessages = _consoleMessages
            .Where(m => m.Contains("[alis:execute] dispatch {"))
            .ToList();

        Assert.That(dispatchMessages, Has.Count.EqualTo(3),
            "Exactly 3 dispatch traces expected (test, test-received, final)");

        Assert.That(dispatchMessages.Any(m => m.Contains("\"event\":\"test\"")), Is.True,
            "Dispatch trace must include event name 'test'");
        Assert.That(dispatchMessages.Any(m => m.Contains("\"event\":\"test-received\"")), Is.True,
            "Dispatch trace must include event name 'test-received'");
        Assert.That(dispatchMessages.Any(m => m.Contains("\"event\":\"final\"")), Is.True,
            "Dispatch trace must include event name 'final'");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trace_captures_element_reaction_targets()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var elementReactionMessages = _consoleMessages
            .Where(m => m.Contains("[alis:execute]") && (m.Contains("set {") || m.Contains("call {")))
            .ToList();

        Assert.That(elementReactionMessages.Any(m => m.Contains("\"target\":\"step-1\"")), Is.True,
            "Element reaction trace must include target 'step-1'");
        Assert.That(elementReactionMessages.Any(m => m.Contains("\"target\":\"step-2\"")), Is.True,
            "Element reaction trace must include target 'step-2'");
        Assert.That(elementReactionMessages.Any(m => m.Contains("\"target\":\"step-3\"")), Is.True,
            "Element reaction trace must include target 'step-3'");
        Assert.That(elementReactionMessages.Any(m => m.Contains("\"target\":\"chain-status\"")), Is.True,
            "Element reaction trace must include target 'chain-status'");

        AssertNoConsoleErrors();
    }
}
