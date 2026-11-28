using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReadsAComponentsProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.prop_scalar_read)]
    public async Task the_text_the_component_holds_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.PropScalarRead);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Alpha");

        var renamed = await NewWorld();
        await renamed.Seed(Seed.TheComponentsText("Beta"));
        await renamed.Page.GotoAsync(BenchPages.PropScalarRead);
        await renamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(renamed.Page.Locator("#shown")).ToHaveTextAsync("Beta");
    }
}
