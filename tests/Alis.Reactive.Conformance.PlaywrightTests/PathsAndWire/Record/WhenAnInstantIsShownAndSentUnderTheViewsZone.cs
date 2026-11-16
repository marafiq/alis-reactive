using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnInstantIsShownAndSentUnderTheViewsZone(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_9_0)]
    public async Task each_instant_shows_and_sends_the_views_wall_time_with_that_zones_offset_for_its_day()
    {
        var newYorkView = await NewWorldInZone(Zones.Kolkata);
        await newYorkView.Page.GotoAsync(CarePages.VisitBookInNewYork);
        await newYorkView.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the visits", Exact = true }).ClickAsync();
        await Expect(newYorkView.Page.Locator("#first-visit")).ToHaveTextAsync("2026-01-15T07:00:00-05:00");
        await Expect(newYorkView.Page.Locator("#second-visit")).ToHaveTextAsync("2026-07-15T08:00:00-04:00");
        await newYorkView.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Starts", Exact = true }).FillAsync("2026-07-15T09:30");
        await newYorkView.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Book", Exact = true }).ClickAsync();
        await Expect(newYorkView.Page.Locator("#booked")).ToHaveTextAsync("2026-07-15T09:30:00-04:00");

        var kolkataView = await NewWorldInZone(Zones.NewYork);
        await kolkataView.Page.GotoAsync(CarePages.VisitBookInKolkata);
        await kolkataView.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the visits", Exact = true }).ClickAsync();
        await Expect(kolkataView.Page.Locator("#first-visit")).ToHaveTextAsync("2026-01-15T17:30:00+05:30");
    }
}
