using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectIsLoadedIntoACard(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.http_into)]
    public async Task the_loaded_card_shows_the_box_holding_its_members_allergy()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.AllergyCard);
        await Expect(helen.Page.Locator("#card")).ToBeEmptyAsync();
        await helen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allergies", Exact = true }).ClickAsync();

        await Expect(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Penicillin", new LocatorGetByTextOptions { Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToBeVisibleAsync();
    }
}
