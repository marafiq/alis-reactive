using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.ValidationContract;

[TestFixture]
public class WhenValidatingWithConditionalVisibility : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ValidationContract/ConditionalHide";
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

    // ── Condition + Show/Hide aligned (veteran) ─────────────

    [Test]
    public async Task hidden_veteran_section_with_truthy_condition_skips_rule()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        // IsVeteran unchecked → veteran-section hidden
        await Expect(Page.Locator("#veteran-section")).ToBeHiddenAsync();

        await SubmitBtn.ClickAsync();

        // VeteranId rule has truthy condition on IsVeteran → condition false → rule skipped
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task shown_veteran_section_with_truthy_condition_fires_rule()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        // Check IsVeteran → veteran-section shown
        await Input("IsVeteran").CheckAsync();
        await Expect(Page.Locator("#veteran-section")).ToBeVisibleAsync();

        await SubmitBtn.ClickAsync();

        // VeteranId rule fires → required error inline
        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");
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

        // 1. Unchecked → hidden → no error
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        // 2. Check → shown → required error
        await Input("IsVeteran").CheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // 3. Fill → no error
        await Input("VeteranId").FillAsync("V12345");
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        // 4. Uncheck → hidden → no error
        await Input("IsVeteran").UncheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    // ── Care level changes show/hide conditional fields ─────

    [Test]
    public async Task independent_hides_physician_and_memory_sections()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();

        // Independent → physician hidden, memory hidden → no errors for either
        await Expect(Page.Locator("#physician-section")).ToBeHiddenAsync();
        await Expect(Page.Locator("#memory-section")).ToBeHiddenAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task assisted_living_shows_physician_section_and_requires_physician()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("CareLevel").SelectOptionAsync("Assisted Living");

        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");
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
        await Input("CareLevel").SelectOptionAsync("Memory Care");

        // Memory section should be shown
        await Expect(Page.Locator("#memory-section")).ToBeVisibleAsync();

        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).ToContainTextAsync("required");
        await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // ── Emergency contact flip (truthy + falsy) ─────────────

    [Test]
    public async Task unchecked_emergency_contact_requires_reason_and_hides_contact_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        // HasEmergencyContact unchecked by default
        // emergency-section hidden, no-contact-section visible

        await SubmitBtn.ClickAsync();

        // Reason required (falsy condition met, field visible)
        await Expect(ErrorFor("ReasonForNoContact")).ToContainTextAsync("required");
        // EmergencyName not required (truthy condition not met)
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
        await Input("HasEmergencyContact").CheckAsync();

        // emergency-section shown, no-contact-section hidden
        await Expect(Page.Locator("#emergency-section")).ToBeVisibleAsync();
        await Expect(Page.Locator("#no-contact-section")).ToBeHiddenAsync();

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("EmergencyName")).ToContainTextAsync("required");
        await Expect(ErrorFor("EmergencyPhone")).ToContainTextAsync("required");
        // Reason NOT required (falsy condition not met)
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

        // 1. Unchecked → Reason required
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // 2. Check → Name required, Reason not required
        await Input("HasEmergencyContact").CheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("EmergencyName")).ToBeVisibleAsync();
        await Expect(ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // 3. Uncheck → flips back
        await Input("HasEmergencyContact").UncheckAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ReasonForNoContact")).ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }
}
