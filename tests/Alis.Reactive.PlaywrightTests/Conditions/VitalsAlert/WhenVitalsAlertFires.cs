using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Conditions.VitalsAlert;

/// <summary>
/// Exercises condition branches that mix FusionNumericTextBox input with HTTP
/// requests, severity tiers, and before/after reaction sequencing.
/// </summary>
[TestFixture]
public class WhenVitalsAlertFires : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/VitalsAlert";

    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_VitalsAlertModel";
    private const string HeartRateId = GeneratedTypeScope + "__HeartRate";

    private NumericTextBoxLocator HeartRate => new(Page, HeartRateId);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task high_heart_rate_posts_alert_and_shows_server_confirmation()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("160");

        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-alert-status"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });

        var timestamp = await Page.Locator("#s1-alert-time").TextContentAsync();
        Assert.That(timestamp, Is.Not.Empty.And.Not.EqualTo("\u2014"),
            "Server timestamp must be populated from HTTP response");

        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task normal_heart_rate_shows_vitals_normal_without_http()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("80");

        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-alert-status"))
            .ToHaveTextAsync("Vitals normal", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-alert-time"))
            .ToHaveTextAsync("", new() { Timeout = 3000 });

        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reactions_before_condition_always_execute_regardless_of_branch()
    {
        await NavigateAndBoot();

        // Use a value different from the initial 72 so the component raises change.
        await HeartRate.FillAndBlur("80");

        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reactions_after_condition_always_execute_regardless_of_branch()
    {
        await NavigateAndBoot();

        // Use a value different from the initial 72 so the component raises change.
        await HeartRate.FillAndBlur("80");

        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reactions_before_and_after_execute_when_then_branch_posts()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("160");

        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-alert-status"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task transition_from_alert_to_normal_clears_timestamp()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("160");
        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-alert-status"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });

        await HeartRate.FillAndBlur("70");
        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-alert-status"))
            .ToHaveTextAsync("Vitals normal", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-alert-time"))
            .ToHaveTextAsync("", new() { Timeout = 3000 });
        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boundary_at_140_takes_else_branch_no_http()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("140");

        await Expect(Page.Locator("#s1-alert-status"))
            .ToHaveTextAsync("Vitals normal", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boundary_at_141_takes_then_branch_posts_alert()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("141");

        await Expect(Page.Locator("#s1-alert-status"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task critical_tier_posts_to_critical_endpoint()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("190");
        await Page.Locator("#s2-check-btn").ClickAsync();

        await Expect(Page.Locator("#s2-tier-status"))
            .ToContainTextAsync("CRITICAL", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-tier-level"))
            .ToHaveTextAsync("critical", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task warning_tier_posts_to_warning_endpoint()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("155");
        await Page.Locator("#s2-check-btn").ClickAsync();

        await Expect(Page.Locator("#s2-tier-status"))
            .ToContainTextAsync("WARNING", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-tier-level"))
            .ToHaveTextAsync("warning", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task normal_tier_shows_text_no_http()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("80");
        await Page.Locator("#s2-check-btn").ClickAsync();

        await Expect(Page.Locator("#s2-tier-status"))
            .ToHaveTextAsync("Normal — no alert needed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-tier-level"))
            .ToHaveTextAsync("normal", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_boundary_at_180_is_critical()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("180");
        await Page.Locator("#s2-check-btn").ClickAsync();

        await Expect(Page.Locator("#s2-tier-status"))
            .ToContainTextAsync("CRITICAL", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_boundary_at_140_is_warning()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("140");
        await Page.Locator("#s2-check-btn").ClickAsync();

        await Expect(Page.Locator("#s2-tier-status"))
            .ToContainTextAsync("WARNING", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task severity_transitions_across_all_tiers()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("80");
        await Page.Locator("#s2-check-btn").ClickAsync();
        await Expect(Page.Locator("#s2-tier-level"))
            .ToHaveTextAsync("normal", new() { Timeout = 5000 });

        await HeartRate.FillAndBlur("150");
        await Page.Locator("#s2-check-btn").ClickAsync();
        await Expect(Page.Locator("#s2-tier-level"))
            .ToHaveTextAsync("warning", new() { Timeout = 5000 });

        await HeartRate.FillAndBlur("200");
        await Page.Locator("#s2-check-btn").ClickAsync();
        await Expect(Page.Locator("#s2-tier-level"))
            .ToHaveTextAsync("critical", new() { Timeout = 5000 });

        await HeartRate.FillAndBlur("60");
        await Page.Locator("#s2-check-btn").ClickAsync();
        await Expect(Page.Locator("#s2-tier-level"))
            .ToHaveTextAsync("normal", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sandwich_before_and_after_run_when_else_branch_fires()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("72");
        await Page.Locator("#s3-sandwich-btn").ClickAsync();

        await Expect(Page.Locator("#s3-before"))
            .ToHaveTextAsync("before-ran", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s3-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        await Expect(Page.Locator("#s3-branch"))
            .ToHaveTextAsync("no alert", new() { Timeout = 3000 });

        await Expect(Page.Locator("#s3-after"))
            .ToHaveTextAsync("after-ran", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sandwich_before_and_after_run_when_then_branch_posts()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("160");
        await Page.Locator("#s3-sandwich-btn").ClickAsync();

        await Expect(Page.Locator("#s3-before"))
            .ToHaveTextAsync("before-ran", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s3-after"))
            .ToHaveTextAsync("after-ran", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s3-branch"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sandwich_transitions_preserve_before_and_after_across_branches()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("72");
        await Page.Locator("#s3-sandwich-btn").ClickAsync();
        await Expect(Page.Locator("#s3-before"))
            .ToHaveTextAsync("before-ran", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s3-branch"))
            .ToHaveTextAsync("no alert", new() { Timeout = 3000 });
        await Expect(Page.Locator("#s3-after"))
            .ToHaveTextAsync("after-ran", new() { Timeout = 3000 });

        await HeartRate.FillAndBlur("160");
        await Page.Locator("#s3-sandwich-btn").ClickAsync();
        await Expect(Page.Locator("#s3-before"))
            .ToHaveTextAsync("before-ran", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s3-branch"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s3-after"))
            .ToHaveTextAsync("after-ran", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
