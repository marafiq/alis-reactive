namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ColorPicker;

/// <summary>
/// Exercises FusionColorPicker SetValue, Value reads, component-read conditions,
/// method calls, and emitted Reactive Plan metadata.
/// </summary>
[TestFixture]
public class WhenColorPicked : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ColorPicker";

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#set-value-result");
    }

    [Test]
    public async Task domready_sets_theme_color()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#set-value-result"))
            .ToHaveTextAsync("SetValue applied", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var valueEcho = await Page.Locator("#value-echo").TextContentAsync();
        Assert.That(valueEcho, Is.Not.EqualTo("\u2014"),
            "Value echo should show a color value after DomReady");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggle_button_calls_toggle_method()
    {
        await NavigateAndBoot();

        var toggleBtn = Page.Locator("#toggle-btn");
        await ClickWhenStable(toggleBtn);

        await Expect(Page.Locator("#method-result"))
            .ToHaveTextAsync("toggle called", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task check_accent_button_evaluates_component_value()
    {
        await NavigateAndBoot();

        var checkBtn = Page.Locator("#check-accent-btn");
        await ClickWhenStable(checkBtn);

        await Expect(Page.Locator("#component-read-result"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        var status = await Page.Locator("#component-read-result").TextContentAsync();
        Assert.That(status, Does.Contain("accent color"),
            "Should report whether accent color is set or not");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_colorpicker_behaviors()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions for SetValue");
        AssertNoConsoleErrors();
    }
}
