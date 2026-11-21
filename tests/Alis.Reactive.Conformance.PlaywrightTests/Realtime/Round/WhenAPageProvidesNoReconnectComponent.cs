using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.NoReconnectComponent))]
[Category(Hosts.NoReconnectComponentName)]
public sealed class WhenAPageProvidesNoReconnectComponent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.X21_12_0)]
    public async Task the_page_opens_no_stream_and_logs_one_error()
    {
        var unoffered = await NewWorld();
        await unoffered.Page.GotoAsync(CarePages.LiveNews);
        await Expect(unoffered.Page.Locator("#board-status")).ToHaveTextAsync("started");
        Assert.That(unoffered.ConsoleErrors, Is.EqualTo(new[] { "The stream '/care/round/live/endpoints/ward-board' did not open: No script provides the plugin 'reconnect'." }));
        await unoffered.Push(CareEndpoints.WardBoard, "news", "{\"text\":\"Ada is up\"}");
        Assert.That(unoffered.Reached(CareEndpoints.WardBoard), Is.EqualTo(0));
        await Expect(unoffered.Page.Locator("#news")).ToBeEmptyAsync();

        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.RecordCard);
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Flag", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#record-card")).ToHaveClassAsync("card flagged");
        Assert.That(record.ConsoleErrors, Is.Empty);
    }
}
