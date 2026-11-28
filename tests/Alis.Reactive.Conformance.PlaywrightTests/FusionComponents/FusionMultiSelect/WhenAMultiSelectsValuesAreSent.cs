using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsValuesAreSent(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.http_post)]
    public async Task the_endpoint_binds_the_codes_the_box_holds()
    {
        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.AllergySaveOfTwo);
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(both.Page.Locator("#saved")).ToHaveTextAsync("Saved 7, 9");

        var none = await NewWorld();
        await none.Page.GotoAsync(CarePages.AllergySaveOfNone);
        await none.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(none.Page.Locator("#saved")).ToHaveTextAsync("Saved none");
    }
}
