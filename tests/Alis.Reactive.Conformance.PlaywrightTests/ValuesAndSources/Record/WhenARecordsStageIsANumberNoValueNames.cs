using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARecordsStageIsANumberNoValueNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_20_3)]
    public async Task the_stage_reads_nothing_so_the_record_reads_the_stage_it_starts_at()
    {
        var eastWard = await NewWorld();
        await eastWard.Page.GotoAsync(CarePages.BedTurnoverOfTheEastWard);
        await eastWard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the beds", Exact = true }).ClickAsync();
        await Expect(eastWard.Page.Locator("#waiting-beds")).ToHaveTextAsync("1");

        var westWard = await NewWorld();
        await westWard.Page.GotoAsync(CarePages.BedTurnoverOfTheWestWard);
        await westWard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the beds", Exact = true }).ClickAsync();
        await Expect(westWard.Page.Locator("#waiting-beds")).ToHaveTextAsync("0");
    }
}
