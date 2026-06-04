namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenArrayPluginManipulates : PlaywrightTestBase
{
    private async Task NavigateAndWaitForDomReady()
    {
        await NavigateTo("/Sandbox/Plugins/ArrayManager");
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#arr-total")).Not.ToHaveTextAsync("—", new() { Timeout = 15000 });
    }

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
    public async Task root_function_plugin_shows_slug()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-root-slug"))
            .ToHaveTextAsync("john-doe", new() { Timeout = 5000 });
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
        await Expect(Page.Locator("#arr-echo-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_button_count_shows_active_count()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
        await Expect(Page.Locator("#arr-f-count"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_button_sum_shows_active_age_sum()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
        await Expect(Page.Locator("#arr-f-age-sum"))
            .ToHaveTextAsync("248", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_button_pluck_shows_first_active_name()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
        await Expect(Page.Locator("#arr-f-first"))
            .ToHaveTextAsync("John Doe", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filter_button_some_shows_true()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Filter Active & Inspect" }));
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
        await Expect(Page.Locator("#arr-f-result")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }
}
