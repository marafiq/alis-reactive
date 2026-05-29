namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionSmartComponents : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/SmartComponents";

    private async Task NavigateAndWaitForSmartComponents()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Page.Locator("textarea[id$='__CareNote']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#smart-paste.e-control"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task smart_textarea_set_value_read_value_and_focus_methods_execute()
    {
        await NavigateAndWaitForSmartComponents();

        await Expect(Page.Locator("#smart-note-value"))
            .ToHaveTextAsync("Resident is", new() { Timeout = 10000 });

        await Page.Locator("#set-smart-note-btn").ClickAsync();
        await Expect(Page.Locator("#smart-note-value"))
            .ToHaveTextAsync("Resident prefers morning therapy", new() { Timeout = 5000 });

        await Page.Locator("#focus-smart-note-btn").ClickAsync();
        await Expect(Page.Locator("#smart-note-focus-status"))
            .ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(Page.Locator("textarea[id$='__CareNote']"))
            .ToBeFocusedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task smart_textarea_suggestion_events_surface_before_and_after_payloads()
    {
        await NavigateAndWaitForSmartComponents();

        var textarea = Page.Locator("textarea[id$='__CareNote']");
        await textarea.ClickAsync();
        await textarea.FillAsync("Resident is");
        await textarea.PressAsync(" ");

        await Expect(Page.Locator("#before-suggestion"))
            .ToContainTextAsync("hydrating well", new() { Timeout = 10000 });

        await textarea.PressAsync("Tab");
        await Expect(Page.Locator("#after-suggestion"))
            .ToContainTextAsync("hydrating well", new() { Timeout = 10000 });
        await Expect(textarea)
            .ToHaveValueAsync(new System.Text.RegularExpressions.Regex("Resident is hydrating well\\s*$"), new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task smart_paste_button_click_method_fills_form_from_endpoint()
    {
        await NavigateAndWaitForSmartComponents();

        await Page.Locator("#run-smart-paste-btn").ClickAsync();

        await Expect(Page.Locator("#paste-command-status"))
            .ToHaveTextAsync("click called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#Resident"))
            .ToHaveValueAsync("Nora", new() { Timeout = 10000 });
        await Expect(Page.Locator("#Room"))
            .ToHaveValueAsync("212B", new() { Timeout = 10000 });
        await Expect(Page.Locator("#Notes"))
            .ToHaveValueAsync("Hydration rounds complete", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task smart_paste_focus_and_disabled_members_execute()
    {
        await NavigateAndWaitForSmartComponents();

        await Page.Locator("#focus-smart-paste-btn").ClickAsync();
        await Expect(Page.Locator("#paste-focus-status"))
            .ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#smart-paste"))
            .ToBeFocusedAsync(new() { Timeout = 5000 });

        await Page.Locator("#disable-smart-paste-btn").ClickAsync();
        await Expect(Page.Locator("#paste-disabled-value"))
            .ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#smart-paste"))
            .ToBeDisabledAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_index_links_to_smart_components_sandbox()
    {
        await NavigateTo("/Sandbox/Components");
        await Expect(Page.Locator("a[href='/Sandbox/Components/SmartComponents/Index']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
