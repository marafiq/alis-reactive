using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionNegatesAComparisonBesideAnd(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X5_0)]
    public async Task each_row_shows_the_branch_the_whole_condition_takes()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.SpareBeds);

        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "other", "Bo", "away with spare beds" });

        var allActive = await NewWorld();
        await allActive.Page.GotoAsync(CarePages.SpareBedsAllActive);

        await Expect(allActive.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "other", "Bo", "other" });
    }
}
