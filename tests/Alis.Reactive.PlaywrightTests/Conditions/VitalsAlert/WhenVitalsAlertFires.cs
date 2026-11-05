using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.VitalsAlert;

namespace Alis.Reactive.PlaywrightTests.Conditions.VitalsAlert;

[TestFixture]
public class WhenVitalsAlertFires : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/VitalsAlert";
    private const string RapidResponsePaged = "Rapid response team paged";
    private const string ChargeNurseAlerted = "Charge nurse alerted";

    private PagePlan<VitalsAlertModel> _plan = null!;

    private NumericTextBoxLocator HeartRate => _plan.NumericTextBox(m => m.HeartRate);
    private ILocator CheckTheHeartRate => Page.Locator("#check-heart-rate-btn");
    private ILocator AlertStatus => Page.Locator("#alert-status");
    private ILocator AlertMessage => Page.Locator("#alert-message");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<VitalsAlertModel>.FromPage(Page);
    }

    private async Task CheckHeartRate(string bpm)
    {
        await HeartRate.FillAndBlur(bpm);
        await ClickWhenStable(CheckTheHeartRate);
    }

    [Test]
    public async Task a_heart_rate_of_180_pages_the_rapid_response_team()
    {
        await NavigateAndBoot();

        await CheckHeartRate("180");

        await Expect(AlertStatus).ToHaveTextAsync(RapidResponsePaged, new() { Timeout = 5000 });
        await Expect(AlertMessage).ToHaveTextAsync(
            "The rapid response team is on its way to room 118: Walter Cho's heart rate is 180 bpm.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_heart_rate_of_179_alerts_the_charge_nurse()
    {
        await NavigateAndBoot();

        await CheckHeartRate("179");

        await Expect(AlertStatus).ToHaveTextAsync(ChargeNurseAlerted, new() { Timeout = 5000 });
        await Expect(AlertMessage).ToHaveTextAsync(
            "Dana Ortiz will recheck Walter Cho within 15 minutes: his heart rate is 179 bpm.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_heart_rate_of_140_alerts_the_charge_nurse()
    {
        await NavigateAndBoot();

        await CheckHeartRate("140");

        await Expect(AlertStatus).ToHaveTextAsync(ChargeNurseAlerted, new() { Timeout = 5000 });
        await Expect(AlertMessage).ToHaveTextAsync(
            "Dana Ortiz will recheck Walter Cho within 15 minutes: his heart rate is 140 bpm.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_heart_rate_of_139_alerts_no_one()
    {
        await NavigateAndBoot();

        await CheckHeartRate("139");

        await Expect(AlertStatus).ToHaveTextAsync("No alert needed", new() { Timeout = 5000 });
        await Expect(AlertMessage).ToHaveTextAsync("Under 140 bpm: no one needs to be alerted.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_heart_rate_back_under_140_after_a_page_needs_no_alert()
    {
        await NavigateAndBoot();
        await CheckHeartRate("196");
        await Expect(AlertStatus).ToHaveTextAsync(RapidResponsePaged, new() { Timeout = 5000 });
        await Expect(AlertMessage).ToHaveTextAsync(
            "The rapid response team is on its way to room 118: Walter Cho's heart rate is 196 bpm.");

        await CheckHeartRate("88");

        await Expect(AlertStatus).ToHaveTextAsync("No alert needed", new() { Timeout = 5000 });
        await Expect(AlertMessage).ToHaveTextAsync("Under 140 bpm: no one needs to be alerted.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_a_cleared_heart_rate_asks_for_the_reading()
    {
        await NavigateAndBoot();
        await CheckHeartRate("196");
        await Expect(AlertStatus).ToHaveTextAsync(RapidResponsePaged, new() { Timeout = 5000 });

        await HeartRate.Clear();
        await HeartRate.Blur();
        await ClickWhenStable(CheckTheHeartRate);

        await Expect(AlertStatus).ToHaveTextAsync("Enter the heart rate first", new() { Timeout = 5000 });
        await Expect(AlertMessage).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }
}
