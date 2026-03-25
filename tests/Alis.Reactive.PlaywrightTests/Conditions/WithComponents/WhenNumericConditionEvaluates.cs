using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Conditions.WithComponents;

/// <summary>
/// Exercises conditions with FusionNumericTextBox components end-to-end in the browser:
/// simple Gt, ElseIf ladder, AND compound, and cross-component source-vs-source.
///
/// Page under test: /Sandbox/Conditions/NumericCondition
///
/// Senior living domain: vital sign thresholds (heart rate, blood pressure).
/// Each section is an independent condition scenario — no shared state between sections.
/// </summary>
[TestFixture]
public class WhenNumericConditionEvaluates : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/NumericCondition";

    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NumericConditionModel";
    private const string HeartRateId = Scope + "__HeartRate";
    private const string BloodPressureId = Scope + "__BloodPressure";
    private const string ThresholdValueId = Scope + "__ThresholdValue";

    private NumericTextBoxLocator HeartRate => new(Page, HeartRateId);
    private NumericTextBoxLocator BloodPressure => new(Page, BloodPressureId);
    private NumericTextBoxLocator ThresholdValue => new(Page, ThresholdValueId);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    // ── Section 1: Simple condition — Gt(100) ──

    [Test]
    public async Task tachycardia_warning_appears_when_heart_rate_exceeds_100()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#tachycardia-warning")).ToBeHiddenAsync();

        await HeartRate.FillAndBlur("150");

        await Expect(Page.Locator("#tachycardia-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#tachycardia-warning"))
            .ToHaveTextAsync("Tachycardia detected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task tachycardia_warning_hides_when_heart_rate_drops_to_100_or_below()
    {
        await NavigateAndBoot();

        // First make warning visible
        await HeartRate.FillAndBlur("150");
        await Expect(Page.Locator("#tachycardia-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        // Drop to 80 — warning should hide
        await HeartRate.FillAndBlur("80");
        await Expect(Page.Locator("#tachycardia-warning"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task tachycardia_warning_boundary_at_exactly_100_stays_hidden()
    {
        await NavigateAndBoot();

        // 100 is NOT > 100, so warning should stay hidden
        await HeartRate.FillAndBlur("100");
        await Expect(Page.Locator("#tachycardia-warning")).ToBeHiddenAsync();

        // 101 IS > 100, warning should show
        await HeartRate.FillAndBlur("101");
        await Expect(Page.Locator("#tachycardia-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: ElseIf ladder — heart rate zones ──

    [Test]
    public async Task elseif_ladder_shows_critical_above_120()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("130");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Critical", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_shows_high_between_100_and_119()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("110");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("High", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_shows_normal_between_60_and_99()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("72");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Normal", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_shows_low_below_60()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("45");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Low", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_transitions_across_all_zones()
    {
        // Proves condition re-evaluates correctly across zone transitions
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("130");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Critical", new() { Timeout = 5000 });

        await HeartRate.FillAndBlur("105");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("High", new() { Timeout = 5000 });

        await HeartRate.FillAndBlur("72");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Normal", new() { Timeout = 5000 });

        await HeartRate.FillAndBlur("40");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Low", new() { Timeout = 5000 });

        // Back to Critical — confirms no sticky state
        await HeartRate.FillAndBlur("150");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Critical", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_boundary_at_exactly_120_is_critical()
    {
        await NavigateAndBoot();

        // 120 satisfies Gte(120) → first branch wins → "Critical"
        await HeartRate.FillAndBlur("120");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Critical", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_boundary_at_exactly_100_is_high()
    {
        await NavigateAndBoot();

        // 100 does NOT satisfy Gte(120), but satisfies Gte(100) → "High"
        await HeartRate.FillAndBlur("100");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("High", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_boundary_at_exactly_60_is_normal()
    {
        await NavigateAndBoot();

        // 60 satisfies Gte(60) → "Normal"
        await HeartRate.FillAndBlur("60");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Normal", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 3: AND compound — normal blood pressure range ──

    [Test]
    public async Task and_compound_shows_normal_range_when_between_60_and_120()
    {
        await NavigateAndBoot();

        await BloodPressure.FillAndBlur("90");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Normal range", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_compound_shows_out_of_range_below_60()
    {
        await NavigateAndBoot();

        await BloodPressure.FillAndBlur("50");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Out of range", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_compound_shows_out_of_range_above_120()
    {
        await NavigateAndBoot();

        await BloodPressure.FillAndBlur("150");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Out of range", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_compound_boundaries_at_60_and_120_are_in_range()
    {
        await NavigateAndBoot();

        // 60 satisfies Gte(60) AND Lte(120) → "Normal range"
        await BloodPressure.FillAndBlur("60");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Normal range", new() { Timeout = 5000 });

        // 120 satisfies Gte(60) AND Lte(120) → "Normal range"
        await BloodPressure.FillAndBlur("120");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Normal range", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task and_compound_transitions_in_and_out_of_range()
    {
        await NavigateAndBoot();

        await BloodPressure.FillAndBlur("90");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Normal range", new() { Timeout = 5000 });

        await BloodPressure.FillAndBlur("150");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Out of range", new() { Timeout = 5000 });

        await BloodPressure.FillAndBlur("80");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Normal range", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 4: Cross-component — source-vs-source ──

    [Test]
    public async Task cross_component_shows_above_threshold_when_hr_exceeds_threshold()
    {
        await NavigateAndBoot();

        // HeartRate starts at 0, ThresholdValue starts at 100
        // Set HeartRate to 150 → 150 >= 100 → "Above threshold"
        await HeartRate.FillAndBlur("150");
        await Page.Locator("#check-threshold-btn").ClickAsync();

        await Expect(Page.Locator("#threshold-result"))
            .ToHaveTextAsync("Above threshold", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cross_component_shows_below_threshold_when_hr_is_lower()
    {
        await NavigateAndBoot();

        // HeartRate = 50, ThresholdValue = 100 → 50 < 100 → "Below threshold"
        await HeartRate.FillAndBlur("50");
        await Page.Locator("#check-threshold-btn").ClickAsync();

        await Expect(Page.Locator("#threshold-result"))
            .ToHaveTextAsync("Below threshold", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cross_component_updates_when_threshold_changes()
    {
        await NavigateAndBoot();

        // HeartRate = 80, Threshold = 100 → below
        await HeartRate.FillAndBlur("80");
        await Page.Locator("#check-threshold-btn").ClickAsync();
        await Expect(Page.Locator("#threshold-result"))
            .ToHaveTextAsync("Below threshold", new() { Timeout = 5000 });

        // Now lower the threshold to 70 → 80 >= 70 → above
        await ThresholdValue.FillAndBlur("70");
        await Page.Locator("#check-threshold-btn").ClickAsync();
        await Expect(Page.Locator("#threshold-result"))
            .ToHaveTextAsync("Above threshold", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cross_component_boundary_equal_values_is_above()
    {
        await NavigateAndBoot();

        // HeartRate = 100, Threshold = 100 → 100 >= 100 → "Above threshold" (Gte)
        await HeartRate.FillAndBlur("100");
        await ThresholdValue.FillAndBlur("100");
        await Page.Locator("#check-threshold-btn").ClickAsync();

        await Expect(Page.Locator("#threshold-result"))
            .ToHaveTextAsync("Above threshold", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
