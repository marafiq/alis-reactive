using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAnMvc5FormIsSentBesideItsQuery(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_46_4)]
    public async Task its_check_reads_the_shift_the_query_sends_beside_the_form()
    {
        var evening = await NewWorld();
        await evening.Page.GotoAsync(CarePages.BedNotes);
        await evening.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save the Evening note", Exact = true }).ClickAsync();
        await Expect(evening.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: A shift's name holds 5 letters at most.");
        Assert.That(evening.SentTo(CareEndpoints.SavedBedNotesOfNorth), Is.EqualTo(0));

        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.BedNotes);
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save the Night note", Exact = true }).ClickAsync();
        await Expect(night.Page.Locator("#noted")).ToHaveTextAsync("North, Night: Bed by the window");
    }
}
