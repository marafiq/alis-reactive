using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheChaplainPagesAReviewCalendar(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1761_0)]
    public async Task the_chaplain_pages_the_month_year_or_decade_actually_displayed()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalChaplaincyPaging);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open monthly review", Exact = true }).PressAsync("Enter");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Advance calendar", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByText("Safar1448", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open annual review", Exact = true }).PressAsync("Enter");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Advance calendar", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByText("1449", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open decade review", Exact = true }).PressAsync("Enter");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Advance calendar", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByText("1451 - 1460", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalChaplaincyPaging);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open monthly review", Exact = true }).PressAsync("Enter");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review previous period", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByText("Dhuʻl-Hijjah1447", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open annual review", Exact = true }).PressAsync("Enter");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review previous period", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByText("1447", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open decade review", Exact = true }).PressAsync("Enter");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review previous period", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByText("1431 - 1440", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
