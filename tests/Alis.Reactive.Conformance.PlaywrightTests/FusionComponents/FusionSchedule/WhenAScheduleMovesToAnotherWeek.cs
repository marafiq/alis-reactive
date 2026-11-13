using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAScheduleMovesToAnotherWeek(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F39.M21_X306_0)]
    public async Task the_label_shows_the_day_it_moves_to_as_its_local_day()
    {
        var next = await NewWorld();
        await next.Page.GotoAsync(CarePages.RotaWeek);
        await next.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next", Exact = true }).ClickAsync();

        await Expect(next.Page.Locator("#week-label")).ToHaveTextAsync("2026-11-16");

        var previous = await NewWorld();
        await previous.Page.GotoAsync(CarePages.RotaWeek);
        await previous.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Previous", Exact = true }).ClickAsync();

        await Expect(previous.Page.Locator("#week-label")).ToHaveTextAsync("2026-11-02");
    }
}
