using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenWardStaffReconcileLinen(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1600_0)]
    public async Task issuing_a_pack_or_one_towel_reduces_the_available_stock_by_the_ordered_quantity()
    {
        var pack = await NewWorld();
        await pack.Page.GotoAsync(CarePages.LinenCount);
        await pack.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take a pack", Exact = true }).ClickAsync();
        await Expect(pack.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("2");

        var single = await NewWorld();
        await single.Page.GotoAsync(CarePages.LinenCount);
        await single.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take one", Exact = true }).ClickAsync();
        await Expect(single.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("7");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1601_0)]
    public async Task receiving_a_bundle_uses_the_quantity_on_the_pick_list()
    {
        var three = await NewWorld();
        await three.Page.GotoAsync(CarePages.LinenCount);
        await three.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Three per bundle", Exact = true }).ClickAsync();
        await three.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add one", Exact = true }).ClickAsync();
        await Expect(three.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("11");
        await Expect(three.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Pick list", Exact = true })).ToHaveTextAsync("Bundle of 3");

        var four = await NewWorld();
        await four.Page.GotoAsync(CarePages.LinenCount);
        await four.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Four per bundle", Exact = true }).ClickAsync();
        await four.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add one", Exact = true }).ClickAsync();
        await Expect(four.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("12");
        await Expect(four.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Pick list", Exact = true })).ToHaveTextAsync("Bundle of 4");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1602_0)]
    public async Task a_zero_delivery_or_return_preserves_the_count_on_the_receipt()
    {
        var delivery = await NewWorld();
        await delivery.Page.GotoAsync(CarePages.LinenCount);
        await delivery.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post zero delivery", Exact = true }).ClickAsync();
        await Expect(delivery.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("8");
        await Expect(delivery.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Adjustment receipt", Exact = true })).ToHaveTextAsync("No stock adjustment");

        var returned = await NewWorld();
        await returned.Page.GotoAsync(CarePages.LinenCount);
        await returned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post zero return", Exact = true }).ClickAsync();
        await Expect(returned.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("8");
        await Expect(returned.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Adjustment receipt", Exact = true })).ToHaveTextAsync("No stock adjustment");

        var recounted = await NewWorld();
        await recounted.Page.GotoAsync(CarePages.LinenCount);
        await recounted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add one", Exact = true }).ClickAsync();
        await recounted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post zero delivery", Exact = true }).ClickAsync();
        await Expect(recounted.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("9");
        await Expect(recounted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Adjustment receipt", Exact = true })).ToHaveTextAsync("No stock adjustment");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1603_0)]
    public async Task issuing_or_receiving_linen_honors_the_selected_reserve_and_cart_capacity()
    {
        var reserve = await NewWorld();
        await reserve.Page.GotoAsync(CarePages.LinenCount);
        await reserve.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Protect six towels", Exact = true }).ClickAsync();
        await reserve.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take a pack", Exact = true }).ClickAsync();
        await Expect(reserve.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("6");
        await Expect(reserve.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Stock policy", Exact = true })).ToHaveTextAsync("Reserve of 6");

        var cart = await NewWorld();
        await cart.Page.GotoAsync(CarePages.LinenCount);
        await cart.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use ten-towel cart", Exact = true }).ClickAsync();
        await cart.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a pack", Exact = true }).ClickAsync();
        await Expect(cart.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("10");
        await Expect(cart.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Stock policy", Exact = true })).ToHaveTextAsync("Capacity of 10");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1604_0)]
    public async Task a_discrepancy_is_either_limited_or_recorded_for_review()
    {
        var strict = await NewWorld();
        await strict.Page.GotoAsync(CarePages.LinenCount);
        await strict.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Permit discrepancy", Exact = true }).ClickAsync();
        await strict.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Enforce cart capacity", Exact = true }).ClickAsync();
        await strict.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton).FillAsync("13.8");
        await strict.Page.Keyboard.PressAsync("Tab");
        await Expect(strict.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("10");
        await Expect(strict.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Discrepancy review", Exact = true })).ToHaveTextAsync("Within cart capacity");

        var review = await NewWorld();
        await review.Page.GotoAsync(CarePages.LinenCount);
        await review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Permit discrepancy", Exact = true }).ClickAsync();
        await review.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton).FillAsync("13.8");
        await review.Page.Keyboard.PressAsync("Tab");
        await Expect(review.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward linen stock", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("14");
        await Expect(review.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Discrepancy review", Exact = true })).ToHaveTextAsync("Supervisor review required");
    }
}
