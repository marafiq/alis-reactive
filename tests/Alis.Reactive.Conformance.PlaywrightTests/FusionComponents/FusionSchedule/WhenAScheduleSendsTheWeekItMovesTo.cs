using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAScheduleSendsTheWeekItMovesTo(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F39.M21_X306_2)]
    public async Task the_body_sends_the_day_it_moves_to_as_a_date_with_no_offset()
    {
        var next = await NewWorld();
        await next.Page.GotoAsync(CarePages.RotaWeekShifts);
        var sent = await next.Page.RunAndWaitForRequestAsync(() => next.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next", Exact = true }).ClickAsync(), "**/rota-week-shifts/shifts");

        Assert.That(sent.Method, Is.EqualTo("POST"));
        Assert.That(sent.PostData, Is.EqualTo("{\"currentDate\":\"2026-11-16\"}"));

        var previous = await NewWorld();
        await previous.Page.GotoAsync(CarePages.RotaWeekShifts);
        var sentBack = await previous.Page.RunAndWaitForRequestAsync(() => previous.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Previous", Exact = true }).ClickAsync(), "**/rota-week-shifts/shifts");

        Assert.That(sentBack.PostData, Is.EqualTo("{\"currentDate\":\"2026-11-02\"}"));
    }
}
