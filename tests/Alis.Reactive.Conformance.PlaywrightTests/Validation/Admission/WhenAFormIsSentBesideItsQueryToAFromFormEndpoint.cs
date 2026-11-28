using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFormIsSentBesideItsQueryToAFromFormEndpoint(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_46_5)]
    public async Task its_check_reads_the_shift_at_its_start_as_the_endpoint_binds_it()
    {
        var evening = await NewWorld();
        await evening.Page.GotoAsync(CarePages.BedNotes);
        await evening.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save the Evening note", Exact = true }).ClickAsync();
        await Expect(evening.Page.Locator("#noted")).ToHaveTextAsync("North: Bed by the window");

        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.BedNotesOnTheEveningShift);
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save the Night note", Exact = true }).ClickAsync();
        await Expect(night.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: A shift's name holds 5 letters at most.");
        Assert.That(night.SentTo(CareEndpoints.EveningSavedBedNotesOfNorth), Is.EqualTo(0));
    }
}
