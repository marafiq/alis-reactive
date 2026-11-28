using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAViewAsksTheAssistantToStream(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F3.M21_X1036_0)]
    public async Task the_answer_shows_whole_with_the_pages_timers_held()
    {
        var held = await NewWorld();
        await held.InstallTheClock();
        await held.Page.GotoAsync(CarePages.MedicationAssistantStreamed);
        await held.HoldTheClock();
        await held.Page.Locator("#medication-assistant").GetByRole(AriaRole.Textbox).ClickAsync();
        await held.Page.Locator("#medication-assistant").GetByRole(AriaRole.Textbox).PressSequentiallyAsync("Can Mr Patel have a second dose?");
        await held.Page.GetByRole(AriaRole.Toolbar, new PageGetByRoleOptions { Name = "assist-footer-toolbar", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();

        await Expect(held.Page.GetByText("Ask the nurse on duty, then record the dose.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Stop Responding", Exact = true })).ToBeHiddenAsync();
        await Expect(held.Page.Locator("#medication-assistant").GetByRole(AriaRole.Textbox)).ToHaveAttributeAsync("placeholder", "Ask about a dose");
    }
}
