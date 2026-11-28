using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnOnLoadDispatchesAnEventAnotherPlanHandles(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F1.M1_X1_0)]
    public async Task the_later_plan_and_the_slots_own_plan_hear_it()
    {
        var board = await NewWorld();
        await board.Page.GotoAsync(CarePages.RoundBoard);
        await Expect(board.Page.Locator("#round-status")).ToHaveTextAsync("Round started");
        await board.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the handover", Exact = true }).ClickAsync();
        await Expect(board.Page.Locator("#handover-status")).ToHaveTextAsync("Slot opened");
        await Expect(board.Page.Locator("#round-status")).ToHaveTextAsync("Slot opened");

        var unheard = await NewWorld();
        await unheard.Page.GotoAsync(CarePages.RoundBoard);
        await Expect(unheard.Page.Locator("#round-status")).ToHaveTextAsync("Round started");
        await unheard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the porter", Exact = true }).ClickAsync();
        await Expect(unheard.Page.Locator("#round-status")).ToHaveTextAsync("Round started");
    }
}
