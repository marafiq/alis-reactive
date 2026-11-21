using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheReviewedMedicationChanges(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X200_0)]
    public async Task the_retained_submitted_prescription_receives_the_allergy_failure()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.ThePharmacyWithdrawsTheAmoxicillinAlternative());
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("AMOX");
        var review = await world.Arm(OwnershipGate.ShowsFailures, CareEndpoints.MoveInPharmacy);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        var held = await review.Held();
        await Expect(world.Page.Locator("#pharmacy-progress")).ToHaveTextAsync("Checking medications");
        await held.Release();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Amoxicillin conflicts with penicillin allergy.");
        await Expect(world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByText("Amoxicillin conflicts with penicillin allergy.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true })).ToHaveClassAsync("alis-has-error");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("No pharmacy clearance on file.");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X201_0)]
    public async Task a_fresh_check_validates_the_edited_body_after_a_clinical_refusal()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.ThePharmacyWithdrawsTheAmoxicillinAlternative());
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("AMOX");
        var review = await world.Arm(OwnershipGate.ShowsFailures, CareEndpoints.MoveInPharmacy);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        var held = await review.Held();
        await Expect(world.Page.Locator("#pharmacy-progress")).ToHaveTextAsync("Checking medications");
        await held.Release();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Amoxicillin conflicts with penicillin allergy.");
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Enter a drug code.");
        await Expect(world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByText("Enter a drug code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        Assert.That(world.SentTo(CareEndpoints.MoveInPharmacy), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("No pharmacy clearance on file.");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X202_0)]
    public async Task a_late_clinical_refusal_marks_only_the_summary_after_row_replacement()
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
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove M1", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("Medication removed: MEDICATIONS9");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M2: Azithromycin 250.00 mg (Morning)." });
        await held.Release();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Amoxicillin conflicts with penicillin allergy.");
        await Expect(world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M2", Exact = true }).GetByText("Amoxicillin conflicts with penicillin allergy.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#pharmacy-status")).ToHaveTextAsync("Pharmacy cleared: CL100");
        await Expect(world.Page.Locator("#pharmacy-directions")).ToHaveTextAsync("Cleared Azithromycin 250 mg.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M2: Azithromycin 250.00 mg (Morning)." });
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("CL100");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X203_0)]
    public async Task a_lost_held_review_leaves_the_replacement_prescription_unmarked()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Proposed drug code", Exact = true }).FillAsync("AZI");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Proposed dose", Exact = true }).FillAsync("250");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add medication", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("Medications saved: MEDICATIONS8");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning).", "M2: Azithromycin 250.00 mg (Morning)." });
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("AMOX");
        var review = await world.Arm(OwnershipGate.SendsSlotInput, CareEndpoints.MoveInPharmacy);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        var held = await review.Held();
        await Expect(world.Page.Locator("#pharmacy-progress")).ToHaveTextAsync("Checking medications");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove M1", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("Medication removed: MEDICATIONS9");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M2: Azithromycin 250.00 mg (Morning)." });
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInPharmacy));
        await held.Release();
        await Expect(world.Page.Locator("#pharmacy-status")).ToHaveTextAsync("Pharmacy review unavailable.");
        await Expect(world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M2", Exact = true }).GetByText("Amoxicillin conflicts with penicillin allergy.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M2: Azithromycin 250.00 mg (Morning)." });
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("No pharmacy clearance on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X204_0)]
    public async Task unloading_medications_keeps_the_late_summary_and_omits_edits_from_the_next_save()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.ThePharmacyWithdrawsTheAmoxicillinAlternative());
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("AMOX");
        var review = await world.Arm(OwnershipGate.ShowsFailures, CareEndpoints.MoveInPharmacy);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        var held = await review.Held();
        await Expect(world.Page.Locator("#pharmacy-progress")).ToHaveTextAsync("Checking medications");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close medication editor", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#medication-status")).ToHaveTextAsync("Medication editor closed.");
        await held.Release();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Amoxicillin conflicts with penicillin allergy.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("No pharmacy clearance on file.");
    }
}
