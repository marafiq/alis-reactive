using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandoverNoteIsLeftEmptyOnNet10(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_57_0)]
    public async Task the_note_binds_null_so_its_rule_refuses_it_and_nothing_is_sent()
    {
        var leftEmpty = await NewWorld();
        await leftEmpty.Page.GotoAsync(CarePages.HandoverNotes);
        await leftEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 1", Exact = true }).FillAsync("Fed at noon");
        await leftEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 3", Exact = true }).FillAsync("Walked to the garden");
        await leftEmpty.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Handed over by", Exact = true }).FillAsync("Ann");
        await leftEmpty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync();
        await Expect(leftEmpty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Note 2", Exact = true }).GetByText("Write each note.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(leftEmpty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Handover", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Write each note." });
        await Expect(leftEmpty.Page.Locator("#handover-status")).ToHaveTextAsync("");
        Assert.That(leftEmpty.SentTo(CareEndpoints.NotesHandedOver), Is.EqualTo(0));

        var written = await NewWorld();
        await written.Page.GotoAsync(CarePages.HandoverNotes);
        await written.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 1", Exact = true }).FillAsync("Fed at noon");
        await written.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 2", Exact = true }).FillAsync("Slept after lunch");
        await written.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note 3", Exact = true }).FillAsync("Walked to the garden");
        await written.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Handed over by", Exact = true }).FillAsync("Ann");
        await written.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync();
        await Expect(written.Page.Locator("#handover-status")).ToHaveTextAsync("3 notes handed over by Ann");
        await written.Page.ReloadAsync();
        await Expect(written.Page.Locator("#on-record")).ToHaveTextAsync("Ann: Fed at noon; Slept after lunch; Walked to the garden");
    }
}
