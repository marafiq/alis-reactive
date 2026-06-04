using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract;

[TestFixture]
public class WhenMultiFieldFormSubmits : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/Contract";
    private const string ResidentModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-reactive-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(string suffix) => Page.Locator($"#{ResidentModelIdPrefix}{suffix}");

    private async Task FillAllRequired()
    {
        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("jane@care.com");
        await Input("ConfirmEmail").FillAsync("jane@care.com");
        await Input("CareLevel").SelectOptionAsync("Independent");
        await Input("Address_Street").FillAsync("123 Main St");
        await Input("Address_City").FillAsync("Springfield");
        await Input("Address_ZipCode").FillAsync("62704");
    }

    [Test]
    public async Task empty_form_blocks_request_and_shows_required_errors_inline()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Name")).ToContainTextAsync("'Name' is required");
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToContainTextAsync("'Email' is required");
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(ErrorFor("CareLevel")).ToContainTextAsync("required");

        await Expect(Input("Name")).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Input("Email")).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Expect(Result).ToHaveTextAsync("");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task name_too_short_shows_minlength_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Name").FillAsync("A");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Name")).ToContainTextAsync("minimum length");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task invalid_zipcode_shows_regex_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Address_ZipCode").FillAsync("abc");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("5 digits");
        await Expect(Input("Address_ZipCode")).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // DSL condition: equalTo.

    [Test]
    public async Task confirm_email_fails_when_different_from_email()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Email").FillAsync("resident@care.com");
        await Input("ConfirmEmail").FillAsync("wrong@email.com");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");
        await Expect(Input("ConfirmEmail")).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirm_email_passes_when_matches_email()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ConfirmEmail")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    // DSL condition: truthy.

    [Test]
    public async Task veteran_id_not_required_when_is_veteran_unchecked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        // IsVeteran is unchecked by default and VeteranId is intentionally empty.

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task veteran_id_required_when_is_veteran_checked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("IsVeteran").CheckAsync();

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");
        await Expect(ErrorFor("VeteranId")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task veteran_id_passes_when_filled_and_is_veteran_checked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("IsVeteran").CheckAsync();
        await Input("VeteranId").FillAsync("V12345");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    // DSL condition: eq.

    [Test]
    public async Task memory_assessment_not_required_when_care_level_is_assisted()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("CareLevel").SelectOptionAsync("Assisted Living");
        await Input("PhysicianName").FillAsync("Dr. Smith"); // required for non-Independent

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task memory_assessment_required_when_care_level_is_memory_care()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("CareLevel").SelectOptionAsync("Memory Care");
        await Input("PhysicianName").FillAsync("Dr. Smith");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("MemoryAssessmentScore")).ToContainTextAsync("required");
        await Expect(ErrorFor("MemoryAssessmentScore")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // Fusion NumericTextBox validates inline with regular validation errors.

    [Test]
    public async Task memory_care_with_fusion_numeric_validates_and_succeeds()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("CareLevel").SelectOptionAsync("Memory Care");
        await Input("PhysicianName").FillAsync("Dr. Smith");
        await Input("ReasonForNoContact").FillAsync("No relatives");

        // Leave the Fusion NumericTextBox empty so submit exercises inline validation.
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).ToContainTextAsync("required");
        await Expect(ErrorFor("MemoryAssessmentScore")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Tab commits the Syncfusion value before the next submit.
        var scoreInput = Page.Locator($"#{ResidentModelIdPrefix}MemoryAssessmentScore");
        await scoreInput.ClickAsync();
        await scoreInput.FillAsync("85");
        await scoreInput.PressAsync("Tab");

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // DSL condition: neq.

    [Test]
    public async Task physician_not_required_when_care_level_is_independent()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("CareLevel").SelectOptionAsync("Independent");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task physician_required_when_care_level_is_assisted()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("CareLevel").SelectOptionAsync("Assisted Living");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");
        await Expect(ErrorFor("PhysicianName")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task physician_required_when_care_level_is_memory_care()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("CareLevel").SelectOptionAsync("Memory Care");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // DSL condition: falsy.

    [Test]
    public async Task reason_for_no_contact_required_when_has_emergency_contact_unchecked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        // HasEmergencyContact is unchecked by default and ReasonForNoContact is intentionally empty.

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).ToContainTextAsync("required");
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reason_for_no_contact_not_required_when_has_emergency_contact_checked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("HasEmergencyContact").CheckAsync();
        await Input("EmergencyName").FillAsync("John");
        await Input("EmergencyPhone").FillAsync("555-0123");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task toggling_is_veteran_toggles_veteran_id_requirement()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        await Input("IsVeteran").CheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input("IsVeteran").UncheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_has_emergency_contact_flips_between_contact_fields_and_reason()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input("HasEmergencyContact").CheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("EmergencyName")).ToBeVisibleAsync();
        await Expect(ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input("HasEmergencyContact").UncheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fixing_errors_and_resubmitting_clears_previous_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input("Name").FillAsync("Jane Smith");
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await FillAllRequired();
        await Input("ReasonForNoContact").FillAsync("No relatives nearby");
        await SubmitBtn.ClickAsync();

        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task empty_address_street_shows_required_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Address_Street").FillAsync("");
        await Input("ReasonForNoContact").FillAsync("No relatives");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task empty_address_city_shows_required_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Address_City").FillAsync("");
        await Input("ReasonForNoContact").FillAsync("No relatives");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.City")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.City")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task invalid_email_format_shows_format_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Email").FillAsync("not-an-email");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Email")).ToContainTextAsync("valid email");
        await Expect(Input("Email")).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task name_error_clears_on_blur_after_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(Input("Name")).ToHaveClassAsync(new Regex("alis-has-error"));

        await Input("Name").FillAsync("Robert Thompson");
        await Input("Name").BlurAsync();

        await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });
        await Expect(Input("Name")).Not.ToHaveClassAsync(new Regex("alis-has-error"), new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task email_error_clears_on_blur_after_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();

        await Input("Email").FillAsync("robert@care.com");
        await Input("Email").BlurAsync();

        await Expect(ErrorFor("Email")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task zipcode_error_clears_on_blur_after_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Address_ZipCode").FillAsync("abc");
        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.ZipCode")).ToBeVisibleAsync();

        await Input("Address_ZipCode").FillAsync("62704");
        await Input("Address_ZipCode").BlurAsync();

        await Expect(ErrorFor("Address.ZipCode")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task complete_form_with_all_conditional_fields_succeeds()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Name").FillAsync("Eleanor Davis");
        await Input("Email").FillAsync("eleanor@care.com");
        await Input("ConfirmEmail").FillAsync("eleanor@care.com");
        await Input("CareLevel").SelectOptionAsync("Memory Care");

        await Input("IsVeteran").CheckAsync();
        await Input("VeteranId").FillAsync("V99999");

        await Input("PhysicianName").FillAsync("Dr. Martinez");
        var assessmentInput = Page.Locator($"#{ResidentModelIdPrefix}MemoryAssessmentScore");
        await assessmentInput.ClickAsync();
        await assessmentInput.FillAsync("72");
        await assessmentInput.PressAsync("Tab");

        await Input("HasEmergencyContact").CheckAsync();
        await Input("EmergencyName").FillAsync("Michael Davis");
        await Input("EmergencyPhone").FillAsync("555-9876");

        await Input("Address_Street").FillAsync("456 Oak Lane");
        await Input("Address_City").FillAsync("Portland");
        await Input("Address_ZipCode").FillAsync("97201");

        await SubmitBtn.ClickAsync();

        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirm_email_error_clears_on_blur_when_corrected()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Email").FillAsync("test@care.com");
        await Input("ConfirmEmail").FillAsync("wrong@care.com");

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");

        await Input("ConfirmEmail").FillAsync("test@care.com");
        await Input("ConfirmEmail").BlurAsync();

        await Expect(ErrorFor("ConfirmEmail")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task switching_care_level_away_from_memory_care_removes_assessment_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("CareLevel").SelectOptionAsync("Memory Care");
        await Input("PhysicianName").FillAsync("Dr. Smith");

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).ToBeVisibleAsync();

        await Input("CareLevel").SelectOptionAsync("Assisted Living");
        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("400");
    }
}
