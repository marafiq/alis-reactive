using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenASavedIntakeKeepsItsDates(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F9.M24_X15_0)]
    public async Task saving_without_an_edit_keeps_the_move_in_day_orientation_time_and_empty_discharge_date()
    {
        var review = await NewWorld();
        await review.Page.GotoAsync(CarePages.SavedIntakeInGerman);
        await review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(review.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Intake receipt", Exact = true }))
            .ToHaveTextAsync("Moving in 2026-11-18; orientation 2026-11-18 14:35:08.123; discharge not set");
    }
}
