using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Conditions.ConditionCoverage;

/// <summary>
/// Vertical-slice coverage for component-read conditions across every Shape kind.
/// Page under test: /Sandbox/Conditions/ConditionCoverage
///
/// Each test navigates fresh, sets the target component to a known state (either
/// via the initial DomReady value or by user interaction), clicks ONE button that
/// fires ONE condition operator reading the component's value via Value&lt;T&gt;(),
/// and verifies the result element. No shared state between tests.
/// </summary>
[TestFixture]
public class WhenComponentReadConditionsEvaluate : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/ConditionCoverage";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ConditionCoverageModel";

    private const string CareLevelId     = Scope + "__CareLevel";
    private const string ResidentNameId  = Scope + "__ResidentName";
    private const string NotesId         = Scope + "__Notes";
    private const string HeartRateId     = Scope + "__HeartRate";
    private const string DosageId        = Scope + "__Dosage";
    private const string AdmissionDateId = Scope + "__AdmissionDate";
    private const string DischargeDateId = Scope + "__DischargeDate";
    private const string AllergiesId     = Scope + "__Allergies";

    private DropDownListLocator  CareLevel     => new(Page, CareLevelId);
    private NativeTextBoxLocator ResidentName  => new(Page, ResidentNameId);
    // Textarea and input share the same user-gesture surface (click, fill, blur),
    // so the TextBox locator suffices for the Notes field.
    private NativeTextBoxLocator Notes         => new(Page, NotesId);
    private NumericTextBoxLocator HeartRate    => new(Page, HeartRateId);
    private NumericTextBoxLocator Dosage       => new(Page, DosageId);
    private SwitchLocator        AcceptedTerms => new(Page, Scope + "__AcceptedTerms");
    private DatePickerLocator    AdmissionDate => new(Page, AdmissionDateId);
    private DatePickerLocator    DischargeDate => new(Page, DischargeDateId);
    private MultiSelectLocator   Allergies     => new(Page, AllergiesId);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private async Task ClickAsync(string id) => await Page.Locator("#" + id).ClickAsync();
    private async Task AssertText(string id, string expected) =>
        await Expect(Page.Locator("#" + id)).ToHaveTextAsync(expected, new() { Timeout = 3000 });

    // ── String: Eq / NotEq on FusionDropDownList ──

    [Test]
    public async Task string_eq_matches_when_dropdown_value_equals_literal()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Memory Care");
        await ClickAsync("btn-str-eq");
        await AssertText("r-str-eq", "match");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_eq_reports_no_match_when_dropdown_value_differs()
    {
        await NavigateAndBoot();
        // DomReady seeds "Standard" — not "Memory Care"
        await ClickAsync("btn-str-eq");
        await AssertText("r-str-eq", "no match");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_notEq_fires_then_branch_when_value_differs_from_literal()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Assisted");
        await ClickAsync("btn-str-neq");
        await AssertText("r-str-neq", "different");
        AssertNoConsoleErrors();
    }

    // ── String: Contains / StartsWith / EndsWith / Matches ──

    [Test]
    public async Task string_contains_matches_when_substring_present()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-str-contains");
        await AssertText("r-str-contains", "yes");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_contains_reports_no_when_substring_absent()
    {
        await NavigateAndBoot();
        await ResidentName.FillAndBlur("Emily Johnson");
        await ClickAsync("btn-str-contains");
        await AssertText("r-str-contains", "no");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_startsWith_matches_on_initial_prefix()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-str-starts");
        await AssertText("r-str-starts", "yes");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_endsWith_matches_on_suffix()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-str-ends");
        await AssertText("r-str-ends", "yes");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_matches_regex_succeeds_when_pattern_matches()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-str-matches");
        await AssertText("r-str-matches", "yes");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_matches_regex_fails_when_first_letter_is_lowercase()
    {
        await NavigateAndBoot();
        await ResidentName.FillAndBlur("jane doe");
        await ClickAsync("btn-str-matches");
        await AssertText("r-str-matches", "no");
        AssertNoConsoleErrors();
    }

    // ── String: MinLength / IsEmpty / NotEmpty ──

    [Test]
    public async Task string_minLength_passes_when_text_meets_threshold()
    {
        await NavigateAndBoot();
        // DomReady seeds "Initial notes" (13 chars ≥ 10)
        await ClickAsync("btn-str-minlen");
        await AssertText("r-str-minlen", "long enough");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_minLength_fails_when_text_is_below_threshold()
    {
        await NavigateAndBoot();
        await Notes.FillAndBlur("short");
        await ClickAsync("btn-str-minlen");
        await AssertText("r-str-minlen", "too short");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_isEmpty_reports_empty_when_textarea_cleared()
    {
        await NavigateAndBoot();
        await Notes.FillAndBlur("");
        await ClickAsync("btn-str-isempty");
        await AssertText("r-str-isempty", "empty");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_notEmpty_reports_filled_when_textarea_has_content()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-str-notempty");
        await AssertText("r-str-notempty", "filled");
        AssertNoConsoleErrors();
    }

    // ── Number: Gt / Gte / Lt / Lte / Between / In ──

    [Test]
    public async Task number_gt_fires_when_value_exceeds_threshold()
    {
        await NavigateAndBoot();
        // DomReady seeds 72, threshold is 60
        await ClickAsync("btn-num-gt");
        await AssertText("r-num-gt", "above");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task number_gt_fails_at_threshold_boundary_exclusive()
    {
        await NavigateAndBoot();
        await HeartRate.FillAndBlur("60");
        await ClickAsync("btn-num-gt");
        await AssertText("r-num-gt", "not above");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task number_gte_passes_at_boundary_inclusive()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-num-gte");
        await AssertText("r-num-gte", "at/above");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task number_lt_reports_under_when_value_is_below_threshold()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-num-lt");
        await AssertText("r-num-lt", "under");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task number_lte_passes_at_boundary_inclusive()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-num-lte");
        await AssertText("r-num-lte", "at/below");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task number_between_reports_normal_when_value_is_in_range()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-num-between");
        await AssertText("r-num-between", "normal");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task number_between_reports_abnormal_when_outside_range()
    {
        await NavigateAndBoot();
        await HeartRate.FillAndBlur("180");
        await ClickAsync("btn-num-between");
        await AssertText("r-num-between", "abnormal");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task number_in_matches_when_value_appears_in_set()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-num-in");
        await AssertText("r-num-in", "known");
        AssertNoConsoleErrors();
    }

    // ── Nullable(Number): IsNull / NotNull ──

    [Test]
    public async Task nullable_number_isNull_fires_when_field_untouched()
    {
        await NavigateAndBoot();
        // Dosage has no initial value — should be null
        await ClickAsync("btn-num-isnull");
        await AssertText("r-num-isnull", "unset");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nullable_number_notNull_fires_after_value_is_entered()
    {
        await NavigateAndBoot();
        await Dosage.FillAndBlur("42.5");
        await ClickAsync("btn-num-notnull");
        await AssertText("r-num-notnull", "set");
        AssertNoConsoleErrors();
    }

    // ── Boolean: Truthy / Falsy / Eq ──

    [Test]
    public async Task boolean_falsy_fires_when_switch_is_off_initially()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-bool-falsy");
        await AssertText("r-bool-falsy", "yes");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boolean_truthy_fires_after_switch_is_toggled_on()
    {
        await NavigateAndBoot();
        await AcceptedTerms.Toggle();
        await ClickAsync("btn-bool-truthy");
        await AssertText("r-bool-truthy", "yes");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boolean_eq_matches_when_switch_is_checked_and_compared_to_true()
    {
        await NavigateAndBoot();
        await AcceptedTerms.Toggle();
        await ClickAsync("btn-bool-eq");
        await AssertText("r-bool-eq", "accepted");
        AssertNoConsoleErrors();
    }

    // ── Date: Eq / Gt / Lt / Between / IsNull / NotNull ──
    // These are the scenarios my shapeEquals fix unblocks — Date equality
    // now uses value comparison via getTime() instead of reference equality.

    [Test]
    public async Task date_eq_matches_when_admission_date_equals_seeded_value()
    {
        await NavigateAndBoot();
        // DomReady seeds 2025-06-15, button tests eq against 2025-06-15
        await ClickAsync("btn-date-eq");
        await AssertText("r-date-eq", "match");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task date_gt_reports_modern_when_admission_exceeds_threshold()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-date-gt");
        await AssertText("r-date-gt", "modern");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task date_lt_reports_past_when_admission_is_before_threshold()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-date-lt");
        await AssertText("r-date-lt", "past/present");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task date_between_reports_in_range_when_admission_is_in_window()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-date-between");
        await AssertText("r-date-between", "in range");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task date_isNull_fires_when_admission_date_is_cleared()
    {
        await NavigateAndBoot();
        await AdmissionDate.Clear();
        await AdmissionDate.Blur();
        await ClickAsync("btn-date-isnull");
        await AssertText("r-date-isnull", "missing");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task date_notNull_fires_when_admission_date_has_a_value()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-date-notnull");
        await AssertText("r-date-notnull", "set");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task date_cross_component_reports_valid_order_when_admission_precedes_discharge()
    {
        await NavigateAndBoot();
        await DischargeDate.SelectDate(2025, 12, 31);
        await ClickAsync("btn-date-cross");
        await AssertText("r-date-cross", "valid order");
        AssertNoConsoleErrors();
    }

    // ── Array(String): IsEmpty / NotEmpty / ArrayContains ──

    [Test]
    public async Task array_isEmpty_fires_when_multiselect_has_no_selections()
    {
        await NavigateAndBoot();
        await ClickAsync("btn-arr-isempty");
        await AssertText("r-arr-isempty", "none");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task array_notEmpty_fires_after_items_are_selected()
    {
        await NavigateAndBoot();
        await Allergies.SelectItem("Penicillin");
        await ClickAsync("btn-arr-notempty");
        await AssertText("r-arr-notempty", "some");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task array_contains_finds_selected_item_in_array_value()
    {
        await NavigateAndBoot();
        await Allergies.SelectItem("Penicillin");
        await ClickAsync("btn-arr-contains");
        await AssertText("r-arr-contains", "allergic");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task array_contains_reports_safe_when_target_not_selected()
    {
        await NavigateAndBoot();
        await Allergies.SelectItem("Latex");
        await ClickAsync("btn-arr-contains");
        await AssertText("r-arr-contains", "safe");
        AssertNoConsoleErrors();
    }
}
