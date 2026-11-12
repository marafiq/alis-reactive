using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEventsPayloadHoldsAPromiseWhereAValueWasDeclared(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_promise)]
    public async Task the_read_of_the_promise_stops_the_handler()
    {
        var promised = await NewWorld();
        await promised.Seed(Seed.TheComponentsVersion(ComponentVersion.Promised));
        await promised.Page.GotoAsync(BenchPages.EventPromise);
        await promised.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(promised.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(promised.Page.Locator("#shown")).ToHaveTextAsync("");
        await Expect(promised.Page.Locator("#done")).ToHaveTextAsync("");

        var current = await NewWorld();
        await current.Page.GotoAsync(BenchPages.EventPromise);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(current.Page.Locator("#shown")).ToHaveTextAsync("14");
        await Expect(current.Page.Locator("#done")).ToHaveTextAsync("after");
    }
}
