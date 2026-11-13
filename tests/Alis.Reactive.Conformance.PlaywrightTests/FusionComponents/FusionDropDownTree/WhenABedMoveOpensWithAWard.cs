using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABedMoveOpensWithAWard(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F21.M21_X1013_0)]
    public async Task the_box_shows_the_ward_the_model_holds()
    {
        var placed = await NewWorld();
        await placed.Page.GotoAsync(CarePages.BedMove);

        await Expect(placed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("North 2");

        var unplaced = await NewWorld();
        await unplaced.Page.GotoAsync(CarePages.BedMoveUnplaced);

        await Expect(unplaced.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("");
    }
}
