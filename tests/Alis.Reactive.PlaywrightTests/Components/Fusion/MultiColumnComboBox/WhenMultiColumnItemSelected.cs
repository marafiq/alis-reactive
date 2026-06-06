using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.MultiColumnComboBox;

[TestFixture]
public class WhenMultiColumnItemSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/MultiColumnComboBox";

    // Generated component IDs are the DOM/Reactive Plan join keys under test.
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_MultiColumnComboBoxModel";
    private const string FacilityId = GeneratedTypeScope + "__Facility";

    private MultiColumnComboBoxLocator Facility => new(Page, FacilityId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("MultiColumnComboBox — Alis.Reactive Sandbox");
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
    public async Task domready_sets_initial_value()
    {
        await NavigateAndBoot();
        await Expect(Facility.Input).ToBeVisibleAsync();

        await Expect(Facility.Input).Not.ToHaveValueAsync("", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var valueEcho = Page.Locator("#value-echo");
        await Expect(valueEcho).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var valueEchoText = await valueEcho.TextContentAsync();
        Assert.That(valueEchoText, Does.Contain("1"),
            "Value echo should contain 1 after dom-ready property read");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showpopup_button_opens_dropdown()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();

        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidepopup_button_closes_dropdown()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#hide-popup-btn").ClickAsync();

        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_displays_new_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Lakeside Care" }).ClickAsync();

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var changeText = await Page.Locator("#change-value").TextContentAsync();
        Assert.That(changeText, Does.Contain("2"),
            $"Change value should contain 2 but was '{changeText}'");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_equals_sunrise()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Sunrise Manor" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("sunrise manor selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_falls_to_else_for_other_values()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Meadow Ridge" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("other facility", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Harbor View" }).ClickAsync();

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
    public async Task gather_sends_initial_facility_value_to_server()
    {
        await NavigateAndBoot();

        var gatherRequest = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/MultiColumnComboBox/Echo");

        var requestBody = gatherRequest.PostData ?? "";
        Assert.That(requestBody, Does.Contain("1"),
            $"Gather POST body must contain the initial value '1' but was '{requestBody}'");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_facility_then_gathering_sends_new_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Lakeside Care" }).ClickAsync();

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        var gatherRequest = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/MultiColumnComboBox/Echo");

        var requestBody = gatherRequest.PostData ?? "";
        Assert.That(requestBody, Does.Contain("\"2\""),
            $"Gather must send the current value '2' but body was '{requestBody}'");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task columns_display_text_city_and_capacity()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        var columnHeaders = Page.Locator(".e-multicolumn-list.e-popup .e-headercell");
        await Expect(columnHeaders).ToHaveCountAsync(3, new() { Timeout = 5000 });

        // Syncfusion appends accessibility text, so match the visible header names by substring.
        var headerTexts = await columnHeaders.AllTextContentsAsync();
        Assert.That(headerTexts.Any(h => h.Contains("Name")), Is.True,
            "Column headers must include 'Name'");
        Assert.That(headerTexts.Any(h => h.Contains("City")), Is.True,
            "Column headers must include 'City'");
        Assert.That(headerTexts.Any(h => h.Contains("Capacity")), Is.True,
            "Column headers must include 'Capacity'");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_then_changing_fires_condition_each_time()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Sunrise Manor" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("sunrise manor selected", new() { Timeout = 5000 });

        // Syncfusion leaves the closing popup in the DOM; wait until it is hidden before re-opening.
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Lakeside Care" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("other facility", new() { Timeout = 5000 });

        // Syncfusion leaves the closing popup in the DOM; wait until it is hidden before re-opening.
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Sunrise Manor" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("sunrise manor selected", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
