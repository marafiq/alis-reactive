namespace Alis.Reactive.PlaywrightTests.Conditions;

/// <summary>
/// Browser-level verification that conditions compose freely with HTTP blocks.
/// Each section mirrors a unit test scenario from WhenMixingConditionsWithHttp,
/// WhenMixingCommandsAndConditions, and WhenUsingConditionsInsideResponseHandlers.
/// </summary>
[TestFixture]
public class WhenConditionFiresAfterHttp : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Conditions/HttpMixing");
        await WaitForTraceMessage("booted", 10000);
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 1: Condition AFTER HTTP — the core pipeline mode fix
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task condition_after_http_shows_badge_when_active()
    {
        await NavigateAndBoot();

        await Page.Locator("#s1-btn-active").ClickAsync();

        // HTTP response sets saved name
        await Expect(Page.Locator("#s1-status")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        // Outer condition evaluates active=true → badge visible
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task condition_after_http_hides_badge_when_inactive()
    {
        await NavigateAndBoot();

        await Page.Locator("#s1-btn-inactive").ClickAsync();

        await Expect(Page.Locator("#s1-status")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        // Outer condition evaluates active=false → badge hidden
        await Expect(Page.Locator("#s1-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task condition_after_http_re_evaluates_on_second_click()
    {
        await NavigateAndBoot();

        // First: active=true → badge shown
        await Page.Locator("#s1-btn-active").ClickAsync();
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Second: active=false → badge hidden (re-evaluation)
        await Page.Locator("#s1-btn-inactive").ClickAsync();
        await Expect(Page.Locator("#s1-status")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-badge")).ToBeHiddenAsync();

        // Third: active=true again → badge shown again
        await Page.Locator("#s1-btn-active").ClickAsync();
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 2: HTTP BETWEEN two conditions
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task http_between_conditions_all_segments_fire_when_both_true()
    {
        await NavigateAndBoot();

        await Page.Locator("#s2-btn-active-with-count").ClickAsync();

        // First condition: active=true → "active"
        await Expect(Page.Locator("#s2-active-status")).ToHaveTextAsync("active", new() { Timeout = 5000 });
        // HTTP: audit logged
        await Expect(Page.Locator("#s2-audit-result")).ToHaveTextAsync("audited:login", new() { Timeout = 5000 });
        // Second condition: count=5 > 0 → badge visible
        await Expect(Page.Locator("#s2-count-badge")).ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task http_between_conditions_both_false_path()
    {
        await NavigateAndBoot();

        await Page.Locator("#s2-btn-inactive-zero").ClickAsync();

        // First condition: active=false → "inactive"
        await Expect(Page.Locator("#s2-active-status")).ToHaveTextAsync("inactive", new() { Timeout = 5000 });
        // HTTP still fires regardless of conditions
        await Expect(Page.Locator("#s2-audit-result")).ToHaveTextAsync("audited:login", new() { Timeout = 5000 });
        // Second condition: count=0 → badge hidden
        await Expect(Page.Locator("#s2-count-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task http_between_conditions_re_evaluates_independently()
    {
        await NavigateAndBoot();

        // First: both true
        await Page.Locator("#s2-btn-active-with-count").ClickAsync();
        await Expect(Page.Locator("#s2-active-status")).ToHaveTextAsync("active", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-count-badge")).ToBeVisibleAsync();

        // Then: both false — conditions re-evaluate independently
        await Page.Locator("#s2-btn-inactive-zero").ClickAsync();
        await Expect(Page.Locator("#s2-active-status")).ToHaveTextAsync("inactive", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-count-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 3: Full interleaving — commands + condition + HTTP + condition + commands
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task full_interleave_all_segments_fire_when_both_conditions_true()
    {
        await NavigateAndBoot();

        await Page.Locator("#s3-btn-all-true").ClickAsync();

        // Pre-HTTP commands
        await Expect(Page.Locator("#s3-header")).ToHaveTextAsync("start", new() { Timeout = 5000 });
        // First condition: active=true → pre-badge shown
        await Expect(Page.Locator("#s3-pre-badge")).ToBeVisibleAsync();
        // Mid-pipeline command
        await Expect(Page.Locator("#s3-loading")).ToHaveTextAsync("please wait");
        // HTTP response
        await Expect(Page.Locator("#s3-http-result")).ToHaveTextAsync("Bob", new() { Timeout = 5000 });
        // Second condition: count=3 > 0 → count shown
        await Expect(Page.Locator("#s3-count")).ToBeVisibleAsync();
        // Post-pipeline command
        await Expect(Page.Locator("#s3-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task full_interleave_commands_fire_even_when_conditions_false()
    {
        await NavigateAndBoot();

        await Page.Locator("#s3-btn-all-false").ClickAsync();

        // Commands always fire regardless of conditions
        await Expect(Page.Locator("#s3-header")).ToHaveTextAsync("start", new() { Timeout = 5000 });
        // Condition: active=false → pre-badge stays hidden
        await Expect(Page.Locator("#s3-pre-badge")).ToBeHiddenAsync();
        await Expect(Page.Locator("#s3-loading")).ToHaveTextAsync("please wait");
        // HTTP still fires
        await Expect(Page.Locator("#s3-http-result")).ToHaveTextAsync("Bob", new() { Timeout = 5000 });
        // Condition: count=0 → count hidden
        await Expect(Page.Locator("#s3-count")).ToBeHiddenAsync();
        // Post-pipeline command still fires
        await Expect(Page.Locator("#s3-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrorsExcept("400");
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 4: Condition INSIDE OnSuccess response handler
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task condition_inside_on_success_shows_badge_when_active()
    {
        await NavigateAndBoot();

        await Page.Locator("#s4-btn-active").ClickAsync();

        // Pre-HTTP command
        await Expect(Page.Locator("#s4-pre")).ToHaveTextAsync("loading", new() { Timeout = 5000 });
        // OnSuccess commands + inner condition
        await Expect(Page.Locator("#s4-status")).ToHaveTextAsync("saved", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s4-inner-badge")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s4-timestamp")).ToHaveTextAsync("now");
        // Post-HTTP command
        await Expect(Page.Locator("#s4-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task condition_inside_on_success_hides_badge_when_inactive()
    {
        await NavigateAndBoot();

        await Page.Locator("#s4-btn-inactive").ClickAsync();

        await Expect(Page.Locator("#s4-status")).ToHaveTextAsync("saved", new() { Timeout = 5000 });
        // Inner condition: active=false → badge hidden
        await Expect(Page.Locator("#s4-inner-badge")).ToBeHiddenAsync();
        // Commands after inner condition still fire
        await Expect(Page.Locator("#s4-timestamp")).ToHaveTextAsync("now");
        await Expect(Page.Locator("#s4-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task condition_inside_on_success_surrounding_commands_always_fire()
    {
        await NavigateAndBoot();

        // Active path
        await Page.Locator("#s4-btn-active").ClickAsync();
        await Expect(Page.Locator("#s4-pre")).ToHaveTextAsync("loading", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s4-status")).ToHaveTextAsync("saved", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s4-timestamp")).ToHaveTextAsync("now");
        await Expect(Page.Locator("#s4-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrorsExcept("400");
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 5: ElseIf chain inside OnSuccess + outer condition
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task elseif_inside_success_enterprise_tier_and_active_hides_trial()
    {
        await NavigateAndBoot();

        await Page.Locator("#s5-btn-enterprise").ClickAsync();

        // Server classifies as enterprise
        await Expect(Page.Locator("#s5-server-tier")).ToHaveTextAsync("enterprise", new() { Timeout = 5000 });
        // Client ElseIf: count=200 > 100 → gold
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("gold");
        // Outer condition: active=true → trial badge hidden
        await Expect(Page.Locator("#s5-trial-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task elseif_inside_success_business_tier()
    {
        await NavigateAndBoot();

        await Page.Locator("#s5-btn-business").ClickAsync();

        await Expect(Page.Locator("#s5-server-tier")).ToHaveTextAsync("business", new() { Timeout = 5000 });
        // Client ElseIf: count=75, 50 < 75 <= 100 → silver
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("silver");
        // active=true → trial hidden
        await Expect(Page.Locator("#s5-trial-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task elseif_inside_success_team_tier_and_inactive_shows_trial()
    {
        await NavigateAndBoot();

        await Page.Locator("#s5-btn-team").ClickAsync();

        await Expect(Page.Locator("#s5-server-tier")).ToHaveTextAsync("team", new() { Timeout = 5000 });
        // Client ElseIf: count=25, 10 < 25 <= 50 → bronze
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("bronze");
        // Outer condition: active=false → trial badge shown
        await Expect(Page.Locator("#s5-trial-badge")).ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task elseif_inside_success_individual_tier()
    {
        await NavigateAndBoot();

        await Page.Locator("#s5-btn-individual").ClickAsync();

        await Expect(Page.Locator("#s5-server-tier")).ToHaveTextAsync("individual", new() { Timeout = 5000 });
        // Client ElseIf: count=3 <= 10 → none (Else branch)
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("none");
        // active=false → trial shown
        await Expect(Page.Locator("#s5-trial-badge")).ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task elseif_chain_takes_only_first_matching_branch()
    {
        await NavigateAndBoot();

        // count=200 satisfies BOTH >100 AND >50 — only first branch fires
        await Page.Locator("#s5-btn-enterprise").ClickAsync();
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("gold", new() { Timeout = 5000 });

        // Switch to 75 — first branch (>100) fails, second (>50) matches
        await Page.Locator("#s5-btn-business").ClickAsync();
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("silver", new() { Timeout = 5000 });

        // Switch back to 200 — confirms branch exclusivity is stable
        await Page.Locator("#s5-btn-enterprise").ClickAsync();
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("gold", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 6: Multiple conditions after HTTP
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task multiple_conditions_after_http_both_match()
    {
        await NavigateAndBoot();

        await Page.Locator("#s6-btn-active-premium").ClickAsync();

        await Expect(Page.Locator("#s6-saved")).ToHaveTextAsync("Dave", new() { Timeout = 5000 });
        // First condition: active=true → badge shown
        await Expect(Page.Locator("#s6-active-badge")).ToBeVisibleAsync();
        // Second condition: category=premium → premium label shown
        await Expect(Page.Locator("#s6-premium-label")).ToBeVisibleAsync();
        // Post-conditions command
        await Expect(Page.Locator("#s6-footer")).ToHaveTextAsync("complete");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task multiple_conditions_after_http_first_true_second_false()
    {
        await NavigateAndBoot();

        await Page.Locator("#s6-btn-active-standard").ClickAsync();

        await Expect(Page.Locator("#s6-saved")).ToHaveTextAsync("Dave", new() { Timeout = 5000 });
        // active=true → badge shown
        await Expect(Page.Locator("#s6-active-badge")).ToBeVisibleAsync();
        // category=standard ≠ premium → premium label hidden
        await Expect(Page.Locator("#s6-premium-label")).ToBeHiddenAsync();
        await Expect(Page.Locator("#s6-footer")).ToHaveTextAsync("complete");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task multiple_conditions_after_http_first_false_second_true()
    {
        await NavigateAndBoot();

        await Page.Locator("#s6-btn-inactive-premium").ClickAsync();

        await Expect(Page.Locator("#s6-saved")).ToHaveTextAsync("Dave", new() { Timeout = 5000 });
        // active=false → badge stays hidden (Then-only, no Else)
        await Expect(Page.Locator("#s6-active-badge")).ToBeHiddenAsync();
        // category=premium → premium label shown
        await Expect(Page.Locator("#s6-premium-label")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s6-footer")).ToHaveTextAsync("complete");

        AssertNoConsoleErrorsExcept("400");
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 7: Condition inside OnError handler
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task condition_inside_on_error_takes_then_when_category_matches()
    {
        await NavigateAndBoot();

        await Page.Locator("#s7-btn-required").ClickAsync();

        // OnError fires (400), inner condition: category="required" → Then branch
        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("missing required fields", new() { Timeout = 5000 });
        // OnSuccess should NOT have fired
        await Expect(Page.Locator("#s7-status")).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task condition_inside_on_error_takes_else_when_category_differs()
    {
        await NavigateAndBoot();

        await Page.Locator("#s7-btn-other").ClickAsync();

        // OnError fires, inner condition: category="format" ≠ "required" → Else branch
        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("validation error", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task condition_inside_on_error_re_evaluates_on_different_category()
    {
        await NavigateAndBoot();

        // First: required → Then branch
        await Page.Locator("#s7-btn-required").ClickAsync();
        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("missing required fields", new() { Timeout = 5000 });

        // Second: format → Else branch
        await Page.Locator("#s7-btn-other").ClickAsync();
        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("validation error", new() { Timeout = 5000 });

        // Third: required again → back to Then
        await Page.Locator("#s7-btn-required").ClickAsync();
        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("missing required fields", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 8: And guard inside OnSuccess
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task and_guard_inside_success_shows_when_both_conditions_pass()
    {
        await NavigateAndBoot();

        // active=true AND count=10 > 5 → both pass → qualified shown
        await Page.Locator("#s8-btn-qualified").ClickAsync();

        await Expect(Page.Locator("#s8-saved")).ToHaveTextAsync("Eve", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s8-qualified")).ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task and_guard_inside_success_hides_when_count_fails()
    {
        await NavigateAndBoot();

        // active=true but count=2 <= 5 → AND fails → qualified hidden
        await Page.Locator("#s8-btn-not-qualified").ClickAsync();

        await Expect(Page.Locator("#s8-saved")).ToHaveTextAsync("Eve", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s8-qualified")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task and_guard_inside_success_hides_when_active_fails()
    {
        await NavigateAndBoot();

        // active=false, count=10 > 5 → first condition fails → AND short-circuits → hidden
        await Page.Locator("#s8-btn-inactive").ClickAsync();

        await Expect(Page.Locator("#s8-saved")).ToHaveTextAsync("Eve", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s8-qualified")).ToBeHiddenAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task and_guard_inside_success_transitions_between_qualified_and_not()
    {
        await NavigateAndBoot();

        // Qualified
        await Page.Locator("#s8-btn-qualified").ClickAsync();
        await Expect(Page.Locator("#s8-qualified")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Not qualified — AND fails
        await Page.Locator("#s8-btn-not-qualified").ClickAsync();
        await Expect(Page.Locator("#s8-saved")).ToHaveTextAsync("Eve", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s8-qualified")).ToBeHiddenAsync();

        // Qualified again
        await Page.Locator("#s8-btn-qualified").ClickAsync();
        await Expect(Page.Locator("#s8-qualified")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    // ════════════════════════════════════════════════════════════════════
    // Page-level checks
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task page_renders_with_correct_title()
    {
        await NavigateTo("/Sandbox/Conditions/HttpMixing");
        await Expect(Page).ToHaveTitleAsync("Conditions + HTTP Mixing — Alis.Reactive Sandbox");
        AssertNoConsoleErrorsExcept("400");
    }
}
