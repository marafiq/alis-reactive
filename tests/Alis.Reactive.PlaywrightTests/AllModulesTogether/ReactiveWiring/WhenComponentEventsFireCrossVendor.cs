namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.ReactiveWiring;

[TestFixture]
public class WhenComponentEventsFireCrossVendor : PlaywrightTestBase
{
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_PlaygroundSyntaxModel";

    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/AllModulesTogether/PlaygroundSyntax");
        await WaitForPageReady(5000);
    }

    // ── Scenario: Fusion component change fires reactive echo ──

    [Test]
    public async Task fusion_numeric_change_updates_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#amount-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        // SF NumericTextBox renders TWO inputs with the same ID — use .First for the visible one
        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("42");
        await numericInput.PressAsync("Tab");

        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Native component change fires reactive echo ──

    [Test]
    public async Task native_dropdown_change_updates_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#status-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Nested property IDs use underscores not dots ──

    [Test]
    public async Task nested_property_ids_use_underscores_not_dots()
    {
        await NavigateAndBoot();

        // Native nested: Address_City (not Address.City)
        var citySelect = Page.Locator($"#{S}__Address_City");
        await Expect(citySelect).ToBeVisibleAsync();

        // Fusion nested: Address_PostalCode — .First because SF renders two inputs
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        await Expect(postalInput).ToBeVisibleAsync();

        // Verify reactive wiring works on nested elements: change City, see echo
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(Page.Locator("#city-echo")).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        // Verify reactive wiring works on nested Fusion: change PostalCode, see echo
        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");
        await Expect(Page.Locator("#postal-echo")).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Reset All button clears both vendors via document event ──

    [Test]
    public async Task reset_all_button_clears_both_vendors()
    {
        await NavigateAndBoot();

        // Set values in both vendors first
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("999");
        await numericInput.PressAsync("Tab");
        await Expect(Page.Locator("#amount-echo")).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        // Reset All — one document event resets both vendors
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();

        // Echo confirms the reset-all pipeline executed
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        // Native dropdown cleared to empty
        await Expect(Page.Locator($"#{S}__Status")).ToHaveValueAsync("");

        // Fusion numeric zeroed — SF may format as "0" or "0.00"
        await Expect(numericInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Non-reactive control stays inert ──

    [Test]
    public async Task non_reactive_control_does_not_fire_change()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        // Category dropdown has NO .Reactive() — selecting must not affect any echo
        await Page.Locator($"#{S}__Category").SelectOptionAsync(new SelectOptionValue { Value = "A" });

        // A non-reactive control must remain inert even after the browser event settles.
        await Expect(statusEcho).ToHaveTextAsync("\u2014", new() { Timeout = 1000 });
        await Expect(amountEcho).ToHaveTextAsync("\u2014", new() { Timeout = 1000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Components remain reactive after a cross-vendor reset ──

    [Test]
    public async Task reset_then_interact_proves_components_still_reactive_after_reset()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        // Step 1: Interact — native dropdown fires its reactive pipeline
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        // Step 2: Reset All — cross-vendor reset clears values and overwrites echo
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(statusEcho).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });
        await Expect(Page.Locator($"#{S}__Status")).ToHaveValueAsync("");
        await Expect(Page.Locator($"#{S}__Amount").First).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        // Step 3: Interact again — native dropdown must still be reactive after reset
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "inactive" });
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        // Step 4: Fusion component must also still be reactive after reset
        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("77");
        await numericInput.PressAsync("Tab");
        await Expect(amountEcho).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Each reactive pipeline is isolated ──

    [Test]
    public async Task each_reactive_pipeline_only_updates_its_own_echo()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        // Change Amount — only amount-echo updates, status-echo stays at em-dash
        var numericInput = Page.Locator($"#{S}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("500");
        await numericInput.PressAsync("Tab");

        await Expect(amountEcho).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });
        await Expect(statusEcho).ToHaveTextAsync("\u2014");

        // Change Status — only status-echo updates, amount-echo stays at "Amount changed"
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });
        await Expect(amountEcho).ToHaveTextAsync("Amount changed");

        AssertNoConsoleErrors();
    }

    // ── Scenario: Nested reactive pipelines do not affect top-level echoes ──

    [Test]
    public async Task nested_reactive_change_does_not_affect_top_level_echoes()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");
        var cityEcho = Page.Locator("#city-echo");

        // Change nested City dropdown
        await Page.Locator($"#{S}__Address_City").SelectOptionAsync(new SelectOptionValue { Value = "seattle" });

        // City echo updates
        await Expect(cityEcho).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        // Top-level echoes remain untouched
        await Expect(statusEcho).ToHaveTextAsync("\u2014");
        await Expect(amountEcho).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrors();
    }

    // ── Scenario: Reset pipeline only targets status-echo, not nested echoes ──

    [Test]
    public async Task reset_all_does_not_affect_nested_echoes()
    {
        await NavigateAndBoot();

        var cityEcho = Page.Locator("#city-echo");
        var postalEcho = Page.Locator("#postal-echo");

        // Change City and PostalCode first to set their echoes
        await Page.Locator($"#{S}__Address_City").SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(cityEcho).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");
        await Expect(postalEcho).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });

        // Reset All — should only affect status-echo, Amount, and Status
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        // Nested echoes must remain from their previous reactive pipeline fire
        await Expect(cityEcho).ToHaveTextAsync("City changed");
        await Expect(postalEcho).ToHaveTextAsync("PostalCode changed");

        AssertNoConsoleErrors();
    }

    // ── Scenario: Fusion numeric fires on every new value ──

    [Test]
    public async Task fusion_numeric_fires_on_every_value_change()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#amount-echo");
        var numericInput = Page.Locator($"#{S}__Amount").First;

        // First value
        await numericInput.ClickAsync();
        await numericInput.FillAsync("100");
        await numericInput.PressAsync("Tab");
        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        // Reset echo via Reset All so we can detect the second fire
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        // Second value — must fire again
        await numericInput.ClickAsync();
        await numericInput.FillAsync("200");
        await numericInput.PressAsync("Tab");
        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Native dropdown fires on every selection change ──

    [Test]
    public async Task native_dropdown_fires_on_every_selection_change()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#status-echo");

        // First selection
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        // Reset echo so we can detect the second fire
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(echo).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        // Second selection — must fire again
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "pending" });
        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
