using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHubPushesTwoArguments(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.X21_4_0)]
    public async Task the_push_runs_no_handler_and_logs_one_error_and_the_next_push_runs()
    {
        var two = await NewWorld();
        await two.Page.GotoAsync(CarePages.LiveRoundFeed);
        await two.Opened(CareEndpoints.RoundFeed);
        await two.Push(CareEndpoints.RoundFeed, "statusChanged", "[{\"resident\":\"Ben\",\"status\":\"Resting\"},{\"resident\":\"Cy\",\"status\":\"Awake\"}]");
        await two.Push(CareEndpoints.RoundFeed, "statusChanged", "[{\"resident\":\"Ada\",\"status\":\"Awake\"}]");
        await Expect(two.Page.Locator("#status-resident")).ToHaveTextAsync("Ada");
        await Expect(two.Page.Locator("#statuses")).ToHaveTextAsync("1");
        Assert.That(two.ConsoleErrors, Is.EqualTo(new[] { "The hub '/care/round/live/endpoints/round-feed' pushed 'statusChanged' with 2 arguments, where its handlers read one object." }));

        var one = await NewWorld();
        await one.Page.GotoAsync(CarePages.LiveRoundFeed);
        await one.Opened(CareEndpoints.RoundFeed);
        await one.Push(CareEndpoints.RoundFeed, "statusChanged", "[{\"resident\":\"Ada\",\"status\":\"Awake\"}]");
        await one.Push(CareEndpoints.RoundFeed, "statusChanged", "[{\"resident\":\"Ben\",\"status\":\"Resting\"}]");
        await Expect(one.Page.Locator("#status-resident")).ToHaveTextAsync("Ben");
        await Expect(one.Page.Locator("#statuses")).ToHaveTextAsync("2");
        Assert.That(one.ConsoleErrors, Is.Empty);
    }
}
