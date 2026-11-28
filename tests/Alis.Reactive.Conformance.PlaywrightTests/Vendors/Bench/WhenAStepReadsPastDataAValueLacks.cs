using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReadsPastDataAValueLacks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.absence_past)]
    public async Task the_missing_data_reads_as_its_types_start()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.AbsencePast);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("none");

        var detailed = await NewWorld();
        await detailed.Seed(Seed.TheComponentsRecordDetail("Full"));
        await detailed.Page.GotoAsync(BenchPages.AbsencePast);
        await detailed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(detailed.Page.Locator("#shown")).ToHaveTextAsync("Full");
    }
}
