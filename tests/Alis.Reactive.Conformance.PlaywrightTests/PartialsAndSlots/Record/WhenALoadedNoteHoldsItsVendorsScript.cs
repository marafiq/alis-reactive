using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALoadedNoteHoldsItsVendorsScript(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X2_0)]
    public async Task the_notes_script_runs_once_at_each_load_and_stamps_the_one_note_it_came_with()
    {
        var pinned = await NewWorld();
        await pinned.Page.GotoAsync(CarePages.StampedNote);
        await pinned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board", Exact = true }).ClickAsync();
        await Expect(pinned.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Noticeboard", Exact = true })).ToBeVisibleAsync();
        await pinned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(pinned.Page.GetByText("The hall closes at nine.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(pinned.Page.Locator("#note-stamp")).ToHaveTextAsync("stamped by its own script");

        var pinnedTwice = await NewWorld();
        await pinnedTwice.Page.GotoAsync(CarePages.StampedNote);
        await pinnedTwice.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board", Exact = true }).ClickAsync();
        await Expect(pinnedTwice.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Noticeboard", Exact = true })).ToBeVisibleAsync();
        await pinnedTwice.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(pinnedTwice.Page.Locator("#notes-pinned")).ToHaveTextAsync("1");
        await pinnedTwice.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(pinnedTwice.Page.Locator("#notes-pinned")).ToHaveTextAsync("2");
        await Expect(pinnedTwice.Page.Locator("#note-stamp")).ToHaveCountAsync(1);
        await Expect(pinnedTwice.Page.Locator("#note-stamp")).ToHaveTextAsync("stamped by its own script");
    }
}
