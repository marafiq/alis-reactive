namespace Alis.Reactive.PlaywrightTests.Requests;

/// <summary>
/// Exercises IncludeAll() gather across all 18 input component types in a single HTTP POST.
/// Page under test: /Sandbox/ComponentGather
///
/// Two submit modes: JSON POST (EchoJson) and FormData POST (EchoFormData).
/// FluentValidation via ComponentGatherValidator — all fields required.
/// Server echoes received fields; tests verify the echo response populates.
/// </summary>
[TestFixture]
public class WhenGatheringAllComponentTypes : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ComponentGather";
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

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("ComponentGather — Alis.Reactive Sandbox");
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
            "**/ComponentGather/EchoJson");

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

        await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-json-btn").ClickAsync(),
            "**/ComponentGather/EchoJson");

        var fieldCount = Page.Locator("#echo-field-count");
        await Expect(fieldCount).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_echo_shows_submit_mode()
    {
        await NavigateAndBoot();
        await FillAllRequiredFields();

        await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-json-btn").ClickAsync(),
            "**/ComponentGather/EchoJson");

        await Expect(Page.Locator("#submit-mode"))
            .ToHaveTextAsync("JSON", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
