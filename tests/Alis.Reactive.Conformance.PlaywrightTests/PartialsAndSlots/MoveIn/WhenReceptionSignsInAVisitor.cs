using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenReceptionSignsInAVisitor(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X250_0)]
    public async Task the_native_form_opens_the_visitors_receipt_and_records_its_successful_controls()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Visitor name", Exact = true }).FillAsync("Alice Ward");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Visitor consent", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Spinbutton, new PageGetByRoleOptions { Name = "Badge copies", Exact = true }).FillAsync("2");
        var sent = await world.Page.RunAndWaitForRequestAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in and open receipt", Exact = true }).ClickAsync(), "**/D100/reception");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        Assert.That(sent.Headers.ContainsKey("requestverificationtoken"), Is.False);
        Assert.That(sent.PostData!.Split('&').Select(field => field.Split('=')[0]).Order(StringComparer.Ordinal), Is.EqualTo(new[] { "Consent", "CoordinatorId", "Copies", "Desk", "DraftId", "Navigation", "OperationId", "Relationship", "ResidentId", "VisitorName", "__RequestVerificationToken" }));
        await Expect(world.Page).ToHaveURLAsync(CareEndpoints.MoveInReception);
        await Expect(world.Page.Locator("#visitor-receipt")).ToHaveTextAsync("Alice Ward signed in for Helen Ward at North desk");
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Admission history", Exact = true }).ClickAsync();
        await Expect(world.Page).ToHaveURLAsync(CareEndpoints.MoveInHistory);
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("REC100 · Alice Ward signed in for Helen Ward at North desk · Daughter · Consent recorded · 2 badges");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X251_0)]
    public async Task zero_badge_copies_refuses_the_save_before_any_sign_in_is_sent()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Visitor name", Exact = true }).FillAsync("Alice Ward");
        await world.Page.GetByRole(AriaRole.Spinbutton, new PageGetByRoleOptions { Name = "Badge copies", Exact = true }).FillAsync("0");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save visitor", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#reception_summary")).ToHaveTextAsync("Choose between one and three badge copies.");
        Assert.That(world.SentTo(CareEndpoints.MoveInReception), Is.EqualTo(0));
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Admission history", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("No admission history.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X252_0)]
    public async Task the_desk_refuses_a_barred_visitor_without_recording_an_acknowledgement()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Visitor name", Exact = true }).FillAsync("Alice Ward");
        await world.Seed(MoveInChanges.ReceptionBars("Alice Ward"));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save visitor", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#reception-answer")).ToHaveTextAsync("This visitor is barred from signing in.");
        Assert.That(world.SentTo(CareEndpoints.MoveInReception), Is.EqualTo(1));
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Admission history", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("No admission history.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X253_0)]
    public async Task a_lost_reception_request_leaves_the_admission_history_without_a_sign_in()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Visitor name", Exact = true }).FillAsync("Alice Ward");
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInReception));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save visitor", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Reception could not confirm this sign-in.");
        await Expect(world.Page.Locator("#reception-answer")).ToHaveTextAsync("Sign-in unconfirmed.");
        Assert.That(world.SentTo(CareEndpoints.MoveInReception), Is.EqualTo(1));
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Admission history", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("No admission history.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X254_0)]
    public async Task reception_files_the_visit_while_the_rejected_prescription_remains_unchanged()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("12.345");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-editor_summary")).ToHaveTextAsync("Use at most two decimal places for the dose.");
        var reader = world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Reception apartment", Exact = true });
        await reader.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Open with no room", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#reception-room")).ToHaveTextAsync("No apartment supplied.");
        await reader.GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Key", Exact = true }).PressAsync("Enter");
        await Expect(world.Page.Locator("#reception-direction")).ToHaveTextAsync("Select an apartment before confirming directions.");
        await reader.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Open with room", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#reception-room")).ToHaveTextAsync("Apartment 7");
        await reader.GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Key", Exact = true }).PressAsync("ArrowRight");
        await Expect(world.Page.Locator("#reception-direction")).ToHaveTextAsync("Directions pending.");
        await reader.GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Key", Exact = true }).PressAsync("Enter");
        await Expect(world.Page.Locator("#reception-direction")).ToHaveTextAsync("Directions confirmed for Apartment 7.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Visitor name", Exact = true }).FillAsync("Alice Ward");
        var sent = await world.Page.RunAndWaitForRequestAsync(() => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save visitor", Exact = true }).ClickAsync(), "**/D100/reception");
        Assert.That(await sent.HeaderValueAsync("X-Coordinator"), Is.EqualTo("CO1"));
        Assert.That(await sent.HeaderValueAsync("X-Operation"), Is.EqualTo("11111111-1111-4111-8111-111111111111"));
        Assert.That(sent.PostData!.Split('&').Select(field => field.Split('=')[0]).Order(StringComparer.Ordinal), Is.EqualTo(new[] { "Consent", "CoordinatorId", "Copies", "Desk", "DraftId", "OperationId", "Relationship", "ResidentId", "VisitorName", "__RequestVerificationToken" }));
        await Expect(world.Page.Locator("#visitor-receipt")).ToHaveTextAsync("Alice Ward signed in for Helen Ward at North desk");
        await Expect(world.Page).ToHaveURLAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#reception-answer")).ToHaveTextAsync("Visitor signed in.");
        await Expect(world.Page.Locator("#medication-editor_summary")).ToHaveTextAsync("Use at most two decimal places for the dose.");
        await world.Page.GetByLabel("Reception document", new PageGetByLabelOptions { Exact = true }).SetInputFilesAsync(new FilePayload { Name = "arrival.txt", MimeType = "text/plain", Buffer = new byte[17] });
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File reception document", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#reception-document-receipt")).ToHaveTextAsync("1 documents received: arrival.txt.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
        await Expect(world.Page.Locator("#document-DOC1")).ToHaveTextAsync("DOC1 · arrival.txt · 17 bytes · Consent · Reception intake");
        await world.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Admission history", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#admission-history")).ToHaveTextAsync("REC100 · Alice Ward signed in for Helen Ward at North desk · Daughter · Consent pending · 1 badge");
    }
}
