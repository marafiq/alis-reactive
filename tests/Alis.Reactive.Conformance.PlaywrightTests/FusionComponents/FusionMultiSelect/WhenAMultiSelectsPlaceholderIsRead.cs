using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsPlaceholderIsRead(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.prop_scalar_read)]
    public async Task the_hint_shows_the_placeholder_the_box_holds()
    {
        var hinted = await NewWorld();
        await hinted.Page.GotoAsync(CarePages.AllergyHintGiven);
        await hinted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show hint", Exact = true }).ClickAsync();

        await Expect(hinted.Page.Locator("#hint")).ToHaveTextAsync("Choose Helen's allergies");

        var unhinted = await NewWorld();
        await unhinted.Page.GotoAsync(CarePages.AllergyHintUnset);
        await unhinted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show hint", Exact = true }).ClickAsync();

        await Expect(unhinted.Page.Locator("#hint")).ToHaveTextAsync("");
    }
}
