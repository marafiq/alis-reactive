using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowIsEditedInTheDialog(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F23.M21_X204_1)]
    public async Task the_dialogs_select_opens_at_the_status_the_row_holds()
    {
        var discharged = await NewWorld();
        await discharged.Page.GotoAsync(CarePages.StatusCorrectionDialog);
        await discharged.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "Discharged", Exact = true }).ClickAsync();
        await discharged.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit", Exact = true }).ClickAsync();

        await Expect(discharged.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Status", Exact = true })).ToHaveValueAsync("Discharged");

        var onLeave = await NewWorld();
        await onLeave.Page.GotoAsync(CarePages.StatusCorrectionDialog);
        await onLeave.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "On leave", Exact = true }).ClickAsync();
        await onLeave.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit", Exact = true }).ClickAsync();

        await Expect(onLeave.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Status", Exact = true })).ToHaveValueAsync("On leave");
    }
}
