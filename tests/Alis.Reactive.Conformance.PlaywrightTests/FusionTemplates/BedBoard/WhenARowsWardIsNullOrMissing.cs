using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowsWardIsNullOrMissing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F1.M22_X3_0)]
    public async Task the_ward_cell_shows_nothing()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.ResidentWards);
        await Expect(board.Page.Locator("#board-status")).ToHaveTextAsync("Loaded");

        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "North", "Bo", "", "Cy", "" });

        var assigned = await NewWorld();
        await assigned.Page.GotoAsync(CarePages.ResidentWardsAllAssigned);
        await Expect(assigned.Page.Locator("#board-status")).ToHaveTextAsync("Loaded");

        await Expect(assigned.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "North", "Bo", "South", "Cy", "East" });
    }
}
