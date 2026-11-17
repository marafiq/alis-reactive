using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepCallsAHub(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.M10_X3_0)]
    public async Task a_call_before_the_first_start_waits_for_it_and_one_while_the_hub_reconnects_stops_its_pipeline()
    {
        var round = await NewWorld();
        await round.Page.GotoAsync(CarePages.LiveWardLine);
        await Expect(round.Page.Locator("#roster")).ToHaveTextAsync("Ada, Ben");
        Assert.That(round.ConsoleErrors, Is.Empty);
        var line = await round.Drop(CareEndpoints.WardLine);
        await Expect(round.Page.Locator("#line-dropped")).ToHaveTextAsync("dropped");
        await line.NextHeld();
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ask for the roster", Exact = true }).ClickAsync();
        await line.Reopen();
        await Expect(round.Page.Locator("#line-back")).ToHaveTextAsync("back");
        await Expect(round.Page.Locator("#roster-again")).ToBeEmptyAsync();
        Assert.That(round.ConsoleErrors, Is.EqualTo(new[] { "An awaited step rejected, so its pipeline stops: The hub '/care/round/live/endpoints/ward-line' did not run Roster: Cannot send data if the connection is not in the 'Connected' State." }));
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ask for the roster", Exact = true }).ClickAsync();
        await Expect(round.Page.Locator("#roster-again")).ToHaveTextAsync("Ada, Ben");
        Assert.That(round.ConsoleErrors, Is.EqualTo(new[] { "An awaited step rejected, so its pipeline stops: The hub '/care/round/live/endpoints/ward-line' did not run Roster: Cannot send data if the connection is not in the 'Connected' State." }));
    }
}
