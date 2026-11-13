using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesAConstantFirst(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X2_0)]
    public async Task the_grid_after_a_condition_ending_on_a_number_still_shows_its_rows()
    {
        var constantFirst = await NewWorld();
        await constantFirst.Page.GotoAsync(CarePages.WardRooms);

        await Expect(constantFirst.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Residents", Exact = true }).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "under 90", "Bo", "90 and over" });
        await Expect(constantFirst.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Rooms", Exact = true }).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "North 1", "spare", "North 2", "full" });

        var bedsFirst = await NewWorld();
        await bedsFirst.Page.GotoAsync(CarePages.WardRoomsBedsFirst);

        await Expect(bedsFirst.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Residents", Exact = true }).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ann", "under 90", "Bo", "90 and over" });
        await Expect(bedsFirst.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Rooms", Exact = true }).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "North 1", "spare", "North 2", "full" });
    }
}
