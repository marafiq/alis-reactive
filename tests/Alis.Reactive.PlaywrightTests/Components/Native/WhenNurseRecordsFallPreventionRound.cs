using System.Collections.Concurrent;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckList;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As a night nurse, I want the fall-prevention round to open with the resident's fall risk factors on
/// file, to call for hourly checks while a recent fall is ticked, to repeat last night's precautions in
/// one step, and to refuse a round without the precautions she needs, so that no resident at risk of
/// falling is left unprotected tonight.
/// </summary>
/// <remarks>
/// Edith Morales's round opens with "Fell in the last 30 days" and "Takes a sedative at night" on file.
/// The hourly-checks banner reads the risk factor list when the round opens (its <c>Value</c>) and as the
/// nurse ticks it (its <c>Change</c>, whose <c>Value</c> carries every box ticked). Unticking the last
/// precaution asks for one at once: the list's change is its commit, which checks it again. "Same
/// precautions as last night" writes the server's answer to the precaution list's <c>Value</c>: the
/// boxes show exactly last night's, and a box the nurse ticks after it adds to them in what is sent.
/// Only the server knows that a recent fall needs the bed alarm on. Not exercised: the CSV input the
/// list keeps for a form posted without the plan, which no page posts.
/// </remarks>
[TestFixture]
public class WhenNurseRecordsFallPreventionRound : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeCheckList";
    private const string RecentFall = "Fell in the last 30 days";
    private const string BedAlarm = "Bed alarm on";
    private const string CallLight = "Call light within reach";
    private const string LowBed = "Bed in its lowest position";
    private const string NonSlipSocks = "Non-slip socks on";
    private const string NightLight = "Night light on";

    private PagePlan<FallPreventionRound> _plan = null!;

    private NativeCheckListLocator RiskFactors => new(Page, _plan.FindComponent(m => m.RiskFactors)!.ElementId);
    private NativeCheckListLocator Precautions => new(Page, _plan.FindComponent(m => m.Precautions)!.ElementId);
    private ILocator HourlyChecks => Page.Locator("#hourly-checks");
    private ILocator RecordRound => Page.Locator("#record-fall-round-btn");
    private ILocator SameAsLastNight => Page.Locator("#same-as-last-night-btn");
    private ILocator Status => Page.Locator("#fall-round-status");

    // Every round the page sends the server to record.
    private ConcurrentQueue<string> _roundsSent = null!;

    private async Task NavigateAndBoot()
    {
        _roundsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains($"{Path}/FallRound/Record")) _roundsSent.Enqueue(request.Url);
        };
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

    // Recording the round afterwards is the server's later answer: a round sent on the tick would come first.
    [Test]
    public async Task ticking_a_precaution_clears_the_request_without_recording_again()
    {
        await NavigateAndBoot();
        await SignAs("JO");
        await ClickWhenStable(RecordRound);
        await Expect(_plan.ErrorFor(m => m.Precautions)).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Precautions.Tick(BedAlarm);

        await Expect(_plan.ErrorFor(m => m.Precautions)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await ClickWhenStable(RecordRound);
        await Expect(Status).ToHaveTextAsync(
            "Round recorded for Edith Morales by JO. Precautions: Bed alarm on. "
            + "Risk factors: Fell in the last 30 days, Takes a sedative at night.",
            new() { Timeout = 10000 });
        Assert.That(_roundsSent, Has.Count.EqualTo(1));
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

    // A precaution ticked before is cleared: the list shows last night's and nothing else.
    [Test]
    public async Task same_precautions_as_last_night_ticks_exactly_last_nights()
    {
        await NavigateAndBoot();
        await Precautions.Tick(NonSlipSocks);

        await ClickWhenStable(SameAsLastNight);

        await Expect(Precautions.Box(BedAlarm)).ToBeCheckedAsync(new() { Timeout = 10000 });
        await Expect(Precautions.Box(LowBed)).ToBeCheckedAsync();
        await Expect(Precautions.Box(CallLight)).ToBeCheckedAsync();
        await Expect(Precautions.Box(NonSlipSocks)).Not.ToBeCheckedAsync();
        await Expect(Precautions.Box(NightLight)).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    // What is sent is what the boxes show: last night's precautions and the one the nurse adds tonight.
    [Test]
    public async Task recording_after_repeating_last_night_sends_last_nights_precautions_with_tonights_addition()
    {
        await NavigateAndBoot();
        await SignAs("JO");
        await ClickWhenStable(SameAsLastNight);
        await Expect(Precautions.Box(BedAlarm)).ToBeCheckedAsync(new() { Timeout = 10000 });
        await Precautions.Tick(NightLight);

        await ClickWhenStable(RecordRound);

        await Expect(Status).ToHaveTextAsync(
            "Round recorded for Edith Morales by JO. Precautions: Bed in its lowest position, Call light within reach, "
            + "Bed alarm on, Night light on. Risk factors: Fell in the last 30 days, Takes a sedative at night.",
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
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
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
