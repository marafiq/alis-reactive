using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesANullsFallback(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_6_2)]
    public async Task a_null_weight_compares_as_its_fallback_and_a_held_weight_as_itself()
    {
        var unweighed = await NewWorld();
        await unweighed.Page.GotoAsync(CarePages.WeightCheckOfAnUnweighedResident);
        await unweighed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(unweighed.Page.Locator("#weight-badge")).ToHaveTextAsync("Heavy");

        var weighed = await NewWorld();
        await weighed.Page.GotoAsync(CarePages.WeightCheckOfAWeighedResident);
        await weighed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(weighed.Page.Locator("#weight-badge")).ToHaveTextAsync("Light");
    }
}
