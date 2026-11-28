using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnItemsPositionIsBelowZero(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F2.M4_X5_0)]
    public async Task the_page_is_refused_where_the_view_renders_since_a_position_is_zero_or_more()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.TheRoomBeforeTheFirst);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: An item's position is zero or more.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.TheFirstRoom);
        await Expect(rendered.Page.Locator("#item")).ToHaveTextAsync("Hall");
    }
}
