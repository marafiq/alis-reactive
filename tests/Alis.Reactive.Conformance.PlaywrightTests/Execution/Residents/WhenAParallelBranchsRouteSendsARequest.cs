using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAParallelBranchsRouteSendsARequest(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_16_0)]
    public async Task the_steps_after_parallel_run_after_the_routes_request_settles()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.ResidentBoard);
        var armed = await board.Arm(LanesGate.Settle, CareEndpoints.ResidentBoardAllergies);

        await board.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load board", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(board.Page.Locator("#board-status")).ToHaveTextAsync("");
        await held.Release();

        await Expect(board.Page.Locator("#board-status")).ToHaveTextAsync("Board ready");

        var withoutAllergies = await NewWorld();
        await withoutAllergies.Page.GotoAsync(CarePages.ResidentBoard);
        await withoutAllergies.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load board without allergies", Exact = true }).ClickAsync();
        await Expect(withoutAllergies.Page.Locator("#board-status")).ToHaveTextAsync("Board ready");
    }
}
