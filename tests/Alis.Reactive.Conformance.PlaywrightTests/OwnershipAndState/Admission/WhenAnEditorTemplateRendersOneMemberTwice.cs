using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.OwnershipAndState;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEditorTemplateRendersOneMemberTwice(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M18_F1.X13_4_0)]
    public async Task the_servers_message_shows_beside_the_street_of_the_address_it_names()
    {
        var postal = await NewWorld();
        await postal.Page.GotoAsync(CarePages.AddressedMoveIn);
        await postal.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada Lee");
        await postal.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Home address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).FillAsync("Main Road");
        await postal.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).FillAsync("Unknown");
        await postal.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(postal.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByText("Give a street the post office knows.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(postal.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Give a street the post office knows." });
        await Expect(postal.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Home address", Exact = true })
            .GetByText("Give a street the post office knows.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(postal.Page.Locator("#move-in-status")).ToHaveTextAsync("Not saved");

        var home = await NewWorld();
        await home.Page.GotoAsync(CarePages.AddressedMoveIn);
        await home.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada Lee");
        await home.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Home address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).FillAsync("Unknown");
        await home.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Street", Exact = true }).FillAsync("Main Road");
        await home.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(home.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Home address", Exact = true })
            .GetByText("Give a street the post office knows.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(home.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Give a street the post office knows." });
        await Expect(home.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Postal address", Exact = true })
            .GetByText("Give a street the post office knows.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }
}
