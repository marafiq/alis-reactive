using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoParallelRoutesWriteOneText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_14_0)]
    public async Task the_text_is_the_route_whose_answer_arrived_last()
    {
        var secondFirst = await NewWorld();
        await secondFirst.Page.GotoAsync(CarePages.CensusRefresh);
        var armedAtTheFirst = await secondFirst.Arm(LanesGate.Settle, CareEndpoints.CensusFromTheFirstSource);
        var armedAtTheSecond = await secondFirst.Arm(LanesGate.Settle, CareEndpoints.CensusFromTheSecondSource);

        await secondFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh census", Exact = true }).ClickAsync();
        var heldAtTheFirst = await armedAtTheFirst.Held();
        var heldAtTheSecond = await armedAtTheSecond.Held();
        await heldAtTheSecond.Release();
        await Expect(secondFirst.Page.Locator("#census-source")).ToHaveTextAsync("second");
        await heldAtTheFirst.Release();

        await Expect(secondFirst.Page.Locator("#census-source")).ToHaveTextAsync("first");

        var firstFirst = await NewWorld();
        await firstFirst.Page.GotoAsync(CarePages.CensusRefresh);
        var armedFirst = await firstFirst.Arm(LanesGate.Settle, CareEndpoints.CensusFromTheFirstSource);
        var armedSecond = await firstFirst.Arm(LanesGate.Settle, CareEndpoints.CensusFromTheSecondSource);

        await firstFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh census", Exact = true }).ClickAsync();
        var heldFirst = await armedFirst.Held();
        var heldSecond = await armedSecond.Held();
        await heldFirst.Release();
        await Expect(firstFirst.Page.Locator("#census-source")).ToHaveTextAsync("first");
        await heldSecond.Release();

        await Expect(firstFirst.Page.Locator("#census-source")).ToHaveTextAsync("second");
    }
}
