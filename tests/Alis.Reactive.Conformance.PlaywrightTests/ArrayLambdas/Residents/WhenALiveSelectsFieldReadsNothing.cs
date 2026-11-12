using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALiveSelectsFieldReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F2.M4_X9_0)]
    public async Task a_field_whose_type_holds_no_null_stops_the_pipeline_and_one_whose_type_holds_null_is_null_in_its_list()
    {
        var scores = await NewWorld();
        await scores.Page.GotoAsync(CarePages.CensusScoresWithAHeadCountOnEveryItem);
        await scores.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(scores.Page.Locator("#score-status")).ToHaveTextAsync("Counting");
        await Expect(scores.Page.Locator("#score-count")).ToHaveTextAsync("-");

        var headCounts = await NewWorld();
        await headCounts.Page.GotoAsync(CarePages.CensusScoresWithAnItemLackingItsHeadCount);
        await headCounts.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count head counts", Exact = true }).ClickAsync();
        await Expect(headCounts.Page.Locator("#head-counts")).ToHaveTextAsync("3");
        await Expect(headCounts.Page.Locator("#head-counts-not-taken")).ToHaveTextAsync("1");
    }
}
