using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReadsAReadOnlyProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.prop_readOnly)]
    public async Task the_text_the_read_only_property_holds_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.PropReadOnly);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("RO-2026-11-10");

        var later = await NewWorld();
        await later.Seed(Seed.TheComponentsReadOnlyText("RO-2026-11-11"));
        await later.Page.GotoAsync(BenchPages.PropReadOnly);
        await later.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(later.Page.Locator("#shown")).ToHaveTextAsync("RO-2026-11-11");
    }
}
