using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReadsAMemberOneObjectDownAProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.prop_dotted_read)]
    public async Task the_count_the_components_outer_object_holds_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.PropDottedRead);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("8");

        var counted = await NewWorld();
        await counted.Seed(Seed.TheComponentsOuterCount(12));
        await counted.Page.GotoAsync(BenchPages.PropDottedRead);
        await counted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(counted.Page.Locator("#shown")).ToHaveTextAsync("12");
    }
}
