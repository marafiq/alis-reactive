using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorReplacesTheSignature(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X235_0)]
    public async Task head_reports_the_stored_size_and_the_signed_file_replaces_it()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Inspect document", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-metadata")).ToHaveTextAsync("consent.txt — 14 bytes");
        await Expect(world.Page.Locator("#signature-version")).ToHaveTextAsync("HTTP 200 · text/plain · ETag \"1\"");
        await world.Page.GetByLabel("Signature file", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "signature.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 87, 97, 114, 100, 10] });
        var sent = await world.Page.RunAndWaitForRequestAsync(
            () => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace signature", Exact = true }).ClickAsync(), "**/DOC1/content?*");
        Assert.That(sent.Method, Is.EqualTo("PUT"));
        Assert.That(await sent.HeaderValueAsync("X-Coordinator"), Is.EqualTo("CO1"));
        Assert.That(await sent.HeaderValueAsync("X-Operation"), Is.EqualTo("11111111-1111-4111-8111-111111111111"));
        Assert.That(sent.Url.Split('&').Last(), Is.EqualTo("Caption="));
        Assert.That(await sent.HeaderValueAsync("If-Match"), Is.EqualTo("\"1\""));
        await Expect(world.Page.Locator("#signature-status")).ToHaveTextAsync("Signature replaced: revision 2");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#signature-on-file")).ToHaveTextAsync("DOC3 · signature.txt");
        await Expect(world.Page.GetByLabel("Stored document text", new PageGetByLabelOptions { Exact = true })).ToHaveValueAsync("Helen Ward\n");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Inspect document", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-metadata")).ToHaveTextAsync("signature.txt — 11 bytes");
        await Expect(world.Page.Locator("#signature-version")).ToHaveTextAsync("HTTP 200 · text/plain · ETag \"2\"");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X236_0)]
    public async Task an_empty_signature_picker_keeps_the_saved_consent()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Inspect document", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-metadata")).ToHaveTextAsync("consent.txt — 14 bytes");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace signature", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-status")).ToHaveTextAsync("Choose a signature file.");
        Assert.That(world.SentTo(CareEndpoints.MoveInConsentContent), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Inspect document", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-metadata")).ToHaveTextAsync("consent.txt — 14 bytes");
        await Expect(world.Page.Locator("#signature-version")).ToHaveTextAsync("HTTP 200 · text/plain · ETag \"1\"");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X237_0)]
    public async Task a_changed_document_revision_keeps_its_bytes()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByLabel("Signature file", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "signature.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 87, 97, 114, 100, 10] });
        var put = await world.Arm(RequestGate.Attempt, CareEndpoints.MoveInConsentContent);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace signature", Exact = true }).ClickAsync();
        var held = await put.Held();
        await world.Seed(MoveInChanges.TheConsentRevisionChanges());
        await held.Release();
        await Expect(world.Page.Locator("#signature-status")).ToHaveTextAsync("The document changed. Inspect the current version.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#signature-on-file")).ToHaveTextAsync("DOC1 · consent.txt");
        await Expect(world.Page.GetByLabel("Stored document text", new PageGetByLabelOptions { Exact = true })).ToHaveValueAsync("Helen agrees.\n");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Inspect document", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-metadata")).ToHaveTextAsync("consent.txt — 14 bytes");
        await Expect(world.Page.Locator("#signature-version")).ToHaveTextAsync("HTTP 200 · text/plain · ETag \"2\"");
        await world.Page.GetByLabel("Signature file", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "signature.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 87, 97, 114, 100, 10] });
        var retry = await world.Page.RunAndWaitForRequestAsync(
            () => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace signature", Exact = true }).ClickAsync(), "**/DOC1/content?*");
        Assert.That(retry.Url.Split('&').Last(), Is.EqualTo("Caption=Reviewed"));
        await Expect(world.Page.Locator("#signature-status")).ToHaveTextAsync("Signature replaced: revision 3");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#document-DOC3")).ToHaveTextAsync("DOC3 · signature.txt · 11 bytes · Signature · Reviewed");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X238_0)]
    public async Task unavailable_metadata_keeps_the_document_unchanged()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.DocumentMetadataIsUnavailable());
        await world.Page.GetByLabel("Signature file", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "signature.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 87, 97, 114, 100, 10] });
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace signature", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-status")).ToHaveTextAsync("Document metadata unavailable.");
        Assert.That(world.SentTo(CareEndpoints.MoveInConsentContent), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#signature-on-file")).ToHaveTextAsync("DOC1 · consent.txt");
        await Expect(world.Page.GetByLabel("Stored document text", new PageGetByLabelOptions { Exact = true })).ToHaveValueAsync("Helen agrees.\n");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X239_0)]
    public async Task unloading_the_picker_before_the_metadata_reply_stops_replacement()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByLabel("Signature file", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "signature.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 87, 97, 114, 100, 10] });
        var inspect = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInConsent);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace signature", Exact = true }).ClickAsync();
        var held = await inspect.Held();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close signature picker", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-picker-status")).ToHaveTextAsync("Signature picker closed");
        await held.Release();
        await Expect(world.Page.Locator("#signature-metadata")).ToHaveTextAsync("consent.txt — 14 bytes");
        await Expect(world.Page.Locator("#signature-version")).ToHaveTextAsync("HTTP 200 · text/plain · ETag \"1\"");
        Assert.That(world.SentTo(CareEndpoints.MoveInConsentContent), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#signature-on-file")).ToHaveTextAsync("DOC1 · consent.txt");
        await Expect(world.Page.GetByLabel("Stored document text", new PageGetByLabelOptions { Exact = true })).ToHaveValueAsync("Helen agrees.\n");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Inspect document", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#signature-metadata")).ToHaveTextAsync("consent.txt — 14 bytes");
        await Expect(world.Page.Locator("#signature-version")).ToHaveTextAsync("HTTP 200 · text/plain · ETag \"1\"");

        var changed = await NewWorld();
        await changed.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await changed.Page.GotoAsync(CarePages.MoveIn);
        await changed.Page.GetByLabel("Signature file", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "signature.txt", MimeType = "text/plain", Buffer = [72, 101, 108, 101, 110, 32, 87, 97, 114, 100, 10] });
        var changedMetadata = await changed.Arm(CompositionGate.Answer, CareEndpoints.MoveInConsent);
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace signature", Exact = true }).ClickAsync();
        var changedAnswer = await changedMetadata.Held();
        await changed.Page.GetByLabel("Signature file", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "signature.exe", MimeType = "text/plain", Buffer = [77, 90] });
        await changedAnswer.Release();
        await Expect(changed.Page.Locator("#signature-status")).ToHaveTextAsync("Choose a signature file.");
        Assert.That(changed.SentTo(CareEndpoints.MoveInConsentContent), Is.EqualTo(0));
        await changed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(changed.Page.GetByLabel("Stored document text", new PageGetByLabelOptions { Exact = true })).ToHaveValueAsync("Helen agrees.\n");
    }
}
