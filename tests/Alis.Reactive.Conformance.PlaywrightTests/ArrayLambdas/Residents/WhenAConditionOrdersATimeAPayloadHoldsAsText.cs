using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionOrdersATimeAPayloadHoldsAsText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_58_1)]
    public async Task a_time_held_outside_its_forms_stops_the_pipeline_and_a_time_no_day_holds_reads_nothing()
    {
        var census = await NewWorld();
        await census.Page.GotoAsync(CarePages.CensusRaisedTime);
        await census.Page.Locator("#census").GetByRole(AriaRole.Searchbox).FillAsync("7:00 PM");
        await census.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(census.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(census.Page.Locator("#part-of-day")).ToHaveTextAsync("-");
        await Expect(census.Page.Locator("#time-log")).ToHaveTextAsync("");
        Assert.That(census.ConsoleErrors, Has.Count.EqualTo(1));
        await census.Page.Locator("#census").GetByRole(AriaRole.Searchbox).FillAsync("19:00");
        await census.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(census.Page.Locator("#part-of-day")).ToHaveTextAsync("afternoon");
        await Expect(census.Page.Locator("#time-log")).ToHaveTextAsync("checked");

        var morning = await NewWorld();
        await morning.Page.GotoAsync(CarePages.CensusRaisedTime);
        await morning.Page.Locator("#census").GetByRole(AriaRole.Searchbox).FillAsync("07:00");
        await morning.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(morning.Page.Locator("#part-of-day")).ToHaveTextAsync("morning");

        var noSuchTime = await NewWorld();
        await noSuchTime.Page.GotoAsync(CarePages.CensusRaisedTime);
        await noSuchTime.Page.Locator("#census").GetByRole(AriaRole.Searchbox).FillAsync("24:00");
        await noSuchTime.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(noSuchTime.Page.Locator("#part-of-day")).ToHaveTextAsync("afternoon");
        await Expect(noSuchTime.Page.Locator("#time-log")).ToHaveTextAsync("checked");
        Assert.That(noSuchTime.ConsoleErrors, Is.Empty);
    }
}
