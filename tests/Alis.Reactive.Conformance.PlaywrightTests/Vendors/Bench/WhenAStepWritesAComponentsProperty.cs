using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesAComponentsProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.prop_scalar_write)]
    public async Task the_component_shows_the_text_the_step_set()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.PropScalarWrite);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Rename" }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha, renamed");

        var untouched = await NewWorld();
        await untouched.Page.GotoAsync(BenchPages.PropScalarWrite);
        await Expect(untouched.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha");
    }
}
