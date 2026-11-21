using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageScriptPreventsEveryClick(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F3.M25_X4_0)]
    public async Task the_rows_link_still_loads_its_panel()
    {
        var guarded = await NewWorld();
        await guarded.Page.GotoAsync(CarePages.ResidentLinksBehindAClickGuard);
        await guarded.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ada", Exact = true }).ClickAsync();
        await Expect(guarded.Page.Locator("#resident-detail").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Ada");
        await Expect(guarded.Page).ToHaveURLAsync(CarePages.ResidentLinksBehindAClickGuard);

        var unguarded = await NewWorld();
        await unguarded.Page.GotoAsync(CarePages.ResidentLinksWithNoClickGuard);
        await unguarded.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ada", Exact = true }).ClickAsync();
        await Expect(unguarded.Page.Locator("#resident-detail").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Ada");
    }
}
