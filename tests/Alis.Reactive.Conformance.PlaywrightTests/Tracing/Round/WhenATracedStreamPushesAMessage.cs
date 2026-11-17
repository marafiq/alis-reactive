using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenATracedStreamPushesAMessage(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";

    [Test]
    [FailsFor(Promises.M10_F1.X21_15_0)]
    public async Task the_messages_run_lists_under_the_pages_trace_with_its_transport_and_event()
    {
        var traced = await NewWorld();
        await traced.Page.GotoAsync(CarePages.LiveTracedBulletins);
        await traced.Opened(CareEndpoints.WardBoard);
        await traced.Push(CareEndpoints.WardBoard, "bulletin", "{}");
        await Expect(traced.Page.Locator("#bulletins")).ToHaveTextAsync("1");
        await traced.Page.GotoAsync(WorldPages.Collector);
        await Expect(traced.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Render + " > alis.run completed",
        ]);
        await traced.Page.GetByRole(AriaRole.Searchbox, new PageGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("server-sent events");
        await traced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search", Exact = true }).ClickAsync();
        await Expect(traced.Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Spans holding server-sent events: 1");
        await Expect(traced.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync([Render + " > alis.run completed"]);
        await traced.Page.GetByRole(AriaRole.Searchbox, new PageGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("bulletin");
        await traced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search", Exact = true }).ClickAsync();
        await Expect(traced.Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Spans holding bulletin: 1");
        await Expect(traced.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync([Render + " > alis.run completed"]);

        var untraced = await NewWorld();
        await untraced.Seed(Seed.TheRenderIsNotRecorded());
        await untraced.Page.GotoAsync(CarePages.LiveTracedBulletins);
        await untraced.Opened(CareEndpoints.WardBoard);
        await untraced.Push(CareEndpoints.WardBoard, "bulletin", "{}");
        await Expect(untraced.Page.Locator("#bulletins")).ToHaveTextAsync("1");
        await untraced.Page.GotoAsync(WorldPages.Collector);
        await Expect(untraced.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Collector", Exact = true })).ToBeVisibleAsync();
        await Expect(untraced.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
