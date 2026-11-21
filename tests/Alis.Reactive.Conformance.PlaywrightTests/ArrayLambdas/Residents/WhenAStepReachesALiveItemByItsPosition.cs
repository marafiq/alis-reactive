using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReachesALiveItemByItsPosition(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F2.M4_X4_0)]
    public async Task a_position_reaches_the_item_it_names_and_one_past_the_lists_end_stops_the_pipeline()
    {
        var pastTheEnd = await NewWorld();
        await pastTheEnd.Page.GotoAsync(CarePages.CensusItemPositions);
        await pastTheEnd.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise second item", Exact = true }).ClickAsync();
        await pastTheEnd.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read third item", Exact = true }).ClickAsync();
        await Expect(pastTheEnd.Page.Locator("#census").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Items", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Hall 20", "Bath 24" });
        await Expect(pastTheEnd.Page.Locator("#item-name")).ToHaveTextAsync("-");
        await Expect(pastTheEnd.Page.Locator("#item-status")).ToHaveTextAsync("Waiting");

        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.CensusItemPositions);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read second item", Exact = true }).ClickAsync();
        await Expect(held.Page.Locator("#item-name")).ToHaveTextAsync("Bath");
        await Expect(held.Page.Locator("#item-status")).ToHaveTextAsync("Read");
    }
}
