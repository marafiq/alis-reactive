using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAssistantAnswersAPrompt(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F3.M21_X901_0)]
    public async Task the_views_answer_ends_the_wait_and_the_stop_button_leaves()
    {
        var byTheBook = await NewWorld();
        await byTheBook.Page.GotoAsync(CarePages.MedicationAssistant);
        await byTheBook.Page.Locator("#medication-assistant").GetByRole(AriaRole.Textbox).ClickAsync();
        await byTheBook.Page.Locator("#medication-assistant").GetByRole(AriaRole.Textbox).PressSequentiallyAsync("Can Mr Patel have a second dose?");
        await byTheBook.Page.GetByRole(AriaRole.Toolbar, new PageGetByRoleOptions { Name = "assist-footer-toolbar", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();

        await Expect(byTheBook.Page.GetByText("Ask the nurse on duty.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(byTheBook.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Stop Responding", Exact = true })).ToBeHiddenAsync();

        var onCall = await NewWorld();
        await onCall.Page.GotoAsync(CarePages.MedicationAssistantOnCall);
        await onCall.Page.Locator("#medication-assistant").GetByRole(AriaRole.Textbox).ClickAsync();
        await onCall.Page.Locator("#medication-assistant").GetByRole(AriaRole.Textbox).PressSequentiallyAsync("Can Mr Patel have a second dose?");
        await onCall.Page.GetByRole(AriaRole.Toolbar, new PageGetByRoleOptions { Name = "assist-footer-toolbar", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();

        await Expect(onCall.Page.GetByText("Check the chart, then call the pharmacist.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(onCall.Page.GetByText("Ask the nurse on duty.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(onCall.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Stop Responding", Exact = true })).ToBeHiddenAsync();
    }
}
