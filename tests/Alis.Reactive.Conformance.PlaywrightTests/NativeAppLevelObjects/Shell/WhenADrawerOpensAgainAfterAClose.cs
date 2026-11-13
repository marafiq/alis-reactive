using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADrawerOpensAgainAfterAClose(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F1.M25_X1_0)]
    public async Task the_drawer_shows_an_empty_heading_and_an_empty_content_whatever_the_page_animates()
    {
        var still = await NewWorld();
        await still.Page.GotoAsync(CarePages.ShiftNotes);
        await still.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open notes", Exact = true }).ClickAsync();
        var notes = still.Page.GetByRole(AriaRole.Complementary, new PageGetByRoleOptions { Name = "Care notes", Exact = true });
        await Expect(notes.GetByText("Ada ate her breakfast.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await notes.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        await still.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open empty", Exact = true }).ClickAsync();
        await Expect(still.Page.GetByRole(AriaRole.Complementary)).ToBeVisibleAsync();
        await Expect(still.Page.Locator("#alis-drawer-title")).ToBeEmptyAsync();
        await Expect(still.Page.Locator("#alis-drawer-content")).ToBeEmptyAsync();

        var sliding = await NewWorld();
        await sliding.Page.GotoAsync(CarePages.ShiftNotesSliding);
        await sliding.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open notes", Exact = true }).ClickAsync();
        var slid = sliding.Page.GetByRole(AriaRole.Complementary, new PageGetByRoleOptions { Name = "Care notes", Exact = true });
        await Expect(slid.GetByText("Ada ate her breakfast.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await slid.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        await sliding.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open empty", Exact = true }).ClickAsync();
        await Expect(sliding.Page.GetByRole(AriaRole.Complementary)).ToBeVisibleAsync();
        await Expect(sliding.Page.Locator("#alis-drawer-title")).ToBeEmptyAsync();
        await Expect(sliding.Page.Locator("#alis-drawer-content")).ToBeEmptyAsync();
    }
}
