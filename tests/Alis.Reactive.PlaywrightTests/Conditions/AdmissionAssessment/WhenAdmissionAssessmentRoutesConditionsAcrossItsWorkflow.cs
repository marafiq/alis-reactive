using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

namespace Alis.Reactive.PlaywrightTests.Conditions.AdmissionAssessment;

[TestFixture]
public class WhenAdmissionAssessmentRoutesConditionsAcrossItsWorkflow : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/AdmissionAssessment";

    private ILocator RiskBadge => Page.Locator("#risk-badge");
    private ILocator CognitiveStatus => Page.Locator("#cognitive-status");
    private ILocator ElopementAlert => Page.Locator("#elopement-result");
    private ILocator HypertensionResult => Page.Locator("#hypertension-result");
    private ILocator DiabetesResult => Page.Locator("#uncontrolled-result");
    private ILocator EscortNeeded => Page.Locator("#escort-indicator");
    private ILocator RoomSetup => Page.Locator("#room-result");
    private ILocator NeuroResult => Page.Locator("#neuro-result");
    private ILocator PainAlert => Page.Locator("#pain-result");
    private ILocator PainLocationRequired => Page.Locator("#location-required");
    private ILocator FallRiskResult => Page.Locator("#fallrisk-result");
    private ILocator SubmitButton => Page.Locator("#submit-btn");
    private ILocator Submitting => Page.Locator("#submit-spinner");
    private ILocator SubmitResult => Page.Locator("#submit-result");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#next-1");
    }

    private static string IdFor<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class
        => IdGenerator.For(expr);

    private ILocator Input<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class
        => Page.Locator($"#{IdFor(expr)}");

    private DropDownListLocator DropDown<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class
        => new(Page, IdFor(expr));

    private ILocator SwitchWrapper<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class
        => Page.Locator($".e-switch-wrapper:has(#{IdFor(expr)})");

    private ILocator ErrorFor(string formId, string fieldName)
        => Page.Locator($"#{formId} span[data-valmsg-for='{fieldName}']");

    private async Task FillAndBlur<TModel>(Expression<Func<TModel, object?>> expr, string value) where TModel : class
    {
        var input = Input(expr);
        await input.ClickAsync();
        await input.FillAsync(value);
        await input.PressAsync("Tab");
    }

    private async Task ToggleSwitch<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class
    {
        await SwitchWrapper(expr).ClickWhenStableAsync(Page);
    }

    private async Task EnterAge(string age, string expectedBadge)
    {
        await FillAndBlur<Step1DemographicsModel>(m => m.Age, age);
        await Expect(RiskBadge).ToHaveTextAsync(expectedBadge, new() { Timeout = 5000 });
        await Expect(RiskBadge).ToBeVisibleAsync();
    }

    private async Task CompleteStep1(string residentName, string age, string diagnosis)
    {
        await FillAndBlur<Step1DemographicsModel>(m => m.ResidentName, residentName);
        await FillAndBlur<Step1DemographicsModel>(m => m.Age, age);
        await DropDown<Step1DemographicsModel>(m => m.PrimaryDiagnosis).Select(diagnosis);
        await ClickWhenStable(Page.Locator("#next-1"));
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task CompleteStep2()
    {
        await ClickWhenStable(Page.Locator("#next-2"));
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task CompleteStep3()
    {
        await ClickWhenStable(Page.Locator("#next-3"));
        await Expect(Page.Locator("#step-4")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task OpenFallInjuryDetails()
    {
        await DropDown<Step3FunctionalModel>(m => m.FallHistory).Select("1-2 falls");
        await Expect(Page.Locator("#fall-details")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await ToggleSwitch<Step3FunctionalModel>(m => m.CausedInjury);
        await Expect(Page.Locator("#injury-details")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task OpenPainAssessment()
    {
        await ToggleSwitch<Step3FunctionalModel>(m => m.TakesPainMedication);
        await Expect(Page.Locator("#pain-section")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task an_age_of_85_or_more_puts_the_resident_in_the_high_risk_tier()
    {
        await NavigateAndBoot();
        await Expect(RiskBadge).ToBeHiddenAsync();

        await EnterAge("85", "High Risk");
        await Expect(DropDown<Step1DemographicsModel>(m => m.RiskTier).Input).ToHaveValueAsync("High");

        await EnterAge("84", "Standard Risk");
        await Expect(DropDown<Step1DemographicsModel>(m => m.RiskTier).Input).ToHaveValueAsync("Standard");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_age_from_65_is_standard_risk_and_under_65_is_low_risk()
    {
        await NavigateAndBoot();
        await Expect(RiskBadge).ToBeHiddenAsync();

        await EnterAge("65", "Standard Risk");
        await Expect(DropDown<Step1DemographicsModel>(m => m.RiskTier).Input).ToHaveValueAsync("Standard");

        await EnterAge("64", "Low Risk");
        await Expect(DropDown<Step1DemographicsModel>(m => m.RiskTier).Input).ToHaveValueAsync("Low");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_veteran_is_asked_for_the_va_id_and_service_branch()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#veteran-section")).ToBeHiddenAsync();

        await ToggleSwitch<Step1DemographicsModel>(m => m.IsVeteran);
        await Expect(Page.Locator("#veteran-section")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ToggleSwitch<Step1DemographicsModel>(m => m.IsVeteran);
        await Expect(Page.Locator("#veteran-section")).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_part_of_a_physicians_name_offers_only_the_physicians_who_match()
    {
        await NavigateAndBoot();
        var physician = new AutoCompleteLocator(Page, IdFor<Step1DemographicsModel>(m => m.AttendingPhysician));

        await physician.Type("Park");

        await Expect(physician.PopupItems).ToHaveTextAsync(new[] { "Dr. Emily Park" }, new() { Timeout = 10000 });
        await physician.SelectItem("Dr. Emily Park");
        await Expect(physician.Input).ToHaveValueAsync("Dr. Emily Park");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task alzheimers_opens_the_cognitive_assessment_and_no_other_clinical_section()
    {
        await NavigateAndBoot();

        await CompleteStep1("Margaret Thompson", "82", "Alzheimer's");

        await Expect(Page.Locator("#cognitive-section")).ToBeVisibleAsync();
        await Expect(Page.Locator("#cardiac-section")).ToBeHiddenAsync();
        await Expect(Page.Locator("#diabetes-section")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_cognitive_score_under_15_routes_the_resident_to_memory_care()
    {
        await NavigateAndBoot();
        await CompleteStep1("Margaret Thompson", "82", "Alzheimer's");

        await FillAndBlur<Step2ClinicalModel>(m => m.CognitiveScore, "14");

        await Expect(CognitiveStatus).ToHaveTextAsync("Severe Impairment — Memory Care", new() { Timeout = 5000 });
        await Expect(Input<Step2ClinicalModel>(m => m.CareUnit)).ToHaveValueAsync("Memory Care");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_cognitive_score_does_not_route_the_resident_to_memory_care()
    {
        await NavigateAndBoot();
        await CompleteStep1("Margaret Thompson", "82", "Alzheimer's");
        await FillAndBlur<Step2ClinicalModel>(m => m.CognitiveScore, "10");
        await Expect(CognitiveStatus).ToHaveTextAsync("Severe Impairment — Memory Care", new() { Timeout = 5000 });

        await FillAndBlur<Step2ClinicalModel>(m => m.CognitiveScore, "");

        await Expect(CognitiveStatus).ToHaveTextAsync("Mild/Normal — Standard");
        await Expect(Input<Step2ClinicalModel>(m => m.CareUnit)).ToHaveValueAsync("Standard Assisted Living");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_moderate_cognitive_score_routes_the_resident_to_assisted_living_with_memory_support()
    {
        await NavigateAndBoot();
        await CompleteStep1("Margaret Thompson", "82", "Alzheimer's");

        await FillAndBlur<Step2ClinicalModel>(m => m.CognitiveScore, "15");
        await Expect(CognitiveStatus)
            .ToHaveTextAsync("Moderate Impairment — Assisted Living + Memory", new() { Timeout = 5000 });
        await Expect(Input<Step2ClinicalModel>(m => m.CareUnit)).ToHaveValueAsync("Assisted Living with Memory Support");

        await FillAndBlur<Step2ClinicalModel>(m => m.CognitiveScore, "25");
        await Expect(CognitiveStatus).ToHaveTextAsync("Mild/Normal — Standard", new() { Timeout = 5000 });
        await Expect(Input<Step2ClinicalModel>(m => m.CareUnit)).ToHaveValueAsync("Standard Assisted Living");

        await FillAndBlur<Step2ClinicalModel>(m => m.CognitiveScore, "24");
        await Expect(CognitiveStatus)
            .ToHaveTextAsync("Moderate Impairment — Assisted Living + Memory", new() { Timeout = 5000 });
        await Expect(Input<Step2ClinicalModel>(m => m.CareUnit)).ToHaveValueAsync("Assisted Living with Memory Support");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_who_wanders_frequently_raises_an_elopement_alert_naming_them()
    {
        await NavigateAndBoot();
        await CompleteStep1("Margaret Thompson", "82", "Alzheimer's");

        await ToggleSwitch<Step2ClinicalModel>(m => m.Wanders);
        await Expect(Page.Locator("#wander-details")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await DropDown<Step2ClinicalModel>(m => m.WanderFrequency).Select("Frequently");

        await Expect(ElopementAlert).ToHaveTextAsync(
            "Elopement risk flagged for Margaret Thompson: wanders frequently", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_who_wanders_occasionally_raises_no_elopement_alert()
    {
        await NavigateAndBoot();
        await CompleteStep1("Margaret Thompson", "82", "Alzheimer's");
        await ToggleSwitch<Step2ClinicalModel>(m => m.Wanders);
        await DropDown<Step2ClinicalModel>(m => m.WanderFrequency).Select("Frequently");
        await Expect(ElopementAlert).ToBeVisibleAsync(new() { Timeout = 5000 });

        await DropDown<Step2ClinicalModel>(m => m.WanderFrequency).Select("Occasionally");

        await Expect(ElopementAlert).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_systolic_pressure_over_140_refers_the_resident_to_cardiology()
    {
        await NavigateAndBoot();
        await CompleteStep1("Harold Jenkins", "79", "Heart Disease");

        await FillAndBlur<Step2ClinicalModel>(m => m.SystolicBP, "141");

        await Expect(HypertensionResult).ToHaveTextAsync(
            "Hypertension flagged for Harold Jenkins: systolic 141 mmHg, cardiology referral", new() { Timeout = 5000 });
        await Expect(HypertensionResult).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_systolic_pressure_of_140_needs_no_cardiology_referral()
    {
        await NavigateAndBoot();
        await CompleteStep1("Harold Jenkins", "79", "Heart Disease");

        await FillAndBlur<Step2ClinicalModel>(m => m.SystolicBP, "140");

        await Expect(HypertensionResult).ToHaveTextAsync(
            "Systolic 140 or under: no cardiology referral.", new() { Timeout = 5000 });
        await Expect(HypertensionResult).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_a1c_over_9_refers_the_resident_to_endocrinology()
    {
        await NavigateAndBoot();
        await CompleteStep1("Dolores Vance", "71", "Diabetes");
        await DropDown<Step2ClinicalModel>(m => m.DiabetesType).Select("Type 2");

        await FillAndBlur<Step2ClinicalModel>(m => m.A1cLevel, "9.1");

        await Expect(DiabetesResult).ToHaveTextAsync(
            "Uncontrolled Type 2 diabetes: A1C 9.1%, endocrinology referral", new() { Timeout = 5000 });
        await Expect(DiabetesResult).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_a1c_of_9_needs_no_endocrinology_referral()
    {
        await NavigateAndBoot();
        await CompleteStep1("Dolores Vance", "71", "Diabetes");

        await FillAndBlur<Step2ClinicalModel>(m => m.A1cLevel, "9");

        await Expect(DiabetesResult).ToHaveTextAsync(
            "A1C at 9% or under: no endocrinology referral.", new() { Timeout = 5000 });
        await Expect(DiabetesResult).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_high_fall_risk_resident_aged_eighty_or_more_needs_continuous_monitoring()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();

        await FillAndBlur<Step3FunctionalModel>(m => m.FallRiskScore, "8");
        await ClickWhenStable(Page.Locator("#check-fallrisk-btn"));
        await Expect(FallRiskResult).ToHaveTextAsync("Continuous monitoring required", new() { Timeout = 5000 });
        await Expect(FallRiskResult).ToBeVisibleAsync();
        await Expect(Input<Step3FunctionalModel>(m => m.MonitoringLevel)).ToHaveValueAsync("Continuous");

        await FillAndBlur<Step3FunctionalModel>(m => m.FallRiskScore, "5");
        await ClickWhenStable(Page.Locator("#check-fallrisk-btn"));
        await Expect(FallRiskResult).ToHaveTextAsync("Standard monitoring", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_high_fall_risk_resident_under_eighty_keeps_standard_monitoring()
    {
        await NavigateAndBoot();
        await CompleteStep1("Evelyn Hart", "74", "Other");
        await CompleteStep2();

        await FillAndBlur<Step3FunctionalModel>(m => m.FallRiskScore, "8");
        await ClickWhenStable(Page.Locator("#check-fallrisk-btn"));

        await Expect(FallRiskResult).ToHaveTextAsync("Standard monitoring", new() { Timeout = 5000 });
        await Expect(FallRiskResult).ToBeVisibleAsync();
        await Expect(Input<Step3FunctionalModel>(m => m.MonitoringLevel)).Not.ToHaveValueAsync("Continuous");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_wheelchair_user_needs_an_escort_and_gets_an_accessible_room()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();

        await DropDown<Step3FunctionalModel>(m => m.MobilityAid).Select("Wheelchair");

        await Expect(RoomSetup).ToHaveTextAsync(
            "Accessible room scheduled for Robert Miles, who uses a wheelchair", new() { Timeout = 5000 });
        await Expect(RoomSetup).ToBeVisibleAsync();
        await Expect(EscortNeeded).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_walker_user_needs_an_escort_but_no_room_change()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();

        await DropDown<Step3FunctionalModel>(m => m.MobilityAid).Select("Walker");

        await Expect(EscortNeeded).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#room-spinner")).ToBeHiddenAsync();
        await Expect(RoomSetup).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_head_injury_from_a_fall_orders_a_neuro_consult_for_the_resident()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();
        await OpenFallInjuryDetails();

        await DropDown<Step3FunctionalModel>(m => m.InjuryType).Select("Head Injury");

        await Expect(NeuroResult).ToHaveTextAsync("Neuro consult ordered for Robert Miles: Head Injury", new() { Timeout = 5000 });
        await Expect(NeuroResult).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_fracture_from_a_fall_orders_no_neuro_consult()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();
        await OpenFallInjuryDetails();

        await DropDown<Step3FunctionalModel>(m => m.InjuryType).Select("Fracture");

        await Expect(NeuroResult).ToHaveTextAsync("Noted", new() { Timeout = 5000 });
        await Expect(NeuroResult).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pain_over_7_alerts_pain_management_with_its_level_and_location()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();
        await OpenPainAssessment();
        await FillAndBlur<Step3FunctionalModel>(m => m.PainLocation, "Lower back");

        await FillAndBlur<Step3FunctionalModel>(m => m.PainLevel, "8");

        await Expect(PainAlert).ToHaveTextAsync("Pain management required: level 8 at Lower back", new() { Timeout = 5000 });
        await Expect(PainLocationRequired).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pain_of_7_clears_the_pain_alert()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();
        await OpenPainAssessment();
        await FillAndBlur<Step3FunctionalModel>(m => m.PainLocation, "Lower back");
        await FillAndBlur<Step3FunctionalModel>(m => m.PainLevel, "8");
        await Expect(PainAlert).ToBeVisibleAsync(new() { Timeout = 5000 });

        await FillAndBlur<Step3FunctionalModel>(m => m.PainLevel, "7");

        await Expect(PainAlert).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(PainLocationRequired).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task step3_blocks_severe_pain_without_location_on_the_client()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();
        await OpenPainAssessment();
        await FillAndBlur<Step3FunctionalModel>(m => m.PainLevel, "8");

        await ClickWhenStable(Page.Locator("#next-3"));

        await Expect(ErrorFor("step3-form", nameof(Step3FunctionalModel.PainLocation)))
            .ToHaveTextAsync("'Pain Location' is required for severe pain.", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-4")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_screening_refused_for_its_missing_contact_is_not_shown_as_submitting_and_submits_once_the_contact_is_added()
    {
        await NavigateAndBoot();
        await CompleteStep1("Evelyn Hart", "74", "Other");
        await CompleteStep2();
        await CompleteStep3();

        await ClickWhenStable(SubmitButton);
        await Expect(ErrorFor("screening-form", nameof(Step4ReviewModel.EmergencyContact)))
            .ToHaveTextAsync("'Emergency Contact' is required.", new() { Timeout = 5000 });
        await Expect(Submitting).ToBeHiddenAsync();
        await Expect(Page.Locator("#step-4")).ToBeVisibleAsync();

        await FillAndBlur<Step4ReviewModel>(m => m.EmergencyContact, "Sam Hart 555-0101");
        await ClickWhenStable(SubmitButton);

        await Expect(SubmitResult).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SubmitResult).ToHaveTextAsync("Assessment complete for Evelyn Hart");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_screening_shows_it_is_submitting_until_the_server_completes_the_assessment()
    {
        await NavigateAndBoot();
        await CompleteStep1("Evelyn Hart", "74", "Other");
        await CompleteStep2();
        await CompleteStep3();
        await FillAndBlur<Step4ReviewModel>(m => m.EmergencyContact, "Sam Hart 555-0101");
        var answerReleased = new SemaphoreSlim(0);
        await Page.RouteAsync("**/Sandbox/Conditions/AdmissionAssessment/Submit", async route =>
        {
            await answerReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        await Expect(Submitting).ToBeHiddenAsync();

        await ClickWhenStable(SubmitButton);

        await Expect(Submitting).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SubmitResult).ToBeHiddenAsync();
        answerReleased.Release();
        await Expect(SubmitResult).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(SubmitResult).ToHaveTextAsync("Assessment complete for Evelyn Hart");
        await Expect(Submitting).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
