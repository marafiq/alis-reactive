using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheServerDropsAStream(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.X21_3_0)]
    public async Task the_page_runs_its_reconnecting_steps_then_its_reconnected_steps_and_the_next_message_runs()
    {
        var dropped = await NewWorld();
        await dropped.Page.GotoAsync(CarePages.LiveNews);
        await dropped.Opened(CareEndpoints.WardBoard);
        var drop = await dropped.Drop(CareEndpoints.WardBoard);
        await Expect(dropped.Page.Locator("#board-dropped")).ToHaveTextAsync("dropped");
        await drop.NextHeld();
        await Expect(dropped.Page.Locator("#board-back")).ToBeEmptyAsync();
        await drop.Reopen();
        await Expect(dropped.Page.Locator("#board-back")).ToHaveTextAsync("back");
        await dropped.Opened(CareEndpoints.WardBoard);
        await dropped.Push(CareEndpoints.WardBoard, "news", "{\"text\":\"Fall in room 4\"}");
        await Expect(dropped.Page.Locator("#news")).ToHaveTextAsync("Fall in room 4");

        var steady = await NewWorld();
        await steady.Page.GotoAsync(CarePages.LiveNews);
        await steady.Opened(CareEndpoints.WardBoard);
        await steady.Push(CareEndpoints.WardBoard, "news", "{\"text\":\"Ada is up\"}");
        await Expect(steady.Page.Locator("#news")).ToHaveTextAsync("Ada is up");
        await Expect(steady.Page.Locator("#board-dropped")).ToBeEmptyAsync();
        await Expect(steady.Page.Locator("#board-back")).ToBeEmptyAsync();
    }
}
