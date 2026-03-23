namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionDropDownList API end-to-end in the browser:
/// property writes, method calls, property reads, events, conditions, and gather.
///
/// Page under test: /Sandbox/Components/DropDownList
///
/// Syncfusion DropDownList renders an input element inside the wrapper div.
/// The wrapper element gets the IdGenerator-based ID; the visible input is a child.
/// Playwright interacts with the wrapper; the ej2 instance fires events.
/// </summary>
[TestFixture]
public class WhenDropdownItemSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DropDownList";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DropDownListModel";
    private const string CategoryId = Scope + "__Category";

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
        await Expect(Page).ToHaveTitleAsync("DropDownList — Alis.Reactive Sandbox");
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
        // SF DropDownList wrapper gets the IdGenerator-based ID
        var wrapper = Page.Locator($"#{CategoryId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // Wait for the value to be set by dom-ready via ej2 instance
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{CategoryId}'); return el && el.ej2_instances && el.ej2_instances[0] && el.ej2_instances[0].value === 'Books'; }}",
            null,
            new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        // The value-echo should show "Books" after dom-ready reads comp.Value()
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var text = await echo.TextContentAsync();
        Assert.That(text, Does.Contain("Books"),
            "Value echo should contain Books after dom-ready property read");
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

        // Click on "Electronics" in the popup list
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Electronics" }).ClickAsync();

        // SF change event payload contains the selected value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var changeText = await Page.Locator("#change-value").TextContentAsync();
        Assert.That(changeText, Does.Contain("Electronics"),
            $"Change value should contain Electronics but was '{changeText}'");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focus_event_shows_focus_state()
    {
        await NavigateAndBoot();

        // SF DropDownList renders: <span class="e-ddl" tabindex="0"> wrapping <input id="CategoryId" tabindex="-1">.
        // The wrapper span intercepts pointer events. Click the wrapper to trigger SF focus.
        var wrapper = Page.Locator($"span.e-ddl:has(#{CategoryId})");
        await wrapper.ClickAsync();

        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("focused", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task blur_event_shows_blur_state()
    {
        await NavigateAndBoot();

        // Click the wrapper span to trigger focus
        var wrapper = Page.Locator($"span.e-ddl:has(#{CategoryId})");
        await wrapper.ClickAsync();
        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("focused", new() { Timeout = 5000 });

        // Press Escape to close popup then Tab out to trigger blur
        await Page.Keyboard.PressAsync("Escape");
        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 5: Conditions — Typed Event-Args + Component-Read ──

    [Test]
    public async Task event_args_condition_matches_when_value_equals_electronics()
    {
        await NavigateAndBoot();

        // Open popup and select Electronics
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Electronics" }).ClickAsync();

        // When(args, x => x.Value).Eq("Electronics") → Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("electronics selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_falls_to_else_for_other_values()
    {
        await NavigateAndBoot();

        // Open popup and select Clothing (not Electronics)
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Clothing" }).ClickAsync();

        // When(args, x => x.Value).Eq("Electronics") → Else branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("other category", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Selected indicator starts hidden (no selection yet from change event)
        // Note: DomReady sets "Books" via SetValue but the condition is wired to Changed event
        // We need to trigger a change event by selecting an item

        // Open popup and select an item
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Clothing" }).ClickAsync();

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

    // ── Multi-step state-cycle scenarios ──

    [Test]
    public async Task selecting_then_clearing_selection_toggles_indicator()
    {
        // Proves When(comp.Value()).NotNull() evaluates correctly across
        // select → clear → re-select transitions.
        // DomReady SetValue("Books") fires the change event, so the indicator
        // is already visible after boot.
        await NavigateAndBoot();

        // Indicator is visible after boot (DomReady SetValue triggers change → NotNull → show)
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });

        // Select "Electronics" → indicator stays visible (still not null)
        await Page.Locator("#show-popup-btn").ClickAsync();
        var popup = Page.Locator(".e-ddl.e-popup");
        await Expect(popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await popup.Locator(".e-list-item").Filter(new() { HasText = "Electronics" }).ClickAsync();
        // Wait for popup to close after selection before asserting
        await Expect(popup).ToBeHiddenAsync(new() { Timeout = 3000 });

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });

        // Clear selection via SF ej2 API → indicator hides
        await Page.EvaluateAsync(@$"() => {{
            const el = document.getElementById('{CategoryId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = null;
            ej2.text = null;
            ej2.dataBind();
            ej2.trigger('change', {{ value: null, itemData: null, isInteracted: false }});
        }}");

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        // Select "Books" → indicator shows again
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await popup.Locator(".e-list-item").Filter(new() { HasText = "Books" }).ClickAsync();
        await Expect(popup).ToBeHiddenAsync(new() { Timeout = 3000 });

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
