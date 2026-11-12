using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentsMemberIsSetIntoAWriteOnlyProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.source_WriteOnlySet)]
    public async Task the_write_only_property_takes_the_value_the_component_held()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SourceWriteOnlySet);
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Write-only", Exact = true })).ToHaveTextAsync("Unwritten");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Write", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Write-only", Exact = true })).ToHaveTextAsync("Written a-1");
    }
}
