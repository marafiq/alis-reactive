using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesOverASlotThatHoldsALoadedCard(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.X12_12_0)]
    public async Task the_card_stays_as_it_was_loaded_and_the_text_is_set_once_the_slot_is_empty_and_unnamed()
    {
        var loaded = await NewWorld();
        await loaded.Page.GotoAsync(CarePages.NotesDesk);
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the card", Exact = true }).ClickAsync();
        await Expect(loaded.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night note", Exact = true })).ToBeVisibleAsync();
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark no card open", Exact = true }).ClickAsync();
        await Expect(loaded.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night note", Exact = true })).ToBeVisibleAsync();
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Acknowledge", Exact = true }).ClickAsync();
        await Expect(loaded.Page.Locator("#note-acknowledged")).ToHaveTextAsync("Acknowledged");
        Assert.That(loaded.ConsoleErrors, Has.Count.EqualTo(1));

        var emptied = await NewWorld();
        await emptied.Page.GotoAsync(CarePages.NotesDesk);
        await emptied.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await emptied.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the card", Exact = true }).ClickAsync();
        await Expect(emptied.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night note", Exact = true })).ToBeVisibleAsync();
        await emptied.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await emptied.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the desk", Exact = true }).ClickAsync();
        await emptied.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark no card open", Exact = true }).ClickAsync();
        await Expect(emptied.Page.Locator("#note-card")).ToHaveTextAsync("No card open");
        Assert.That(emptied.ConsoleErrors, Is.Empty);
    }
}
