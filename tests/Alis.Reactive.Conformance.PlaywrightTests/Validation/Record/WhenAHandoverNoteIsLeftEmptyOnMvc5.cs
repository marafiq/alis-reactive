using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAHandoverNoteIsLeftEmptyOnMvc5(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_57_1)]
    public async Task the_note_binds_an_empty_text_so_its_rule_passes_and_the_handover_is_saved()
    {
        var leftEmpty = await NewWorld();
        await leftEmpty.Page.GotoAsync(CarePages.HandoverNotes);
        await leftEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 1", Exact = true }).FillAsync("Fed at noon");
        await leftEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 3", Exact = true }).FillAsync("Walked to the garden");
        await leftEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Handed over by", Exact = true }).FillAsync("Ann");
        await leftEmpty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync();
        await Expect(leftEmpty.Page.Locator("#handover-status")).ToHaveTextAsync("3 notes handed over by Ann");
        await leftEmpty.Page.ReloadAsync();
        await Expect(leftEmpty.Page.Locator("#on-record")).ToHaveTextAsync("Ann: Fed at noon; ; Walked to the garden");

        var nobodyNamed = await NewWorld();
        await nobodyNamed.Page.GotoAsync(CarePages.HandoverNotes);
        await nobodyNamed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 1", Exact = true }).FillAsync("Fed at noon");
        await nobodyNamed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 2", Exact = true }).FillAsync("Slept after lunch");
        await nobodyNamed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 3", Exact = true }).FillAsync("Walked to the garden");
        await nobodyNamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync();
        await Expect(nobodyNamed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Handed over by", Exact = true }).GetByText("Name who hands over.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(nobodyNamed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Handover", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Name who hands over." });
        await Expect(nobodyNamed.Page.Locator("#handover-status")).ToHaveTextAsync("");
        Assert.That(nobodyNamed.SentTo(CareEndpoints.NotesHandedOver), Is.EqualTo(0));
    }
}
