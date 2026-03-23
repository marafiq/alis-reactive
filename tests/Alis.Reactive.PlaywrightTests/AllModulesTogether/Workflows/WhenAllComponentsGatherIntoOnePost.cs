namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.Workflows;

/// <summary>
/// Exercises IncludeAll() gather across all 18 input component types in a single HTTP POST.
/// Page under test: /Sandbox/AllModulesTogether/ComponentGather
///
/// Two submit modes: JSON POST (EchoJson) and FormData POST (EchoFormData).
/// FluentValidation via ComponentGatherValidator — all fields required.
/// Server echoes received fields; tests verify the echo response populates.
/// </summary>
[TestFixture]
public class WhenAllComponentsGatherIntoOnePost : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/ComponentGather";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ComponentGatherModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
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

        // Fusion components — set via ej2 API
        await Page.EvaluateAsync(@$"() => {{
            // NumericTextBox — MonthlyRate (seeded 4250 but ensure it's set)
            // FacilityId
            const ddl = document.getElementById('{Scope}FacilityId');
            if (ddl && ddl.ej2_instances) {{ ddl.ej2_instances[0].value = 'fac-1'; ddl.ej2_instances[0].dataBind(); }}

            // PhysicianName
            const ac = document.getElementById('{Scope}PhysicianName');
            if (ac && ac.ej2_instances) {{ ac.ej2_instances[0].value = 'Dr. Smith'; ac.ej2_instances[0].dataBind(); }}

            // AdmissionDate
            const dp = document.getElementById('{Scope}AdmissionDate');
            if (dp && dp.ej2_instances) {{ dp.ej2_instances[0].value = new Date(2024, 2, 15); dp.ej2_instances[0].dataBind(); }}

            // MedicationTime
            const tp = document.getElementById('{Scope}MedicationTime');
            if (tp && tp.ej2_instances) {{ tp.ej2_instances[0].value = new Date(1970, 0, 1, 8, 30, 0); tp.ej2_instances[0].dataBind(); }}

            // AppointmentTime
            const dtp = document.getElementById('{Scope}AppointmentTime');
            if (dtp && dtp.ej2_instances) {{ dtp.ej2_instances[0].value = new Date(2024, 2, 15, 14, 0, 0); dtp.ej2_instances[0].dataBind(); }}

            // StayStart (DateRangePicker)
            const drp = document.getElementById('{Scope}StayStart');
            if (drp && drp.ej2_instances) {{ drp.ej2_instances[0].startDate = new Date(2024, 0, 15); drp.ej2_instances[0].endDate = new Date(2024, 5, 15); drp.ej2_instances[0].dataBind(); }}

            // InsuranceProvider (MultiColumnComboBox)
            const mccb = document.getElementById('{Scope}InsuranceProvider');
            if (mccb && mccb.ej2_instances) {{ mccb.ej2_instances[0].value = 'blue-cross'; mccb.ej2_instances[0].dataBind(); }}

            // PhoneNumber (InputMask)
            const mask = document.getElementById('{Scope}PhoneNumber');
            if (mask && mask.ej2_instances) {{ mask.ej2_instances[0].value = '5551234567'; mask.ej2_instances[0].dataBind(); }}

            // CarePlan (RichTextEditor)
            const rte = document.getElementById('{Scope}CarePlan');
            if (rte && rte.ej2_instances) {{ rte.ej2_instances[0].value = '<p>Care plan content</p>'; rte.ej2_instances[0].dataBind(); }}

            // DietaryRestrictions (MultiSelect)
            const ms = document.getElementById('{Scope}DietaryRestrictions');
            if (ms && ms.ej2_instances) {{ ms.ej2_instances[0].value = ['vegetarian', 'halal']; ms.ej2_instances[0].dataBind(); }}
        }}");
    }

    private async Task SubmitJsonAndWaitForEcho()
    {
        await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-json-btn").ClickAsync(),
            "**/AllModulesTogether/ComponentGather/EchoJson");
        // Wait for the echo response to populate
        await Expect(Page.Locator("#echo-resident-name"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
    }

    private async Task SubmitFormDataAndWaitForEcho()
    {
        await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-form-btn").ClickAsync(),
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

    [Test]
    public async Task plan_json_contains_include_all()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"kind\": \"all\""),
            "Plan must contain IncludeAll gather item");
        AssertNoConsoleErrors();
    }

    // ── JSON POST ──

    [Test]
    public async Task json_post_gathers_all_fields()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-json-btn").ClickAsync(),
            "**/AllModulesTogether/ComponentGather/EchoJson");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("Margaret Thompson"), "JSON body must contain ResidentName");
        Assert.That(body, Does.Contain("CareNotes"), "JSON body must contain CareNotes key");
        Assert.That(body, Does.Contain("ReceiveNotifications"), "JSON body must contain ReceiveNotifications");
        Assert.That(body, Does.Contain("fac-1"), "JSON body must contain FacilityId");

        // Verify echo response displays
        await Expect(Page.Locator("#echo-resident-name"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
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

        await Expect(Page.Locator("#echo-admission-date"))
            .ToContainTextAsync("2024", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-medication-time"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-appointment-time"))
            .ToContainTextAsync("2024", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-stay-start"))
            .ToContainTextAsync("2024", new() { Timeout = 5000 });
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

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-form-btn").ClickAsync(),
            "**/AllModulesTogether/ComponentGather/EchoFormData");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("Margaret Thompson"), "FormData body must contain ResidentName");
        Assert.That(body, Does.Contain("fac-1"), "FormData body must contain FacilityId");

        await Expect(Page.Locator("#echo-resident-name"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
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
    public async Task form_data_post_echo_shows_all_20_fields_received()
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

    // ── Validation ──

    [Test]
    public async Task submitting_empty_form_does_not_post_to_server()
    {
        await NavigateAndBoot();

        // Clear seeded values so validation will fail
        await Page.Locator($"#{Scope}ResidentName").FillAsync("");
        await Page.Locator($"#{Scope}CareNotes").FillAsync("");
        await Page.EvaluateAsync(@$"() => {{
            const ntb = document.getElementById('{Scope}MonthlyRate');
            if (ntb && ntb.ej2_instances) {{ ntb.ej2_instances[0].value = 0; ntb.ej2_instances[0].dataBind(); }}
        }}");

        await Page.Locator("#submit-json-btn").ClickAsync();

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
        await Page.Locator($"#{Scope}ResidentName").FillAsync("");

        await Page.Locator("#submit-json-btn").ClickAsync();

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
        await Page.Locator($"#{Scope}CareNotes").FillAsync("");

        await Page.Locator("#submit-json-btn").ClickAsync();

        var errorSlot = Page.Locator($"span[data-valmsg-for='CareNotes']");
        await Expect(errorSlot).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task validation_shows_mobility_level_required_error()
    {
        await NavigateAndBoot();

        // MobilityLevel is not seeded — submit without selecting it
        await Page.Locator("#submit-json-btn").ClickAsync();

        var errorSlot = Page.Locator($"span[data-valmsg-for='MobilityLevel']");
        await Expect(errorSlot).ToContainTextAsync("required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fixing_validation_errors_and_resubmitting_succeeds()
    {
        await NavigateAndBoot();

        // First submit — validation should block (missing required fields)
        await Page.Locator("#submit-json-btn").ClickAsync();
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
}
