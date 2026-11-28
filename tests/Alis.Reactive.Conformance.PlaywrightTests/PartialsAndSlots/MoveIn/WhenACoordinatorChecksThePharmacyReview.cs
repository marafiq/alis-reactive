using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorChecksThePharmacyReview(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X195_0)]
    public async Task clearing_aspirin_opens_its_instructions_and_saves_the_pharmacy_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#pharmacy-status")).ToHaveTextAsync("Pharmacy cleared: CL100");
        await Expect(world.Page.Locator("#pharmacy-directions")).ToHaveTextAsync("Continue Aspirin 75 mg with food.");
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("AMOX");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reported allergy 1", Exact = true }).FillAsync("");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#pharmacy-directions")).ToHaveTextAsync("Ask the prescriber about Azithromycin.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("CL100");
        await Expect(world.Page.Locator("#pharmacy-reference")).ToHaveTextAsync("Reviewed with pharmacy reference R101.");
        var reference = world.Page.Locator("#pharmacy-panel").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true });
        await reference.PressAsync("ArrowDown");
        await reference.PressAsync("Enter");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#pharmacy-directions")).ToHaveTextAsync("Continue Aspirin 75 mg with food.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#pharmacy-reference")).ToHaveTextAsync("Reviewed without a pharmacy reference.");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X196_0)]
    public async Task a_missing_drug_code_is_refused_before_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Enter a drug code.");
        await Expect(world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByText("Enter a drug code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        Assert.That(world.SentTo(CareEndpoints.MoveInPharmacy), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("No pharmacy clearance on file.");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X197_0)]
    public async Task a_catalogued_alternative_is_read_for_the_conflicting_medication()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Medication M1", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Drug code", Exact = true }).FillAsync("AMOX");
        var review = await world.Arm(LanesGate.SettleTheTail, CareEndpoints.MoveInAlternatives);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        var held = await review.Held();
        await Expect(world.Page.Locator("#pharmacy-status")).ToHaveTextAsync("M1 conflicts with penicillin.");
        await Expect(world.Page.Locator("#pharmacy-progress")).ToHaveTextAsync("Checking medications");
        await held.Release();
        await Expect(world.Page.Locator("#pharmacy-directions")).ToHaveTextAsync("Ask the prescriber about Azithromycin.");
        await Expect(world.Page.Locator("#pharmacy-progress")).ToHaveTextAsync("Review finished");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("No pharmacy clearance on file.");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X198_0)]
    public async Task losing_the_instructions_after_clearance_retains_the_clearance()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        var instructions = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInDirections);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        var held = await instructions.Held();
        await Expect(world.Page.Locator("#pharmacy-status")).ToHaveTextAsync("Pharmacy cleared: CL100");
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(false));
        await held.Release();
        await Expect(world.Page.Locator("#pharmacy-directions")).ToHaveTextAsync("Pharmacy instructions unavailable.");
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(true));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("CL100");
        await Expect(world.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Medication orders on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "M1: Aspirin 75.00 mg (Morning)." });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X199_0)]
    public async Task changing_the_payer_keeps_the_outstanding_pharmacy_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        var review = await world.Arm(OwnershipGate.SendsSlotInput, CareEndpoints.MoveInPharmacy);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        var held = await review.Held();
        await Expect(world.Page.Locator("#pharmacy-progress")).ToHaveTextAsync("Checking medications");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Funding source", Exact = true }).SelectOptionAsync("Private");
        await Expect(world.Page.GetByText("Private funding", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await held.Release();
        await Expect(world.Page.Locator("#pharmacy-status")).ToHaveTextAsync("Pharmacy cleared: CL100");
        await Expect(world.Page.Locator("#pharmacy-directions")).ToHaveTextAsync("Continue Aspirin 75 mg with food.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#pharmacy-on-file")).ToHaveTextAsync("CL100");
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Private funding. No secondary coverage.");
    }
}
