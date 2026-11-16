using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotReloadsAnInputThePagesLiveCheckReads(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.X12_3_0)]
    public async Task the_reloaded_input_shows_its_message_when_it_loses_the_focus_empty()
    {
        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.AddressAdmission);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the home address", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Home address", Exact = true })).ToBeVisibleAsync();
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the postal address", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })).ToBeVisibleAsync();
        await empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).ClickAsync();
        await empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).PressAsync("Tab");
        await Expect(empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByText("Name the street.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).FillAsync("3 Mill Lane");
        await Expect(empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByText("Name the street.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.AddressAdmission);
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the home address", Exact = true }).ClickAsync();
        await Expect(named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Home address", Exact = true })).ToBeVisibleAsync();
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the postal address", Exact = true }).ClickAsync();
        await Expect(named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })).ToBeVisibleAsync();
        await named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).FillAsync("12 Elm Row");
        await named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).PressAsync("Tab");
        await Expect(named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByText("Name the street.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Home address", Exact = true })).ToHaveCountAsync(0);
    }
}
