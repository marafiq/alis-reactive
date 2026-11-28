using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.FusionShell))]
[Category(Hosts.FusionShellName)]
public sealed class WhenStaffHideTheResidentSidebar(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F40.M21_X1024_0)]
    public async Task residents_start_open_or_closed_as_configured_and_hide_on_request()
    {
        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.FusionResidentSidebar);
        var openResidents = open.Page.GetByText("Residents", new PageGetByTextOptions { Exact = true });
        await Expect(openResidents).ToBeVisibleAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hide residents", Exact = true }).ClickAsync();
        await Expect(openResidents).ToBeHiddenAsync();

        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.FusionResidentSidebarCollapsed);
        var closedResidents = closed.Page.GetByText("Residents", new PageGetByTextOptions { Exact = true });
        await Expect(closedResidents).ToBeHiddenAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show residents", Exact = true }).ClickAsync();
        await Expect(closedResidents).ToBeVisibleAsync();
    }
}
