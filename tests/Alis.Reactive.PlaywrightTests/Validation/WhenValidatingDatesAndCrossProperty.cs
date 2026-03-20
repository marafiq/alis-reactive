namespace Alis.Reactive.PlaywrightTests.Validation;

/// <summary>
/// Playwright tests for /Sandbox/DateValidation — verifies date-aware validation
/// with coerceAs: "date" and cross-property comparisons (discharge > admission).
/// Uses FusionDatePicker (Syncfusion) components.
/// </summary>
[TestFixture]
public class WhenValidatingDatesAndCrossProperty : PlaywrightTestBase
{
    private const string Path = "/Sandbox/DateValidation";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateValidationModel__";

    private ILocator ValidateBtn => Page.Locator("#validate-dates-btn");
    private ILocator Result => Page.Locator("#date-result");
    private ILocator ErrorFor(string suffix) => Page.Locator($"#{R}{suffix}_error");

    private async Task SetDatePickerValue(string suffix, string isoDate)
    {
        var elementId = R + suffix;
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{elementId}'); " +
            $"const inst = el.ej2_instances[0]; " +
            $"inst.value = new Date('{isoDate}T00:00:00'); " +
            $"inst.dataBind(); }}");
    }

    private async Task ClearDatePicker(string suffix)
    {
        var elementId = R + suffix;
        await Page.EvaluateAsync(
            $"() => {{ const el = document.getElementById('{elementId}'); " +
            $"const inst = el.ej2_instances[0]; " +
            $"inst.value = null; " +
            $"inst.dataBind(); }}");
    }

    // ── Required ─────────────────────────────────────────────

    [Test]
    public async Task empty_dates_show_required_errors_on_submit()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });
        await Expect(ErrorFor("DischargeDate")).ToContainTextAsync("required", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    // ── Min date (admission >= 2020-01-01) ───────────────────

    [Test]
    public async Task admission_date_before_2020_shows_min_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Submit first to trigger validation
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });

        // Set date before 2020
        await SetDatePickerValue("AdmissionDate", "2019-06-15");

        // Trigger blur/change re-validation by clicking elsewhere
        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("AdmissionDate")).ToContainTextAsync("2020", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_admission_date_passes_min_check()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Submit first
        await ValidateBtn.ClickAsync();
        await Expect(ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });

        // Set valid date
        await SetDatePickerValue("AdmissionDate", "2025-03-15");

        // Re-validate
        await ValidateBtn.ClickAsync();

        // Admission should pass (but discharge will still fail)
        await Expect(ErrorFor("AdmissionDate")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    // ── Cross-property (discharge > admission) ───────────────

    [Test]
    public async Task discharge_before_admission_shows_gt_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Set admission to March 15, 2025
        await SetDatePickerValue("AdmissionDate", "2025-03-15");
        // Set discharge BEFORE admission (March 10, 2025)
        await SetDatePickerValue("DischargeDate", "2025-03-10");

        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("DischargeDate")).ToContainTextAsync("after admission", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task discharge_equal_to_admission_shows_gt_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Same date — gt requires strictly greater
        await SetDatePickerValue("AdmissionDate", "2025-03-15");
        await SetDatePickerValue("DischargeDate", "2025-03-15");

        await ValidateBtn.ClickAsync();

        await Expect(ErrorFor("DischargeDate")).ToContainTextAsync("after admission", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_discharge_after_admission_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Admission: March 15, Discharge: March 20
        await SetDatePickerValue("AdmissionDate", "2025-03-15");
        await SetDatePickerValue("DischargeDate", "2025-03-20");

        await ValidateBtn.ClickAsync();

        await Expect(Result).ToContainTextAsync("valid", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Full valid form ──────────────────────────────────────

    [Test]
    public async Task all_valid_dates_pass_validation()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SetDatePickerValue("AdmissionDate", "2025-01-10");
        await SetDatePickerValue("DischargeDate", "2025-02-15");

        await ValidateBtn.ClickAsync();

        await Expect(Result).ToContainTextAsync("valid", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
