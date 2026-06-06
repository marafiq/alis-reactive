namespace Alis.Reactive.PlaywrightTests.Conditions.Guards;

[TestFixture]
public class WhenGuardsControlExecution : PlaywrightTestBase
{
    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Conditions/Guards");
        await WaitForTraceMessage("booted", 5000);
    }

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

    [Test]
    public async Task null_nested_object_is_null()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nested-null-result");

        await Page.Locator("#btn-nested-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        await Page.Locator("#btn-nested-present").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has Address");

        // Missing payload keys are treated as null by IsNull().
        await Page.Locator("#btn-nested-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task mixed_nested_and_flat_in_and()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nested-and-result");

        await Page.Locator("#btn-nested-and-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Valid");

        // Null and missing nested objects both make the nested NotNull() guard fail.
        await Page.Locator("#btn-nested-and-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("Invalid");

        await Page.Locator("#btn-nested-and-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("Invalid");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task null_leaf_in_comparison_takes_else_no_crash()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#null-leaf-result");

        // Null leaf values are coerced to an empty string for string comparison.
        await Page.Locator("#btn-null-leaf-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not Seattle");

        await Page.Locator("#btn-null-leaf-match").ClickAsync();
        await Expect(result).ToHaveTextAsync("Seattle");

        // Null parent objects also fall through without throwing.
        await Page.Locator("#btn-null-leaf-obj-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not Seattle");

        AssertNoConsoleErrors();
    }

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

    [Test]
    public async Task unguarded_reaction_still_runs_when_guarded_branch_matches()
    {
        await NavigateAndBoot();
        var always = Page.Locator("#single-command-condition-result");
        var bonus = Page.Locator("#single-command-condition-bonus");

        await Page.Locator("#btn-single-command-condition-high").ClickAsync();
        await Expect(always).ToHaveTextAsync("Always runs");
        await Expect(bonus).ToHaveTextAsync("Bonus!");

        // A failed guard skips only the guarded reaction; unguarded reactions still run.
        await Page.Locator("#btn-single-command-condition-low").ClickAsync();
        await Expect(always).ToHaveTextAsync("Always runs");
        // Bonus keeps the previous successful branch value; the fresh-page test covers the default state.

        AssertNoConsoleErrors();
    }

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

    [Test]
    public async Task confirm_ok_path()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#confirm-result");

        await Page.Locator("#btn-confirm").ClickAsync();

        // Confirm is an async user-decision boundary; the app-level dialog is the proof surface.
        var dialog = Page.Locator("#alisConfirmDialog");
        await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 5000 });

        var confirmOkButton = dialog.Locator("button.e-primary");
        await confirmOkButton.ClickAsync();

        await Expect(result).ToHaveTextAsync("Confirmed");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirm_cancel_path()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#confirm-result");

        await Page.Locator("#btn-confirm").ClickAsync();

        // Confirm is an async user-decision boundary; the app-level dialog is the proof surface.
        var dialog = Page.Locator("#alisConfirmDialog");
        await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 5000 });

        var confirmCancelButton = dialog.Locator("button:not(.e-primary)").Last;
        await confirmCancelButton.ClickAsync();

        await Expect(result).ToHaveTextAsync("Cancelled");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_chain_only_executes_first_matching_branch()
    {
        await NavigateAndBoot();
        var grade = Page.Locator("#grade");

        await Page.Locator("#btn-score-95").ClickAsync();
        await Expect(grade).ToHaveTextAsync("A");

        await Page.Locator("#btn-score-85").ClickAsync();
        await Expect(grade).ToHaveTextAsync("B");

        await Page.Locator("#btn-score-40").ClickAsync();
        await Expect(grade).ToHaveTextAsync("F");

        await Page.Locator("#btn-score-95").ClickAsync();
        await Expect(grade).ToHaveTextAsync("A");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task null_nested_object_does_not_crash_and_evaluates_to_null()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nested-null-result");

        // Explicit null must stay null-safe before and after a present address dispatch.
        await Page.Locator("#btn-nested-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        await Page.Locator("#btn-nested-present").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has Address");

        await Page.Locator("#btn-nested-null").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task missing_key_in_payload_evaluates_as_null()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nested-null-result");

        // Missing structured payload paths resolve as undefined; IsNull treats that as null across repeated dispatches.
        await Page.Locator("#btn-nested-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        await Page.Locator("#btn-nested-present").ClickAsync();
        await Expect(result).ToHaveTextAsync("Has Address");

        await Page.Locator("#btn-nested-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("No Address");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_composition_short_circuits_on_first_false()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#and-result");

        await Page.Locator("#btn-and-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Nope");

        await Page.Locator("#btn-and-pass").ClickAsync();
        await Expect(result).ToHaveTextAsync("Active High Scorer");

        await Page.Locator("#btn-and-fail").ClickAsync();
        await Expect(result).ToHaveTextAsync("Nope");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task or_composition_succeeds_on_second_match()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#or-result");

        await Page.Locator("#btn-or-super").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        await Page.Locator("#btn-or-viewer").ClickAsync();
        await Expect(result).ToHaveTextAsync("Denied");

        await Page.Locator("#btn-or-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task not_inverts_truthy_to_falsy_and_vice_versa()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#not-result");

        await Page.Locator("#btn-not-user").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not admin");

        await Page.Locator("#btn-not-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Is admin");

        await Page.Locator("#btn-not-user").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not admin");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unguarded_reaction_still_runs_when_guarded_branch_fails_on_fresh_page()
    {
        await NavigateAndBoot();
        var always = Page.Locator("#single-command-condition-result");
        var bonus = Page.Locator("#single-command-condition-bonus");

        // Fresh navigation proves a failed guard does not inherit a prior bonus value.
        await Page.Locator("#btn-single-command-condition-low").ClickAsync();
        await Expect(always).ToHaveTextAsync("Always runs");
        await Expect(bonus).ToHaveTextAsync("\u2014");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task direct_and_fails_when_first_condition_fails()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#direct-and-result");

        await Page.Locator("#btn-direct-and-score-low").ClickAsync();
        await Expect(result).ToHaveTextAsync("Fail");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task direct_or_succeeds_when_first_condition_matches()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#direct-or-result");

        await Page.Locator("#btn-direct-or-admin").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        await Page.Locator("#btn-direct-or-viewer").ClickAsync();
        await Expect(result).ToHaveTextAsync("Denied");

        await Page.Locator("#btn-direct-or-super").ClickAsync();
        await Expect(result).ToHaveTextAsync("Authorized");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_branch_inside_then_executes_inner_branch_cases()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#nested-branch-result");

        await Page.Locator("#btn-nested-branch-active").ClickAsync();
        await Expect(result).ToHaveTextAsync("Senior Active");

        await Page.Locator("#btn-nested-branch-inactive").ClickAsync();
        await Expect(result).ToHaveTextAsync("Senior Inactive");

        await Page.Locator("#btn-nested-branch-low").ClickAsync();
        await Expect(result).ToHaveTextAsync("Junior");

        await Page.Locator("#btn-nested-branch-active").ClickAsync();
        await Expect(result).ToHaveTextAsync("Senior Active");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task null_leaf_with_missing_address_key_takes_else_no_crash()
    {
        await NavigateAndBoot();
        var result = Page.Locator("#null-leaf-result");

        // Missing address resolves as undefined, falls through the string comparison, and does not poison the next dispatch.
        await Page.Locator("#btn-null-leaf-missing").ClickAsync();
        await Expect(result).ToHaveTextAsync("Not Seattle");

        await Page.Locator("#btn-null-leaf-match").ClickAsync();
        await Expect(result).ToHaveTextAsync("Seattle");

        AssertNoConsoleErrors();
    }
}
