using System.Globalization;
using Alis.Reactive.Playwright.Extensions;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ProgressButton;

[TestFixture]
public class WhenUsingFusionProgressButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionProgressButton";
    private const string ButtonId = "resident-progress-button";

    private FusionProgressButtonLocator Button => new(Page, ButtonId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#content-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionProgressButton — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_progress_button_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(ButtonId));
        Assert.That(planJson, Does.Contain("\"content\""));
        Assert.That(planJson, Does.Contain("\"disabled\""));
        Assert.That(planJson, Does.Contain("\"cssClass\""));
        Assert.That(planJson, Does.Contain("\"enableProgress\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"start\""));
        Assert.That(planJson, Does.Contain("\"startAt\""));
        Assert.That(planJson, Does.Contain("\"progressComplete\""));
        Assert.That(planJson, Does.Contain("\"focusIn\""));
        Assert.That(planJson, Does.Contain("\"begin\""));
        Assert.That(planJson, Does.Contain("\"progress\""));
        Assert.That(planJson, Does.Contain("\"end\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_visible_content_and_enables_progress_source()
    {
        await NavigateAndBoot();

        await Expect(Button.Content).ToHaveTextAsync("Ready Save", new() { Timeout = 5000 });
        await Expect(Page.Locator("#content-echo")).ToHaveTextAsync("Ready Save", new() { Timeout = 5000 });
        await Expect(Page.Locator("#progress-enabled-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Button.Progress).ToHaveCountAsync(1, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_content_and_disabled_update_visible_state_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-content-btn").ClickAsync();
        await Expect(Button.Content).ToHaveTextAsync("Discharge Save", new() { Timeout = 5000 });
        await Expect(Page.Locator("#content-echo")).ToHaveTextAsync("Discharge Save", new() { Timeout = 5000 });

        await Page.Locator("#disable-progress-button-btn").ClickAsync();
        await Expect(Button.Button).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Page.Locator("#enable-progress-button-btn").ClickAsync();
        await Expect(Button.Button).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_css_and_progress_enabled_update_dom_sources_and_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-css-btn").ClickAsync();
        Assert.That(await Button.HasClass("e-success"), Is.True);
        Assert.That(await Button.HasClass("extra-class"), Is.True);
        await Expect(Page.Locator("#css-echo")).ToHaveTextAsync("e-success extra-class", new() { Timeout = 5000 });

        await Page.Locator("#check-progress-enabled-btn").ClickAsync();
        await Expect(Page.Locator("#progress-enabled-state")).ToHaveTextAsync("progress enabled", new() { Timeout = 5000 });

        await Page.Locator("#disable-progress-btn").ClickAsync();
        await Expect(Button.Progress).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Page.Locator("#progress-enabled-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });

        await Page.Locator("#check-progress-enabled-btn").ClickAsync();
        await Expect(Page.Locator("#progress-enabled-state")).ToHaveTextAsync("progress disabled", new() { Timeout = 5000 });

        await Page.Locator("#enable-progress-btn").ClickAsync();
        await Expect(Button.Progress).ToHaveCountAsync(1, new() { Timeout = 5000 });
        await Expect(Page.Locator("#progress-enabled-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task start_method_fires_begin_payload_and_complete_fires_end_payload()
    {
        await NavigateAndBoot();

        await Page.Locator("#start-progress-btn").ClickAsync();
        await Expect(Page.Locator("#start-state")).ToHaveTextAsync("start called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#begin-percent")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#begin-step")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#begin-condition")).ToHaveTextAsync("begin zero", new() { Timeout = 5000 });

        await Page.Locator("#complete-progress-btn").ClickAsync();
        await Expect(Page.Locator("#complete-state")).ToHaveTextAsync("complete called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#end-percent")).ToHaveTextAsync("100", new() { Timeout = 5000 });
        await Expect(Page.Locator("#end-step")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#end-condition")).ToHaveTextAsync("complete", new() { Timeout = 5000 });
        Assert.That(await Button.IsProgressActive(), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task start_at_method_fires_progress_payload_and_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#start-at-progress-btn").ClickAsync();
        await Expect(Page.Locator("#start-state")).ToHaveTextAsync("start at 60 called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#progress-condition")).ToHaveTextAsync("at least sixty", new() { Timeout = 5000 });

        var progressPercent = await NumericText(Page.Locator("#progress-percent"));
        Assert.That(progressPercent, Is.GreaterThanOrEqualTo(60d));
        await Expect(Page.Locator("#progress-step")).ToHaveTextAsync("1", new() { Timeout = 5000 });

        await Page.Locator("#complete-progress-btn").ClickAsync();
        await Expect(Page.Locator("#end-percent")).ToHaveTextAsync("100", new() { Timeout = 5000 });
        await Expect(Page.Locator("#end-condition")).ToHaveTextAsync("complete", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_progress_button()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-progress-btn").ClickAsync();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(Button.Button).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_progress_button_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-content-btn").ClickAsync();
        await Page.Locator("#set-css-btn").ClickAsync();
        await Page.Locator("#disable-progress-button-btn").ClickAsync();
        await Page.Locator("#disable-progress-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-content")).ToHaveTextAsync("Discharge Save", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-disabled")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-css")).ToHaveTextAsync("e-success extra-class", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-progress-enabled")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("Discharge Save:True:e-success extra-class:False", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    private static async Task<double> NumericText(ILocator locator)
    {
        var text = await locator.TextContentAsync();
        return double.Parse(text ?? string.Empty, CultureInfo.InvariantCulture);
    }
}
