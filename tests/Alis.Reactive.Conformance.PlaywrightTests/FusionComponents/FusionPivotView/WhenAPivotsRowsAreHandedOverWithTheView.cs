using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPivotsRowsAreHandedOverWithTheView(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F34.M21_X1043_0)]
    public async Task the_table_counts_each_ward_by_the_members_json_names()
    {
        var wards = await NewWorld();
        await wards.Page.GotoAsync(CarePages.WardOccupancy);

        await Expect(wards.Page.GetByRole(AriaRole.Row, new PageGetByRoleOptions { Name = "East 1", Exact = true })).ToBeVisibleAsync();
        await Expect(wards.Page.GetByRole(AriaRole.Row, new PageGetByRoleOptions { Name = "North 2", Exact = true })).ToBeVisibleAsync();

        var moved = await NewWorld();
        await moved.Page.GotoAsync(CarePages.WardOccupancyAfterAMove);

        await Expect(moved.Page.GetByRole(AriaRole.Row, new PageGetByRoleOptions { Name = "East 2", Exact = true })).ToBeVisibleAsync();
        await Expect(moved.Page.GetByRole(AriaRole.Row, new PageGetByRoleOptions { Name = "North 1", Exact = true })).ToBeVisibleAsync();
    }
}
