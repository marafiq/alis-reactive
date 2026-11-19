using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTransportEquipmentIsRequested(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F13.M21_X1213_0)]
    public async Task the_desk_waits_for_stock_then_shows_the_selected_aids_collection_point()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.EquipmentAvailability);
        var armed = await desk.Arm(RunGate.Settle, CareEndpoints.TransportStock);
        await desk.Page.GetByRole(AriaRole.Combobox).PressAsync("ArrowDown");
        var held = await armed.Held();
        await Expect(desk.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Collection instructions", Exact = true })).ToHaveTextAsync("Checking stock");
        await held.Release();

        await Expect(desk.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Collection instructions", Exact = true })).ToHaveTextAsync("Collect chair W-14 from North stores");

        await desk.Page.GetByRole(AriaRole.Combobox).PressAsync("ArrowDown");

        await Expect(desk.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Collection instructions", Exact = true })).ToHaveTextAsync("Collect frame F-08 from South stores");
    }
}
