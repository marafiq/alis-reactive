using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoLiveContainersHoldOneInput(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_20_0)]
    public async Task the_message_a_blur_gives_shows_once()
    {
        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.ResidentAdmission);
        await empty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).ClickAsync();
        await empty.Page.Keyboard.PressAsync("Tab");
        await Expect(empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Resident", Exact = true })
            .GetByText("Name is required.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(empty.Page.GetByText("Name is required.", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(1);

        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.ResidentAdmission);
        await named.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ann");
        await named.Page.Keyboard.PressAsync("Tab");
        await Expect(named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Resident", Exact = true })
            .GetByText("Name is required.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }
}
