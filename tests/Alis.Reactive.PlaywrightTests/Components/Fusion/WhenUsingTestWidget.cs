namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingTestWidget : PlaywrightTestBase
{
    private const string Path = "/Sandbox/TestWidget";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task Page_loads_with_no_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("TestWidget — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── 1: Property Write (static) ──

    [Test]
    public async Task Property_write_sets_value_on_dom_ready()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#tw-write input"))
            .ToHaveValueAsync("initialized", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 2: Void Method — Clear ──

    [Test]
    public async Task Clear_empties_the_widget()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#tw-clear input")).ToHaveValueAsync("clear-me");
        await Page.Locator("#btn-clear").ClickAsync();
        await Expect(Page.Locator("#tw-clear input"))
            .ToHaveValueAsync("", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 3: Void Method — Focus ──

    [Test]
    public async Task Focus_gives_inner_input_focus()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-focus").ClickAsync();
        await Expect(Page.Locator("#tw-focus input"))
            .ToBeFocusedAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 4: Event Payload → Component SetValue ──

    [Test]
    public async Task Event_payload_writes_value_to_mirror_widget()
    {
        await NavigateAndBoot();
        await Page.Locator("#tw-event-write input").FillAsync("hello");
        await Expect(Page.Locator("#tw-event-mirror input"))
            .ToHaveValueAsync("hello", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 5: Component Read → Display ──

    [Test]
    public async Task Component_read_displays_value()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#read-echo"))
            .ToHaveTextAsync("read-me", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 6: Component Value Condition ──

    [Test]
    public async Task Condition_shows_indicator_when_not_empty()
    {
        await NavigateAndBoot();
        await Expect(Page.Locator("#comp-cond-ind")).ToBeHiddenAsync();
        await Page.Locator("#tw-comp-cond input").FillAsync("x");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Condition_hides_indicator_when_empty()
    {
        await NavigateAndBoot();
        await Page.Locator("#tw-comp-cond input").FillAsync("x");
        await Expect(Page.Locator("#comp-cond-ind")).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Page.Locator("#tw-comp-cond input").FillAsync("");
        await Expect(Page.Locator("#comp-cond-ind"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 7: Method + Arg (SetItems from event) ──

    [Test]
    public async Task SetItems_from_event_payload()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-load-items").ClickAsync();
        // setItems renders <li> elements inside the widget
        await Expect(Page.Locator("#tw-items .test-widget-item"))
            .ToHaveCountAsync(3, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── 8: Cross-Component Read → Write ──

    [Test]
    public async Task Cross_component_read_writes_to_target()
    {
        await NavigateAndBoot();
        await Page.Locator("#tw-source input").FillAsync("synced");
        await Expect(Page.Locator("#tw-target input"))
            .ToHaveValueAsync("synced", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── 9: Gather → HTTP Echo ──

    [Test]
    public async Task Gather_posts_widget_value()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-gather").ClickAsync();
        await Expect(Page.Locator("#gather-result"))
            .ToHaveTextAsync("gathered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── 10: Response Body → Component Property Write ──

    [Test]
    public async Task Response_body_sets_widget_value()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-datasource").ClickAsync();
        await Expect(Page.Locator("#tw-ds-target input"))
            .ToHaveValueAsync("beta", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── 11: Response Body → Component Method (SetItems) ──

    [Test]
    public async Task Response_body_sets_widget_items()
    {
        await NavigateAndBoot();
        await Page.Locator("#btn-datasource").ClickAsync();
        // setItems renders <li> elements: alpha, beta, gamma
        await Expect(Page.Locator("#tw-ds-target .test-widget-item"))
            .ToHaveCountAsync(3, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
