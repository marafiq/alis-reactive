namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Tab;

/// <summary>
/// Exercises FusionTab behaviors end-to-end:
/// DomReady SetSelectedItem, tab selection event with condition branching,
/// programmatic Select via button, HideTab/ShowTab toggle,
/// and lazy-load tab content via HTTP + Into.
///
/// Page under test: /Sandbox/Components/Tab
/// </summary>
[TestFixture]
public class WhenTabSwitches : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Tab";

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#set-selected-result");
    }

    // ── DomReady SetSelectedItem ──

    [Test]
    public async Task domready_selects_second_tab()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#set-selected-result"))
            .ToHaveTextAsync("SetSelectedItem applied (tab 2)", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Tab selection event ──

    [Test]
    public async Task clicking_tab_echoes_selected_index()
    {
        await NavigateAndBoot();

        // Click the first tab (Residents)
        var firstTab = Page.Locator("#demo-tab .e-tab-header .e-toolbar-item").First;
        await ClickWhenStable(firstTab);

        await Expect(Page.Locator("#selected-index"))
            .ToHaveTextAsync("0", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Condition branching on tab selection ──

    [Test]
    public async Task selecting_residents_tab_shows_condition_text()
    {
        await NavigateAndBoot();

        var firstTab = Page.Locator("#demo-tab .e-tab-header .e-toolbar-item").First;
        await ClickWhenStable(firstTab);

        await Expect(Page.Locator("#condition-result"))
            .ToHaveTextAsync("Residents tab active", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Programmatic Select via button ──

    [Test]
    public async Task select_button_navigates_to_facilities_tab()
    {
        await NavigateAndBoot();

        var selectBtn = Page.Locator("#select-tab-btn");
        await ClickWhenStable(selectBtn);

        await Expect(Page.Locator("#method-result"))
            .ToHaveTextAsync("select(2) called", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── HideTab toggle ──

    [Test]
    public async Task hide_button_hides_reports_tab()
    {
        await NavigateAndBoot();

        var hideBtn = Page.Locator("#hide-tab-btn");
        await ClickWhenStable(hideBtn);

        await Expect(Page.Locator("#hide-result"))
            .ToHaveTextAsync("hideTab(3, true) called", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task show_button_restores_reports_tab()
    {
        await NavigateAndBoot();

        // Hide first, then show
        await ClickWhenStable(Page.Locator("#hide-tab-btn"));
        await Expect(Page.Locator("#hide-result"))
            .ToHaveTextAsync("hideTab(3, true) called", new() { Timeout = 5000 });

        await ClickWhenStable(Page.Locator("#show-tab-btn"));
        await Expect(Page.Locator("#hide-result"))
            .ToHaveTextAsync("hideTab(3, false) called", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Lazy-load via HTTP + Into ──

    [Test]
    public async Task lazy_tab_loads_content_via_http_on_boot()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#lazy-load-status"))
            .ToHaveTextAsync("Residents loaded on boot", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // ── Plan JSON ──

    [Test]
    public async Task plan_json_contains_tab_behaviors()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("demo-tab"));
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        AssertNoConsoleErrors();
    }
}
