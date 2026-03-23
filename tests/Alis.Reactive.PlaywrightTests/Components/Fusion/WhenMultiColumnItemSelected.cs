namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionMultiColumnComboBox API end-to-end in the browser:
/// property writes, method calls, property reads, events, conditions, and gather.
///
/// Page under test: /Sandbox/Components/MultiColumnComboBox
///
/// Syncfusion MultiColumnComboBox renders an input element inside the wrapper div.
/// The wrapper element gets the IdGenerator-based ID; the visible input is a child.
/// Playwright interacts with the wrapper; the ej2 instance fires events.
/// </summary>
[TestFixture]
public class WhenMultiColumnItemSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/MultiColumnComboBox";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_MultiColumnComboBoxModel";
    private const string FacilityId = Scope + "__Facility";

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
        await Expect(Page).ToHaveTitleAsync("MultiColumnComboBox — Alis.Reactive Sandbox");
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
        // SF MultiColumnComboBox wrapper gets the IdGenerator-based ID
        var wrapper = Page.Locator($"#{FacilityId}");
        await Expect(wrapper).ToBeVisibleAsync();

        // Wait for the value to be set by dom-ready via ej2 instance
        await Page.WaitForFunctionAsync(
            $"() => {{ const el = document.getElementById('{FacilityId}'); return el && el.ej2_instances && el.ej2_instances[0] && el.ej2_instances[0].value === '1'; }}",
            null,
            new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        // The value-echo should show "1" after dom-ready reads comp.Value()
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var text = await echo.TextContentAsync();
        Assert.That(text, Does.Contain("1"),
            "Value echo should contain 1 after dom-ready property read");
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
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidepopup_button_closes_dropdown()
    {
        await NavigateAndBoot();

        // Open popup first
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click HidePopup button
        await Page.Locator("#hide-popup-btn").ClickAsync();

        // The SF popup list should be hidden
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
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
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click on "Lakeside Care" in the popup grid
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Lakeside Care" }).ClickAsync();

        // SF change event payload contains the selected value
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var changeText = await Page.Locator("#change-value").TextContentAsync();
        Assert.That(changeText, Does.Contain("2"),
            $"Change value should contain 2 but was '{changeText}'");
        AssertNoConsoleErrors();
    }

    // ── Section 5: Conditions — Typed Event-Args + Component-Read ──

    [Test]
    public async Task event_args_condition_matches_when_value_equals_sunrise()
    {
        await NavigateAndBoot();

        // Open popup and select Sunrise Manor (value="1")
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Sunrise Manor" }).ClickAsync();

        // When(args, x => x.Value).Eq("1") -> Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("sunrise manor selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_falls_to_else_for_other_values()
    {
        await NavigateAndBoot();

        // Open popup and select Meadow Ridge (value="3", not "1")
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Meadow Ridge" }).ClickAsync();

        // When(args, x => x.Value).Eq("1") -> Else branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("other facility", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        // Open popup and select an item
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Harbor View" }).ClickAsync();

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
    public async Task gather_sends_initial_facility_value_to_server()
    {
        await NavigateAndBoot();

        // DomReady sets Facility to "1" — gather must POST that value
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/MultiColumnComboBox/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("1"),
            $"Gather POST body must contain the initial value '1' but was '{body}'");

        // Confirm the round-trip completes — response handler fires
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_facility_then_gathering_sends_new_value()
    {
        await NavigateAndBoot();

        // Change the value from "1" to "2" via popup selection
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Lakeside Care" }).ClickAsync();

        // Wait for change event to confirm the value took effect
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // Gather must POST the CURRENT value "2", not the initial "1"
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/MultiColumnComboBox/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("\"2\""),
            $"Gather must send the current value '2' but body was '{body}'");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Column headers ──

    [Test]
    public async Task columns_display_text_city_and_capacity()
    {
        await NavigateAndBoot();

        // Open popup to reveal the multi-column grid
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Verify column headers are visible — SF renders .e-headercell for each column
        var headers = Page.Locator(".e-multicolumn-list.e-popup .e-headercell");
        await Expect(headers).ToHaveCountAsync(3, new() { Timeout = 5000 });

        // Verify specific column header text: Name, City, Capacity
        // SF appends accessibility text ("Press Enter to sort") so use substring matching
        var headerTexts = await headers.AllTextContentsAsync();
        Assert.That(headerTexts.Any(h => h.Contains("Name")), Is.True,
            "Column headers must include 'Name'");
        Assert.That(headerTexts.Any(h => h.Contains("City")), Is.True,
            "Column headers must include 'City'");
        Assert.That(headerTexts.Any(h => h.Contains("Capacity")), Is.True,
            "Column headers must include 'Capacity'");

        AssertNoConsoleErrors();
    }

    // ── Select → condition → re-select → condition re-fires ──

    [Test]
    public async Task selecting_then_changing_fires_condition_each_time()
    {
        await NavigateAndBoot();

        // Select Sunrise Manor (value="1") — condition: Eq("1") → "sunrise manor selected"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Sunrise Manor" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("sunrise manor selected", new() { Timeout = 5000 });

        // Wait for popup to close after selection before re-opening
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        // Now select Lakeside Care (value="2") — condition: Eq("1") fails → "other facility"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Lakeside Care" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("other facility", new() { Timeout = 5000 });

        // Wait for popup to close after selection before re-opening
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        // Select Sunrise Manor again — condition must fire again with "sunrise manor selected"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-multicolumn-list.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-multicolumn-list.e-popup .e-row").Filter(new() { HasText = "Sunrise Manor" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("sunrise manor selected", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
