using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffCountWardLinen(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1110_0)]
    public async Task adding_a_pack_increases_the_count_by_six()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.LinenCount);
        var towels = world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a pack", Exact = true }).ClickAsync();
        await Expect(towels).ToHaveValueAsync("14");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1110_1)]
    public async Task adding_one_towel_increases_the_count_by_one()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.LinenCount);
        var towels = world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add one", Exact = true }).ClickAsync();
        await Expect(towels).ToHaveValueAsync("9");
    }
}
