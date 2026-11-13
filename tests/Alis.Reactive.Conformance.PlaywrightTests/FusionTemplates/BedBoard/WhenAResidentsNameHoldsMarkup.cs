using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAResidentsNameHoldsMarkup(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F1.X15_1_0)]
    public async Task the_name_cell_shows_the_markup_as_text()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.ResidentNames);
        await Expect(board.Page.Locator("#board-status")).ToHaveTextAsync("Loaded");

        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "<img src=x onerror=alert(1)>" });
        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Img)).ToHaveCountAsync(0);

        var renamed = await NewWorld();
        await renamed.Page.GotoAsync(CarePages.ResidentNamesRenamed);
        await Expect(renamed.Page.Locator("#board-status")).ToHaveTextAsync("Loaded");

        await Expect(renamed.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "Bo" });
        await Expect(renamed.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Img)).ToHaveCountAsync(0);
    }
}
