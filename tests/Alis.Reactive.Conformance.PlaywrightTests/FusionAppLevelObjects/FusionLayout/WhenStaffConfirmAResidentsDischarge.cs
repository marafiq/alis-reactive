using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.FusionShell))]
[Category(Hosts.FusionShellName)]
public sealed class WhenStaffConfirmAResidentsDischarge(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M23_F2.M23_X2_0)]
    public async Task the_question_shows_the_persons_words_as_plain_text()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.FusionDischarge);
        await world.Page.GetByLabel("Question").FillAsync("<b>Discharge Ada?</b>");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asking = world.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Confirm", Exact = true });
        await Expect(asking.Locator("#alisConfirmDialog_dialog-content")).ToHaveTextAsync("<b>Discharge Ada?</b>");
        await asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#discharge-decision")).ToHaveTextAsync("Discharged");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.FusionDischarge);
        await plain.Page.GetByLabel("Question").FillAsync("Discharge Bo?");
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asked = plain.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Confirm", Exact = true });
        await Expect(asked.Locator("#alisConfirmDialog_dialog-content")).ToHaveTextAsync("Discharge Bo?");
        await asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#discharge-decision")).ToHaveTextAsync("Discharged");
    }

    [Test]
    [FailsFor(Promises.M23_F2.M23_X3_0)]
    public async Task the_title_and_each_answer_use_the_layouts_language()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.FusionDischargeInSpanish);
        var discharge = world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Dar de alta", Exact = true });
        var asking = world.Page.GetByRole(AriaRole.Dialog);
        var yes = asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Sí", Exact = true });
        var no = asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Cancelar", Exact = true });
        var decision = world.Page.Locator("#discharge-decision");

        await discharge.ClickAsync();
        await Expect(asking.Locator("#alisConfirmDialog_title")).ToHaveTextAsync("Confirmar");
        await Expect(yes).ToBeVisibleAsync();
        await no.ClickAsync();
        await Expect(decision).ToHaveTextAsync("Sigue");

        await discharge.ClickAsync();
        await Expect(asking.Locator("#alisConfirmDialog_dialog-content")).ToHaveTextAsync("¿Dar de alta a Ada?");
        await world.Page.Keyboard.PressAsync("Escape");
        await Expect(asking).ToBeHiddenAsync();
        await Expect(decision).ToHaveTextAsync("Sigue");

        await discharge.ClickAsync();
        await Expect(no).ToBeVisibleAsync();
        await yes.ClickAsync();
        await Expect(decision).ToHaveTextAsync("Alta");

        var defaults = await NewWorld();
        await defaults.Page.GotoAsync(CarePages.FusionDischarge);
        await defaults.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        var asked = defaults.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Confirm", Exact = true });
        await Expect(asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Cancel", Exact = true })).ToBeVisibleAsync();
        await asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK", Exact = true }).ClickAsync();
        await Expect(defaults.Page.Locator("#discharge-decision")).ToHaveTextAsync("Discharged");
    }
}
