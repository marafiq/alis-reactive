using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.MedicationReconciliation;

/// <summary>
/// As a nurse reconciling a resident's medications, I want each line checked by its own rules, a
/// controlled substance by the extra ones, and the allergy lines by theirs, so that a mistake is named
/// on the line that holds it before the reconciliation is saved.
/// </summary>
/// <remarks>
/// Save checks the page first and sends nothing it can refuse. Print the MAR sends the reconciliation
/// as it is, and the server, running the same validator, says what it refuses.
/// </remarks>
[TestFixture]
public class WhenNurseReconcilesMedications : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/MedicationReconciliation";
    private const string Saved = "Reconciliation saved for Helen Park.";
    private const string ServerRefusal = "the server responded with a status of 400";

    private PagePlan<MedicationReconciliationModel> _plan = null!;
    private ConcurrentQueue<string> _savesSent = null!;
    private ConcurrentQueue<string> _printsSent = null!;

    private ILocator SaveReconciliation => Page.Locator("#save-reconciliation-btn");
    private ILocator PrintTheMar => Page.Locator("#print-mar-btn");
    private ILocator ReconciliationStatus => Page.Locator("#reconciliation-status");
    private ILocator MarStatus => Page.Locator("#mar-status");
    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");
    private NativeCheckBoxLocator HasAllergies => _plan.NativeCheckBox(m => m.HasAllergies);
    private NativeCheckListLocator AdministrationChecks =>
        new(Page, IdGenerator.For<MedicationReconciliationModel, List<string>>(m => m.AdministrationChecks));

    // IdGenerator is the framework's own id rule: it resolves the indexed lines, which PagePlan cannot.
    private ILocator Field(Expression<Func<MedicationReconciliationModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private ILocator ErrorFor(Expression<Func<MedicationReconciliationModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}_error");

    private async Task NavigateAndBoot()
    {
        _savesSent = new ConcurrentQueue<string>();
        _printsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/MedicationReconciliation/Save")) _savesSent.Enqueue(request.Url);
            if (request.Url.EndsWith("/Sandbox/Validation/MedicationReconciliation/Print")) _printsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<MedicationReconciliationModel>.FromPage(Page);
    }

    private async Task Write(Expression<Func<MedicationReconciliationModel, object?>> member, string text)
    {
        await Field(member).FillAsync(text);
        await Page.Keyboard.PressAsync("Tab");
    }

    // The resident, both medications named, and one administration check: what a complete reconciliation needs.
    private async Task CompleteTheReconciliation()
    {
        await Write(m => m.ResidentName, "Helen Park");
        await Write(m => m.Medications[0].Name, "Lisinopril");
        await Write(m => m.Medications[1].Name, "Metformin");
        await AdministrationChecks.Tick("Identity checked");
    }

    private async Task AssertRefusedOnTheLine(Expression<Func<MedicationReconciliationModel, object?>> member, string message)
    {
        await ClickWhenStable(SaveReconciliation);
        await Expect(ErrorFor(member)).ToHaveTextAsync(message);
        await Expect(ReconciliationStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the line, so nothing was sent.");
    }

    private async Task AssertSaved(int savesSoFar)
    {
        await ClickWhenStable(SaveReconciliation);
        await Expect(ReconciliationStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(savesSoFar), "The page let the reconciliation through, and the server accepted it.");
    }

    [Test]
    public async Task every_medication_needs_its_name_and_naming_one_clears_only_its_own_line()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Helen Park");
        await AdministrationChecks.Tick("Identity checked");

        await ClickWhenStable(SaveReconciliation);

        await Expect(ErrorFor(m => m.Medications[0].Name)).ToHaveTextAsync("Name the medication.");
        await Expect(ErrorFor(m => m.Medications[1].Name)).ToHaveTextAsync("Name the medication.");
        Assert.That(_savesSent, Is.Empty);

        await Write(m => m.Medications[0].Name, "Lisinopril");
        await Expect(ErrorFor(m => m.Medications[0].Name)).ToHaveTextAsync("");
        await Expect(ErrorFor(m => m.Medications[1].Name)).ToHaveTextAsync("Name the medication.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_dose_of_zero_is_refused_on_its_own_line_and_one_milligram_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheReconciliation();
        await Write(m => m.Medications[0].DoseMg, "0");

        await AssertRefusedOnTheLine(m => m.Medications[0].DoseMg, "The dose is above 0 mg.");
        await Expect(ErrorFor(m => m.Medications[1].DoseMg)).ToHaveTextAsync("");

        await Write(m => m.Medications[0].DoseMg, "1");
        await AssertSaved(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hours_not_written_as_numbers_are_refused_on_their_line()
    {
        await NavigateAndBoot();
        await CompleteTheReconciliation();
        await Write(m => m.Medications[1].Hours, "8am");

        await AssertRefusedOnTheLine(m => m.Medications[1].Hours, "Enter the hours given, like 8,20.");
        await Expect(ErrorFor(m => m.Medications[0].Hours)).ToHaveTextAsync("");

        await Write(m => m.Medications[1].Hours, "8,20");
        await AssertSaved(1);
        AssertNoConsoleErrors();
    }

    // The controlled substance rules are a validator of their own, included on a line only while it is controlled.
    [Test]
    public async Task a_controlled_substance_needs_its_count_verified_and_an_uncontrolled_one_does_not()
    {
        await NavigateAndBoot();
        await CompleteTheReconciliation();
        await Field(m => m.Medications[0].Controlled).CheckAsync();

        await AssertRefusedOnTheLine(m => m.Medications[0].CountVerifiedBy, "A second nurse verifies a controlled substance count: name them.");
        await Expect(ErrorFor(m => m.Medications[1].CountVerifiedBy)).ToHaveTextAsync("");

        await Field(m => m.Medications[0].Controlled).UncheckAsync();
        await AssertSaved(1);

        await Field(m => m.Medications[0].Controlled).CheckAsync();
        await Write(m => m.Medications[0].CountVerifiedBy, "Dana Ortiz");
        await AssertSaved(2);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_prescriber_npi_of_nine_digits_is_refused_on_its_line_and_ten_digits_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheReconciliation();
        await Write(m => m.Medications[1].Prescriber.Npi, "123456789");

        await AssertRefusedOnTheLine(m => m.Medications[1].Prescriber.Npi, "A prescriber's NPI is ten digits.");
        await Expect(ErrorFor(m => m.Medications[0].Prescriber.Npi)).ToHaveTextAsync("");

        await Write(m => m.Medications[1].Prescriber.Npi, "1234567890");
        await AssertSaved(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_administration_check_is_refused_and_one_tick_saves()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Helen Park");
        await Write(m => m.Medications[0].Name, "Lisinopril");
        await Write(m => m.Medications[1].Name, "Metformin");

        await AssertRefusedOnTheLine(m => m.AdministrationChecks, "Tick at least one administration check.");

        await AdministrationChecks.Tick("Dose double-checked");
        await AssertSaved(1);
        AssertNoConsoleErrors();
    }

    // A check on the allergy line that only the pharmacy can make: the server runs it, and its answer
    // lands on the line it names, since the page knows the line.
    [Test]
    public async Task an_allergen_the_pharmacy_does_not_list_is_refused_by_the_mar_printer()
    {
        await NavigateAndBoot();
        await CompleteTheReconciliation();
        await HasAllergies.Check();
        await Write(m => m.Allergies[0], "Moonlight");
        await Write(m => m.Allergies[1], "Penicillin");

        await ClickWhenStable(PrintTheMar);

        await Expect(ErrorFor(m => m.Allergies[0])).ToHaveTextAsync("The pharmacy does not list this allergen.");
        await Expect(ErrorFor(m => m.Allergies[1])).ToHaveTextAsync("");
        await Expect(ValidationSummary).ToBeHiddenAsync();
        await Expect(MarStatus).ToHaveTextAsync("The MAR was not printed.");
        Assert.That(_printsSent, Has.Count.EqualTo(1), "The MAR takes the reconciliation as it is; the server decides.");

        await Write(m => m.Allergies[0], "Latex");
        await ClickWhenStable(PrintTheMar);
        await Expect(MarStatus).ToHaveTextAsync("MAR printed for Helen Park.");
        Assert.That(_printsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_blank_allergy_line_is_refused_before_it_is_sent()
    {
        await NavigateAndBoot();
        await CompleteTheReconciliation();
        await HasAllergies.Check();
        await Write(m => m.Allergies[1], "Penicillin");

        await AssertRefusedOnTheLine(m => m.Allergies[0], "Name the allergy or clear the line.");
        await Expect(ErrorFor(m => m.Allergies[1])).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task without_allergies_the_allergy_lines_are_not_checked()
    {
        await NavigateAndBoot();
        await CompleteTheReconciliation();

        await AssertSaved(1);
        await Expect(ErrorFor(m => m.Allergies[0])).ToHaveTextAsync("");
        await Expect(ValidationSummary).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
