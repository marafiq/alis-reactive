using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Patterns.Workflows;

// TODO: Replace NonParallelizable once Syncfusion popup interactions use stable component-ready signals.
// Under parallel load, 14+ popup interactions can overlap with other browser instances.
[TestFixture, NonParallelizable]
public class WhenAllComponentsGatherIntoOnePost : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/ComponentGather";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ComponentGatherModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private async Task FillAllRequiredFields()
    {
        var scope = new ComponentScope(Page, ModelIdPrefix);

        await Page.Locator($"#{ModelIdPrefix}MobilityLevel").SelectOptionAsync("wheelchair");

        await Page.Locator($"#{ModelIdPrefix}CareLevel_r1").ClickAsync();

        await Page.Locator($"#{ModelIdPrefix}Allergies_c1").ClickAsync();

        var facility = scope.DropDownList("FacilityId");
        await facility.Select("Main Campus");

        var physician = scope.AutoComplete("PhysicianName");
        await physician.TypeAndSelect("Smith", "Dr. Smith");

        var currentCalendarPage = DateTime.Now;
        var admissionDate = scope.DatePicker("AdmissionDate");
        await admissionDate.SelectDate(currentCalendarPage.Year, currentCalendarPage.Month, 15);

        var medTime = scope.TimePicker("MedicationTime");
        await medTime.SelectTime("8:30 AM");

        var aptTime = scope.DateTimePicker("AppointmentTime");
        await aptTime.Select(currentCalendarPage.Year, currentCalendarPage.Month, 10, "2:00 PM");

        var stay = scope.DateRangePicker("StayPeriod");
        await stay.SelectRange(
            currentCalendarPage.Year,
            currentCalendarPage.Month,
            5,
            currentCalendarPage.Year,
            currentCalendarPage.Month,
            20);

        var insurance = scope.MultiColumnComboBox("InsuranceProvider");
        await insurance.Select("Blue Cross");

        var phone = scope.InputMask("PhoneNumber");
        await phone.FillAndBlur("5551234567");

        var carePlan = scope.RichTextEditor("CarePlan");
        await carePlan.FillAndBlur("Care plan content");

        var dietary = scope.MultiSelect("DietaryRestrictions");
        await dietary.SelectItems("Vegetarian", "Halal");
    }

    private async Task SubmitJsonAndWaitForEcho()
    {
        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#submit-json-btn")),
            "**/Patterns/ComponentGather/EchoJson");
        await Expect(Page.Locator("#echo-resident-name"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
    }

    private async Task SubmitFormDataAndWaitForEcho()
    {
        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#submit-form-btn")),
            "**/Patterns/ComponentGather/EchoFormData");
        await Expect(Page.Locator("#echo-resident-name"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("ComponentGather \u2014 Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_element_is_present_and_non_empty()
    {
        await NavigateAndBoot();
        var planEl = Page.Locator("#plan-json");
        await Expect(planEl).ToBeAttachedAsync(new() { Timeout = 5000 });
        var text = await planEl.TextContentAsync();
        Assert.That(text, Is.Not.Null.And.Not.Empty, "Plan JSON must be present for runtime boot");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_gathers_all_fields()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();

        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-resident-name"))
            .ToContainTextAsync("Margaret Thompson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-facility-id"))
            .ToContainTextAsync("fac-1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("JSON", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_field_count()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();

        await SubmitJsonAndWaitForEcho();

        var fieldCount = Page.Locator("#echo-field-count");
        await Expect(fieldCount).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_submit_mode()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();

        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("JSON", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_hidden_fields_from_server_seed()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-resident-id"))
            .ToContainTextAsync("RES-1042", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-form-token"))
            .ToContainTextAsync("csrf-abc123", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_native_text_fields()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-resident-name"))
            .ToContainTextAsync("Margaret Thompson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-care-notes"))
            .ToContainTextAsync("Initial assessment", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_native_dropdown_value()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-mobility-level"))
            .ToContainTextAsync("wheelchair", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_native_radio_group_value()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-care-level"))
            .ToContainTextAsync("assisted", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_fusion_facility_dropdown()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-facility-id"))
            .ToContainTextAsync("fac-1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_fusion_autocomplete_value()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-physician-name"))
            .ToContainTextAsync("Dr. Smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_fusion_insurance_provider()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-insurance-provider"))
            .ToContainTextAsync("blue-cross", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_fusion_phone_number()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-phone-number"))
            .ToContainTextAsync("555", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_numeric_monthly_rate()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-monthly-rate"))
            .ToContainTextAsync("4250", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_date_fields()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        var year = DateTime.Now.Year.ToString();
        await Expect(Page.Locator("#echo-admission-date"))
            .ToContainTextAsync(year, new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-medication-time"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-appointment-time"))
            .ToContainTextAsync(year, new() { Timeout = 5000 });
        var stayEcho = Page.Locator("#echo-stay-start");
        await Expect(stayEcho).ToContainTextAsync(year, new() { Timeout = 5000 });
        var stayText = await stayEcho.TextContentAsync();
        Assert.That(stayText, Does.Contain(","),
            "StayPeriod echo must contain two dates (comma-separated in JSON array)");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_care_plan_content()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-care-plan"))
            .ToContainTextAsync("Care plan content", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_all_20_fields_received()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-field-count"))
            .ToHaveTextAsync("20", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_result_shows_success_styling()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitJsonAndWaitForEcho();

        var echoResult = Page.Locator("#echo-result");
        await Expect(echoResult).ToHaveClassAsync(
            new Regex("text-green-600"), new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_echo_shows_submit_mode()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitFormDataAndWaitForEcho();

        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("FormData", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_gathers_all_fields()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();

        await SubmitFormDataAndWaitForEcho();

        await Expect(Page.Locator("#echo-resident-name"))
            .ToContainTextAsync("Margaret Thompson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-facility-id"))
            .ToContainTextAsync("fac-1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("FormData", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_echo_shows_hidden_fields()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitFormDataAndWaitForEcho();

        await Expect(Page.Locator("#echo-resident-id"))
            .ToContainTextAsync("RES-1042", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-form-token"))
            .ToContainTextAsync("csrf-abc123", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_echo_shows_resident_name()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitFormDataAndWaitForEcho();

        await Expect(Page.Locator("#echo-resident-name"))
            .ToContainTextAsync("Margaret Thompson", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_echo_shows_facility_id()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitFormDataAndWaitForEcho();

        await Expect(Page.Locator("#echo-facility-id"))
            .ToContainTextAsync("fac-1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_echo_receives_gathered_fields()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitFormDataAndWaitForEcho();

        await Expect(Page.Locator("#echo-field-count"))
            .ToHaveTextAsync("20", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_echo_shows_success_styling()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();
        await SubmitFormDataAndWaitForEcho();

        var echoResult = Page.Locator("#echo-result");
        await Expect(echoResult).ToHaveClassAsync(
            new Regex("text-green-600"), new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task explicit_component_id_include_posts_the_registered_member_value_only()
    {
        await NavigateAndBoot();
        await Page.Locator($"#{ModelIdPrefix}ResidentName").FillAsync("Explicit Ada");

        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#submit-explicit-id-btn")),
            "**/Patterns/ComponentGather/EchoJson");

        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("ExplicitId", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-resident-name"))
            .ToContainTextAsync("Explicit Ada", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-field-count"))
            .ToHaveTextAsync("1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task submitting_empty_form_does_not_post_to_server()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}ResidentName").FillAsync("");
        await Page.Locator($"#{ModelIdPrefix}CareNotes").FillAsync("");
        var monthlyRate = new ComponentScope(Page, ModelIdPrefix).NumericTextBox("MonthlyRate");
        await monthlyRate.Clear();
        await monthlyRate.FillAndBlur("0");

        await Page.Locator("#submit-json-btn").ClickAsync();

        await Expect(Page.Locator("#echo-resident-name"))
            .ToHaveTextAsync("\u2014", new() { Timeout = 3000 });
        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("\u2014", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_shows_resident_name_required_error()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}ResidentName").FillAsync("");

        await Page.Locator("#submit-json-btn").ClickAsync();

        var errorSlot = Page.Locator($"span[data-valmsg-for='ResidentName']");
        await Expect(errorSlot).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_shows_care_notes_required_error()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}CareNotes").FillAsync("");

        await Page.Locator("#submit-json-btn").ClickAsync();

        var errorSlot = Page.Locator($"span[data-valmsg-for='CareNotes']");
        await Expect(errorSlot).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_shows_mobility_level_required_error()
    {
        await NavigateAndBoot();

        await Page.Locator("#submit-json-btn").ClickAsync();

        var errorSlot = Page.Locator($"span[data-valmsg-for='MobilityLevel']");
        await Expect(errorSlot).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fixing_validation_errors_and_resubmitting_succeeds()
    {
        await NavigateAndBoot();

        await Page.Locator("#submit-json-btn").ClickAsync();
        await Expect(Page.Locator($"span[data-valmsg-for='MobilityLevel']"))
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await FillAllRequiredFields();

        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-resident-name"))
            .ToContainTextAsync("Margaret Thompson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("JSON", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
