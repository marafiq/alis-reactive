using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowsLinkIsClickedWhileAnotherRowsRequestIsInFlight(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F3.X17_36_0)]
    public async Task under_replace_the_panel_shows_the_row_clicked_last()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.ResidentDirectoryUnderReplace);
        var armed = await replaced.Arm(LanesGate.Respond, CareEndpoints.AdasResidentPanel);
        await replaced.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ada", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await replaced.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Bea", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#resident-detail").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Bea");
        await held.Release();
        await Expect(replaced.Page.Locator("#resident-detail").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Bea");

        var overlapped = await NewWorld();
        await overlapped.Page.GotoAsync(CarePages.ResidentDirectoryUnderOverlap);
        var armedAgain = await overlapped.Arm(LanesGate.Respond, CareEndpoints.AdasResidentPanel);
        await overlapped.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ada", Exact = true }).ClickAsync();
        var heldAgain = await armedAgain.Held();
        await overlapped.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Bea", Exact = true }).ClickAsync();
        await Expect(overlapped.Page.Locator("#resident-detail").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Bea");
        await heldAgain.Release();
        await Expect(overlapped.Page.Locator("#resident-detail").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Ada");
    }
}
