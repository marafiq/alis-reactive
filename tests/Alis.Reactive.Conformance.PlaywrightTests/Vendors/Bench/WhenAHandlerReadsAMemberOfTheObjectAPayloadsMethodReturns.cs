using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerReadsAMemberOfTheObjectAPayloadsMethodReturns(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_returnedObject_payload_before)]
    public async Task the_name_of_the_item_the_payloads_method_returns_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15ReturnedObjectPayloadBefore);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("One");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(BenchPages.Poc15ReturnedObjectPayloadBefore);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise plain", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#shown")).ToHaveTextAsync("Two");
    }
}
