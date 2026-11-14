using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardsStepWritesOverTheSlotItWasLoadedInto(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.X12_12_1)]
    public async Task the_step_stops_and_the_tools_stay_as_they_were_loaded_while_a_note_beside_them_takes_its_text()
    {
        var tray = await NewWorld();
        await tray.Page.GotoAsync(CarePages.ShiftTray);
        await tray.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the tray", Exact = true }).ClickAsync();
        await Expect(tray.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeVisibleAsync();
        await tray.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the tray", Exact = true }).ClickAsync();
        await Expect(tray.Page.Locator("#tray-status")).ToHaveTextAsync("Clearing");
        await Expect(tray.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeVisibleAsync();
        Assert.That(tray.ConsoleErrors, Has.Count.EqualTo(1));

        var marked = await NewWorld();
        await marked.Page.GotoAsync(CarePages.ShiftTray);
        await marked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the tray", Exact = true }).ClickAsync();
        await Expect(marked.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeVisibleAsync();
        await marked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark the shift", Exact = true }).ClickAsync();
        await Expect(marked.Page.Locator("#shift-note")).ToHaveTextAsync("Shift marked");
        Assert.That(marked.ConsoleErrors, Is.Empty);
    }
}
