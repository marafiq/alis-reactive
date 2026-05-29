namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionMention : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Mention";

    private async Task NavigateAndWaitForMention()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Page.Locator("#care-note"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task search_method_opens_popup_with_matching_care_team_member()
    {
        await NavigateAndWaitForMention();

        await Page.Locator("#care-note").EvaluateAsync("el => { el.focus(); el.setSelectionRange(2, 2); }");
        await Page.Locator("#mention-search-btn").ClickAsync();

        await Expect(Page.Locator("#mention-command-status"))
            .ToHaveTextAsync("search called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#mention-popup-status"))
            .ToHaveTextAsync("opened", new() { Timeout = 5000 });
        await Expect(Page.Locator(".e-popup .e-list-item").Filter(new() { HasTextString = "Nora Nurse" }))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_item_fires_typed_changed_event()
    {
        await NavigateAndWaitForMention();

        await Page.Locator("#care-note").EvaluateAsync("el => { el.focus(); el.setSelectionRange(2, 2); }");
        await Page.Locator("#mention-search-btn").ClickAsync();
        await Page.Locator(".e-popup .e-list-item").Filter(new() { HasTextString = "Nora Nurse" }).ClickAsync();

        await Expect(Page.Locator("#mention-value"))
            .ToContainTextAsync("Nora Nurse", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task show_and_hide_methods_drive_popup_lifecycle_events()
    {
        await NavigateAndWaitForMention();

        await Page.Locator("#mention-show-btn").ClickAsync();
        await Expect(Page.Locator("#mention-command-status"))
            .ToHaveTextAsync("show called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#mention-popup-status"))
            .ToHaveTextAsync("opened", new() { Timeout = 10000 });

        await Page.Locator("#auto-hide-note").EvaluateAsync("el => { el.focus(); el.setSelectionRange(2, 2); }");
        await Page.Locator("#mention-auto-hide-btn").ClickAsync();
        await Expect(Page.Locator("#mention-auto-hide-command-status"))
            .ToHaveTextAsync("search called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#mention-hide-status"))
            .ToHaveTextAsync("hide called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#mention-auto-hide-popup-status"))
            .ToHaveTextAsync("closed", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_index_links_to_mention_sandbox()
    {
        await NavigateTo("/Sandbox/Components");
        await Expect(Page.Locator("a[href='/Sandbox/Components/Mention/Index']"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
