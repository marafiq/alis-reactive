using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaOrdersADayARecordHoldsAsText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_58_0)]
    public async Task a_day_held_outside_its_forms_stops_the_pipeline_and_a_day_no_month_holds_reads_nothing()
    {
        var slashed = await NewWorld();
        await slashed.Page.GotoAsync(CarePages.CensusDaysWithADayHeldAsSlashedText);
        await slashed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(slashed.Page.Locator("#earlier-count")).ToHaveTextAsync("-");
        await Expect(slashed.Page.Locator("#count-log")).ToHaveTextAsync("");
        Assert.That(slashed.ConsoleErrors, Has.Count.EqualTo(1));

        var dated = await NewWorld();
        await dated.Page.GotoAsync(CarePages.CensusDaysWithEveryDayHeldAsADate);
        await dated.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(dated.Page.Locator("#earlier-count")).ToHaveTextAsync("2");
        await Expect(dated.Page.Locator("#count-log")).ToHaveTextAsync("checked");

        var noSuchDay = await NewWorld();
        await noSuchDay.Page.GotoAsync(CarePages.CensusDaysWithADayNoMonthHolds);
        await noSuchDay.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(noSuchDay.Page.Locator("#earlier-count")).ToHaveTextAsync("1");
        await Expect(noSuchDay.Page.Locator("#count-log")).ToHaveTextAsync("checked");
        Assert.That(noSuchDay.ConsoleErrors, Is.Empty);
    }
}
