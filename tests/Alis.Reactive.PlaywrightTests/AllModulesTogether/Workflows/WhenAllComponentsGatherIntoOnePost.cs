using System.Text.RegularExpressions;
using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.Workflows;

// Heavy form-fill tests (14+ SF popup interactions) — cannot run in parallel reliably.
// Under parallel load, SF popup animations overlap with other browser instances.
[TestFixture, NonParallelizable]
public class WhenAllComponentsGatherIntoOnePost : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/ComponentGather";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ComponentGatherModel__";
    private ComponentGatherPage _page = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForPageReady(10000);
        _page = new ComponentGatherPage(Page);
    }

    private async Task FillAllRequiredFields()
    {
        // Native scalars (ResidentName + CareNotes are seeded, but fill others)
        // MobilityLevel — select an option
        await Page.Locator($"#{Scope}MobilityLevel").SelectOptionAsync("wheelchair");

        // CareLevel — click a radio button
        await Page.Locator($"#{Scope}CareLevel_r1").ClickAsync(); // "assisted"

        // NativeCheckList — check Dairy (index 1)
        await Page.Locator($"#{Scope}Allergies_c1").ClickAsync();

        // Fusion components — real browser gestures via locator classes

        // FacilityId (DropDownList) — type "Main Campus" + Enter
        await _page.Facility.Select("Main Campus");

        // PhysicianName (AutoComplete) — type partial text, click suggestion
        await _page.Physician.TypeAndSelect("Smith", "Dr. Smith");

        // AdmissionDate (DatePicker) — select a date in current month (no navigation needed)
        var now = DateTime.Now;
        await _page.AdmissionDate.SelectDate(now.Year, now.Month, 15);

        // MedicationTime (TimePicker) — open popup, click "8:30 AM"
        await _page.MedicationTime.SelectTime("8:30 AM");

        // AppointmentTime (DateTimePicker) — use current month date + time
        await _page.AppointmentTime.Select(now.Year, now.Month, 10, "2:00 PM");

        // StayPeriod (DateRangePicker) — current month to next month
        await _page.StayPeriod.SelectRange(now.Year, now.Month, 5, now.Year, now.Month, 20);

        // InsuranceProvider (MultiColumnComboBox) — type "Blue Cross" + Enter
        await _page.InsuranceProvider.Select("Blue Cross");

        // PhoneNumber (InputMask) — type digits into masked input
        await _page.PhoneNumber.FillAndBlur("5551234567");

        // CarePlan (RichTextEditor) — type into contenteditable area
        await _page.CarePlan.FillAndBlur("Care plan content");

        // DietaryRestrictions (MultiSelect) — click each item in popup
        await _page.DietaryRestrictions.SelectItems("Vegetarian", "Halal");
    }

    private async Task SubmitJsonAndWaitForEcho()
    {
        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#submit-json-btn")),
            "**/AllModulesTogether/ComponentGather/EchoJson");
        // Wait for the echo response to populate
        await Expect(Page.Locator("#echo-resident-name"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
    }

    private async Task SubmitFormDataAndWaitForEcho()
    {
        await Page.RunAndWaitForRequestAsync(
            async () => await ClickWhenStable(Page.Locator("#submit-form-btn")),
            "**/AllModulesTogether/ComponentGather/EchoFormData");
        // Wait for the echo response to populate
        await Expect(Page.Locator("#echo-resident-name"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("ComponentGather \u2014 Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── JSON POST ──

    [Test]
    public async Task json_post_gathers_all_fields()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();

        await SubmitJsonAndWaitForEcho();

        // Verify server echoed back key gathered fields
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

    // ── JSON POST — individual field echo verification ──

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

        // Dates are selected using DateTime.Now (current year), not hardcoded 2024
        var year = DateTime.Now.Year.ToString();
        await Expect(Page.Locator("#echo-admission-date"))
            .ToContainTextAsync(year, new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-medication-time"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-appointment-time"))
            .ToContainTextAsync(year, new() { Timeout = 5000 });
        // DateRangePicker: StayPeriod is DateTime[] — echo shows JSON array of date strings
        var stayEcho = Page.Locator("#echo-stay-start");
        await Expect(stayEcho).ToContainTextAsync(year, new() { Timeout = 5000 });
        // Verify both dates are present (array has 2 elements, both contain the year)
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

    // ── FormData POST ──

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

        // Verify server echoed back key gathered fields
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

        // FormData encoding may round-trip fewer fields than JSON (e.g., booleans,
        // arrays serialize differently via multipart/form-data). Verify the server
        // received a substantial number of fields proving gather worked.
        var fieldCountText = await Page.Locator("#echo-field-count").TextContentAsync();
        var fieldCount = int.Parse(fieldCountText!);
        Assert.That(fieldCount, Is.GreaterThanOrEqualTo(14),
            "FormData POST should gather at least 14 of 20 fields");
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

    // ── Validation ──

    [Test]
    public async Task submitting_empty_form_does_not_post_to_server()
    {
        await NavigateAndBoot();

        // Clear seeded values so validation will fail
        await Page.Locator($"#{Scope}ResidentName").ClearWhenStableAsync();
        await Page.Locator($"#{Scope}CareNotes").ClearWhenStableAsync();
        await _page.MonthlyRate.Clear();
        await _page.MonthlyRate.FillAndBlur("0");

        await ClickWhenStable(Page.Locator("#submit-json-btn"));

        // Echo should remain in its default state (em dash)
        await Expect(Page.Locator("#echo-resident-name"))
            .ToHaveTextAsync("\u2014", new() { Timeout = 3000 });
        // Submit mode should still be the default em dash
        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("\u2014", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_shows_resident_name_required_error()
    {
        await NavigateAndBoot();

        // Clear the seeded resident name
        await Page.Locator($"#{Scope}ResidentName").ClearWhenStableAsync();

        await ClickWhenStable(Page.Locator("#submit-json-btn"));

        // Validation error message should appear for ResidentName
        var errorSlot = Page.Locator($"span[data-valmsg-for='ResidentName']");
        await Expect(errorSlot).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_shows_care_notes_required_error()
    {
        await NavigateAndBoot();

        // Clear the seeded care notes
        await Page.Locator($"#{Scope}CareNotes").ClearWhenStableAsync();

        await ClickWhenStable(Page.Locator("#submit-json-btn"));

        var errorSlot = Page.Locator($"span[data-valmsg-for='CareNotes']");
        await Expect(errorSlot).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_shows_mobility_level_required_error()
    {
        await NavigateAndBoot();

        // MobilityLevel is not seeded — submit without selecting it
        await ClickWhenStable(Page.Locator("#submit-json-btn"));

        var errorSlot = Page.Locator($"span[data-valmsg-for='MobilityLevel']");
        await Expect(errorSlot).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fixing_validation_errors_and_resubmitting_succeeds()
    {
        await NavigateAndBoot();

        // First submit — validation should block (missing required fields)
        await ClickWhenStable(Page.Locator("#submit-json-btn"));
        await Expect(Page.Locator($"span[data-valmsg-for='MobilityLevel']"))
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        // Now fill all required fields
        await FillAllRequiredFields();

        // Resubmit — should succeed this time
        await SubmitJsonAndWaitForEcho();

        await Expect(Page.Locator("#echo-resident-name"))
            .ToContainTextAsync("Margaret Thompson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("JSON", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    private sealed class ComponentGatherPage
    {
        private readonly IPage _page;

        public ComponentGatherPage(IPage page)
        {
            _page = page;
        }

        public DropDownListLocator Facility => new(_page, Scope + "FacilityId");
        public AutoCompleteLocator Physician => new(_page, Scope + "PhysicianName");
        public DatePickerLocator AdmissionDate => new(_page, Scope + "AdmissionDate");
        public TimePickerLocator MedicationTime => new(_page, Scope + "MedicationTime");
        public DateTimePickerLocator AppointmentTime => new(_page, Scope + "AppointmentTime");
        public DateRangePickerLocator StayPeriod => new(_page, Scope + "StayPeriod");
        public MultiColumnComboBoxLocator InsuranceProvider => new(_page, Scope + "InsuranceProvider");
        public InputMaskLocator PhoneNumber => new(_page, Scope + "PhoneNumber");
        public RichTextEditorLocator CarePlan => new(_page, Scope + "CarePlan");
        public MultiSelectLocator DietaryRestrictions => new(_page, Scope + "DietaryRestrictions");
        public NumericTextBoxLocator MonthlyRate => new(_page, Scope + "MonthlyRate");
    }
}
