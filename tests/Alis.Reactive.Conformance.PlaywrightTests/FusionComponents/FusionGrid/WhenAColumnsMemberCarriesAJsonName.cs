using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAColumnsMemberCarriesAJsonName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F23.M21_X206_0)]
    public async Task the_column_shows_each_row_the_view_hands_over()
    {
        var rooms = await NewWorld();
        await rooms.Page.GotoAsync(CarePages.RoomBeds);

        await Expect(rooms.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "East 1", Exact = true })).ToBeVisibleAsync();
        await Expect(rooms.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "East 2", Exact = true })).ToBeVisibleAsync();

        var wards = await NewWorld();
        await wards.Page.GotoAsync(CarePages.RoomBeds);

        await Expect(wards.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "East", Exact = true })).ToHaveTextAsync(new[] { "East", "East" });
    }
}
