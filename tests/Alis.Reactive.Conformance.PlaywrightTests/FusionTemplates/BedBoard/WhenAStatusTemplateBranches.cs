using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStatusTemplateBranches(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.X15_3_0)]
    public async Task each_row_shows_the_branch_its_status_takes()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.ResidentStatuses);

        await Expect(board.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "Active", "Bo", "Away" });

        var allActive = await NewWorld();
        await allActive.Page.GotoAsync(CarePages.ResidentStatusesAllActive);

        await Expect(allActive.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "Active", "Bo", "Active" });
    }
}
