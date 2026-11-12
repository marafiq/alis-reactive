using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentReceivesDatesAnInstantANullAndItsOwnRecord(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_10_0)]
    public async Task each_date_arrives_as_a_date_the_instant_as_its_moment_the_null_as_null_and_the_record_as_it_is()
    {
        var planner = await NewWorld();
        await planner.Page.GotoAsync(CarePages.VisitPlanner);
        await planner.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take the week", Exact = true }).ClickAsync();
        await planner.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(planner.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[]
            {
                "Date 2026-11-09 09:00",
                "Date 2026-11-09 09:00",
                "Date 2026-11-09 09:00",
                "Date 2026-11-09 09:00",
                "null",
                "Date 2026-11-09 09:00",
                "Date 2026-11-09T14:00:00.000Z",
                "same record",
                "Date 2026-11-12 10:00",
            });

        var texted = await NewWorld();
        await texted.Page.GotoAsync(CarePages.VisitPlanner);
        await texted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log text", Exact = true }).ClickAsync();
        await Expect(texted.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "text 2026-11-09T09:00:00" });
    }
}
