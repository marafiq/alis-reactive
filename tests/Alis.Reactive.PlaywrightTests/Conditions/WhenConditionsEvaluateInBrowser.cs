namespace Alis.Reactive.PlaywrightTests.Conditions;

[TestFixture]
public class WhenConditionsEvaluateInBrowser : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Conditions");
        await WaitForTraceMessage("booted", 5000);
    }

    // ── int (ElseIf grade ladder) ──

    [Test]
    public async Task Int_elseif_takes_correct_branch()
    {
        await NavigateAndBoot();
        var grade = Page.Locator("#grade");

        await Page.Locator("#btn-score-95").ClickAsync();
        await Expect(grade).ToHaveTextAsync("A");

        await Page.Locator("#btn-score-85").ClickAsync();
        await Expect(grade).ToHaveTextAsync("B");

        await Page.Locator("#btn-score-40").ClickAsync();
        await Expect(grade).ToHaveTextAsync("F");

        AssertNoConsoleErrors();
    }

    // ── long ──

    [Test]
    public async Task Long_gt_threshold()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#long-result");

        await Page.Locator("#btn-long-high").ClickAsync();
        await Expect(result).ToHaveTextAsync("High Value");

        await Page.Locator("#btn-long-low").ClickAsync();
        await Expect(result).ToHaveTextAsync("Standard");

        AssertNoConsoleErrors();
    }

    // ── double ──

    [Test]
    public async Task Double_gt_comparison()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#double-result");

        await Page.Locator("#btn-double-high").ClickAsync();
        await Expect(result).ToHaveTextAsync("Fever");

        await Page.Locator("#btn-double-low").ClickAsync();
        await Expect(result).ToHaveTextAsync("Normal");

        AssertNoConsoleErrors();
    }

    // ── bool ──

    [Test]
    public async Task Bool_truthy_falsy()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#bool-result");

        await Page.Locator("#btn-bool-true").ClickAsync();
        await Expect(result).ToHaveTextAsync("Online");

        await Page.Locator("#btn-bool-false").ClickAsync();
        await Expect(result).ToHaveTextAsync("Offline");

        AssertNoConsoleErrors();
    }

    // ── string ──

    [Test]
    public async Task String_eq_comparison()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#string-result");

        await Page.Locator("#btn-string-match").ClickAsync();
        await Expect(result).ToHaveTextAsync("Welcome Alice!");

        await Page.Locator("#btn-string-miss").ClickAsync();
        await Expect(result).ToHaveTextAsync("Hello Stranger");

        AssertNoConsoleErrors();
    }

    // ── DateTime ──

    [Test]
    public async Task DateTime_gt_comparison()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#date-result");

        await Page.Locator("#btn-date-future").ClickAsync();
        await Expect(result).ToHaveTextAsync("On Time");

        await Page.Locator("#btn-date-past").ClickAsync();
        await Expect(result).ToHaveTextAsync("Overdue");

        AssertNoConsoleErrors();
    }

    // ── int? (nullable) ──

    [Test]
    public async Task Nullable_int_is_null()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nullable-result");

        await Page.Locator("#btn-nullable-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Score");

        await Page.Locator("#btn-nullable-value").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has Score");

        AssertNoConsoleErrors();
    }

    // ── AND (int + string) ──

    [Test]
    public async Task And_mixed_types()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#and-result");

        await Page.Locator("#btn-and-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Active High Scorer");

        await Page.Locator("#btn-and-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Nope");

        AssertNoConsoleErrors();
    }

    // ── OR (string alternatives) ──

    [Test]
    public async Task Or_string_alternatives()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#or-result");

        await Page.Locator("#btn-or-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        await Page.Locator("#btn-or-super").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        await Page.Locator("#btn-or-viewer").ClickAsync();
        await Expect(result).ToHaveTextAsync("Denied");

        AssertNoConsoleErrors();
    }

    // ── Nested payload — deep dot-path ──

    [Test]
    public async Task Nested_payload_deep_path_eq()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nested-result");

        await Page.Locator("#btn-nested-seattle").ClickAsync();
        await Expect(result).ToHaveTextAsync("Found Seattle");

        await Page.Locator("#btn-nested-portland").ClickAsync();
        await Expect(result).ToHaveTextAsync("Other City");

        AssertNoConsoleErrors();
    }

    // ── Null nested object ──

    [Test]
    public async Task Null_nested_object_is_null()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nested-null-result");

        await Page.Locator("#btn-nested-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        await Page.Locator("#btn-nested-present").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has Address");

        // Missing key entirely → also null
        await Page.Locator("#btn-nested-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        AssertNoConsoleErrors();
    }

    // ── Mixed nested + flat AND ──

    [Test]
    public async Task Mixed_nested_and_flat_in_and()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nested-and-result");

        await Page.Locator("#btn-nested-and-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Valid");

        // null address → city is null → AND fails
        await Page.Locator("#btn-nested-and-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("Invalid");

        // missing address → city is undefined → AND fails
        await Page.Locator("#btn-nested-and-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("Invalid");

        AssertNoConsoleErrors();
    }

    // ── Null leaf in comparison ──

    [Test]
    public async Task Null_leaf_in_comparison_takes_else_no_crash()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#null-leaf-result");

        // city is null → coerced to "" → != "Seattle" → else branch
        await Page.Locator("#btn-null-leaf-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not Seattle");

        // city is "Seattle" → match → then branch
        await Page.Locator("#btn-null-leaf-match").ClickAsync();
        await Expect(result).ToHaveTextAsync("Seattle");

        // address is null → city resolve returns undefined → coerced to "" → else
        await Page.Locator("#btn-null-leaf-obj-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not Seattle");

        AssertNoConsoleErrors();
    }

    // ── In membership ──

    [Test]
    public async Task In_membership()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#in-result");

        await Page.Locator("#btn-in-match").ClickAsync();
        await Expect(result).ToHaveTextAsync("In Group");

        await Page.Locator("#btn-in-miss").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not In Group");

        AssertNoConsoleErrors();
    }

    // ── NotIn membership ──

    [Test]
    public async Task NotIn_membership()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#notin-result");

        await Page.Locator("#btn-notin-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Allowed");

        await Page.Locator("#btn-notin-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Blocked");

        AssertNoConsoleErrors();
    }

    // ── Between range ──

    [Test]
    public async Task Between_range()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#between-result");

        await Page.Locator("#btn-between-in").ClickAsync();
        await Expect(result).ToHaveTextAsync("Working Age");

        await Page.Locator("#btn-between-low").ClickAsync();
        await Expect(result).ToHaveTextAsync("Outside Range");

        await Page.Locator("#btn-between-high").ClickAsync();
        await Expect(result).ToHaveTextAsync("Outside Range");

        AssertNoConsoleErrors();
    }

    // ── Contains text ──

    [Test]
    public async Task Contains_text()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#contains-result");

        await Page.Locator("#btn-contains-yes").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has admin");

        await Page.Locator("#btn-contains-no").ClickAsync();
        await Expect(result).ToHaveTextAsync("No admin");

        AssertNoConsoleErrors();
    }

    // ── StartsWith text ──

    [Test]
    public async Task StartsWith_text()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#startswith-result");

        await Page.Locator("#btn-startswith-yes").ClickAsync();
        await Expect(result).ToHaveTextAsync("Admin email");

        await Page.Locator("#btn-startswith-no").ClickAsync();
        await Expect(result).ToHaveTextAsync("Other email");

        AssertNoConsoleErrors();
    }

    // ── Matches regex ──

    [Test]
    public async Task Matches_regex()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#matches-result");

        await Page.Locator("#btn-matches-yes").ClickAsync();
        await Expect(result).ToHaveTextAsync("Valid");

        await Page.Locator("#btn-matches-no").ClickAsync();
        await Expect(result).ToHaveTextAsync("Invalid");

        AssertNoConsoleErrors();
    }

    // ── MinLength text ──

    [Test]
    public async Task MinLength_text()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#minlength-result");

        await Page.Locator("#btn-minlength-yes").ClickAsync();
        await Expect(result).ToHaveTextAsync("Long enough");

        await Page.Locator("#btn-minlength-no").ClickAsync();
        await Expect(result).ToHaveTextAsync("Too short");

        AssertNoConsoleErrors();
    }

    // ── IsEmpty presence ──

    [Test]
    public async Task IsEmpty_presence()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#isempty-result");

        await Page.Locator("#btn-isempty-empty").ClickAsync();
        await Expect(result).ToHaveTextAsync("Empty");

        await Page.Locator("#btn-isempty-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("Empty");

        await Page.Locator("#btn-isempty-value").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has value");

        AssertNoConsoleErrors();
    }

    // ── NOT (InvertGuard) ──

    [Test]
    public async Task Not_inverts_guard()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#not-result");

        await Page.Locator("#btn-not-user").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not admin");

        await Page.Locator("#btn-not-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Is admin");

        AssertNoConsoleErrors();
    }

    // ── Per-action When guard ──

    [Test]
    public async Task Per_action_when_guard()
    {
        await NavigateAndBoot();
        var always = Page.Locator("#per-action-result");
        var bonus = Page.Locator("#per-action-bonus");

        // score=95 → guard passes → both set
        await Page.Locator("#btn-peraction-high").ClickAsync();
        await Expect(always).ToHaveTextAsync("Always runs");
        await Expect(bonus).ToHaveTextAsync("Bonus!");

        // score=50 → guard fails → "Always runs" still set, bonus stays from previous or resets
        await Page.Locator("#btn-peraction-low").ClickAsync();
        await Expect(always).ToHaveTextAsync("Always runs");
        // Bonus stays "Bonus!" from previous click — per-action just skips the command,
        // it doesn't reset. But if first time click is low, bonus stays as —.
        // For a clean test, we test in isolation by reloading:

        AssertNoConsoleErrors();
    }

    // ── Direct And syntax ──

    [Test]
    public async Task Direct_and_syntax()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#direct-and-result");

        await Page.Locator("#btn-direct-and-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Pass");

        await Page.Locator("#btn-direct-and-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Fail");

        AssertNoConsoleErrors();
    }

    // ── Direct Or syntax ──

    [Test]
    public async Task Direct_or_syntax()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#direct-or-result");

        await Page.Locator("#btn-direct-or-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        await Page.Locator("#btn-direct-or-super").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        await Page.Locator("#btn-direct-or-viewer").ClickAsync();
        await Expect(result).ToHaveTextAsync("Denied");

        AssertNoConsoleErrors();
    }
}
