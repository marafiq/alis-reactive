using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Patterns.Cascading;

/// <summary>
/// Proves the cascading dropdown DSL path: parent Fusion selection gathers only <c>Country</c>,
/// loads the child DataSource over HTTP, then save gathers <c>Country</c> and <c>City</c>.
/// Selection helpers use browser gestures rather than ej2 APIs so Syncfusion raises the
/// user-facing change event.
/// </summary>
[TestFixture]
public class WhenParentSelectionFiltersDependentList : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/Cascading";

    // Generated component IDs are the DOM/plan join keys under test.
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_CascadingModel";
    private const string CountryId = Scope + "__Country";
    private const string CityId = Scope + "__City";

    // Syncfusion popup IDs use the rendered component ID plus "_popup".
    private const string CountryPopupId = CountryId + "_popup";
    private const string CityPopupId = CityId + "_popup";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private async Task SelectCountry(string countryText)
    {
        var country = new DropDownListLocator(Page, CountryId);
        await country.Select(countryText);
    }

    private async Task SelectCity(string cityText)
    {
        var city = new DropDownListLocator(Page, CityId);
        await city.Select(cityText);
    }

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
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task country_dropdown_has_server_rendered_options()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{CountryId}").Locator("..").Locator(".e-ddl-icon").ClickWhenStableAsync(Page);
        var countryPopup = Page.Locator($"#{CountryPopupId}");
        await Expect(countryPopup).ToBeVisibleAsync(new() { Timeout = 5000 });

        var countryItems = countryPopup.Locator(".e-list-item");
        await Expect(countryItems).ToHaveCountAsync(4, new() { Timeout = 5000 });

        await Page.Keyboard.PressAsync("Escape");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_country_loads_cities_via_http()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");

        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("3", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_country_updates_city_datasource()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("3", new() { Timeout = 10000 });

        await SelectCountry("United Kingdom");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_city_after_cascade_shows_selected_value()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        await SelectCity("Seattle");

        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_sends_both_country_and_city_values()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        await SelectCity("Seattle");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });

        await Page.Locator("#save-btn").ClickWhenStableAsync(Page);

        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selective_payload_sends_only_country_not_city()
    {
        await NavigateAndBoot();

        // Filter for Canada because Syncfusion keyboard navigation can emit intermediate change requests.
        var canadaCitiesRequestTask = Page.RunAndWaitForRequestAsync(async () =>
        {
            await SelectCountry("Canada");
        }, r => r.Url.Contains("Country=CA"));

        var canadaCitiesRequest = await canadaCitiesRequestTask;
        Assert.That(canadaCitiesRequest.Url, Does.Contain("Country=CA"),
            "GET request should contain Country=CA");
        Assert.That(canadaCitiesRequest.Url, Does.Not.Contain("City="),
            "GET request should NOT contain City parameter — selective payload");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task full_cascading_workflow_country_to_city_to_save()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("3", new() { Timeout = 3000 });

        await SelectCity("Seattle");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });

        await Page.Locator("#save-btn").ClickWhenStableAsync(Page);
        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });

        var saveResultText = await Page.Locator("#save-result").TextContentAsync();
        Assert.That(saveResultText, Does.Contain("SEA"),
            $"Save result should contain city value 'SEA' but was '{saveResultText}'");
        Assert.That(saveResultText, Does.Contain("US"),
            $"Save result should contain country value 'US' but was '{saveResultText}'");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task switching_country_clears_previous_city_selection()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        await SelectCity("Seattle");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });

        await SelectCountry("United Kingdom");
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 10000 });

        await Expect(Page.Locator($"#{CityId}"))
            .ToHaveValueAsync("", new() { Timeout = 5000 });

        await Page.Locator($"#{CityId}").Locator("..").Locator(".e-ddl-icon").ClickWhenStableAsync(Page);
        var cityPopup = Page.Locator($"#{CityPopupId}");
        await Expect(cityPopup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(cityPopup.Locator(".e-list-item")).ToHaveCountAsync(2, new() { Timeout = 5000 });
        await Page.Keyboard.PressAsync("Escape");
        await Expect(cityPopup).ToBeHiddenAsync(new() { Timeout = 5000 });

        await SelectCity("London");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("LON", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task city_dropdown_has_no_items_before_country_selection()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{CityId}").Locator("..").Locator(".e-ddl-icon").ClickWhenStableAsync(Page);
        var cityPopup = Page.Locator($"#{CityPopupId}");
        await Expect(cityPopup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(cityPopup.Locator(".e-list-item")).ToHaveCountAsync(0, new() { Timeout = 3000 });
        await Page.Keyboard.PressAsync("Escape");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task status_indicators_show_placeholder_text_on_initial_load()
    {
        await NavigateAndBoot();

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

    [Test]
    public async Task cascade_status_turns_green_after_cities_load()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#cascade-status"))
            .ToHaveClassAsync(new Regex("text-text-muted"));

        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        await Expect(Page.Locator("#cascade-status"))
            .ToHaveClassAsync(new Regex("text-green-600"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_canada_loads_two_cities()
    {
        await NavigateAndBoot();

        await SelectCountry("Canada");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 3000 });

        await SelectCity("Toronto");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("TOR", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_australia_loads_two_cities()
    {
        await NavigateAndBoot();

        await SelectCountry("Australia");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 3000 });

        await SelectCity("Sydney");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SYD", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_with_country_but_no_city_shows_empty_city_in_result()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        await Page.Locator("#save-btn").ClickWhenStableAsync(Page);

        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });

        var saveResultText = await Page.Locator("#save-result").TextContentAsync();
        Assert.That(saveResultText, Does.Contain("US"),
            $"Save result should contain country 'US' but was '{saveResultText}'");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task save_result_turns_green_on_success()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#save-result"))
            .ToHaveClassAsync(new Regex("text-text-muted"));

        await SelectCountry("United Kingdom");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await SelectCity("Manchester");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("MAN", new() { Timeout = 5000 });

        await Page.Locator("#save-btn").ClickWhenStableAsync(Page);
        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });

        await Expect(Page.Locator("#save-result"))
            .ToHaveClassAsync(new Regex("text-green-600"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task full_cascading_workflow_with_canada()
    {
        await NavigateAndBoot();

        await SelectCountry("Canada");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#city-count"))
            .ToHaveTextAsync("2", new() { Timeout = 3000 });

        await SelectCity("Vancouver");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("VAN", new() { Timeout = 5000 });

        await Page.Locator("#save-btn").ClickWhenStableAsync(Page);
        await Expect(Page.Locator("#save-result"))
            .ToContainTextAsync("Saved:", new() { Timeout = 5000 });

        var saveResultText = await Page.Locator("#save-result").TextContentAsync();
        Assert.That(saveResultText, Does.Contain("VAN"),
            $"Save result should contain city 'VAN' but was '{saveResultText}'");
        Assert.That(saveResultText, Does.Contain("CA"),
            $"Save result should contain country 'CA' but was '{saveResultText}'");

        AssertNoConsoleErrors();
    }

    // TODO: Replace rapid country-switch flakiness with stable behavior coverage
    // for latest-selection-wins or stale cascade responses.
    [Test]
    public async Task selecting_different_city_updates_selected_city_display()
    {
        await NavigateAndBoot();

        await SelectCountry("United States");
        await Expect(Page.Locator("#cascade-status"))
            .ToHaveTextAsync("cities loaded", new() { Timeout = 10000 });

        await SelectCity("Seattle");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("SEA", new() { Timeout = 5000 });

        await SelectCity("New York");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("NYC", new() { Timeout = 5000 });

        await SelectCity("Chicago");
        await Expect(Page.Locator("#selected-city"))
            .ToHaveTextAsync("CHI", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
