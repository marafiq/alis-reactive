using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Conditions.WithComponents;

/// <summary>
/// Exercises realistic condition → HTTP patterns with FusionNumericTextBox:
///   Section 1: When(comp.Value()).Gt(140) → POST alert, Else → text only
///   Section 2: ElseIf severity ladder → different POST per tier
///   Section 3: Command sandwich — before + condition + after always execute
///
/// Page under test: /Sandbox/Conditions/VitalsAlert
///
/// This is the first test in the codebase where HTTP lives INSIDE a condition branch.
/// All prior condition tests only flip element text/visibility. This slice proves
/// condition branches can contain full HTTP pipelines with typed responses.
///
/// Senior living domain: nurse vital sign monitoring with automated alerts.
/// </summary>
[TestFixture]
public class WhenVitalsAlertFires : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/VitalsAlert";

    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_VitalsAlertModel";
    private const string HeartRateId = Scope + "__HeartRate";

    private NumericTextBoxLocator HeartRate => new(Page, HeartRateId);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    // ── Section 1: Condition → HTTP in Then ──

    [Test]
    public async Task high_heart_rate_posts_alert_and_shows_server_confirmation()
    {
        await NavigateAndBoot();

        // Enter HR > 140 → Then branch fires → POST /Alert → server returns message + timestamp
        await HeartRate.FillAndBlur("160");

        // Before condition: last-reading always updates with current value
        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // Condition output: server response (contains "Alert sent for HR")
        await Expect(Page.Locator("#s1-alert-status"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });

        // Condition output: timestamp should be non-empty (server-generated)
        var timestamp = await Page.Locator("#s1-alert-time").TextContentAsync();
        Assert.That(timestamp, Is.Not.Empty.And.Not.EqualTo("\u2014"),
            "Server timestamp must be populated from HTTP response");

        // After condition: check-status always updates
        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task normal_heart_rate_shows_vitals_normal_without_http()
    {
        await NavigateAndBoot();

        // Enter HR <= 140 → Else branch fires → text only, no HTTP
        await HeartRate.FillAndBlur("80");

        // Before condition: last-reading always updates with current value
        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // Condition output: "Vitals normal" text, no HTTP
        await Expect(Page.Locator("#s1-alert-status"))
            .ToHaveTextAsync("Vitals normal", new() { Timeout = 5000 });

        // Condition output: timestamp empty (no HTTP call happened)
        await Expect(Page.Locator("#s1-alert-time"))
            .ToHaveTextAsync("", new() { Timeout = 3000 });

        // After condition: check-status always updates
        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task commands_before_condition_always_execute_regardless_of_branch()
    {
        await NavigateAndBoot();

        // Enter normal HR (different from initial 72 to trigger change event)
        await HeartRate.FillAndBlur("80");

        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task commands_after_condition_always_execute_regardless_of_branch()
    {
        await NavigateAndBoot();

        // Enter normal HR (different from initial 72 to trigger change event)
        await HeartRate.FillAndBlur("80");

        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task commands_before_and_after_execute_when_then_branch_posts()
    {
        await NavigateAndBoot();

        // Enter high HR → both before and after commands execute even though Then POSTs
        await HeartRate.FillAndBlur("160");

        // Before: last-reading populated
        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        // After: check-status says "checked"
        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 5000 });

        // And the HTTP response also arrived
        await Expect(Page.Locator("#s1-alert-status"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task transition_from_alert_to_normal_clears_timestamp()
    {
        await NavigateAndBoot();

        // First trigger alert — full UI state after alert
        await HeartRate.FillAndBlur("160");
        await Expect(Page.Locator("#s1-last-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-alert-status"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-check-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });

        // Now enter normal — full UI state after transition
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

        // 140 is NOT > 140, should take Else
        await HeartRate.FillAndBlur("140");

        await Expect(Page.Locator("#s1-alert-status"))
            .ToHaveTextAsync("Vitals normal", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boundary_at_141_takes_then_branch_posts_alert()
    {
        await NavigateAndBoot();

        // 141 IS > 140, should POST alert
        await HeartRate.FillAndBlur("141");

        await Expect(Page.Locator("#s1-alert-status"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: ElseIf → different HTTP per severity tier ──

    [Test]
    public async Task critical_tier_posts_to_critical_endpoint()
    {
        await NavigateAndBoot();

        // Set HR to 190 (>= 180) → Critical tier
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

        // Set HR to 155 (>= 140 but < 180) → Warning tier
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

        // Set HR to 80 (< 140) → Normal tier, no HTTP
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

        // Normal → Warning → Critical → Normal
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

    // ── Section 3: Command sandwich ──

    [Test]
    public async Task sandwich_before_and_after_run_when_else_branch_fires()
    {
        await NavigateAndBoot();

        // HR = 72 → Else branch (no HTTP)
        await HeartRate.FillAndBlur("72");
        await Page.Locator("#s3-sandwich-btn").ClickAsync();

        // Before command
        await Expect(Page.Locator("#s3-before"))
            .ToHaveTextAsync("before-ran", new() { Timeout = 5000 });

        // Reading (component value)
        await Expect(Page.Locator("#s3-reading"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 3000 });

        // Branch (Else)
        await Expect(Page.Locator("#s3-branch"))
            .ToHaveTextAsync("no alert", new() { Timeout = 3000 });

        // After command
        await Expect(Page.Locator("#s3-after"))
            .ToHaveTextAsync("after-ran", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sandwich_before_and_after_run_when_then_branch_posts()
    {
        await NavigateAndBoot();

        // HR = 160 → Then branch (HTTP POST)
        await HeartRate.FillAndBlur("160");
        await Page.Locator("#s3-sandwich-btn").ClickAsync();

        // Before command — must run
        await Expect(Page.Locator("#s3-before"))
            .ToHaveTextAsync("before-ran", new() { Timeout = 5000 });

        // After command — must run even though Then contains HTTP
        await Expect(Page.Locator("#s3-after"))
            .ToHaveTextAsync("after-ran", new() { Timeout = 5000 });

        // Branch (Then → HTTP response)
        await Expect(Page.Locator("#s3-branch"))
            .ToContainTextAsync("Alert sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sandwich_transitions_preserve_before_and_after_across_branches()
    {
        await NavigateAndBoot();

        // First: Else path
        await HeartRate.FillAndBlur("72");
        await Page.Locator("#s3-sandwich-btn").ClickAsync();
        await Expect(Page.Locator("#s3-before"))
            .ToHaveTextAsync("before-ran", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s3-branch"))
            .ToHaveTextAsync("no alert", new() { Timeout = 3000 });
        await Expect(Page.Locator("#s3-after"))
            .ToHaveTextAsync("after-ran", new() { Timeout = 3000 });

        // Second: Then path — before/after still run, branch changes
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
