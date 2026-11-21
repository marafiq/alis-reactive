using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TypeKnowledge;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.JsonContract))]
[Category(Hosts.JsonContractName)]
public sealed class WhenTheAppsJsonContractLeavesAMemberOut(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M16_F1.X18_6_0)]
    public async Task the_body_leaves_the_member_out_and_the_check_reads_its_start()
    {
        var leftOut = await NewWorld();
        await leftOut.Page.GotoAsync(CarePages.WardPharmacyAdmission);
        await leftOut.Page.GetByLabel("Given name", new PageGetByLabelOptions { Exact = true }).FillAsync("Rose");
        await leftOut.Page.GetByLabel("Ward", new PageGetByLabelOptions { Exact = true }).FillAsync("East");
        await leftOut.Page.GetByLabel("Pharmacy", new PageGetByLabelOptions { Exact = true }).FillAsync("North");
        await leftOut.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(leftOut.Page.Locator("#admit-status")).ToHaveTextAsync("admitted");
        await Expect(leftOut.Page.GetByText("New admissions go to Rehab.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(leftOut.Page.GetByText("New admissions use the central pharmacy.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var inItsContract = await NewWorld();
        await inItsContract.Page.GotoAsync(CarePages.WardPharmacyAdmissionInItsContract);
        await inItsContract.Page.GetByLabel("Given name", new PageGetByLabelOptions { Exact = true }).FillAsync("Rose");
        await inItsContract.Page.GetByLabel("Ward", new PageGetByLabelOptions { Exact = true }).FillAsync("East");
        await inItsContract.Page.GetByLabel("Pharmacy", new PageGetByLabelOptions { Exact = true }).FillAsync("North");
        await inItsContract.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(inItsContract.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Ward placement", Exact = true })
            .GetByText("New admissions go to Rehab.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(inItsContract.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Pharmacy choice", Exact = true })
            .GetByText("New admissions use the central pharmacy.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(inItsContract.Page.Locator("#admit-status")).ToHaveTextAsync("");
    }
}
