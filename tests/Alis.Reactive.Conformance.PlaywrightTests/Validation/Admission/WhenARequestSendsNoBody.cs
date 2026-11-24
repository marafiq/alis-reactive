using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestSendsNoBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_46_2)]
    public async Task its_check_reads_every_member_at_its_start()
    {
        var unwarded = await NewWorld();
        await unwarded.Page.GotoAsync(CarePages.BedNotes);
        await unwarded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear notes", Exact = true }).ClickAsync();
        await Expect(unwarded.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Ward is required.");
        Assert.That(unwarded.SentTo(CareEndpoints.BedNotesOfNorth), Is.EqualTo(0));

        var admitted = await NewWorld();
        await admitted.Page.GotoAsync(CarePages.BedNotesStartingInAdmissions);
        await admitted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear notes", Exact = true }).ClickAsync();
        await Expect(admitted.Page.Locator("#noted")).ToHaveTextAsync("Admissions: notes cleared");
    }
}
