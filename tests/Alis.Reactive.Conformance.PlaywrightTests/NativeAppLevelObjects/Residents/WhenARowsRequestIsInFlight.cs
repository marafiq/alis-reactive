using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowsRequestIsInFlight(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F3.X17_38_0)]
    public async Task only_the_clicked_rows_control_shows_busy()
    {
        var bea = await NewWorld();
        await bea.Page.GotoAsync(CarePages.BusyDischarges);
        var armed = await bea.Arm(LanesGate.Respond, CareEndpoints.BeasDischarge);
        await bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();
        var held = await armed.Held();
        await Expect(bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button)).ToHaveAttributeAsync("aria-busy", "true");
        await Expect(bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ada", Exact = true }).GetByRole(AriaRole.Button)).Not.ToHaveAttributeAsync("aria-busy", "true");
        await Expect(bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Cy", Exact = true }).GetByRole(AriaRole.Button)).Not.ToHaveAttributeAsync("aria-busy", "true");
        await held.Release();
        await Expect(bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button)).ToHaveTextAsync("Discharged");
        await Expect(bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button)).Not.ToHaveAttributeAsync("aria-busy", "true");

        var ada = await NewWorld();
        await ada.Page.GotoAsync(CarePages.BusyDischarges);
        var armedAda = await ada.Arm(LanesGate.Respond, CareEndpoints.AdasDischarge);
        await ada.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ada", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();
        var heldAda = await armedAda.Held();
        await Expect(ada.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ada", Exact = true }).GetByRole(AriaRole.Button)).ToHaveAttributeAsync("aria-busy", "true");
        await Expect(ada.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button)).Not.ToHaveAttributeAsync("aria-busy", "true");
        await heldAda.Release();
        await Expect(ada.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ada", Exact = true }).GetByRole(AriaRole.Button)).ToHaveTextAsync("Discharged");
    }
}
