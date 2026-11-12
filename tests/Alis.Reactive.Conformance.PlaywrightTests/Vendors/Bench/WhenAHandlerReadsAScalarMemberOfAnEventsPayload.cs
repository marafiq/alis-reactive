using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerReadsAScalarMemberOfAnEventsPayload(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_scalar)]
    public async Task the_flag_the_payload_holds_decides_what_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.EventScalar);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Flagged");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(BenchPages.EventScalar);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise plain", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#shown")).ToHaveTextAsync("Plain");
    }
}
