using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenIntakeContinuesInItsOwnScrollRegion(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1767_0)]
    public async Task the_scroll_parent_of_the_mounted_review_selects_the_correct_handover()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalRegisteredReview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open transfer review", Exact = true }).ClickAsync();
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue calendar review", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Review instruction", Exact = true })).ToHaveTextAsync("Use the next record in this transfer register.");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalSheetReview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open transfer review", Exact = true }).ClickAsync();
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue calendar review", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Review instruction", Exact = true })).ToHaveTextAsync("Use the handover on the booking sheet.");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }
}
