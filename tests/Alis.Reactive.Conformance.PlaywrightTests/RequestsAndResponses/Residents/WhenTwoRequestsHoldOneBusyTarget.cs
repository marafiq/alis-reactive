using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoRequestsHoldOneBusyTarget(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_16_0)]
    public async Task a_target_stays_busy_until_the_last_request_that_holds_it_answers()
    {
        var shared = await NewWorld();
        await shared.Page.GotoAsync(CarePages.SummaryOfOneCard);
        var armedFirst = await shared.Arm(CompositionGate.Answer, CareEndpoints.SummaryOfTheFirstWing);
        var armedSecond = await shared.Arm(CompositionGate.Answer, CareEndpoints.SummaryOfTheSecondWing);
        await shared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        var heldFirst = await armedFirst.Held();
        var heldSecond = await armedSecond.Held();
        await Expect(shared.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: summary-card");
        await heldFirst.Release();
        await Expect(shared.Page.Locator("#summary-card")).ToHaveTextAsync("first wing");
        await Expect(shared.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: summary-card");
        await Expect(shared.Page.Locator("#summary-card")).ToHaveAttributeAsync("aria-busy", "true");
        await heldSecond.Release();
        await Expect(shared.Page.Locator("#summary-card")).ToHaveTextAsync("second wing");
        await Expect(shared.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(shared.Page.Locator("#summary-card")).Not.ToHaveAttributeAsync("aria-busy", "true");

        var twoCards = await NewWorld();
        await twoCards.Page.GotoAsync(CarePages.SummaryOfTwoCards);
        var armedAtTheFirstCard = await twoCards.Arm(CompositionGate.Answer, CareEndpoints.SummaryOfTheFirstWing);
        var armedAtTheSecondCard = await twoCards.Arm(CompositionGate.Answer, CareEndpoints.SummaryOfTheSecondWing);
        await twoCards.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        var heldAtTheFirstCard = await armedAtTheFirstCard.Held();
        var heldAtTheSecondCard = await armedAtTheSecondCard.Held();
        await Expect(twoCards.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: first-card, second-card");
        await heldAtTheFirstCard.Release();
        await Expect(twoCards.Page.Locator("#first-card")).ToHaveTextAsync("first wing");
        await Expect(twoCards.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: second-card");
        await Expect(twoCards.Page.Locator("#first-card")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await Expect(twoCards.Page.Locator("#second-card")).ToHaveAttributeAsync("aria-busy", "true");
        await heldAtTheSecondCard.Release();
        await Expect(twoCards.Page.Locator("#second-card")).ToHaveTextAsync("second wing");
        await Expect(twoCards.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");

        var together = await NewWorld();
        await together.Page.GotoAsync(CarePages.SummaryOfOneCard);
        var armedTogetherFirst = await together.Arm(CompositionGate.Answer, CareEndpoints.SummaryOfTheFirstWing);
        var armedTogetherSecond = await together.Arm(CompositionGate.Answer, CareEndpoints.SummaryOfTheSecondWing);
        await together.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        var heldTogetherFirst = await armedTogetherFirst.Held();
        var heldTogetherSecond = await armedTogetherSecond.Held();
        await heldTogetherFirst.Release();
        await heldTogetherSecond.Release();
        await Expect(together.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(together.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true }))
            .ToHaveTextAsync("Busy calls: show summary-card; hide summary-card");
    }
}
