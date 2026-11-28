using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheDeskSchedulesFirstVisits(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F17.M21_X2020_0)]
    public async Task the_morning_round_offers_start_times_for_the_chosen_visit_length()
    {
        var checkIn = await NewWorld();
        await checkIn.Page.GotoAsync(CarePages.FirstVisitBooked);
        await checkIn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Brief check-in", Exact = true }).ClickAsync();
        var checkInBooking = checkIn.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First visit", Exact = true }).GetByRole(AriaRole.Combobox);
        await checkInBooking.PressAsync("Alt+ArrowDown");
        await checkInBooking.PressAsync("Alt+ArrowDown");

        await Expect(checkIn.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "09:00", "09:15", "09:30" });

        var assessment = await NewWorld();
        await assessment.Page.GotoAsync(CarePages.FirstVisitBooked);
        await assessment.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Brief check-in", Exact = true }).ClickAsync();
        await assessment.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admission assessment", Exact = true }).ClickAsync();
        var assessmentBooking = assessment.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First visit", Exact = true }).GetByRole(AriaRole.Combobox);
        await assessmentBooking.PressAsync("Alt+ArrowDown");
        await assessmentBooking.PressAsync("Alt+ArrowDown");

        await Expect(assessment.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "09:00", "09:30" });
    }
}
