using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InputMask;

/// <summary>
/// Exercises FusionInputMask property writes, value reads, changed-event conditions,
/// and component-read conditions for phone numbers, SSNs, and insurance IDs.
/// </summary>
/// <remarks>
/// InputMaskLocator fills and blurs so Syncfusion commits the masked value and raises <c>change</c>.
/// </remarks>
[TestFixture]
public class WhenMaskedInputEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/InputMask";

    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_InputMaskModel";
    private const string PhoneNumberId = GeneratedTypeScope + "__PhoneNumber";

    private InputMaskLocator PhoneNumber => new(Page, PhoneNumberId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionInputMask — Alis.Reactive Sandbox");
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
    public async Task domready_sets_initial_phone_value()
    {
        await NavigateAndBoot();
        var wrapper = Page.Locator($"#{PhoneNumberId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // Set-prop writes Syncfusion ej2.value; the visible input proves it applied.
        var inputValue = await PhoneNumber.Input.InputValueAsync();
        Assert.That(inputValue, Is.Not.Null.And.Not.Empty,
            $"Expected FusionInputMask input to have a value but got '{inputValue}'");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_displays_new_value()
    {
        await NavigateAndBoot();

        await PhoneNumber.FillAndBlur("9876543210");

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_empty()
    {
        await NavigateAndBoot();

        await PhoneNumber.FillAndBlur("9876543210");

        // When(args, x => x.Value).NotEmpty() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("phone entered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_empty()
    {
        await NavigateAndBoot();

        await PhoneNumber.FillAndBlur("9876543210");

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("phone on file", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        // DomReady seeds a value, so clear before checking the empty branch.
        await PhoneNumber.Clear();
        await PhoneNumber.Blur();

        await Page.Locator("#check-phone-btn").ClickAsync();

        var warning = Page.Locator("#phone-warning");
        await Expect(warning).ToHaveTextAsync("phone number is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        await PhoneNumber.FillAndBlur("5559876543");

        await Page.Locator("#check-phone-btn").ClickAsync();

        var warning = Page.Locator("#phone-warning");
        await Expect(warning).ToHaveTextAsync("phone number set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_mask_value_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        await PhoneNumber.FillAndBlur("5551234567");
        await Expect(argsCondition).ToHaveTextAsync("phone entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("phone on file", new() { Timeout = 3000 });

        await PhoneNumber.FillAndBlur("5559876543");
        await Expect(argsCondition).ToHaveTextAsync("phone entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_mask_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var checkPhoneButton = Page.Locator("#check-phone-btn");
        var warning = Page.Locator("#phone-warning");

        await PhoneNumber.Clear();
        await PhoneNumber.Blur();
        await checkPhoneButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("phone number is required", new() { Timeout = 3000 });

        await PhoneNumber.FillAndBlur("5551234567");
        await checkPhoneButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("phone number set", new() { Timeout = 3000 });

        await PhoneNumber.Clear();
        await PhoneNumber.Blur();
        await checkPhoneButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("phone number is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
