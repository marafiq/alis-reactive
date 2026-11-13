using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenEscapeAnswersTheConfirmOverADrawer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F4.X17_7_0)]
    public async Task escape_closes_the_dialog_and_the_drawer_behind_it_stays_open()
    {
        var answering = await NewWorld();
        await answering.Page.GotoAsync(CarePages.DrawerDischarge);
        await answering.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open notes", Exact = true }).ClickAsync();
        var drawer = answering.Page.GetByRole(AriaRole.Complementary);
        await drawer.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asking = answering.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asking.GetByText("Discharge Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await answering.Page.Keyboard.PressAsync("Escape");
        await Expect(asking).ToBeHiddenAsync();
        await Expect(answering.Page.Locator("#decision")).ToHaveTextAsync("Kept");
        await Expect(drawer.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Discharge", Exact = true })).ToBeVisibleAsync();

        var closing = await NewWorld();
        await closing.Page.GotoAsync(CarePages.DrawerDischarge);
        await closing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open notes", Exact = true }).ClickAsync();
        var closed = closing.Page.GetByRole(AriaRole.Complementary);
        await closed.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asked = closing.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asked.GetByText("Discharge Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await closing.Page.Keyboard.PressAsync("Escape");
        await Expect(closing.Page.Locator("#decision")).ToHaveTextAsync("Kept");
        await closing.Page.Keyboard.PressAsync("Escape");
        await Expect(closed).ToBeHiddenAsync();
    }
}
