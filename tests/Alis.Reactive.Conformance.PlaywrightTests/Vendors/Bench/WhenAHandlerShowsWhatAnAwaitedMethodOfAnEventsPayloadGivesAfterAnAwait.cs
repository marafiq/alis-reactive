using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerShowsWhatAnAwaitedMethodOfAnEventsPayloadGivesAfterAnAwait(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_methodAsyncValue_payload_after)]
    public async Task the_text_the_promise_gives_is_shown_once_both_awaits_end()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15MethodAsyncValuePayloadAfter);
        var armedAtCount = await bench.Arm(RunGate.Settle, WorldEndpoints.Count);
        var armedAtItsOwn = await bench.Arm(RunGate.Settle, WorldEndpoints.Text);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var heldAtCount = await armedAtCount.Held();
        await heldAtCount.Release();
        var heldAtItsOwn = await armedAtItsOwn.Held();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("");
        await heldAtItsOwn.Release();

        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Settled text");
    }
}
