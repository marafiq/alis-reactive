using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

[TestFixture]
public class WhenDateRulesEnforce : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/DateRules";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateValidationModel__";

    private ILocator ValidateBtn => Page.Locator("#validate-dates-btn");
    private ILocator Result => Page.Locator("#date-result");
    private ILocator ErrorFor(string suffix) => Page.Locator($"#{ModelIdPrefix}{suffix}_error");

    private DatePickerLocator DatePicker(string suffix) => new(Page, ModelIdPrefix + suffix);

    [Test]
    public async Task empty_dates_show_required_errors_on_submit()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });
        await Expect(ErrorFor("DischargeDate")).ToContainTextAsync("required", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admission_date_before_2020_shows_min_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });

        // Set date before 2020 using text input instead of calendar navigation.
        // Calendar navigation to 2019 would require 80+ month clicks from 2026.
        await DatePicker("AdmissionDate").FillAndBlur("06/15/2019");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("AdmissionDate")).ToContainTextAsync("2020", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_admission_date_passes_min_check()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });

        await DatePicker("AdmissionDate").SelectDate(2025, 3, 15);

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("AdmissionDate")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task discharge_before_admission_shows_gt_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await DatePicker("AdmissionDate").SelectDate(2025, 3, 15);
        await DatePicker("DischargeDate").SelectDate(2025, 3, 10);

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("DischargeDate")).ToContainTextAsync("after admission", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task discharge_equal_to_admission_shows_gt_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await DatePicker("AdmissionDate").SelectDate(2025, 3, 15);
        await DatePicker("DischargeDate").SelectDate(2025, 3, 15);

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("DischargeDate")).ToContainTextAsync("after admission", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_discharge_after_admission_passes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await DatePicker("AdmissionDate").SelectDate(2025, 3, 15);
        await DatePicker("DischargeDate").SelectDate(2025, 3, 20);

        await ClickWhenStable(ValidateBtn);

        await Expect(Result).ToContainTextAsync("valid", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_valid_dates_pass_validation()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await DatePicker("AdmissionDate").SelectDate(2025, 1, 10);
        await DatePicker("DischargeDate").SelectDate(2025, 2, 15);

        await ClickWhenStable(ValidateBtn);

        await Expect(Result).ToContainTextAsync("valid", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
