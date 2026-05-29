using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Button";
    private const string ButtonId = "fusion-action-button";

    private FusionButtonLocator Button => new(Page, ButtonId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#content-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionButton — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_button_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(ButtonId));
        Assert.That(planJson, Does.Contain("\"content\""));
        Assert.That(planJson, Does.Contain("\"disabled\""));
        Assert.That(planJson, Does.Contain("\"iconCss\""));
        Assert.That(planJson, Does.Contain("\"iconPosition\""));
        Assert.That(planJson, Does.Contain("\"cssClass\""));
        Assert.That(planJson, Does.Contain("\"isPrimary\""));
        Assert.That(planJson, Does.Contain("\"isToggle\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"click\""));
        Assert.That(planJson, Does.Contain("\"focusIn\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_visible_content_and_reads_content_source()
    {
        await NavigateAndBoot();

        await Expect(Button.Button).ToContainTextAsync("Ready", new() { Timeout = 5000 });
        await Expect(Page.Locator("#content-echo")).ToHaveTextAsync("Ready", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_content_updates_visible_button_and_content_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-content-btn").ClickAsync();

        await Expect(Button.Button).ToContainTextAsync("Queued", new() { Timeout = 5000 });
        await Expect(Page.Locator("#content-echo")).ToHaveTextAsync("Queued", new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("content set", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task disabled_write_updates_dom_source_and_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-button-btn").ClickAsync();
        await Expect(Page.Locator("#disabled-state")).ToHaveTextAsync("enabled", new() { Timeout = 5000 });

        await Page.Locator("#disable-btn").ClickAsync();
        await Expect(Button.Button).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Page.Locator("#check-button-btn").ClickAsync();
        await Expect(Page.Locator("#disabled-state")).ToHaveTextAsync("disabled", new() { Timeout = 5000 });

        await Page.Locator("#enable-btn").ClickAsync();
        await Expect(Button.Button).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task icon_css_class_and_primary_writes_update_visible_classes()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-icon-btn").ClickAsync();
        await Expect(Button.Icon).ToHaveCountAsync(1);
        var iconClasses = await Button.IconClassAttribute();
        Assert.That(iconClasses, Does.Contain("e-edit"));
        Assert.That(iconClasses, Does.Contain("e-icon-right"));

        await Page.Locator("#set-css-btn").ClickAsync();
        Assert.That(await Button.HasClass("e-success"), Is.True);
        Assert.That(await Button.HasClass("extra-class"), Is.True);
        await Expect(Page.Locator("#css-echo")).ToHaveTextAsync("e-success extra-class", new() { Timeout = 5000 });

        await Page.Locator("#set-primary-btn").ClickAsync();
        Assert.That(await Button.HasClass("e-primary"), Is.True);
        await Expect(Page.Locator("#primary-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task click_method_invokes_button_and_toggle_can_be_disabled()
    {
        await NavigateAndBoot();

        await Page.Locator("#click-button-btn").ClickAsync();
        await Expect(Page.Locator("#click-state")).ToHaveTextAsync("click called", new() { Timeout = 5000 });
        Assert.That(await Button.HasClass("e-active"), Is.True);

        await Page.Locator("#set-toggle-btn").ClickAsync();
        await Expect(Page.Locator("#toggle-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        Assert.That(await Button.HasClass("e-active"), Is.False);

        await Page.Locator("#click-button-btn").ClickAsync();
        Assert.That(await Button.HasClass("e-active"), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_button()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-button-btn").ClickAsync();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(Button.Button).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_button_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-content-btn").ClickAsync();
        await Page.Locator("#set-css-btn").ClickAsync();
        await Page.Locator("#set-primary-btn").ClickAsync();
        await Page.Locator("#set-toggle-btn").ClickAsync();
        await Page.Locator("#disable-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-content")).ToHaveTextAsync("Queued", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-disabled")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-css")).ToHaveTextAsync("e-success extra-class", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-primary")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-toggle")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("Queued:True:True:False", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
