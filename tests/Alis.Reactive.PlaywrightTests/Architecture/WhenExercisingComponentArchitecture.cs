namespace Alis.Reactive.PlaywrightTests.Architecture;

/// <summary>
/// Verifies the vendor-agnostic component architecture end-to-end.
/// Each test exercises one interaction type (property write, void method, gather, event, validation)
/// with both native and fusion vendors side-by-side.
///
/// Page under test: /Sandbox/Architecture
///
/// TestWidgetSyncFusion is a REAL TS component compiled as IIFE, mounted via ej2_instances pattern.
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

    // ── Page loads ──

    [Test]
    public async Task PageLoadsWithNoErrors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("Architecture — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Property Write (jsEmit) — both vendors ──

    [Test]
    public async Task NativePropertyWriteViaJsEmit()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#native-write")).ToHaveValueAsync("written-native");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task FusionPropertyWriteViaJsEmit()
    {
        await NavigateAndBoot();
        // TestWidget syncs inner input when value is set
        await Expect(Page.Locator("#fusion-write input")).ToHaveValueAsync("written-fusion");
        AssertNoConsoleErrors();
    }

    // ── Void Method Call (jsEmit) — fusion ──

    [Test]
    public async Task FusionVoidMethodViaJsEmit()
    {
        await NavigateAndBoot();
        // dom-ready called ej2_instances[0].focus() — inner input gets focus
        await Expect(Page.Locator("#fusion-focus input")).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    // ── Property Read via Gather — both vendors ──

    [Test]
    public async Task NativePropertyReadViaGather()
    {
        await NavigateAndBoot();
        await Page.Locator("#gather-native-btn").ClickAsync();
        await Expect(Page.Locator("#gather-native-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task FusionPropertyReadViaGather()
    {
        await NavigateAndBoot();
        await Page.Locator("#gather-fusion-btn").ClickAsync();
        await Expect(Page.Locator("#gather-fusion-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Event Wiring — both vendors ──

    [Test]
    public async Task NativeEventWiringFiresOnUserInput()
    {
        await NavigateAndBoot();
        await Page.Locator("#native-event").FillAsync("user-typed");
        await Page.Locator("#native-event").DispatchEventAsync("change");
        await Expect(Page.Locator("#native-event-result"))
            .ToHaveTextAsync("user-typed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task FusionEventWiringFiresOnUserInput()
    {
        await NavigateAndBoot();
        // Type in TestWidget's INNER input — widget fires "change" with {newValue}
        var inner = Page.Locator("#fusion-event input");
        await inner.FillAsync("user-typed-fusion");
        await Expect(Page.Locator("#fusion-event-result"))
            .ToHaveTextAsync("user-typed-fusion", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Deep Dot-Path Walk ──

    [Test]
    public async Task DeepDotPathWalk()
    {
        await NavigateAndBoot();
        await Page.Locator("#deep-walk-btn").ClickAsync();
        await Expect(Page.Locator("#deep-total")).ToHaveTextAsync("99.5", new() { Timeout = 3000 });
        await Expect(Page.Locator("#deep-city")).ToHaveTextAsync("NY", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Validation — cross-vendor ──

    [Test]
    public async Task ValidationFailsForEmptyNativeAndFusionFields()
    {
        await NavigateAndBoot();
        await Page.Locator("#validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='native-val-field']"))
            .ToContainTextAsync("Required", new() { Timeout = 3000 });
        await Expect(Page.Locator("[data-valmsg-for='fusion-val-field']"))
            .ToContainTextAsync("Required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ValidationPassesWhenBothFieldsFilled()
    {
        await NavigateAndBoot();
        await Page.Locator("#native-val-field").FillAsync("hello");
        await Page.Locator("#fusion-val-field input").FillAsync("42");
        await Page.Locator("#validate-btn").ClickAsync();
        await Expect(Page.Locator("#val-result"))
            .ToContainTextAsync("Both fields passed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ValidationEqualToComparesAcrossVendors()
    {
        await NavigateAndBoot();
        await Page.Locator("#native-password").FillAsync("secret");
        await Page.Locator("#fusion-confirm input").FillAsync("wrong");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("[data-valmsg-for='fusion-confirm']"))
            .ToContainTextAsync("Must match", new() { Timeout = 3000 });

        // Fix: fill matching value
        await Page.Locator("#fusion-confirm input").FillAsync("secret");
        await Page.Locator("#cross-validate-btn").ClickAsync();
        await Expect(Page.Locator("#equalto-result"))
            .ToContainTextAsync("Passwords match", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Both vendors gathered together ──

    [Test]
    public async Task SameReadExprGathersBothVendors()
    {
        await NavigateAndBoot();
        await Page.Locator("#both-vendors-btn").ClickAsync();
        await Expect(Page.Locator("#both-vendors-result"))
            .ToHaveTextAsync("both-gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
