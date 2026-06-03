using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.CheckBox;

[TestFixture]
public class WhenUsingFusionCheckBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionCheckBox";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FusionCheckBoxModel";
    private const string ConsentAcceptedId = Scope + "__ConsentAccepted";
    private const string ReviewNeededId = Scope + "__ReviewNeeded";

    private FusionCheckBoxLocator ConsentAccepted => new(Page, ConsentAcceptedId);
    private FusionCheckBoxLocator ReviewNeeded => new(Page, ReviewNeededId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#checked-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionCheckBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_checkbox_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(ConsentAcceptedId));
        Assert.That(planJson, Does.Contain(ReviewNeededId));
        Assert.That(planJson, Does.Contain("\"checked\""));
        Assert.That(planJson, Does.Contain("\"indeterminate\""));
        Assert.That(planJson, Does.Contain("\"disabled\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"click\""));
        Assert.That(planJson, Does.Contain("\"focusIn\""));
        Assert.That(planJson, Does.Contain("\"change\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_checked_and_indeterminate_state_sources()
    {
        await NavigateAndBoot();

        Assert.That(await ConsentAccepted.IsChecked(), Is.True);
        Assert.That(await ConsentAccepted.FrameHasClass("e-check"), Is.True);
        await Expect(Page.Locator("#checked-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        Assert.That(await ReviewNeeded.IsIndeterminate(), Is.True);
        Assert.That(await ReviewNeeded.FrameHasClass("e-stop"), Is.True);
        await Expect(Page.Locator("#indeterminate-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_checked_updates_visible_state_source_and_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-consent-btn").ClickAsync();
        await Expect(Page.Locator("#checked-state")).ToHaveTextAsync("accepted", new() { Timeout = 5000 });

        await Page.Locator("#set-unchecked-btn").ClickAsync();
        Assert.That(await ConsentAccepted.IsChecked(), Is.False);
        Assert.That(await ConsentAccepted.FrameHasClass("e-check"), Is.False);
        await Expect(Page.Locator("#checked-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });

        await Page.Locator("#check-consent-btn").ClickAsync();
        await Expect(Page.Locator("#checked-state")).ToHaveTextAsync("missing", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_indeterminate_updates_visible_state_and_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-indeterminate-btn").ClickAsync();

        Assert.That(await ReviewNeeded.IsIndeterminate(), Is.True);
        Assert.That(await ReviewNeeded.FrameHasClass("e-stop"), Is.True);
        await Expect(Page.Locator("#indeterminate-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("indeterminate set", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_disabled_updates_visible_state_and_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#disable-review-btn").ClickAsync();
        await Expect(ReviewNeeded.Input).ToBeDisabledAsync(new() { Timeout = 5000 });
        Assert.That(await ReviewNeeded.WrapperHasClass("e-checkbox-disabled"), Is.True);
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Page.Locator("#enable-review-btn").ClickAsync();
        await Expect(ReviewNeeded.Input).ToBeEnabledAsync(new() { Timeout = 5000 });
        Assert.That(await ReviewNeeded.WrapperHasClass("e-checkbox-disabled"), Is.False);
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task click_method_toggles_checked_state_and_change_event_reads_payload()
    {
        await NavigateAndBoot();

        await Page.Locator("#click-consent-btn").ClickAsync();

        await Expect(Page.Locator("#click-state")).ToHaveTextAsync("click called", new() { Timeout = 5000 });
        Assert.That(await ConsentAccepted.IsChecked(), Is.False);
        await Expect(Page.Locator("#change-checked")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("unchecked", new() { Timeout = 5000 });

        await Page.Locator("#click-consent-btn").ClickAsync();
        Assert.That(await ConsentAccepted.IsChecked(), Is.True);
        await Expect(Page.Locator("#change-checked")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("checked", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_checkbox()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-consent-btn").ClickAsync();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(ConsentAccepted.Input).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_checkbox_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-unchecked-btn").ClickAsync();
        await Page.Locator("#set-indeterminate-btn").ClickAsync();
        await Page.Locator("#disable-review-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-checked")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-indeterminate")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-disabled")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("False:True:True", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
