namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionComboBox API end-to-end in the browser:
/// property writes, method calls, property reads, events, conditions, and gather.
///
/// Page under test: /Sandbox/ComboBox
///
/// Syncfusion AutoComplete (ComboBox) renders an input element inside the wrapper div.
/// The wrapper element gets the IdGenerator-based ID; the visible input is a child.
/// Playwright interacts with the wrapper; the ej2 instance fires events.
/// </summary>
[TestFixture]
public class WhenUsingFusionComboBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ComboBox";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ComboBoxModel";
    private const string PhysicianId = Scope + "__Physician";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("ComboBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write ──

    [Test]
    public async Task domready_sets_initial_value()
    {
        await NavigateAndBoot();
        // SF AutoComplete wrapper gets the IdGenerator-based ID
        var wrapper = Page.Locator($"#{PhysicianId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // Wait for the value to be set by dom-ready via ej2 instance
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{PhysicianId}'); return el && el.ej2_instances && el.ej2_instances[0] && el.ej2_instances[0].value === 'Dr. Smith'; }}",
            null,
            new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        // The value-echo should show "Dr. Smith" after dom-ready reads comp.Value()
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var text = await echo.TextContentAsync();
        Assert.That(text, Does.Contain("Dr. Smith"),
            "Value echo should contain Dr. Smith after dom-ready property read");
        AssertNoConsoleErrors();
    }

    // ── Section 3: Method Calls (ShowPopup/HidePopup) ──

    [Test]
    public async Task showpopup_button_opens_dropdown()
    {
        await NavigateAndBoot();

        // Click ShowPopup button
        await Page.Locator("#show-popup-btn").ClickAsync();

        // The SF popup list should be visible
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidepopup_button_closes_dropdown()
    {
        await NavigateAndBoot();

        // Open popup first
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click HidePopup button
        await Page.Locator("#hide-popup-btn").ClickAsync();

        // The SF popup list should be hidden
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Events ──

    [Test]
    public async Task changed_event_displays_new_value()
    {
        await NavigateAndBoot();

        // Open the dropdown and select an item
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click on "Dr. Johnson" in the popup list
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();

        // SF change event payload contains the selected value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var changeText = await Page.Locator("#change-value").TextContentAsync();
        Assert.That(changeText, Does.Contain("Dr. Johnson"),
            $"Change value should contain Dr. Johnson but was '{changeText}'");
        AssertNoConsoleErrors();
    }

    // ── Section 5: Conditions — Typed Event-Args + Component-Read ──

    [Test]
    public async Task event_args_condition_matches_when_value_equals_dr_smith()
    {
        await NavigateAndBoot();

        // Open popup and select Dr. Smith
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Smith" }).ClickAsync();

        // When(args, x => x.Value).Eq("Dr. Smith") -> Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("dr smith selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_falls_to_else_for_other_values()
    {
        await NavigateAndBoot();

        // Open popup and select Dr. Johnson (not Dr. Smith)
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();

        // When(args, x => x.Value).Eq("Dr. Smith") -> Else branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("other physician", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Open popup and select an item
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Williams" }).ClickAsync();

        // Indicator should appear with text "selected"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 6: Gather ──

    [Test]
    public async Task gather_button_posts_component_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
