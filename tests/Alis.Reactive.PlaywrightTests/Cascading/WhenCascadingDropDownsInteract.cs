namespace Alis.Reactive.PlaywrightTests.Cascading;

/// <summary>
/// Exercises cascading FusionDropDownList end-to-end in the browser:
/// Country selection triggers HTTP GET to load cities via SetDataSource + DataBind,
/// selective payload sends only the changed value, and gather submits both values.
///
/// Page under test: /Sandbox/Cascading
///
/// SF DropDownList renders: span.e-ddl wrapping the input element.
/// Popup divs are created with IDs: {componentId}_popup.
/// To reliably trigger the SF change event, we use showPopup() via ej2 API
/// then click the list item — same pattern as the existing DropDownList tests.
/// </summary>
[TestFixture]
public class WhenCascadingDropDownsInteract : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Cascading";

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
    /// Opens the country dropdown via ej2 showPopup() API, waits for the popup to be
    /// fully open, then clicks the matching list item. Using showPopup() is more reliable
    /// than clicking the wrapper because it guarantees the ej2 popup lifecycle completes.
    /// </summary>
    private async Task SelectCountry(string text)
    {
        // Use ej2 API to open popup (same pattern as existing DropDownList tests)
        await Page.EvaluateAsync($@"() => {{
            const el = document.getElementById('{CountryId}');
            el.ej2_instances[0].showPopup();
        }}");
        var popup = Page.Locator($"#{CountryPopupId}");
        await Expect(popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await popup.Locator(".e-list-item").Filter(new() { HasText = text }).ClickAsync();
    }

    /// <summary>
    /// Opens the city dropdown via ej2 showPopup() API, waits for popup, selects item.
    /// </summary>
    private async Task SelectCity(string text)
    {
        await Page.EvaluateAsync($@"() => {{
            const el = document.getElementById('{CityId}');
            el.ej2_instances[0].showPopup();
        }}");
        var popup = Page.Locator($"#{CityPopupId}");
        await Expect(popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await popup.Locator(".e-list-item").Filter(new() { HasText = text }).ClickAsync();
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

        // Open the country dropdown popup via ej2 API
        await Page.EvaluateAsync($@"() => {{
            const el = document.getElementById('{CountryId}');
            el.ej2_instances[0].showPopup();
        }}");
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

        // Intercept the cities GET request
        var requestTask = Page.RunAndWaitForRequestAsync(async () =>
        {
            await SelectCountry("Canada");
        }, r => r.Url.Contains("/Cities"));

        var request = await requestTask;
        // Verify the request URL contains Country=CA
        Assert.That(request.Url, Does.Contain("Country=CA"),
            "GET request should contain Country=CA");
        // Verify it does NOT contain City parameter
        Assert.That(request.Url, Does.Not.Contain("City="),
            "GET request should NOT contain City parameter — selective payload");

        AssertNoConsoleErrors();
    }
}
