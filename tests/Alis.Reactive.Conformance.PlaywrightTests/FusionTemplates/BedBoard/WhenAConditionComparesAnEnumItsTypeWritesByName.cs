using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesAnEnumItsTypeWritesByName(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X8_0)]
    public async Task each_row_shows_the_branch_its_name_takes()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.BedCleaning);

        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Bed 1", "ready", "Bed 2", "awaiting cleaning" });

        var swapped = await NewWorld();
        await swapped.Page.GotoAsync(CarePages.BedCleaningSwapped);

        await Expect(swapped.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Bed 1", "awaiting cleaning", "Bed 2", "ready" });
    }
}
