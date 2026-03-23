using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Same tests as WhenUsingFusionAutoComplete but using Playwright.Extensions locators.
/// Side-by-side comparison: is it better or worse?
/// </summary>
[TestFixture]
public class WhenAutoCompleteFiltersRemotely : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AutoComplete";

    private ComponentScope _scope = null!;
    private AutoCompleteLocator _physician = null!;
    private AutoCompleteLocator _medication = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);

        _scope = new ComponentScope(Page,
            "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_AutoCompleteModel__");
        _physician = _scope.AutoComplete("Physician");
        _medication = _scope.AutoComplete("MedicationType");
    }

    // ── Section 1: Property Write ──

    [Test]
    public async Task domready_sets_initial_value_ext()
    {
        await NavigateAndBoot();
        await Expect(_physician.Input).ToBeVisibleAsync();
        await Expect(_physician.Input).ToHaveValueAsync("Dr. Smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo_ext()
    {
        await NavigateAndBoot();
        await Expect(_scope.Element("value-echo"))
            .ToContainTextAsync("smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Method Calls ──

    [Test]
    public async Task showpopup_button_opens_dropdown_ext()
    {
        await NavigateAndBoot();
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(_physician.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidepopup_button_closes_dropdown_ext()
    {
        await NavigateAndBoot();
        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(_physician.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator("#hide-popup-btn").ClickAsync();
        await Expect(_physician.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Events ──

    [Test]
    public async Task changed_event_displays_new_value_ext()
    {
        await NavigateAndBoot();
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Johnson");
        await Expect(_scope.Element("change-value"))
            .ToContainTextAsync("johnson", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 5: Conditions ──

    [Test]
    public async Task event_args_condition_matches_smith_ext()
    {
        await NavigateAndBoot();
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Smith");
        await Expect(_scope.Element("args-condition"))
            .ToHaveTextAsync("dr smith selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_else_for_other_ext()
    {
        await NavigateAndBoot();
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Johnson");
        await Expect(_scope.Element("args-condition"))
            .ToHaveTextAsync("other physician", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_ext()
    {
        await NavigateAndBoot();
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Williams");
        await Expect(_scope.Element("selected-indicator")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(_scope.Element("selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 6: Gather ──

    [Test]
    public async Task gather_sends_current_value_after_change_ext()
    {
        await NavigateAndBoot();

        // Change to Dr. Johnson via popup
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Johnson");
        await Expect(_scope.Element("change-value"))
            .ToContainTextAsync("johnson", new() { Timeout = 5000 });

        // Gather must POST the CURRENT value
        var request = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/AutoComplete/Echo");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("johnson"));
        Assert.That(body, Does.Not.Contain("smith"));

        await Expect(_scope.Element("gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle ──

    [Test]
    public async Task selecting_multiple_values_fires_change_each_time_ext()
    {
        await NavigateAndBoot();

        var changeValue = _scope.Element("change-value");
        var argsCondition = _scope.Element("args-condition");

        // Cycle 1: Dr. Johnson → "other physician"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Johnson");
        await Expect(changeValue).ToContainTextAsync("johnson", new() { Timeout = 5000 });
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        // Cycle 2: Dr. Williams → still "other physician"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Williams");
        await Expect(changeValue).ToContainTextAsync("williams", new() { Timeout = 5000 });
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        // Cycle 3: Dr. Smith → flips to "dr smith selected"
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Smith");
        await Expect(changeValue).ToContainTextAsync("smith", new() { Timeout = 5000 });
        await Expect(argsCondition).ToHaveTextAsync("dr smith selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_and_reselecting_toggles_indicator_ext()
    {
        await NavigateAndBoot();

        var indicator = _scope.Element("selected-indicator");

        // Select Dr. Johnson → indicator visible
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Johnson");
        await Expect(indicator).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(indicator).ToHaveTextAsync("selected", new() { Timeout = 3000 });

        // Clear via user gesture → indicator hides
        await _physician.Clear();
        await _physician.Blur();
        await Expect(indicator).ToBeHiddenAsync(new() { Timeout = 5000 });

        // Reselect Dr. Smith → indicator back
        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Smith");
        await Expect(indicator).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 7: Filtering ──

    [Test]
    public async Task filtering_fires_http_and_populates_popup_ext()
    {
        await NavigateAndBoot();
        await _medication.Type("anti");
        await Expect(_scope.Element("filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });
        await Expect(_medication.PopupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
