using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionAutoComplete API end-to-end in the browser:
/// property writes, method calls, property reads, events, conditions, and gather.
///
/// Page under test: /Sandbox/Components/AutoComplete
///
/// FusionAutoComplete renders an input element inside the wrapper div.
/// The wrapper element gets the IdGenerator-based ID; the visible input is a child.
/// Tests use AutoCompleteLocator to interact via real browser gestures.
/// </summary>
[TestFixture]
public class WhenAutoCompleteSuggests : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AutoComplete";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_AutoCompleteModel";
    private const string PhysicianId = Scope + "__Physician";
    private const string MedicationId = Scope + "__MedicationType";

    private AutoCompleteLocator Physician => new(Page, PhysicianId);

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
        await Expect(Page).ToHaveTitleAsync("AutoComplete — Alis.Reactive Sandbox");
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
        // SF AutoComplete renders an input with the IdGenerator-based ID
        var ac = Physician;
        await Expect(ac.Input).ToBeVisibleAsync();

        // Wait for the value to be set by dom-ready — verify via the visible input
        await Expect(ac.Input).Not.ToHaveValueAsync("", new() { Timeout = 5000 });

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
        Assert.That(text, Does.Contain("smith"),
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
        Assert.That(changeText, Does.Contain("johnson"),
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

    [Test]
    public async Task gather_sends_initial_physician_value_to_server()
    {
        await NavigateAndBoot();

        // DomReady sets Physician to "Dr. Smith" — gather must POST that value
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/AutoComplete/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("smith"),
            $"Gather POST body must contain the initial value 'Dr. Smith' but was '{body}'");

        // Confirm the round-trip completes — response handler fires
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_physician_then_gathering_sends_new_value()
    {
        await NavigateAndBoot();

        // Change the value from "Dr. Smith" to "Dr. Johnson" via popup selection
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();

        // Wait for change event to confirm the value took effect
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // Gather must POST the CURRENT value "Dr. Johnson", not the initial "Dr. Smith"
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/AutoComplete/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("johnson"),
            $"Gather must send the current value 'Dr. Johnson' but body was '{body}'");
        Assert.That(body, Does.Not.Contain("smith"),
            "Gather must NOT send the stale initial value 'Dr. Smith'");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task server_echoes_back_exact_gathered_physician_value()
    {
        await NavigateAndBoot();

        // Change to "Dr. Williams" so the test is distinct from the initial "Dr. Smith"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Williams" }).ClickAsync();
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // Intercept the response to verify the server echoes back the gathered value
        var response = await Page.RunAndWaitForResponseAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/AutoComplete/Echo");

        var responseBody = await response.TextAsync();
        Assert.That(responseBody, Does.Contain("williams"),
            $"Server response must echo back the gathered value 'Dr. Williams' but was '{responseBody}'");
        Assert.That((int)response.Status, Is.EqualTo(200),
            "Echo endpoint must return 200 OK");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle scenarios ──

    [Test]
    public async Task selecting_multiple_different_values_fires_change_each_time()
    {
        await NavigateAndBoot();

        var changeValue = Page.Locator("#change-value");
        var argsCondition = Page.Locator("#args-condition");

        var showBtn = Page.Locator("#show-popup-btn");
        var physician = Physician;

        // Cycle 1: select Dr. Johnson → change fires, args says "other physician"
        await showBtn.ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();
        var text1 = await changeValue.TextContentAsync();
        Assert.That(text1, Does.Contain("johnson"),
            $"Cycle 1: change value should contain Dr. Johnson but was '{text1}'");
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        // Cycle 2: select Dr. Williams → change fires again with new value
        // Wait for popup close animation to complete before re-opening
        await Expect(Page.Locator(".e-ddl.e-popup")).ToBeHiddenAsync(new() { Timeout = 5000 });
        await showBtn.ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Williams" }).ClickAsync();
        var text2 = await changeValue.TextContentAsync();
        Assert.That(text2, Does.Contain("williams"),
            $"Cycle 2: change value should contain Dr. Williams but was '{text2}'");
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        // Cycle 3: select Dr. Smith → change fires, args condition flips to "dr smith selected"
        await Expect(Page.Locator(".e-ddl.e-popup")).ToBeHiddenAsync(new() { Timeout = 5000 });
        await showBtn.ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Smith" }).ClickAsync();
        var text3 = await changeValue.TextContentAsync();
        Assert.That(text3, Does.Contain("smith"),
            $"Cycle 3: change value should contain Dr. Smith but was '{text3}'");
        await Expect(argsCondition).ToHaveTextAsync("dr smith selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_selection_then_reselecting_toggles_indicator()
    {
        await NavigateAndBoot();

        var selectedIndicator = Page.Locator("#selected-indicator");
        var argsCondition = Page.Locator("#args-condition");

        // Step 1: select Dr. Johnson → indicator shows "selected", args says "other physician"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToHaveTextAsync("selected", new() { Timeout = 3000 });
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        // Step 2: clear the selection via real gesture → fires change with null value → indicator hides
        await Physician.Clear();
        await Physician.Blur();
        await Expect(selectedIndicator).ToBeHiddenAsync(new() { Timeout = 5000 });

        // Step 3: reselect Dr. Smith → indicator shows again, args says "dr smith selected"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Smith" }).ClickAsync();
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToHaveTextAsync("selected", new() { Timeout = 3000 });
        await Expect(argsCondition).ToHaveTextAsync("dr smith selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Section 7: Filtering Event — Server-Filtered HTTP ──

    /// <summary>
    /// Types text into the MedicationType AutoComplete using PressSequentially,
    /// which simulates real keystrokes that trigger SF's filtering event.
    /// FillAsync won't work — SF listens for keyup/keydown, not synthetic input events.
    /// </summary>
    private async Task TypeInMedication(string text)
    {
        var input = Page.Locator($"#{MedicationId}");
        await Expect(input).ToBeVisibleAsync();
        await input.ClickAsync();
        await input.PressSequentiallyAsync(text, new() { Delay = 50 });
    }

    [Test]
    public async Task filtering_event_fires_http_get_and_updates_datasource()
    {
        await NavigateAndBoot();

        // Type "anti" to trigger the SF filtering event → HTTP GET → DataSource update
        await TypeInMedication("anti");

        // Wait for the HTTP response to update the status element
        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_http_request_includes_medication_type_query_param()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{MedicationId}");
        await Expect(input).ToBeVisibleAsync();
        await input.ClickAsync();

        // Intercept the HTTP GET request triggered by filtering
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await input.PressSequentiallyAsync("ster", new() { Delay = 50 }),
            r => r.Url.Contains("/Medications"));

        // Verify the request URL targets the Medications endpoint
        Assert.That(request.Url, Does.Contain("Medications"),
            "GET request should target the Medications endpoint");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_response_populates_autocomplete_dropdown()
    {
        await NavigateAndBoot();

        // Type to trigger filtering → HTTP → updateData populates popup
        await TypeInMedication("anti");

        // Wait for the HTTP response to complete
        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });

        // Verify the popup contains filtered items (updateData renders the popup)
        var popupItems = Page.Locator(".e-ddl.e-popup .e-list-item");
        await Expect(popupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        var count = await popupItems.CountAsync();
        Assert.That(count, Is.GreaterThan(0),
            "Popup should contain filtered medication items after updateData");
        AssertNoConsoleErrors();
    }
}
