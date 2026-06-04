namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeRadioGroup text-only, text-with-description, form submission,
/// and component-read condition behavior in the browser.
/// </summary>
[TestFixture]
public class WhenRadioOptionSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeRadioGroup";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeRadioGroupModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeRadioGroup — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task text_only_radio_selects_and_echoes_value()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}MealPlan_r1").ClickAsync();

        var echo = Page.Locator("#meal-echo");
        await Expect(echo).ToHaveTextAsync("Vegetarian", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task text_only_hidden_input_syncs_on_click()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}MealPlan_r2").ClickAsync();

        var hidden = Page.Locator($"input[type='hidden']#{ModelIdPrefix}MealPlan");
        await Expect(hidden).ToHaveValueAsync("Diabetic", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task description_radio_shows_condition_match()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}CareLevel_r1").ClickAsync();

        var notice = Page.Locator("#care-notice");
        await Expect(notice).ToHaveTextAsync("Memory Care selected — assessment score required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task description_radio_shows_else_for_non_match()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}CareLevel_r2").ClickAsync();

        var notice = Page.Locator("#care-notice");
        await Expect(notice).ToHaveTextAsync("Standard admission process", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_validation_blocks_empty_submit()
    {
        await NavigateAndBoot();

        await Page.Locator("#submit-btn").ClickAsync();

        var nameError = Page.Locator($"[data-valmsg-for='ResidentName']");
        await Expect(nameError).ToContainTextAsync("required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_submit_succeeds_with_valid_data()
    {
        await NavigateAndBoot();

        var nameInput = Page.Locator($"#{ModelIdPrefix}ResidentName");
        await nameInput.FillAsync("Margaret Thompson");

        await Page.Locator($"#{ModelIdPrefix}RoomType_r1").ClickAsync();

        await Page.Locator("#submit-btn").ClickAsync();

        var result = Page.Locator("#result");
        await Expect(result).ToHaveTextAsync("Preferences saved", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_set()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}CareLevel_r0").ClickAsync();

        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_selection_multiple_times_updates_each_time()
    {
        await NavigateAndBoot();

        var notice = Page.Locator("#care-notice");

        await Page.Locator($"#{ModelIdPrefix}CareLevel_r1").ClickAsync();
        await Expect(notice).ToHaveTextAsync("Memory Care selected — assessment score required", new() { Timeout = 3000 });

        await Page.Locator($"#{ModelIdPrefix}CareLevel_r0").ClickAsync();
        await Expect(notice).ToHaveTextAsync("Standard admission process", new() { Timeout = 3000 });

        await Page.Locator($"#{ModelIdPrefix}CareLevel_r1").ClickAsync();
        await Expect(notice).ToHaveTextAsync("Memory Care selected — assessment score required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_inputs_render_for_all_groups()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator($"input[type='hidden']#{ModelIdPrefix}CareLevel")).ToHaveCountAsync(1);
        await Expect(Page.Locator($"input[type='hidden']#{ModelIdPrefix}MealPlan")).ToHaveCountAsync(1);
        await Expect(Page.Locator($"input[type='hidden']#{ModelIdPrefix}RoomType")).ToHaveCountAsync(1);
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

    [Test]
    public async Task plan_carries_native_vendor()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"vendor\": \"native\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_value_value_member()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"member\": \"value\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boot_trace_is_emitted()
    {
        await NavigateAndBoot();
        var hasBootTrace = _consoleMessages.Any(m => m.Contains("booted"));
        Assert.That(hasBootTrace, Is.True);
        AssertNoConsoleErrors();
    }
}
