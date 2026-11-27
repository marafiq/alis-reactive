using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.FusionShell))]
[Category(Hosts.FusionShellName)]
public sealed class WhenStaffChooseACareWorkflow(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F51.M21_X1032_0)]
    public async Task the_selected_workflow_names_the_command_staff_clicked()
    {
        var discharge = await NewWorld();
        await discharge.Page.GotoAsync(CarePages.FusionCareToolbar);
        await Expect(discharge.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ward workflows", Exact = true })).ToBeVisibleAsync();
        var toolbar = discharge.Page.GetByRole(AriaRole.Toolbar);
        var selected = discharge.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Selected workflow", Exact = true });
        await Expect(toolbar.GetByRole(AriaRole.Button)).ToHaveCountAsync(2);
        await Expect(selected).ToHaveTextAsync("Nothing clicked");
        await toolbar.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        await Expect(selected).ToHaveTextAsync("Discharge");

        var admit = await NewWorld();
        await admit.Page.GotoAsync(CarePages.FusionCareToolbar);
        await admit.Page.GetByRole(AriaRole.Toolbar).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(admit.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Selected workflow", Exact = true })).ToHaveTextAsync("Admit");
    }
}
