using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepClosesAStream(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.M10_X4_0)]
    public async Task the_stream_stays_closed_runs_no_closed_steps_and_shows_no_reconnect_offer()
    {
        var stopped = await NewWorld();
        await stopped.Page.GotoAsync(CarePages.LiveAlerts);
        await stopped.Opened(CareEndpoints.FacilityAlerts);
        await stopped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Stop live alerts", Exact = true }).ClickAsync();
        await Expect(stopped.Page.Locator("#alerts-state")).ToHaveTextAsync("stopped");
        await stopped.Closed(CareEndpoints.FacilityAlerts);
        await stopped.Push(CareEndpoints.FacilityAlerts, "alert", "{\"message\":\"All clear\"}");
        Assert.That(stopped.Reached(CareEndpoints.FacilityAlerts), Is.EqualTo(0));
        await Expect(stopped.Page.Locator("#alert")).ToBeEmptyAsync();
        await Expect(stopped.Page.Locator("#alerts-closed")).ToBeEmptyAsync();
        await Expect(stopped.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();

        var live = await NewWorld();
        await live.Page.GotoAsync(CarePages.LiveAlerts);
        await live.Opened(CareEndpoints.FacilityAlerts);
        await live.Push(CareEndpoints.FacilityAlerts, "alert", "{\"message\":\"All clear\"}");
        await Expect(live.Page.Locator("#alert")).ToHaveTextAsync("All clear");
    }
}
