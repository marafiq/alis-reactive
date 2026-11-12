using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesAWriteOnlyProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.prop_writeOnly)]
    public async Task the_component_shows_the_text_it_was_written()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.PropWriteOnly);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Write" }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Write-only" })).ToHaveTextAsync("Written Kappa");

        var untouched = await NewWorld();
        await untouched.Page.GotoAsync(BenchPages.PropWriteOnly);
        await Expect(untouched.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Write-only" })).ToHaveTextAsync("Unwritten");
    }
}
