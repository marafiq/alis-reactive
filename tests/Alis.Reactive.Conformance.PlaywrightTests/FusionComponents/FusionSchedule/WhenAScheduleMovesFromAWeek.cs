using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAScheduleMovesFromAWeek(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F39.M21_X306_1)]
    public async Task the_label_shows_the_day_it_moves_from_as_its_local_day()
    {
        var once = await NewWorld();
        await once.Page.GotoAsync(CarePages.RotaWeekBefore);
        await once.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next", Exact = true }).ClickAsync();

        await Expect(once.Page.Locator("#previous-label")).ToHaveTextAsync("2026-11-09");

        var twice = await NewWorld();
        await twice.Page.GotoAsync(CarePages.RotaWeekBefore);
        await twice.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next", Exact = true }).ClickAsync();
        await Expect(twice.Page.Locator("#previous-label")).ToHaveTextAsync("2026-11-09");
        await twice.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next", Exact = true }).ClickAsync();

        await Expect(twice.Page.Locator("#previous-label")).ToHaveTextAsync("2026-11-16");
    }
}
