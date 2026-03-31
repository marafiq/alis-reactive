using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.BugHunting;

/// <summary>
/// Bug-hunting: focuses on alis-has-error CSS class persistence through visibility toggles
/// and the combination of server validation errors with conditional fields.
///
/// When clearAllInline runs during re-submit, it clears error spans and removes alis-has-error.
/// But between submissions, Show/Hide toggling doesn't touch error state.
/// This tests whether the alis-has-error CLASS persists on inputs after hide-show,
/// even if the error text is eventually cleared by re-submission.
///
/// Also tests: server error (400) arrives for fields that are now hidden.
///
/// Page under test: /Sandbox/Validation/Contract/ConditionalHide
/// </summary>
[TestFixture]
public class WhenErrorStylingPersistsAfterVisibilityToggle : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/Contract/ConditionalHide";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");
    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private NumericTextBoxLocator MemoryScore =>
        new(Page, $"{R}MemoryAssessmentScore");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

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

    // ── Bug Hunt: alis-has-error class persists through hide-show cycle ──

    [Test]
    public async Task alis_has_error_class_persists_on_physician_after_hide_show_cycle()
    {
        // Show physician → submit (error + alis-has-error) → hide → show → check class
        await NavigateAndBoot();
        await FillAllRequired();

        // Show physician section
        await Input("CareLevel").SelectOptionAsync("Assisted Living");
        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        // Submit → PhysicianName gets error + alis-has-error class
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");
        await Expect(Input("PhysicianName")).ToHaveClassAsync(new Regex("alis-has-error"));

        // Hide physician section
        await Input("CareLevel").SelectOptionAsync("Independent");
        await Expect(Page.Locator("#physician-section")).ToBeHiddenAsync();

        // Re-show physician section
        await Input("CareLevel").SelectOptionAsync("Assisted Living");
        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        // BUG CHECK: does the alis-has-error class persist?
        var hasErrorClass = await Input("PhysicianName")
            .EvaluateAsync<bool>("el => el.classList.contains('alis-has-error')");

        if (hasErrorClass)
        {
            TestContext.Out.WriteLine(
                "[BUG FOUND] alis-has-error class persists on PhysicianName input after hide-show cycle — " +
                "input has red border from stale error styling without any visible error message");
        }

        // The error class should NOT persist — field is in a fresh visual state
        await Expect(Input("PhysicianName")).Not.ToHaveClassAsync(new Regex("alis-has-error"));

        AssertNoConsoleErrors();
    }

    // ── Bug Hunt: re-submit after hide-show cycle clears stale state ──

    [Test]
    public async Task resubmit_after_hide_show_clears_stale_errors_and_classes()
    {
        // Even if stale error text/class persists through the toggle,
        // a fresh submit should clear everything and re-evaluate from scratch.
        await NavigateAndBoot();
        await FillAllRequired();

        // Show physician section → submit → error
        await Input("CareLevel").SelectOptionAsync("Assisted Living");
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");

        // Hide → show cycle
        await Input("CareLevel").SelectOptionAsync("Independent");
        await Input("CareLevel").SelectOptionAsync("Assisted Living");
        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        // Fill the field this time
        await Input("PhysicianName").FillAsync("Dr. Smith");

        // Re-submit → should succeed (all required fields are filled)
        await ClickWhenStable(SubmitBtn);

        // PhysicianName should have NO error and NO error class
        await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Input("PhysicianName")).Not.ToHaveClassAsync(new Regex("alis-has-error"));

        AssertNoConsoleErrors();
    }

    // ── Bug Hunt: Memory score Fusion field error class after hide-show ──

    [Test]
    public async Task memory_score_fusion_field_error_class_persists_after_hide_show()
    {
        // Same test for Fusion component — alis-has-error on the SF input element
        await NavigateAndBoot();
        await FillAllRequired();

        // Show memory section
        await Input("CareLevel").SelectOptionAsync("Memory Care");
        await Expect(Page.Locator("#memory-section")).ToBeVisibleAsync();

        // Submit → MemoryAssessmentScore gets error
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("MemoryAssessmentScore")).ToContainTextAsync("required");

        // Hide memory section
        await Input("CareLevel").SelectOptionAsync("Independent");
        await Expect(Page.Locator("#memory-section")).ToBeHiddenAsync();

        // Re-show memory section
        await Input("CareLevel").SelectOptionAsync("Memory Care");
        await Expect(Page.Locator("#memory-section")).ToBeVisibleAsync();

        // BUG CHECK: does the alis-has-error class persist on the Fusion input?
        var hasErrorClass = await MemoryScore.Input
            .EvaluateAsync<bool>("el => el.classList.contains('alis-has-error')");

        if (hasErrorClass)
        {
            TestContext.Out.WriteLine(
                "[BUG FOUND] alis-has-error class persists on FusionNumericTextBox after hide-show cycle — " +
                "Fusion component shows red error styling from stale state");
        }

        await Expect(MemoryScore.Input).Not.ToHaveClassAsync(new Regex("alis-has-error"));

        AssertNoConsoleErrors();
    }

    // ── Bug Hunt: submit with ALL conditional sections shown and filled ──

    [Test]
    public async Task submit_succeeds_when_all_conditional_fields_are_filled_correctly()
    {
        // Full happy path: show every section, fill everything, submit → success.
        // Catches any accumulation of stale state preventing valid submissions.
        await NavigateAndBoot();

        // Fill all basic required fields
        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("jane@care.com");
        await Input("ConfirmEmail").FillAsync("jane@care.com");
        await Input("Address_Street").FillAsync("123 Main St");
        await Input("Address_City").FillAsync("Springfield");
        await Input("Address_ZipCode").FillAsync("62704");

        // Memory Care shows both memory + physician sections
        await Input("CareLevel").SelectOptionAsync("Memory Care");
        await Expect(Page.Locator("#memory-section")).ToBeVisibleAsync();
        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        // Fill conditional fields
        await MemoryScore.FillAndBlur("85");
        await Input("PhysicianName").FillAsync("Dr. Smith");

        // Veteran section
        await Input("IsVeteran").CheckAsync();
        await Input("VeteranId").FillAsync("V12345");

        // Emergency contact
        await Input("HasEmergencyContact").CheckAsync();
        await Input("EmergencyName").FillAsync("John Doe");
        await Input("EmergencyPhone").FillAsync("555-0123");

        // Submit → should succeed
        await ClickWhenStable(SubmitBtn);
        await Expect(Page.Locator("#result"))
            .ToHaveTextAsync("Admission saved", new() { Timeout = 5000 });

        // Zero inline errors, zero summary
        await Expect(ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();
        await Expect(Page.Locator("[data-reactive-validation-summary]")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }
}
