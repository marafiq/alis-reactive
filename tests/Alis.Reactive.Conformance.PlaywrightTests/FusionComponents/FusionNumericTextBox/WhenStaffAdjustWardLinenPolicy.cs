using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffAdjustWardLinenPolicy(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1627_0)]
    public async Task the_invoice_can_use_the_community_currency_or_keep_the_contract_currency()
    {
        var community = await NewWorld();
        await community.Page.GotoAsync(CarePages.LinenLaundryCharge);
        await community.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare euro invoice", Exact = true }).ClickAsync();
        await community.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use community billing currency", Exact = true }).ClickAsync();
        await Expect(community.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Invoice preview", Exact = true })).ToHaveTextAsync("$12.50");
        await Expect(community.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Billing source", Exact = true })).ToHaveTextAsync("Community billing currency");

        var contract = await NewWorld();
        await contract.Page.GotoAsync(CarePages.LinenLaundryCharge);
        await contract.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep euro contract", Exact = true }).ClickAsync();
        await Expect(contract.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Invoice preview", Exact = true })).ToHaveTextAsync("€12.50");
        await Expect(contract.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Billing source", Exact = true })).ToHaveTextAsync("Contract billing currency");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1628_0)]
    public async Task releasing_a_reserve_or_capacity_allows_the_requested_transfer()
    {
        var issue = await NewWorld();
        await issue.Page.GotoAsync(CarePages.LinenTransfer);
        await issue.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release reserve and issue pack", Exact = true }).ClickAsync();
        await Expect(issue.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("2");
        await Expect(issue.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transfer policy", Exact = true })).ToHaveTextAsync("Reserve released");

        var delivery = await NewWorld();
        await delivery.Page.GotoAsync(CarePages.LinenTransfer);
        await delivery.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release capacity and receive pack", Exact = true }).ClickAsync();
        await Expect(delivery.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("14");
        await Expect(delivery.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transfer policy", Exact = true })).ToHaveTextAsync("Cart capacity released");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1629_0)]
    public async Task a_community_print_style_and_an_intentionally_unbranded_copy_have_distinct_notices()
    {
        var community = await NewWorld();
        await community.Page.GotoAsync(CarePages.LinenScaleSheet);
        await community.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print narrow Arabic column", Exact = true }).ClickAsync();
        await community.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use community print style", Exact = true }).ClickAsync();
        await Expect(community.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print layout", Exact = true })).ToHaveTextAsync("Community print style");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.LinenScaleSheet);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print narrow Arabic column", Exact = true }).ClickAsync();
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Make unbranded copy", Exact = true }).ClickAsync();
        await Expect(plain.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print layout", Exact = true })).ToHaveTextAsync("Unbranded print copy");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1630_0)]
    public async Task emergency_stock_checks_each_keyboard_step_while_the_counter_has_focus()
    {
        var shortage = await NewWorld();
        await shortage.Page.GotoAsync(CarePages.LinenEmergency);
        await shortage.Page.GetByRole(AriaRole.Spinbutton).PressAsync("ArrowDown");
        await Expect(shortage.Page.GetByRole(AriaRole.Spinbutton)).ToBeFocusedAsync();
        await Expect(ReserveFieldAndWarning(shortage.Page)).ToHaveCountAsync(2);
        await Expect(shortage.Page.GetByText("Keep eight towels for emergency stock", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var enough = await NewWorld();
        await enough.Page.GotoAsync(CarePages.LinenEmergency);
        await enough.Page.GetByRole(AriaRole.Spinbutton).PressAsync("ArrowUp");
        await Expect(enough.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("9");
        await Expect(enough.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Reserve review", Exact = true })).ToHaveTextAsync("Emergency reserve met");
        await Expect(ReserveFieldAndWarning(enough.Page)).ToHaveCountAsync(1);
        await Expect(enough.Page.GetByText("Keep eight towels for emergency stock", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1631_0)]
    public async Task a_first_empty_count_and_withdrawing_zero_generate_distinct_log_entries()
    {
        var first = await NewWorld();
        await first.Page.GotoAsync(CarePages.LinenStocktake);
        await first.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark count outstanding", Exact = true }).ClickAsync();
        await first.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record empty cart", Exact = true }).ClickAsync();
        await Expect(first.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Stock change", Exact = true })).ToHaveTextAsync("First count confirmed empty");

        var withdrawn = await NewWorld();
        await withdrawn.Page.GotoAsync(CarePages.LinenStocktake);
        await withdrawn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record empty cart", Exact = true }).ClickAsync();
        await withdrawn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark count outstanding", Exact = true }).ClickAsync();
        await Expect(withdrawn.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Stock change", Exact = true })).ToHaveTextAsync("Empty-cart count withdrawn");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1632_0)]
    public async Task receiving_and_issuing_a_weighed_parcel_adjusts_the_load_by_its_fractional_weight()
    {
        var receipt = await NewWorld();
        await receipt.Page.GotoAsync(CarePages.LinenParcel);
        await receipt.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Receive weighed parcel", Exact = true }).ClickAsync();
        await Expect(receipt.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer load", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("12.750");

        var issue = await NewWorld();
        await issue.Page.GotoAsync(CarePages.LinenParcel);
        await issue.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Issue weighed parcel", Exact = true }).ClickAsync();
        await Expect(issue.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer load", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("12.250");
    }

    private static ILocator ReserveFieldAndWarning(IPage page)
    {
        var reserve = page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Emergency reserve", Exact = true });
        return reserve.GetByRole(AriaRole.Spinbutton)
            .Or(reserve.GetByText("Keep eight towels for emergency stock", new LocatorGetByTextOptions { Exact = true }));
    }
}
