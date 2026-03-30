using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionInputMask API end-to-end in the browser:
/// property writes, property reads, events with typed conditions,
/// and component-read conditions.
///
/// Page under test: /Sandbox/Components/InputMask
///
/// FusionInputMask renders an input element inside a wrapper span.
/// The wrapper element gets the IdGenerator-based ID. Tests use
/// InputMaskLocator to interact via real browser gestures (typing into
/// the masked input) rather than ej2 instance manipulation.
///
/// Senior living domain: phone numbers, SSN, insurance IDs.
/// </summary>
[TestFixture]
public class WhenMaskedInputEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/InputMask";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_InputMaskModel";
    private const string PhoneNumberId = Scope + "__PhoneNumber";

    private InputMaskLocator PhoneNumber => new(Page, PhoneNumberId);

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
        await Expect(Page).ToHaveTitleAsync("FusionInputMask — Alis.Reactive Sandbox");
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
    public async Task domready_sets_initial_phone_value()
    {
        await NavigateAndBoot();
        var wrapper = Page.Locator($"#{PhoneNumberId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // The framework sets ej2.value = "(555) 123-4567" via set-prop.
        // Verify the value was applied by checking the visible input element's value.
        var inputValue = await PhoneNumber.Input.InputValueAsync();
        Assert.That(inputValue, Is.Not.Null.And.Not.Empty,
            $"Expected FusionInputMask input to have a value but got '{inputValue}'");

        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Events — Changed with typed condition ──

    [Test]
    public async Task changed_event_displays_new_value()
    {
        await NavigateAndBoot();

        // Type phone number and blur — triggers SF change event
        await PhoneNumber.FillAndBlur("9876543210");

        // SF change event payload contains the new value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_not_empty()
    {
        await NavigateAndBoot();

        // Type phone number and blur — triggers SF change event
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

        // Type phone number and blur — triggers SF change event
        await PhoneNumber.FillAndBlur("9876543210");

        // Indicator should appear with text "phone on file"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("phone on file", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component-Read Condition ──

    [Test]
    public async Task component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        // Clear the DomReady-set value first
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

        // Type a phone number and blur
        await PhoneNumber.FillAndBlur("5559876543");

        // Click check button
        await Page.Locator("#check-phone-btn").ClickAsync();

        var warning = Page.Locator("#phone-warning");
        await Expect(warning).ToHaveTextAsync("phone number set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle scenarios ──

    [Test]
    public async Task changing_mask_value_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        // Cycle 1: type a phone number — condition evaluates "phone entered", indicator shows
        await PhoneNumber.FillAndBlur("5551234567");
        await Expect(argsCondition).ToHaveTextAsync("phone entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("phone on file", new() { Timeout = 3000 });

        // Cycle 2: change to a different number — condition still fires and re-evaluates
        await PhoneNumber.FillAndBlur("5559876543");
        // args-condition should still say "phone entered" (value is still not empty)
        await Expect(argsCondition).ToHaveTextAsync("phone entered", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Verify the change-value updated
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_then_refilling_mask_updates_condition_both_ways()
    {
        await NavigateAndBoot();

        var btn = Page.Locator("#check-phone-btn");
        var warning = Page.Locator("#phone-warning");

        // Step 1: clear the DomReady-set value, then check — "phone number is required"
        await PhoneNumber.Clear();
        await PhoneNumber.Blur();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("phone number is required", new() { Timeout = 3000 });

        // Step 2: type a phone number — click check — "phone number set"
        await PhoneNumber.FillAndBlur("5551234567");
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("phone number set", new() { Timeout = 3000 });

        // Step 3: clear the phone number — click check — "phone number is required" again
        await PhoneNumber.Clear();
        await PhoneNumber.Blur();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("phone number is required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
