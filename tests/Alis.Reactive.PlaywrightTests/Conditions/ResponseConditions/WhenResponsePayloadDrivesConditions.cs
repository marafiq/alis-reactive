namespace Alis.Reactive.PlaywrightTests.Conditions.ResponseConditions;

/// <summary>
/// Browser-level verification that response body properties drive conditions
/// inside OnSuccess and OnError handlers. Each section proves one capability
/// of the uniform typed access DSL: When, ElseIf, And, OnError catch-all,
/// and OnError typed with conditions.
/// </summary>
[TestFixture]
public class WhenResponsePayloadDrivesConditions : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Conditions/ResponseConditions");
        await WaitForTraceMessage("booted", 10000);
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 1: When on success response body
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task approved_response_shows_badge_and_sets_result()
    {
        await NavigateAndBoot();
        await Page.Locator("#s1-btn-approve").ClickAsync();

        await Expect(Page.Locator("#s1-result")).ToHaveTextAsync("approved", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task denied_response_hides_badge_and_sets_not_approved()
    {
        await NavigateAndBoot();
        await Page.Locator("#s1-btn-deny").ClickAsync();

        await Expect(Page.Locator("#s1-result")).ToHaveTextAsync("not approved", new() { Timeout = 5000 });
        await Expect(Page.Locator("#s1-badge")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task badge_toggles_between_approve_and_deny()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Page.Locator("#s1-btn-approve"));
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(Page.Locator("#s1-btn-deny"));
        await Expect(Page.Locator("#s1-badge")).ToBeHiddenAsync(new() { Timeout = 5000 });

        await ClickWhenStable(Page.Locator("#s1-btn-approve"));
        await Expect(Page.Locator("#s1-badge")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 2: ElseIf chain on response body
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task elseif_chain_shows_green_for_approved()
    {
        await NavigateAndBoot();
        await Page.Locator("#s2-btn-approve").ClickAsync();

        await Expect(Page.Locator("#s2-label")).ToHaveTextAsync("green", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_chain_shows_yellow_for_pending()
    {
        await NavigateAndBoot();
        await Page.Locator("#s2-btn-pending").ClickAsync();

        await Expect(Page.Locator("#s2-label")).ToHaveTextAsync("yellow", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_chain_shows_red_for_denied()
    {
        await NavigateAndBoot();
        await Page.Locator("#s2-btn-deny").ClickAsync();

        await Expect(Page.Locator("#s2-label")).ToHaveTextAsync("red", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 3: And composition on response body
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task and_guard_passes_when_both_conditions_met()
    {
        await NavigateAndBoot();
        await Page.Locator("#s3-btn-approve").ClickAsync();

        await Expect(Page.Locator("#s3-result")).ToHaveTextAsync("approved with items", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_guard_fails_when_status_wrong()
    {
        await NavigateAndBoot();
        await Page.Locator("#s3-btn-deny").ClickAsync();

        await Expect(Page.Locator("#s3-result")).ToHaveTextAsync("condition not met", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 4: OnError catch-all
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task error_catchall_fires_on_422()
    {
        await NavigateAndBoot();
        await Page.Locator("#s4-btn-fail").ClickAsync();

        await Expect(Page.Locator("#s4-result")).ToHaveTextAsync("error caught", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("422");
    }

    // ════════════════════════════════════════════════════════════════════
    // Section 5: OnError typed with conditions
    // ════════════════════════════════════════════════════════════════════

    [Test]
    public async Task typed_error_condition_matches_422_code()
    {
        await NavigateAndBoot();
        await Page.Locator("#s5-btn-fail").ClickAsync();

        await Expect(Page.Locator("#s5-result")).ToHaveTextAsync("validation error", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("422");
    }
}
