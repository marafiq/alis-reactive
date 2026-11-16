using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANativeFormIsSent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_15_0)]
    public async Task its_submission_set_holds_the_control_it_owns_outside_its_element_its_hand_written_input_and_one_antiforgery_field()
    {
        var owned = await NewWorld();
        await owned.Page.GotoAsync(CarePages.IntakeAdmit);
        await owned.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await owned.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East");
        await owned.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Code", Exact = true }).FillAsync("A7");
        await owned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(owned.Page.Locator("#received")).ToHaveTextAsync("Ward East, Code A7, fields 1");

        var apart = await NewWorld();
        await apart.Page.GotoAsync(CarePages.IntakeAdmitWardApart);
        await apart.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await apart.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East");
        await apart.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Code", Exact = true }).FillAsync("A7");
        await apart.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(apart.Page.Locator("#received")).ToHaveTextAsync("Ward none, Code A7, fields 1");
    }
}
