using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.ValidationContract;

[TestFixture]
public class WhenValidatingAllFieldsOnOnePage : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ValidationContract";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-alis-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");

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

    // ── Unconditional rules ──────────────────────────────────

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

        await Input("Name").FillAsync("A");
        await FillAllRequired();
        await Input("Name").FillAsync("A"); // Override back to short name

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

    // ── equalTo ──────────────────────────────────────────────

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

    // ── Condition: truthy ────────────────────────────────────

    [Test]
    public async Task veteran_id_not_required_when_is_veteran_unchecked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        // IsVeteran unchecked by default, VeteranId empty

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

    // ── Condition: eq ────────────────────────────────────────

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

    // ── Condition: neq ───────────────────────────────────────

    [Test]
    public async Task physician_not_required_when_care_level_is_independent()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        // CareLevel defaults to "Independent" via FillAllRequired

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

    // ── Condition: falsy ─────────────────────────────────────

    [Test]
    public async Task reason_for_no_contact_required_when_has_emergency_contact_unchecked()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        // HasEmergencyContact unchecked by default, ReasonForNoContact empty

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

    // ── Condition transitions ────────────────────────────────

    [Test]
    public async Task toggling_is_veteran_toggles_veteran_id_requirement()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        // Unchecked → no error
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        // Check → required
        await Input("IsVeteran").CheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Uncheck → no error again
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

        // Unchecked → ReasonForNoContact required, EmergencyName not required
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Check → EmergencyName required, ReasonForNoContact not required
        await Input("HasEmergencyContact").CheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("EmergencyName")).ToBeVisibleAsync();
        await Expect(ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Uncheck → flips back
        await Input("HasEmergencyContact").UncheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // ── Multi-step fix and resubmit ──────────────────────────

    [Test]
    public async Task fixing_errors_and_resubmitting_clears_previous_errors()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Submit empty → multiple errors
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Fix Name only → Name error gone, others remain
        await Input("Name").FillAsync("Jane Smith");
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Fill remaining → all pass
        await FillAllRequired();
        await Input("ReasonForNoContact").FillAsync("No relatives nearby");
        await SubmitBtn.ClickAsync();

        // All validation should pass — POST should be sent
        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }
}
