using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoParallelRequestsAnswerApart(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_6_0)]
    public async Task the_steps_after_parallel_run_once_after_the_slower_answers()
    {
        var residentsFirst = await NewWorld();
        await residentsFirst.Page.GotoAsync(CarePages.WingCounts);
        var armedAtResidents = await residentsFirst.Arm(LanesGate.Settle, CareEndpoints.WingCountsResidents);
        var armedAtDischarges = await residentsFirst.Arm(LanesGate.Settle, CareEndpoints.WingCountsDischarges);

        await residentsFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh counts", Exact = true }).ClickAsync();
        var heldAtResidents = await armedAtResidents.Held();
        var heldAtDischarges = await armedAtDischarges.Held();
        await heldAtResidents.Release();
        await Expect(residentsFirst.Page.Locator("#activity-log")).ToHaveTextAsync("residents");
        await heldAtDischarges.Release();

        await Expect(residentsFirst.Page.Locator("#activity-log")).ToHaveTextAsync("residents discharges settled");

        var dischargesFirst = await NewWorld();
        await dischargesFirst.Page.GotoAsync(CarePages.WingCounts);
        var armedResidents = await dischargesFirst.Arm(LanesGate.Settle, CareEndpoints.WingCountsResidents);
        var armedDischarges = await dischargesFirst.Arm(LanesGate.Settle, CareEndpoints.WingCountsDischarges);

        await dischargesFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh counts", Exact = true }).ClickAsync();
        var heldResidents = await armedResidents.Held();
        var heldDischarges = await armedDischarges.Held();
        await heldDischarges.Release();
        await Expect(dischargesFirst.Page.Locator("#activity-log")).ToHaveTextAsync("discharges");
        await heldResidents.Release();

        await Expect(dischargesFirst.Page.Locator("#activity-log")).ToHaveTextAsync("discharges residents settled");
    }
}
