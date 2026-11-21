using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorEditsTheMedicationOrders(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X190_0)]
    public async Task adding_a_prescription_preserves_the_existing_row_and_saves_the_new_identity()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Proposed drug code", Exact = true }).FillAsync("AZI");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Proposed dose", Exact = true }).FillAsync("250");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add medication", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("Medications saved: MEDICATIONS8");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning).", "M2: Azithromycin 250.00 mg (Morning)." });
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning).", "M2: Azithromycin 250.00 mg (Morning)." });
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#round-doses-on-file")).ToHaveTextAsync("Minimum 0.50 mg. Rounds: 0.50, 1.00, unassigned.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X191_0)]
    public async Task precision_and_the_owning_round_minimum_are_checked_before_a_save()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("12.345");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-editor_summary")).ToHaveTextAsync("Use at most two decimal places for the dose.");
        Assert.That(world.SentTo(CareEndpoints.MoveInMedications), Is.EqualTo(0));
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("75");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Round dose 2", Exact = true }).FillAsync("0.25");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-editor_summary")).ToHaveTextAsync("Each round dose must meet the minimum.");
        Assert.That(world.SentTo(CareEndpoints.MoveInMedications), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
        await Expect(world.Page.Locator("#round-doses-on-file")).ToHaveTextAsync("Minimum 0.50 mg. Rounds: 0.50, 1.00, unassigned.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X192_0)]
    public async Task the_drug_catalogue_and_draft_version_govern_prescription_edits()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("500");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-editor_summary")).ToHaveTextAsync("Aspirin has a maximum dose of 300.00 mg.");
        Assert.That(world.SentTo(CareEndpoints.MoveInMedications), Is.EqualTo(1));
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("100");
        await world.Seed(MoveInChanges.ThePharmacyRenewsTheOrders());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("The medication orders changed. Reopen this draft.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X193_0)]
    public async Task an_unreachable_medication_save_leaves_the_chart_as_it_was()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInMedications));
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Proposed drug code", Exact = true }).FillAsync("AZI");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Proposed dose", Exact = true }).FillAsync("250");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add medication", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("Medication save unavailable.");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X194_0)]
    public async Task deleting_a_held_reviews_first_row_preserves_the_edited_retained_prescription()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Proposed drug code", Exact = true }).FillAsync("AZI");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Proposed dose", Exact = true }).FillAsync("250");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add medication", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("Medications saved: MEDICATIONS8");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning).", "M2: Azithromycin 250.00 mg (Morning)." });
        await world.Seed(MoveInChanges.ThePharmacyWithdrawsTheAmoxicillinAlternative());
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("AMOX");
        var review = await world.Arm(OwnershipGate.ShowsFailures, CareEndpoints.MoveInPharmacy);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        var held = await review.Held();
        await Expect(world.Page.Locator("#pharmacy-progress")).ToHaveTextAsync("Checking medications");
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M2", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("125");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove M1", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("Medication removed: MEDICATIONS9");
        await held.Release();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Amoxicillin conflicts with penicillin allergy.");
        await Expect(world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M2", Exact = true }).GetByText("Amoxicillin conflicts with penicillin allergy.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M2: Azithromycin 125.00 mg (Morning)." });
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M2: Azithromycin 125.00 mg (Morning)." });
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("9");
    }
}
