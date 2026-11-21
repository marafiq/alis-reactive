using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStreamSendsDataThatIsNotJson(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.X21_1_0)]
    public async Task the_message_runs_no_handler_and_logs_one_error_and_the_next_message_runs()
    {
        var unreadable = await NewWorld();
        await unreadable.Page.GotoAsync(CarePages.LiveBulletins);
        await unreadable.Opened(CareEndpoints.WardBoard);
        await unreadable.Push(CareEndpoints.WardBoard, "bulletin", "a bulletin");
        await unreadable.Push(CareEndpoints.WardBoard, "allClear", "{}");
        await Expect(unreadable.Page.Locator("#all-clear")).ToHaveTextAsync("clear");
        await Expect(unreadable.Page.Locator("#bulletins")).ToHaveTextAsync("0");
        Assert.That(unreadable.ConsoleErrors, Is.EqualTo(new[] { "The stream '/care/round/live/endpoints/ward-board' sent the event 'bulletin', whose data is not JSON." }));

        var readable = await NewWorld();
        await readable.Page.GotoAsync(CarePages.LiveBulletins);
        await readable.Opened(CareEndpoints.WardBoard);
        await readable.Push(CareEndpoints.WardBoard, "bulletin", "{}");
        await Expect(readable.Page.Locator("#bulletins")).ToHaveTextAsync("1");
        Assert.That(readable.ConsoleErrors, Is.Empty);
    }
}
