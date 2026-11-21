using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANoteIsPinnedToABoardThatIsClosed(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X5_0)]
    public async Task the_pin_stops_its_own_pipeline_and_the_bell_still_rings()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.PinBoard);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Noticeboard", Exact = true })).ToBeVisibleAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByText("Choir practice moves to Friday.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(closed.Page.Locator("#pin-after")).ToHaveTextAsync("1");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the board", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#board-slot")).ToBeEmptyAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#pin-ended")).ToHaveTextAsync("2");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#page-heard")).ToHaveTextAsync("1");
        await Expect(closed.Page.Locator("#pin-tried")).ToHaveTextAsync("2");
        await Expect(closed.Page.Locator("#pin-after")).ToHaveTextAsync("1");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.PinBoard);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the board", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Noticeboard", Exact = true })).ToBeVisibleAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#pin-after")).ToHaveTextAsync("1");
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin a note to the board", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#pin-ended")).ToHaveTextAsync("2");
        await Expect(open.Page.Locator("#pin-tried")).ToHaveTextAsync("2");
        await Expect(open.Page.Locator("#pin-after")).ToHaveTextAsync("2");
    }
}
