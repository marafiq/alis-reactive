using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASupplierDeliveryIsChecked(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F13.M21_X1217_0)]
    public async Task the_desk_waits_for_the_chosen_suppliers_delivery_window()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.SupplierDelivery);
        await desk.Page.GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(desk.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Northbank Medical", Exact = true })).ToBeVisibleAsync();
        await desk.Page.GetByRole(AriaRole.Combobox).PressAsync("ArrowDown");
        await desk.Page.GetByRole(AriaRole.Combobox).PressAsync("Enter");
        await Expect(desk.Page.GetByRole(AriaRole.Combobox)).ToHaveValueAsync("Northbank Medical");

        var armed = await desk.Arm(RunGate.Settle, CareEndpoints.SupplierCatalogue);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check delivery", Exact = true }).ClickAsync();

        await Expect(desk.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Delivery window", Exact = true })).ToHaveTextAsync("Checking delivery windows");
        var held = await armed.Held();
        await held.Release();

        await Expect(desk.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Delivery window", Exact = true })).ToHaveTextAsync("Northbank delivers Monday at 09:30");

        await desk.Page.GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await Expect(desk.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Harbor Mobility", Exact = true })).ToBeVisibleAsync();
        await desk.Page.GetByRole(AriaRole.Combobox).PressAsync("ArrowDown");
        await desk.Page.GetByRole(AriaRole.Combobox).PressAsync("Enter");
        await Expect(desk.Page.GetByRole(AriaRole.Combobox)).ToHaveValueAsync("Harbor Mobility");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check delivery", Exact = true }).ClickAsync();

        await Expect(desk.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Delivery window", Exact = true })).ToHaveTextAsync("Harbor delivers Tuesday at 14:00");
    }
}
