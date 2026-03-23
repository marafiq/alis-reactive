namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeDropDown API end-to-end in the browser:
/// property writes (SetValue), property reads (Value as source),
/// reactive events (Changed with typed condition), and component-read conditions.
///
/// Page under test: /Sandbox/Components/NativeDropDown
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

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeDropDown — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write — DomReady sets care level ──

    [Test]
    public async Task domready_sets_initial_care_level()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}CareLevel");
        await Expect(select).ToHaveValueAsync("Memory Care");
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read — DomReady reads care level into echo ──

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("Memory Care", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Changed event with typed condition ──

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

    // ── Section 4: Component value condition ──

    [Test]
    public async Task component_value_condition_confirms_when_selected()
    {
        await NavigateAndBoot();

        // DomReady already set care level to "Memory Care" — just click check
        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        // Reset care level to placeholder (empty value)
        var select = Page.Locator($"#{Scope}CareLevel");
        await select.SelectOptionAsync("");

        // Click the button that checks care level
        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-step state-cycle scenarios ──

    [Test]
    public async Task changing_selection_multiple_times_updates_status_each_time()
    {
        // Proves the reactive handler fires on EVERY change, not just the first
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}FacilityType");
        var notice = Page.Locator("#medical-notice");

        // Select Medical — should show "medical facility selected"
        await select.SelectOptionAsync("Medical");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });

        // Select Residential — should show "not a medical facility"
        await select.SelectOptionAsync("Residential");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });

        // Select Medical again — should show "medical facility selected" again
        await select.SelectOptionAsync("Medical");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });

        // Select Rehabilitation — should show "not a medical facility"
        await select.SelectOptionAsync("Rehabilitation");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_care_level_then_reselecting_updates_condition_both_ways()
    {
        // Proves the component-read condition re-evaluates after clear->reselect cycle
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}CareLevel");
        var btn = Page.Locator("#check-care-btn");
        var status = Page.Locator("#care-confirmation");

        // DomReady set "Memory Care" — button should confirm
        await btn.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });

        // Clear to placeholder
        await select.SelectOptionAsync("");
        await btn.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });

        // Select a different value
        await select.SelectOptionAsync("Skilled Nursing");
        await btn.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Plan JSON structure — refactoring safety ──

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"prop\""),
            "Plan must contain structured prop field for SetValue");
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
    public async Task plan_carries_value_readexpr_for_component_source()
    {
        // NativeDropDown.ReadExpr is "value" — the plan's ComponentSource must
        // carry this so the runtime reads el.value (not el.checked or el.textContent).
        // If readExpr changes or is lost, component value reads return wrong data.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"readExpr\": \"value\""),
            "Plan must carry readExpr 'value' for NativeDropDown component sources — " +
            "runtime walks this path to read the selected value");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_prop_value_for_setvalue_mutation()
    {
        // SetValue writes to prop "value" (not "checked" or "textContent").
        // If prop changes, the runtime writes to the wrong DOM property.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"prop\": \"value\""),
            "Plan must carry prop 'value' for SetValue mutation — " +
            "runtime uses bracket notation root[prop] = val");
        AssertNoConsoleErrors();
    }

    // ── Initial DOM state — element rendering ──

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
        // If the options disappear, SelectOptionAsync fails silently
        // and SetValue can't match "Memory Care".
        await NavigateAndBoot();

        var options = Page.Locator($"#{Scope}CareLevel option");
        // 4 items + 1 placeholder = 5 options
        await Expect(options).ToHaveCountAsync(5);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task facility_type_dropdown_has_all_expected_options()
    {
        // Controller provides 3 facility type items plus 1 placeholder.
        await NavigateAndBoot();

        var options = Page.Locator($"#{Scope}FacilityType option");
        // 3 items + 1 placeholder = 4 options
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

    // ── Boot trace ──

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
