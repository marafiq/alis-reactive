namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

/// <summary>
/// Issue #86: Dispatch payloads carry live component values — flat, literal, and nested.
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
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("Bob Jones");
        await Expect(Page.Locator("#received-status-value")).ToHaveTextAsync("active");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dispatch_reads_updated_textbox_value_on_second_click()
    {
        await NavigateToDispatchSourcePage();

        var textbox = Page.GetByPlaceholder("Type a name...");
        var button = Page.GetByRole(AriaRole.Button, new() { Name = "Dispatch Transfer" });

        await textbox.FillAsync("First");
        await ClickWhenStable(button);
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("First");

        await textbox.FillAsync("Second");
        await ClickWhenStable(button);
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("Second");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dispatch_carries_nested_address_city_from_textbox()
    {
        await NavigateToDispatchSourcePage();

        await Page.GetByPlaceholder("Type a name...").FillAsync("Alice");
        await Page.GetByPlaceholder("Type a city...").FillAsync("Seattle");
        await ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = "Dispatch Transfer" }));

        await Expect(Page.Locator("#received-result")).ToHaveTextAsync("Received!");
        await Expect(Page.Locator("#received-name")).ToHaveTextAsync("Alice");
        await Expect(Page.Locator("#received-city")).ToHaveTextAsync("Seattle");
        await Expect(Page.Locator("#received-status-value")).ToHaveTextAsync("active");
        AssertNoConsoleErrors();
    }
}
