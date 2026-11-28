using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectTagsAnItem(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.event_object)]
    public async Task the_tagged_items_name_is_shown()
    {
        var latex = await NewWorld();
        await latex.Page.GotoAsync(CarePages.AllergyTagging);
        await latex.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await latex.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Latex", Exact = true }).ClickAsync();

        await Expect(latex.Page.Locator("#tagged")).ToHaveTextAsync("Latex");

        var peanut = await NewWorld();
        await peanut.Page.GotoAsync(CarePages.AllergyTagging);
        await peanut.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await peanut.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Peanut", Exact = true }).ClickAsync();

        await Expect(peanut.Page.Locator("#tagged")).ToHaveTextAsync("Peanut");
    }
}
