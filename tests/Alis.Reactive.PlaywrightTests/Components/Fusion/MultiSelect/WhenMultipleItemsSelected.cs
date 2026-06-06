using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.MultiSelect;

// Tests use real popup gestures through MultiSelectLocator rather than direct
// EJ2 instance manipulation.
[TestFixture]
public class WhenMultipleItemsSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/MultiSelect";

    // Controlled component IDs follow IdGenerator's type-scope + property convention.
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_MultiSelectModel";
    private const string AllergiesId = GeneratedTypeScope + "__Allergies";
    private const string DietaryRestrictionsId = GeneratedTypeScope + "__DietaryRestrictions";

    private MultiSelectLocator Allergies => new(Page, AllergiesId);
    private MultiSelectLocator DietaryRestrictions => new(Page, DietaryRestrictionsId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

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
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task both_multiselect_components_render()
    {
        await NavigateAndBoot();

        await Expect(Allergies.Wrapper).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(DietaryRestrictions.Wrapper).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_preselected_value_into_echo()
    {
        await NavigateAndBoot();

        // Builder Value(new[] { "peanuts" }) preselects before DomReady reads comp.Value().
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var text = await echo.TextContentAsync();
        Assert.That(text, Does.Contain("peanuts"),
            "Value echo should contain peanuts after dom-ready property read");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_fires_when_selecting_allergy()
    {
        await NavigateAndBoot();

        await Allergies.SelectItem("Shellfish");

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_fires_when_selecting_dietary_restriction()
    {
        await NavigateAndBoot();

        await DietaryRestrictions.SelectItem("Vegetarian");

        await Expect(Page.Locator("#dietary-change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        await Allergies.SelectItem("Dairy");

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

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
    public async Task selecting_multiple_items_then_gathering_sends_all_values()
    {
        await NavigateAndBoot();

        // Peanuts is already selected by the builder. Box mode hides selected popup items
        // with e-hide-listitem, so only new values can be clicked.
        await Allergies.SelectItems("Shellfish", "Dairy");

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/MultiSelect/Echo");

        var requestBody = request.PostData ?? "";
        Assert.That(requestBody, Does.Contain("peanuts"),
            $"Gather must contain peanuts (pre-selected) but was '{requestBody}'");
        Assert.That(requestBody, Does.Contain("shellfish"),
            $"Gather must contain shellfish but was '{requestBody}'");
        Assert.That(requestBody, Does.Contain("dairy"),
            $"Gather must contain dairy but was '{requestBody}'");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task grouped_items_display_under_correct_group_headers()
    {
        await NavigateAndBoot();

        await Allergies.Open();

        // Preselected items get e-hide-listitem in the popup; visible items prove
        // there is still selectable content after builder preselection.
        await Expect(Allergies.Popup.Locator(".e-list-item:not(.e-hide-listitem)").First)
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        var allergiesPopup = Allergies.Popup;

        // Syncfusion renders GroupBy headers as .e-list-group-item.
        var groupHeaders = allergiesPopup.Locator(".e-list-group-item");
        await Expect(groupHeaders).ToHaveCountAsync(3, new() { Timeout = 5000 });

        var headerTexts = await groupHeaders.AllTextContentsAsync();
        Assert.That(headerTexts, Does.Contain("Food"),
            "Group headers must include 'Food'");
        Assert.That(headerTexts, Does.Contain("Medication"),
            "Group headers must include 'Medication'");
        Assert.That(headerTexts, Does.Contain("Environmental"),
            "Group headers must include 'Environmental'");

        await Page.Keyboard.PressAsync("Escape");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task removing_one_selection_updates_value()
    {
        await NavigateAndBoot();

        await Allergies.SelectItems("Shellfish", "Dairy");

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // In Box mode, selected items are hidden in the popup (e-hide-listitem).
        // Removal must use the chip close icon instead of the popup item.
        var shellChip = Allergies.Wrapper.Locator(".e-chips").Filter(new() { HasText = "Shellfish" });
        await shellChip.Locator(".e-chips-close").ClickAsync();

        var selectedChipTexts = await Allergies.Wrapper.Locator(".e-chips .e-chipcontent").AllTextContentsAsync();
        Assert.That(selectedChipTexts, Does.Contain("Peanuts"), "Chips must contain Peanuts");
        Assert.That(selectedChipTexts, Does.Contain("Dairy"), "Chips must contain Dairy");
        Assert.That(selectedChipTexts, Does.Not.Contain("Shellfish"), "Chips must NOT contain removed Shellfish");
        AssertNoConsoleErrors();
    }

    private const string SuppliesId = GeneratedTypeScope + "__Supplies";

    /// <summary>
    /// Types real keystrokes into the Syncfusion filtering input. FillAsync does not
    /// trigger the filtering event because Syncfusion listens to keyboard events.
    /// </summary>
    private async Task TypeInSupplies(string searchText)
    {
        // Filtering input is a sibling of the generated component input.
        var filterInput = Page.Locator($"#{SuppliesId}").Locator("xpath=preceding-sibling::input[contains(@class,'e-dropdownbase')]");
        await Expect(filterInput).ToBeVisibleAsync(new() { Timeout = 5000 });
        await filterInput.ClickAsync();
        await filterInput.PressSequentiallyAsync(searchText, new() { Delay = 50 });
    }

    [Test]
    public async Task filtering_event_fires_http_get_and_updates_datasource()
    {
        await NavigateAndBoot();

        await TypeInSupplies("gl");

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

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await filterInput.PressSequentiallyAsync("band", new() { Delay = 50 }),
            r => r.Url.Contains("/Supplies"));

        Assert.That(request.Url, Does.Contain("Supplies"),
            "GET request should target the Supplies endpoint");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_response_populates_multiselect_dropdown()
    {
        await NavigateAndBoot();

        await TypeInSupplies("gl");

        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });

        var popupItems = Page.Locator(".e-ddl.e-popup .e-list-item");
        await Expect(popupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        var popupItemCount = await popupItems.CountAsync();
        Assert.That(popupItemCount, Is.GreaterThan(0),
            "Popup should contain filtered supply items after updateData");
        AssertNoConsoleErrors();
    }
}
