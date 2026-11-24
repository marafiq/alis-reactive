using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestSendsATextOrAFileAsItsWholeBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_46_0)]
    public async Task its_check_of_a_text_reads_every_member_at_its_start()
    {
        var unwarded = await NewWorld();
        await unwarded.Page.GotoAsync(CarePages.BedNotes);
        await unwarded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send note", Exact = true }).ClickAsync();
        await Expect(unwarded.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Ward is required.");
        Assert.That(unwarded.SentTo(CareEndpoints.BedNotesOfNorth), Is.EqualTo(0));

        var admitted = await NewWorld();
        await admitted.Page.GotoAsync(CarePages.BedNotesStartingInAdmissions);
        await admitted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send note", Exact = true }).ClickAsync();
        await Expect(admitted.Page.Locator("#noted")).ToHaveTextAsync("Admissions: Bed by the window");
    }

    [Test]
    [FailsFor(Promises.M8_F4.X16_46_1)]
    public async Task its_check_of_a_file_reads_every_member_at_its_start()
    {
        var unwarded = await NewWorld();
        await unwarded.Page.GotoAsync(CarePages.BedNotes);
        await unwarded.Page.GetByLabel("Bed photo", new PageGetByLabelOptions { Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "bed.png", MimeType = "image/png", Buffer = [137, 80, 78, 71] });
        await unwarded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send photo", Exact = true }).ClickAsync();
        await Expect(unwarded.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Ward is required.");
        Assert.That(unwarded.SentTo(CareEndpoints.BedPhotosOfNorth), Is.EqualTo(0));

        var admitted = await NewWorld();
        await admitted.Page.GotoAsync(CarePages.BedNotesStartingInAdmissions);
        await admitted.Page.GetByLabel("Bed photo", new PageGetByLabelOptions { Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "bed.png", MimeType = "image/png", Buffer = [137, 80, 78, 71] });
        await admitted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send photo", Exact = true }).ClickAsync();
        await Expect(admitted.Page.Locator("#noted")).ToHaveTextAsync("Admissions: image/png, 4 bytes");
    }
}
