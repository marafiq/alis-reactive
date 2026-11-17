using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenATracedClickInvokesAHub(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";

    [Test]
    [FailsFor(Promises.M10_F1.X21_16_0)]
    public async Task the_run_lists_under_the_pages_trace_and_no_request_of_the_hub_carries_its_header()
    {
        var traced = await NewWorld();
        await traced.Page.GotoAsync(CarePages.LiveTracedNurseLine);
        await traced.Opened(CareEndpoints.NurseLine);
        await traced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call nurse", Exact = true }).ClickAsync();
        await Expect(traced.Page.Locator("#called")).ToHaveTextAsync("none");
        await traced.Page.GotoAsync(WorldPages.Collector);
        await Expect(traced.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Render + " > alis.run completed",
            Render + " > alis.run > alis.hub completed",
        ]);

        var untraced = await NewWorld();
        await untraced.Seed(Seed.TheRenderIsNotRecorded());
        await untraced.Page.GotoAsync(CarePages.LiveTracedNurseLine);
        await untraced.Opened(CareEndpoints.NurseLine);
        await untraced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call nurse", Exact = true }).ClickAsync();
        await Expect(untraced.Page.Locator("#called")).ToHaveTextAsync("none");
        await untraced.Page.GotoAsync(WorldPages.Collector);
        await Expect(untraced.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Collector", Exact = true })).ToBeVisibleAsync();
        await Expect(untraced.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
