using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEnumsFallbackStandsInForAChartThatReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_12_0)]
    public async Task the_fallback_compares_equal_to_the_literal_as_the_members_own_converter_writes_it()
    {
        var chartless = await NewWorld();
        await chartless.Page.GotoAsync(CarePages.MedicationCheckOfAResidentWithNoChart);
        await chartless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the chart", Exact = true }).ClickAsync();
        await Expect(chartless.Page.Locator("#chart-resident")).ToHaveTextAsync("Ben Okafor");
        await chartless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(chartless.Page.Locator("#dose-badge")).ToHaveTextAsync("Given");

        var missed = await NewWorld();
        await missed.Page.GotoAsync(CarePages.MedicationCheckOfAResidentWhoseChartSaysMissed);
        await missed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the chart", Exact = true }).ClickAsync();
        await Expect(missed.Page.Locator("#chart-resident")).ToHaveTextAsync("Ada Hart");
        await missed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(missed.Page.Locator("#dose-badge")).ToHaveTextAsync("Not given");
    }
}
