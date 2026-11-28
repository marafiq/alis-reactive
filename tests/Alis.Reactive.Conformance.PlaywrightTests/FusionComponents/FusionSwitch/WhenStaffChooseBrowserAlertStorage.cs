using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffChooseBrowserAlertStorage(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F46.M21_X1149_0)]
    public async Task an_opted_in_browser_remembers_the_switch_on_the_next_visit()
    {
        var remembered = await NewWorld();
        await remembered.Page.GotoAsync(CarePages.AlertPreferencesBrowserStorage);
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remember on this browser", Exact = true }).ClickAsync();
        await remembered.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await remembered.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Review saved alert preference", Exact = true }).ClickAsync();
        await Expect(AlertSwitch(remembered.Page)).ToBeCheckedAsync();

        var defaulted = await NewWorld();
        await defaulted.Page.GotoAsync(CarePages.AlertPreferencesBrowserStorage);
        await defaulted.Page.GetByText("Night alerts", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await defaulted.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Review saved alert preference", Exact = true }).ClickAsync();
        await Expect(AlertSwitch(defaulted.Page)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
        await Expect(defaulted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Routing status", Exact = true })).ToHaveTextAsync("Do not send alerts");
    }

    [Test]
    [FailsFor(Promises.M21_F46.M21_X1150_0)]
    public async Task the_storage_review_reports_the_live_browser_preference()
    {
        var remembered = await NewWorld();
        await remembered.Page.GotoAsync(CarePages.AlertPreferencesBrowserStorage);
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remember on this browser", Exact = true }).ClickAsync();
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review storage policy", Exact = true }).ClickAsync();
        await Expect(remembered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Storage review", Exact = true })).ToHaveTextAsync("Remembered here");

        var defaulted = await NewWorld();
        await defaulted.Page.GotoAsync(CarePages.AlertPreferencesBrowserStorage);
        await defaulted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review storage policy", Exact = true }).ClickAsync();
        await Expect(defaulted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Storage review", Exact = true })).ToHaveTextAsync("Use server default");
    }

    private static ILocator AlertSwitch(IPage page) => page.GetByRole(AriaRole.Switch, new PageGetByRoleOptions { Name = "Night alerts", Exact = true });
}
