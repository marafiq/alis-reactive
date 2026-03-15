namespace Alis.Reactive.PlaywrightTests.Architecture;

/// <summary>
/// Cross-vendor regression harness for the vendor-agnostic component architecture.
///
/// Page under test: /Sandbox/Architecture
///
/// Each test exercises a cross-cutting concern that MUST work identically for both
/// native (raw DOM) and fusion (ej2_instances) vendors. If any test fails, the
/// vendor-agnostic architecture has regressed.
///
/// TestWidgetSyncFusion is a REAL TS component mounted via the ej2_instances pattern.
/// Playwright interacts with its INNER input — no Page.EvaluateAsync() to fire events.
/// </summary>
[TestFixture]
public class WhenExercisingComponentArchitecture : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Architecture";

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
            "**/Sandbox/Architecture/Echo");

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
            "**/Sandbox/Architecture/Echo");

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
}
