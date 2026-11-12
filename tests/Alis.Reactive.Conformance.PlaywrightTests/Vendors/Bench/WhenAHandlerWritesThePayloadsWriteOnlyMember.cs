using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerWritesThePayloadsWriteOnlyMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_writeOnly_payload_before)]
    public async Task the_component_reads_the_text_the_handler_wrote_once_the_handler_returns()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15WriteOnlyPayloadBefore);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");

        var silent = await NewWorld();
        await silent.Page.GotoAsync(BenchPages.EventMethod);
        await silent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(silent.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(silent.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: none");
    }
}
