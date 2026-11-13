using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Schema;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoPlansHandleEventsOfOneElement(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M14_F1.X19_4_0)]
    public async Task each_handler_reads_the_payload_its_own_event_hands()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SchemaElementPartials);
        await bench.Page.Locator("#element").GetByRole(AriaRole.Textbox).ClickAsync();
        await bench.Page.Locator("#element").GetByRole(AriaRole.Textbox).PressAsync("7");
        await Expect(bench.Page.Locator("#pressed-key")).ToHaveTextAsync("7");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open with room", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#opened-room")).ToHaveTextAsync("4");

        var roomFive = await NewWorld();
        await roomFive.Page.GotoAsync(BenchPages.SchemaElementPartialsForRoomFive);
        await roomFive.Page.Locator("#element").GetByRole(AriaRole.Textbox).ClickAsync();
        await roomFive.Page.Locator("#element").GetByRole(AriaRole.Textbox).PressAsync("8");
        await Expect(roomFive.Page.Locator("#pressed-key")).ToHaveTextAsync("8");
        await roomFive.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open with room", Exact = true }).ClickAsync();
        await Expect(roomFive.Page.Locator("#opened-room")).ToHaveTextAsync("5");
    }
}
