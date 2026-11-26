using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenARequestSendsATextOrAFileAsItsWholeBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_46_0)]
    public async Task its_check_of_a_text_reads_the_ward_its_path_sends()
    {
        var northeast = await NewWorld();
        await northeast.Page.GotoAsync(CarePages.BedNotes);
        await northeast.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the note to Northeast", Exact = true }).ClickAsync();
        await Expect(northeast.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: A ward's name holds 5 letters at most.");
        Assert.That(northeast.SentTo(CareEndpoints.BedNotesOfNortheast), Is.EqualTo(0));

        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.BedNotes);
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the note to North", Exact = true }).ClickAsync();
        await Expect(north.Page.Locator("#noted")).ToHaveTextAsync("North: Bed by the window");
    }

    [Test]
    [FailsFor(Promises.M8_F4.X16_46_1)]
    public async Task its_check_of_a_file_reads_the_ward_its_path_sends()
    {
        var northeast = await NewWorld();
        await northeast.Page.GotoAsync(CarePages.BedNotes);
        await northeast.Page.GetByLabel("Bed photo", new PageGetByLabelOptions { Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "bed.png", MimeType = "image/png", Buffer = [137, 80, 78, 71] });
        await northeast.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the photo to Northeast", Exact = true }).ClickAsync();
        await Expect(northeast.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: A ward's name holds 5 letters at most.");
        Assert.That(northeast.SentTo(CareEndpoints.BedPhotosOfNortheast), Is.EqualTo(0));

        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.BedNotes);
        await north.Page.GetByLabel("Bed photo", new PageGetByLabelOptions { Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "bed.png", MimeType = "image/png", Buffer = [137, 80, 78, 71] });
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send the photo to North", Exact = true }).ClickAsync();
        await Expect(north.Page.Locator("#noted")).ToHaveTextAsync("North: image/png, 4 bytes");
    }
}
