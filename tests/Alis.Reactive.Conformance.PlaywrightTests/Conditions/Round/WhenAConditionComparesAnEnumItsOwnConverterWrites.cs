using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesAnEnumItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_23_0)]
    public async Task the_literal_equals_the_found_status_as_the_members_own_converter_writes_it()
    {
        var charted = await NewWorld();
        await charted.Page.GotoAsync(CarePages.MedicationChartWithAspirinGiven);
        await charted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(charted.Page.Locator("#round-status")).ToHaveTextAsync("Given");
        await charted.Page.GotoAsync(CarePages.MedicationChartWithAspirinDue);
        await charted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(charted.Page.Locator("#round-status")).ToHaveTextAsync("Due");

        var uncharted = await NewWorld();
        await uncharted.Page.GotoAsync(CarePages.MedicationChartWithNoAspirin);
        await uncharted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(uncharted.Page.Locator("#round-status")).ToHaveTextAsync("Due");
    }
}
