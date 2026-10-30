using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract;

[TestFixture]
public class WhenHiddenFieldsSkipValidation : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/Contract/ConditionalHide";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-reactive-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(Expression<Func<ResidentModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}");

    private async Task FillAllRequired()
    {
        await Input(m => m.Name).FillAsync("Jane Smith");
        await Input(m => m.Email).FillAsync("jane@care.com");
        await Input(m => m.ConfirmEmail).FillAsync("jane@care.com");
        await Input(m => m.CareLevel).SelectOptionAsync("Independent");
        await Input(m => m.Address.Street).FillAsync("123 Main St");
        await Input(m => m.Address.City).FillAsync("Springfield");
        await Input(m => m.Address.ZipCode).FillAsync("62704");
    }

    [Test]
    public async Task hidden_veteran_section_with_truthy_condition_skips_rule()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Expect(Page.Locator("#veteran-section")).ToBeHiddenAsync();

        await ClickWhenStable(SubmitBtn);

        // The page did check the form: the reason for no contact is asked for, the veteran ID is not.
        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task shown_veteran_section_with_truthy_condition_fires_rule()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.IsVeteran).CheckAsync();
        await Expect(Page.Locator("#veteran-section")).ToBeVisibleAsync();

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("VeteranId")).ToHaveTextAsync("'Veteran ID' is required when veteran.");
        await Expect(ErrorFor("VeteranId")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_checkbox_toggles_visibility_and_validation_together()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        await Input(m => m.IsVeteran).CheckAsync();
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("VeteranId")).ToHaveTextAsync("'Veteran ID' is required when veteran.");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input(m => m.VeteranId).FillAsync("V12345");
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        await Input(m => m.IsVeteran).UncheckAsync();
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task independent_hides_physician_and_memory_sections()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await Expect(Page.Locator("#physician-section")).ToBeHiddenAsync();
        await Expect(Page.Locator("#memory-section")).ToBeHiddenAsync();
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assisted_living_shows_physician_section_and_requires_physician()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.CareLevel).SelectOptionAsync("Assisted Living");

        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("PhysicianName")).ToHaveTextAsync("'Physician' is required unless Independent.");
        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_shows_memory_section_and_requires_assessment()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.CareLevel).SelectOptionAsync("Memory Care");

        await Expect(Page.Locator("#memory-section")).ToBeVisibleAsync();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("MemoryAssessmentScore")).ToHaveTextAsync("'Memory Assessment' is required for Memory Care.");
        await Expect(ErrorFor("PhysicianName")).ToHaveTextAsync("'Physician' is required unless Independent.");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unchecked_emergency_contact_requires_reason_and_hides_contact_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checked_emergency_contact_requires_name_phone_and_hides_reason()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.HasEmergencyContact).CheckAsync();

        await Expect(Page.Locator("#emergency-section")).ToBeVisibleAsync();
        await Expect(Page.Locator("#no-contact-section")).ToBeHiddenAsync();

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("EmergencyName")).ToHaveTextAsync("'Emergency Name' is required.");
        await Expect(ErrorFor("EmergencyPhone")).ToHaveTextAsync("'Emergency Phone' is required.");
        await Expect(ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_emergency_contact_flips_all_validation_rules()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input(m => m.HasEmergencyContact).CheckAsync();
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("EmergencyName")).ToBeVisibleAsync();
        await Expect(ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input(m => m.HasEmergencyContact).UncheckAsync();
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }
}
