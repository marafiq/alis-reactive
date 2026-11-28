using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerCountsTheItemsAnEventsPayloadHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_list)]
    public async Task the_items_a_lambda_keeps_are_counted_by_each_items_name()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.EventList);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("2");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(BenchPages.EventList);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise plain", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#shown")).ToHaveTextAsync("0");
    }
}
