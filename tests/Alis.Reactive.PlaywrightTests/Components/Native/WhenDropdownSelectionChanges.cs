namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeDropDown SetValue, Value reads, Changed-event conditions,
/// and component-read conditions.
/// </summary>
[TestFixture]
public class WhenDropdownSelectionChanges : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeDropDown";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeDropDownModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeDropDown — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_initial_care_level()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}CareLevel");
        await Expect(select).ToHaveValueAsync("Memory Care");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("Memory Care", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_medical_notice()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}FacilityType");
        await select.SelectOptionAsync("Medical");

        var notice = Page.Locator("#medical-notice");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_else_for_non_medical()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}FacilityType");
        await select.SelectOptionAsync("Residential");

        var notice = Page.Locator("#medical-notice");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_selected()
    {
        await NavigateAndBoot();

        // DomReady already set the value under test.
        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}CareLevel");
        await select.SelectOptionAsync("");

        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_selection_multiple_times_updates_status_each_time()
    {
        // The reactive handler must fire on every change, not only the first.
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}FacilityType");
        var notice = Page.Locator("#medical-notice");

        await select.SelectOptionAsync("Medical");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });

        await select.SelectOptionAsync("Residential");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });

        await select.SelectOptionAsync("Medical");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });

        await select.SelectOptionAsync("Rehabilitation");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_care_level_then_reselecting_updates_condition_both_ways()
    {
        // The component-read condition must re-evaluate after a clear-to-reselect cycle.
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}CareLevel");
        var checkCareButton = Page.Locator("#check-care-btn");
        var status = Page.Locator("#care-confirmation");

        await checkCareButton.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });

        await select.SelectOptionAsync("");
        await checkCareButton.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });

        await select.SelectOptionAsync("Skilled Nursing");
        await checkCareButton.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"property\""),
            "Plan must contain structured property field for SetValue");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_native_vendor_for_dropdown_mutations()
    {
        // The plan must declare vendor "native" so the runtime resolves
        // the raw DOM element (not ej2_instances). If vendor is wrong,
        // resolveRoot returns undefined and SetValue silently fails.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"vendor\": \"native\""),
            "Plan must carry vendor 'native' for dropdown mutations — " +
            "runtime uses this to choose resolveRoot strategy");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_value_value_member_for_component_source()
    {
        // NativeDropDown valueMember is "value" — the plan's ComponentSource must
        // carry this so the runtime reads el.value (not el.checked or el.textContent).
        // If valueMember changes or is lost, component value reads return wrong data.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"member\": \"value\""),
            "Plan must carry property member 'value' for NativeDropDown component sources — " +
            "runtime reads this property to get the selected value");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_prop_value_for_setvalue_mutation()
    {
        // TODO: This raw JSON assertion can match DOM text updates instead of NativeDropDown SetValue.
        // Replace it with a focused value-property proof in a test-design slice.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"property\": \"text\""),
            "Plan must carry property .text. for SetValue mutation — " +
            "runtime uses bracket notation root[prop] = val");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task both_dropdowns_render_with_correct_element_ids()
    {
        // IdGenerator creates scoped IDs from the model namespace + property name.
        // If IdGenerator changes, these elements vanish and all reactive wiring breaks.
        await NavigateAndBoot();

        await Expect(Page.Locator($"#{Scope}CareLevel")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{Scope}FacilityType")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task both_dropdowns_render_as_select_elements()
    {
        // NativeDropDownBuilder renders <select>. If the HTML element type changes,
        // the runtime's el.value read/write path breaks silently.
        await NavigateAndBoot();

        var careLevel = Page.Locator($"select#{Scope}CareLevel");
        var facilityType = Page.Locator($"select#{Scope}FacilityType");

        await Expect(careLevel).ToHaveCountAsync(1);
        await Expect(facilityType).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task care_level_dropdown_has_all_expected_options()
    {
        // Controller provides 4 care level items plus 1 placeholder.
        // Missing options prevent SelectOptionAsync and SetValue from matching "Memory Care".
        await NavigateAndBoot();

        var options = Page.Locator($"#{Scope}CareLevel option");
        await Expect(options).ToHaveCountAsync(5);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task facility_type_dropdown_has_all_expected_options()
    {
        // Controller provides 3 facility type items plus 1 placeholder.
        await NavigateAndBoot();

        var options = Page.Locator($"#{Scope}FacilityType option");
        await Expect(options).ToHaveCountAsync(4);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task care_level_placeholder_has_empty_value()
    {
        // The placeholder option must have value="" so clearing the dropdown
        // via SelectOptionAsync("") works for the component-read condition test.
        await NavigateAndBoot();

        var placeholder = Page.Locator($"#{Scope}CareLevel option").First;
        await Expect(placeholder).ToHaveAttributeAsync("value", "");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boot_trace_is_emitted_on_page_load()
    {
        // auto-boot.ts emits a "booted" trace message. If boot fails silently,
        // no reactive behavior works and tests pass vacuously.
        await NavigateAndBoot();

        var hasBootTrace = _consoleMessages.Any(m => m.Contains("booted"));
        Assert.That(hasBootTrace, Is.True,
            "Boot trace must be emitted — confirms auto-boot discovered and executed the plan");
        AssertNoConsoleErrors();
    }
}
