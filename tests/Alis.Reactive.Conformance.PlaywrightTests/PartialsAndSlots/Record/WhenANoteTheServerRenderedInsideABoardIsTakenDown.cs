using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANoteTheServerRenderedInsideABoardIsTakenDown(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X3_0)]
    public async Task the_notes_handler_leaves_with_the_note_slot_and_the_hall_still_hears_the_bell()
    {
        var takenDown = await NewWorld();
        await takenDown.Page.GotoAsync(CarePages.StandingNote);
        await takenDown.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board", Exact = true }).ClickAsync();
        await Expect(takenDown.Page.GetByText("Lunch is served at noon.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await takenDown.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(takenDown.Page.Locator("#note-heard")).ToHaveTextAsync("1");
        await takenDown.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take the note down", Exact = true }).ClickAsync();
        await Expect(takenDown.Page.Locator("#standing-note-slot")).ToBeEmptyAsync();
        await takenDown.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(takenDown.Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(takenDown.Page.Locator("#note-heard")).ToHaveTextAsync("1");

        var leftUp = await NewWorld();
        await leftUp.Page.GotoAsync(CarePages.StandingNote);
        await leftUp.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board", Exact = true }).ClickAsync();
        await Expect(leftUp.Page.GetByText("Lunch is served at noon.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await leftUp.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(leftUp.Page.Locator("#note-heard")).ToHaveTextAsync("1");
        await leftUp.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(leftUp.Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(leftUp.Page.Locator("#note-heard")).ToHaveTextAsync("2");
    }
}
