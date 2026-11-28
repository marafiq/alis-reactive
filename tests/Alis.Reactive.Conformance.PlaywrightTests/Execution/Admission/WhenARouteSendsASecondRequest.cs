using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARouteSendsASecondRequest(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_14_0)]
    public async Task finally_chained_and_the_step_after_run_after_the_routes_request_settles()
    {
        var assigning = await NewWorld();
        await assigning.Page.GotoAsync(CarePages.BedAdmission);
        var armed = await assigning.Arm(LanesGate.SettleTheTail, CareEndpoints.BedAdmissionBeds);

        await assigning.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(assigning.Page.Locator("#admission-log")).ToHaveTextAsync("Admitted");
        await held.Release();

        await Expect(assigning.Page.Locator("#admission-log")).ToHaveTextAsync("Admitted Bed assigned Finally Chained After");

        var withNoBedRequest = await NewWorld();
        await withNoBedRequest.Page.GotoAsync(CarePages.BedAdmissionWithNoBedRequest);
        await withNoBedRequest.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(withNoBedRequest.Page.Locator("#admission-log")).ToHaveTextAsync("Admitted Bed assigned Finally Chained After");
    }
}
