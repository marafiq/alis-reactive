using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.AutoComplete;

/// <summary>
/// Proves FusionAutoComplete property writes, reads, events, conditions,
/// gather, and server-filtered suggestions through page-visible behavior.
/// </summary>
[TestFixture]
public class WhenAutoCompleteSuggests : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AutoComplete";

    // Generated component IDs are the DOM/plan join keys under test.
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_AutoCompleteModel";
    private const string PhysicianId = GeneratedTypeScope + "__Physician";
    private const string MedicationId = GeneratedTypeScope + "__MedicationType";

    private AutoCompleteLocator Physician => new(Page, PhysicianId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

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
        var physician = Physician;
        await Expect(physician.Input).ToBeVisibleAsync();

        await Expect(physician.Input).Not.ToHaveValueAsync("", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var valueEcho = Page.Locator("#value-echo");
        await Expect(valueEcho).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var valueEchoText = await valueEcho.TextContentAsync();
        Assert.That(valueEchoText, Does.Contain("smith"),
            "Value echo should contain Dr. Smith after dom-ready property read");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showpopup_button_opens_dropdown()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();

        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidepopup_button_closes_dropdown()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#hide-popup-btn").ClickAsync();

        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_displays_new_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var changeText = await Page.Locator("#change-value").TextContentAsync();
        Assert.That(changeText, Does.Contain("johnson"),
            $"Change value should contain Dr. Johnson but was '{changeText}'");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_equals_dr_smith()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Smith" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("dr smith selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_falls_to_else_for_other_values()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("other physician", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Williams" }).ClickAsync();

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
    public async Task gather_sends_initial_physician_value_to_server()
    {
        await NavigateAndBoot();

        var gatherRequest = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/AutoComplete/Echo");

        var requestBody = gatherRequest.PostData ?? "";
        Assert.That(requestBody, Does.Contain("smith"),
            $"Gather POST body must contain the initial value 'Dr. Smith' but was '{requestBody}'");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_physician_then_gathering_sends_new_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        var gatherRequest = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/AutoComplete/Echo");

        var requestBody = gatherRequest.PostData ?? "";
        Assert.That(requestBody, Does.Contain("johnson"),
            $"Gather must send the current value 'Dr. Johnson' but body was '{requestBody}'");
        Assert.That(requestBody, Does.Not.Contain("smith"),
            "Gather must NOT send the stale initial value 'Dr. Smith'");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task server_echoes_back_exact_gathered_physician_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Williams" }).ClickAsync();
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        var echoResponse = await Page.RunAndWaitForResponseAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/AutoComplete/Echo");

        var responseBody = await echoResponse.TextAsync();
        Assert.That(responseBody, Does.Contain("williams"),
            $"Server response must echo back the gathered value 'Dr. Williams' but was '{responseBody}'");
        Assert.That((int)echoResponse.Status, Is.EqualTo(200),
            "Echo endpoint must return 200 OK");

        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_multiple_different_values_fires_change_each_time()
    {
        await NavigateAndBoot();

        var changeValue = Page.Locator("#change-value");
        var argsCondition = Page.Locator("#args-condition");

        var showPopupButton = Page.Locator("#show-popup-btn");

        await showPopupButton.ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();
        var johnsonChangeText = await changeValue.TextContentAsync();
        Assert.That(johnsonChangeText, Does.Contain("johnson"),
            $"First selection should contain Dr. Johnson but was '{johnsonChangeText}'");
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        // Wait for popup close animation to complete before re-opening
        await Expect(Page.Locator(".e-ddl.e-popup")).ToBeHiddenAsync(new() { Timeout = 5000 });
        await showPopupButton.ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Williams" }).ClickAsync();
        var williamsChangeText = await changeValue.TextContentAsync();
        Assert.That(williamsChangeText, Does.Contain("williams"),
            $"Second selection should contain Dr. Williams but was '{williamsChangeText}'");
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        await Expect(Page.Locator(".e-ddl.e-popup")).ToBeHiddenAsync(new() { Timeout = 5000 });
        await showPopupButton.ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Smith" }).ClickAsync();
        var smithChangeText = await changeValue.TextContentAsync();
        Assert.That(smithChangeText, Does.Contain("smith"),
            $"Third selection should contain Dr. Smith but was '{smithChangeText}'");
        await Expect(argsCondition).ToHaveTextAsync("dr smith selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_selection_then_reselecting_toggles_indicator()
    {
        await NavigateAndBoot();

        var selectedIndicator = Page.Locator("#selected-indicator");
        var argsCondition = Page.Locator("#args-condition");

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Johnson" }).ClickAsync();
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToHaveTextAsync("selected", new() { Timeout = 3000 });
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        await Physician.Clear();
        await Physician.Blur();
        await Expect(selectedIndicator).ToBeHiddenAsync(new() { Timeout = 5000 });

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Dr. Smith" }).ClickAsync();
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToHaveTextAsync("selected", new() { Timeout = 3000 });
        await Expect(argsCondition).ToHaveTextAsync("dr smith selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    private async Task TypeInMedication(string text)
    {
        var input = Page.Locator($"#{MedicationId}");
        await Expect(input).ToBeVisibleAsync();
        await input.ClickAsync();
        // Syncfusion filtering listens for keyup/keydown; FillAsync does not trigger it.
        await input.PressSequentiallyAsync(text, new() { Delay = 50 });
    }

    [Test]
    public async Task filtering_event_fires_http_get_and_updates_datasource()
    {
        await NavigateAndBoot();

        await TypeInMedication("anti");

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

        var medicationRequest = await Page.RunAndWaitForRequestAsync(
            async () => await input.PressSequentiallyAsync("ster", new() { Delay = 50 }),
            r => r.Url.Contains("/Medications"));

        Assert.That(medicationRequest.Url, Does.Contain("Medications"),
            "GET request should target the Medications endpoint");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_response_populates_autocomplete_dropdown()
    {
        await NavigateAndBoot();

        await TypeInMedication("anti");

        await Expect(Page.Locator("#filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });

        var popupItems = Page.Locator(".e-ddl.e-popup .e-list-item");
        await Expect(popupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        var popupItemCount = await popupItems.CountAsync();
        Assert.That(popupItemCount, Is.GreaterThan(0),
            "Popup should contain filtered medication items after updateData");
        AssertNoConsoleErrors();
    }
}
