namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeCheckList API end-to-end in the browser:
/// text + description variation, text-only variation,
/// form submission (JSON POST + FluentValidation), component-read conditions,
/// model binding proof (pre-selected checkboxes).
///
/// Page under test: /Sandbox/NativeCheckList
/// </summary>
[TestFixture]
public class WhenUsingNativeCheckList : PlaywrightTestBase
{
    private const string Path = "/Sandbox/NativeCheckList";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeCheckListModel__";

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
        await Expect(Page).ToHaveTitleAsync("NativeCheckList — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Text + description checkbox list ──

    [Test]
    public async Task clicking_checkbox_echoes_comma_separated_value()
    {
        await NavigateAndBoot();

        // Click Shellfish (index 1) — Peanuts + Dairy already pre-checked
        await Page.Locator($"#{Scope}Allergies_c1").ClickAsync();

        var echo = Page.Locator("#allergy-echo");
        await Expect(echo).ToHaveTextAsync("Peanuts,Shellfish,Dairy", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unchecking_updates_comma_separated_value()
    {
        await NavigateAndBoot();

        // Uncheck Dairy (index 2) — was pre-checked
        await Page.Locator($"#{Scope}Allergies_c2").ClickAsync();

        var echo = Page.Locator("#allergy-echo");
        await Expect(echo).ToHaveTextAsync("Peanuts", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_input_syncs_on_checkbox_change()
    {
        await NavigateAndBoot();

        // Click Gluten (index 3)
        await Page.Locator($"#{Scope}Allergies_c3").ClickAsync();

        var hidden = Page.Locator($"input[type='hidden']#{Scope}Allergies");
        await Expect(hidden).ToHaveValueAsync("Peanuts,Dairy,Gluten", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 2: Text-only checkbox list ──

    [Test]
    public async Task text_only_checkboxes_render()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator($"input[type='checkbox'][name='Amenities']")).ToHaveCountAsync(5);
        AssertNoConsoleErrors();
    }

    // ── Section 3: Form — validation blocks when empty ──

    [Test]
    public async Task form_validation_blocks_empty_submit()
    {
        await NavigateAndBoot();

        await Page.Locator("#submit-btn").ClickAsync();

        var nameError = Page.Locator("[data-valmsg-for='ResidentName']");
        await Expect(nameError).ToContainTextAsync("required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_submit_succeeds_with_valid_data()
    {
        await NavigateAndBoot();

        var nameInput = Page.Locator($"#{Scope}ResidentName");
        await nameInput.FillAsync("Margaret Thompson");

        // Check a dietary need
        await Page.Locator($"#{Scope}DietaryNeeds_c0").ClickAsync();

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

        // Allergies are pre-selected (Peanuts,Dairy)
        await Page.Locator("#check-allergy-btn").ClickAsync();

        var status = Page.Locator("#allergy-confirmation");
        await Expect(status).ToHaveTextAsync("allergies recorded", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        // Uncheck both pre-selected allergies
        await Page.Locator($"#{Scope}Allergies_c0").ClickAsync();
        await Page.Locator($"#{Scope}Allergies_c2").ClickAsync();

        await Page.Locator("#check-allergy-btn").ClickAsync();

        var status = Page.Locator("#allergy-confirmation");
        await Expect(status).ToHaveTextAsync("no allergies selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 5: Model binding proof ──

    [Test]
    public async Task pre_selected_checkboxes_are_checked_on_load()
    {
        await NavigateAndBoot();

        // Peanuts (index 0) should be checked
        await Expect(Page.Locator($"#{Scope}Allergies_c0")).ToBeCheckedAsync();
        // Shellfish (index 1) should NOT be checked
        await Expect(Page.Locator($"#{Scope}Allergies_c1")).Not.ToBeCheckedAsync();
        // Dairy (index 2) should be checked
        await Expect(Page.Locator($"#{Scope}Allergies_c2")).ToBeCheckedAsync();
        // Gluten (index 3) should NOT be checked
        await Expect(Page.Locator($"#{Scope}Allergies_c3")).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_input_has_pre_selected_value()
    {
        await NavigateAndBoot();

        var hidden = Page.Locator($"input[type='hidden']#{Scope}Allergies");
        await Expect(hidden).ToHaveValueAsync("Peanuts,Dairy", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── DOM structure ──

    [Test]
    public async Task hidden_inputs_render_for_all_groups()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator($"input[type='hidden']#{Scope}Allergies")).ToHaveCountAsync(1);
        await Expect(Page.Locator($"input[type='hidden']#{Scope}Amenities")).ToHaveCountAsync(1);
        await Expect(Page.Locator($"input[type='hidden']#{Scope}DietaryNeeds")).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task allergy_list_has_four_checkboxes()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator($"input[type='checkbox'][name='Allergies']")).ToHaveCountAsync(4);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task amenity_list_has_five_checkboxes()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator($"input[type='checkbox'][name='Amenities']")).ToHaveCountAsync(5);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dietary_list_has_three_checkboxes()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator($"input[type='checkbox'][name='DietaryNeeds']")).ToHaveCountAsync(3);
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

    // ── Multi-step toggle cycle ──

    [Test]
    public async Task toggling_multiple_checkboxes_updates_correctly()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#allergy-echo");

        // Check Shellfish
        await Page.Locator($"#{Scope}Allergies_c1").ClickAsync();
        await Expect(echo).ToHaveTextAsync("Peanuts,Shellfish,Dairy", new() { Timeout = 3000 });

        // Uncheck Peanuts
        await Page.Locator($"#{Scope}Allergies_c0").ClickAsync();
        await Expect(echo).ToHaveTextAsync("Shellfish,Dairy", new() { Timeout = 3000 });

        // Check Gluten
        await Page.Locator($"#{Scope}Allergies_c3").ClickAsync();
        await Expect(echo).ToHaveTextAsync("Shellfish,Dairy,Gluten", new() { Timeout = 3000 });

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
