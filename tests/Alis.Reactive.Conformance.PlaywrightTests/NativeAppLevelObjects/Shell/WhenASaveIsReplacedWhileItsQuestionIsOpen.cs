using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASaveIsReplacedWhileItsQuestionIsOpen(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F4.X17_8_0)]
    public async Task its_question_closes_and_only_the_second_saves_question_shows()
    {
        var replaced = await NewWorld();
        var armed = await replaced.Arm(CompositionGate.Answer, CareEndpoints.DischargeIntakeAmendedName);
        await replaced.Page.GotoAsync(CarePages.DischargeIntake);
        await Expect(replaced.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Discharge intake of Ada", Exact = true })).ToBeVisibleAsync();
        var held = await armed.Held();
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        var asking = replaced.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await Expect(asking.GetByText("Save the intake for Ada?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await held.Release();
        await Expect(asking.GetByText("Save the intake for Ada Lovelace?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#saved")).ToHaveTextAsync("1 Ada Lovelace");
        await Expect(asking).ToBeHiddenAsync();

        var answered = await NewWorld();
        var armedAgain = await answered.Arm(CompositionGate.Answer, CareEndpoints.DischargeIntakeAmendedName);
        await answered.Page.GotoAsync(CarePages.DischargeIntake);
        await Expect(answered.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Discharge intake of Ada", Exact = true })).ToBeVisibleAsync();
        var heldAgain = await armedAgain.Held();
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        var asked = answered.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true });
        await asked.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(answered.Page.Locator("#saved")).ToHaveTextAsync("1 Ada");
        await heldAgain.Release();
    }
}
