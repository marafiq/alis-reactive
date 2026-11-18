using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenRecordsAreSetFromABodyThatNamesTheirStage(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_20_1)]
    public async Task each_record_in_the_panels_list_holds_its_stage_as_its_number()
    {
        var northWard = await NewWorld();
        await northWard.Page.GotoAsync(CarePages.BedTurnoverOfTheNorthWard);
        await northWard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the beds", Exact = true }).ClickAsync();
        await Expect(northWard.Page.Locator("#ready-beds")).ToHaveTextAsync("1");

        var southWard = await NewWorld();
        await southWard.Page.GotoAsync(CarePages.BedTurnoverOfTheSouthWard);
        await southWard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the beds", Exact = true }).ClickAsync();
        await Expect(southWard.Page.Locator("#ready-beds")).ToHaveTextAsync("2");
    }
}
