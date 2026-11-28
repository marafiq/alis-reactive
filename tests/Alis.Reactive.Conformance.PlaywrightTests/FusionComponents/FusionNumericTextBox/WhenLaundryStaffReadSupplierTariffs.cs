using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenLaundryStaffReadSupplierTariffs(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1636_0)]
    public async Task supplier_tariffs_render_the_contract_amount_labels_and_print_width()
    {
        var europe = await NewWorld();
        await europe.Page.GotoAsync(CarePages.LinenEuropeanTariff);
        await Expect(europe.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("€12.38");
        await Expect(europe.Page.GetByText("European laundry", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(europe.Page.GetByText("Euro tariff", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await europe.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review printed tariff", Exact = true }).ClickAsync();
        await Expect(europe.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tariff review", Exact = true })).ToHaveTextAsync("Compact European tariff");

        var domestic = await NewWorld();
        await domestic.Page.GotoAsync(CarePages.LinenDomesticTariff);
        await Expect(domestic.Page.GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("$12.4");
        await Expect(domestic.Page.GetByText("Domestic laundry", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(domestic.Page.GetByText("Dollar tariff", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await domestic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review printed tariff", Exact = true }).ClickAsync();
        await Expect(domestic.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Tariff review", Exact = true })).ToHaveTextAsync("Full domestic tariff");
    }
}
