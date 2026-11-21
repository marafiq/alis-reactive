using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsListIsShownByAStep(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.method_void)]
    public async Task the_list_opens_with_every_allergy()
    {
        var south = await NewWorld();
        await south.Page.GotoAsync(CarePages.AllergyListOnTheSouthWard);
        await south.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show allergies", Exact = true }).ClickAsync();

        await Expect(south.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Penicillin", "Latex", "Peanut" });

        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.AllergyListOnTheNorthWard);
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show allergies", Exact = true }).ClickAsync();

        await Expect(north.Page.Locator("#ward-list")).ToHaveTextAsync("Kept closed");
        await Expect(north.Page.GetByRole(AriaRole.Listbox)).ToBeHiddenAsync();
    }
}
