using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestsRouteWritesComponents(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_19_0)]
    public async Task each_component_its_route_writes_is_busy_and_a_route_that_writes_only_inside_a_condition_holds_the_page()
    {
        var panels = await NewWorld();
        await panels.Page.GotoAsync(CarePages.CensusPanels);
        var armedNorth = await panels.Arm(CompositionGate.Answer, CareEndpoints.CensusOfTheNorthPanel);
        var armedSouth = await panels.Arm(CompositionGate.Answer, CareEndpoints.CensusOfTheSouthPanel);
        await panels.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load both", Exact = true }).ClickAsync();
        var heldNorth = await armedNorth.Held();
        var heldSouth = await armedSouth.Held();
        await Expect(panels.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: census-north, census-south");
        await Expect(panels.Page.Locator("#census-north")).ToHaveAttributeAsync("aria-busy", "true");
        await Expect(panels.Page.Locator("#census-south")).ToHaveAttributeAsync("aria-busy", "true");
        await heldNorth.Release();
        await Expect(panels.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: census-south");
        await Expect(panels.Page.Locator("#census-north")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await heldSouth.Release();
        await Expect(panels.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        var armedThird = await panels.Arm(CompositionGate.Answer, CareEndpoints.CensusOfTheThirdCount);
        await panels.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load third", Exact = true }).ClickAsync();
        var heldThird = await armedThird.Held();
        await Expect(panels.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await heldThird.Release();
        await Expect(panels.Page.Locator("#third-status")).ToHaveTextAsync("Counted");
        await Expect(panels.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");

        var inTheirPanel = await NewWorld();
        await inTheirPanel.Page.GotoAsync(CarePages.CensusPanelsBusyInTheirPanel);
        var armedNorthInThePanel = await inTheirPanel.Arm(CompositionGate.Answer, CareEndpoints.CensusOfTheNorthPanel);
        var armedSouthInThePanel = await inTheirPanel.Arm(CompositionGate.Answer, CareEndpoints.CensusOfTheSouthPanel);
        await inTheirPanel.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load both", Exact = true }).ClickAsync();
        var heldNorthInThePanel = await armedNorthInThePanel.Held();
        var heldSouthInThePanel = await armedSouthInThePanel.Held();
        await Expect(inTheirPanel.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: panel");
        await Expect(inTheirPanel.Page.Locator("#panel")).ToHaveAttributeAsync("aria-busy", "true");
        await Expect(inTheirPanel.Page.Locator("#census-north")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await heldNorthInThePanel.Release();
        await heldSouthInThePanel.Release();
        await Expect(inTheirPanel.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
    }
}
