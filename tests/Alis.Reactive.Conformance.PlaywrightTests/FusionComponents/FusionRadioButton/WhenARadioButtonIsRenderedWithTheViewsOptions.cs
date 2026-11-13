using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARadioButtonIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F36.M21_X1022_0)]
    public async Task the_groups_chosen_value_is_read_from_the_button_the_plan_names()
    {
        var companion = await NewWorld();
        await companion.Page.GotoAsync(CarePages.RoomType);
        await companion.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room type", Exact = true }).GetByText("Companion suite", new LocatorGetByTextOptions { Exact = true }).ClickAsync();
        await companion.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show room", Exact = true }).ClickAsync();

        await Expect(companion.Page.Locator("#room-choice")).ToHaveTextAsync("companion");

        var single = await NewWorld();
        await single.Page.GotoAsync(CarePages.RoomType);
        await single.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room type", Exact = true }).GetByText("Single room", new LocatorGetByTextOptions { Exact = true }).ClickAsync();
        await single.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show room", Exact = true }).ClickAsync();

        await Expect(single.Page.Locator("#room-choice")).ToHaveTextAsync("single");
    }
}
