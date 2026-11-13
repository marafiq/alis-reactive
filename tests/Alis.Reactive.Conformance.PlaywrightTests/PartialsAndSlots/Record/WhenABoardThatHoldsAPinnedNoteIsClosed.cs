using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABoardThatHoldsAPinnedNoteIsClosed(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.X12_1_0)]
    public async Task the_board_and_the_note_inside_it_stop_hearing_the_bell_and_the_hall_still_hears_it()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.HallNotices);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board", Exact = true }).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByText("The hall closes at nine.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#note-heard")).ToHaveTextAsync("1");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the board", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#board-slot")).ToBeEmptyAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(closed.Page.Locator("#board-heard")).ToHaveTextAsync("1");
        await Expect(closed.Page.Locator("#note-heard")).ToHaveTextAsync("1");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.HallNotices);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByText("The hall closes at nine.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#note-heard")).ToHaveTextAsync("1");
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(open.Page.Locator("#board-heard")).ToHaveTextAsync("2");
        await Expect(open.Page.Locator("#note-heard")).ToHaveTextAsync("2");
    }
}
