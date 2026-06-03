using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.NumericTextBox;

/// <summary>
/// Proves FusionNumericTextBox property writes, reads, methods, events,
/// conditions, and gather through browser-visible behavior.
/// </summary>
[TestFixture]
public class WhenNumericValueEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NumericTextBox";

    // Generated component IDs are the DOM/plan join keys under test.
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NumericTextBoxModel";
    private const string AmountId = Scope + "__Amount";
    private const string TemperatureId = Scope + "__Temperature";

    private NumericTextBoxLocator Amount => new(Page, AmountId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

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
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_initial_value()
    {
        await NavigateAndBoot();
        await Expect(Amount.Input).ToBeVisibleAsync();

        await Expect(Amount.Input).Not.ToHaveValueAsync("", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var valueEcho = Page.Locator("#value-echo");
        await Expect(valueEcho).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var valueEchoText = await valueEcho.TextContentAsync();
        Assert.That(valueEchoText, Does.Contain("42"),
            "Value echo should contain 42 after dom-ready property read");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task increment_button_increases_quantity()
    {
        await NavigateAndBoot();

        await Page.Locator("#qty-inc-btn").ClickAsync();

        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task decrement_button_decreases_quantity()
    {
        await NavigateAndBoot();

        await Page.Locator("#qty-inc-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Page.Locator("#qty-dec-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("1", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task change_event_displays_new_value()
    {
        await NavigateAndBoot();

        // Syncfusion commits typed numeric values on blur, so FillAsync must be followed by Tab.
        var temperatureInput = Page.Locator($"#{TemperatureId}");
        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("37");
        await temperatureInput.PressAsync("Tab");

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

        var temperatureInput = Page.Locator($"#{TemperatureId}");
        await temperatureInput.ClickAsync();

        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("focused", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task blur_event_shows_blur_state()
    {
        await NavigateAndBoot();

        var temperatureInput = Page.Locator($"#{TemperatureId}");
        await temperatureInput.ClickAsync();
        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("focused", new() { Timeout = 5000 });

        await temperatureInput.PressAsync("Tab");
        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_shows_high_temp_warning_above_100()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#high-temp-warning")).ToBeHiddenAsync();

        var temperatureInput = Page.Locator($"#{TemperatureId}");
        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("120");
        await temperatureInput.PressAsync("Tab");

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

        var temperatureInput = Page.Locator($"#{TemperatureId}");
        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("120");
        await temperatureInput.PressAsync("Tab");
        await Expect(Page.Locator("#high-temp-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("100");
        await temperatureInput.PressAsync("Tab");

        await Expect(Page.Locator("#high-temp-warning"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_read_condition_shows_positive_indicator_above_zero()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#positive-indicator")).ToBeHiddenAsync();

        var temperatureInput = Page.Locator($"#{TemperatureId}");
        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("25");
        await temperatureInput.PressAsync("Tab");

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

        var temperatureInput = Page.Locator($"#{TemperatureId}");
        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("10");
        await temperatureInput.PressAsync("Tab");
        await Expect(Page.Locator("#positive-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("0");
        await temperatureInput.PressAsync("Tab");

        await Expect(Page.Locator("#positive-indicator"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_button_posts_component_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task increment_then_decrement_cycle_updates_echo_each_time()
    {
        await NavigateAndBoot();

        await Page.Locator("#qty-inc-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Page.Locator("#qty-inc-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("3", new() { Timeout = 5000 });

        await Page.Locator("#qty-inc-btn").ClickAsync();
        await Expect(Page.Locator("#qty-echo"))
            .ToHaveTextAsync("4", new() { Timeout = 5000 });

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
        await NavigateAndBoot();

        var temperatureInput = Page.Locator($"#{TemperatureId}");

        await Expect(Page.Locator("#positive-indicator")).ToBeHiddenAsync();

        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("50");
        await temperatureInput.PressAsync("Tab");

        await Expect(Page.Locator("#positive-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#positive-indicator"))
            .ToHaveTextAsync("positive", new() { Timeout = 3000 });

        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("-10");
        await temperatureInput.PressAsync("Tab");

        await Expect(Page.Locator("#positive-indicator"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        await temperatureInput.ClickAsync();
        await temperatureInput.FillAsync("1");
        await temperatureInput.PressAsync("Tab");

        await Expect(Page.Locator("#positive-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#positive-indicator"))
            .ToHaveTextAsync("positive", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
