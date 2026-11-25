using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.AntiforgeryField))]
[Category(Hosts.AntiforgeryFieldName)]
public sealed class WhenAContainerHoldsAControlTheViewWroteByHand(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_15_0)]
    public async Task it_sends_its_framework_inputs_the_input_outside_it_and_one_antiforgery_field_and_leaves_the_hand_written_control_out()
    {
        var wardSent = await NewWorld();
        await wardSent.Page.GotoAsync(CarePages.IntakeAdmit);
        await wardSent.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await wardSent.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East");
        await wardSent.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Code", Exact = true }).FillAsync("A7");
        await wardSent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(wardSent.Page.Locator("#received")).ToHaveTextAsync("Ward East, Code none, fields 1");

        var wardLeftOut = await NewWorld();
        await wardLeftOut.Page.GotoAsync(CarePages.IntakeAdmitWithoutTheWard);
        await wardLeftOut.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await wardLeftOut.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East");
        await wardLeftOut.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Code", Exact = true }).FillAsync("A7");
        await wardLeftOut.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(wardLeftOut.Page.Locator("#received")).ToHaveTextAsync("Ward none, Code none, fields 1");
    }
}
