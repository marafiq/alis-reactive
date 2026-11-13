using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheWardTourStartsAtASlide(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F9.M21_X1005_0)]
    public async Task the_carousel_shows_and_reads_the_slide_its_view_set()
    {
        var wardC = await NewWorld();
        await wardC.Page.GotoAsync(CarePages.WardTour);
        await wardC.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Which ward", Exact = true }).ClickAsync();

        await Expect(wardC.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Slide show", Exact = true }).GetByRole(AriaRole.Group)).ToHaveTextAsync("Ward C");
        await Expect(wardC.Page.Locator("#ward-shown")).ToHaveTextAsync("2");

        var wardB = await NewWorld();
        await wardB.Page.GotoAsync(CarePages.WardTourFromWardB);
        await wardB.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Which ward", Exact = true }).ClickAsync();

        await Expect(wardB.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Slide show", Exact = true }).GetByRole(AriaRole.Group)).ToHaveTextAsync("Ward B");
        await Expect(wardB.Page.Locator("#ward-shown")).ToHaveTextAsync("1");
    }
}
