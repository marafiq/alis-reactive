using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerCountsTheObjectsAPayloadsMethodReturnsAfterAnAwait(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_returnedObjects_payload_after)]
    public async Task the_payloads_items_are_counted_once_the_await_ends()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15ReturnedObjectsPayloadAfter);
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("");
        await held.Release();

        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("2");
    }
}
