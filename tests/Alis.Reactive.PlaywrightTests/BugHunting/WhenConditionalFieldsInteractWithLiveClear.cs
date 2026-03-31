using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.BugHunting;

/// <summary>
/// Bug-hunting tests for conditional visibility + live-clear interaction.
///
/// Hypothesis: live-clear event listeners are attached at boot time via wireLiveValidation().
/// Fields inside hidden containers (veteran-section, memory-section, physician-section) ARE
/// in the DOM at boot, but Fusion components inside hidden containers may not be fully initialized,
/// causing resolveRoot() to throw during live-clear wiring.
///
/// Additionally: validation errors set during one submit persist through hide/show visibility
/// toggles, causing stale errors to reappear when a section is re-shown.
///
/// Page under test: /Sandbox/Validation/Contract/ConditionalHide
/// </summary>
[TestFixture]
public class WhenConditionalFieldsInteractWithLiveClear : PlaywrightTestBase
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

    // ── Bug Hunt 1: Live-clear on native field shown via condition ──

    [Test]
    public async Task live_clear_works_on_veteran_id_after_section_is_shown()
    {
        // VeteranId is a NativeTextBox inside veteran-section (initially hidden).
        // Hypothesis: live-clear listeners were attached at boot (element in DOM even when hidden).
        // If live-clear works: typing clears the error. If not: error persists after typing.
        await NavigateAndBoot();
        await FillAllRequired();

        // Show veteran section
        await Input("IsVeteran").CheckAsync();
        await Expect(Page.Locator("#veteran-section")).ToBeVisibleAsync();

        // Submit → VeteranId required error shows
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");

        // Type in VeteranId → live-clear should fire, error should clear on input
        await Input("VeteranId").FillAsync("V12345");
        await Expect(ErrorFor("VeteranId")).ToBeHiddenAsync(new() { Timeout = 2000 });

        // Error class should be removed
        await Expect(Input("VeteranId")).Not.ToHaveClassAsync(new Regex("alis-has-error"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task live_revalidate_works_on_veteran_id_after_clearing()
    {
        // After live-clear, clearing the field and blurring should re-show the error.
        await NavigateAndBoot();
        await FillAllRequired();

        // Show veteran section, submit to trigger errors
        await Input("IsVeteran").CheckAsync();
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");

        // Fix the error
        await Input("VeteranId").FillAsync("V12345");
        await Expect(ErrorFor("VeteranId")).ToBeHiddenAsync(new() { Timeout = 2000 });

        // Clear and blur → error should reappear via re-validate
        await Input("VeteranId").ClearAsync();
        await Input("VeteranId").BlurAsync();
        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    // ── Bug Hunt 2: Live-clear on Fusion field shown via condition ──

    [Test]
    public async Task live_clear_works_on_memory_score_after_section_is_shown()
    {
        // MemoryAssessmentScore is a FusionNumericTextBox inside memory-section (initially hidden).
        // CRITICAL: if SF didn't initialize the component in a hidden container, resolveRoot()
        // would throw during wireLiveValidation(), leaving this field (and possibly others) unwired.
        // This test proves whether Fusion live-clear works on conditionally-shown Fusion fields.
        await NavigateAndBoot();
        await FillAllRequired();

        // Show memory section
        await Input("CareLevel").SelectOptionAsync("Memory Care");
        await Expect(Page.Locator("#memory-section")).ToBeVisibleAsync();

        // Submit → MemoryAssessmentScore required error shows
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("MemoryAssessmentScore")).ToContainTextAsync("required");

        // Fill via Fusion NumericTextBox interaction → live-clear should fire
        await MemoryScore.FillAndBlur("85");

        // Error should be cleared by live-clear
        await Expect(ErrorFor("MemoryAssessmentScore"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Bug Hunt 3: Stale validation errors after hide-show cycle ──

    [Test]
    public async Task physician_error_does_not_persist_after_hide_show_cycle()
    {
        // Hypothesis: validation errors set inline are NOT cleared by Show/Hide commands.
        // If a field shows an error, gets hidden, and is re-shown, the stale error
        // from the previous submit would reappear — confusing the user.
        await NavigateAndBoot();
        await FillAllRequired();

        // Show physician section (Assisted Living ≠ Independent)
        await Input("CareLevel").SelectOptionAsync("Assisted Living");
        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        // Submit → PhysicianName required error shows inline
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");

        // Hide physician section (switch to Independent)
        await Input("CareLevel").SelectOptionAsync("Independent");
        await Expect(Page.Locator("#physician-section")).ToBeHiddenAsync();

        // Re-show physician section (switch back to Assisted Living)
        await Input("CareLevel").SelectOptionAsync("Assisted Living");
        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        // BUG CHECK: Is the stale error from the first submit still visible?
        // If the Show/Hide commands don't clear validation state, this would fail
        // because the old error text is still in the DOM.
        //
        // Expected behavior: error should NOT be visible after hide-show cycle
        // without a new submit. The user changed their selection — fresh start.
        var errorLocator = ErrorFor("PhysicianName");
        var errorText = await errorLocator.TextContentAsync();

        // Report what we find — this is a bug-hunting test
        if (!string.IsNullOrWhiteSpace(errorText))
        {
            TestContext.Out.WriteLine($"[BUG FOUND] Stale validation error persists after hide-show cycle: '{errorText}'");
        }

        // The stale error from a previous submit should not be visible
        // after toggling visibility without re-submitting
        await Expect(errorLocator).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task emergency_error_does_not_persist_after_toggle_cycle()
    {
        // Same pattern: check → submit with error → uncheck → check → stale error?
        await NavigateAndBoot();
        await FillAllRequired();

        // Check HasEmergencyContact → show emergency section
        await Input("HasEmergencyContact").CheckAsync();
        await Expect(Page.Locator("#emergency-section")).ToBeVisibleAsync();

        // Submit → EmergencyName required error shows
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("EmergencyName")).ToContainTextAsync("required");

        // Uncheck → hide emergency section
        await Input("HasEmergencyContact").UncheckAsync();
        await Expect(Page.Locator("#emergency-section")).ToBeHiddenAsync();

        // Re-check → show emergency section again
        await Input("HasEmergencyContact").CheckAsync();
        await Expect(Page.Locator("#emergency-section")).ToBeVisibleAsync();

        // BUG CHECK: stale error from previous submit
        var errorLocator = ErrorFor("EmergencyName");
        var errorText = await errorLocator.TextContentAsync();

        if (!string.IsNullOrWhiteSpace(errorText))
        {
            TestContext.Out.WriteLine($"[BUG FOUND] Stale EmergencyName error persists after toggle: '{errorText}'");
        }

        await Expect(errorLocator).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    // ── Bug Hunt 4: Console errors from boot with hidden Fusion components ──

    [Test]
    public async Task page_boots_without_console_errors_despite_hidden_fusion_components()
    {
        // The ConditionalHide page has a FusionNumericTextBox inside a hidden container.
        // If resolveRoot() throws during wireLiveValidation() because SF didn't fully
        // initialize the hidden component, we'd see console errors.
        await NavigateAndBoot();

        // Just boot and check — no interactions needed
        AssertNoConsoleErrors();
    }

    // ── Bug Hunt 5: Submit succeeds after fixing all conditionally-shown errors ──

    [Test]
    public async Task full_workflow_with_all_conditional_sections_and_live_clear()
    {
        // Full user journey: reveal all conditional sections, trigger all errors,
        // fix each via live-clear, then submit successfully.
        await NavigateAndBoot();

        // Fill basic required fields
        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("jane@care.com");
        await Input("ConfirmEmail").FillAsync("jane@care.com");
        await Input("Address_Street").FillAsync("123 Main St");
        await Input("Address_City").FillAsync("Springfield");
        await Input("Address_ZipCode").FillAsync("62704");

        // Select Memory Care → shows memory section AND physician section
        await Input("CareLevel").SelectOptionAsync("Memory Care");
        await Expect(Page.Locator("#memory-section")).ToBeVisibleAsync();
        await Expect(Page.Locator("#physician-section")).ToBeVisibleAsync();

        // Check veteran → shows veteran section
        await Input("IsVeteran").CheckAsync();
        await Expect(Page.Locator("#veteran-section")).ToBeVisibleAsync();

        // Check emergency contact → shows emergency section
        await Input("HasEmergencyContact").CheckAsync();
        await Expect(Page.Locator("#emergency-section")).ToBeVisibleAsync();

        // Submit → multiple errors on conditional fields
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");
        await Expect(ErrorFor("MemoryAssessmentScore")).ToContainTextAsync("required");
        await Expect(ErrorFor("PhysicianName")).ToContainTextAsync("required");
        await Expect(ErrorFor("EmergencyName")).ToContainTextAsync("required");
        await Expect(ErrorFor("EmergencyPhone")).ToContainTextAsync("required");

        // Fix all errors via typing (testing live-clear on each)
        await Input("VeteranId").FillAsync("V12345");
        await Expect(ErrorFor("VeteranId")).ToBeHiddenAsync(new() { Timeout = 2000 });

        await MemoryScore.FillAndBlur("85");
        await Expect(ErrorFor("MemoryAssessmentScore")).ToBeHiddenAsync(new() { Timeout = 3000 });

        await Input("PhysicianName").FillAsync("Dr. Smith");
        await Expect(ErrorFor("PhysicianName")).ToBeHiddenAsync(new() { Timeout = 2000 });

        await Input("EmergencyName").FillAsync("John Doe");
        await Expect(ErrorFor("EmergencyName")).ToBeHiddenAsync(new() { Timeout = 2000 });

        await Input("EmergencyPhone").FillAsync("555-0123");
        await Expect(ErrorFor("EmergencyPhone")).ToBeHiddenAsync(new() { Timeout = 2000 });

        // Re-submit → should succeed
        await ClickWhenStable(SubmitBtn);
        await Expect(Page.Locator("#result"))
            .ToHaveTextAsync("Admission saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
