using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerCallsThePayloadsMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_methodValue_payload_before)]
    public async Task the_total_the_payloads_method_returns_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15MethodValuePayloadBefore);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("3");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(BenchPages.Poc15MethodValuePayloadBefore);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise plain", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#shown")).ToHaveTextAsync("1");
    }
}
