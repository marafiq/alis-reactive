using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListViewUnselectsAnItemOfObjects(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F28.M21_X509_1)]
    public async Task a_step_unselects_the_item_its_object_names_and_no_other()
    {
        var withLena = await NewWorld();
        await withLena.Page.GotoAsync(CarePages.RoundResidentsWithLena);
        await Expect(withLena.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Lena Ortiz" })).ToHaveClassAsync("e-list-item e-level-1 e-active");
        await withLena.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Unselect Lena", Exact = true }).ClickAsync();

        await Expect(withLena.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Lena Ortiz" })).ToHaveClassAsync("e-list-item e-level-1");
        await Expect(withLena.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Ada Brooks" })).ToHaveClassAsync("e-list-item e-level-1");

        var withAda = await NewWorld();
        await withAda.Page.GotoAsync(CarePages.RoundResidentsWithLena);
        await Expect(withAda.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Lena Ortiz" })).ToHaveClassAsync("e-list-item e-level-1 e-active");
        await withAda.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Ada Brooks" }).ClickAsync();
        await Expect(withAda.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Ada Brooks" })).ToHaveClassAsync("e-list-item e-level-1 e-active");
        await withAda.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Unselect Lena", Exact = true }).ClickAsync();
        await Expect(withAda.Page.Locator("#round-status")).ToHaveTextAsync("Lena Ortiz off the round");

        await Expect(withAda.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Ada Brooks" })).ToHaveClassAsync("e-list-item e-level-1 e-active");
    }
}
