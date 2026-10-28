using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Stepper;

/// <summary>
/// As an admissions coordinator, I want to take a new resident's admission one step at a time, each step
/// checked and saved by the server before the next one opens, and to go back to any step I passed and find
/// it as I saved it, so that no admission reaches the care team half written or refused.
/// </summary>
/// <remarks>
/// The steps are Intake, Care, Contacts and Review; the server sends each one when the stepper opens it.
/// Continue saves the step through the server and moves the stepper on; Back moves it back; a click on the
/// stepper opens only a step the coordinator already passed. The server refuses what only it can know: a
/// resident who already lives at Maple Grove, a care level with no open bed, the resident named as their
/// own responsible party, a coordinator outside the admissions team.
/// Each step's fields are located by <c>IdGenerator</c>, not <c>PagePlan</c>: a step's plan leaves the markup
/// when the step loads, so the only plan on the page is the wizard's, which holds none of the steps' fields.
/// The wizard moves with <c>NextStep</c> and <c>PreviousStep</c>, not by writing <c>ActiveStep</c>: Syncfusion
/// reports a written active step to <c>stepChanging</c> with the new step as both the previous and the active
/// one, the same as a click on the open step, which the gate refuses. <c>ActiveStep</c>, <c>Reset</c>,
/// <c>StepClick</c> and <c>StepChanged</c>'s <c>PreviousStep</c> and <c>IsInteracted</c> are proven by
/// <see cref="WhenUsingFusionStepper"/>; <c>RefreshProgressbar</c> is not exercised: it redraws the bar after
/// the stepper's container changes size, which this page never does.
/// </remarks>
[TestFixture]
public class WhenUsingFusionStepperLazyWizard : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionStepper/Wizard";

    // Chromium logs an answer of 400 as a failed request; the server's refusals answer 400.
    private const string RefusedRequest = "the server responded with a status of 400";

    private FusionStepperLocator Steps => new(Page, "admission-steps");
    private ILocator Status => Page.Locator("#admission-status");

    private ILocator IntakeForm => Page.Locator("#admission-intake");
    private ILocator CareForm => Page.Locator("#admission-care");
    private ILocator ContactsForm => Page.Locator("#admission-contacts");
    private ILocator ReviewForm => Page.Locator("#admission-review");

    private ILocator IntakeContinue => Page.Locator("#intake-continue");
    private ILocator CareBack => Page.Locator("#care-back");
    private ILocator CareContinue => Page.Locator("#care-continue");
    private ILocator ContactsBack => Page.Locator("#contacts-back");
    private ILocator ContactsContinue => Page.Locator("#contacts-continue");
    private ILocator ReviewBack => Page.Locator("#review-back");
    private ILocator SubmitAdmission => Page.Locator("#review-submit");

    private FusionTextBoxLocator ResidentName => new(Page, IdGenerator.For<AdmissionIntake>(m => m.ResidentName));
    private NumericTextBoxLocator Age => new(Page, IdGenerator.For<AdmissionIntake>(m => m.Age));
    private DropDownListLocator AdmissionType => new(Page, IdGenerator.For<AdmissionIntake>(m => m.AdmissionType));

    private DropDownListLocator CareLevel => new(Page, IdGenerator.For<AdmissionCare>(m => m.CareLevel));
    private DropDownListLocator Diagnosis => new(Page, IdGenerator.For<AdmissionCare>(m => m.PrimaryDiagnosis));
    private NumericTextBoxLocator FallRisk => new(Page, IdGenerator.For<AdmissionCare>(m => m.FallRiskScore));
    private FusionTextBoxLocator MemoryAssessment => new(Page, IdGenerator.For<AdmissionCare>(m => m.MemoryAssessment));

    private FusionTextBoxLocator ResponsibleParty => new(Page, IdGenerator.For<AdmissionContacts>(m => m.ResponsibleParty));
    private FusionTextBoxLocator Phone => new(Page, IdGenerator.For<AdmissionContacts>(m => m.Phone));
    private FusionTextBoxLocator Email => new(Page, IdGenerator.For<AdmissionContacts>(m => m.Email));

    private FusionTextBoxLocator Coordinator => new(Page, IdGenerator.For<AdmissionReview>(m => m.AdmissionCoordinator));

    // The message beside a field, in the step that holds it.
    private ILocator MessageFor<TStep>(Expression<Func<TStep, object?>> field) where TStep : class =>
        Page.Locator($"#{IdGenerator.For(field)}_error");

    private async Task OpenTheAdmission()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(IntakeForm).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task FillIntake(string residentName, string age, string admissionType)
    {
        await ResidentName.FillAndBlur(residentName);
        await Age.FillAndBlur(age);
        await AdmissionType.Select(admissionType);
    }

    private async Task FillCare(string careLevel, string diagnosis, string fallRisk)
    {
        await CareLevel.Select(careLevel);
        await Diagnosis.Select(diagnosis);
        await FallRisk.FillAndBlur(fallRisk);
    }

    private async Task FillContacts(string responsibleParty)
    {
        await ResponsibleParty.FillAndBlur(responsibleParty);
        await Phone.FillAndBlur("555-142-0188");
        await Email.FillAndBlur("mina.patel@example.com");
    }

    // Amina Patel, 82, for a memory care evaluation.
    private async Task SaveTheIntake()
    {
        await FillIntake("Amina Patel", "82", "Memory Care Evaluation");
        await ClickWhenStable(IntakeContinue);
        await Expect(CareForm).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // Memory Care for Alzheimer's, fall risk 7, with its memory assessment.
    private async Task SaveTheCarePlan()
    {
        await FillCare("Memory Care", "Alzheimer's", "7");
        await MemoryAssessment.FillAndBlur("MoCA 18; wanders at night");
        await ClickWhenStable(CareContinue);
        await Expect(ContactsForm).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // Mina Patel, her daughter.
    private async Task SaveTheResponsibleParty()
    {
        await FillContacts("Mina Patel");
        await ClickWhenStable(ContactsContinue);
        await Expect(ReviewForm).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task ReachTheReview()
    {
        await OpenTheAdmission();
        await SaveTheIntake();
        await SaveTheCarePlan();
        await SaveTheResponsibleParty();
    }

    [Test]
    public async Task the_admission_opens_on_the_intake_step()
    {
        await NavigateToAndWaitForBoot(Path);

        await Expect(IntakeForm).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(ResidentName.Input).ToBeEditableAsync();
        await Expect(Steps.OpenStep).ToContainTextAsync("Intake");
        await Expect(Status).ToHaveTextAsync("Start with the resident's intake.");
        AssertNoConsoleErrors();
    }

    // The browser says what is missing without asking the server: a refusal from the server would log a
    // failed request.
    [Test]
    public async Task continuing_with_nothing_entered_says_what_the_intake_needs()
    {
        await OpenTheAdmission();

        await ClickWhenStable(IntakeContinue);

        await Expect(MessageFor<AdmissionIntake>(m => m.ResidentName))
            .ToHaveTextAsync("Enter the resident's full name.", new() { Timeout = 5000 });
        await Expect(MessageFor<AdmissionIntake>(m => m.Age)).ToHaveTextAsync("Enter the resident's age.");
        await Expect(MessageFor<AdmissionIntake>(m => m.AdmissionType)).ToHaveTextAsync("Choose the admission type.");
        await Expect(Steps.OpenStep).ToContainTextAsync("Intake");
        await Expect(CareForm).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_who_already_lives_at_maple_grove_is_refused()
    {
        await OpenTheAdmission();
        await FillIntake("Margaret Hale", "84", "Assisted Living");

        await ClickWhenStable(IntakeContinue);

        await Expect(MessageFor<AdmissionIntake>(m => m.ResidentName))
            .ToHaveTextAsync("Margaret Hale already lives at Maple Grove.", new() { Timeout = 10000 });
        await Expect(Steps.OpenStep).ToContainTextAsync("Intake");
        await Expect(Status).ToHaveTextAsync("Start with the resident's intake.");
        AssertNoConsoleErrorsExcept(RefusedRequest);
    }

    [Test]
    public async Task continuing_saves_the_intake_and_opens_the_care_plan()
    {
        await OpenTheAdmission();
        await FillIntake("Amina Patel", "82", "Memory Care Evaluation");

        await ClickWhenStable(IntakeContinue);

        await Expect(Status).ToHaveTextAsync("Saved Amina Patel's intake.", new() { Timeout = 10000 });
        await Expect(Steps.OpenStep).ToContainTextAsync("Care");
        await Expect(CareForm).ToContainTextAsync("Care plan for Amina Patel", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_care_plan_does_not_open_from_the_stepper_before_the_intake_is_saved()
    {
        await OpenTheAdmission();

        await ClickWhenStable(Steps.StepLabel("Care"));

        await Expect(Status).ToHaveTextAsync("Continue saves this step and opens the next one.", new() { Timeout = 5000 });
        await Expect(Steps.OpenStep).ToContainTextAsync("Intake");
        await Expect(IntakeForm).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_the_open_step_keeps_what_was_typed()
    {
        await OpenTheAdmission();
        await ResidentName.FillAndBlur("Amina Patel");

        await ClickWhenStable(Steps.StepLabel("Intake"));

        await Expect(Status).ToHaveTextAsync("Continue saves this step and opens the next one.", new() { Timeout = 5000 });
        await Expect(ResidentName.Input).ToHaveValueAsync("Amina Patel");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task back_from_the_care_plan_returns_to_the_intake_as_it_was_saved()
    {
        await OpenTheAdmission();
        await SaveTheIntake();

        await ClickWhenStable(CareBack);

        await Expect(Steps.OpenStep).ToContainTextAsync("Intake", new() { Timeout = 5000 });
        await Expect(ResidentName.Input).ToHaveValueAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Age.Input).ToHaveValueAsync("82");
        await Expect(AdmissionType.Input).ToHaveValueAsync("Memory Care Evaluation");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_memory_care_asks_for_the_memory_assessment()
    {
        await OpenTheAdmission();
        await SaveTheIntake();
        await Expect(MemoryAssessment.Input).ToBeHiddenAsync();

        await CareLevel.Select("Memory Care");
        await Expect(MemoryAssessment.Input).ToBeVisibleAsync(new() { Timeout = 5000 });

        await CareLevel.Select("Assisted Living");
        await Expect(MemoryAssessment.Input).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_memory_care_plan_without_its_memory_assessment_is_not_saved()
    {
        await OpenTheAdmission();
        await SaveTheIntake();
        await FillCare("Memory Care", "Alzheimer's", "7");

        await ClickWhenStable(CareContinue);

        await Expect(MessageFor<AdmissionCare>(m => m.MemoryAssessment))
            .ToHaveTextAsync("Write the memory assessment for a Memory Care admission.", new() { Timeout = 5000 });
        await Expect(Steps.OpenStep).ToContainTextAsync("Care");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_care_level_with_no_open_bed_is_refused()
    {
        await OpenTheAdmission();
        await SaveTheIntake();
        await FillCare("Skilled Nursing", "Stroke", "4");

        await ClickWhenStable(CareContinue);

        await Expect(MessageFor<AdmissionCare>(m => m.CareLevel)).ToHaveTextAsync(
            "No Skilled Nursing bed is open at Maple Grove. Choose Assisted Living or Memory Care.", new() { Timeout = 10000 });
        await Expect(Steps.OpenStep).ToContainTextAsync("Care");
        AssertNoConsoleErrorsExcept(RefusedRequest);
    }

    [Test]
    public async Task continuing_saves_the_care_plan_and_opens_the_responsible_party()
    {
        await OpenTheAdmission();
        await SaveTheIntake();
        await FillCare("Memory Care", "Alzheimer's", "7");
        await MemoryAssessment.FillAndBlur("MoCA 18; wanders at night");

        await ClickWhenStable(CareContinue);

        await Expect(Status).ToHaveTextAsync("Saved Amina Patel's care plan: Memory Care.", new() { Timeout = 10000 });
        await Expect(Steps.OpenStep).ToContainTextAsync("Contacts");
        await Expect(ContactsForm).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task back_from_the_responsible_party_returns_to_the_care_plan_as_it_was_saved()
    {
        await OpenTheAdmission();
        await SaveTheIntake();
        await SaveTheCarePlan();

        await ClickWhenStable(ContactsBack);

        await Expect(Steps.OpenStep).ToContainTextAsync("Care", new() { Timeout = 5000 });
        await Expect(CareLevel.Input).ToHaveValueAsync("Memory Care", new() { Timeout = 10000 });
        await Expect(Diagnosis.Input).ToHaveValueAsync("Alzheimer's");
        await Expect(FallRisk.Input).ToHaveValueAsync("7");
        await Expect(MemoryAssessment.Input).ToBeVisibleAsync();
        await Expect(MemoryAssessment.Input).ToHaveValueAsync("MoCA 18; wanders at night");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task naming_the_resident_as_their_own_responsible_party_is_refused()
    {
        await OpenTheAdmission();
        await SaveTheIntake();
        await SaveTheCarePlan();
        await FillContacts("Amina Patel");

        await ClickWhenStable(ContactsContinue);

        await Expect(MessageFor<AdmissionContacts>(m => m.ResponsibleParty)).ToHaveTextAsync(
            "Amina Patel is the resident. Name someone else as the responsible party.", new() { Timeout = 10000 });
        await Expect(Steps.OpenStep).ToContainTextAsync("Contacts");
        AssertNoConsoleErrorsExcept(RefusedRequest);
    }

    [Test]
    public async Task continuing_from_the_responsible_party_opens_the_review_of_the_whole_admission()
    {
        await OpenTheAdmission();
        await SaveTheIntake();
        await SaveTheCarePlan();
        await FillContacts("Mina Patel");

        await ClickWhenStable(ContactsContinue);

        await Expect(Status).ToHaveTextAsync("Saved Mina Patel as Amina Patel's responsible party.", new() { Timeout = 10000 });
        await Expect(Steps.OpenStep).ToContainTextAsync("Review");
        await Expect(Page.Locator("#review-resident")).ToHaveTextAsync("Amina Patel, Memory Care Evaluation", new() { Timeout = 10000 });
        await Expect(Page.Locator("#review-care")).ToHaveTextAsync("Memory Care, Alzheimer's");
        await Expect(Page.Locator("#review-contact")).ToHaveTextAsync("Mina Patel, 555-142-0188");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task back_from_the_review_returns_to_the_responsible_party_as_it_was_saved()
    {
        await ReachTheReview();

        await ClickWhenStable(ReviewBack);

        await Expect(Steps.OpenStep).ToContainTextAsync("Contacts", new() { Timeout = 5000 });
        await Expect(ResponsibleParty.Input).ToHaveValueAsync("Mina Patel", new() { Timeout = 10000 });
        await Expect(Phone.Input).ToHaveValueAsync("555-142-0188");
        await Expect(Email.Input).ToHaveValueAsync("mina.patel@example.com");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_a_step_already_passed_opens_it_as_it_was_saved()
    {
        await ReachTheReview();

        await ClickWhenStable(Steps.StepLabel("Intake"));

        await Expect(Steps.OpenStep).ToContainTextAsync("Intake", new() { Timeout = 5000 });
        await Expect(ResidentName.Input).ToHaveValueAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(AdmissionType.Input).ToHaveValueAsync("Memory Care Evaluation");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_admission_signed_off_by_someone_outside_admissions_is_refused()
    {
        await ReachTheReview();
        await Coordinator.FillAndBlur("Sam Field");

        await ClickWhenStable(SubmitAdmission);

        await Expect(MessageFor<AdmissionReview>(m => m.AdmissionCoordinator))
            .ToHaveTextAsync("Sam Field is not on Maple Grove's admissions team.", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("Saved Mina Patel as Amina Patel's responsible party.");
        AssertNoConsoleErrorsExcept(RefusedRequest);
    }

    [Test]
    public async Task submitting_the_review_submits_the_admission_with_its_coordinator()
    {
        await ReachTheReview();
        await Coordinator.FillAndBlur("Nora Jensen");

        await ClickWhenStable(SubmitAdmission);

        await Expect(Status).ToHaveTextAsync(
            "Submitted Amina Patel's admission for Memory Care. Nora Jensen is the coordinator.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
