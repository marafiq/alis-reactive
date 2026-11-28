using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListHoldsAPagedResultWhereAListWasDeclared(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_7_0)]
    public async Task the_count_stops_its_pipeline_and_a_list_that_holds_null_reads_nothing_and_goes_on()
    {
        var paged = await NewWorld();
        await paged.Page.GotoAsync(CarePages.CensusPanelRecords);
        await paged.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Paged", Exact = true }).ClickAsync();
        await paged.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the records", Exact = true }).ClickAsync();
        await Expect(paged.Page.Locator("#record-count")).ToHaveTextAsync("-");
        await Expect(paged.Page.Locator("#panel-status")).ToHaveTextAsync("Waiting");

        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.CensusPanelRecords);
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear", Exact = true }).ClickAsync();
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the records", Exact = true }).ClickAsync();
        await Expect(cleared.Page.Locator("#panel-status")).ToHaveTextAsync("Counted");
        await Expect(cleared.Page.Locator("#record-count")).ToHaveTextAsync("");
    }
}
