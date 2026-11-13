using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADrawersSizeIsReadAfterAClose(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F1.M25_X2_0)]
    public async Task the_size_reads_md_after_a_close_and_lg_while_it_is_open_wide()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.WideDrawer);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open wide", Exact = true }).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Complementary).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Close", Exact = true }).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read size", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#size-read")).ToHaveTextAsync("md");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.WideDrawer);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open wide", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read size", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#size-read")).ToHaveTextAsync("lg");
    }
}
