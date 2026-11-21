using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABalanceIsShownUnderTheRequestsCulture(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_8_0)]
    public async Task the_balance_shows_as_the_servers_view_shows_it()
    {
        var german = await NewWorld();
        await german.Page.GotoAsync(CarePages.ResidentAccountInGerman);
        await german.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the balance", Exact = true }).ClickAsync();
        await Expect(german.Page.Locator("#balance")).ToHaveTextAsync("1234,5");
        await Expect(german.Page.Locator("#server-balance")).ToHaveTextAsync("1234,5");

        var english = await NewWorld();
        await english.Page.GotoAsync(CarePages.ResidentAccountInEnglish);
        await english.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the balance", Exact = true }).ClickAsync();
        await Expect(english.Page.Locator("#balance")).ToHaveTextAsync("1234.5");
        await Expect(english.Page.Locator("#server-balance")).ToHaveTextAsync("1234.5");
    }
}
