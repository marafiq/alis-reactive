namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.CrossVendor;

/// <summary>
/// Cross-vendor regression harness for the vendor-agnostic component architecture.
///
/// Page under test: /Sandbox/AllModulesTogether/Architecture
///
/// Each test exercises a cross-cutting concern that MUST work identically for both
/// native (raw DOM) and fusion (ej2_instances) vendors. If any test fails, the
/// vendor-agnostic architecture has regressed.
///
/// TestWidgetSyncFusion is a REAL TS component mounted via the ej2_instances pattern.
/// Playwright interacts with its INNER input — no Page.EvaluateAsync() to fire events.
/// </summary>
[TestFixture]
public class WhenBothVendorsExecuteSamePlan : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/Architecture";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    // ── Scenario 1: Same mutation works on both vendors ──
    // WHY: proves resolveRoot() works for both vendors with the same set-prop mutation.
    // If resolveRoot is broken for either vendor, the value won't appear.

    [Test]
    public async Task property_write_sets_value_on_both_native_and_fusion_simultaneously()
    {
        await NavigateAndBoot();

        // Native: el.value = "written-native" via set-prop mutation
        await Expect(Page.Locator("#native-write")).ToHaveValueAsync("written-native");

        // Fusion: ej2_instances[0].value = "written-fusion" via same set-prop mutation
        // TestWidget syncs its inner input when value is set on the ej2 instance
        await Expect(Page.Locator("#fusion-write input")).ToHaveValueAsync("written-fusion");

        AssertNoConsoleErrors();
    }

    // ── Scenario 2: Gather reads from both vendors in same POST ──
    // WHY: proves component.ts evalRead works for both vendor roots in a single gather.
    // Same readExpr "value" resolves to el.value for native and ej2_instances[0].value for fusion.

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
    // WHY: proves ComponentEventTrigger wiring works for both vendors.
    // Native wires DOM "change" on the element. Fusion wires "change" on the ej2 instance.
    // Both must resolve the correct value from the event and update the DOM.

    [Test]
    public async Task native_change_event_fires_reaction_and_fusion_change_event_fires_reaction()
    {
        await NavigateAndBoot();

        // Native: type in input, fire change → evt.value walks to result
        await Page.Locator("#native-event").FillAsync("user-typed");
        await Page.Locator("#native-event").DispatchEventAsync("change");
        await Expect(Page.Locator("#native-event-result"))
            .ToHaveTextAsync("user-typed", new() { Timeout = 3000 });

        // Fusion: type in TestWidget's INNER input → widget fires "change" with {newValue}
        var inner = Page.Locator("#fusion-event input");
        await inner.FillAsync("user-typed-fusion");
        await Expect(Page.Locator("#fusion-event-result"))
            .ToHaveTextAsync("user-typed-fusion", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 4: Deep dot-path walking resolves nested payloads ──
    // WHY: proves walk.ts resolves 3+ level deep paths correctly.
    // evt.result.detail.total (3 levels) and evt.result.detail.address.city (4 levels)
    // must both resolve from a single dispatched payload.

    [Test]
    public async Task deep_dot_path_resolves_three_level_nested_payload()
    {
        await NavigateAndBoot();

        // Click dispatches payload with result.detail.total=99.5 and result.detail.address.city="NY"
        await Page.Locator("#deep-walk-btn").ClickAsync();

        await Expect(Page.Locator("#deep-total"))
            .ToHaveTextAsync("99.5", new() { Timeout = 3000 });
        await Expect(Page.Locator("#deep-city"))
            .ToHaveTextAsync("NY", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 5: Cross-vendor validation compares values ──
    // WHY: proves validation reads from both vendor roots for comparison.
    // EqualTo rule reads native password via el.value and fusion confirm via ej2_instances[0].value.
    // Mismatched values must fail, matching values must pass.

    [Test]
    public async Task cross_vendor_equalto_validation_compares_native_vs_fusion()
    {
        await NavigateAndBoot();

        // Type password in native input
        await Page.Locator("#native-password").FillAsync("secret");

        // Type DIFFERENT confirm in fusion widget → validation must fail
        await Page.Locator("#fusion-confirm input").FillAsync("wrong");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='fusion-confirm']"))
            .ToContainTextAsync("Must match", new() { Timeout = 3000 });

        // Type MATCHING confirm → validation must pass
        await Page.Locator("#fusion-confirm input").FillAsync("secret");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("#equalto-result"))
            .ToContainTextAsync("Passwords match", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 6: Cross-vendor gather integrity ──
    // WHY: proves readExpr "value" resolves differently per vendor root.
    // Native: el.value. Fusion: ej2_instances[0].value.
    // Each gather must read the correct vendor root independently.

    [Test]
    public async Task native_gather_reads_via_el_value_and_fusion_gather_reads_via_ej2_value()
    {
        await NavigateAndBoot();

        // Gather native — reads el.value from #native-gather (pre-filled "native-42")
        await Page.Locator("#gather-native-btn").ClickAsync();
        await Expect(Page.Locator("#gather-native-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        // Gather fusion — reads ej2_instances[0].value from #fusion-gather (pre-filled "fusion-42")
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
    // WHY: same as Scenario 7 but for the fusion vendor. Proves ej2_instances[0].value
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

    // ── Scenario 9: Cross-vendor EqualTo validation is live, not cached ──
    // WHY: proves validation re-reads CURRENT component values on every click.
    // If validation cached values at boot or first click, the second validate would
    // still pass after changing the confirm field — a dangerous silent bug.

    [Test]
    public async Task validation_passes_after_filling_both_vendor_fields()
    {
        await NavigateAndBoot();

        // Step 1: Fill both fields with MATCHING values → validation must pass
        await Page.Locator("#native-password").FillAsync("MySecret123");
        await Page.Locator("#fusion-confirm input").FillAsync("MySecret123");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("#equalto-result"))
            .ToContainTextAsync("Passwords match", new() { Timeout = 5000 });

        // Step 2: Change the fusion confirm to a DIFFERENT value → validation must FAIL
        // This proves validation reads the CURRENT fusion widget value, not a cached one
        await Page.Locator("#fusion-confirm input").FillAsync("Mismatch!");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='fusion-confirm']"))
            .ToContainTextAsync("Must match", new() { Timeout = 5000 });

        // Step 3: Fix the fusion confirm back to matching → validation must pass AGAIN
        // This proves the validation cycle is fully live — pass/fail/pass works
        await Page.Locator("#fusion-confirm input").FillAsync("MySecret123");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("#equalto-result"))
            .ToContainTextAsync("Passwords match", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 10: Void method call focuses fusion widget on dom-ready ──
    // WHY: proves CallMutation with method:"focus" and no args resolves through resolveRoot
    // and calls ej2_instances[0].focus(). The TestWidget sets _focused=true and calls
    // input.focus(). If resolveRoot or bracket notation is broken, the input won't be focused.

    [Test]
    public async Task void_method_call_focuses_fusion_widget_inner_input_on_boot()
    {
        await NavigateAndBoot();

        // The dom-ready plan calls focus() on the fusion-focus TestWidget.
        // TestWidget.focus() calls this._input.focus(), so the inner input should be focused.
        await Expect(Page.Locator("#fusion-focus input")).ToBeFocusedAsync();

        AssertNoConsoleErrors();
    }

    // ── Scenario 11: Required validation blocks POST when both vendor fields are empty ──
    // WHY: proves validation reads both vendor roots and enforces required on each independently.
    // When both native and fusion fields are empty, validation must block the POST entirely.
    // The success message "Both fields passed!" must NOT appear — proving the POST never fires.

    [Test]
    public async Task required_validation_blocks_post_when_both_vendor_fields_are_empty()
    {
        await NavigateAndBoot();

        // Click validate with both fields empty — validation must block the POST
        await Page.Locator("#validate-btn").ClickAsync();

        // Validation errors must appear on both fields — proves validation ran and blocked the POST
        await Expect(Page.Locator("[data-valmsg-for='native-val-field']"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("[data-valmsg-for='fusion-val-field']"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });

        // The success message must NOT appear — POST was blocked by client validation
        await Expect(Page.Locator("#val-result"))
            .ToHaveTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }

    // ── Scenario 12: Required validation passes when both vendor fields are filled ──
    // WHY: proves evalRead works for both vendors to read non-empty values during validation.
    // The success handler sets "Both fields passed!" text — confirming the POST was not
    // blocked by client-side validation and the response pipeline ran.

    [Test]
    public async Task required_validation_passes_when_both_vendor_fields_are_filled()
    {
        await NavigateAndBoot();

        // Fill native input
        await Page.Locator("#native-val-field").FillAsync("native-value");

        // Fill fusion TestWidget's inner input
        await Page.Locator("#fusion-val-field input").FillAsync("fusion-value");

        // Click validate — both fields have values, validation must pass
        await Page.Locator("#validate-btn").ClickAsync();
        await Expect(Page.Locator("#val-result"))
            .ToHaveTextAsync("Both fields passed!", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 13: Live-clear removes native password error after typing ──
    // WHY: proves live-clear wiring works for native vendor fields in the EqualTo form.
    // After validation shows "Required" on the native password, typing in the field must
    // clear the error via live-clear. If live-clear is broken for native, the error persists.

    [Test]
    public async Task live_clear_removes_native_password_error_after_typing()
    {
        await NavigateAndBoot();

        // Trigger required error on native-password by validating with both fields empty
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='native-password']"))
            .ToContainTextAsync("Required", new() { Timeout = 3000 });
        await Expect(Page.Locator("[data-valmsg-for='native-password']"))
            .ToBeVisibleAsync();

        // Type in native password — live-clear must hide the error
        await Page.Locator("#native-password").FillAsync("typed-password");
        await Expect(Page.Locator("[data-valmsg-for='native-password']"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 14: Fusion live-revalidation clears error when values match ──
    // WHY: proves live-revalidation wiring works for fusion vendor. Unlike native (which
    // clears on "input"), fusion wires "change" as re-validate — so when the user types a
    // matching value, the equalTo rule passes and the error is cleared. If resolveRoot or
    // event wiring is broken for fusion, the re-validate never fires and the error persists.

    [Test]
    public async Task fusion_live_revalidation_clears_equalto_error_when_values_match()
    {
        await NavigateAndBoot();

        // Fill native password first, then mismatch fusion confirm
        await Page.Locator("#native-password").FillAsync("same-value");
        await Page.Locator("#fusion-confirm input").FillAsync("mismatch");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='fusion-confirm']"))
            .ToContainTextAsync("Must match", new() { Timeout = 3000 });

        // Now type the MATCHING value in fusion confirm — live-revalidation must
        // re-run the equalTo check, find values equal, and clear the error
        await Page.Locator("#fusion-confirm input").FillAsync("same-value");
        await Expect(Page.Locator("[data-valmsg-for='fusion-confirm']"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 15: Mixed vendor gather POST body contains values from both vendors ──
    // WHY: proves that a single POST gather reads BOTH vendor roots and includes both
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
    // WHY: proves the native-password field's "required" rule fires independently of the
    // equalTo rule on fusion-confirm. Both fields have required rules. Submitting with
    // both empty must show required errors on both, not just the equalTo error.

    [Test]
    public async Task equalto_form_shows_required_errors_when_both_fields_empty()
    {
        await NavigateAndBoot();

        // Click validate with both password and confirm empty
        await Page.Locator("#cross-validate-btn").ClickAsync();

        // Native password must show "Required"
        await Expect(Page.Locator("[data-valmsg-for='native-password']"))
            .ToContainTextAsync("Required", new() { Timeout = 3000 });

        // Fusion confirm must show "Required" (required fires before equalTo)
        await Expect(Page.Locator("[data-valmsg-for='fusion-confirm']"))
            .ToContainTextAsync("Required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 17: Page renders plan JSON for debugging ──
    // WHY: proves the plan JSON is serialized and visible on the page for developer inspection.
    // The plan contains entries with triggers and reactions — if serialization is broken,
    // the #plan-json element would be empty or malformed.

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
