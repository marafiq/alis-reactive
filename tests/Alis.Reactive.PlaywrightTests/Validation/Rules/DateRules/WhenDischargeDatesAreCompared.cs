using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.DateRules;

[TestFixture]
public sealed class WhenDischargeDatesAreCompared : PlaywrightTestBase
{
    private DateValidationPage Dates => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/DateRules"));

    [Test]
    public async Task discharge_before_admission_shows_an_error()
    {
        await Dates.Open();

        await Dates.DatePicker("AdmissionDate").SelectDate(2025, 3, 15);
        await Dates.DatePicker("DischargeDate").SelectDate(2025, 3, 10);
        await ClickWhenStable(Dates.ValidateButton);

        await Expect(Dates.ErrorFor("DischargeDate")).ToContainTextAsync("after admission", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task discharge_equal_to_admission_still_shows_an_error()
    {
        await Dates.Open();

        await Dates.DatePicker("AdmissionDate").SelectDate(2025, 3, 15);
        await Dates.DatePicker("DischargeDate").SelectDate(2025, 3, 15);
        await ClickWhenStable(Dates.ValidateButton);

        await Expect(Dates.ErrorFor("DischargeDate")).ToContainTextAsync("after admission", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task discharge_after_admission_passes_validation()
    {
        await Dates.Open();

        await Dates.DatePicker("AdmissionDate").SelectDate(2025, 3, 15);
        await Dates.DatePicker("DischargeDate").SelectDate(2025, 3, 20);
        await ClickWhenStable(Dates.ValidateButton);

        await Expect(Dates.Result).ToContainTextAsync("valid", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fully_valid_dates_show_success()
    {
        await Dates.Open();

        await Dates.DatePicker("AdmissionDate").SelectDate(2025, 1, 10);
        await Dates.DatePicker("DischargeDate").SelectDate(2025, 2, 15);
        await ClickWhenStable(Dates.ValidateButton);

        await Expect(Dates.Result).ToContainTextAsync("valid", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
