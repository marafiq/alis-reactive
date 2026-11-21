using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowsButtonIsClicked(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F3.X17_33_0)]
    public async Task the_action_runs_for_the_row_clicked_and_relabels_its_own_control()
    {
        var bea = await NewWorld();
        await bea.Page.GotoAsync(CarePages.ResidentDischarges);
        await bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(bea.Page.Locator("#status")).ToHaveTextAsync("Bea discharged");
        await Expect(bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button)).ToHaveTextAsync("Discharged");
        await Expect(bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ada", Exact = true }).GetByRole(AriaRole.Button)).ToHaveTextAsync("Discharge");
        await Expect(bea.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Cy", Exact = true }).GetByRole(AriaRole.Button)).ToHaveTextAsync("Discharge");

        var ada = await NewWorld();
        await ada.Page.GotoAsync(CarePages.ResidentDischarges);
        await ada.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ada", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(ada.Page.Locator("#status")).ToHaveTextAsync("Ada discharged");
        await Expect(ada.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ada", Exact = true }).GetByRole(AriaRole.Button)).ToHaveTextAsync("Discharged");
        await Expect(ada.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button)).ToHaveTextAsync("Discharge");
        await Expect(ada.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Cy", Exact = true }).GetByRole(AriaRole.Button)).ToHaveTextAsync("Discharge");
    }
}
