using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentsMemberIsSetIntoAProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.source_PropertySet)]
    public async Task the_property_holds_the_value_the_component_held()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SourcePropertySet);
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Copy", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("a-1");
    }
}
