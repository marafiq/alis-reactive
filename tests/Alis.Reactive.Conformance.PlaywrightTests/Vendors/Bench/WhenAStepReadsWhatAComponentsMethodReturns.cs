using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReadsWhatAComponentsMethodReturns(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_value)]
    public async Task the_total_the_method_returns_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodValue);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("3");

        var single = await NewWorld();
        await single.Seed(Seed.TheComponentsCount(1));
        await single.Page.GotoAsync(BenchPages.MethodValue);
        await single.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(single.Page.Locator("#shown")).ToHaveTextAsync("1");
    }
}
