using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.RealtimeNoOffer))]
[Category(Hosts.RealtimeNoOfferName)]
public sealed class WhenAPageNamesAStreamWithNoReconnectChoice(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.X21_11_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_both_choices()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.ActivityBoard);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A page that names a stream or a hub needs the app's reconnect choice: call ShowReconnect() or NoReconnectOffer() in AddReactiveRealtime.",
            Exact = true,
        })).ToBeVisibleAsync();

        var unoffered = await NewWorldUnder(Hosts.NoReconnectOfferName);
        await unoffered.Page.GotoAsync(RefusalPages.ActivityBoard);
        await Expect(unoffered.Page.Locator("#activity-news")).ToHaveTextAsync("Bingo at three");
    }
}
