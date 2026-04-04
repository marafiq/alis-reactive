using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.DateRules;

[TestFixture]
public sealed class WhenAdmissionDatesAreValidated : PlaywrightTestBase
{
    private DateValidationPage Dates => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/DateRules"));

    [Test]
    public async Task empty_dates_show_required_errors_on_submit()
    {
        await Dates.Open();

        await ClickWhenStable(Dates.ValidateButton);

        await Expect(Dates.ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });
        await Expect(Dates.ErrorFor("DischargeDate")).ToContainTextAsync("required", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admission_date_before_the_minimum_shows_an_error()
    {
        await Dates.Open();

        await ClickWhenStable(Dates.ValidateButton);
        await Expect(Dates.ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });

        await Dates.DatePicker("AdmissionDate").FillAndBlur("06/15/2019");
        await ClickWhenStable(Dates.ValidateButton);

        await Expect(Dates.ErrorFor("AdmissionDate")).ToContainTextAsync("2020", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admission_date_after_the_minimum_clears_the_error()
    {
        await Dates.Open();

        await ClickWhenStable(Dates.ValidateButton);
        await Expect(Dates.ErrorFor("AdmissionDate")).ToContainTextAsync("required", new() { Timeout = 2000 });

        await Dates.DatePicker("AdmissionDate").SelectDate(2025, 3, 15);
        await ClickWhenStable(Dates.ValidateButton);

        await Expect(Dates.ErrorFor("AdmissionDate")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }
}
