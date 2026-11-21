using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorUploadsAdmissionDocuments(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X230_0)]
    public async Task a_batch_reaches_the_receipt_and_its_files_remain_on_the_draft()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new[] { new FilePayload { Name = "consent.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 97, 103, 114, 101, 101, 115, 46, 10] }, new FilePayload { Name = "medications.txt", MimeType = "text/plain", Buffer = [65, 115, 112, 105, 114, 105, 110, 32, 55, 53, 32, 109, 103, 10] } });
        await world.Page.GetByLabel("Document caption", new PageGetByLabelOptions { Exact = true }).FillAsync("Signed papers");
        await Expect(world.Page.Locator("#selected-documents")).ToHaveTextAsync("2 files selected · 28 bytes");
        var upload = await world.Page.RunAndWaitForRequestAsync(
            () => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync(), "**/D100/documents");
        Assert.That(await upload.HeaderValueAsync("X-Coordinator"), Is.EqualTo("CO1"));
        Assert.That(await upload.HeaderValueAsync("X-Operation"), Is.EqualTo("11111111-1111-4111-8111-111111111111"));
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("2 documents accepted");
        await Expect(world.Page.Locator("#receipt-DOCUMENTS1")).ToHaveTextAsync("2 documents received: consent.txt, medications.txt.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#document-DOC1")).ToHaveTextAsync("DOC1 · consent.txt · 14 bytes · Consent · Signed papers");
        await Expect(world.Page.Locator("#document-DOC2")).ToHaveTextAsync("DOC2 · medications.txt · 14 bytes · Consent · Signed papers");

        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "consent.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 97, 103, 114, 101, 101, 115, 46, 10] });
        await Expect(world.Page.Locator("#latest-document")).ToHaveTextAsync("Most recently modified: consent.txt");
        await world.Page.GetByLabel("Include authority document", new PageGetByLabelOptions { Exact = true }).CheckAsync();
        await world.Page.GetByLabel("Authority document", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "authority.pdf", MimeType = "application/pdf", Buffer = [37, 80, 68, 70] });
        var policy = await world.Page.RunAndWaitForRequestAsync(
            () => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync(), "**/D100/documents?*");
        Assert.That(policy.Method, Is.EqualTo("OPTIONS"));
        Assert.That(policy.Url.Replace(new Uri(policy.Url).GetLeftPart(UriPartial.Authority), "", StringComparison.Ordinal), Is.EqualTo("/care/move-ins/D100/documents?Kind=Consent&Culture=en-US"));
        await Expect(world.Page.Locator("#receipt-DOCUMENTS2")).ToHaveTextAsync("2 documents received: consent.txt, authority.pdf.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#document-DOC4")).ToHaveTextAsync("DOC4 · authority.pdf · 4 bytes · Authority · Admission documents");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X231_0)]
    public async Task an_unsupported_selection_is_refused_before_upload()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "setup.exe", MimeType = "application/octet-stream", Buffer = [66, 73, 78] });
        await Expect(world.Page.Locator("#selected-documents")).ToHaveTextAsync("Unsupported document type.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("Choose supported admission files.");
        Assert.That(world.SentTo(CareEndpoints.MoveInDocuments), Is.EqualTo(0));
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "oversize.txt", MimeType = "text/plain", Buffer = new byte[1048577] });
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("The upload policy refused these files.");
        Assert.That(world.SentTo(CareEndpoints.MoveInDocuments), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#stored-documents")).ToHaveTextAsync("No documents on file.");

        var changed = await NewWorld();
        await changed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(changed.Page.Locator("#document-status")).ToHaveTextAsync("Ready to upload");
        var policy = await changed.Arm(RequestGate.Route, CareEndpoints.MoveInDocuments);
        await changed.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "consent.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 97, 103, 114, 101, 101, 115, 46, 10] });
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync();
        var held = await policy.Held();
        await changed.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "renamed.exe", MimeType = "text/plain", Buffer = [77, 90] });
        await held.Release();
        await Expect(changed.Page.Locator("#document-status")).ToHaveTextAsync("Choose supported admission files.");
        Assert.That(changed.SentTo(CareEndpoints.MoveInDocuments), Is.EqualTo(1));
        await changed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(changed.Page.Locator("#stored-documents")).ToHaveTextAsync("No documents on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X232_0)]
    public async Task a_quarantined_member_rejects_the_whole_batch()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new[] { new FilePayload { Name = "consent.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 97, 103, 114, 101, 101, 115, 46, 10] }, new FilePayload { Name = "medications.txt", MimeType = "text/plain", Buffer = [68, 69, 78, 73, 69, 68] } });
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("A document failed its scan. No documents accepted.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#stored-documents")).ToHaveTextAsync("No documents on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X233_0)]
    public async Task a_lost_upload_leaves_the_document_register_empty()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "consent.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 97, 103, 114, 101, 101, 115, 46, 10] });
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInDocuments));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("Documents could not be uploaded.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#stored-documents")).ToHaveTextAsync("No documents on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X234_0)]
    public async Task closing_a_held_upload_leaves_the_new_card_on_its_own_read()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByLabel("Admission files", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "consent.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 97, 103, 114, 101, 101, 115, 46, 10] });
        await world.Page.GetByLabel("Document caption", new PageGetByLabelOptions { Exact = true }).FillAsync("Signed papers");
        var upload = await world.Arm(RequestGate.Attempt, CareEndpoints.MoveInDocuments);
        var response = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInDocuments);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Upload documents", Exact = true }).ClickAsync();
        var held = await upload.Held();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close upload card", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open upload card", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("Ready to upload");
        await held.Release();
        var answer = await response.Held();
        await answer.Release();
        await Expect(world.Page.Locator("#document-status")).ToHaveTextAsync("Ready to upload");
        Assert.That(world.SentTo(CareEndpoints.MoveInReceipts), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#document-DOC1")).ToHaveTextAsync("DOC1 · consent.txt · 14 bytes · Consent · Signed papers");
    }
}
