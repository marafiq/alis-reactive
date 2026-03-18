namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeRadioGroup API end-to-end in the browser:
/// text-only variation, text + description variation,
/// form submission (JSON POST + FluentValidation), component-read conditions.
///
/// Page under test: /Sandbox/NativeRadioGroup
/// </summary>
[TestFixture]
public class WhenUsingNativeRadioGroup : PlaywrightTestBase
{
    private const string Path = "/Sandbox/NativeRadioGroup";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeRadioGroupModel__";

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
        await Expect(Page).ToHaveTitleAsync("NativeRadioGroup — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Text-only radio group ──

    [Test]
    public async Task text_only_radio_selects_and_echoes_value()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{Scope}MealPlan_r1").ClickAsync();

        var echo = Page.Locator("#meal-echo");
        await Expect(echo).ToHaveTextAsync("Vegetarian", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task text_only_hidden_input_syncs_on_click()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{Scope}MealPlan_r2").ClickAsync();

        var hidden = Page.Locator($"input[type='hidden']#{Scope}MealPlan");
        await Expect(hidden).ToHaveValueAsync("Diabetic", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 2: Text + description radio group ──

    [Test]
    public async Task description_radio_shows_condition_match()
    {
        await NavigateAndBoot();

        // Click "Memory Care" (index 1)
        await Page.Locator($"#{Scope}CareLevel_r1").ClickAsync();

        var notice = Page.Locator("#care-notice");
        await Expect(notice).ToHaveTextAsync("Memory Care selected — assessment score required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task description_radio_shows_else_for_non_match()
    {
        await NavigateAndBoot();

        // Click "Independent Living" (index 2)
        await Page.Locator($"#{Scope}CareLevel_r2").ClickAsync();

        var notice = Page.Locator("#care-notice");
        await Expect(notice).ToHaveTextAsync("Standard admission process", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Form — validation blocks when empty ──

    [Test]
    public async Task form_validation_blocks_empty_submit()
    {
        await NavigateAndBoot();

        await Page.Locator("#submit-btn").ClickAsync();

        // Both fields should show required errors
        var nameError = Page.Locator($"[data-valmsg-for='ResidentName']");
        await Expect(nameError).ToContainTextAsync("required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_submit_succeeds_with_valid_data()
    {
        await NavigateAndBoot();

        // Fill name
        var nameInput = Page.Locator($"#{Scope}ResidentName");
        await nameInput.FillAsync("Margaret Thompson");

        // Select room type
        await Page.Locator($"#{Scope}RoomType_r1").ClickAsync();

        // Submit
        await Page.Locator("#submit-btn").ClickAsync();

        var result = Page.Locator("#result");
        await Expect(result).ToHaveTextAsync("Preferences saved", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component value condition ──

    [Test]
    public async Task component_value_condition_confirms_when_set()
    {
        await NavigateAndBoot();

        // Select a care level first
        await Page.Locator($"#{Scope}CareLevel_r0").ClickAsync();

        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        // Don't select any care level — hidden input is empty
        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-step state cycle ──

    [Test]
    public async Task changing_selection_multiple_times_updates_each_time()
    {
        await NavigateAndBoot();

        var notice = Page.Locator("#care-notice");

        await Page.Locator($"#{Scope}CareLevel_r1").ClickAsync();
        await Expect(notice).ToHaveTextAsync("Memory Care selected — assessment score required", new() { Timeout = 3000 });

        await Page.Locator($"#{Scope}CareLevel_r0").ClickAsync();
        await Expect(notice).ToHaveTextAsync("Standard admission process", new() { Timeout = 3000 });

        await Page.Locator($"#{Scope}CareLevel_r1").ClickAsync();
        await Expect(notice).ToHaveTextAsync("Memory Care selected — assessment score required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── DOM structure ──

    [Test]
    public async Task hidden_inputs_render_for_all_groups()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator($"input[type='hidden']#{Scope}CareLevel")).ToHaveCountAsync(1);
        await Expect(Page.Locator($"input[type='hidden']#{Scope}MealPlan")).ToHaveCountAsync(1);
        await Expect(Page.Locator($"input[type='hidden']#{Scope}RoomType")).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task care_level_has_four_radio_options()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator($"input[type='radio'][name='CareLevel']")).ToHaveCountAsync(4);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task meal_plan_has_four_radio_options()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator($"input[type='radio'][name='MealPlan']")).ToHaveCountAsync(4);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task room_type_has_three_radio_options()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator($"input[type='radio'][name='RoomType']")).ToHaveCountAsync(3);
        AssertNoConsoleErrors();
    }

    // ── Plan JSON ──

    [Test]
    public async Task plan_carries_native_vendor()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"vendor\": \"native\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_value_readexpr()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"readExpr\": \"value\""));
        AssertNoConsoleErrors();
    }

    // ── Boot trace ──

    [Test]
    public async Task boot_trace_is_emitted()
    {
        await NavigateAndBoot();
        var hasBootTrace = _consoleMessages.Any(m => m.Contains("booted"));
        Assert.That(hasBootTrace, Is.True);
        AssertNoConsoleErrors();
    }
}
