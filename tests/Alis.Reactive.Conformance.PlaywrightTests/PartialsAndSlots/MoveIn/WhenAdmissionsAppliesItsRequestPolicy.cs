using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Admissions))]
[Category(Hosts.AdmissionsName)]
public sealed class WhenAdmissionsAppliesItsRequestPolicy(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X255_0)]
    public async Task the_saved_receipt_is_routed_before_the_flush_and_the_chained_read_renews_its_own_session()
    {
        var world = await NewWorld();
        var firstLookup = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await OpenTheDraftAfterItsLookupsFlush(world, firstLookup);
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD");
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review resident details", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#allocation-status")).ToHaveTextAsync("Not reviewed");
        await Expect(world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#handover-history")).ToHaveTextAsync("No admission work handed over.");
        var renew = await world.Arm(RunGate.Settle, WorldEndpoints.SessionRenew);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Kitchen note", Exact = true }).FillAsync("Use the blue tray");
        var save = await world.Arm(CompositionGate.Answer, CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        var heldRenew = await renew.Held();
        await Expect(world.Page.Locator("#draft-progress")).ToHaveTextAsync("Ready");
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        await heldRenew.Release();
        var heldSave = await save.Held();
        await Expect(world.Page.Locator("#draft-progress")).ToHaveTextAsync("Saving admission");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        var flush = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await heldSave.Release();
        var heldFlush = await flush.Held();
        await Expect(world.Page.Locator("#draft-receipt")).ToHaveTextAsync("SAVE8, ETag \"8\", referral");
        await Expect(world.Page.Locator("#draft-progress")).ToHaveTextAsync("Ready");
        Assert.That(world.SentTo(CareEndpoints.MoveInReceipts), Is.EqualTo(0));
        var receiptRenewal = await world.Arm(RunGate.Settle, WorldEndpoints.SessionRenew);
        await heldFlush.Release();
        var heldReceiptRenewal = await receiptRenewal.Held();
        await Expect(world.Page.Locator("#diet-on-file")).ToHaveTextAsync("Confirmed: Vegetarian, low sodium (3), 3 meals. Use the blue tray.");
        Assert.That(world.SentTo(WorldEndpoints.SessionRenew), Is.EqualTo(7));
        var receiptFlush = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        Assert.That(world.SentTo(CareEndpoints.MoveInReceipts), Is.EqualTo(0));
        await heldReceiptRenewal.Release();
        var heldReceiptFlush = await receiptFlush.Held();
        await Expect(world.Page.Locator("#receipt-SAVE8")).ToHaveTextAsync("SAVE8: version 8 saved by Maya Chen.");
        Assert.That(world.SentTo(CareEndpoints.MoveInReceipts), Is.EqualTo(1));
        await heldReceiptFlush.Release();
        await world.Page.GotoAsync(CareEndpoints.MoveInHistory);
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("SAVE8 · save");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#handover-history")).ToHaveTextAsync("SAVE8 · save");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X256_0)]
    public async Task a_rejected_dose_keeps_the_admission_and_session_untouched()
    {
        var world = await NewWorld();
        var firstLookup = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await OpenTheDraftAfterItsLookupsFlush(world, firstLookup);
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD");
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review resident details", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#allocation-status")).ToHaveTextAsync("Not reviewed");
        await Expect(world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#handover-history")).ToHaveTextAsync("No admission work handed over.");
        Assert.That(world.SentTo(WorldEndpoints.SessionRenew), Is.EqualTo(5));
        Assert.That(world.SentTo(WorldEndpoints.SessionFlush), Is.EqualTo(5));
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("75.123");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Use at most two decimal places for the dose.");
        await Expect(world.Page.Locator("#draft-progress")).ToHaveTextAsync("Ready");
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        Assert.That(world.SentTo(WorldEndpoints.SessionRenew), Is.EqualTo(5));
        Assert.That(world.SentTo(WorldEndpoints.SessionFlush), Is.EqualTo(5));
        await world.Page.GotoAsync(CareEndpoints.MoveInHistory);
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("No admission history.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X257_0)]
    public async Task a_refused_session_neither_flushes_nor_writes_the_draft()
    {
        var world = await NewWorld();
        var firstLookup = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await OpenTheDraftAfterItsLookupsFlush(world, firstLookup);
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD");
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review resident details", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#allocation-status")).ToHaveTextAsync("Not reviewed");
        await Expect(world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#handover-history")).ToHaveTextAsync("No admission work handed over.");
        await world.Seed(Seed.TheSessionSignedOut());
        Assert.That(world.SentTo(WorldEndpoints.SessionFlush), Is.EqualTo(5));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#admission-session")).ToHaveTextAsync("Session expired. Reopen the admission before saving.");
        await world.Seed(MoveInChanges.TheCoordinatorSignsInAgain());
        await Expect(world.Page.Locator("#draft-progress")).ToHaveTextAsync("Ready");
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        Assert.That(world.SentTo(WorldEndpoints.SessionFlush), Is.EqualTo(5));
        await world.Page.GotoAsync(CareEndpoints.MoveInHistory);
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("No admission history.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X258_0)]
    public async Task two_temporary_desk_failures_share_one_lifecycle_and_finish_at_the_current_status()
    {
        var world = await NewWorld();
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.AdmissionDeskStatus, 503, 503, 200));
        await world.InstallTheClock();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.HoldTheClock();
        var first = await world.Arm(CompositionGate.Answer, CareEndpoints.AdmissionDeskStatus);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check desk availability", Exact = true }).ClickAsync();
        var heldFirst = await first.Held();
        await Expect(world.Page.Locator("#desk-status")).ToHaveTextAsync("Checking the admission desk");
        await heldFirst.Release();
        var second = await world.Arm(CompositionGate.Answer, CareEndpoints.AdmissionDeskStatus);
        await world.RunTheClockUntilItSends(CareEndpoints.AdmissionDeskStatus, 2);
        var heldSecond = await second.Held();
        Assert.That(world.SentTo(WorldEndpoints.SessionRenew), Is.EqualTo(1));
        Assert.That(world.SentTo(WorldEndpoints.SessionFlush), Is.EqualTo(0));
        await heldSecond.Release();
        await world.RunTheClockUntilItSends(CareEndpoints.AdmissionDeskStatus, 3);
        await Expect(world.Page.Locator("#desk-status")).ToHaveTextAsync("Admission desk available");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show desk-status; hide desk-status");
        Assert.That(world.SentTo(WorldEndpoints.SessionRenew), Is.EqualTo(1));
        Assert.That(world.SentTo(WorldEndpoints.SessionFlush), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await Expect(world.Page.Locator("#handover-version")).ToHaveTextAsync("Version 7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X259_0)]
    public async Task the_last_of_the_quote_and_bed_answers_releases_the_shared_busy_panel()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        var quote = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInCurrentOffer);
        var bed = await world.Arm(RealtimeGate.Answer, CareEndpoints.MoveInAvailability);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read current quote and bed", Exact = true }).ClickAsync();
        var heldQuote = await quote.Held();
        var heldBed = await bed.Held();
        await heldQuote.Release();
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: placement-handover");
        await heldBed.Release();
        await Expect(world.Page.Locator("#handover-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show placement-handover; hide placement-handover");
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await Expect(world.Page.Locator("#saved-handover-quote")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD");
        await Expect(world.Page.Locator("#saved-handover-bed")).ToHaveTextAsync("BED4 revision 12, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X260_0)]
    public async Task closing_the_filing_tray_during_renewal_prevents_the_save_and_restores_ready()
    {
        var world = await NewWorld();
        var firstLookup = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await OpenTheDraftAfterItsLookupsFlush(world, firstLookup);
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD");
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review resident details", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#allocation-status")).ToHaveTextAsync("Not reviewed");
        await Expect(world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#handover-history")).ToHaveTextAsync("No admission work handed over.");
        var renew = await world.Arm(RunGate.Settle, WorldEndpoints.SessionRenew);
        var flush = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        var heldRenew = await renew.Held();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close filing tray", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#filing-tray-status")).ToHaveTextAsync("Filing tray closed");
        await heldRenew.Release();
        var heldFlush = await flush.Held();
        await Expect(world.Page.Locator("#draft-progress")).ToHaveTextAsync("Ready");
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        await heldFlush.Release();
        await world.Page.GotoAsync(CareEndpoints.MoveInHistory);
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("No admission history.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X263_0)]
    public async Task only_a_recoverable_status_checks_the_desk_and_a_failed_recovery_finishes_once()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.MoveInServices, 409));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: warning: Admission desk available");
        Assert.That(world.SentTo(CareEndpoints.AdmissionDeskStatus), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.MoveInServices, 403));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: This coordinator cannot perform this action.");
        Assert.That(world.SentTo(CareEndpoints.AdmissionDeskStatus), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.MoveInServices, 404));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The admissions request was refused.");
        Assert.That(world.SentTo(CareEndpoints.AdmissionDeskStatus), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.MoveInServices, 302));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The admissions request moved to another site.");
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.MoveInServices, 206));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The admissions response could not be read.");
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.AdmissionDeskStatus, 503));
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.MoveInServices, 409));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The admission desk could not confirm recovery.");
        Assert.That(world.SentTo(CareEndpoints.AdmissionDeskStatus), Is.EqualTo(2));
        await world.InstallTheClock();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.HoldTheClock();
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInServices));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        await world.RunTheClockUntilItSends(CareEndpoints.MoveInServices, 10);
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The admissions request lost its connection.");
        Assert.That(world.SentTo(CareEndpoints.AdmissionDeskStatus), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await Expect(world.Page.Locator("#handover-version")).ToHaveTextAsync("Version 7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X264_0)]
    public async Task an_independent_upload_policy_read_omits_the_site_and_sessions_and_shows_its_own_refusal()
    {
        var world = await NewWorld();
        var firstLookup = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await OpenTheDraftAfterItsLookupsFlush(world, firstLookup);
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("75.123");
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "consent.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110] });
        var policy = await world.Arm(RequestGate.Route, CareEndpoints.MoveInDocuments);
        var outgoing = await world.Page.RunAndWaitForRequestAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync(), "**/D100/documents?**");
        var held = await policy.Held();
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "renamed.exe", MimeType = "text/plain", Buffer = [77, 90] });
        await held.Release();
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("Choose supported admission files.");
        Assert.That(outgoing.Method, Is.EqualTo("OPTIONS"));
        Assert.That(outgoing.Headers.ContainsKey("care-site"), Is.False);
        Assert.That(world.SentTo(WorldEndpoints.SessionRenew), Is.EqualTo(5));
        Assert.That(world.SentTo(WorldEndpoints.SessionFlush), Is.EqualTo(5));
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(false));
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "consent.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110] });
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("Document uploads are unavailable.");
        Assert.That(world.SentTo(CareEndpoints.MoveInDocuments), Is.EqualTo(2));
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(true));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#stored-documents")).ToHaveTextAsync("No documents on file.");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X265_0)]
    public async Task a_failed_save_and_discard_leave_the_draft_open_for_the_coordinator_to_try_again()
    {
        var world = await NewWorld();
        await world.InstallTheClock();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved admission", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#offered-quote")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD");
        await world.HoldTheClock();
        await world.Seed(Seed.TheEndpointAnswers(CarePages.MoveIn, 503, 200));
        var outgoing = await world.Page.RunAndWaitForRequestAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync(), "**/move-ins/D100?**");
        Assert.That(outgoing.Headers["care-site"], Is.EqualTo("North"));
        Assert.That(outgoing.Headers.ContainsKey("requestverificationtoken"), Is.True);
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: warning: The admission desk is temporarily unavailable.");
        await world.RunTheClockFor(TimeSpan.FromSeconds(2));
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await world.Seed(Seed.TheEndpointAnswers(CarePages.MoveIn, 503, 200));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discard draft", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Please confirm", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Yes", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: warning: The admission desk is temporarily unavailable.");
        await world.RunTheClockFor(TimeSpan.FromSeconds(2));
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(4));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X266_0)]
    public async Task the_background_bed_snapshot_confirms_the_bed_without_blocking_the_desk()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved admission", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        var snapshot = await world.Arm(RealtimeGate.Answer, CareEndpoints.MoveInAvailability);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open bed card", Exact = true }).ClickAsync();
        var held = await snapshot.Held();
        await Expect(world.Page.Locator("#card-connection")).ToHaveTextAsync("Bed card connected");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await held.Release();
        await Expect(world.Page.Locator("#card-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await Expect(world.Page.Locator("#saved-handover-bed")).ToHaveTextAsync("BED4 revision 12, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X267_0)]
    public async Task the_registered_session_and_outage_notices_precede_the_generic_recovery_route()
    {
        var world = await NewWorld();
        await world.InstallTheClock();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.HoldTheClock();
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.MoveInServices, 401));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: warning: Your admissions session has ended.");
        Assert.That(world.SentTo(CareEndpoints.AdmissionDeskStatus), Is.EqualTo(0));
        await world.Seed(Seed.TheEndpointAnswers(CareEndpoints.AdmissionDeskStatus, 503, 503, 503, 503));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check desk availability", Exact = true }).ClickAsync();
        await world.RunTheClockUntilItSends(CareEndpoints.AdmissionDeskStatus, 4);
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: warning: Your admissions session has ended.; warning: The admission desk is temporarily unavailable.");
        Assert.That(world.SentTo(CareEndpoints.AdmissionDeskStatus), Is.EqualTo(4));
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await Expect(world.Page.Locator("#handover-version")).ToHaveTextAsync("Version 7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X268_0)]
    public async Task a_conditional_service_count_holds_the_page_until_the_catalogue_is_known()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        var catalogue = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInServices);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read service catalogue", Exact = true }).ClickAsync();
        var held = await catalogue.Held();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await held.Release();
        await Expect(world.Page.Locator("#handover-services")).ToHaveTextAsync("3 available services");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show the page; hide the page");
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await Expect(world.Page.Locator("#handover-version")).ToHaveTextAsync("Version 7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X269_0)]
    public async Task the_authority_read_marks_only_its_own_contact_action_busy()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved admission", Exact = true }).ClickAsync();
        var alice = world.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true });
        var ben = world.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true });
        var authority = await world.Arm(CompositionGate.Answer, CareEndpoints.AdmissionContactAuthority);
        await alice.GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Authority on file", Exact = true }).ClickAsync();
        var held = await authority.Held();
        await Expect(alice.GetByRole(AriaRole.Link)).ToHaveAttributeAsync("aria-busy", "true");
        await Expect(ben.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true })).Not.ToHaveAttributeAsync("aria-busy", "true");
        await held.Release();
        await Expect(alice.GetByRole(AriaRole.Link)).ToHaveTextAsync("Authority opened");
        await Expect(world.Page.Locator("#authority-on-file")).ToHaveTextAsync("Alice Ward: power of attorney POA-100, from 2026-01-01 to 2027-01-01, consent given");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true })).ToHaveTextAsync("Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X270_0)]
    public async Task a_refused_flush_keeps_the_saved_admission_and_leaves_handover_pending()
    {
        var world = await NewWorld();
        var firstLookup = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await OpenTheDraftAfterItsLookupsFlush(world, firstLookup);
        await world.Seed(Seed.TheEndpointAnswers(WorldEndpoints.SessionFlush, 503));
        var flush = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        var held = await flush.Held();
        await Expect(world.Page.Locator("#draft-receipt")).ToHaveTextAsync("SAVE8, ETag \"8\", referral");
        await Expect(world.Page.Locator("#draft-progress")).ToHaveTextAsync("Ready");
        await held.Release();
        await Expect(world.Page.Locator("#admission-session")).ToHaveTextAsync("Handover history could not be updated.");
        Assert.That(world.SentTo(CareEndpoints.MoveInReceipts), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await Expect(world.Page.Locator("#handover-version")).ToHaveTextAsync("Version 8");
        await Expect(world.Page.Locator("#handover-history")).ToHaveTextAsync("No admission work handed over.");
        await world.Page.GotoAsync(CareEndpoints.MoveInHistory);
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("SAVE8 · save");
    }

    private static async Task OpenTheDraftAfterItsLookupsFlush(TestWorld world, ArmedGate first)
    {
        var second = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        var third = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        var fourth = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        var fifth = await world.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);
        await world.Page.GotoAsync(CarePages.AdmissionHandover);
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved admission", Exact = true }).ClickAsync();
        await (await first.Held()).Release();
        await (await second.Held()).Release();
        await (await third.Held()).Release();
        await (await fourth.Held()).Release();
        await (await fifth.Held()).Release();
    }
}
