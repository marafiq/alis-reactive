using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowLacksTheAddressItsTemplateReads(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F1.X15_2_0)]
    public async Task the_city_cell_is_empty_and_every_row_renders()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.ResidentCities);
        await Expect(board.Page.Locator("#board-status")).ToHaveTextAsync("Loaded");

        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "Leeds", "Bo", "", "Cy", "" });

        var placed = await NewWorld();
        await placed.Page.GotoAsync(CarePages.ResidentCitiesAllPlaced);
        await Expect(placed.Page.Locator("#board-status")).ToHaveTextAsync("Loaded");

        await Expect(placed.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "Leeds", "Bo", "York", "Cy", "Hull" });
    }
}
