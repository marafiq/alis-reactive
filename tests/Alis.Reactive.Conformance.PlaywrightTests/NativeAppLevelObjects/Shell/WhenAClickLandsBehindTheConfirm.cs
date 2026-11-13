using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAClickLandsBehindTheConfirm(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F4.M25_X7_0)]
    public async Task a_click_behind_the_open_question_runs_nothing()
    {
        var behind = await NewWorld();
        await behind.Page.GotoAsync(CarePages.DischargeDesk);
        await behind.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asking = behind.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asking.GetByText("Discharge Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await behind.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add note", Exact = true }).ClickAsync(new LocatorClickOptions { Force = true });
        await Expect(behind.Page.Locator("#notes")).ToBeEmptyAsync();
        await Expect(asking.GetByText("Discharge Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.DischargeDesk);
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asked = answered.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "No", Exact = true }).ClickAsync();
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add note", Exact = true }).ClickAsync();
        await Expect(answered.Page.Locator("#notes")).ToHaveTextAsync("added");
    }
}
