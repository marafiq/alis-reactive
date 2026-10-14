using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.NumericCondition;

namespace Alis.Reactive.PlaywrightTests.Conditions.NumericCondition;

/// <summary>
/// As a nurse taking a resident's morning vital signs, I want each reading placed against its normal range
/// and against the heart rate limit in her care plan as I enter it, so that I know at once whether to
/// recheck it or call the nurse.
/// </summary>
/// <remarks>
/// Margaret Hale's care-plan limit is 100 bpm until the nurse changes it. A heart rate over 100 warns of
/// tachycardia; its zone is Critical from 120, High from 100, Normal from 60 and Low below. A systolic
/// reading from 60 to 120 is in range. Checking against the limit reads the heart rate and the limit as
/// they stand when the button is pressed. Each reading sits on the edge it tests: exactly at a threshold,
/// one above it, one below it.
/// </remarks>
[TestFixture]
public class WhenNumericConditionEvaluates : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/NumericCondition";

    private PagePlan<NumericConditionModel> _plan = null!;

    private NumericTextBoxLocator HeartRate => _plan.NumericTextBox(m => m.HeartRate);
    private NumericTextBoxLocator BloodPressure => _plan.NumericTextBox(m => m.BloodPressure);
    private NumericTextBoxLocator CarePlanLimit => _plan.NumericTextBox(m => m.ThresholdValue);
    private ILocator TachycardiaWarning => _plan.Element("tachycardia-warning");
    private ILocator Zone => _plan.Element("hr-zone");
    private ILocator Systolic => _plan.Element("bp-range");
    private ILocator CheckAgainstTheLimit => Page.Locator("#check-threshold-btn");
    private ILocator Reading => _plan.Element("threshold-band");
    private ILocator Recheck => _plan.Element("threshold-recheck");
    private ILocator NurseCall => _plan.Element("threshold-call");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<NumericConditionModel>.FromPage(Page);
    }

    private async Task EnterHeartRate(string bpm, string expectedZone)
    {
        await HeartRate.FillAndBlur(bpm);
        await Expect(Zone).ToHaveTextAsync(expectedZone, new() { Timeout = 5000 });
    }

    private async Task CheckHeartRateAgainstTheLimit(string bpm)
    {
        await HeartRate.FillAndBlur(bpm);
        await ClickWhenStable(CheckAgainstTheLimit);
    }

    [Test]
    public async Task a_heart_rate_over_100_warns_of_tachycardia()
    {
        await NavigateAndBoot();
        await Expect(TachycardiaWarning).ToBeHiddenAsync();

        await EnterHeartRate("101", "High");

        await Expect(TachycardiaWarning).ToBeVisibleAsync();
        await Expect(TachycardiaWarning).ToHaveTextAsync("Tachycardia detected");
        AssertNoConsoleErrors();
    }

    // The zone shows the reading was taken; a heart rate of exactly 100 is not yet tachycardia.
    [Test]
    public async Task a_heart_rate_of_exactly_100_gives_no_tachycardia_warning()
    {
        await NavigateAndBoot();

        await EnterHeartRate("100", "High");

        await Expect(TachycardiaWarning).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_tachycardia_warning_clears_when_the_heart_rate_comes_down()
    {
        await NavigateAndBoot();
        await EnterHeartRate("150", "Critical");
        await Expect(TachycardiaWarning).ToBeVisibleAsync();

        await EnterHeartRate("80", "Normal");

        await Expect(TachycardiaWarning).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // Each reading changes the zone shown, so every step is seen to be read.
    [Test]
    public async Task the_zone_is_critical_from_120_up()
    {
        await NavigateAndBoot();

        await EnterHeartRate("130", "Critical");
        await EnterHeartRate("119", "High");
        await EnterHeartRate("120", "Critical");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_zone_is_high_from_100()
    {
        await NavigateAndBoot();

        await EnterHeartRate("99", "Normal");
        await EnterHeartRate("100", "High");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_zone_is_normal_from_60_and_low_under_it()
    {
        await NavigateAndBoot();

        await EnterHeartRate("59", "Low");
        await EnterHeartRate("60", "Normal");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_systolic_reading_of_60_is_in_range_and_59_is_not()
    {
        await NavigateAndBoot();

        await BloodPressure.FillAndBlur("59");
        await Expect(Systolic).ToHaveTextAsync("Out of range", new() { Timeout = 5000 });

        await BloodPressure.FillAndBlur("60");
        await Expect(Systolic).ToHaveTextAsync("Normal range", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_systolic_reading_of_120_is_in_range_and_121_is_not()
    {
        await NavigateAndBoot();

        await BloodPressure.FillAndBlur("121");
        await Expect(Systolic).ToHaveTextAsync("Out of range", new() { Timeout = 5000 });

        await BloodPressure.FillAndBlur("120");
        await Expect(Systolic).ToHaveTextAsync("Normal range", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_reading_at_or_over_the_care_plan_limit_is_rechecked_and_one_under_it_is_not()
    {
        await NavigateAndBoot();

        await CheckHeartRateAgainstTheLimit("100");
        await Expect(Recheck).ToHaveTextAsync("Recheck the heart rate in 15 minutes.", new() { Timeout = 5000 });

        await CheckHeartRateAgainstTheLimit("99");
        await Expect(Recheck).ToHaveTextAsync("No recheck needed.", new() { Timeout = 5000 });

        await CheckHeartRateAgainstTheLimit("101");
        await Expect(Recheck).ToHaveTextAsync("Recheck the heart rate in 15 minutes.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_reading_is_checked_against_the_limit_the_nurse_last_set()
    {
        await NavigateAndBoot();
        await CheckHeartRateAgainstTheLimit("80");
        await Expect(Recheck).ToHaveTextAsync("No recheck needed.", new() { Timeout = 5000 });

        await CarePlanLimit.FillAndBlur("70");
        await ClickWhenStable(CheckAgainstTheLimit);

        await Expect(Recheck).ToHaveTextAsync("Recheck the heart rate in 15 minutes.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_care_plan_limit_calls_for_no_recheck()
    {
        await NavigateAndBoot();
        await CarePlanLimit.FillAndBlur("70");
        await CheckHeartRateAgainstTheLimit("80");
        await Expect(Recheck).ToHaveTextAsync("Recheck the heart rate in 15 minutes.", new() { Timeout = 5000 });

        await CarePlanLimit.Clear();
        await CarePlanLimit.Blur();
        await ClickWhenStable(CheckAgainstTheLimit);

        await Expect(Recheck).ToHaveTextAsync("No recheck needed.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // At exactly 100 a Gte or Lte in place of Gt or Lt would answer Over or Under.
    [Test]
    public async Task a_reading_over_under_or_at_the_care_plan_limit_is_named_as_such()
    {
        await NavigateAndBoot();

        await CheckHeartRateAgainstTheLimit("101");
        await Expect(Reading).ToHaveTextAsync("Over the care-plan limit", new() { Timeout = 5000 });

        await CheckHeartRateAgainstTheLimit("99");
        await Expect(Reading).ToHaveTextAsync("Under the care-plan limit", new() { Timeout = 5000 });

        await CheckHeartRateAgainstTheLimit("100");
        await Expect(Reading).ToHaveTextAsync("At the care-plan limit", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // A cleared limit is no limit: every ordering against it is false and it equals nothing.
    [Test]
    public async Task a_reading_with_no_care_plan_limit_is_not_placed_against_one()
    {
        await NavigateAndBoot();
        await CheckHeartRateAgainstTheLimit("80");
        await Expect(Reading).ToHaveTextAsync("Under the care-plan limit", new() { Timeout = 5000 });

        await CarePlanLimit.Clear();
        await CarePlanLimit.Blur();
        await ClickWhenStable(CheckAgainstTheLimit);

        await Expect(Reading).ToHaveTextAsync("No care-plan limit set", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // A reading at or under the limit needs no call: an Lt would call at 100, an Eq at 99.
    [Test]
    public async Task a_reading_at_or_under_the_care_plan_limit_needs_no_nurse_call_but_one_over_it_does()
    {
        await NavigateAndBoot();

        await CheckHeartRateAgainstTheLimit("100");
        await Expect(NurseCall).ToHaveTextAsync("No nurse call needed", new() { Timeout = 5000 });

        await CheckHeartRateAgainstTheLimit("101");
        await Expect(NurseCall).ToHaveTextAsync("Call the nurse", new() { Timeout = 5000 });

        await CheckHeartRateAgainstTheLimit("99");
        await Expect(NurseCall).ToHaveTextAsync("No nurse call needed", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
