using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARatingIsRenderedForAMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F37.M21_X613_0)]
    public async Task the_rating_starts_at_the_score_the_member_holds()
    {
        var four = await NewWorld();
        await four.Page.GotoAsync(CarePages.LunchRatedFour);

        await Expect(four.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Lunch", Exact = true }).GetByRole(AriaRole.Slider)).ToHaveAttributeAsync("aria-valuenow", "4");

        var two = await NewWorld();
        await two.Page.GotoAsync(CarePages.LunchRatedTwo);

        await Expect(two.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Lunch", Exact = true }).GetByRole(AriaRole.Slider)).ToHaveAttributeAsync("aria-valuenow", "2");
    }
}
