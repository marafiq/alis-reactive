using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.NoRequests))]
[Category(Hosts.NoRequestsName)]
public sealed class WhenARequestRendersInAnAppThatNeverCalledAddReactiveRequests(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X10_28_0)]
    public async Task the_page_is_refused_where_it_renders_naming_the_startup_call()
    {
        var neverCalled = await NewWorld();
        await neverCalled.Page.GotoAsync(RefusalPages.RoundsBoard);
        await Expect(neverCalled.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'GET /Rounds' is rendered in an app that never called AddReactiveRequests; "
                + "call services.AddReactiveRequests(requests => ..) once at startup.",
            Exact = true,
        })).ToBeVisibleAsync();

        var calledAtStartup = await NewWorldUnder(Hosts.Net10Name);
        await calledAtStartup.Page.GotoAsync(RefusalPages.RoundsBoard);
        await calledAtStartup.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(calledAtStartup.Page.Locator("#rounds")).ToHaveTextAsync("Morning, Noon, Night");
    }
}
