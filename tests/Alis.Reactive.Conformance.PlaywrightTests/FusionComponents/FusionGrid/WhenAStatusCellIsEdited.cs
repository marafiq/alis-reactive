using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStatusCellIsEdited(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F23.M21_X204_0)]
    public async Task the_select_opens_at_the_status_the_row_holds()
    {
        var discharged = await NewWorld();
        await discharged.Page.GotoAsync(CarePages.StatusCorrections);
        await discharged.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "Discharged", Exact = true }).DblClickAsync();

        await Expect(discharged.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("Discharged");

        var onLeave = await NewWorld();
        await onLeave.Page.GotoAsync(CarePages.StatusCorrections);
        await onLeave.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "On leave", Exact = true }).DblClickAsync();

        await Expect(onLeave.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("On leave");
    }
}
