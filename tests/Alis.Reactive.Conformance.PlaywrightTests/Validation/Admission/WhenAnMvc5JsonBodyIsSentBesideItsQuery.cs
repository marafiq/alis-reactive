using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAnMvc5JsonBodyIsSentBesideItsQuery(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_46_7)]
    public async Task its_check_reads_a_shift_left_empty_as_the_null_mvc5_binds()
    {
        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.BedNotes);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over the note", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Name the shift.");
        Assert.That(empty.SentTo(CareEndpoints.BedHandoversOfNorth), Is.EqualTo(0));

        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.BedNotes);
        await night.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Shift", Exact = true }).FillAsync("Night");
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over the note", Exact = true }).ClickAsync();
        await Expect(night.Page.Locator("#noted")).ToHaveTextAsync("North, Night: Bed by the window");
    }

    [Test]
    [FailsFor(Promises.M8_F4.X16_46_8)]
    public async Task its_check_reads_an_empty_shift_on_duty_as_the_null_mvc5_binds()
    {
        var offDuty = await NewWorld();
        await offDuty.Page.GotoAsync(CarePages.BedNotes);
        await offDuty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over to the shift on duty", Exact = true }).ClickAsync();
        await Expect(offDuty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Name the shift.");
        Assert.That(offDuty.SentTo(CareEndpoints.BedHandoversOfNorth), Is.EqualTo(0));

        var evening = await NewWorld();
        await evening.Page.GotoAsync(CarePages.BedNotesOnTheEveningShift);
        await evening.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over to the shift on duty", Exact = true }).ClickAsync();
        await Expect(evening.Page.Locator("#noted")).ToHaveTextAsync("North, Evening: Bed by the window");
    }
}
