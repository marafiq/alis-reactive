namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeCheckBox API end-to-end in the browser:
/// property writes (SetChecked), property reads (Value as source),
/// reactive events (Changed with typed condition), and component-read conditions.
///
/// Page under test: /Sandbox/Components/CheckBox
/// </summary>
[TestFixture]
public class WhenCheckboxToggles : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/CheckBox";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_CheckBoxModel__";

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
        await Expect(Page).ToHaveTitleAsync("NativeCheckBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"prop\""),
            "Plan must contain structured prop field");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write — DomReady unchecks medication checkbox ──

    [Test]
    public async Task domready_unchecks_medication_checkbox()
    {
        // ReceivesMedication starts checked in the model, but DomReady calls SetChecked(false).
        // Expected: checkbox is unchecked after boot.
        await NavigateAndBoot();

        var cb = Page.Locator($"#{Scope}ReceivesMedication");
        await Expect(cb).Not.ToBeCheckedAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read — DomReady reads AllowsVisitors value into echo ──

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("false", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Changed event with typed condition ──

    [Test]
    public async Task changed_event_shows_restrictions_when_checked()
    {
        await NavigateAndBoot();

        // Restrictions panel starts hidden
        await Expect(Page.Locator("#restrictions-panel")).ToBeHiddenAsync();

        // Check the dietary restrictions checkbox
        await Page.Locator($"#{Scope}HasDietaryRestrictions").CheckAsync();

        // Restrictions panel should appear and status should say "checked"
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

        // Check then uncheck the dietary restrictions checkbox
        await Page.Locator($"#{Scope}HasDietaryRestrictions").CheckAsync();
        await Expect(Page.Locator("#restrictions-panel"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });

        await Page.Locator($"#{Scope}HasDietaryRestrictions").UncheckAsync();

        // Restrictions panel should hide and status should say "unchecked"
        await Expect(Page.Locator("#restrictions-panel"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#restrictions-status"))
            .ToHaveTextAsync("unchecked", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component value condition ──

    [Test]
    public async Task component_value_condition_confirms_when_checked()
    {
        await NavigateAndBoot();

        // DomReady unchecked the medication checkbox — re-check it first
        await Page.Locator($"#{Scope}ReceivesMedication").CheckAsync();

        // Click the button that checks medication status
        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("resident receives medication", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_unchecked()
    {
        await NavigateAndBoot();

        // DomReady already unchecked the medication checkbox — just click check
        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("no medication on record", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-step state-cycle scenarios ──

    [Test]
    public async Task checking_then_unchecking_toggles_extras_visibility_both_ways()
    {
        // Proves the full show->hide->show cycle works (catches state management bugs)
        await NavigateAndBoot();

        var cb = Page.Locator($"#{Scope}HasDietaryRestrictions");
        var panel = Page.Locator("#restrictions-panel");
        var status = Page.Locator("#restrictions-status");

        // Check -> extras show
        await cb.CheckAsync();
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(status).ToHaveTextAsync("checked", new() { Timeout = 3000 });

        // Uncheck -> extras hide
        await cb.UncheckAsync();
        await Expect(panel).ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(status).ToHaveTextAsync("unchecked", new() { Timeout = 3000 });

        // Check again -> extras show again (proves state is not stuck)
        await cb.CheckAsync();
        await Expect(panel).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(status).ToHaveTextAsync("checked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_medication_checkbox_updates_condition_result_each_time()
    {
        // Proves the component-read condition re-evaluates on every button click
        // after the checkbox state changes (catches stale reads)
        await NavigateAndBoot();

        var cb = Page.Locator($"#{Scope}ReceivesMedication");
        var btn = Page.Locator("#check-medication-btn");
        var warning = Page.Locator("#medication-warning");

        // DomReady unchecked it -> button should say "no medication on record"
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("no medication on record", new() { Timeout = 3000 });

        // Check it -> button should now say "resident receives medication"
        await cb.CheckAsync();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("resident receives medication", new() { Timeout = 3000 });

        // Uncheck it again -> button should revert to "no medication on record"
        await cb.UncheckAsync();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("no medication on record", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Plan JSON structure — refactoring safety ──

    [Test]
    public async Task plan_carries_native_vendor_for_checkbox_mutations()
    {
        // The plan must declare vendor "native" so the runtime resolves
        // the raw DOM element (not ej2_instances). If vendor is missing or wrong,
        // resolveRoot returns the wrong object and SetChecked silently breaks.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"vendor\": \"native\""),
            "Plan must carry vendor 'native' for checkbox mutations — " +
            "runtime uses this to choose resolveRoot strategy");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_checked_readexpr_for_component_source()
    {
        // NativeCheckBox.ReadExpr is "checked" — the plan's ComponentSource must
        // carry this so the runtime reads el.checked (not el.value).
        // If readExpr changes or is lost, component value reads return wrong data.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"readExpr\": \"checked\""),
            "Plan must carry readExpr 'checked' for NativeCheckBox component sources — " +
            "runtime walks this path to read the checkbox state");
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
        Assert.That(planJson, Does.Contain("\"coerce\": \"boolean\""),
            "Plan must carry coerce 'boolean' for SetChecked — " +
            "without it, string 'false' is truthy and checkbox stays checked");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_prop_checked_for_setchecked_mutation()
    {
        // SetChecked writes to prop "checked" (not "value"). If prop changes,
        // the runtime writes to the wrong DOM property.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"prop\": \"checked\""),
            "Plan must carry prop 'checked' for SetChecked mutation — " +
            "runtime uses bracket notation root[prop] = val");
        AssertNoConsoleErrors();
    }

    // ── Initial DOM state — element rendering ──

    [Test]
    public async Task all_three_checkboxes_render_with_correct_element_ids()
    {
        // IdGenerator creates scoped IDs from the model namespace + property name.
        // If IdGenerator changes, these elements vanish and all reactive wiring breaks.
        await NavigateAndBoot();

        await Expect(Page.Locator($"#{Scope}ReceivesMedication")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{Scope}AllowsVisitors")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{Scope}HasDietaryRestrictions")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_checkboxes_render_as_input_type_checkbox()
    {
        // NativeCheckBoxBuilder renders <input type="checkbox">. If the HTML element
        // type changes, the runtime's el.checked read/write path breaks silently.
        await NavigateAndBoot();

        var medication = Page.Locator($"#{Scope}ReceivesMedication");
        var visitors = Page.Locator($"#{Scope}AllowsVisitors");
        var dietary = Page.Locator($"#{Scope}HasDietaryRestrictions");

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
