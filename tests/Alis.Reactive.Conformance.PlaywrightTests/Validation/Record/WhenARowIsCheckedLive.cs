using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenARowIsCheckedLive(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_54_0)]
    public async Task the_failure_shows_once_beside_the_input_under_the_name_its_container_sends()
    {
        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.ShiftRota);
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).ClickAsync();
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).FillAsync("");
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).PressAsync("Tab");
        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).GetByText("Choose a carer.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByText("Choose a carer.", new LocatorGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var typed = await NewWorld();
        await typed.Page.GotoAsync(CarePages.ShiftRota);
        await typed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).ClickAsync();
        await typed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).FillAsync("Ann");
        await typed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).PressAsync("Tab");
        await Expect(typed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByText("Choose a carer.", new LocatorGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
    }
}
