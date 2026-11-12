using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnElementDispatchesAnEventWithNoDetail(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_3_0)]
    public async Task the_handler_reads_the_event_as_declared_on_every_dispatch()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsElementSignals);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open with room", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#opened-type")).ToHaveTextAsync("opened");
        await Expect(bench.Page.Locator("#opened-room")).ToHaveTextAsync("12");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open with no room", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#opened-room")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#opened-type")).ToHaveTextAsync("opened");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open with room", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#opened-room")).ToHaveTextAsync("12");
        await Expect(bench.Page.Locator("#opened-type")).ToHaveTextAsync("opened");
    }
}
