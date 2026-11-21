using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.OwnershipAndState;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenADeskLoadsUnderAnotherTimeZone(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M18_F1.M18_X1_0)]
    public async Task each_visit_is_shown_under_its_own_plans_time_zone()
    {
        var kolkataDesk = await NewWorldInZone(Zones.Utc);
        await kolkataDesk.Page.GotoAsync(CarePages.VisitBoardInNewYork);
        await kolkataDesk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Kolkata desk", Exact = true }).ClickAsync();
        await kolkataDesk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show desk visit", Exact = true }).ClickAsync();
        await Expect(kolkataDesk.Page.Locator("#desk-visit")).ToHaveTextAsync("2026-01-15T17:30:00+05:30");
        await kolkataDesk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show visit", Exact = true }).ClickAsync();
        await Expect(kolkataDesk.Page.Locator("#board-visit")).ToHaveTextAsync("2026-01-15T07:00:00-05:00");

        var newYorkDesk = await NewWorldInZone(Zones.Utc);
        await newYorkDesk.Page.GotoAsync(CarePages.VisitBoardWithNewYorkDeskInNewYork);
        await newYorkDesk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Kolkata desk", Exact = true }).ClickAsync();
        await newYorkDesk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show desk visit", Exact = true }).ClickAsync();
        await Expect(newYorkDesk.Page.Locator("#desk-visit")).ToHaveTextAsync("2026-01-15T07:00:00-05:00");
    }
}
