namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.CrossVendor;

[TestFixture]
public class WhenNativeAndFusionShareOneWorkflow : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/Architecture";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForPageReady(5000);
    }

    // ── Scenario 1: Same mutation works on both vendors ──

    [Test]
    public async Task property_write_sets_value_on_both_native_and_fusion_simultaneously()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#native-write")).ToHaveValueAsync("written-native");

        await Expect(Page.Locator("#fusion-write input")).ToHaveValueAsync("written-fusion");

        AssertNoConsoleErrors();
    }

    // ── Scenario 2: Gather reads from both vendors in same POST ──

    [Test]
    public async Task gather_reads_values_from_both_vendors_in_single_post()
    {
        await NavigateAndBoot();

        // Section 8: one button gathers native "n-both" and fusion "f-both" together
        await Page.Locator("#both-vendors-btn").ClickAsync();
        await Expect(Page.Locator("#both-vendors-result"))
            .ToHaveTextAsync("both-gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 3: Component events wire on both vendors ──
    // Both must resolve the correct value from the event and update the DOM.

    [Test]
    public async Task native_change_event_runs_workflow_and_fusion_change_event_runs_workflow()
    {
        await NavigateAndBoot();

        // Native: type in input, fire change, and verify the echoed value.
        await Page.Locator("#native-event").FillAsync("user-typed");
        await Page.Locator("#native-event").DispatchEventAsync("change");
        await Expect(Page.Locator("#native-event-result"))
            .ToHaveTextAsync("user-typed", new() { Timeout = 3000 });

        // Fusion: type in the visible input and verify the echoed value.
        var inner = Page.Locator("#fusion-event input");
        await inner.FillAsync("user-typed-fusion");
        await Expect(Page.Locator("#fusion-event-result"))
            .ToHaveTextAsync("user-typed-fusion", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // Both nested values should appear after one interaction.

    [Test]
    public async Task deep_dot_path_resolves_three_level_nested_payload()
    {
        await NavigateAndBoot();

        // Click to load the nested values into the page.
        await Page.Locator("#deep-walk-btn").ClickAsync();

        await Expect(Page.Locator("#deep-total"))
            .ToHaveTextAsync("99.5", new() { Timeout = 3000 });
        await Expect(Page.Locator("#deep-city"))
            .ToHaveTextAsync("NY", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 5: EqualTo validation compares password fields ──
    // WHY: proves validation reads field values and compares via equalTo rule.
    // Mismatched values must fail, matching values must pass.

    [Test]
    public async Task equalto_validation_compares_password_vs_confirm()
    {
        await NavigateAndBoot();

        // Type password
        await Page.Locator("[name='Password']").FillAsync("secret");

        // Type DIFFERENT confirm → validation must fail
        await Page.Locator("[name='ConfirmPassword']").FillAsync("wrong");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='ConfirmPassword']"))
            .ToContainTextAsync("Must match", new() { Timeout = 3000 });

        // Type MATCHING confirm → validation must pass
        await Page.Locator("[name='ConfirmPassword']").FillAsync("secret");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("#equalto-result"))
            .ToContainTextAsync("Passwords match", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 6: Cross-vendor gather integrity ──

    [Test]
    public async Task native_and_fusion_gather_each_send_their_current_values()
    {
        await NavigateAndBoot();

        // Gather native from the pre-filled control.
        await Page.Locator("#gather-native-btn").ClickAsync();
        await Expect(Page.Locator("#gather-native-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        // Gather fusion from the pre-filled control.
        await Page.Locator("#gather-fusion-btn").ClickAsync();
        await Expect(Page.Locator("#gather-fusion-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 7: Native gather sends CURRENT value, not cached ──
    // WHY: proves gather reads the live DOM value at click time, not a stale cached value.
    // If gather cached the initial value at boot, this test would fail — the server would
    // receive "native-42" instead of the user-typed value.

    [Test]
    public async Task modifying_native_input_then_gathering_sends_updated_value()
    {
        await NavigateAndBoot();

        // Overwrite the pre-filled "native-42" with a fresh value
        await Page.Locator("#native-gather").FillAsync("fresh-native-99");

        // Click gather and intercept the POST to verify the request body
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-native-btn").ClickAsync(),
            "**/Sandbox/AllModulesTogether/Architecture/Echo");

        // The POST body must contain the CURRENT value, not the original "native-42"
        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("fresh-native-99"),
            "Gather must send the current native input value, not the original pre-filled value");
        Assert.That(body, Does.Not.Contain("native-42"),
            "Gather must NOT send the stale pre-filled value");

        // Confirm the round-trip completes — response handler fires
        await Expect(Page.Locator("#gather-native-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 8: Fusion gather sends CURRENT value, not cached ──
    // WHY: same as Scenario 7 but for the fusion vendor. Proves Syncfusion component value
    // is read live at gather time. The TestWidget updates its internal _value when
    // the inner input fires "input" — gather must pick up that live state.

    [Test]
    public async Task modifying_fusion_widget_then_gathering_sends_updated_value()
    {
        await NavigateAndBoot();

        // Overwrite the pre-filled "fusion-42" by typing in the TestWidget's inner input
        await Page.Locator("#fusion-gather input").FillAsync("fresh-fusion-77");

        // Click gather and intercept the POST to verify the request body
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-fusion-btn").ClickAsync(),
            "**/Sandbox/AllModulesTogether/Architecture/Echo");

        // The POST body must contain the CURRENT widget value, not the original "fusion-42"
        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("fresh-fusion-77"),
            "Gather must send the current fusion widget value, not the original pre-filled value");
        Assert.That(body, Does.Not.Contain("fusion-42"),
            "Gather must NOT send the stale pre-filled value");

        // Confirm the round-trip completes — response handler fires
        await Expect(Page.Locator("#gather-fusion-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 9: EqualTo validation is live, not cached ──
    // WHY: proves validation re-reads CURRENT field values on every click.
    // If validation cached values at boot or first click, the second validate would
    // still pass after changing the confirm field — a dangerous silent bug.

    [Test]
    public async Task validation_passes_after_filling_both_fields()
    {
        await NavigateAndBoot();

        // Step 1: Fill both fields with MATCHING values → validation must pass
        await Page.Locator("[name='Password']").FillAsync("MySecret123");
        await Page.Locator("[name='ConfirmPassword']").FillAsync("MySecret123");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("#equalto-result"))
            .ToContainTextAsync("Passwords match", new() { Timeout = 5000 });

        // Step 2: Change the confirm field to a DIFFERENT value → validation must FAIL
        // This proves validation reads the CURRENT confirm field value, not a cached one
        await Page.Locator("[name='ConfirmPassword']").FillAsync("Mismatch!");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='ConfirmPassword']"))
            .ToContainTextAsync("Must match", new() { Timeout = 5000 });

        // Step 3: Fix the confirm field back to matching → validation must pass AGAIN
        // This proves the validation cycle is fully live — pass/fail/pass works
        await Page.Locator("[name='ConfirmPassword']").FillAsync("MySecret123");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("#equalto-result"))
            .ToContainTextAsync("Passwords match", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 10: Void method call focuses fusion widget on dom-ready ──
    // and calls the Syncfusion focus API. The TestWidget sets _focused=true and calls

    [Test]
    public async Task void_method_call_focuses_fusion_widget_inner_input_on_boot()
    {
        await NavigateAndBoot();

        // The dom-ready plan calls focus() on the fusion-focus TestWidget.
        // TestWidget.focus() calls this._input.focus(), so the inner input should be focused.
        await Expect(Page.Locator("#fusion-focus input")).ToBeFocusedAsync();

        AssertNoConsoleErrors();
    }

    // ── Scenario 11: Required validation blocks POST when both fields are empty ──
    // WHY: proves validation reads both fields and enforces required on each independently.
    // When both native and confirm fields are empty, validation must block the POST entirely.
    // The success message "Both fields passed!" must NOT appear — proving the POST never fires.

    [Test]
    public async Task required_validation_blocks_post_when_both_fields_are_empty()
    {
        await NavigateAndBoot();

        // Click validate with both fields empty — validation must block the POST
        await ClickWhenStable(Page.Locator("#validate-btn"));

        // The success message "Both fields passed!" must NOT appear —
        // proving the POST was blocked by client-side validation.
        // The result text should remain at its default "Click to validate".
        await Expect(Page.Locator("#val-result"))
            .ToHaveTextAsync("Click to validate", new() { Timeout = 3000 });

        var nativeError = Page.Locator("[data-valmsg-for='NativeRequired']");
        var fusionError = Page.Locator("[data-valmsg-for='FusionRequired']");
        await Expect(nativeError).ToHaveTextAsync("Required", new() { Timeout = 3000 });
        await Expect(fusionError).ToHaveTextAsync("Required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 12: Required validation passes when both fields are filled ──
    // WHY: proves evalRead works for both fields to read non-empty values during validation.
    // The success handler sets "Both fields passed!" text — confirming the POST was not
    // blocked by client-side validation and the response pipeline ran.

    [Test]
    public async Task required_validation_passes_when_both_fields_are_filled()
    {
        await NavigateAndBoot();

        // Fill native input
        await Page.Locator("[name='NativeRequired']").FillAsync("native-value");

        // Fill confirm field
        await Page.Locator("[name='FusionRequired']").FillAsync("fusion-value");

        // Click validate — both fields have values, validation must pass
        await Page.Locator("#validate-btn").ClickAsync();
        await Expect(Page.Locator("#val-result"))
            .ToHaveTextAsync("Both fields passed!", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 13: Live-clear removes password error after typing ──
    // WHY: proves live-clear wiring works for the password field in the EqualTo form.
    // After validation shows "Required" on the password, typing in the field must
    // clear the error via live-clear. If live-clear is broken, the error persists.

    [Test]
    public async Task live_clear_removes_password_error_after_typing()
    {
        await NavigateAndBoot();

        // Trigger required error on password by validating with both fields empty
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='Password']"))
            .ToContainTextAsync("Required", new() { Timeout = 3000 });
        await Expect(Page.Locator("[data-valmsg-for='Password']"))
            .ToBeVisibleAsync();

        // Type in password — live-clear must hide the error
        await Page.Locator("[name='Password']").FillAsync("typed-password");
        await Expect(Page.Locator("[data-valmsg-for='Password']"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 14: Live-revalidation clears confirm field error when values match ──
    // WHY: proves live-revalidation wiring works for the confirm field. When the user types a
    // matching value, the equalTo rule passes and the error is cleared. If live-revalidation
    // is broken, the error persists even after entering matching values.

    [Test]
    public async Task live_revalidation_clears_equalto_error_when_values_match()
    {
        await NavigateAndBoot();

        // Fill password first, then mismatch confirm field
        await Page.Locator("[name='Password']").FillAsync("same-value");
        await Page.Locator("[name='ConfirmPassword']").FillAsync("mismatch");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='ConfirmPassword']"))
            .ToContainTextAsync("Must match", new() { Timeout = 3000 });

        // Now type the MATCHING value in confirm field — live-revalidation must
        // re-run the equalTo check, find values equal, and clear the error
        await Page.Locator("[name='ConfirmPassword']").FillAsync("same-value");
        await Expect(Page.Locator("[data-valmsg-for='ConfirmPassword']"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 15: Mixed vendor gather POST body contains values from both vendors ──
    // values in the request body. If gather only reads one vendor, the other value is
    // missing from the POST. This intercepts the actual HTTP request to verify payload.

    [Test]
    public async Task mixed_vendor_gather_post_body_contains_both_native_and_fusion_values()
    {
        await NavigateAndBoot();

        // Section 8: native-both has pre-filled value "n-both", fusion-both has "f-both"
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#both-vendors-btn").ClickAsync(),
            "**/Sandbox/AllModulesTogether/Architecture/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("n-both"),
            "Gather POST must include the native vendor's value");
        Assert.That(body, Does.Contain("f-both"),
            "Gather POST must include the fusion vendor's value");

        // Confirm the round-trip completes
        await Expect(Page.Locator("#both-vendors-result"))
            .ToHaveTextAsync("both-gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 16: EqualTo validation shows required error on empty password ──
    // WHY: proves the password field's "required" rule fires independently of the
    // equalTo rule on the confirm field. Both fields have required rules. Submitting with
    // both empty must show required errors on both, not just the equalTo error.

    [Test]
    public async Task equalto_form_shows_required_errors_when_both_fields_empty()
    {
        await NavigateAndBoot();

        // Click validate with both password and confirm empty
        await Page.Locator("#cross-validate-btn").ClickAsync();

        // Password must show "Required"
        await Expect(Page.Locator("[data-valmsg-for='Password']"))
            .ToContainTextAsync("Required", new() { Timeout = 3000 });

        // Confirm field must show "Required" (required fires before equalTo)
        await Expect(Page.Locator("[data-valmsg-for='ConfirmPassword']"))
            .ToContainTextAsync("Required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 18: Mixed vendor gather with user-modified values sends current state ──
    // WHY: proves that when BOTH native AND fusion values are changed before a mixed gather,
    // the POST body reflects the CURRENT state of both vendors — not the pre-filled values.
    // This is the strongest test of live gather: two vendors, both modified, single POST.

    [Test]
    public async Task mixed_vendor_gather_sends_current_values_after_both_modified()
    {
        await NavigateAndBoot();

        // Modify both vendor fields from their pre-filled values
        await Page.Locator("#native-both").FillAsync("native-updated");
        await Page.Locator("#fusion-both input").FillAsync("fusion-updated");

        // Intercept the POST to verify both current values are sent
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#both-vendors-btn").ClickAsync(),
            "**/Sandbox/AllModulesTogether/Architecture/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("native-updated"),
            "Mixed gather must send the CURRENT native value, not 'n-both'");
        Assert.That(body, Does.Contain("fusion-updated"),
            "Mixed gather must send the CURRENT fusion value, not 'f-both'");
        Assert.That(body, Does.Not.Contain("n-both"),
            "Mixed gather must NOT send the stale native pre-filled value");
        Assert.That(body, Does.Not.Contain("f-both"),
            "Mixed gather must NOT send the stale fusion pre-filled value");

        // Confirm the round-trip completes
        await Expect(Page.Locator("#both-vendors-result"))
            .ToHaveTextAsync("both-gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
