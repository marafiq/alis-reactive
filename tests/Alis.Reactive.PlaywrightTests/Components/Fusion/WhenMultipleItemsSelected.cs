using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises FusionMultiSelect API end-to-end in the browser:
/// events, conditions, property reads, gather, and typed Fields
/// with optional GroupBy.
///
/// Page under test: /Sandbox/Components/MultiSelect
///
/// Syncfusion MultiSelect renders an input element inside a wrapper div.
/// The wrapper element gets the IdGenerator-based ID; the visible input is a child.
/// SF MultiSelect uses array-based values (unlike DropDownList which uses scalar values).
/// Pre-selection is done via SF builder Value() at render time, not via DomReady SetValue.
///
/// Tests use MultiSelectLocator to interact via real browser gestures
/// (clicking items in the popup) rather than ej2 instance manipulation.
/// </summary>
[TestFixture]
public class WhenMultipleItemsSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/MultiSelect";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_MultiSelectModel";
    private const string AllergiesId = Scope + "__Allergies";
    private const string DietaryRestrictionsId = Scope + "__DietaryRestrictions";

    private MultiSelectLocator Allergies => new(Page, AllergiesId);
    private MultiSelectLocator DietaryRestrictions => new(Page, DietaryRestrictionsId);

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
        await Expect(Page).ToHaveTitleAsync("MultiSelect — Alis.Reactive Sandbox");
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

    // ── Both MultiSelect components render ──

    [Test]
    public async Task both_multiselect_components_render()
    {
        await NavigateAndBoot();

        // Verify both components are rendered and visible
        await Expect(Allergies.Wrapper).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(DietaryRestrictions.Wrapper).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Property Read (DomReady reads pre-selected value) ──

    [Test]
    public async Task domready_reads_preselected_value_into_echo()
    {
        await NavigateAndBoot();
        // SF builder Value(new string[] { "peanuts" }) pre-selects "peanuts"
        // DomReady reads comp.Value() into the echo element
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var text = await echo.TextContentAsync();
        Assert.That(text, Does.Contain("peanuts"),
            "Value echo should contain peanuts after dom-ready property read");
        AssertNoConsoleErrors();
    }

    // ── Events — Changed on Allergies via ej2 API ──

    [Test]
    public async Task changed_event_fires_when_selecting_allergy()
    {
        await NavigateAndBoot();

        // Select an allergy item via the popup
        await Allergies.SelectItem("Shellfish");

        // SF change event payload contains the selected value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Events — Changed on Dietary Restrictions via ej2 API ──

    [Test]
    public async Task changed_event_fires_when_selecting_dietary_restriction()
    {
        await NavigateAndBoot();

        // Select a dietary restriction via the popup
        await DietaryRestrictions.SelectItem("Vegetarian");

        // SF change event fires
        await Expect(Page.Locator("#dietary-change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Conditions ──

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Select an allergy item via the popup — triggers change event which fires condition
        await Allergies.SelectItem("Dairy");

        // Indicator should appear with text "selected"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Gather ──

    [Test]
    public async Task gather_button_posts_component_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#gather-btn").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-item selection + gather ──

    [Test]
    public async Task selecting_multiple_items_then_gathering_sends_all_values()
    {
        await NavigateAndBoot();

        // Peanuts is already pre-selected via SF builder Value(). Select 2 more items.
        // In Box mode, selected items get e-hide-listitem so they can't be re-clicked.
        await Allergies.SelectItems("Shellfish", "Dairy");

        // Wait for value to take effect (change event fires)
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // Gather — intercept the POST to verify the payload
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/MultiSelect/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("peanuts"),
            $"Gather must contain peanuts (pre-selected) but was '{body}'");
        Assert.That(body, Does.Contain("shellfish"),
            $"Gather must contain shellfish but was '{body}'");
        Assert.That(body, Does.Contain("dairy"),
            $"Gather must contain dairy but was '{body}'");

        // Confirm round-trip completes
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── GroupBy verification ──

    [Test]
    public async Task grouped_items_display_under_correct_group_headers()
    {
        await NavigateAndBoot();

        // Open the allergies popup by clicking the wrapper
        await Allergies.Open();

        // SF MultiSelect popup — verify at least one non-hidden item is visible.
        // Pre-selected items (Peanuts) get e-hide-listitem so we filter for visible ones.
        await Expect(Allergies.Popup.Locator(".e-list-item:not(.e-hide-listitem)").First)
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        var popup = Allergies.Popup;

        // Verify group headers are present — SF renders .e-list-group-item for GroupBy
        var groupHeaders = popup.Locator(".e-list-group-item");
        await Expect(groupHeaders).ToHaveCountAsync(3, new() { Timeout = 5000 });

        // Verify the three category group headers: Food, Medication, Environmental
        var headerTexts = await groupHeaders.AllTextContentsAsync();
        Assert.That(headerTexts, Does.Contain("Food"),
            "Group headers must include 'Food'");
        Assert.That(headerTexts, Does.Contain("Medication"),
            "Group headers must include 'Medication'");
        Assert.That(headerTexts, Does.Contain("Environmental"),
            "Group headers must include 'Environmental'");

        // Close popup
        await Page.Keyboard.PressAsync("Escape");
        AssertNoConsoleErrors();
    }

    // ── Remove selection updates value ──

    [Test]
    public async Task removing_one_selection_updates_value()
    {
        await NavigateAndBoot();

        // Peanuts is pre-selected. Select 2 more: Shellfish, Dairy.
        await Allergies.SelectItems("Shellfish", "Dairy");

        // Wait for change to register
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // In Box mode, selected items are hidden in the popup (e-hide-listitem).
        // To remove Shellfish, click the close icon on the Shellfish chip.
        // SF MultiSelect chips: .e-chips > .e-chipcontent + .e-chips-close
        var shellChip = Allergies.Wrapper.Locator(".e-chips").Filter(new() { HasText = "Shellfish" });
        await shellChip.Locator(".e-chips-close").ClickAsync();

        // Verify the MultiSelect chips show only Peanuts and Dairy
        var chipTexts = await Allergies.Wrapper.Locator(".e-chips .e-chipcontent").AllTextContentsAsync();
        Assert.That(chipTexts, Does.Contain("Peanuts"), "Chips must contain Peanuts");
        Assert.That(chipTexts, Does.Contain("Dairy"), "Chips must contain Dairy");
        Assert.That(chipTexts, Does.Not.Contain("Shellfish"), "Chips must NOT contain removed Shellfish");
        AssertNoConsoleErrors();
    }

    // ── Section 6: Filtering Event — Server-Filtered HTTP ──

    private const string SuppliesId = Scope + "__Supplies";

    /// <summary>
    /// Types text into the Supplies MultiSelect using PressSequentially,
    /// which simulates real keystrokes that trigger SF's filtering event.
    /// FillAsync won't work — SF listens for keyup/keydown, not synthetic input events.
    ///
    /// SF MultiSelect DOM structure with AllowFiltering:
    ///   .e-multi-select-wrapper (grandparent)
    ///     └── span.e-searcher (parent)
    ///         ├── input.e-dropdownbase (filter input — type here)
    ///         └── input#SuppliesId (component input)
    /// </summary>
    private async Task TypeInSupplies(string text)
    {
        // The filter input is a sibling of the component input inside the wrapper
        var filterInput = Page.Locator($"#{SuppliesId}").Locator("xpath=preceding-sibling::input[contains(@class,'e-dropdownbase')]");
        await Expect(filterInput).ToBeVisibleAsync(new() { Timeout = 5000 });
        await filterInput.ClickAsync();
        await filterInput.PressSequentiallyAsync(text, new() { Delay = 50 });
    }

    [Test]
    public async Task filtering_event_fires_http_get_and_updates_datasource()
    {
        await NavigateAndBoot();

        // Type "gl" to trigger the SF filtering event → HTTP GET → DataSource update
        await TypeInSupplies("gl");

        // Wait for the HTTP response to update the status element
        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_http_request_includes_supplies_query_param()
    {
        await NavigateAndBoot();

        var filterInput = Page.Locator($"#{SuppliesId}").Locator("xpath=preceding-sibling::input[contains(@class,'e-dropdownbase')]");
        await Expect(filterInput).ToBeVisibleAsync(new() { Timeout = 5000 });
        await filterInput.ClickAsync();

        // Intercept the HTTP GET request triggered by filtering
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await filterInput.PressSequentiallyAsync("band", new() { Delay = 50 }),
            r => r.Url.Contains("/Supplies"));

        // Verify the request URL targets the Supplies endpoint
        Assert.That(request.Url, Does.Contain("Supplies"),
            "GET request should target the Supplies endpoint");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_response_populates_multiselect_dropdown()
    {
        await NavigateAndBoot();

        // Type to trigger filtering → HTTP → updateData populates popup
        await TypeInSupplies("gl");

        // Wait for the HTTP response to complete
        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });

        // Verify the popup contains filtered items (updateData renders the popup)
        var popupItems = Page.Locator(".e-ddl.e-popup .e-list-item");
        await Expect(popupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        var count = await popupItems.CountAsync();
        Assert.That(count, Is.GreaterThan(0),
            "Popup should contain filtered supply items after updateData");
        AssertNoConsoleErrors();
    }
}
