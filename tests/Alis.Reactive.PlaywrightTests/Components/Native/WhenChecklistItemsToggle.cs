namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeCheckList selection, form submission, component-read conditions,
/// and pre-selected checkbox model binding in the browser.
/// </summary>
[TestFixture]
public class WhenChecklistItemsToggle : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeCheckList";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeCheckListModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeCheckList — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_checkbox_echoes_comma_separated_value()
    {
        await NavigateAndBoot();

        // Peanuts and Dairy are pre-selected before Shellfish is clicked.
        await Page.Locator($"#{ModelIdPrefix}Allergies_c1").ClickAsync();

        var echo = Page.Locator("#allergy-echo");
        await Expect(echo).ToHaveTextAsync("Peanuts,Shellfish,Dairy", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unchecking_updates_comma_separated_value()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}Allergies_c2").ClickAsync();

        var echo = Page.Locator("#allergy-echo");
        await Expect(echo).ToHaveTextAsync("Peanuts", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_input_syncs_on_checkbox_change()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}Allergies_c3").ClickAsync();

        // Hidden input is inside the container div, which carries the scoped ID.
        var hidden = Page.Locator($"#{ModelIdPrefix}Allergies input[type='hidden']");
        await Expect(hidden).ToHaveValueAsync("Peanuts,Dairy,Gluten", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task text_only_checkboxes_render()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator($"input[type='checkbox'][name='Amenities']")).ToHaveCountAsync(5);
        AssertNoConsoleErrors();
    }

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

        var nameInput = Page.Locator($"#{ModelIdPrefix}ResidentName");
        await nameInput.FillAsync("Margaret Thompson");

        await Page.Locator($"#{ModelIdPrefix}DietaryNeeds_c0").ClickAsync();

        await Page.Locator("#submit-btn").ClickAsync();

        var result = Page.Locator("#result");
        await Expect(result).ToHaveTextAsync("Preferences saved", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_submit_sends_array_values_in_json_body()
    {
        await NavigateAndBoot();

        var nameInput = Page.Locator($"#{ModelIdPrefix}ResidentName");
        await nameInput.FillAsync("Margaret Thompson");

        await Page.Locator($"#{ModelIdPrefix}DietaryNeeds_c0").ClickAsync();

        // Intercept the POST because the array payload shape is the behavior under test.
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#submit-btn").ClickAsync(),
            "**/Sandbox/Components/NativeCheckList/Submit");

        var body = request.PostData ?? "";
        // Pre-selected Allergies must stay an array in JSON.
        Assert.That(body, Does.Contain("\"Allergies\""),
            $"Body must contain Allergies key but was '{body}'");
        Assert.That(body, Does.Contain("Peanuts"),
            $"Body must contain Peanuts but was '{body}'");
        Assert.That(body, Does.Contain("Dairy"),
            $"Body must contain Dairy but was '{body}'");
        // Newly selected DietaryNeeds must also stay an array in JSON.
        Assert.That(body, Does.Contain("\"DietaryNeeds\""),
            $"Body must contain DietaryNeeds key but was '{body}'");
        Assert.That(body, Does.Contain("LowSodium"),
            $"Body must contain LowSodium but was '{body}'");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_set()
    {
        await NavigateAndBoot();

        // Allergies are pre-selected.
        await Page.Locator("#check-allergy-btn").ClickAsync();

        var status = Page.Locator("#allergy-confirmation");
        await Expect(status).ToHaveTextAsync("allergies recorded", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{ModelIdPrefix}Allergies_c0").ClickAsync();
        await Page.Locator($"#{ModelIdPrefix}Allergies_c2").ClickAsync();

        await Page.Locator("#check-allergy-btn").ClickAsync();

        var status = Page.Locator("#allergy-confirmation");
        await Expect(status).ToHaveTextAsync("no allergies selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pre_selected_checkboxes_are_checked_on_load()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator($"#{ModelIdPrefix}Allergies_c0")).ToBeCheckedAsync();
        await Expect(Page.Locator($"#{ModelIdPrefix}Allergies_c1")).Not.ToBeCheckedAsync();
        await Expect(Page.Locator($"#{ModelIdPrefix}Allergies_c2")).ToBeCheckedAsync();
        await Expect(Page.Locator($"#{ModelIdPrefix}Allergies_c3")).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_input_has_pre_selected_value()
    {
        await NavigateAndBoot();

        var hidden = Page.Locator($"#{ModelIdPrefix}Allergies input[type='hidden']");
        await Expect(hidden).ToHaveValueAsync("Peanuts,Dairy", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_inputs_render_for_all_groups()
    {
        await NavigateAndBoot();

        // Hidden inputs live inside container divs, which carry the scoped IDs.
        await Expect(Page.Locator($"#{ModelIdPrefix}Allergies input[type='hidden']")).ToHaveCountAsync(1);
        await Expect(Page.Locator($"#{ModelIdPrefix}Amenities input[type='hidden']")).ToHaveCountAsync(1);
        await Expect(Page.Locator($"#{ModelIdPrefix}DietaryNeeds input[type='hidden']")).ToHaveCountAsync(1);
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
    public async Task toggling_multiple_checkboxes_updates_correctly()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#allergy-echo");

        await Page.Locator($"#{ModelIdPrefix}Allergies_c1").ClickAsync();
        await Expect(echo).ToHaveTextAsync("Peanuts,Shellfish,Dairy", new() { Timeout = 3000 });

        await Page.Locator($"#{ModelIdPrefix}Allergies_c0").ClickAsync();
        await Expect(echo).ToHaveTextAsync("Shellfish,Dairy", new() { Timeout = 3000 });

        await Page.Locator($"#{ModelIdPrefix}Allergies_c3").ClickAsync();
        await Expect(echo).ToHaveTextAsync("Shellfish,Dairy,Gluten", new() { Timeout = 3000 });

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
