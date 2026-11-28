using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenARequestSendsNoBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_46_2)]
    public async Task its_check_reads_the_ward_its_path_sends()
    {
        var northeast = await NewWorld();
        await northeast.Page.GotoAsync(CarePages.BedNotes);
        await northeast.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the notes of Northeast", Exact = true }).ClickAsync();
        await Expect(northeast.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: A ward's name holds 5 letters at most.");
        Assert.That(northeast.SentTo(CareEndpoints.BedNotesOfNortheast), Is.EqualTo(0));

        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.BedNotes);
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the notes of North", Exact = true }).ClickAsync();
        await Expect(north.Page.Locator("#noted")).ToHaveTextAsync("North: notes cleared");
    }

    [Test]
    [FailsFor(Promises.M8_F4.X16_46_3)]
    public async Task its_check_reads_the_shift_its_query_sends()
    {
        var evening = await NewWorld();
        await evening.Page.GotoAsync(CarePages.BedNotes);
        await evening.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the Evening notes", Exact = true }).ClickAsync();
        await Expect(evening.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: A shift's name holds 5 letters at most.");
        Assert.That(evening.SentTo(CareEndpoints.BedNotesOfNorth), Is.EqualTo(0));

        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.BedNotes);
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the Night notes", Exact = true }).ClickAsync();
        await Expect(night.Page.Locator("#noted")).ToHaveTextAsync("North, Night: notes cleared");
    }
}
