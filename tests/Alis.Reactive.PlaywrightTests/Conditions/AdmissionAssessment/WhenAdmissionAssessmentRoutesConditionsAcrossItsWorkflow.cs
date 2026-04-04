using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Conditions.AdmissionAssessment;

[TestFixture]
public class WhenAdmissionAssessmentRoutesConditionsAcrossItsWorkflow : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/AdmissionAssessment";
    private const string Step1Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_AdmissionAssessment_Step1DemographicsModel__";
    private const string Step2Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_AdmissionAssessment_Step2ClinicalModel__";
    private const string Step3Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_AdmissionAssessment_Step3FunctionalModel__";
    private const string Step4Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_AdmissionAssessment_Step4ReviewModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#next-1");
    }

    private ILocator ErrorFor(string formId, string fieldName)
        => Page.Locator($"#{formId} span[data-valmsg-for='{fieldName}']");

    private static string Id(string scope, string member) => scope + member;

    private NativeTextBoxLocator TextBox(string scope, string member) => new(Page, Id(scope, member));
    private NumericTextBoxLocator NumberBox(string scope, string member) => new(Page, Id(scope, member));
    private DropDownListLocator DropDown(string scope, string member) => new(Page, Id(scope, member));
    private SwitchLocator Switch(string scope, string member) => new(Page, Id(scope, member));

    private async Task FillAndBlur(string scope, string member, string value)
    {
        await TextBox(scope, member).FillAndBlur(value);
    }

    private async Task ToggleSwitch(string scope, string member)
    {
        await Switch(scope, member).Toggle();
    }

    private async Task TypeAndSelectAutoComplete(string scope, string member, string searchText, string itemText)
    {
        var fieldId = Id(scope, member);
        var input = Page.Locator($"#{fieldId}");
        var popup = Page.Locator($"#{fieldId}_popup");

        await input.ClickWhenStableAsync();
        await input.PressSequentiallyAsync(searchText, new() { Delay = 50 });
        await popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await popup.Locator(".e-list-item").GetByText(itemText, new() { Exact = true }).ClickWhenStableAsync();
        await popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });
    }

    private async Task CompleteStep1(string residentName, string age, string diagnosis)
    {
        await FillAndBlur(Step1Scope, "ResidentName", residentName);
        await FillAndBlur(Step1Scope, "Age", age);
        await DropDown(Step1Scope, "PrimaryDiagnosis").Select(diagnosis);
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

    [Test]
    public async Task step1_updates_risk_tier_reveals_veteran_fields_and_filters_physicians()
    {
        await NavigateAndBoot();

        await FillAndBlur(Step1Scope, "Age", "88");
        await Expect(Page.Locator("#risk-badge")).ToHaveTextAsync("High Risk", new() { Timeout = 5000 });

        await ToggleSwitch(Step1Scope, "IsVeteran");
        await Expect(Page.Locator("#veteran-section")).ToBeVisibleAsync();

        await TypeAndSelectAutoComplete(Step1Scope, "AttendingPhysician", "Sarah", "Dr. Sarah Chen");
        await Expect(Page.Locator($"#{Id(Step1Scope, "AttendingPhysician")}")).ToHaveValueAsync("Dr. Sarah Chen");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task step2_shows_the_diagnosis_driven_cognitive_slice_and_sends_the_elopement_alert()
    {
        await NavigateAndBoot();
        await CompleteStep1("Margaret Thompson", "82", "Alzheimer's");

        await Expect(Page.Locator("#cognitive-section")).ToBeVisibleAsync();
        await Expect(Page.Locator("#cardiac-section")).ToBeHiddenAsync();
        await Expect(Page.Locator("#diabetes-section")).ToBeHiddenAsync();

        await FillAndBlur(Step2Scope, "CognitiveScore", "12");
        await Expect(Page.Locator("#cognitive-status")).ToContainTextAsync("Memory Care", new() { Timeout = 5000 });

        await ToggleSwitch(Step2Scope, "Wanders");
        await Expect(Page.Locator("#wander-details")).ToBeVisibleAsync();
        await DropDown(Step2Scope, "WanderFrequency").Select("Frequently");
        await Expect(Page.Locator("#elopement-result")).ToContainTextAsync("Elopement risk flagged", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task step3_executes_room_setup_neuro_and_pain_alert_branches()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();

        await DropDown(Step3Scope, "MobilityAid").Select("Wheelchair");
        await Expect(Page.Locator("#escort-indicator")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#room-result")).ToContainTextAsync("Accessible room scheduled", new() { Timeout = 5000 });

        await DropDown(Step3Scope, "FallHistory").Select("1-2 falls");
        await Expect(Page.Locator("#fall-details")).ToBeVisibleAsync();
        await ToggleSwitch(Step3Scope, "CausedInjury");
        await Expect(Page.Locator("#injury-details")).ToBeVisibleAsync();
        await DropDown(Step3Scope, "InjuryType").Select("Head Injury");
        await Expect(Page.Locator("#neuro-result")).ToContainTextAsync("Neuro consult ordered", new() { Timeout = 5000 });

        await ToggleSwitch(Step3Scope, "TakesPainMedication");
        await Expect(Page.Locator("#pain-section")).ToBeVisibleAsync();
        await FillAndBlur(Step3Scope, "PainLocation", "Lower back");
        await FillAndBlur(Step3Scope, "PainLevel", "8");
        await Expect(Page.Locator("#pain-result")).ToContainTextAsync("Pain management required", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task step4_blocks_submission_without_contact_and_succeeds_after_the_saved_steps_are_complete()
    {
        await NavigateAndBoot();
        await CompleteStep1("Evelyn Hart", "74", "Other");
        await CompleteStep2();
        await CompleteStep3();

        await ClickWhenStable(Page.Locator("#submit-btn"));
        await Expect(ErrorFor("screening-form", "EmergencyContact")).ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-4")).ToBeVisibleAsync();

        await FillAndBlur(Step4Scope, "EmergencyContact", "Sam Hart 555-0101");
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator("#submit-result")).ToContainTextAsync("Assessment complete for Evelyn Hart", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
