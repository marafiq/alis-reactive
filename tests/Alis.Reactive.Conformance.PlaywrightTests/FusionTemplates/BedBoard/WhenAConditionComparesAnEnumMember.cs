using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesAnEnumMember(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X7_0)]
    public async Task each_row_shows_the_branch_its_number_takes()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.RoomStates);

        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "North 1", "open", "North 2", "closed" });

        var swapped = await NewWorld();
        await swapped.Page.GotoAsync(CarePages.RoomStatesSwapped);

        await Expect(swapped.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "North 1", "closed", "North 2", "open" });
    }
}
