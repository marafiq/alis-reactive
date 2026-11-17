using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageNamesNoHub(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.X21_5_0)]
    public async Task the_page_requests_no_signalr_client_and_a_page_that_names_a_hub_requests_it_once()
    {
        var round = await NewWorld();
        await round.Page.GotoAsync(CarePages.LiveAlerts);
        await round.Opened(CareEndpoints.FacilityAlerts);
        await round.Push(CareEndpoints.FacilityAlerts, "alert", "{\"message\":\"Fire drill at noon\"}");
        await Expect(round.Page.Locator("#alert")).ToHaveTextAsync("Fire drill at noon");
        Assert.That(round.SentTo(FrameworkAssets.HubClient), Is.EqualTo(0));
        await round.Page.GotoAsync(CarePages.LiveWardLine);
        await Expect(round.Page.Locator("#line-call")).ToHaveTextAsync("1");
        Assert.That(round.SentTo(FrameworkAssets.HubClient), Is.EqualTo(1));
    }
}
