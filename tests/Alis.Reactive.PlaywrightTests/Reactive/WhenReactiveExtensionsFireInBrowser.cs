namespace Alis.Reactive.PlaywrightTests.Reactive;

/// <summary>
/// Verifies that .Reactive() extensions on both Fusion and Native builders
/// wire component-event triggers end-to-end: C# DSL → plan JSON → TS runtime → DOM mutation.
///
/// Page under test: /Sandbox/PlaygroundSyntax
///
/// ID Pattern Discovery (nested property m => m.Address.City):
///   - Native (ASP.NET Html.IdFor): Address_City  (underscores)
///   - Fusion (SF EJ2):             AddressCity    (dots removed, no separator)
///
/// Both vendors carry bindingPath as dot-notation (Address.City) for future HTTP gather.
/// </summary>
[TestFixture]
public class WhenReactiveExtensionsFireInBrowser : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/PlaygroundSyntax");
        await WaitForTraceMessage("booted", 5000);
    }

    // ── Plan JSON structure ──

    [Test]
    public async Task Plan_json_contains_both_vendor_triggers()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("component-event"),
            "Plan must contain component-event trigger kind");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        Assert.That(planJson, Does.Contain("\"vendor\": \"native\""),
            "Plan must contain native vendor");
        Assert.That(planJson, Does.Contain("custom-event"),
            "Plan must contain custom-event trigger for reset-all");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Plan_json_contains_native_binding_paths()
    {
        await NavigateAndBoot();

        // Native vendor carries bindingPath (NativeDropDownBuilder stores it explicitly)
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"bindingPath\": \"Status\""),
            "Native flat property bindingPath must be present");
        Assert.That(planJson, Does.Contain("\"bindingPath\": \"Address.City\""),
            "Native nested property bindingPath must use dot notation");

        AssertNoConsoleErrors();
    }

    // ── Fusion vendor — flat property ──

    [Test]
    public async Task Fusion_numeric_change_fires_reaction()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#amount-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        // SF NumericTextBox: type a value, Tab triggers change event
        var numericInput = Page.Locator("#Amount");
        await numericInput.ClickAsync();
        await numericInput.FillAsync("42");
        await numericInput.PressAsync("Tab");

        await Expect(echo).ToHaveTextAsync("Amount changed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Native vendor — flat property ──

    [Test]
    public async Task Native_dropdown_change_fires_reaction()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#status-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        await Page.Locator("#Status").SelectOptionAsync("Active");

        await Expect(echo).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Native_dropdown_without_reactive_does_not_fire()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");
        var amountEcho = Page.Locator("#amount-echo");

        // Category dropdown has NO .Reactive() — selecting must not affect any echo
        await Page.Locator("#Category").SelectOptionAsync("Category A");

        await Expect(statusEcho).ToHaveTextAsync("\u2014");
        await Expect(amountEcho).ToHaveTextAsync("\u2014");
        AssertNoConsoleErrors();
    }

    // ── Nested properties — ID pattern verification ──

    [Test]
    public async Task Native_nested_element_id_uses_underscores()
    {
        await NavigateAndBoot();

        // m => m.Address.City → Native convention: Address_City (underscores)
        var citySelect = Page.Locator("#Address_City");
        await Expect(citySelect).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Fusion_nested_element_id_removes_dots()
    {
        await NavigateAndBoot();

        // m => m.Address.PostalCode → SF convention: AddressPostalCode (dots removed)
        var postalInput = Page.Locator("#AddressPostalCode");
        await Expect(postalInput).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Nested_native_dropdown_change_fires_reaction()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#city-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        await Page.Locator("#Address_City").SelectOptionAsync("seattle");

        await Expect(echo).ToHaveTextAsync("City changed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Nested_fusion_numeric_change_fires_reaction()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#postal-echo");
        await Expect(echo).ToHaveTextAsync("\u2014");

        var postalInput = Page.Locator("#AddressPostalCode");
        await postalInput.ClickAsync();
        await postalInput.FillAsync("98101");
        await postalInput.PressAsync("Tab");

        await Expect(echo).ToHaveTextAsync("PostalCode changed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Plan_json_shows_vendor_specific_component_ids()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        // Native nested: underscores
        Assert.That(planJson, Does.Contain("\"componentId\": \"Address_City\""),
            "Native nested componentId must use underscores");
        // Fusion nested: dots removed
        Assert.That(planJson, Does.Contain("\"componentId\": \"AddressPostalCode\""),
            "Fusion nested componentId must have dots removed (SF convention)");

        AssertNoConsoleErrors();
    }

    // ── Cross-vendor custom event ──

    [Test]
    public async Task Reset_all_custom_event_mutates_both_vendors()
    {
        await NavigateAndBoot();

        var statusEcho = Page.Locator("#status-echo");

        // First trigger a native change
        await Page.Locator("#Status").SelectOptionAsync("Active");
        await Expect(statusEcho).ToHaveTextAsync("Status changed", new() { Timeout = 3000 });

        // Reset All — cross-vendor pipeline resets both + updates echo
        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();

        await Expect(statusEcho).ToHaveTextAsync("All fields reset", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Reset_all_clears_native_dropdown_value()
    {
        await NavigateAndBoot();

        var statusSelect = Page.Locator("#Status");
        await statusSelect.SelectOptionAsync("Pending");

        await Page.Locator("button:has-text('Reset All Fields')").ClickAsync();

        await Page.Locator("#status-echo").Filter(new() { HasText = "All fields reset" })
            .WaitForAsync(new() { Timeout = 3000 });

        await Expect(statusSelect).ToHaveValueAsync("");
        AssertNoConsoleErrors();
    }

    // ── Wiring verification ──

    [Test]
    public async Task Boot_trace_shows_component_event_wiring()
    {
        await NavigateAndBoot();

        AssertTraceContains("trigger", "component-event");
        AssertNoConsoleErrors();
    }

    // ── Page structure ──

    [Test]
    public async Task Page_renders_all_sections()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("h1")).ToHaveTextAsync("Playground Syntax");

        await Expect(Page.Locator("h2:has-text('Fusion')")).ToBeVisibleAsync();
        await Expect(Page.Locator("h2:has-text('Native')")).ToBeVisibleAsync();
        await Expect(Page.Locator("h2:has-text('Cross-Vendor')")).ToBeVisibleAsync();
        await Expect(Page.Locator("h2:has-text('Read Properties')")).ToBeVisibleAsync();
        await Expect(Page.Locator("h2:has-text('Nested Properties')")).ToBeVisibleAsync();
        await Expect(Page.Locator("#plan-json")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }
}
