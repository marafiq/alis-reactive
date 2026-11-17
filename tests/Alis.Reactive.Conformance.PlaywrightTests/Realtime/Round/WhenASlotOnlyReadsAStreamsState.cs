using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotOnlyReadsAStreamsState(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.X21_2_0)]
    public async Task the_slot_holds_the_stream_open_while_it_is_loaded()
    {
        var card = await NewWorld();
        await card.Page.GotoAsync(CarePages.LiveBoardCard);
        await Expect(card.Page.Locator("#board-lines")).ToHaveTextAsync("0");
        await card.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the bed card", Exact = true }).ClickAsync();
        await Expect(card.Page.Locator("#board-lines")).ToHaveTextAsync("1");
        await card.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the board's state", Exact = true }).ClickAsync();
        await Expect(card.Page.Locator("#board-state")).ToHaveTextAsync("open");
        await card.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the bed card", Exact = true }).ClickAsync();
        await Expect(card.Page.Locator("#board-lines")).ToHaveTextAsync("0");

        var unopened = await NewWorld();
        await unopened.Page.GotoAsync(CarePages.LiveBoardCard);
        await Expect(unopened.Page.Locator("#board-lines")).ToHaveTextAsync("0");
        await Expect(unopened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the board's state", Exact = true })).ToHaveCountAsync(0);
    }
}
