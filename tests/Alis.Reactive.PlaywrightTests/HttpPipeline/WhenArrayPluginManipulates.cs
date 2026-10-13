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

    // The memory-care ward has a critical resident: the array plugin's Some(..) result, read in a
    // condition, now holds. An unread result (missing is falsy) would keep "No critical residents".
    [Test]
    public async Task loading_a_ward_with_a_critical_resident_flags_it()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-no-critical")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Load the Memory-Care Ward" }));

        await Expect(Page.Locator("#arr-has-critical")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#arr-no-critical")).ToBeHiddenAsync();
        await Expect(Page.Locator("#arr-total")).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    // The array plugin's count of residents against the community plugin's beds per ward (5): the
    // roster's five fill the ward, the memory-care ward's three leave beds open. A beds value never
    // read would leave every ward "open"; an Eq that ignored its right side would fill every ward.
    [Test]
    public async Task a_ward_whose_residents_fill_every_bed_says_so_and_one_with_room_says_beds_are_open()
    {
        await NavigateAndWaitForDomReady();
        await Expect(Page.Locator("#arr-ward-full")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Load the Memory-Care Ward" }));

        await Expect(Page.Locator("#arr-beds-open")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#arr-ward-full")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // Each literal reaches the plugin as its declared kind: a bool, a long, a DateTime (a Date on the
    // day picked), a decimal and a double. The plugin writes "?" for any argument of another kind.
    [Test]
    public async Task the_visitor_pass_preview_reads_each_detail_as_its_kind()
    {
        await NavigateAndWaitForDomReady();

        await Expect(Page.Locator("#arr-pass-heading")).ToHaveTextAsync("Escorted visitor · pass 4000000001 · 2026-11-14");
        await Expect(Page.Locator("#arr-pass-charges")).ToHaveTextAsync("Parking $12.50 · 1.5 hours");
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
        await Expect(planJson).ToContainTextAsync("$plugin:array");
        await Expect(planJson).ToContainTextAsync("$plugin:analytics");
        AssertNoConsoleErrors();
    }

    // The gathered count is the array plugin's count of the active residents (3 of 5), a value no
    // literal, string length or whole-roster count gives.
    [Test]
    public async Task send_button_echoes_count_value()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Send to Server" }));
        await Expect(Page.Locator("#arr-echo-count"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task send_button_echoes_header_value()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Send to Server" }));
        await Expect(Page.Locator("#arr-echo-header"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task send_button_applies_success_class_and_void_fire_no_errors()
    {
        await NavigateAndWaitForDomReady();
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Send to Server" }));
        await Expect(Page.Locator("#arr-echo-count"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });
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
