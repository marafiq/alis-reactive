using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenLaundryStaffPrepareChargesAndWeights(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1608_0)]
    public async Task laundry_invoices_preserve_the_billing_currency_in_the_printed_preview()
    {
        var euro = await NewWorld();
        await euro.Page.GotoAsync(CarePages.LinenLaundryCharge);
        await euro.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare euro invoice", Exact = true }).ClickAsync();
        await Expect(euro.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("€12.50");
        await Expect(euro.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Invoice preview", Exact = true })).ToHaveTextAsync("€12.50");

        var dollar = await NewWorld();
        await dollar.Page.GotoAsync(CarePages.LinenLaundryCharge);
        await dollar.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare dollar invoice", Exact = true }).ClickAsync();
        await Expect(dollar.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("$12.50");
        await Expect(dollar.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Invoice preview", Exact = true })).ToHaveTextAsync("$12.50");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1609_0)]
    public async Task the_scale_can_reject_excess_fractional_digits_or_retain_the_full_reading()
    {
        var fixedScale = await NewWorld();
        await fixedScale.Page.GotoAsync(CarePages.LinenScale);
        await fixedScale.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use hundredths scale", Exact = true }).ClickAsync();
        await fixedScale.Page.GetByRole(AriaRole.Spinbutton).ClearAsync();
        await fixedScale.Page.GetByRole(AriaRole.Spinbutton).PressSequentiallyAsync("1.");
        await fixedScale.Page.GetByRole(AriaRole.Spinbutton).PressSequentiallyAsync("23");
        await fixedScale.Page.GetByRole(AriaRole.Spinbutton).PressAsync("4");
        await Expect(fixedScale.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("1.23");

        var fullScale = await NewWorld();
        await fullScale.Page.GotoAsync(CarePages.LinenScale);
        await fullScale.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Allow untrimmed entry", Exact = true }).ClickAsync();
        await fullScale.Page.GetByRole(AriaRole.Spinbutton).ClearAsync();
        await fullScale.Page.GetByRole(AriaRole.Spinbutton).PressSequentiallyAsync("1.2");
        await fullScale.Page.GetByRole(AriaRole.Spinbutton).PressSequentiallyAsync("34");
        await Expect(fullScale.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("1.234");
    }

    [Test]
    [FailsFor(Promises.M21_F32.M21_X1610_0)]
    public async Task scale_tickets_use_whole_or_unrestricted_precision_for_the_measured_bundle()
    {
        var whole = await NewWorld();
        await whole.Page.GotoAsync(CarePages.LinenScale);
        await whole.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Weigh in whole units", Exact = true }).ClickAsync();
        await Expect(whole.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Scale ticket", Exact = true })).ToHaveTextAsync("12.000 kg");

        var precise = await NewWorld();
        await precise.Page.GotoAsync(CarePages.LinenScale);
        await precise.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use hundredths scale", Exact = true }).ClickAsync();
        await precise.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Weigh at full precision", Exact = true }).ClickAsync();
        await Expect(precise.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Scale ticket", Exact = true })).ToHaveTextAsync("12.375 kg");
    }
}
