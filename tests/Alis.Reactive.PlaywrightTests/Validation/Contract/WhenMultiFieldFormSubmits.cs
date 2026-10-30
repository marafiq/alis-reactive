using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract;

[TestFixture]
public class WhenMultiFieldFormSubmits : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/Contract";

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
    public async Task empty_form_blocks_request_and_shows_required_errors_inline()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Name")).ToHaveTextAsync("'Name' is required.");
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToHaveTextAsync("'Email' is required.");
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(ErrorFor("CareLevel")).ToHaveTextAsync("'Care Level' is required.");

        await Expect(Input(m => m.Name)).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Input(m => m.Email)).ToHaveClassAsync(new Regex("alis-has-error"));
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
        await Input(m => m.Name).FillAsync("A");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Name")).ToHaveTextAsync("'Name' must have a minimum length of 2.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task invalid_zipcode_shows_regex_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.Address.ZipCode).FillAsync("abc");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.ZipCode")).ToHaveTextAsync("'Zip Code' must be 5 digits.");
        await Expect(Input(m => m.Address.ZipCode)).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirm_email_fails_when_different_from_email()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.Email).FillAsync("resident@care.com");
        await Input(m => m.ConfirmEmail).FillAsync("wrong@email.com");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ConfirmEmail")).ToHaveTextAsync("'Confirm Email' must match 'Email'.");
        await Expect(Input(m => m.ConfirmEmail)).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    // The page did check the form: the reason for no contact is asked for, and nothing about the emails.
    [Test]
    public async Task confirm_email_passes_when_matches_email()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("ConfirmEmail")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task veteran_id_not_required_when_is_veteran_unchecked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task veteran_id_required_when_is_veteran_checked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.IsVeteran).CheckAsync();

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("VeteranId")).ToHaveTextAsync("'Veteran ID' is required when veteran.");
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
        await Input(m => m.IsVeteran).CheckAsync();
        await Input(m => m.VeteranId).FillAsync("V12345");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_assessment_not_required_when_care_level_is_assisted()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.CareLevel).SelectOptionAsync("Assisted Living");
        await Input(m => m.PhysicianName).FillAsync("Dr. Smith");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_assessment_required_when_care_level_is_memory_care()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.CareLevel).SelectOptionAsync("Memory Care");
        await Input(m => m.PhysicianName).FillAsync("Dr. Smith");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("MemoryAssessmentScore")).ToHaveTextAsync("'Memory Assessment' is required for Memory Care.");
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
        await Input(m => m.CareLevel).SelectOptionAsync("Memory Care");
        await Input(m => m.PhysicianName).FillAsync("Dr. Smith");
        await Input(m => m.ReasonForNoContact).FillAsync("No relatives");

        // Leave the Fusion NumericTextBox empty so submit exercises inline validation.
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).ToHaveTextAsync("'Memory Assessment' is required for Memory Care.");
        await Expect(ErrorFor("MemoryAssessmentScore")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Tab commits the Syncfusion value before the next submit.
        var scoreInput = Input(m => m.MemoryAssessmentScore);
        await scoreInput.ClickAsync();
        await scoreInput.FillAsync("85");
        await scoreInput.PressAsync("Tab");

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task physician_not_required_when_care_level_is_independent()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.CareLevel).SelectOptionAsync("Independent");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task physician_required_when_care_level_is_assisted()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.CareLevel).SelectOptionAsync("Assisted Living");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("PhysicianName")).ToHaveTextAsync("'Physician' is required unless Independent.");
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
        await Input(m => m.CareLevel).SelectOptionAsync("Memory Care");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("PhysicianName")).ToHaveTextAsync("'Physician' is required unless Independent.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reason_for_no_contact_required_when_has_emergency_contact_unchecked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    // With a contact named and reachable, nothing else is asked for: the admission is saved.
    [Test]
    public async Task reason_for_no_contact_not_required_when_has_emergency_contact_checked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.HasEmergencyContact).CheckAsync();
        await Input(m => m.EmergencyName).FillAsync("John");
        await Input(m => m.EmergencyPhone).FillAsync("555-0123");

        await SubmitBtn.ClickAsync();

        await Expect(Result).ToHaveTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_is_veteran_toggles_veteran_id_requirement()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        await Input(m => m.IsVeteran).CheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).ToHaveTextAsync("'Veteran ID' is required when veteran.");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input(m => m.IsVeteran).UncheckAsync();
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

        await Input(m => m.HasEmergencyContact).CheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("EmergencyName")).ToBeVisibleAsync();
        await Expect(ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input(m => m.HasEmergencyContact).UncheckAsync();
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

        await Input(m => m.Name).FillAsync("Jane Smith");
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await FillAllRequired();
        await Input(m => m.ReasonForNoContact).FillAsync("No relatives nearby");
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
        await Input(m => m.Address.Street).FillAsync("");
        await Input(m => m.ReasonForNoContact).FillAsync("No relatives");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.Street")).ToHaveTextAsync("'Street' is required.");
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task empty_address_city_shows_required_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.Address.City).FillAsync("");
        await Input(m => m.ReasonForNoContact).FillAsync("No relatives");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.City")).ToHaveTextAsync("'City' is required.");
        await Expect(ErrorFor("Address.City")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task invalid_email_format_shows_format_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.Email).FillAsync("not-an-email");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Email")).ToHaveTextAsync("'Email' must be a valid email address.");
        await Expect(Input(m => m.Email)).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    // The email rule asks for one @ with something on each side, not a dot after it;
    // the browser asks the same, and the server saves the admission.
    [Test]
    public async Task an_email_with_no_dot_after_the_at_is_accepted_as_the_server_accepts_it()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.Email).FillAsync("jane@sunnyacres");
        await Input(m => m.ConfirmEmail).FillAsync("jane@sunnyacres");
        await Input(m => m.ReasonForNoContact).FillAsync("No relatives nearby");
        await SubmitBtn.ClickAsync();

        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(ErrorFor("Email")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task name_error_clears_on_blur_after_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(Input(m => m.Name)).ToHaveClassAsync(new Regex("alis-has-error"));

        await Input(m => m.Name).FillAsync("Robert Thompson");
        await Input(m => m.Name).BlurAsync();

        await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });
        await Expect(Input(m => m.Name)).Not.ToHaveClassAsync(new Regex("alis-has-error"), new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task email_error_clears_on_blur_after_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();

        await Input(m => m.Email).FillAsync("robert@care.com");
        await Input(m => m.Email).BlurAsync();

        await Expect(ErrorFor("Email")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task zipcode_error_clears_on_blur_after_valid_input()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.Address.ZipCode).FillAsync("abc");
        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.ZipCode")).ToBeVisibleAsync();

        await Input(m => m.Address.ZipCode).FillAsync("62704");
        await Input(m => m.Address.ZipCode).BlurAsync();

        await Expect(ErrorFor("Address.ZipCode")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task complete_form_with_all_conditional_fields_succeeds()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Name).FillAsync("Eleanor Davis");
        await Input(m => m.Email).FillAsync("eleanor@care.com");
        await Input(m => m.ConfirmEmail).FillAsync("eleanor@care.com");
        await Input(m => m.CareLevel).SelectOptionAsync("Memory Care");

        await Input(m => m.IsVeteran).CheckAsync();
        await Input(m => m.VeteranId).FillAsync("V99999");

        await Input(m => m.PhysicianName).FillAsync("Dr. Martinez");
        var assessmentInput = Input(m => m.MemoryAssessmentScore);
        await assessmentInput.ClickAsync();
        await assessmentInput.FillAsync("72");
        await assessmentInput.PressAsync("Tab");

        await Input(m => m.HasEmergencyContact).CheckAsync();
        await Input(m => m.EmergencyName).FillAsync("Michael Davis");
        await Input(m => m.EmergencyPhone).FillAsync("555-9876");

        await Input(m => m.Address.Street).FillAsync("456 Oak Lane");
        await Input(m => m.Address.City).FillAsync("Portland");
        await Input(m => m.Address.ZipCode).FillAsync("97201");

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
        await Input(m => m.Email).FillAsync("test@care.com");
        await Input(m => m.ConfirmEmail).FillAsync("wrong@care.com");

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ConfirmEmail")).ToHaveTextAsync("'Confirm Email' must match 'Email'.");

        await Input(m => m.ConfirmEmail).FillAsync("test@care.com");
        await Input(m => m.ConfirmEmail).BlurAsync();

        await Expect(ErrorFor("ConfirmEmail")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task switching_care_level_away_from_memory_care_removes_assessment_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.CareLevel).SelectOptionAsync("Memory Care");
        await Input(m => m.PhysicianName).FillAsync("Dr. Smith");

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).ToBeVisibleAsync();

        await Input(m => m.CareLevel).SelectOptionAsync("Assisted Living");
        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ReasonForNoContact")).ToHaveTextAsync("'Reason' is required when no emergency contact.");
        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }
}
