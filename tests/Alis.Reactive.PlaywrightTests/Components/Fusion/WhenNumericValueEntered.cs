using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionNumericTextBox API end-to-end in the browser:
/// property writes, method calls, property reads, events, conditions, and gather.
///
/// Page under test: /Sandbox/Components/NumericTextBox
///
/// FusionNumericTextBox renders an inner input element inside the wrapper div.
/// The wrapper element gets the IdGenerator-based ID. Tests use
/// NumericTextBoxLocator to interact via real browser gestures.
/// </summary>
[TestFixture]
public class WhenNumericValueEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NumericTextBox";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NumericTextBoxModel";
    private const string AmountId = Scope + "__Amount";
    private const string TemperatureId = Scope + "__Temperature";
    private const string QuantityId = Scope + "__Quantity";

    private NumericTextBoxLocator Amount => new(Page, AmountId);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NumericTextBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write ──

    [Test]
    public async Task domready_sets_initial_value()
    {
        await NavigateAndBoot();
        // SF NumericTextBox renders an input with the IdGenerator-based ID
        await Expect(Amount.Input).ToBeVisibleAsync();

        // Wait for the value to be set by dom-ready — verify via the visible input
        await Expect(Amount.Input).Not.ToHaveValueAsync("", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        // The value-echo should show "42" after dom-ready reads comp.Value()
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var text = await echo.TextContentAsync();
        Assert.That(text, Does.Contain("42"),
            "Value echo should contain 42 after dom-ready property read");
        AssertNoConsoleErrors();
    }

    // ── Section 3: Method Calls (Increment/Decrement) ──

    [Test]
    public async Task increment_button_increases_quantity()
    {
        await NavigateAndBoot();

        // Quantity starts at 1, click increment
        await Page.Locator("#qty-inc-btn").ClickAsync();

        // Wait for the change event to update the echo
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task decrement_button_decreases_quantity()
    {
        await NavigateAndBoot();

        // Quantity starts at 1, increment first then decrement to get back to 1
        await Page.Locator("#qty-inc-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Page.Locator("#qty-dec-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Events ──

    [Test]
    public async Task change_event_displays_new_value()
    {
        await NavigateAndBoot();

        // SF NumericTextBox renders the <input> directly with the IdGenerator-based ID
        // after client-side init. Use FillAsync to set the value, then Tab to trigger
        // the SF change event (which reads the committed value).
        var tempInput = Page.Locator($"#{TemperatureId}");
        await tempInput.ClickAsync();
        await tempInput.FillAsync("37");
        await tempInput.PressAsync("Tab");

        // SF change event payload contains the committed numeric value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var changeText = await Page.Locator("#change-value").TextContentAsync();
        Assert.That(changeText, Does.Contain("37"),
            $"Change value should contain 37 but was '{changeText}'");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focus_event_shows_focus_state()
    {
        await NavigateAndBoot();

        // Click into Temperature to trigger focus
        var tempInput = Page.Locator($"#{TemperatureId}");
        await tempInput.ClickAsync();

        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("focused", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task blur_event_shows_blur_state()
    {
        await NavigateAndBoot();

        // Click into Temperature, then Tab out to trigger blur
        var tempInput = Page.Locator($"#{TemperatureId}");
        await tempInput.ClickAsync();
        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("focused", new() { Timeout = 5000 });

        await tempInput.PressAsync("Tab");
        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 5: Conditions ──

    [Test]
    public async Task event_args_condition_shows_high_temp_warning_above_100()
    {
        await NavigateAndBoot();

        // High-temp warning starts hidden
        await Expect(Page.Locator("#high-temp-warning")).ToBeHiddenAsync();

        // Type a value > 100 in Temperature
        var tempInput = Page.Locator($"#{TemperatureId}");
        await tempInput.ClickAsync();
        await tempInput.FillAsync("120");
        await tempInput.PressAsync("Tab");

        // Warning should appear with text "high"
        await Expect(Page.Locator("#high-temp-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#high-temp-warning"))
            .ToHaveTextAsync("high", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_hides_high_temp_warning_at_100_or_below()
    {
        await NavigateAndBoot();

        // First make warning visible
        var tempInput = Page.Locator($"#{TemperatureId}");
        await tempInput.ClickAsync();
        await tempInput.FillAsync("120");
        await tempInput.PressAsync("Tab");
        await Expect(Page.Locator("#high-temp-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Now set to 100 (not > 100)
        await tempInput.ClickAsync();
        await tempInput.FillAsync("100");
        await tempInput.PressAsync("Tab");

        await Expect(Page.Locator("#high-temp-warning"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_read_condition_shows_positive_indicator_above_zero()
    {
        await NavigateAndBoot();

        // Positive indicator starts hidden
        await Expect(Page.Locator("#positive-indicator")).ToBeHiddenAsync();

        // Type a positive value in Temperature
        var tempInput = Page.Locator($"#{TemperatureId}");
        await tempInput.ClickAsync();
        await tempInput.FillAsync("25");
        await tempInput.PressAsync("Tab");

        // Indicator should appear with text "positive"
        await Expect(Page.Locator("#positive-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#positive-indicator"))
            .ToHaveTextAsync("positive", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_read_condition_hides_positive_indicator_at_zero_or_negative()
    {
        await NavigateAndBoot();

        // First make it visible
        var tempInput = Page.Locator($"#{TemperatureId}");
        await tempInput.ClickAsync();
        await tempInput.FillAsync("10");
        await tempInput.PressAsync("Tab");
        await Expect(Page.Locator("#positive-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Now set to 0
        await tempInput.ClickAsync();
        await tempInput.FillAsync("0");
        await tempInput.PressAsync("Tab");

        await Expect(Page.Locator("#positive-indicator"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 6: Gather ──

    [Test]
    public async Task gather_button_posts_component_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-step state-cycle scenarios ──

    [Test]
    public async Task increment_then_decrement_cycle_updates_echo_each_time()
    {
        // Proves method calls fire change events and DOM updates on every click
        await NavigateAndBoot();

        // Quantity starts at 1 — click increment 3 times: 1→2→3→4
        await Page.Locator("#qty-inc-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Page.Locator("#qty-inc-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("3", new() { Timeout = 5000 });

        await Page.Locator("#qty-inc-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("4", new() { Timeout = 5000 });

        // Click decrement 2 times: 4→3→2
        await Page.Locator("#qty-dec-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("3", new() { Timeout = 5000 });

        await Page.Locator("#qty-dec-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_temperature_toggles_positive_indicator_across_zero_boundary()
    {
        // Proves When(comp.Value()).Gt(0m) condition re-evaluates correctly
        // across positive → negative → positive transitions
        await NavigateAndBoot();

        var tempInput = Page.Locator($"#{TemperatureId}");

        // Positive indicator starts hidden
        await Expect(Page.Locator("#positive-indicator")).ToBeHiddenAsync();

        // Set temp to 50 → positive shows
        await tempInput.ClickAsync();
        await tempInput.FillAsync("50");
        await tempInput.PressAsync("Tab");

        await Expect(Page.Locator("#positive-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#positive-indicator"))
            .ToHaveTextAsync("positive", new() { Timeout = 3000 });

        // Set temp to -10 → positive hides
        await tempInput.ClickAsync();
        await tempInput.FillAsync("-10");
        await tempInput.PressAsync("Tab");

        await Expect(Page.Locator("#positive-indicator"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        // Set temp to 1 → positive shows again
        await tempInput.ClickAsync();
        await tempInput.FillAsync("1");
        await tempInput.PressAsync("Tab");

        await Expect(Page.Locator("#positive-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#positive-indicator"))
            .ToHaveTextAsync("positive", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
