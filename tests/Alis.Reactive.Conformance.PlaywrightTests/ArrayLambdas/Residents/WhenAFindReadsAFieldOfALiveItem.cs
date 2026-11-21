using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFindReadsAFieldOfALiveItem(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F2.M4_X7_0)]
    public async Task a_find_that_keeps_no_item_reads_nothing_and_a_kept_item_that_lacks_the_field_stops_the_pipeline()
    {
        var census = await NewWorld();
        await census.Page.GotoAsync(CarePages.CensusFirstActiveItems);
        await census.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ask the census", Exact = true }).ClickAsync();
        await Expect(census.Page.Locator("#census-status")).ToHaveTextAsync("Asked");
        await Expect(census.Page.Locator("#census-name")).ToHaveTextAsync("");

        var south = await NewWorld();
        await south.Page.GotoAsync(CarePages.CensusFirstActiveItems);
        await south.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ask the south wing census", Exact = true }).ClickAsync();
        await Expect(south.Page.Locator("#census-south-status")).ToHaveTextAsync("Asking");
        await Expect(south.Page.Locator("#census-south-name")).ToHaveTextAsync("unknown");
    }
}
