using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorPreparesTheAdmissionPacket(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X240_0)]
    public async Task the_accepted_packet_is_downloaded_and_its_note_and_print_intent_are_saved()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn + "?referral=Family");
        var download = await world.Page.RunAndWaitForDownloadAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Download packet", Exact = true }).ClickAsync());
        Assert.That(download.SuggestedFilename, Is.EqualTo("admission-D100.txt"));
        await Expect(world.Page.Locator("#packet-download")).ToHaveTextAsync("admission-D100.txt · text/plain · 107 bytes");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview packet", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByLabel("Printable packet", new PageGetByLabelOptions { Exact = true })).ToHaveValueAsync("Admission D100\nHelen Ward\nHarbor House / Cedar / 7 / Window\nMonthly total: 3040.00 USD\nDeposit: 500.00 USD\n");
        await Expect(world.Page.GetByLabel("Draft reference", new PageGetByLabelOptions { Exact = true })).ToHaveValueAsync("D100");
        var note = await world.Page.RunAndWaitForRequestAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save packet note", Exact = true }).ClickAsync(), "**/packet-notes?*");
        Assert.That(await note.HeaderValueAsync("X-Coordinator"), Is.EqualTo("CO1"));
        Assert.That(await note.HeaderValueAsync("X-Operation"), Is.EqualTo("11111111-1111-4111-8111-111111111111"));
        Assert.That(note.PostData, Is.EqualTo("Family brings clothing"));
        await Expect(world.Page.Locator("#packet-note-status")).ToHaveTextAsync("Packet note saved");
        var intent = await world.Page.RunAndWaitForRequestAsync(
            () => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print packet", Exact = true }).ClickAsync(), "**/print-intents");
        Assert.That(await intent.HeaderValueAsync("X-Coordinator"), Is.EqualTo("CO1"));
        Assert.That(await intent.HeaderValueAsync("X-Operation"), Is.EqualTo("11111111-1111-4111-8111-111111111111"));
        Assert.That(intent.PostData!.Replace(new Uri(note.Url).GetLeftPart(UriPartial.Authority), "", StringComparison.Ordinal),
            Is.EqualTo("{\"draftId\":\"D100\",\"packetRevision\":3,\"pageUrl\":\"/care/move-ins/D100?referral=Family\",\"signer\":\"HW\"}"));
        await Expect(world.Page.Locator("#packet-print-status")).ToHaveTextAsync("Print requested by HW");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#packet-note-file")).ToHaveTextAsync("PN100 · Family brings clothing · /care/move-ins/D100?referral=Family");
        await Expect(world.Page.Locator("#packet-print-file")).ToHaveTextAsync("PRINT100 · HW · revision 3");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X241_0)]
    public async Task the_packet_requires_an_accepted_quote_revision()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Download packet", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#packet-status")).ToHaveTextAsync("Accept a quote before creating the packet.");
        Assert.That(world.SentTo(CareEndpoints.MoveInPacket), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#packet-quote")).ToHaveTextAsync("No accepted quote.");
        await Expect(world.Page.Locator("#packet-note-file")).ToHaveTextAsync("No packet note on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X242_0)]
    public async Task a_sealed_packet_refuses_the_note()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.ThePacketIsSealed());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save packet note", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#packet-note-status")).ToHaveTextAsync("Packet is sealed.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#packet-note-file")).ToHaveTextAsync("No packet note on file.");
        await Expect(world.Page.Locator("#packet-quote")).ToHaveTextAsync("Accepted quote revision 3.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X243_0)]
    public async Task a_failed_packet_download_leaves_its_receipt_empty()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.ThePacketServiceIsUnavailable());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Download packet", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#packet-status")).ToHaveTextAsync("Packet download unavailable.");
        await Expect(world.Page.Locator("#packet-download")).ToHaveTextAsync("No packet downloaded.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#packet-quote")).ToHaveTextAsync("Accepted quote revision 3.");
        await Expect(world.Page.Locator("#packet-print-file")).ToHaveTextAsync("No print intent on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X244_0)]
    public async Task a_repriced_quote_refuses_the_print_intent_before_the_browser_prints()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview packet", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByLabel("Printable packet", new PageGetByLabelOptions { Exact = true })).ToHaveValueAsync("Admission D100\nHelen Ward\nHarbor House / Cedar / 7 / Window\nMonthly total: 3040.00 USD\nDeposit: 500.00 USD\n");
        await world.Seed(MoveInChanges.ThePharmacyPriceChanges());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print packet", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#packet-print-status")).ToHaveTextAsync("The packet price changed. Reopen the quote.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 4: 3060.00 USD");
        await Expect(world.Page.Locator("#packet-print-file")).ToHaveTextAsync("No print intent on file.");
    }
}
