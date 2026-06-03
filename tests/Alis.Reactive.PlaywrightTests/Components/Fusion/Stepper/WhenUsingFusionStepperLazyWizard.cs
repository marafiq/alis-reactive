using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Stepper;

[TestFixture]
public class WhenUsingFusionStepperLazyWizard : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionStepper/Wizard";
    private const string ComponentsPath = "/Sandbox/Components";

    private async Task NavigateAndLoadIntake()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-intake-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private static string IdFor<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class =>
        IdGenerator.For(expr);

    private FusionTextBoxLocator TextBox<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class =>
        new(Page, IdFor(expr));

    private NumericTextBoxLocator NumericTextBox<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class =>
        new(Page, IdFor(expr));

    private DropDownListLocator DropDown<TModel>(Expression<Func<TModel, object?>> expr) where TModel : class =>
        new(Page, IdFor(expr));

    private ILocator ErrorFor(string formId, string fieldName) =>
        Page.Locator($"#stepper-wizard-step #{formId} span[data-valmsg-for='{fieldName}']");

    private ILocator StepNumber(int zeroBasedIndex) =>
        Page.Locator("#stepper-wizard .e-step-container > .e-indicator").Nth(zeroBasedIndex);

    [Test]
    public async Task components_index_links_to_the_lazy_wizard()
    {
        await NavigateTo(ComponentsPath);

        var card = Page.Locator("a").Filter(new() { HasTextString = "Stepper Lazy Wizard" });
        await Expect(card).ToBeVisibleAsync(new() { Timeout = 5000 });
        await card.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/Sandbox/Components/FusionStepper/Wizard$"));
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-intake-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lazy_loaded_steps_gate_navigation_with_framework_validation_and_recover_saved_drafts()
    {
        await NavigateAndLoadIntake();

        await StepNumber(2).ClickAsync(new() { Force = true });
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-intake-form")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-status")).ToHaveTextAsync("complete care before opening contacts", new() { Timeout = 5000 });

        await ClickWhenStable(Page.Locator("#stepper-wizard-next-intake"));
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-intake-form")).ToBeVisibleAsync();
        await Expect(ErrorFor("stepper-wizard-intake-form", nameof(FusionStepperWizardIntakeModel.ResidentName)))
            .ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(ErrorFor("stepper-wizard-intake-form", nameof(FusionStepperWizardIntakeModel.Age)))
            .ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(ErrorFor("stepper-wizard-intake-form", nameof(FusionStepperWizardIntakeModel.AdmissionType)))
            .ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("0", new() { Timeout = 5000 });

        await TextBox<FusionStepperWizardIntakeModel>(m => m.ResidentName).FillAndBlur("Amina Patel");
        await NumericTextBox<FusionStepperWizardIntakeModel>(m => m.Age).FillAndBlur("82");
        await DropDown<FusionStepperWizardIntakeModel>(m => m.AdmissionType).Select("Memory Care Evaluation");
        await ClickWhenStable(Page.Locator("#stepper-wizard-next-intake"));

        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-care-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-status")).ToContainTextAsync("Intake saved for Amina Patel", new() { Timeout = 5000 });

        await ClickWhenStable(Page.Locator("#stepper-wizard-back-care"));
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-intake-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(TextBox<FusionStepperWizardIntakeModel>(m => m.ResidentName).Input).ToHaveValueAsync("Amina Patel", new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("0", new() { Timeout = 5000 });

        await ClickWhenStable(Page.Locator("#stepper-wizard-next-intake"));
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-care-form")).ToBeVisibleAsync(new() { Timeout = 10000 });

        await DropDown<FusionStepperWizardCareModel>(m => m.CareLevel).Select("Memory Care");
        await Expect(Page.Locator("#memory-assessment-panel")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await DropDown<FusionStepperWizardCareModel>(m => m.PrimaryDiagnosis).Select("Alzheimer's");
        await NumericTextBox<FusionStepperWizardCareModel>(m => m.FallRiskScore).FillAndBlur("7");
        await ClickWhenStable(Page.Locator("#stepper-wizard-next-care"));
        await Expect(ErrorFor("stepper-wizard-care-form", nameof(FusionStepperWizardCareModel.MemoryAssessment)))
            .ToContainTextAsync("required for Memory Care", new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("1", new() { Timeout = 5000 });

        await TextBox<FusionStepperWizardCareModel>(m => m.MemoryAssessment).FillAndBlur("MoCA 18 with nightly wandering risk");
        await ClickWhenStable(Page.Locator("#stepper-wizard-next-care"));
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-contact-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("2", new() { Timeout = 5000 });

        await TextBox<FusionStepperWizardContactModel>(m => m.ResponsibleParty).FillAndBlur("Mina Patel");
        await TextBox<FusionStepperWizardContactModel>(m => m.Phone).FillAndBlur("555");
        await TextBox<FusionStepperWizardContactModel>(m => m.Email).FillAndBlur("not-email");
        await ClickWhenStable(Page.Locator("#stepper-wizard-next-contact"));
        await Expect(ErrorFor("stepper-wizard-contact-form", nameof(FusionStepperWizardContactModel.Phone)))
            .ToContainTextAsync("123-456-7890", new() { Timeout = 5000 });
        await Expect(ErrorFor("stepper-wizard-contact-form", nameof(FusionStepperWizardContactModel.Email)))
            .ToContainTextAsync("valid email", new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("2", new() { Timeout = 5000 });

        await TextBox<FusionStepperWizardContactModel>(m => m.Phone).FillAndBlur("555-142-0188");
        await TextBox<FusionStepperWizardContactModel>(m => m.Email).FillAndBlur("mina.patel@example.com");
        await ClickWhenStable(Page.Locator("#stepper-wizard-next-contact"));
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-review-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-summary-resident")).ToHaveTextAsync("Amina Patel");
        await Expect(Page.Locator("#stepper-wizard-summary-care")).ToHaveTextAsync("Memory Care");
        await Expect(Page.Locator("#stepper-wizard-summary-contact")).ToHaveTextAsync("Mina Patel");

        await StepNumber(1).ClickAsync(new() { Force = true });
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-care-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#stepper-wizard-current")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#memory-assessment-panel")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(Page.Locator("#stepper-wizard-next-care"));
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-contact-form")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await ClickWhenStable(Page.Locator("#stepper-wizard-next-contact"));
        await Expect(Page.Locator("#stepper-wizard-step #stepper-wizard-review-form")).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#stepper-wizard-submit"));
        await Expect(ErrorFor("stepper-wizard-review-form", nameof(FusionStepperWizardReviewModel.AdmissionCoordinator)))
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await TextBox<FusionStepperWizardReviewModel>(m => m.AdmissionCoordinator).FillAndBlur("Nora Jensen");
        await ClickWhenStable(Page.Locator("#stepper-wizard-submit"));
        await Expect(Page.Locator("#stepper-wizard-submit-result"))
            .ToContainTextAsync("Admission packet submitted for Amina Patel", new() { Timeout = 5000 });
        await Expect(Page.Locator("#stepper-wizard-status")).ToHaveTextAsync("submitted", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
