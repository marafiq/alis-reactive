using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestsBusyTargetIsInAPartialNotLoaded(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_17_0)]
    public async Task the_request_stops_before_it_sends()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.WeekSummary);

        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load summary", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#summary-status")).ToHaveTextAsync("Loading");
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the summary", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "This week", Exact = true })).ToBeVisibleAsync();
        Assert.That(record.SentTo(CareEndpoints.WeekSummaryNotes), Is.EqualTo(0));

        var sent = await record.Page.RunAndWaitForRequestAsync(() => record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load summary", Exact = true }).ClickAsync(), "**" + CareEndpoints.WeekSummaryNotes);
        Assert.That(sent.Method, Is.EqualTo("GET"));
        await Expect(record.Page.Locator("#summary-notes")).ToHaveTextAsync("4 notes this week");
        await Expect(record.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show summary-tab; hide summary-tab; show summary-panel; hide summary-panel");
        Assert.That(record.SentTo(CareEndpoints.WeekSummaryNotes), Is.EqualTo(1));
    }
}
