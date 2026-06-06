namespace Alis.Reactive.PlaywrightTests.Components.Native;

[TestFixture]
public class WhenDropdownSelectionChanges : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeDropDown";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeDropDownModel__";

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

        var select = Page.Locator($"#{ModelIdPrefix}CareLevel");
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

        var select = Page.Locator($"#{ModelIdPrefix}FacilityType");
        await select.SelectOptionAsync("Medical");

        var notice = Page.Locator("#medical-notice");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_else_for_non_medical()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{ModelIdPrefix}FacilityType");
        await select.SelectOptionAsync("Residential");

        var notice = Page.Locator("#medical-notice");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_selected()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{ModelIdPrefix}CareLevel");
        await select.SelectOptionAsync("");

        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_selection_multiple_times_updates_status_each_time()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{ModelIdPrefix}FacilityType");
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
        await NavigateAndBoot();

        var select = Page.Locator($"#{ModelIdPrefix}CareLevel");
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
    public async Task plan_carries_native_vendor_for_dropdown_set_reactions()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"vendor\": \"native\""),
            "Plan must carry vendor 'native' for dropdown set reactions — " +
            "runtime uses this to choose resolveRoot strategy");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_value_value_member_for_component_source()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"member\": \"value\""),
            "Plan must carry property member 'value' for NativeDropDown component sources — " +
            "runtime reads this property to get the selected value");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_prop_value_for_setvalue_reaction()
    {
        // TODO: This raw JSON assertion can match DOM text updates instead of NativeDropDown SetValue.
        // Replace it with a focused value-property proof in a test-design slice.
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"property\": \"text\""),
            "Plan must carry property .text. for SetValue reaction — " +
            "runtime uses bracket notation root[prop] = val");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task both_dropdowns_render_with_correct_element_ids()
    {
        // Generated IDs are the DOM/Reactive Plan join keys.
        await NavigateAndBoot();

        await Expect(Page.Locator($"#{ModelIdPrefix}CareLevel")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{ModelIdPrefix}FacilityType")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task both_dropdowns_render_as_select_elements()
    {
        // Native dropdown runtime reads and writes el.value.
        await NavigateAndBoot();

        var careLevel = Page.Locator($"select#{ModelIdPrefix}CareLevel");
        var facilityType = Page.Locator($"select#{ModelIdPrefix}FacilityType");

        await Expect(careLevel).ToHaveCountAsync(1);
        await Expect(facilityType).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task care_level_dropdown_has_all_expected_options()
    {
        await NavigateAndBoot();

        var careLevelOptions = Page.Locator($"#{ModelIdPrefix}CareLevel option");
        const int expectedOptionsIncludingPlaceholder = 5;

        await Expect(careLevelOptions).ToHaveCountAsync(expectedOptionsIncludingPlaceholder);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task facility_type_dropdown_has_all_expected_options()
    {
        await NavigateAndBoot();

        var facilityTypeOptions = Page.Locator($"#{ModelIdPrefix}FacilityType option");
        const int expectedOptionsIncludingPlaceholder = 4;

        await Expect(facilityTypeOptions).ToHaveCountAsync(expectedOptionsIncludingPlaceholder);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task care_level_placeholder_has_empty_value()
    {
        // Placeholder option must have value="" so clearing the dropdown
        // via SelectOptionAsync("") works for the component-read condition test.
        await NavigateAndBoot();

        var placeholder = Page.Locator($"#{ModelIdPrefix}CareLevel option").First;
        await Expect(placeholder).ToHaveAttributeAsync("value", "");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boot_trace_is_emitted_on_page_load()
    {
        await NavigateAndBoot();

        var hasBootTrace = _consoleMessages.Any(m => m.Contains("booted"));
        Assert.That(hasBootTrace, Is.True,
            "Boot trace must be emitted — confirms runtime boot discovered and executed the plan");
        AssertNoConsoleErrors();
    }
}
