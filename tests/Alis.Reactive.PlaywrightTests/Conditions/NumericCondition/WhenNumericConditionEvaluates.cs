using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Conditions.NumericCondition;

/// <summary>
/// Exercises FusionNumericTextBox conditions for numeric thresholds, ElseIf ordering,
/// compound ranges, and source-vs-source comparisons.
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

        await HeartRate.FillAndBlur("150");
        await Expect(Page.Locator("#tachycardia-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        await HeartRate.FillAndBlur("80");
        await Expect(Page.Locator("#tachycardia-warning"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task tachycardia_warning_boundary_at_exactly_100_stays_hidden()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("100");
        await Expect(Page.Locator("#tachycardia-warning")).ToBeHiddenAsync();

        await HeartRate.FillAndBlur("101");
        await Expect(Page.Locator("#tachycardia-warning"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

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

        await HeartRate.FillAndBlur("150");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Critical", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_boundary_at_exactly_120_is_critical()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("120");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Critical", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_boundary_at_exactly_100_is_high()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("100");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("High", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task elseif_ladder_boundary_at_exactly_60_is_normal()
    {
        await NavigateAndBoot();

        await HeartRate.FillAndBlur("60");
        await Expect(Page.Locator("#hr-zone"))
            .ToHaveTextAsync("Normal", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

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

        await BloodPressure.FillAndBlur("60");
        await Expect(Page.Locator("#bp-range"))
            .ToHaveTextAsync("Normal range", new() { Timeout = 5000 });

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

    [Test]
    public async Task cross_component_shows_above_threshold_when_hr_exceeds_threshold()
    {
        await NavigateAndBoot();

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

        await HeartRate.FillAndBlur("80");
        await Page.Locator("#check-threshold-btn").ClickAsync();
        await Expect(Page.Locator("#threshold-result"))
            .ToHaveTextAsync("Below threshold", new() { Timeout = 5000 });

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

        await HeartRate.FillAndBlur("100");
        await ThresholdValue.FillAndBlur("100");
        await Page.Locator("#check-threshold-btn").ClickAsync();

        await Expect(Page.Locator("#threshold-result"))
            .ToHaveTextAsync("Above threshold", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
