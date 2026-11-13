using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Schema;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoCachedFragmentsHoldPlansThePageRefuses(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M14_F1.M14_X1_0)]
    public async Task the_pages_own_plan_starts_and_each_refused_script_shows_its_own_error()
    {
        var ward = await NewWorld();
        await ward.Seed(Seed.CachedRoundsOfPlanVersion(6));
        await ward.Seed(Seed.CachedVitalsCutOffInsideAnObject());
        await ward.Page.GotoAsync(CarePages.WardOverview);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the ward", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#ward-name")).ToHaveTextAsync("North");
        Assert.That(ward.ConsoleErrors, Has.Count.EqualTo(2));
        Assert.That(ward.ConsoleErrors, Has.Member("This plan was written for plan version 6; this page runs version 7."));

        var whole = await NewWorld();
        await whole.Seed(Seed.CachedRoundsOfPlanVersion(7));
        await whole.Seed(Seed.CachedVitalsOfPlanVersion(7));
        await whole.Page.GotoAsync(CarePages.WardOverview);
        await whole.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the ward", Exact = true }).ClickAsync();
        await Expect(whole.Page.Locator("#ward-name")).ToHaveTextAsync("North");
        Assert.That(whole.ConsoleErrors, Is.Empty);
    }
}
