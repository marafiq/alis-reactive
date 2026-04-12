namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

/// <summary>
/// Plugin Source vertical slice — exercises all 5 array methods (count, pluck, filter, sum, some)
/// with mixed framework features: DomReady HTTP, conditions (Truthy), gather, headers,
/// nested plugin composition, void .Fire(), and button-click pipelines.
///
/// Server data: 5 residents
///   { id:1, name:"John Doe",       status:"active",     age:82 }
///   { id:2, name:"Jane Smith",     status:"active",     age:75 }
///   { id:3, name:"Bob Johnson",    status:"discharged", age:68 }
///   { id:4, name:"Alice Brown",    status:"active",     age:91 }
///   { id:5, name:"Charlie Wilson", status:"pending",    age:77 }
///
/// Active residents: John Doe (82), Jane Smith (75), Alice Brown (91)
/// </summary>
[TestFixture]
public class WhenArrayPluginManipulates : PlaywrightTestBase
{
    private async Task NavigateAndWaitForDomReady()
    {
        await NavigateTo("/Sandbox/Plugins/ArrayManager");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#arr-total")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

    // ── Section 1: DomReady — count, pluck, filter+count, sum, some+condition ──

    [Test]
    public async Task count_shows_total_residents()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-total"))
            .ToHaveTextAsync("5", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pluck_shows_first_resident_name()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-first-name"))
            .ToHaveTextAsync("John Doe", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_plus_count_shows_active_residents()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-active-count"))
            .ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sum_shows_total_age()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-total-age"))
            .ToHaveTextAsync("393", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task some_condition_shows_no_critical_message()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-no-critical")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#arr-has-critical")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_ready_applies_success_class()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-results")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_renders_plugin_types()
    {
        await NavigateAndWaitForDomReady();
        var planJson = Page.Locator("#plan-json");
        await Expect(planJson).ToBeVisibleAsync();
        await Expect(planJson).ToContainTextAsync("plugin.array");
        await Expect(planJson).ToContainTextAsync("plugin.analytics");
        AssertNoConsoleErrors();
    }

    // ── Section 2: Send button — gather + header with plugin arg propagation ──

    [Test]
    public async Task send_button_echoes_count_value()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Send to Server" }));
        await Expect(Page.Locator("#arr-echo-count"))
            .ToHaveTextAsync("5", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task send_button_echoes_header_value()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Send to Server" }));
        await Expect(Page.Locator("#arr-echo-header"))
            .ToHaveTextAsync("5", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task send_button_applies_success_class_and_void_fire_no_errors()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Send to Server" }));
        await Expect(Page.Locator("#arr-echo-count"))
            .ToHaveTextAsync("5", new() { Timeout = 10000 });
        // Success class proves OnSuccess ran (which includes analytics.track void .Fire())
        await Expect(Page.Locator("#arr-echo-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    // ── Section 3: Filter button — all 5 methods in button HTTP pipeline ──

    [Test]
    public async Task filter_button_count_shows_active_count()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
        // filter(items, "status", "active") → 3 items → count = 3
        await Expect(Page.Locator("#arr-f-count"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_button_sum_shows_active_age_sum()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
        // sum(active, "age") = 82 + 75 + 91 = 248
        await Expect(Page.Locator("#arr-f-age-sum"))
            .ToHaveTextAsync("248", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_button_pluck_shows_first_active_name()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
        // pluck(active, 0, "name") = "John Doe"
        await Expect(Page.Locator("#arr-f-first"))
            .ToHaveTextAsync("John Doe", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_button_some_shows_true()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
        // some(active, "status", "active") = true
        await Expect(Page.Locator("#arr-f-some"))
            .ToHaveTextAsync("true", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_button_applies_success_class_and_void_fire_no_errors()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
        await Expect(Page.Locator("#arr-f-count"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });
        // Success class proves pipeline complete (includes analytics.track void .Fire())
        await Expect(Page.Locator("#arr-f-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }
}
