using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListViewSelectsAnItemOfObjects(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F28.M21_X509_0)]
    public async Task a_step_selects_the_item_its_object_names()
    {
        var stepped = await NewWorld();
        await stepped.Page.GotoAsync(CarePages.RoundResidents);
        await Expect(stepped.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Lena Ortiz" })).ToHaveClassAsync("e-list-item e-level-1");
        await stepped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Select Lena", Exact = true }).ClickAsync();

        await Expect(stepped.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Lena Ortiz" })).ToHaveClassAsync("e-list-item e-level-1 e-active");
        await Expect(stepped.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Ada Brooks" })).ToHaveClassAsync("e-list-item e-level-1");

        var chosenByHand = await NewWorld();
        await chosenByHand.Page.GotoAsync(CarePages.RoundResidents);
        await chosenByHand.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Ada Brooks" }).ClickAsync();

        await Expect(chosenByHand.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Ada Brooks" })).ToHaveClassAsync("e-list-item e-level-1 e-active");
        await Expect(chosenByHand.Page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "Lena Ortiz" })).ToHaveClassAsync("e-list-item e-level-1");
    }
}
