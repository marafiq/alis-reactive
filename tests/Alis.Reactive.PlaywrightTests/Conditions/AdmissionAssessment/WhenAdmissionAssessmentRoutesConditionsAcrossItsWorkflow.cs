using System.Linq.Expressions;
using Alis.Reactive;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

namespace Alis.Reactive.PlaywrightTests.Conditions.AdmissionAssessment;

[TestFixture]
public class WhenAdmissionAssessmentRoutesConditionsAcrossItsWorkflow : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/AdmissionAssessment";

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

    private async Task TypeAndSelectAutoComplete<TModel>(
        Expression<Func<TModel, object?>> expr,
        string searchText,
        string itemText) where TModel : class
    {
        var id = IdFor(expr);
        var input = Page.Locator($"#{id}");
        var popup = Page.Locator($"#{id}_popup");

        await input.ClickWhenStableAsync(Page);
        await input.PressSequentiallyAsync(searchText, new() { Delay = 50 });
        await popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await popup.Locator(".e-list-item").GetByText(itemText, new() { Exact = true }).ClickWhenStableAsync(Page);
        await popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });
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

    [Test]
    public async Task step1_updates_risk_tier_reveals_veteran_fields_and_filters_physicians()
    {
        await NavigateAndBoot();

        await FillAndBlur<Step1DemographicsModel>(m => m.Age, "88");
        await Expect(Page.Locator("#risk-badge")).ToHaveTextAsync("High Risk", new() { Timeout = 5000 });

        await ToggleSwitch<Step1DemographicsModel>(m => m.IsVeteran);
        await Expect(Page.Locator("#veteran-section")).ToBeVisibleAsync();

        await TypeAndSelectAutoComplete<Step1DemographicsModel>(m => m.AttendingPhysician, "Sarah", "Dr. Sarah Chen");
        await Expect(Input<Step1DemographicsModel>(m => m.AttendingPhysician)).ToHaveValueAsync("Dr. Sarah Chen");

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

        await FillAndBlur<Step2ClinicalModel>(m => m.CognitiveScore, "12");
        await Expect(Page.Locator("#cognitive-status")).ToContainTextAsync("Memory Care", new() { Timeout = 5000 });

        await ToggleSwitch<Step2ClinicalModel>(m => m.Wanders);
        await Expect(Page.Locator("#wander-details")).ToBeVisibleAsync();
        await DropDown<Step2ClinicalModel>(m => m.WanderFrequency).Select("Frequently");
        await Expect(Page.Locator("#elopement-result")).ToContainTextAsync("Elopement risk flagged", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task step3_executes_room_setup_neuro_and_pain_alert_branches()
    {
        await NavigateAndBoot();
        await CompleteStep1("Robert Miles", "88", "Other");
        await CompleteStep2();

        await DropDown<Step3FunctionalModel>(m => m.MobilityAid).Select("Wheelchair");
        await Expect(Page.Locator("#escort-indicator")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#room-result")).ToContainTextAsync("Accessible room scheduled", new() { Timeout = 5000 });

        await DropDown<Step3FunctionalModel>(m => m.FallHistory).Select("1-2 falls");
        await Expect(Page.Locator("#fall-details")).ToBeVisibleAsync();
        await ToggleSwitch<Step3FunctionalModel>(m => m.CausedInjury);
        await Expect(Page.Locator("#injury-details")).ToBeVisibleAsync();
        await DropDown<Step3FunctionalModel>(m => m.InjuryType).Select("Head Injury");
        await Expect(Page.Locator("#neuro-result")).ToContainTextAsync("Neuro consult ordered", new() { Timeout = 5000 });

        await ToggleSwitch<Step3FunctionalModel>(m => m.TakesPainMedication);
        await Expect(Page.Locator("#pain-section")).ToBeVisibleAsync();
        await FillAndBlur<Step3FunctionalModel>(m => m.PainLocation, "Lower back");
        await FillAndBlur<Step3FunctionalModel>(m => m.PainLevel, "8");
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

        await FillAndBlur<Step4ReviewModel>(m => m.EmergencyContact, "Sam Hart 555-0101");
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator("#submit-result")).ToContainTextAsync("Assessment complete for Evelyn Hart", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
