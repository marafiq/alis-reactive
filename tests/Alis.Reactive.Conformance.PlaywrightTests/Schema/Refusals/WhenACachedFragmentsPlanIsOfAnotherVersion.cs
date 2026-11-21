using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Schema;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACachedFragmentsPlanIsOfAnotherVersion(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M14_F1.X19_3_0)]
    public async Task the_plan_is_refused_with_both_versions_and_its_handler_never_starts()
    {
        var ward = await NewWorld();
        await ward.Seed(Seed.CachedRoundsOfPlanVersion(6));
        await ward.Page.GotoAsync(RefusalPages.Ward);

        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark the round" }).ClickAsync();

        await Expect(ward.Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("");
        Assert.That(ward.ConsoleErrors, Is.EqualTo(new[] { "This plan was written for plan version 6; this page runs version 7." }));

        var current = await NewWorld();
        await current.Seed(Seed.CachedRoundsOfPlanVersion(7));
        await current.Page.GotoAsync(RefusalPages.Ward);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark the round" }).ClickAsync();

        await Expect(current.Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("done");
        Assert.That(current.ConsoleErrors, Is.Empty);
    }
}
