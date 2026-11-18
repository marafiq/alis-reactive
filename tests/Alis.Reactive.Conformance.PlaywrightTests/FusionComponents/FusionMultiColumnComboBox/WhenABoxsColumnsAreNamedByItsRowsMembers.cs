using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABoxsColumnsAreNamedByItsRowsMembers(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F31.M21_X1052_0)]
    public async Task each_row_shows_its_label_in_the_bed_column_by_the_name_its_member_carries()
    {
        var beds = await NewWorld();
        await beds.Page.GotoAsync(CarePages.BedLabels);
        await beds.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");

        await Expect(beds.Page.GetByRole(AriaRole.Row, new PageGetByRoleOptions { Name = "12A Maple 12A", Exact = true })).ToBeVisibleAsync();
        await Expect(beds.Page.GetByRole(AriaRole.Row, new PageGetByRoleOptions { Name = "14B Cedar 14B", Exact = true })).ToBeVisibleAsync();
    }
}
