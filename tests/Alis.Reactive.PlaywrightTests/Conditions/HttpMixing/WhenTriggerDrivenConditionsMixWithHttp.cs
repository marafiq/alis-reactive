namespace Alis.Reactive.PlaywrightTests.Conditions.HttpMixing;

/// <summary>
/// Playwright coverage that Active Plan trigger pipelines compose conditions,
/// HTTP blocks, response routes, dispatches, plugins, and chained route gathers.
/// </summary>
[TestFixture]
public class WhenTriggerDrivenConditionsMixWithHttp : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Conditions/HttpMixing");
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task condition_after_http_shows_badge_when_active()
    {
        await NavigateAndBoot();

        await Page.Locator("#s1-btn-active").ClickAsync();

        await Expect(Page.Locator("#s1-status")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task condition_after_http_hides_badge_when_inactive()
    {
        await NavigateAndBoot();

        await Page.Locator("#s1-btn-inactive").ClickAsync();

        await Expect(Page.Locator("#s1-status")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task condition_after_http_re_evaluates_on_second_click()
    {
        await NavigateAndBoot();

        await Page.Locator("#s1-btn-active").ClickAsync();
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#s1-btn-inactive").ClickAsync();
        await Expect(Page.Locator("#s1-status")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-badge")).ToBeHiddenAsync();

        await Page.Locator("#s1-btn-active").ClickAsync();
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task http_between_conditions_all_segments_fire_when_both_true()
    {
        await NavigateAndBoot();

        await Page.Locator("#s2-btn-active-with-count").ClickAsync();

        await Expect(Page.Locator("#s2-active-status")).ToHaveTextAsync("active", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-audit-result")).ToHaveTextAsync("audited:login", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-count-badge")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task http_between_conditions_both_false_path()
    {
        await NavigateAndBoot();

        await Page.Locator("#s2-btn-inactive-zero").ClickAsync();

        await Expect(Page.Locator("#s2-active-status")).ToHaveTextAsync("inactive", new() { Timeout = 5000 });
        // The HTTP segment is independent of the surrounding condition branches.
        await Expect(Page.Locator("#s2-audit-result")).ToHaveTextAsync("audited:login", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-count-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task http_between_conditions_re_evaluates_independently()
    {
        await NavigateAndBoot();

        await Page.Locator("#s2-btn-active-with-count").ClickAsync();
        await Expect(Page.Locator("#s2-active-status")).ToHaveTextAsync("active", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-count-badge")).ToBeVisibleAsync();

        await Page.Locator("#s2-btn-inactive-zero").ClickAsync();
        await Expect(Page.Locator("#s2-active-status")).ToHaveTextAsync("inactive", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-count-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task full_interleave_all_segments_fire_when_both_conditions_true()
    {
        await NavigateAndBoot();

        await Page.Locator("#s3-btn-all-true").ClickAsync();

        await Expect(Page.Locator("#s3-header")).ToHaveTextAsync("start", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s3-pre-badge")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s3-loading")).ToHaveTextAsync("please wait");
        await Expect(Page.Locator("#s3-http-result")).ToHaveTextAsync("Bob", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s3-count")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s3-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task full_interleave_commands_fire_even_when_conditions_false()
    {
        await NavigateAndBoot();

        await Page.Locator("#s3-btn-all-false").ClickAsync();

        await Expect(Page.Locator("#s3-header")).ToHaveTextAsync("start", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s3-pre-badge")).ToBeHiddenAsync();
        await Expect(Page.Locator("#s3-loading")).ToHaveTextAsync("please wait");
        await Expect(Page.Locator("#s3-http-result")).ToHaveTextAsync("Bob", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s3-count")).ToBeHiddenAsync();
        await Expect(Page.Locator("#s3-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task condition_inside_on_success_shows_badge_when_active()
    {
        await NavigateAndBoot();

        await Page.Locator("#s4-btn-active").ClickAsync();

        await Expect(Page.Locator("#s4-pre")).ToHaveTextAsync("loading", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s4-status")).ToHaveTextAsync("saved", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s4-inner-badge")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s4-timestamp")).ToHaveTextAsync("now");
        await Expect(Page.Locator("#s4-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task condition_inside_on_success_hides_badge_when_inactive()
    {
        await NavigateAndBoot();

        await Page.Locator("#s4-btn-inactive").ClickAsync();

        await Expect(Page.Locator("#s4-status")).ToHaveTextAsync("saved", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s4-inner-badge")).ToBeHiddenAsync();
        await Expect(Page.Locator("#s4-timestamp")).ToHaveTextAsync("now");
        await Expect(Page.Locator("#s4-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task condition_inside_on_success_surrounding_commands_always_fire()
    {
        await NavigateAndBoot();

        await Page.Locator("#s4-btn-active").ClickAsync();
        await Expect(Page.Locator("#s4-pre")).ToHaveTextAsync("loading", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s4-status")).ToHaveTextAsync("saved", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s4-timestamp")).ToHaveTextAsync("now");
        await Expect(Page.Locator("#s4-footer")).ToHaveTextAsync("done");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_inside_success_enterprise_tier_and_active_hides_trial()
    {
        await NavigateAndBoot();

        await Page.Locator("#s5-btn-enterprise").ClickAsync();

        await Expect(Page.Locator("#s5-server-tier")).ToHaveTextAsync("enterprise", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("gold");
        await Expect(Page.Locator("#s5-trial-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_inside_success_business_tier()
    {
        await NavigateAndBoot();

        await Page.Locator("#s5-btn-business").ClickAsync();

        await Expect(Page.Locator("#s5-server-tier")).ToHaveTextAsync("business", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("silver");
        await Expect(Page.Locator("#s5-trial-badge")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_inside_success_team_tier_and_inactive_shows_trial()
    {
        await NavigateAndBoot();

        await Page.Locator("#s5-btn-team").ClickAsync();

        await Expect(Page.Locator("#s5-server-tier")).ToHaveTextAsync("team", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("bronze");
        await Expect(Page.Locator("#s5-trial-badge")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_inside_success_individual_tier()
    {
        await NavigateAndBoot();

        await Page.Locator("#s5-btn-individual").ClickAsync();

        await Expect(Page.Locator("#s5-server-tier")).ToHaveTextAsync("individual", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("none");
        await Expect(Page.Locator("#s5-trial-badge")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_chain_takes_only_first_matching_branch()
    {
        await NavigateAndBoot();

        // First matching ElseIf branch wins even when later guards would also match.
        await Page.Locator("#s5-btn-enterprise").ClickAsync();
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("gold", new() { Timeout = 5000 });

        await Page.Locator("#s5-btn-business").ClickAsync();
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("silver", new() { Timeout = 5000 });

        await Page.Locator("#s5-btn-enterprise").ClickAsync();
        await Expect(Page.Locator("#s5-client-tier")).ToHaveTextAsync("gold", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task multiple_conditions_after_http_both_match()
    {
        await NavigateAndBoot();

        await Page.Locator("#s6-btn-active-premium").ClickAsync();

        await Expect(Page.Locator("#s6-saved")).ToHaveTextAsync("Dave", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s6-active-badge")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s6-premium-label")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s6-footer")).ToHaveTextAsync("complete");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task multiple_conditions_after_http_first_true_second_false()
    {
        await NavigateAndBoot();

        await Page.Locator("#s6-btn-active-standard").ClickAsync();

        await Expect(Page.Locator("#s6-saved")).ToHaveTextAsync("Dave", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s6-active-badge")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s6-premium-label")).ToBeHiddenAsync();
        await Expect(Page.Locator("#s6-footer")).ToHaveTextAsync("complete");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task multiple_conditions_after_http_first_false_second_true()
    {
        await NavigateAndBoot();

        await Page.Locator("#s6-btn-inactive-premium").ClickAsync();

        await Expect(Page.Locator("#s6-saved")).ToHaveTextAsync("Dave", new() { Timeout = 5000 });
        // Then-only conditions leave the previous/default state untouched when the guard fails.
        await Expect(Page.Locator("#s6-active-badge")).ToBeHiddenAsync();
        await Expect(Page.Locator("#s6-premium-label")).ToBeVisibleAsync();
        await Expect(Page.Locator("#s6-footer")).ToHaveTextAsync("complete");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task condition_inside_on_error_takes_then_when_category_matches()
    {
        await NavigateAndBoot();

        await Page.Locator("#s7-btn-required").ClickAsync();

        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("missing required fields", new() { Timeout = 5000 });
        // Error routing must leave success-route mutations untouched.
        await Expect(Page.Locator("#s7-status")).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task condition_inside_on_error_takes_else_when_category_differs()
    {
        await NavigateAndBoot();

        await Page.Locator("#s7-btn-other").ClickAsync();

        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("validation error", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task condition_inside_on_error_re_evaluates_on_different_category()
    {
        await NavigateAndBoot();

        await Page.Locator("#s7-btn-required").ClickAsync();
        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("missing required fields", new() { Timeout = 5000 });

        await Page.Locator("#s7-btn-other").ClickAsync();
        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("validation error", new() { Timeout = 5000 });

        await Page.Locator("#s7-btn-required").ClickAsync();
        await Expect(Page.Locator("#s7-error-msg")).ToHaveTextAsync("missing required fields", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task and_guard_inside_success_shows_when_both_conditions_pass()
    {
        await NavigateAndBoot();

        await Page.Locator("#s8-btn-qualified").ClickAsync();

        await Expect(Page.Locator("#s8-saved")).ToHaveTextAsync("Eve", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s8-qualified")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_guard_inside_success_hides_when_count_fails()
    {
        await NavigateAndBoot();

        await Page.Locator("#s8-btn-not-qualified").ClickAsync();

        await Expect(Page.Locator("#s8-saved")).ToHaveTextAsync("Eve", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s8-qualified")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_guard_inside_success_hides_when_active_fails()
    {
        await NavigateAndBoot();

        await Page.Locator("#s8-btn-inactive").ClickAsync();

        await Expect(Page.Locator("#s8-saved")).ToHaveTextAsync("Eve", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s8-qualified")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_guard_inside_success_transitions_between_qualified_and_not()
    {
        await NavigateAndBoot();

        await Page.Locator("#s8-btn-qualified").ClickAsync();
        await Expect(Page.Locator("#s8-qualified")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#s8-btn-not-qualified").ClickAsync();
        await Expect(Page.Locator("#s8-saved")).ToHaveTextAsync("Eve", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s8-qualified")).ToBeHiddenAsync();

        await Page.Locator("#s8-btn-qualified").ClickAsync();
        await Expect(Page.Locator("#s8-qualified")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ordered_mixed_modules_after_http_run_after_response_routes()
    {
        await NavigateAndBoot();

        await Page.Locator("#s12-btn-run").ClickAsync();

        await Expect(Page.Locator("#s12-http-done")).ToHaveTextAsync("done", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s12-plugin")).ToHaveTextAsync("after-http");
        await Expect(Page.Locator("#s12-order")).ToHaveTextAsync("branch-after");
        await Expect(Page.Locator("#s12-sequence")).ToHaveTextAsync(
            "start>branch-before>http-success>dispatch>plugin>tail>branch-after");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ordered_mixed_modules_after_http_runs_else_branches_in_order()
    {
        await NavigateAndBoot();

        await Page.EvaluateAsync(
            "() => document.dispatchEvent(new CustomEvent('s12-ordered-mixed-modules', { detail: { Active: false, Category: 'standard', Count: 0 } }))");

        await Expect(Page.Locator("#s12-http-done")).ToHaveTextAsync("done", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s12-plugin")).ToHaveTextAsync("after-http");
        await Expect(Page.Locator("#s12-order")).ToHaveTextAsync("branch-after-else");
        await Expect(Page.Locator("#s12-sequence")).ToHaveTextAsync(
            "start>branch-before-else>http-success>dispatch>plugin>tail>branch-after-else");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ordered_mixed_modules_after_parallel_wait_for_all_settled_reactions()
    {
        await NavigateAndBoot();

        await Page.Locator("#s13-btn-run").ClickAsync();

        await Expect(Page.Locator("#s13-a")).ToHaveTextAsync("Parallel A", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s13-b")).ToHaveTextAsync("audited:parallel-b", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s13-all")).ToHaveTextAsync("settled");
        await Expect(Page.Locator("#s13-plugin")).ToHaveTextAsync("after-parallel");
        await Expect(Page.Locator("#s13-order")).ToHaveTextAsync("branch-after");
        await Expect(Page.Locator("#s13-sequence")).ToHaveTextAsync(
            "start>branch-before>settled>dispatch>plugin>tail>branch-after");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ordered_mixed_modules_after_parallel_runs_else_branches_in_order()
    {
        await NavigateAndBoot();

        await Page.EvaluateAsync(
            "() => document.dispatchEvent(new CustomEvent('s13-ordered-parallel-mix', { detail: { Active: false, Category: 'standard', Count: 0 } }))");

        await Expect(Page.Locator("#s13-a")).ToHaveTextAsync("Parallel A", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s13-b")).ToHaveTextAsync("audited:parallel-b", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s13-all")).ToHaveTextAsync("settled");
        await Expect(Page.Locator("#s13-plugin")).ToHaveTextAsync("after-parallel");
        await Expect(Page.Locator("#s13-order")).ToHaveTextAsync("branch-after-else");
        await Expect(Page.Locator("#s13-sequence")).ToHaveTextAsync(
            "start>branch-before-else>settled>dispatch>plugin>tail>branch-after-else");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task route_template_gather_reads_event_payload_and_reuses_plugin_source_in_chained_request()
    {
        await NavigateAndBoot();

        await Page.Locator("#s14-btn-active-high").ClickAsync();

        await Expect(Page.Locator("#s14-route-id")).ToHaveTextAsync("314", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s14-body-action")).ToHaveTextAsync("VIP Review");
        await Expect(Page.Locator("#s14-header-category")).ToHaveTextAsync("VIP Review");
        await Expect(Page.Locator("#s14-trail-id")).ToHaveTextAsync("314");
        await Expect(Page.Locator("#s14-trail-slug")).ToHaveTextAsync("vip-review");
        await Expect(Page.Locator("#s14-trail-step")).ToHaveTextAsync("chained");
        await Expect(Page.Locator("#s14-plugin")).ToHaveTextAsync("vip-review");
        await Expect(Page.Locator("#s14-order")).ToHaveTextAsync("branch-after");
        await Expect(Page.Locator("#s14-sequence")).ToHaveTextAsync(
            "start>branch-before>http-success>trail-success>dispatch>plugin>branch-after");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task route_template_gather_re_evaluates_event_payload_conditions_and_chained_route_params()
    {
        await NavigateAndBoot();

        await Page.Locator("#s14-btn-active-high").ClickAsync();
        await Expect(Page.Locator("#s14-trail-slug")).ToHaveTextAsync("vip-review", new() { Timeout = 5000 });

        await Page.Locator("#s14-btn-inactive-low").ClickAsync();

        await Expect(Page.Locator("#s14-route-id")).ToHaveTextAsync("271", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s14-body-action")).ToHaveTextAsync("Routine Check");
        await Expect(Page.Locator("#s14-header-category")).ToHaveTextAsync("Routine Check");
        await Expect(Page.Locator("#s14-trail-id")).ToHaveTextAsync("271");
        await Expect(Page.Locator("#s14-trail-slug")).ToHaveTextAsync("routine-check");
        await Expect(Page.Locator("#s14-trail-step")).ToHaveTextAsync("chained");
        await Expect(Page.Locator("#s14-plugin")).ToHaveTextAsync("routine-check");
        await Expect(Page.Locator("#s14-order")).ToHaveTextAsync("branch-after-else");

        var sequence = await Page.Locator("#s14-sequence").TextContentAsync();
        Assert.That(sequence, Does.EndWith(
            "start>branch-before-else>http-success>trail-success>dispatch>plugin>branch-after-else"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task page_renders_with_correct_title()
    {
        await NavigateTo("/Sandbox/Conditions/HttpMixing");
        await Expect(Page).ToHaveTitleAsync("Conditions + HTTP Mixing — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }
}
