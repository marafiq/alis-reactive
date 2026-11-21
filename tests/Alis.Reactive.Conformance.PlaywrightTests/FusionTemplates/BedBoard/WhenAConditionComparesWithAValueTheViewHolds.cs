using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesWithAValueTheViewHolds(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X6_0)]
    public async Task each_row_shows_the_branch_the_views_value_decides()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.SpareRooms);

        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "North 1", "spare", "North 2", "full" });

        var aboveNone = await NewWorld();
        await aboveNone.Page.GotoAsync(CarePages.SpareRoomsAboveNone);

        await Expect(aboveNone.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "North 1", "spare", "North 2", "spare" });
    }
}
