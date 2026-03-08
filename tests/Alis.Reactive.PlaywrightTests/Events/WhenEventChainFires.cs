namespace Alis.Reactive.PlaywrightTests.Events;

[TestFixture]
public class WhenEventChainFires : PlaywrightTestBase
{
    [Test]
    public async Task ThreeHopChainCompletesWithAllDispatchesInTrace()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        AssertTraceContains("command", "\"event\":\"test\"");
        AssertTraceContains("command", "\"event\":\"test-received\"");
        AssertTraceContains("command", "\"event\":\"final\"");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task DispatchesOccurInChronologicalOrder()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        var dispatches = _consoleMessages
            .Where(m => m.Contains("dispatch"))
            .ToList();

        var testIdx = dispatches.FindIndex(m => m.Contains("\"event\":\"test\"") && !m.Contains("test-received"));
        var receivedIdx = dispatches.FindIndex(m => m.Contains("\"event\":\"test-received\""));
        var finalIdx = dispatches.FindIndex(m => m.Contains("\"event\":\"final\""));

        Assert.That(testIdx, Is.GreaterThanOrEqualTo(0), "test dispatch traced");
        Assert.That(receivedIdx, Is.GreaterThan(testIdx), "test-received dispatched after test");
        Assert.That(finalIdx, Is.GreaterThan(receivedIdx), "final dispatched after test-received");
    }

    [Test]
    public async Task FinalEventCarriesPayload()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        var finalDispatch = _consoleMessages
            .FirstOrDefault(m => m.Contains("dispatch") && m.Contains("\"event\":\"final\""));

        Assert.That(finalDispatch, Is.Not.Null, "final dispatch traced");
        Assert.That(finalDispatch, Does.Contain("eventName"), "Payload field present in trace");
    }

    [Test]
    public async Task PlanDrivesChainStatusDomMutations()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        // All steps should have text-green-600 class added by plan mutate-element commands
        var step1 = Page.Locator("#step-1");
        var step2 = Page.Locator("#step-2");
        var step3 = Page.Locator("#step-3");
        var status = Page.Locator("#chain-status");

        await Expect(step1).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(step2).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(step3).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        // Chain status text updated by plan
        await Expect(status).ToContainTextAsync("Chain complete");

        AssertNoConsoleErrors();
    }
}
