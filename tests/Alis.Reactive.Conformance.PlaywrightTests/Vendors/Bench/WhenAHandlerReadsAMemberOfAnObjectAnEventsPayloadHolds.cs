using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerReadsAMemberOfAnObjectAnEventsPayloadHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_object)]
    public async Task the_name_of_the_middle_the_payloads_outer_holds_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.EventObject);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("M-12");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(BenchPages.EventObject);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise plain", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#shown")).ToHaveTextAsync("M-14");
    }
}
