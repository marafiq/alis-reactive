using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMethodReturnsAPromiseWhereAValueWasDeclared(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.absence_promise)]
    public async Task the_call_stops_the_pipeline()
    {
        var promised = await NewWorld();
        await promised.Seed(Seed.TheComponentsVersion(ComponentVersion.Promised));
        await promised.Page.GotoAsync(BenchPages.AbsencePromise);
        await promised.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(promised.Page.Locator("#shown")).ToHaveTextAsync("");
        await Expect(promised.Page.Locator("#done")).ToHaveTextAsync("");

        var current = await NewWorld();
        await current.Page.GotoAsync(BenchPages.AbsencePromise);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(current.Page.Locator("#shown")).ToHaveTextAsync("3");
        await Expect(current.Page.Locator("#done")).ToHaveTextAsync("after");
    }
}
