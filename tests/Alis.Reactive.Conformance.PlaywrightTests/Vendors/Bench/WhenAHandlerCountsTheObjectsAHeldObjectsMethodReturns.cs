using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerCountsTheObjectsAHeldObjectsMethodReturns(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_returnedObjects_held_before)]
    public async Task the_items_the_held_objects_method_returns_are_counted()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15ReturnedObjectsHeldBefore);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("1");

        var unmatched = await NewWorld();
        await unmatched.Seed(Seed.TheHeldObjectsMatchingItems(0));
        await unmatched.Page.GotoAsync(BenchPages.Poc15ReturnedObjectsHeldBefore);
        await unmatched.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(unmatched.Page.Locator("#shown")).ToHaveTextAsync("0");
    }
}
