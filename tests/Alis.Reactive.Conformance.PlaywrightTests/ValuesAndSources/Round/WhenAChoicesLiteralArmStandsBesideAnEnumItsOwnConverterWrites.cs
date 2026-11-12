using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAChoicesLiteralArmStandsBesideAnEnumItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_12_1)]
    public async Task the_literal_arm_compares_equal_to_the_literal_as_the_members_own_converter_writes_it()
    {
        var ticked = await NewWorld();
        await ticked.Page.GotoAsync(CarePages.MedicationTestCheck);
        await ticked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the chart", Exact = true }).ClickAsync();
        await Expect(ticked.Page.Locator("#chart-resident")).ToHaveTextAsync("Ada Hart");
        await ticked.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Test", Exact = true }).CheckAsync();
        await ticked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(ticked.Page.Locator("#dose-badge")).ToHaveTextAsync("Given");

        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.MedicationTestCheck);
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the chart", Exact = true }).ClickAsync();
        await Expect(cleared.Page.Locator("#chart-resident")).ToHaveTextAsync("Ada Hart");
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(cleared.Page.Locator("#dose-badge")).ToHaveTextAsync("Not given");
    }
}
