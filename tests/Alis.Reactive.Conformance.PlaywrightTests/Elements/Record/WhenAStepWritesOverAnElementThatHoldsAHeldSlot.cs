using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesOverAnElementThatHoldsAHeldSlot(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.X12_12_2)]
    public async Task the_step_stops_and_the_tools_stay_inside_the_board_while_an_element_that_holds_no_slot_takes_its_text()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.TeamBoard);
        await board.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board's tools", Exact = true }).ClickAsync();
        await Expect(board.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Day shift", Exact = true })).ToBeVisibleAsync();
        await board.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the board", Exact = true }).ClickAsync();
        await Expect(board.Page.Locator("#board-status")).ToHaveTextAsync("Clearing");
        await Expect(board.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Day shift", Exact = true })).ToBeVisibleAsync();
        Assert.That(board.ConsoleErrors, Has.Count.EqualTo(1));

        var noticeboard = await NewWorld();
        await noticeboard.Page.GotoAsync(CarePages.TeamBoard);
        await noticeboard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the noticeboard", Exact = true }).ClickAsync();
        await Expect(noticeboard.Page.Locator("#noticeboard")).ToHaveTextAsync("Noticeboard cleared");
        Assert.That(noticeboard.ConsoleErrors, Is.Empty);
    }
}
