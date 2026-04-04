namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

[TestFixture]
public class WhenEventsChainAcrossListeners : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CoreBehaviors/Events";

    [Test]
    public async Task three_hop_chain_completes_in_order()
    {
        // If any hop in the chain breaks (boot ordering, dispatch wiring, workflow execution),
        // one or more steps will still show "waiting..." text instead of completion text.
        await NavigateTo(Path);
        await WaitForPageReady(5000);

        var step1 = Page.Locator("#step-1");
        var step2 = Page.Locator("#step-2");
        var step3 = Page.Locator("#step-3");

        // Each step's text proves the PREVIOUS dispatch arrived and the workflow executed
        await Expect(step1).ToContainTextAsync("dom-ready fired");
        await Expect(step2).ToContainTextAsync("\"test\" received");
        await Expect(step3).ToContainTextAsync("\"test-received\" received");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task chain_status_turns_green_on_completion()
    {
        // Workflow 4 listens for "final" and mutates chain-status:
        //   RemoveClass("text-text-muted") + AddClass("text-green-600") + AddClass("font-semibold") + SetText(...)
        // If the final event never fires (broken chain) OR mutations fail, this element stays gray.
        await NavigateTo(Path);
        await WaitForPageReady(5000);

        var status = Page.Locator("#chain-status");

        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("font-semibold"));
        await Expect(status).ToContainTextAsync("Chain complete");

        AssertNoConsoleErrors();
    }
}
