using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALivePredicateReadsAMemberAnItemLacks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F2.M4_X8_0)]
    public async Task the_count_stops_its_pipeline_and_a_nullable_list_that_holds_null_counts_as_nothing()
    {
        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.CensusActiveCounts);
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the north wing census", Exact = true }).ClickAsync();
        await Expect(north.Page.Locator("#census-north-status")).ToHaveTextAsync("Counting");
        await Expect(north.Page.Locator("#census-north-count")).ToHaveTextAsync("-");

        var census = await NewWorld();
        await census.Page.GotoAsync(CarePages.CensusActiveCounts);
        await census.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the census", Exact = true }).ClickAsync();
        await Expect(census.Page.Locator("#census-status")).ToHaveTextAsync("Counted");
        await Expect(census.Page.Locator("#census-count")).ToHaveTextAsync("");
    }
}
