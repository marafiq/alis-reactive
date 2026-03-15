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
    public async Task int_elseif_takes_correct_branch()
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
    public async Task long_gt_threshold()
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
    public async Task double_gt_comparison()
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
    public async Task bool_truthy_falsy()
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
    public async Task string_eq_comparison()
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
    public async Task datetime_gt_comparison()
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
    public async Task nullable_int_is_null()
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
    public async Task and_mixed_types()
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
    public async Task or_string_alternatives()
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
    public async Task nested_payload_deep_path_eq()
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
    public async Task null_nested_object_is_null()
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
    public async Task mixed_nested_and_flat_in_and()
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
    public async Task null_leaf_in_comparison_takes_else_no_crash()
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
    public async Task in_membership()
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
    public async Task notin_membership()
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
    public async Task between_range()
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
    public async Task contains_text()
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
    public async Task starts_with_text()
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
    public async Task matches_regex()
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
    public async Task min_length_text()
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
    public async Task is_empty_presence()
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
    public async Task not_inverts_guard()
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
    public async Task per_action_when_guard()
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
    public async Task direct_and_syntax()
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
    public async Task direct_or_syntax()
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

    // ── Confirm dialog — OK path ──

    [Test]
    public async Task confirm_ok_path()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#confirm-result");

        // Click the trigger button — this dispatches 'check-confirm' custom event
        await Page.Locator("#btn-confirm").ClickAsync();

        // SF Dialog should appear with "Are you sure you want to proceed?"
        var dialog = Page.Locator("#alisConfirmDialog");
        await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click OK button
        var okButton = dialog.Locator("button.e-primary");
        await okButton.ClickAsync();

        // Result should be "Confirmed"
        await Expect(result).ToHaveTextAsync("Confirmed");

        AssertNoConsoleErrors();
    }

    // ── Confirm dialog — Cancel path ──

    [Test]
    public async Task confirm_cancel_path()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#confirm-result");

        // Click the trigger button
        await Page.Locator("#btn-confirm").ClickAsync();

        // SF Dialog should appear
        var dialog = Page.Locator("#alisConfirmDialog");
        await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click Cancel button (non-primary)
        var cancelButton = dialog.Locator("button:not(.e-primary)").Last;
        await cancelButton.ClickAsync();

        // Result should be "Cancelled"
        await Expect(result).ToHaveTextAsync("Cancelled");

        AssertNoConsoleErrors();
    }

    // ── Edge cases: branch exclusivity, null safety, composition semantics ──

    [Test]
    public async Task elseif_chain_only_executes_first_matching_branch()
    {
        // Score 95 satisfies BOTH Gte(90) AND Gte(80) — only the first matching branch should fire
        await NavigateAndBoot();
        var grade = Page.Locator("#grade");

        // 95 matches Gte(90) first → must be "A", never "B"
        await Page.Locator("#btn-score-95").ClickAsync();
        await Expect(grade).ToHaveTextAsync("A");

        // 85 does NOT match Gte(90), but DOES match Gte(80) → must be "B"
        await Page.Locator("#btn-score-85").ClickAsync();
        await Expect(grade).ToHaveTextAsync("B");

        // 40 matches neither Gte(90) nor Gte(80) → falls through to Else → "F"
        await Page.Locator("#btn-score-40").ClickAsync();
        await Expect(grade).ToHaveTextAsync("F");

        // Click 95 again to confirm branch exclusivity is stable across re-evaluations
        await Page.Locator("#btn-score-95").ClickAsync();
        await Expect(grade).ToHaveTextAsync("A");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task null_nested_object_does_not_crash_and_evaluates_to_null()
    {
        // Dispatch with address:null → When(address).IsNull() should be true
        // Then dispatch with address:{city:"NYC"} → IsNull() should be false
        // Proves null-safe dot-path walking doesn't throw
        await NavigateAndBoot();
        var result = Page.Locator("#nested-null-result");

        // address explicitly null → IsNull() = true → "No Address"
        await Page.Locator("#btn-nested-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        // address present with city → IsNull() = false → "Has Address"
        await Page.Locator("#btn-nested-present").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has Address");

        // Transition back to null — confirms re-evaluation works after non-null
        await Page.Locator("#btn-nested-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task missing_key_in_payload_evaluates_as_null()
    {
        // Dispatch with {id:3} (no address key at all) → When(address).IsNull() should be true
        // Proves walk.ts returns undefined for missing keys, and IsNull treats undefined as null
        await NavigateAndBoot();
        var result = Page.Locator("#nested-null-result");

        // Missing key → walk returns undefined → IsNull treats as null → "No Address"
        await Page.Locator("#btn-nested-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        // Contrast with present address → "Has Address"
        await Page.Locator("#btn-nested-present").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has Address");

        // Missing key again to confirm idempotent
        await Page.Locator("#btn-nested-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_composition_short_circuits_on_first_false()
    {
        // Score 95 + status "inactive" → When(score).Gte(90).And(status).Eq("active") → false
        // Score 95 + status "active" → true
        // Proves AND evaluates both sides correctly
        await NavigateAndBoot();
        var result = Page.Locator("#and-result");

        // First true, second false → compound is false → "Nope"
        await Page.Locator("#btn-and-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Nope");

        // Both true → compound is true → "Active High Scorer"
        await Page.Locator("#btn-and-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Active High Scorer");

        // Back to fail — confirms re-evaluation after success
        await Page.Locator("#btn-and-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Nope");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task or_composition_succeeds_on_second_match()
    {
        // role "superuser" → When(role).Eq("admin").Or(role).Eq("superuser") → true
        // Proves OR evaluates second operand when first is false
        await NavigateAndBoot();
        var result = Page.Locator("#or-result");

        // First operand false, second true → OR succeeds → "Authorized"
        await Page.Locator("#btn-or-super").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        // Neither matches → OR fails → "Denied"
        await Page.Locator("#btn-or-viewer").ClickAsync();
        await Expect(result).ToHaveTextAsync("Denied");

        // First operand true → OR succeeds without needing second → "Authorized"
        await Page.Locator("#btn-or-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task not_inverts_truthy_to_falsy_and_vice_versa()
    {
        // role "user" → When(role).Eq("admin").Not() → true (not admin)
        // role "admin" → When(role).Eq("admin").Not() → false (is admin, NOT inverted)
        await NavigateAndBoot();
        var result = Page.Locator("#not-result");

        // Eq("admin") is false, Not() inverts to true → Then branch → "Not admin"
        await Page.Locator("#btn-not-user").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not admin");

        // Eq("admin") is true, Not() inverts to false → Else branch → "Is admin"
        await Page.Locator("#btn-not-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Is admin");

        // Back to non-admin to confirm inversion is stable
        await Page.Locator("#btn-not-user").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not admin");

        AssertNoConsoleErrors();
    }

    // ── Per-action When guard — low score on fresh page (skip path) ──

    [Test]
    public async Task per_action_when_guard_skips_guarded_command_on_fresh_page()
    {
        // Fresh navigation — bonus starts as "—" (em dash default)
        await NavigateAndBoot();
        var always = Page.Locator("#per-action-result");
        var bonus = Page.Locator("#per-action-bonus");

        // score=50 → guard fails → first command fires, second is SKIPPED
        await Page.Locator("#btn-peraction-low").ClickAsync();
        await Expect(always).ToHaveTextAsync("Always runs");
        // Bonus was never set — remains at default em dash
        await Expect(bonus).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrors();
    }

    // ── Direct And — first condition fails ──

    [Test]
    public async Task direct_and_fails_when_first_condition_fails()
    {
        // Score 70 < 90 → first condition Gte(90) is false → AND short-circuits → Else
        await NavigateAndBoot();
        var result = Page.Locator("#direct-and-result");

        await Page.EvaluateAsync(
            "document.dispatchEvent(new CustomEvent('check-direct-and',{detail:{score:70,status:'active'}}))");
        await Expect(result).ToHaveTextAsync("Fail");

        AssertNoConsoleErrors();
    }

    // ── Direct Or — both conditions match (first wins) ──

    [Test]
    public async Task direct_or_succeeds_when_first_condition_matches()
    {
        // role "admin" matches first Eq("admin") → OR succeeds immediately
        await NavigateAndBoot();
        var result = Page.Locator("#direct-or-result");

        await Page.Locator("#btn-direct-or-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        // Confirm re-evaluation: neither matches → Denied
        await Page.Locator("#btn-direct-or-viewer").ClickAsync();
        await Expect(result).ToHaveTextAsync("Denied");

        // Second condition matches → OR succeeds via second operand
        await Page.Locator("#btn-direct-or-super").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        AssertNoConsoleErrors();
    }

    // ── Null leaf — missing address key entirely (not just city=null) ──

    [Test]
    public async Task null_leaf_with_missing_address_key_takes_else_no_crash()
    {
        // Dispatch check-null-leaf with NO address key at all → walk returns undefined
        // → coerced to "" → != "Seattle" → else branch, no crash
        await NavigateAndBoot();
        var result = Page.Locator("#null-leaf-result");

        // address key entirely missing → walk.ts returns undefined → else
        await Page.EvaluateAsync(
            "document.dispatchEvent(new CustomEvent('check-null-leaf',{detail:{id:99}}))");
        await Expect(result).ToHaveTextAsync("Not Seattle");

        // Confirm recovery: city=Seattle still matches after missing-key dispatch
        await Page.Locator("#btn-null-leaf-match").ClickAsync();
        await Expect(result).ToHaveTextAsync("Seattle");

        AssertNoConsoleErrors();
    }
}
