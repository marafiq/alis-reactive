using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAButtonIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F8.M21_X1004_0)]
    public async Task the_button_shows_the_views_content_and_a_step_reads_it_at_its_id()
    {
        var toGive = await NewWorld();
        await toGive.Page.GotoAsync(CarePages.DoseCardToGive);
        await Expect(toGive.Page.Locator("#give-dose")).ToHaveTextAsync("Give dose");
        await toGive.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read label", Exact = true }).ClickAsync();

        await Expect(toGive.Page.Locator("#dose-label")).ToHaveTextAsync("Give dose");

        var toSkip = await NewWorld();
        await toSkip.Page.GotoAsync(CarePages.DoseCardToSkip);
        await Expect(toSkip.Page.Locator("#give-dose")).ToHaveTextAsync("Skip dose");
        await toSkip.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read label", Exact = true }).ClickAsync();

        await Expect(toSkip.Page.Locator("#dose-label")).ToHaveTextAsync("Skip dose");
    }
}
