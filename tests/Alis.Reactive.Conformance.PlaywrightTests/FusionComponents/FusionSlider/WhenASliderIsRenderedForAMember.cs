using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASliderIsRenderedForAMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F41.M21_X615_0)]
    public async Task the_slider_starts_at_the_value_the_member_holds()
    {
        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.RoomComfort);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();

        await Expect(held.Page.Locator("#temperature-reading")).ToHaveTextAsync("72");

        var moved = await NewWorld();
        await moved.Page.GotoAsync(CarePages.RoomComfort);
        await moved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room temperature", Exact = true }).GetByRole(AriaRole.Slider).PressAsync("ArrowRight");
        await moved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();

        await Expect(moved.Page.Locator("#temperature-reading")).ToHaveTextAsync("73");
    }
}
