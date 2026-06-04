namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion;

/// <summary>
/// Exercises FusionAccordion behaviors end-to-end:
/// DomReady expand, event args echo, condition branching,
/// button-triggered disable, and lazy-load via HTTP + Into.
/// </summary>
[TestFixture]
public class WhenAccordionPanelExpands : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Accordion";

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#expand-result");
    }

    [Test]
    public async Task domready_expands_second_panel()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#expand-result"))
            .ToHaveTextAsync("Panel 1 expanded via ExpandItem", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_panel_echoes_expanded_index()
    {
        await NavigateAndBoot();

        var firstHeader = Page.Locator("#demo-accordion .e-acrdn-header").First;
        await ClickWhenStable(firstHeader);

        await Expect(Page.Locator("#expanded-index"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task expanding_panel_shows_expanded_condition()
    {
        await NavigateAndBoot();

        var firstHeader = Page.Locator("#demo-accordion .e-acrdn-header").First;
        await ClickWhenStable(firstHeader);

        await Expect(Page.Locator("#condition-result"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        var result = await Page.Locator("#condition-result").TextContentAsync();
        Assert.That(result, Does.Contain("Panel").And.Contain("ed"),
            "Condition should show 'Panel expanded' or 'Panel collapsed'");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task disable_button_disables_contact_panel()
    {
        await NavigateAndBoot();

        var disableBtn = Page.Locator("#btn-disable");
        await ClickWhenStable(disableBtn);

        await Expect(Page.Locator("#enable-result"))
            .ToHaveTextAsync("Panel 2 disabled via EnableItem", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task expanding_lazy_panel_loads_content_via_http()
    {
        await NavigateAndBoot();

        var lazyHeader = Page.Locator("#lazy-accordion .e-acrdn-header").First;
        await ClickWhenStable(lazyHeader);

        await Expect(Page.Locator("#lazy-load-status"))
            .ToHaveTextAsync("Overview loaded", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_accordion_behaviors()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("demo-accordion"));
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        AssertNoConsoleErrors();
    }
}
