using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesThroughAFindOverLiveItems(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F2.M4_X6_0)]
    public async Task a_find_that_keeps_no_item_stops_its_pipeline_and_one_that_keeps_some_writes_the_first_alone()
    {
        var census = await NewWorld();
        await census.Page.GotoAsync(CarePages.CensusItemChecks);
        await census.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the census", Exact = true }).ClickAsync();
        await Expect(census.Page.Locator("#census-status")).ToHaveTextAsync("Checking");
        await Expect(census.Page.Locator("#census").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Items", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Hall 0", "Bath 0" });

        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.CensusItemChecks);
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the north wing census", Exact = true }).ClickAsync();
        await Expect(north.Page.Locator("#census-north").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Items", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Lounge 0", "Garden 1", "Annex 0" });
        await Expect(north.Page.Locator("#census-north-status")).ToHaveTextAsync("Checked");
    }
}
