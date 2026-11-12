using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComparisonReadsAMissingWardAndANullPriority(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F2.X8_7_0)]
    public async Task not_equal_and_a_negated_equal_keep_the_dose_and_an_ordering_leaves_the_null_out()
    {
        var wardless = await NewWorld();
        await wardless.Page.GotoAsync(CarePages.DoseRoundWithAWardlessDose);
        await wardless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(wardless.Page.Locator("#outside-north-count")).ToHaveTextAsync("2");
        await Expect(wardless.Page.Locator("#not-in-north-count")).ToHaveTextAsync("2");
        await Expect(wardless.Page.Locator("#not-second-priority-count")).ToHaveTextAsync("3");
        await Expect(wardless.Page.Locator("#top-priority-count")).ToHaveTextAsync("1");

        var warded = await NewWorld();
        await warded.Page.GotoAsync(CarePages.DoseRoundWithEveryDoseInAWard);
        await warded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(warded.Page.Locator("#outside-north-count")).ToHaveTextAsync("1");
        await Expect(warded.Page.Locator("#not-in-north-count")).ToHaveTextAsync("1");
    }
}
