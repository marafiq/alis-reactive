using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADropDownButtonIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F19.M21_X1012_0)]
    public async Task the_button_lists_the_views_items_and_the_chosen_one_reaches_the_views_handler()
    {
        var skipping = await NewWorld();
        await skipping.Page.GotoAsync(CarePages.DoseRow);
        await Expect(skipping.Page.Locator("#dose-actions")).ToHaveTextAsync("Dose actions");
        await skipping.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Dose actions", Exact = true }).ClickAsync();
        await skipping.Page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions { Name = "Skip dose", Exact = true }).ClickAsync();

        await Expect(skipping.Page.Locator("#dose-choice")).ToHaveTextAsync("Skip dose");

        var giving = await NewWorld();
        await giving.Page.GotoAsync(CarePages.DoseRow);
        await giving.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Dose actions", Exact = true }).ClickAsync();
        await giving.Page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions { Name = "Give dose", Exact = true }).ClickAsync();

        await Expect(giving.Page.Locator("#dose-choice")).ToHaveTextAsync("Give dose");
    }
}
