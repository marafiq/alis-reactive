using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorSavesTheDraft(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X215_0)]
    public async Task saving_an_incomplete_draft_returns_its_etag_and_addressed_receipt()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveInFromReferral);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Notes", Exact = true }).FillAsync("Sister will bring the suitcase");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await Expect(world.Page.Locator("#draft-receipt")).ToHaveTextAsync("SAVE8, ETag \"8\", referral HOSP-42");
        await Expect(world.Page.Locator("#receipt-SAVE8")).ToHaveTextAsync("SAVE8: version 8 saved by Maya Chen.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#saved-draft-notes")).ToHaveTextAsync("Saved notes: Sister will bring the suitcase");
        await Expect(world.Page.Locator("#saved-location")).ToHaveTextAsync("none");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X216_0)]
    public async Task a_malformed_dose_stops_the_draft_before_sending()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("75.123");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Use at most two decimal places for the dose.");
        await Expect(world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Dose", Exact = true })).ToHaveClassAsync("alis-has-error");
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X217_0)]
    public async Task the_comparison_reads_the_other_coordinators_version_without_replacing_live_notes()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Notes", Exact = true }).FillAsync("Coordinator awaiting the family");
        await world.Seed(MoveInChanges.LuisSavesTheTransportPlan());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#comparison-version")).ToHaveTextAsync("Version 8 saved by Luis Ortiz");
        await Expect(world.Page.Locator("#working-comparison")).ToHaveTextAsync("Your notes: Coordinator awaiting the family");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Notes", Exact = true })).ToHaveValueAsync("Luis arranged transport");

        var contact = await NewWorld();
        await contact.Page.GotoAsync(CarePages.MoveIn);
        await contact.Seed(MoveInChanges.LuisUpdatesTheEmergencyContact());
        var comparison = await contact.Arm(LanesGate.SettleTheTail, CareEndpoints.MoveInComparisons);
        await contact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        var held = await comparison.Held();
        await contact.Seed(MoveInChanges.LuisSavesTheTransportPlan());
        await held.Release();
        await Expect(contact.Page.Locator("#comparison-version")).ToHaveTextAsync("Version 8 saved by Luis Ortiz");
        await Expect(contact.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Comparison", Exact = true }).GetByText("Saved notes: Family brings clothing", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await contact.Page.GotoAsync(CarePages.MoveIn);
        await Expect(contact.Page.Locator("#saved-version")).ToHaveTextAsync("9");
        await Expect(contact.Page.Locator("#saved-draft-notes")).ToHaveTextAsync("Saved notes: Luis arranged transport");

        var submitting = await NewWorld();
        await submitting.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await submitting.Page.GotoAsync(CarePages.MoveIn);
        await submitting.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await submitting.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await submitting.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await submitting.Seed(MoveInChanges.LuisUpdatesTheEmergencyContact());
        await submitting.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await Expect(submitting.Page.Locator("#comparison-version")).ToHaveTextAsync("Version 8 saved by Luis Ortiz");
        await submitting.Page.GotoAsync(CarePages.MoveIn);
        await Expect(submitting.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(submitting.Page.Locator("#admission-on-file")).ToHaveTextAsync("No admission on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X218_0)]
    public async Task a_lost_put_keeps_the_last_saved_draft()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Notes", Exact = true }).FillAsync("Bring a wheelchair");
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CarePages.MoveIn));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Draft could not be saved.");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Notes", Exact = true })).ToHaveValueAsync("Family brings clothing");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X219_0)]
    public async Task unloading_secondary_coverage_before_saving_excludes_its_working_policy()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary policy number", Exact = true }).FillAsync("POL-300");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary provider", Exact = true }).FillAsync("North Benefits");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).UncheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await Expect(world.Page.Locator("#receipt-SAVE8")).ToHaveTextAsync("SAVE8: version 8 saved by Maya Chen.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Insurance: Harbor Mutual POL-100 at 80%. No secondary coverage.");
    }
}
