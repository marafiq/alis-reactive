using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerCountsAHeldObjectsItems(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_liveObjects_held_before)]
    public async Task the_named_items_of_the_object_the_payload_holds_are_counted()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15LiveObjectsHeldBefore);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("4");

        var fewer = await NewWorld();
        await fewer.Seed(Seed.TheHeldObjectsNamedItems(3));
        await fewer.Page.GotoAsync(BenchPages.Poc15LiveObjectsHeldBefore);
        await fewer.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(fewer.Page.Locator("#shown")).ToHaveTextAsync("3");
    }
}
