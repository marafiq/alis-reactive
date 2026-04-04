using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Conditions.AdmissionWizard;

[TestFixture]
public class WhenAdmissionWizardFlowsAcrossServerLoadedSteps : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/AdmissionWizard";
    private const string Step1Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_AdmissionWizard_Step1DemographicsModel__";
    private const string Step2Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_AdmissionWizard_Step2ClinicalModel__";
    private const string Step4Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_AdmissionWizard_Step4ReviewModel__";

    private async Task NavigateAndLoadStep1()
    {
        await NavigateTo(Path);
        await WaitForPageReady(10000);
        await Expect(Page.Locator("#step-container #next-1")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private static string Id(string scope, string member) => scope + member;
    private ILocator StepContainerField(string fieldId) => Page.Locator($"#step-container #{fieldId}");
    private DropDownListLocator DropDown(string scope, string member) => new(Page, Id(scope, member));
    private ILocator ErrorFor(string formId, string fieldName)
        => Page.Locator($"#step-container #{formId} span[data-valmsg-for='{fieldName}']");

    private async Task FillAndBlur(string scope, string member, string value)
    {
        var input = StepContainerField(Id(scope, member));
        await input.ClickAsync();
        await input.FillAsync(value);
        await input.PressAsync("Tab");
    }

    private async Task CompleteStep1(string residentName, string age, string diagnosis)
    {
        await FillAndBlur(Step1Scope, "ResidentName", residentName);
        await FillAndBlur(Step1Scope, "Age", age);
        await DropDown(Step1Scope, "PrimaryDiagnosis").Select(diagnosis);
        await ClickWhenStable(Page.Locator("#step-container #next-1"));
        await Expect(Page.Locator("#step-container #step2-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task CompleteStep2()
    {
        await ClickWhenStable(Page.Locator("#step-container #next-2"));
        await Expect(Page.Locator("#step-container #step3-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task CompleteStep3()
    {
        await ClickWhenStable(Page.Locator("#step-container #next-3"));
        await Expect(Page.Locator("#step-container #screening-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task dom_ready_loads_step1_and_invalid_save_keeps_the_same_container_visible()
    {
        await NavigateAndLoadStep1();

        await ClickWhenStable(Page.Locator("#step-container #next-1"));

        await Expect(Page.Locator("#step-container #step1-form")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-container #step2-form")).ToHaveCountAsync(0);
        await Expect(ErrorFor("step1-form", "ResidentName")).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_step1_advances_to_step2_and_persists_the_generated_screening_id()
    {
        await NavigateAndLoadStep1();
        await CompleteStep1("Martha Green", "78", "Alzheimer's");

        var screeningId = await StepContainerField(Id(Step2Scope, "ScreeningId")).InputValueAsync();
        Assert.That(screeningId, Is.Not.Empty);
        await Expect(Page.Locator("#step-container")).ToContainTextAsync("Diagnosis: Alzheimer's");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task previous_and_next_reload_saved_drafts_and_keep_the_diagnosis_specific_step2_content()
    {
        await NavigateAndLoadStep1();
        await CompleteStep1("Nina Adams", "81", "Alzheimer's");

        await Expect(Page.Locator("#step-container")).ToContainTextAsync("Cognitive Assessment");

        await ClickWhenStable(Page.Locator("#step-container #prev-2"));
        await Expect(Page.Locator("#step-container #step1-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(StepContainerField(Id(Step1Scope, "ResidentName"))).ToHaveValueAsync("Nina Adams");

        await ClickWhenStable(Page.Locator("#step-container #next-1"));
        await Expect(Page.Locator("#step-container #step2-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#step-container")).ToContainTextAsync("Diagnosis: Alzheimer's");
        await Expect(Page.Locator("#step-container")).ToContainTextAsync("Cognitive Assessment");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task final_submit_blocks_without_contact_and_succeeds_after_the_saved_steps_have_loaded()
    {
        await NavigateAndLoadStep1();
        await CompleteStep1("Omar Lee", "72", "Other");
        await CompleteStep2();
        await CompleteStep3();

        await ClickWhenStable(Page.Locator("#step-container #submit-btn"));
        await Expect(ErrorFor("screening-form", "EmergencyContact")).ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(Page.Locator("#step-container #screening-form")).ToBeVisibleAsync();

        await FillAndBlur(Step4Scope, "EmergencyContact", "Mina Lee 555-0199");
        await ClickWhenStable(Page.Locator("#step-container #submit-btn"));

        await Expect(Page.Locator("#step-container #submit-result")).ToContainTextAsync("Assessment complete for Omar Lee", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
