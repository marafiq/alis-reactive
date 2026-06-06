namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeCheckBox SetChecked, Value reads, Changed-event conditions,
/// and component-read conditions.
/// </summary>
[TestFixture]
public class WhenCheckboxToggles : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/CheckBox";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_CheckBoxModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeCheckBox — Alis.Reactive Sandbox");
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
            "Plan must contain structured property field");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_unchecks_medication_checkbox()
    {
        // ReceivesMedication starts checked in the model, but DomReady calls SetChecked(false).
        await NavigateAndBoot();

        var medicationCheckbox = Page.Locator($"#{ModelIdPrefix}ReceivesMedication");
        await Expect(medicationCheckbox).Not.ToBeCheckedAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("false", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_shows_restrictions_when_checked()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#restrictions-panel")).ToBeHiddenAsync();

        await Page.Locator($"#{ModelIdPrefix}HasDietaryRestrictions").CheckAsync();

        await Expect(Page.Locator("#restrictions-panel"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#restrictions-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_hides_restrictions_when_unchecked()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}HasDietaryRestrictions").CheckAsync();
        await Expect(Page.Locator("#restrictions-panel"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });

        await Page.Locator($"#{ModelIdPrefix}HasDietaryRestrictions").UncheckAsync();

        await Expect(Page.Locator("#restrictions-panel"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#restrictions-status"))
            .ToHaveTextAsync("unchecked", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_checked()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}ReceivesMedication").CheckAsync();

        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("resident receives medication", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_unchecked()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("no medication on record", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_then_unchecking_toggles_extras_visibility_both_ways()
    {
        await NavigateAndBoot();

        var dietaryRestrictionsCheckbox = Page.Locator($"#{ModelIdPrefix}HasDietaryRestrictions");
        var panel = Page.Locator("#restrictions-panel");
        var status = Page.Locator("#restrictions-status");

        await dietaryRestrictionsCheckbox.CheckAsync();
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(status).ToHaveTextAsync("checked", new() { Timeout = 3000 });

        await dietaryRestrictionsCheckbox.UncheckAsync();
        await Expect(panel).ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(status).ToHaveTextAsync("unchecked", new() { Timeout = 3000 });

        await dietaryRestrictionsCheckbox.CheckAsync();
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(status).ToHaveTextAsync("checked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_medication_checkbox_updates_condition_result_each_time()
    {
        await NavigateAndBoot();

        var medicationCheckbox = Page.Locator($"#{ModelIdPrefix}ReceivesMedication");
        var checkMedicationButton = Page.Locator("#check-medication-btn");
        var warning = Page.Locator("#medication-warning");

        await checkMedicationButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("no medication on record", new() { Timeout = 3000 });

        await medicationCheckbox.CheckAsync();
        await checkMedicationButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("resident receives medication", new() { Timeout = 3000 });

        await medicationCheckbox.UncheckAsync();
        await checkMedicationButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("no medication on record", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_native_vendor_for_checkbox_set_reactions()
    {
        // The plan must declare vendor "native" so the runtime resolves
        // the raw DOM element (not ej2_instances). If vendor is missing or wrong,
        // resolveRoot returns the wrong object and SetChecked silently breaks.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"vendor\": \"native\""),
            "Plan must carry vendor 'native' for checkbox set reactions — " +
            "runtime uses this to choose resolveRoot strategy");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_checked_value_member_for_component_source()
    {
        // NativeCheckBox valueMember is "checked" — the plan's ComponentSource must
        // carry this so the runtime reads el.checked (not el.value).
        // If valueMember changes or is lost, component value reads return wrong data.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"member\": \"checked\""),
            "Plan must carry property member 'checked' for NativeCheckBox component sources — " +
            "runtime reads this property to get the checkbox state");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_boolean_coerce_for_setchecked()
    {
        // SetChecked emits coerce:"boolean" so the runtime coerces the string "false"
        // to boolean false before assigning to el.checked. Without coerce, the string
        // "false" is truthy and the checkbox stays checked.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"kind\": \"boolean\""),
            "Plan must carry shape boolean for SetChecked — " +
            "without it, string 'false' is truthy and checkbox stays checked");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_prop_checked_for_setchecked_reaction()
    {
        // SetChecked writes to prop "checked" (not "value"). If prop changes,
        // the runtime writes to the wrong DOM property.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"property\": \"checked\""),
            "Plan must carry property .checked. for SetChecked reaction — " +
            "runtime uses bracket notation root[prop] = val");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_three_checkboxes_render_with_correct_element_ids()
    {
        // IdGenerator creates scoped IDs from the model namespace + property name.
        // If IdGenerator changes, these elements vanish and all reactive wiring breaks.
        await NavigateAndBoot();

        await Expect(Page.Locator($"#{ModelIdPrefix}ReceivesMedication")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{ModelIdPrefix}AllowsVisitors")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{ModelIdPrefix}HasDietaryRestrictions")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_checkboxes_render_as_input_type_checkbox()
    {
        // NativeCheckBoxBuilder renders <input type="checkbox">. If the HTML element
        // type changes, the runtime's el.checked read/write path breaks silently.
        await NavigateAndBoot();

        var medication = Page.Locator($"#{ModelIdPrefix}ReceivesMedication");
        var visitors = Page.Locator($"#{ModelIdPrefix}AllowsVisitors");
        var dietary = Page.Locator($"#{ModelIdPrefix}HasDietaryRestrictions");

        await Expect(medication).ToHaveAttributeAsync("type", "checkbox");
        await Expect(visitors).ToHaveAttributeAsync("type", "checkbox");
        await Expect(dietary).ToHaveAttributeAsync("type", "checkbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task restrictions_panel_starts_hidden_before_any_interaction()
    {
        // The restrictions panel has hidden attribute in markup. If the attribute
        // is removed or the element ID changes, the show/hide reactive chain breaks.
        await NavigateAndBoot();

        var panel = Page.Locator("#restrictions-panel");
        await Expect(panel).ToBeHiddenAsync();
        await Expect(panel).ToHaveAttributeAsync("hidden", "");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boot_trace_is_emitted_on_page_load()
    {
        // Runtime boot emits a "booted" trace message. If boot fails silently,
        // no reactive behavior works and tests pass vacuously.
        await NavigateAndBoot();

        var hasBootTrace = _consoleMessages.Any(m => m.Contains("booted"));
        Assert.That(hasBootTrace, Is.True,
            "Boot trace must be emitted — confirms runtime boot discovered and executed the plan");
        AssertNoConsoleErrors();
    }
}
