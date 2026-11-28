using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DefaultHooks))]
[Category(Hosts.DefaultHooksName)]
public sealed class WhenADrawersCancelIsWrittenAfterTheDefaultsLoadingAwaits(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_18_0)]
    public async Task the_discharge_page_is_refused_where_the_view_renders()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.Index);
        await refused.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Discharge Ada", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A write to the event's payload after an awaited step reaches no vendor: "
                + "write it before the first request, Confirm or async method.",
            Exact = true,
        })).ToBeVisibleAsync();

        var awaitingNothing = await NewWorldUnder(Hosts.Net10Name);
        await awaitingNothing.Page.GotoAsync(RefusalPages.Index);
        await awaitingNothing.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Discharge Ada", Exact = true }).ClickAsync();
        await awaitingNothing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open discharge", Exact = true }).ClickAsync();
        var drawer = awaitingNothing.Page.GetByRole(AriaRole.Complementary, new PageGetByRoleOptions { Name = "Discharge Ada", Exact = true });
        await drawer.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        await Expect(awaitingNothing.Page.Locator("#discharge-sent")).ToHaveTextAsync("Discharge sent");
        await Expect(drawer).ToBeVisibleAsync();
    }
}
