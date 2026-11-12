using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaTrimsANameAnItemWasSentWithout(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_3_0)]
    public async Task the_item_sent_with_no_name_is_not_counted_and_the_step_after_the_count_runs()
    {
        var roster = await NewWorld();
        await roster.Page.GotoAsync(CarePages.RosterWithAnUnnamedResident);
        await roster.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the unnamed", Exact = true }).ClickAsync();
        await Expect(roster.Page.Locator("#unnamed-count")).ToHaveTextAsync("1");
        await Expect(roster.Page.Locator("#roster-status")).ToHaveTextAsync("Counted");

        var withABlankName = await NewWorld();
        await withABlankName.Page.GotoAsync(CarePages.RosterWithAnUnnamedAndABlankResident);
        await withABlankName.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the unnamed", Exact = true }).ClickAsync();
        await Expect(withABlankName.Page.Locator("#unnamed-count")).ToHaveTextAsync("2");
    }
}
