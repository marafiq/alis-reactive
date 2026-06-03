using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.AutoComplete;

/// <summary>
/// Proves the AutoComplete locator helpers against the same browser-visible
/// property, event, condition, gather, and filtering behavior.
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
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");

        _scope = new ComponentScope(Page,
            "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_AutoCompleteModel__");
        _physician = _scope.AutoComplete("Physician");
        _medication = _scope.AutoComplete("MedicationType");
    }

    [Test]
    public async Task domready_sets_initial_value_ext()
    {
        await NavigateAndBoot();
        await Expect(_physician.Input).ToBeVisibleAsync();
        await Expect(_physician.Input).ToHaveValueAsync("Dr. Smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo_ext()
    {
        await NavigateAndBoot();
        await Expect(_scope.Element("value-echo"))
            .ToContainTextAsync("smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

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

    [Test]
    public async Task gather_sends_current_value_after_change_ext()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Johnson");
        await Expect(_scope.Element("change-value"))
            .ToContainTextAsync("johnson", new() { Timeout = 5000 });

        var gatherRequest = await Page.RunAndWaitForRequestAsync(
            async () => await Page.Locator("#gather-btn").ClickAsync(),
            "**/Sandbox/Components/AutoComplete/Echo");

        var requestBody = gatherRequest.PostData ?? "";
        Assert.That(requestBody, Does.Contain("johnson"));
        Assert.That(requestBody, Does.Not.Contain("smith"));

        await Expect(_scope.Element("gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_multiple_values_fires_change_each_time_ext()
    {
        await NavigateAndBoot();

        var changeValue = _scope.Element("change-value");
        var argsCondition = _scope.Element("args-condition");

        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Johnson");
        await Expect(changeValue).ToContainTextAsync("johnson", new() { Timeout = 5000 });
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Williams");
        await Expect(changeValue).ToContainTextAsync("williams", new() { Timeout = 5000 });
        await Expect(argsCondition).ToHaveTextAsync("other physician", new() { Timeout = 3000 });

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

        var selectedIndicator = _scope.Element("selected-indicator");

        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Johnson");
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToHaveTextAsync("selected", new() { Timeout = 3000 });

        await _physician.Clear();
        await _physician.Blur();
        await Expect(selectedIndicator).ToBeHiddenAsync(new() { Timeout = 5000 });

        await Page.Locator("#show-popup-btn").ClickAsync();
        await _physician.SelectItem("Dr. Smith");
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

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
