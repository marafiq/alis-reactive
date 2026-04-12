namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

/// <summary>
/// Issue #86: Dispatch payloads can now carry live component values
/// alongside literal values in the same typed payload.
/// </summary>
[TestFixture]
public class WhenDispatchingWithComponentValue : PlaywrightTestBase
{
    private async Task NavigateToDispatchSourcePage()
    {
        await NavigateToAndWaitForBoot("/Sandbox/CoreBehaviors/DispatchSource");
    }

    [Test]
    public async Task dispatch_carries_live_textbox_value_to_listener()
    {
        await NavigateToDispatchSourcePage();

        await Page.GetByPlaceholder("Type a name...").FillAsync("Jane Smith");
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Dispatch Transfer" }));

        await Expect(Page.Locator("#received-result")).ToHaveTextAsync("Received!");
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("Jane Smith");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dispatch_carries_literal_status_alongside_source_name()
    {
        await NavigateToDispatchSourcePage();

        await Page.GetByPlaceholder("Type a name...").FillAsync("Bob Jones");
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Dispatch Transfer" }));

        await Expect(Page.Locator("#received-result")).ToHaveTextAsync("Received!");
        // Source field — live value from textbox
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("Bob Jones");
        // Literal field — baked at build time
        await Expect(Page.Locator("#received-status-value")).ToHaveTextAsync("active");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dispatch_reads_updated_textbox_value_on_second_click()
    {
        await NavigateToDispatchSourcePage();

        var textbox = Page.GetByPlaceholder("Type a name...");
        var button = Page.GetByRole(AriaRole.Button, new() { Name = "Dispatch Transfer" });

        // First dispatch
        await textbox.FillAsync("First");
        await ClickWhenStable(button);
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("First");

        // Change value and dispatch again — proves runtime read, not cached
        await textbox.FillAsync("Second");
        await ClickWhenStable(button);
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("Second");
        AssertNoConsoleErrors();
    }
}
