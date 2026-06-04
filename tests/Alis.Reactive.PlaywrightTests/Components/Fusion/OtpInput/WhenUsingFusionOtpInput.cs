using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.OtpInput;

[TestFixture]
public class WhenUsingFusionOtpInput : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/OtpInput";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_OtpInputModel";
    private const string PasscodeId = GeneratedTypeScope + "__Passcode";
    private const string AutoBlurCodeId = GeneratedTypeScope + "__AutoBlurCode";

    private FusionOtpInputLocator Passcode => new(Page, PasscodeId);
    private FusionOtpInputLocator AutoBlurCode => new(Page, AutoBlurCodeId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionOtpInput — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_otp_input_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(PasscodeId));
        Assert.That(planJson, Does.Contain("\"value\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"focusIn\""));
        Assert.That(planJson, Does.Contain("\"focusOut\""));
        Assert.That(planJson, Does.Contain("\"input\""));
        Assert.That(planJson, Does.Contain("\"valueChanged\""));
        Assert.That(planJson, Does.Contain("\"focus\""));
        Assert.That(planJson, Does.Contain("\"blur\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_visible_value_and_reads_value_source()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("1357", new() { Timeout = 5000 });
        Assert.That(await Passcode.HiddenValue(), Is.EqualTo("1357"));
        Assert.That(await Passcode.FieldValue(0), Is.EqualTo("1"));
        Assert.That(await Passcode.FieldValue(1), Is.EqualTo("3"));
        Assert.That(await Passcode.FieldValue(2), Is.EqualTo("5"));
        Assert.That(await Passcode.FieldValue(3), Is.EqualTo("7"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_value_updates_visible_fields_and_valuechanged_payload()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-code-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("code set", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("9876", new() { Timeout = 5000 });
        Assert.That(await Passcode.HiddenValue(), Is.EqualTo("9876"));
        Assert.That(await Passcode.FieldValue(0), Is.EqualTo("9"));
        Assert.That(await Passcode.FieldValue(1), Is.EqualTo("8"));
        Assert.That(await Passcode.FieldValue(2), Is.EqualTo("7"));
        Assert.That(await Passcode.FieldValue(3), Is.EqualTo("6"));
        await Expect(Page.Locator("#changed-value")).ToHaveTextAsync("9876", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-previous")).ToHaveTextAsync("1357", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-interacted")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_first_empty_field_and_focus_event_reads_payload()
    {
        await NavigateAndBoot();

        await Page.Locator("#clear-code-btn").ClickAsync();
        await Page.Locator("#focus-code-btn").ClickAsync();

        await Expect(Passcode.Field(0)).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focused", new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-value")).ToHaveTextAsync("", new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-index")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-interacted")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task browser_input_events_expose_input_and_valuechanged_payloads()
    {
        await NavigateAndBoot();

        await Page.Locator("#clear-code-btn").ClickAsync();
        await Passcode.FillCode("2468");

        await Expect(Page.Locator("#input-value")).ToHaveTextAsync("2468", new() { Timeout = 5000 });
        await Expect(Page.Locator("#input-previous")).ToHaveTextAsync("246", new() { Timeout = 5000 });
        await Expect(Page.Locator("#input-index")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-value")).ToHaveTextAsync("2468", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-previous")).ToHaveTextAsync("1357", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("complete", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task blur_event_exposes_current_value_index_and_interaction_state()
    {
        await NavigateAndBoot();

        await Page.Locator("#clear-code-btn").ClickAsync();
        await Passcode.FillCode("2468");
        await Passcode.Field(3).BlurAsync();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        await Expect(Page.Locator("#blur-value")).ToHaveTextAsync("2468", new() { Timeout = 5000 });
        await Expect(Page.Locator("#blur-index")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        await Expect(Page.Locator("#blur-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusout_method_removes_focus_without_button_click_stealing_focus()
    {
        await NavigateAndBoot();

        await AutoBlurCode.Focus();

        await Expect(Page.Locator("#focusout-method-state")).ToHaveTextAsync("focusout called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#autoblur-state")).ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        await Expect(AutoBlurCode.Field(0)).Not.ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_reads_current_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-code-btn").ClickAsync();
        await Page.Locator("#check-code-btn").ClickAsync();
        await Expect(Page.Locator("#code-state")).ToHaveTextAsync("verified", new() { Timeout = 5000 });

        await Page.Locator("#clear-code-btn").ClickAsync();
        await Page.Locator("#check-code-btn").ClickAsync();
        await Expect(Page.Locator("#code-state")).ToHaveTextAsync("pending", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_otp_input_value_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-code-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-code")).ToHaveTextAsync("9876", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("code:9876", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
