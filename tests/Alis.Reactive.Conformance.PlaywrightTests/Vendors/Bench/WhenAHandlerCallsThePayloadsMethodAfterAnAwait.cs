using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerCallsThePayloadsMethodAfterAnAwait(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_methodValue_payload_after)]
    public async Task the_method_is_called_after_the_await_and_not_before_it()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15MethodValuePayloadAfter);
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("");
        await held.Release();

        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("2");

        var unawaited = await NewWorld();
        await unawaited.Page.GotoAsync(BenchPages.Poc15MethodValuePayloadBefore);
        await unawaited.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(unawaited.Page.Locator("#shown")).ToHaveTextAsync("3");
    }
}
