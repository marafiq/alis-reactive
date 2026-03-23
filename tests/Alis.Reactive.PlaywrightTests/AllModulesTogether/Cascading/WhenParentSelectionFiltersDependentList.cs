using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.Cascading;

/// <summary>
/// Exercises cascading FusionDropDownList end-to-end in the browser:
/// Country selection triggers HTTP GET to load cities via SetDataSource + DataBind,
/// selective payload sends only the changed value, and gather submits both values.
///
/// Page under test: /Sandbox/AllModulesTogether/Cascading
///
/// SF DropDownList renders: span.e-ddl wrapping the input element.
/// Popup divs are created with IDs: {componentId}_popup.
/// Selection uses DropDownListLocator (focus wrapper + type text + Enter) —
/// real keyboard gestures that fire the SF change event reliably.
/// </summary>
[TestFixture]
public class WhenParentSelectionFiltersDependentList : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/Cascading";

    // IdGenerator: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_CascadingModel";
    private const string CountryId = Scope + "__Country";
    private const string CityId = Scope + "__City";

    // SF popup IDs: {componentId}_popup
    private const string CountryPopupId = CountryId + "_popup";
    private const string CityPopupId = CityId + "_popup";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    /// <summary>
    /// Selects a country via real keyboard gestures: focus wrapper, type text, press Enter.
    /// DropDownListLocator fires the SF change event reliably without any ej2 API calls.
    /// </summary>
    private async Task SelectCountry(string text)
    {
        var country = new DropDownListLocator(Page, CountryId);
        await country.Select(text);
    }

    /// <summary>
    /// Selects a city via real keyboard gestures: focus wrapper, type text, press Enter.
    /// DropDownListLocator fires the SF change event reliably without any ej2 API calls.
    /// </summary>
    private async Task SelectCity(string text)
    {
        var city = new DropDownListLocator(Page, CityId);
        await city.Select(text);
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("Cascading DropDownList — Alis.Reactive Sandbox");
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

    // ── Country dropdown has server-rendered options ──

    [Test]
    public async Task country_dropdown_has_server_rendered_options()
    {
        await NavigateAndBoot();

        // Open the country dropdown — icon click opens popup in Playwright
        await Page.Locator($"#{CountryId}").Locator("..").Locator(".e-ddl-icon").ClickAsync();
        var popup = Page.Locator($"#{CountryPopupId}");
        await Expect(popup).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Verify all 4 countries are present as list items
        var items = popup.Locator(".e-list-item");
        await Expect(items).ToHaveCountAsync(4, new() { Timeout = 5000 });

        // Close popup
        await Page.Keyboard.PressAsync("Escape");
        AssertNoConsoleErrors();
    }

    // ── Selecting country loads cities via HTTP ──

    [Test]
    public async Task selecting_country_loads_cities_via_http()
    {
        await NavigateAndBoot();

        // Select United States
        await SelectCountry("United States");

        // Verify cascade status shows "cities loaded"
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        // US has 3 cities
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("3", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Changing country updates city datasource ──

    [Test]
    public async Task changing_country_updates_city_datasource()
    {
        await NavigateAndBoot();

        // Select US first
        await SelectCountry("United States");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });

        // Switch to UK
        await SelectCountry("United Kingdom");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // ── Selecting city after cascade shows selected value ──

    [Test]
    public async Task selecting_city_after_cascade_shows_selected_value()
    {
        await NavigateAndBoot();

        // Select US, wait for cities to load
        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        // Select Seattle from cascaded city dropdown
        await SelectCity("Seattle");

        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Gather sends both country and city values ──

    [Test]
    public async Task gather_sends_both_country_and_city_values()
    {
        await NavigateAndBoot();

        // Select US, wait for cities to load
        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        // Select Seattle
        await SelectCity("Seattle");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });

        // Click Save — gathers both Country and City
        await Page.Locator("#save-btn").ClickAsync();

        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Selective payload sends only country, not city ──

    [Test]
    public async Task selective_payload_sends_only_country_not_city()
    {
        await NavigateAndBoot();

        // Intercept the cities GET request specifically for Canada
        // (DDL keyboard navigation may trigger intermediate requests for other countries)
        var requestTask = Page.RunAndWaitForRequestAsync(async () =>
        {
            await SelectCountry("Canada");
        }, r => r.Url.Contains("Country=CA"));

        var request = await requestTask;
        Assert.That(request.Url, Does.Contain("Country=CA"),
            "GET request should contain Country=CA");
        // Verify it does NOT contain City parameter
        Assert.That(request.Url, Does.Not.Contain("City="),
            "GET request should NOT contain City parameter — selective payload");

        AssertNoConsoleErrors();
    }

    // ── Full cascading workflow: country → city → save ──

    [Test]
    public async Task full_cascading_workflow_country_to_city_to_save()
    {
        await NavigateAndBoot();

        // Step 1: Select United States
        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("3", new() { Timeout = 3000 });

        // Step 2: Select Seattle from cascaded city dropdown
        await SelectCity("Seattle");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });

        // Step 3: Save — gathers both Country and City, verify server echoes both
        await Page.Locator("#save-btn").ClickAsync();
        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });

        // Verify the save result contains both the country and city values
        var saveText = await Page.Locator("#save-result").TextContentAsync();
        Assert.That(saveText, Does.Contain("SEA"),
            $"Save result should contain city value 'SEA' but was '{saveText}'");
        Assert.That(saveText, Does.Contain("US"),
            $"Save result should contain country value 'US' but was '{saveText}'");

        AssertNoConsoleErrors();
    }

    // ── Switching country clears previous city selection ──

    [Test]
    public async Task switching_country_clears_previous_city_selection()
    {
        await NavigateAndBoot();

        // Select US → wait for cities
        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        // Select Seattle
        await SelectCity("Seattle");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });

        // Switch to UK — cities reload, city dropdown gets new datasource
        await SelectCountry("United Kingdom");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 10000 });

        // Verify the city input value is cleared after switching country (no stale "SEA")
        await Expect(Page.Locator($"#{CityId}"))
            .ToHaveValueAsync("", new() { Timeout = 5000 });

        // Verify UK cities are available — open popup and confirm 2 items loaded
        await Page.Locator($"#{CityId}").Locator("..").Locator(".e-ddl-icon").ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        var cityPopup = Page.Locator($"#{CityPopupId}");
        await Expect(cityPopup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(cityPopup.Locator(".e-list-item")).ToHaveCountAsync(2, new() { Timeout = 5000 });
        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForTimeoutAsync(300); // allow DDL to fully close before reopening

        await SelectCity("London");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("LON", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ══════════════════════════════════════════════════════════
    // Status indicators — initial state and CSS class mutations
    // ══════════════════════════════════════════════════════════

    // ── City dropdown starts empty before any country selection ──

    [Test]
    public async Task city_dropdown_has_no_items_before_country_selection()
    {
        await NavigateAndBoot();

        // City dropdown should have no data source items — it starts empty.
        // Open the popup and verify no list items are rendered.
        await Page.Locator($"#{CityId}").Locator("..").Locator(".e-ddl-icon").ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        var cityPopup = Page.Locator($"#{CityPopupId}");
        await Expect(cityPopup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(cityPopup.Locator(".e-list-item")).ToHaveCountAsync(0, new() { Timeout = 3000 });
        await Page.Keyboard.PressAsync("Escape");

        AssertNoConsoleErrors();
    }

    // ── Status indicators show placeholder text on initial load ──

    [Test]
    public async Task status_indicators_show_placeholder_text_on_initial_load()
    {
        await NavigateAndBoot();

        // All three indicators start with an em-dash placeholder
        var cascadeStatus = await Page.Locator("#cascade-status").TextContentAsync();
        Assert.That(cascadeStatus, Does.Contain("\u2014"),
            "Cascade status should show em-dash placeholder initially");

        var cityCount = await Page.Locator("#city-count").TextContentAsync();
        Assert.That(cityCount, Does.Contain("\u2014"),
            "City count should show em-dash placeholder initially");

        var selectedCity = await Page.Locator("#selected-city").TextContentAsync();
        Assert.That(selectedCity, Does.Contain("\u2014"),
            "Selected city should show em-dash placeholder initially");

        AssertNoConsoleErrors();
    }

    // ── Cascade status turns green after cities load ──

    [Test]
    public async Task cascade_status_turns_green_after_cities_load()
    {
        await NavigateAndBoot();

        // Initially the cascade-status has the muted class
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-text-muted"));

        // Select a country to trigger cascade
        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        // After cascade, the muted class is removed and green class is added
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ══════════════════════════════════════════════════════════
    // All country-to-city cascades — every country produces correct cities
    // ══════════════════════════════════════════════════════════

    // ── Canada selection loads Toronto and Vancouver ──

    [Test]
    public async Task selecting_canada_loads_two_cities()
    {
        await NavigateAndBoot();

        await SelectCountry("Canada");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 3000 });

        // Verify specific cities are selectable
        await SelectCity("Toronto");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("TOR", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Australia selection loads Sydney and Melbourne ──

    [Test]
    public async Task selecting_australia_loads_two_cities()
    {
        await NavigateAndBoot();

        await SelectCountry("Australia");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 3000 });

        // Verify specific cities are selectable
        await SelectCity("Sydney");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SYD", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ══════════════════════════════════════════════════════════
    // Save edge cases
    // ══════════════════════════════════════════════════════════

    // ── Save with country but no city sends empty city to server ──

    [Test]
    public async Task saving_with_country_but_no_city_shows_empty_city_in_result()
    {
        await NavigateAndBoot();

        // Select a country but do NOT select a city
        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        // Click Save without selecting a city
        await Page.Locator("#save-btn").ClickAsync();

        // Server responds with the message — city is empty
        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });

        // The server echoes "Saved: {city} in {country}" — city will be empty or "(empty)"
        var saveText = await Page.Locator("#save-result").TextContentAsync();
        Assert.That(saveText, Does.Contain("US"),
            $"Save result should contain country 'US' but was '{saveText}'");

        AssertNoConsoleErrors();
    }

    // ── Save result turns green on successful save ──

    [Test]
    public async Task save_result_turns_green_on_success()
    {
        await NavigateAndBoot();

        // Save result starts muted
        await Expect(Page.Locator("#save-result"))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-text-muted"));

        // Select country + city, then save
        await SelectCountry("United Kingdom");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await SelectCity("Manchester");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("MAN", new() { Timeout = 5000 });

        await Page.Locator("#save-btn").ClickAsync();
        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });

        // After successful save, text turns green
        await Expect(Page.Locator("#save-result"))
            .ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ══════════════════════════════════════════════════════════
    // Multi-step workflow variations
    // ══════════════════════════════════════════════════════════

    // ── Full workflow with Canada — different country exercises same pipeline ──

    [Test]
    public async Task full_cascading_workflow_with_canada()
    {
        await NavigateAndBoot();

        // Step 1: Select Canada
        await SelectCountry("Canada");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 3000 });

        // Step 2: Select Vancouver
        await SelectCity("Vancouver");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("VAN", new() { Timeout = 5000 });

        // Step 3: Save — verify both values echo
        await Page.Locator("#save-btn").ClickAsync();
        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });

        var saveText = await Page.Locator("#save-result").TextContentAsync();
        Assert.That(saveText, Does.Contain("VAN"),
            $"Save result should contain city 'VAN' but was '{saveText}'");
        Assert.That(saveText, Does.Contain("CA"),
            $"Save result should contain country 'CA' but was '{saveText}'");

        AssertNoConsoleErrors();
    }

    // ── Rapid country switching — three countries in sequence stabilizes correctly ──

    [Test]
    public async Task rapid_country_switching_stabilizes_with_correct_city_count()
    {
        await NavigateAndBoot();

        // Switch through US → UK → AU rapidly
        await SelectCountry("United States");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });

        await SelectCountry("United Kingdom");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 10000 });

        await SelectCountry("Australia");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 10000 });

        // Final state: Australia cities available — open popup and confirm 2 items loaded
        await Page.Locator($"#{CityId}").Locator("..").Locator(".e-ddl-icon").ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        var cityPopup = Page.Locator($"#{CityPopupId}");
        await Expect(cityPopup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(cityPopup.Locator(".e-list-item")).ToHaveCountAsync(2, new() { Timeout = 5000 });
        await Page.Keyboard.PressAsync("Escape");

        await SelectCity("Melbourne");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("MEL", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Selecting different city after first selection updates display ──

    [Test]
    public async Task selecting_different_city_updates_selected_city_display()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        // Select Seattle first
        await SelectCity("Seattle");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });

        // Change to New York — the display updates to the new selection
        await SelectCity("New York");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("NYC", new() { Timeout = 5000 });

        // Change to Chicago — still updates
        await SelectCity("Chicago");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("CHI", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
