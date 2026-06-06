using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DropDownList;

[TestFixture]
public class WhenDropdownItemSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DropDownList";

    // Generated component IDs are the DOM/Reactive Plan join keys under test.
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DropDownListModel";
    private const string CategoryId = GeneratedTypeScope + "__Category";

    private DropDownListLocator Category => new(Page, CategoryId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("DropDownList — Alis.Reactive Sandbox");
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
        await Expect(Category.Input).ToBeVisibleAsync();

        // Syncfusion shows display text in the input, not the raw selected value.
        await Expect(Category.Input).Not.ToHaveValueAsync("", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var valueEcho = Page.Locator("#value-echo");
        await Expect(valueEcho).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var valueEchoText = await valueEcho.TextContentAsync();
        Assert.That(valueEchoText, Does.Contain("Books"),
            "Value echo should contain Books after dom-ready property read");
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

        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Electronics" }).ClickAsync();

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        var changeText = await Page.Locator("#change-value").TextContentAsync();
        Assert.That(changeText, Does.Contain("Electronics"),
            $"Change value should contain Electronics but was '{changeText}'");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focus_event_shows_focus_state()
    {
        await NavigateAndBoot();

        // Syncfusion wrapper owns pointer focus; the generated input has tabindex="-1".
        var wrapper = Page.Locator($"span.e-ddl:has(#{CategoryId})");
        await wrapper.ClickAsync();

        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("focused", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task blur_event_shows_blur_state()
    {
        await NavigateAndBoot();

        var wrapper = Page.Locator($"span.e-ddl:has(#{CategoryId})");
        await wrapper.ClickAsync();
        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("focused", new() { Timeout = 5000 });

        await Page.Keyboard.PressAsync("Escape");
        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.Locator("#focus-state"))
            .ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_value_equals_electronics()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Electronics" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("electronics selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_falls_to_else_for_other_values()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Clothing" }).ClickAsync();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("other category", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_value_not_null()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Page.Locator(".e-ddl.e-popup"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = "Clothing" }).ClickAsync();

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
    public async Task selecting_different_values_keeps_indicator_visible()
    {
        await NavigateAndBoot();

        // DomReady SetValue("Books") fires change, so NotNull is already true after boot.
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });

        await Category.Select("Electronics");

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });

        await Category.Select("Clothing");

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });

        await Category.Select("Books");

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
