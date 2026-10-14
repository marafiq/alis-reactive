using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckList;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As a night nurse, I want the fall-prevention round to open with the resident's fall risk factors on
/// file, to call for hourly checks while a recent fall is ticked, and to refuse a round without the
/// precautions she needs, so that no resident at risk of falling is left unprotected tonight.
/// </summary>
/// <remarks>
/// Edith Morales's round opens with "Fell in the last 30 days" and "Takes a sedative at night" on file.
/// The hourly-checks banner reads the risk factor list when the round opens (its <c>Value</c>) and as the
/// nurse ticks it (its <c>Change</c>, whose <c>Value</c> carries every box ticked). Unticking the last
/// precaution asks for one at once: the list's change is its commit, which checks it again. Only the
/// server knows that a recent fall needs the bed alarm on. Not exercised: writing the list's <c>Value</c>
/// from a plan, because the list keeps no setter, so neither its boxes nor the CSV it keeps for a plain
/// form post would follow the write; that CSV input itself, which only a form posted without the plan reads.
/// </remarks>
[TestFixture]
public class WhenNurseRecordsFallPreventionRound : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeCheckList";
    private const string RecentFall = "Fell in the last 30 days";
    private const string BedAlarm = "Bed alarm on";
    private const string CallLight = "Call light within reach";

    private PagePlan<FallPreventionRound> _plan = null!;

    private NativeCheckListLocator RiskFactors => new(Page, _plan.FindComponent(m => m.RiskFactors)!.ElementId);
    private NativeCheckListLocator Precautions => new(Page, _plan.FindComponent(m => m.Precautions)!.ElementId);
    private ILocator HourlyChecks => Page.Locator("#hourly-checks");
    private ILocator RecordRound => Page.Locator("#record-fall-round-btn");
    private ILocator Status => Page.Locator("#fall-round-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FallPreventionRound>.FromPage(Page);
    }

    private async Task SignAs(string initials) => await _plan.TextBox(m => m.CheckedBy).Fill(initials);

    [Test]
    public async Task the_round_opens_with_the_risk_factors_on_file_ticked()
    {
        await NavigateAndBoot();

        await Expect(RiskFactors.Box(RecentFall)).ToBeCheckedAsync();
        await Expect(RiskFactors.Box("Takes a sedative at night")).ToBeCheckedAsync();
        await Expect(RiskFactors.Box("Gets up to the bathroom at night")).Not.ToBeCheckedAsync();
        await Expect(RiskFactors.Box("Uses a walker")).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_recent_fall_on_file_calls_for_hourly_checks_when_the_round_opens()
    {
        await NavigateAndBoot();

        await Expect(HourlyChecks).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(HourlyChecks).ToHaveTextAsync("High fall risk: check on Edith every hour tonight.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_recent_fall_drops_the_hourly_checks_and_ticking_it_again_calls_for_them()
    {
        await NavigateAndBoot();
        await Expect(HourlyChecks).ToBeVisibleAsync(new() { Timeout = 5000 });

        await RiskFactors.Clear(RecentFall);
        await Expect(HourlyChecks).ToBeHiddenAsync(new() { Timeout = 5000 });

        await RiskFactors.Tick(RecentFall);
        await Expect(HourlyChecks).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The list's change carries every box ticked, not only the one just clicked.
    [Test]
    public async Task ticking_another_risk_factor_keeps_the_hourly_checks()
    {
        await NavigateAndBoot();
        await Expect(HourlyChecks).ToBeVisibleAsync(new() { Timeout = 5000 });

        await RiskFactors.Tick("Uses a walker");

        await Expect(RiskFactors.Box("Uses a walker")).ToBeCheckedAsync();
        await Expect(HourlyChecks).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_round_is_not_recorded_until_a_precaution_is_ticked()
    {
        await NavigateAndBoot();
        await SignAs("JO");

        await ClickWhenStable(RecordRound);

        await Expect(_plan.ErrorFor(m => m.Precautions)).ToHaveTextAsync("Tick the precautions in place tonight.", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("The round is not recorded yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ticking_a_precaution_clears_the_request_without_recording_again()
    {
        await NavigateAndBoot();
        await SignAs("JO");
        await ClickWhenStable(RecordRound);
        await Expect(_plan.ErrorFor(m => m.Precautions)).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Precautions.Tick(BedAlarm);

        await Expect(_plan.ErrorFor(m => m.Precautions)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("The round is not recorded yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_last_precaution_asks_for_one_at_once()
    {
        await NavigateAndBoot();
        await Precautions.Tick(BedAlarm);
        await Expect(_plan.ErrorFor(m => m.Precautions)).ToBeHiddenAsync();

        await Precautions.Clear(BedAlarm);

        await Expect(_plan.ErrorFor(m => m.Precautions)).ToHaveTextAsync("Tick the precautions in place tonight.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_round_is_not_recorded_without_the_nurses_initials()
    {
        await NavigateAndBoot();
        await Precautions.Tick(BedAlarm);

        await ClickWhenStable(RecordRound);

        await Expect(_plan.ErrorFor(m => m.CheckedBy)).ToHaveTextAsync("Enter your initials.", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("The round is not recorded yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_the_round_sends_the_precautions_and_risk_factors_ticked()
    {
        await NavigateAndBoot();
        await SignAs("JO");
        await Precautions.Tick(CallLight);
        await Precautions.Tick(BedAlarm);

        await ClickWhenStable(RecordRound);

        await Expect(Status).ToHaveTextAsync(
            "Round recorded for Edith Morales by JO. Precautions: Call light within reach, Bed alarm on. "
            + "Risk factors: Fell in the last 30 days, Takes a sedative at night.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_server_refuses_a_round_without_the_bed_alarm_after_a_recent_fall()
    {
        await NavigateAndBoot();
        await SignAs("JO");
        await Precautions.Tick(CallLight);

        await ClickWhenStable(RecordRound);

        await Expect(_plan.ErrorFor(m => m.Precautions)).ToHaveTextAsync(
            "A resident who fell in the last 30 days needs the bed alarm on.", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("The round is not recorded yet.");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task a_resident_with_no_recent_fall_is_recorded_without_the_bed_alarm()
    {
        await NavigateAndBoot();
        await SignAs("JO");
        await RiskFactors.Clear(RecentFall);
        await Precautions.Tick(CallLight);

        await ClickWhenStable(RecordRound);

        await Expect(Status).ToHaveTextAsync(
            "Round recorded for Edith Morales by JO. Precautions: Call light within reach. Risk factors: Takes a sedative at night.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
