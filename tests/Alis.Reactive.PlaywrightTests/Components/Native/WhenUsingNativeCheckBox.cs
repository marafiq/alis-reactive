namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeCheckBox API end-to-end in the browser:
/// property writes (SetChecked), reactive events (Changed), and conditions.
///
/// Page under test: /Sandbox/CheckBox
///
/// BUG-001 REPRODUCTION:
/// SetChecked(false) passes the string "false" as val with coerce: "boolean".
/// The runtime coerces "false" to boolean false before assigning to el.checked.
/// This fixes the original bug where raw string assignment caused truthy coercion.
/// </summary>
[TestFixture]
public class WhenUsingNativeCheckBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CheckBox";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    // ── Page loads ──

    [Test]
    public async Task Page_loads_with_no_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeCheckBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"prop\""),
            "Plan must contain structured prop field");
        AssertNoConsoleErrors();
    }

    // ── Section 1: SetChecked(false) — BUG REPRODUCTION ──

    [Test]
    public async Task Checkbox_starts_checked()
    {
        await NavigateAndBoot();
        var cb = Page.Locator("#cb-test-uncheck");
        await Expect(cb).ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task SetChecked_false_unchecks_the_checkbox()
    {
        // BUG-001: This test reproduces the bug.
        // SetChecked(false) emits { prop: "checked", coerce: "boolean" } with value "false".
        // The runtime coerces "false" to boolean false via coerce().
        // Expected: checkbox unchecked after clicking button.

        await NavigateAndBoot();

        // Verify the checkbox starts checked
        var cb = Page.Locator("#cb-test-uncheck");
        await Expect(cb).ToBeCheckedAsync();

        // Click the button that triggers SetChecked(false)
        await Page.Locator("#btn-uncheck").ClickAsync();

        // Assert: checkbox should now be unchecked
        await Expect(cb).Not.ToBeCheckedAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: SetChecked(true) — control case ──

    [Test]
    public async Task Checkbox_starts_unchecked()
    {
        await NavigateAndBoot();
        var cb = Page.Locator("#cb-test-check");
        await Expect(cb).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task SetChecked_true_checks_the_checkbox()
    {
        // SetChecked(true) emits { prop: "checked", coerce: "boolean" } with value "true".
        // The runtime coerces "true" to boolean true.

        await NavigateAndBoot();

        // Verify the checkbox starts unchecked
        var cb = Page.Locator("#cb-test-check");
        await Expect(cb).Not.ToBeCheckedAsync();

        // Click the button that triggers SetChecked(true)
        await Page.Locator("#btn-check").ClickAsync();

        // Assert: checkbox should now be checked
        await Expect(cb).ToBeCheckedAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Section 3: Reactive event — change toggles show/hide ──

    [Test]
    public async Task Change_event_shows_extras_when_checked()
    {
        await NavigateAndBoot();

        // Extras panel starts hidden
        await Expect(Page.Locator("#reactive-extras")).ToBeHiddenAsync();

        // Check the reactive checkbox
        await Page.Locator("#cb-reactive").CheckAsync();

        // Extras should appear and status should say "checked"
        await Expect(Page.Locator("#reactive-extras"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#reactive-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Change_event_hides_extras_when_unchecked()
    {
        await NavigateAndBoot();

        // Check then uncheck the reactive checkbox
        await Page.Locator("#cb-reactive").CheckAsync();
        await Expect(Page.Locator("#reactive-extras"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });

        await Page.Locator("#cb-reactive").UncheckAsync();

        // Extras should hide and status should say "unchecked"
        await Expect(Page.Locator("#reactive-extras"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#reactive-status"))
            .ToHaveTextAsync("unchecked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
