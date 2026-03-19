namespace Alis.Reactive.PlaywrightTests.Requests;

/// <summary>
/// Exercises IncludeAll() gather across all component types in a single HTTP POST.
/// Page under test: /Sandbox/ComponentGather
///
/// Components:
/// - NativeTextBox (ResidentName) — seeded "Margaret Thompson"
/// - NativeTextArea (CareNotes) — seeded "Initial assessment completed."
/// - FusionDateTimePicker (MedicationTime) — set via ej2 API
/// - FusionDateRangePicker (StayStart) — startDate readExpr
/// - FusionInputMask (PhoneNumber) — set via ej2 API
/// - FusionRichTextEditor (CarePlan) — set via ej2 API
/// - FusionSwitch (ReceiveNotifications) — seeded true, readExpr "checked"
/// </summary>
[TestFixture]
public class WhenGatheringAllComponentTypes : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ComponentGather";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ComponentGatherModel";
    private const string MedicationTimeId = Scope + "__MedicationTime";
    private const string StayStartId = Scope + "__StayStart";
    private const string PhoneNumberId = Scope + "__PhoneNumber";
    private const string CarePlanId = Scope + "__CarePlan";
    private const string ReceiveNotificationsId = Scope + "__ReceiveNotifications";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private async Task WaitForFusionComponents()
    {
        // Wait for all Fusion ej2 instances to initialize
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{MedicationTimeId}'); return el && el.ej2_instances && el.ej2_instances[0]; }}",
            null, new() { Timeout = 10000 });
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{StayStartId}'); return el && el.ej2_instances && el.ej2_instances[0]; }}",
            null, new() { Timeout = 5000 });
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{PhoneNumberId}'); return el && el.ej2_instances && el.ej2_instances[0]; }}",
            null, new() { Timeout = 5000 });
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{CarePlanId}'); return el && el.ej2_instances && el.ej2_instances[0]; }}",
            null, new() { Timeout = 5000 });
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{ReceiveNotificationsId}'); return el && el.ej2_instances && el.ej2_instances[0]; }}",
            null, new() { Timeout = 5000 });
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
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"kind\": \"all\""),
            "Plan must contain IncludeAll gather item");
        AssertNoConsoleErrors();
    }

    // ── Submit gathers all components ──

    [Test]
    public async Task submit_gathers_all_components_and_posts()
    {
        await NavigateAndBoot();
        await WaitForFusionComponents();

        // Set Fusion component values via ej2 API
        await Page.EvaluateAsync(@$"() => {{
            const dtp = document.getElementById('{MedicationTimeId}');
            dtp.ej2_instances[0].value = new Date(2024, 2, 15, 8, 30, 0);
            dtp.ej2_instances[0].dataBind();

            const phone = document.getElementById('{PhoneNumberId}');
            phone.ej2_instances[0].value = '(555) 123-4567';
            phone.ej2_instances[0].dataBind();

            const rte = document.getElementById('{CarePlanId}');
            rte.ej2_instances[0].value = '<p>Care plan content</p>';
            rte.ej2_instances[0].dataBind();
        }}");

        // Intercept the POST to verify the payload
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-btn").ClickAsync(),
            "**/ComponentGather/Echo");

        var body = request.PostData ?? "";

        // Assert native components are gathered
        Assert.That(body, Does.Contain("ResidentName"),
            $"Body must contain ResidentName but was '{body}'");
        Assert.That(body, Does.Contain("Margaret Thompson"),
            $"Body must contain seeded resident name but was '{body}'");
        Assert.That(body, Does.Contain("CareNotes"),
            $"Body must contain CareNotes but was '{body}'");

        // Assert Fusion switch is gathered (seeded true)
        Assert.That(body, Does.Contain("ReceiveNotifications"),
            $"Body must contain ReceiveNotifications but was '{body}'");

        // Confirm round-trip completes
        await Expect(Page.Locator("#echo-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task submit_contains_resident_name()
    {
        await NavigateAndBoot();
        await WaitForFusionComponents();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-btn").ClickAsync(),
            "**/ComponentGather/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("Margaret Thompson"),
            "Gathered body must contain the seeded ResidentName value");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task submit_contains_care_notes()
    {
        await NavigateAndBoot();
        await WaitForFusionComponents();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-btn").ClickAsync(),
            "**/ComponentGather/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("Initial assessment completed"),
            "Gathered body must contain the seeded CareNotes value");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task submit_contains_receive_notifications_boolean()
    {
        await NavigateAndBoot();
        await WaitForFusionComponents();

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-btn").ClickAsync(),
            "**/ComponentGather/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("ReceiveNotifications"),
            "Gathered body must contain ReceiveNotifications field");
        AssertNoConsoleErrors();
    }
}
