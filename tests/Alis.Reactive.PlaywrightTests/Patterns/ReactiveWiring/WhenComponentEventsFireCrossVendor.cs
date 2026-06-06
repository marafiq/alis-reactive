namespace Alis.Reactive.PlaywrightTests.Patterns.ReactiveWiring;

[TestFixture]
public class WhenComponentEventsFireCrossVendor : PlaywrightTestBase
{
    private const string PlaygroundSyntaxModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_PlaygroundSyntaxModel";

    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Patterns/PlaygroundSyntax");
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task fusion_numeric_change_updates_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#amount-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        // Syncfusion NumericTextBox renders duplicate inputs with this generated ID; the first is editable.
        var numericInput = Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("42");
        await numericInput.PressAsync("Tab");

        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task native_dropdown_change_updates_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#status-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_property_ids_use_underscores_not_dots()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Address_City");
        await Expect(citySelect).ToBeVisibleAsync();

        // Syncfusion NumericTextBox renders duplicate inputs with this generated ID; the first is editable.
        var postalInput = Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Address_PostalCode").First;
        await Expect(postalInput).ToBeVisibleAsync();

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(Page.Locator("#city-echo")).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");
        await Expect(Page.Locator("#postal-echo")).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reset_all_button_clears_both_vendors()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        var numericInput = Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("999");
        await numericInput.PressAsync("Tab");
        await Expect(Page.Locator("#amount-echo")).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();

        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        await Expect(Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status")).ToHaveValueAsync("");

        // Syncfusion may display the zeroed numeric value as either "0" or "0.00".
        await Expect(numericInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task non_reactive_control_does_not_fire_change()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Category").SelectOptionAsync(new SelectOptionValue { Value = "A" });

        // Non-reactive control must remain inert even after the browser event settles.
        await Expect(statusEcho).ToHaveTextAsync("\u2014", new() { Timeout = 1000 });
        await Expect(amountEcho).ToHaveTextAsync("\u2014", new() { Timeout = 1000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reset_then_interact_proves_components_still_reactive_after_reset()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(statusEcho).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });
        await Expect(Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status")).ToHaveValueAsync("");
        await Expect(Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Amount").First).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status").SelectOptionAsync(new SelectOptionValue { Value = "inactive" });
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        var numericInput = Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("77");
        await numericInput.PressAsync("Tab");
        await Expect(amountEcho).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task each_reactive_pipeline_only_updates_its_own_echo()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        var numericInput = Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Amount").First;
        await numericInput.ClickAsync();
        await numericInput.FillAsync("500");
        await numericInput.PressAsync("Tab");

        await Expect(amountEcho).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });
        await Expect(statusEcho).ToHaveTextAsync("\u2014");

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });
        await Expect(amountEcho).ToHaveTextAsync("Amount changed");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_reactive_change_does_not_affect_top_level_echoes()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");
        var cityEcho = Page.Locator("#city-echo");

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Address_City").SelectOptionAsync(new SelectOptionValue { Value = "seattle" });

        await Expect(cityEcho).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        await Expect(statusEcho).ToHaveTextAsync("\u2014");
        await Expect(amountEcho).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reset_all_does_not_affect_nested_echoes()
    {
        await NavigateAndBoot();

        var cityEcho = Page.Locator("#city-echo");
        var postalEcho = Page.Locator("#postal-echo");

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Address_City").SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(cityEcho).ToHaveTextAsync("City changed", new() { Timeout = 3000 });

        var postalInput = Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Address_PostalCode").First;
        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");
        await Expect(postalEcho).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        await Expect(cityEcho).ToHaveTextAsync("City changed");
        await Expect(postalEcho).ToHaveTextAsync("PostalCode changed");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_element_is_present_and_non_empty()
    {
        await NavigateAndBoot();
        var planEl = Page.Locator("#plan-json");
        await Expect(planEl).ToBeAttachedAsync(new() { Timeout = 5000 });
        var text = await planEl.TextContentAsync();
        Assert.That(text, Is.Not.Null.And.Not.Empty, "Plan JSON must be present for runtime boot");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fusion_numeric_fires_on_every_value_change()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#amount-echo");
        var numericInput = Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Amount").First;

        await numericInput.ClickAsync();
        await numericInput.FillAsync("100");
        await numericInput.PressAsync("Tab");
        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(Page.Locator("#status-echo")).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        await numericInput.ClickAsync();
        await numericInput.FillAsync("200");
        await numericInput.PressAsync("Tab");
        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task native_dropdown_fires_on_every_selection_change()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#status-echo");

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();
        await Expect(echo).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });

        await Page.Locator($"#{PlaygroundSyntaxModelIdPrefix}__Status").SelectOptionAsync(new SelectOptionValue { Value = "pending" });
        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
