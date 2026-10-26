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

    // Typed dates reach any year without paging the calendar; leaving each one lets its message settle.
    private async Task EnterStay(string admission, string discharge)
    {
        await DatePicker("AdmissionDate").FillAndBlur(admission);
        await DatePicker("DischargeDate").FillAndBlur(discharge);
    }

    [Test]
    public async Task empty_dates_show_required_errors_on_submit()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("AdmissionDate")).ToHaveTextAsync("Admission date is required.", new() { Timeout = 2000 });
        await Expect(ErrorFor("DischargeDate")).ToHaveTextAsync("Discharge date is required.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }

    // The last day of 2019 is before the earliest admission day; the first day of 2020 is it.
    [Test]
    public async Task admission_date_before_2020_shows_min_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("AdmissionDate")).ToHaveTextAsync("Admission date is required.", new() { Timeout = 2000 });

        // Typed dates reach 2019 without paging the calendar back from 2026.
        await DatePicker("AdmissionDate").FillAndBlur("12/31/2019");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("AdmissionDate")).ToHaveTextAsync("Admission date must be on or after January 1, 2020.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await DatePicker("AdmissionDate").FillAndBlur("01/01/2020");
        await Expect(ErrorFor("AdmissionDate")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_admission_date_passes_min_check()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("AdmissionDate")).ToHaveTextAsync("Admission date is required.", new() { Timeout = 2000 });

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

        await Expect(ErrorFor("DischargeDate")).ToHaveTextAsync("Discharge date must be after admission date.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");

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

        await Expect(ErrorFor("DischargeDate")).ToHaveTextAsync("Discharge date must be after admission date.", new() { Timeout = 2000 });
        await Expect(Result).ToHaveTextAsync("Click to validate");

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

        await Expect(Result).ToHaveTextAsync("All dates valid!", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_care_plan_review_dated_december_31_2024_is_refused_and_january_1_2025_is_accepted()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await EnterStay("12/01/2024", "03/31/2025");
        await DatePicker("CarePlanReviewDate").FillAndBlur("12/31/2024");

        await ClickWhenStable(ValidateBtn);

        // GreaterThan(December 31, 2024): the day itself is not after it.
        await Expect(ErrorFor("CarePlanReviewDate")).ToHaveTextAsync("Date the care plan review after December 31, 2024.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await DatePicker("CarePlanReviewDate").FillAndBlur("01/01/2025");
        await ClickWhenStable(ValidateBtn);
        await Expect(Result).ToHaveTextAsync("All dates valid!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_care_plan_review_before_admission_is_refused_and_one_on_the_admission_day_is_accepted()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await EnterStay("03/01/2025", "03/31/2025");
        await DatePicker("CarePlanReviewDate").FillAndBlur("02/28/2025");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("CarePlanReviewDate")).ToHaveTextAsync("The care plan review cannot come before admission.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await DatePicker("CarePlanReviewDate").FillAndBlur("03/01/2025");
        await ClickWhenStable(ValidateBtn);
        await Expect(Result).ToHaveTextAsync("All dates valid!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_care_plan_review_on_the_discharge_day_is_refused()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await EnterStay("03/01/2025", "03/31/2025");
        await DatePicker("CarePlanReviewDate").FillAndBlur("03/31/2025");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("CarePlanReviewDate")).ToHaveTextAsync("The care plan review must come before discharge.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await DatePicker("CarePlanReviewDate").FillAndBlur("03/30/2025");
        await ClickWhenStable(ValidateBtn);
        await Expect(Result).ToHaveTextAsync("All dates valid!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_follow_up_visit_after_discharge_is_refused_and_one_on_the_discharge_day_is_accepted()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await EnterStay("03/01/2025", "03/31/2025");
        await DatePicker("FollowUpVisitDate").FillAndBlur("04/01/2025");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("FollowUpVisitDate")).ToHaveTextAsync("The follow-up visit cannot come after discharge.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await DatePicker("FollowUpVisitDate").FillAndBlur("03/31/2025");
        await ClickWhenStable(ValidateBtn);
        await Expect(Result).ToHaveTextAsync("All dates valid!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_follow_up_visit_on_the_care_plan_review_day_is_refused()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await EnterStay("03/01/2025", "03/31/2025");
        await DatePicker("CarePlanReviewDate").FillAndBlur("03/10/2025");
        await DatePicker("FollowUpVisitDate").FillAndBlur("03/10/2025");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("FollowUpVisitDate"))
            .ToHaveTextAsync("Book the follow-up visit on a different day from the care plan review.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await DatePicker("FollowUpVisitDate").FillAndBlur("03/11/2025");
        await ClickWhenStable(ValidateBtn);
        await Expect(Result).ToHaveTextAsync("All dates valid!", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_move_out_inspection_off_the_discharge_day_is_refused()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await EnterStay("03/01/2025", "03/31/2025");
        await DatePicker("MoveOutInspectionDate").FillAndBlur("03/30/2025");

        await ClickWhenStable(ValidateBtn);

        await Expect(ErrorFor("MoveOutInspectionDate")).ToHaveTextAsync("The move-out inspection happens on the discharge day.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        // A day after discharge is off the discharge day too.
        await DatePicker("MoveOutInspectionDate").FillAndBlur("03/31/2025");
        await Expect(ErrorFor("MoveOutInspectionDate")).ToHaveTextAsync("");
        await DatePicker("MoveOutInspectionDate").FillAndBlur("04/01/2025");
        await ClickWhenStable(ValidateBtn);
        await Expect(ErrorFor("MoveOutInspectionDate")).ToHaveTextAsync("The move-out inspection happens on the discharge day.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await DatePicker("MoveOutInspectionDate").FillAndBlur("03/31/2025");
        await ClickWhenStable(ValidateBtn);
        await Expect(Result).ToHaveTextAsync("All dates valid!", new() { Timeout = 5000 });
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

        await Expect(Result).ToHaveTextAsync("All dates valid!", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
