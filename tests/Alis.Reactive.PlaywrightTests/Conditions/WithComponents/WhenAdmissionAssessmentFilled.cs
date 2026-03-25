using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Conditions.WithComponents;

/// <summary>
/// Exercises the 4-step AdmissionAssessment wizard end-to-end:
///   Step 1: Demographics — age risk tier, diagnosis section visibility, veteran toggle
///   Step 2: Clinical — cognitive (MMSE score), cardiac (BP alert), diabetes (A1C alert)
///   Step 3: Functional — fall history, mobility/room setup, medication polypharmacy, pain alert
///   Step 4: Review & Submit
///
/// Page under test: /Sandbox/Conditions/AdmissionAssessment
///
/// Senior living domain: multi-step admission health screening with conditional clinical
/// sections, server-side alerts, and condition-gated room setup.
/// </summary>
[TestFixture]
public class WhenAdmissionAssessmentFilled : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/AdmissionAssessment";

    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_HealthScreeningModel";

    // Step 1 components
    private const string AgeId = Scope + "__Age";
    private const string PrimaryDiagnosisId = Scope + "__PrimaryDiagnosis";
    private const string IsVeteranId = Scope + "__IsVeteran";

    // Step 2: Cognitive
    private const string CognitiveScoreId = Scope + "__CognitiveScore";
    private const string WandersId = Scope + "__Wanders";
    private const string WanderFrequencyId = Scope + "__WanderFrequency";

    // Step 2: Cardiac
    private const string SystolicBPId = Scope + "__SystolicBP";
    private const string HasPacemakerId = Scope + "__HasPacemaker";

    // Step 2: Diabetes
    private const string A1cLevelId = Scope + "__A1cLevel";
    private const string InsulinDependentId = Scope + "__InsulinDependent";

    // Step 3: Falls & Mobility
    private const string FallHistoryId = Scope + "__FallHistory";
    private const string FallRiskScoreId = Scope + "__FallRiskScore";
    private const string MobilityAidId = Scope + "__MobilityAid";

    // Step 3: Medications
    private const string MedicationCountId = Scope + "__MedicationCount";
    private const string TakesPainMedicationId = Scope + "__TakesPainMedication";
    private const string PainLevelId = Scope + "__PainLevel";

    // ─── Locators ───

    private NumericTextBoxLocator Age => new(Page, AgeId);
    private NumericTextBoxLocator CognitiveScore => new(Page, CognitiveScoreId);
    private NumericTextBoxLocator SystolicBP => new(Page, SystolicBPId);
    private NumericTextBoxLocator A1cLevel => new(Page, A1cLevelId);
    private NumericTextBoxLocator FallRiskScore => new(Page, FallRiskScoreId);
    private NumericTextBoxLocator MedicationCount => new(Page, MedicationCountId);
    private NumericTextBoxLocator PainLevel => new(Page, PainLevelId);

    private SwitchLocator IsVeteran => new(Page, IsVeteranId);
    private SwitchLocator Wanders => new(Page, WandersId);
    private SwitchLocator HasPacemaker => new(Page, HasPacemakerId);
    private SwitchLocator InsulinDependent => new(Page, InsulinDependentId);
    private SwitchLocator TakesPainMedication => new(Page, TakesPainMedicationId);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    /// <summary>
    /// Select a DDL value via SF ej2 API — reliable for re-selections.
    /// Passes text as a Playwright argument to avoid JS injection from apostrophes.
    /// </summary>
    private async Task SelectDropDown(string componentId, string text)
    {
        await Page.EvaluateAsync(@"(args) => {
            const el = document.getElementById(args.id);
            const ej2 = el.ej2_instances[0];
            ej2.value = args.text;
            ej2.dataBind();
        }", new { id = componentId, text });
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 1: Step Navigation
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task page_loads_and_step_1_is_visible()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#step-1")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-2")).ToBeHiddenAsync();
        await Expect(Page.Locator("#step-3")).ToBeHiddenAsync();
        await Expect(Page.Locator("#step-4")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task next_button_advances_to_step_2()
    {
        await NavigateAndBoot();

        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#step-1")).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task previous_button_returns_to_step_1()
    {
        await NavigateAndBoot();

        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#prev-2").ClickAsync();
        await Expect(Page.Locator("#step-1")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-2")).ToBeHiddenAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 2: Age -> Risk Tier
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task age_85_sets_risk_tier_to_high()
    {
        await NavigateAndBoot();

        await Age.FillAndBlur("85");

        await Expect(Page.Locator("#risk-badge"))
            .ToContainTextAsync("High", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task age_70_sets_risk_tier_to_standard()
    {
        await NavigateAndBoot();

        await Age.FillAndBlur("70");

        await Expect(Page.Locator("#risk-badge"))
            .ToContainTextAsync("Standard", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task age_50_sets_risk_tier_to_low()
    {
        await NavigateAndBoot();

        await Age.FillAndBlur("50");

        await Expect(Page.Locator("#risk-badge"))
            .ToContainTextAsync("Low", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 3: Diagnosis -> Section Visibility
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task selecting_alzheimers_shows_cognitive_section()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Alzheimer's");

        // Navigate to step 2 to see the section
        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#cognitive-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#cardiac-section")).ToBeHiddenAsync();
        await Expect(Page.Locator("#diabetes-section")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_heart_disease_shows_cardiac_section()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Heart Disease");

        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#cardiac-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#cognitive-section")).ToBeHiddenAsync();
        await Expect(Page.Locator("#diabetes-section")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_diabetes_shows_diabetes_section()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Diabetes");

        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#diabetes-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#cognitive-section")).ToBeHiddenAsync();
        await Expect(Page.Locator("#cardiac-section")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 4: Veteran Toggle
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task veteran_toggle_shows_veteran_section()
    {
        await NavigateAndBoot();

        await IsVeteran.Toggle();

        await Expect(Page.Locator("#veteran-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task veteran_toggle_off_hides_veteran_section()
    {
        await NavigateAndBoot();

        // Toggle on
        await IsVeteran.Toggle();
        await Expect(Page.Locator("#veteran-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Toggle off
        await IsVeteran.Toggle();
        await Expect(Page.Locator("#veteran-section"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 5: Cognitive Assessment
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task cognitive_score_12_sets_care_unit_to_memory_care()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Alzheimer's");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#cognitive-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await CognitiveScore.FillAndBlur("12");

        await Expect(Page.Locator("#cognitive-status"))
            .ToContainTextAsync("Memory Care", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cognitive_score_20_sets_care_unit_to_assisted_living_with_memory()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Alzheimer's");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#cognitive-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await CognitiveScore.FillAndBlur("20");

        await Expect(Page.Locator("#cognitive-status"))
            .ToContainTextAsync("Assisted Living", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task wanders_toggle_shows_wander_details()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Alzheimer's");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#cognitive-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await Wanders.Toggle();

        await Expect(Page.Locator("#wander-details"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task frequent_wandering_posts_elopement_alert()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Alzheimer's");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#cognitive-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await Wanders.Toggle();
        await Expect(Page.Locator("#wander-details"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await SelectDropDown(WanderFrequencyId, "Frequently");

        await Expect(Page.Locator("#elopement-result"))
            .ToBeVisibleAsync(new() { Timeout = 8000 });
        await Expect(Page.Locator("#elopement-result"))
            .ToContainTextAsync("Elopement", new() { Timeout = 5000 });

        TestContext.Out.WriteLine("MANUAL: Verify spinner showed during elopement alert HTTP call");

        AssertNoConsoleErrors();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 6: Cardiac Assessment
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task systolic_above_140_posts_hypertension_alert()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Heart Disease");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#cardiac-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await SystolicBP.FillAndBlur("160");

        await Expect(Page.Locator("#hypertension-result"))
            .ToBeVisibleAsync(new() { Timeout = 8000 });
        await Expect(Page.Locator("#hypertension-result"))
            .ToContainTextAsync("Hypertension", new() { Timeout = 5000 });

        TestContext.Out.WriteLine("MANUAL: Verify spinner showed during hypertension alert HTTP call");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task systolic_at_120_shows_normal()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Heart Disease");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#cardiac-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await SystolicBP.FillAndBlur("120");

        await Expect(Page.Locator("#hypertension-result"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#hypertension-result"))
            .ToContainTextAsync("Normal", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pacemaker_toggle_shows_details()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Heart Disease");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#cardiac-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await HasPacemaker.Toggle();

        await Expect(Page.Locator("#pacemaker-details"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 7: Diabetes Assessment
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task a1c_above_9_posts_uncontrolled_alert()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Diabetes");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#diabetes-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await A1cLevel.FillAndBlur("10.5");

        await Expect(Page.Locator("#uncontrolled-result"))
            .ToBeVisibleAsync(new() { Timeout = 8000 });
        await Expect(Page.Locator("#uncontrolled-result"))
            .ToContainTextAsync("Uncontrolled", new() { Timeout = 5000 });

        TestContext.Out.WriteLine("MANUAL: Verify spinner showed during uncontrolled diabetes alert HTTP call");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a1c_at_7_shows_controlled()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Diabetes");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#diabetes-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await A1cLevel.FillAndBlur("7");

        await Expect(Page.Locator("#uncontrolled-result"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#uncontrolled-result"))
            .ToContainTextAsync("Controlled", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task insulin_toggle_shows_schedule()
    {
        await NavigateAndBoot();

        await SelectDropDown(PrimaryDiagnosisId, "Diabetes");
        await Page.Locator("#next-1").ClickAsync();

        await Expect(Page.Locator("#diabetes-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await InsulinDependent.Toggle();

        await Expect(Page.Locator("#insulin-details"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 8: Falls & Mobility (Step 3)
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task fall_history_shows_fall_details()
    {
        await NavigateAndBoot();

        // Navigate to step 3
        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator("#next-2").ClickAsync();
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await SelectDropDown(FallHistoryId, "3+ falls");

        await Expect(Page.Locator("#fall-details"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task wheelchair_shows_escort_and_posts_room_setup()
    {
        await NavigateAndBoot();

        // Navigate to step 3
        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator("#next-2").ClickAsync();
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await SelectDropDown(MobilityAidId, "Wheelchair");

        await Expect(Page.Locator("#escort-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#room-result"))
            .ToBeVisibleAsync(new() { Timeout = 8000 });

        TestContext.Out.WriteLine("MANUAL: Verify room-spinner showed during room setup HTTP call");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task compound_fallrisk_and_age_sets_continuous_monitoring()
    {
        await NavigateAndBoot();

        // Set age first (step 1)
        await Age.FillAndBlur("82");
        await Expect(Page.Locator("#risk-badge"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Navigate to step 3
        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator("#next-2").ClickAsync();
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await FallRiskScore.FillAndBlur("8");
        await Page.Locator("#check-fallrisk-btn").ClickAsync();

        await Expect(Page.Locator("#fallrisk-result"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#fallrisk-result"))
            .ToContainTextAsync("Continuous", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Group 9: Medications (Step 3)
    // ═══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task medication_count_above_10_shows_polypharmacy_warning()
    {
        await NavigateAndBoot();

        // Navigate to step 3
        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator("#next-2").ClickAsync();
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await MedicationCount.FillAndBlur("14");

        await Expect(Page.Locator("#polypharmacy-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pain_above_7_posts_alert_and_shows_location_required()
    {
        await NavigateAndBoot();

        // Navigate to step 3
        await Page.Locator("#next-1").ClickAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator("#next-2").ClickAsync();
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Enable pain medication section
        await TakesPainMedication.Toggle();
        await Expect(Page.Locator("#pain-section"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await PainLevel.FillAndBlur("9");

        await Expect(Page.Locator("#location-required"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#pain-result"))
            .ToBeVisibleAsync(new() { Timeout = 8000 });

        TestContext.Out.WriteLine("MANUAL: Verify pain-spinner showed during pain alert HTTP call");

        AssertNoConsoleErrors();
    }
}
