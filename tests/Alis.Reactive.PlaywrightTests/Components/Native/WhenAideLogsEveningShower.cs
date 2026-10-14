using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckBox;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As a care aide, I want the shower log to remind me of the shower chair a resident's care plan calls
/// for, to put the shower details away when she declines, and to make me pass a skin concern to the nurse
/// before I save, so that the evening shower is logged as it happened.
/// </summary>
/// <remarks>
/// Dorothy Reyes's log opens with her care plan's shower chair ticked; the chair note reads that box when
/// the log opens (its <c>Checked</c>) and follows it as the aide ticks it (its <c>Change</c> and the
/// change's <c>Checked</c>). Declining the shower clears the water temperature (a <c>Checked</c> write,
/// seen when the aide takes the refusal back), and a skin concern moves the cursor to telling the nurse
/// (<c>Focus</c>). Unchecking the water temperature asks for it at once: a box's change is its commit,
/// which checks it again. Every box is sent with the log, and the server's answer says what it received.
/// </remarks>
[TestFixture]
public class WhenAideLogsEveningShower : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/CheckBox";

    private PagePlan<ShowerLog> _plan = null!;

    private NativeCheckBoxLocator ShowerChair => _plan.NativeCheckBox(m => m.UsesShowerChair);
    private NativeCheckBoxLocator Declined => _plan.NativeCheckBox(m => m.ShowerDeclined);
    private NativeCheckBoxLocator WaterTemperature => _plan.NativeCheckBox(m => m.WaterTemperatureChecked);
    private NativeCheckBoxLocator SkinConcern => _plan.NativeCheckBox(m => m.SkinConcern);
    private NativeCheckBoxLocator NurseTold => _plan.NativeCheckBox(m => m.NurseTold);
    private ILocator ShowerChairNote => Page.Locator("#shower-chair-note");
    private ILocator ShowerDetails => Page.Locator("#shower-details");
    private ILocator OfferAgain => Page.Locator("#offer-again");
    private ILocator SkinConcernStep => Page.Locator("#skin-concern-step");
    private ILocator SaveLog => Page.Locator("#save-shower-log-btn");
    private ILocator Status => Page.Locator("#shower-log-status");

    private ConcurrentQueue<string> _logsSent = null!;

    private async Task NavigateAndBoot()
    {
        _logsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Components/CheckBox/ShowerLog/Save")) _logsSent.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<ShowerLog>.FromPage(Page);
    }

    [Test]
    public async Task the_shower_chair_note_shows_when_her_care_plan_calls_for_one()
    {
        await NavigateAndBoot();

        await Expect(ShowerChair.Input).ToBeCheckedAsync();
        await Expect(ShowerChairNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ShowerChairNote).ToHaveTextAsync("Bring the shower chair from the Willow supply room.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_shower_chair_hides_its_note_and_ticking_it_shows_it_again()
    {
        await NavigateAndBoot();
        await Expect(ShowerChairNote).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ShowerChair.Input.UncheckAsync();
        await Expect(ShowerChairNote).ToBeHiddenAsync(new() { Timeout = 5000 });

        await ShowerChair.Check();
        await Expect(ShowerChairNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task declining_the_shower_puts_the_details_away_and_says_to_offer_it_again()
    {
        await NavigateAndBoot();
        await Expect(ShowerDetails).ToBeVisibleAsync();

        await Declined.Check();

        await Expect(ShowerDetails).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(OfferAgain).ToHaveTextAsync("Offer the shower again before 9 pm.");
        await Expect(OfferAgain).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task taking_back_a_refusal_brings_the_details_back_with_the_water_temperature_to_check_again()
    {
        await NavigateAndBoot();
        await WaterTemperature.Check();
        await Declined.Check();
        await Expect(ShowerDetails).ToBeHiddenAsync(new() { Timeout = 5000 });

        await Declined.Input.UncheckAsync();

        await Expect(ShowerDetails).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(WaterTemperature.Input).Not.ToBeCheckedAsync();
        await Expect(OfferAgain).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task noticing_a_skin_concern_puts_the_cursor_on_telling_the_nurse()
    {
        await NavigateAndBoot();

        await SkinConcern.Check();

        await Expect(SkinConcernStep).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NurseTold.Input).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Space");
        await Expect(NurseTold.Input).ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_skin_concern_taken_back_puts_the_nurse_step_away()
    {
        await NavigateAndBoot();
        await SkinConcern.Check();
        await Expect(SkinConcernStep).ToBeVisibleAsync(new() { Timeout = 5000 });

        await SkinConcern.Input.UncheckAsync();

        await Expect(SkinConcernStep).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_log_is_not_saved_until_the_water_temperature_is_checked()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SaveLog);

        await Expect(_plan.ErrorFor(m => m.WaterTemperatureChecked))
            .ToHaveTextAsync("Check the water temperature before Dorothy's shower.", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("The shower log is not saved yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_the_water_temperature_clears_its_message_without_saving_again()
    {
        await NavigateAndBoot();
        await ClickWhenStable(SaveLog);
        await Expect(_plan.ErrorFor(m => m.WaterTemperatureChecked)).ToBeVisibleAsync(new() { Timeout = 5000 });

        await WaterTemperature.Check();

        await Expect(_plan.ErrorFor(m => m.WaterTemperatureChecked)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("The shower log is not saved yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unchecking_the_water_temperature_asks_for_it_at_once()
    {
        await NavigateAndBoot();
        await WaterTemperature.Check();

        await WaterTemperature.Input.UncheckAsync();

        await Expect(_plan.ErrorFor(m => m.WaterTemperatureChecked))
            .ToHaveTextAsync("Check the water temperature before Dorothy's shower.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // Leaving the nurse step for Save shows the message before Save is handled, so the refused log is shown never
    // to leave the page by the save that follows: the log with the nurse told is the only one sent.
    [Test]
    public async Task a_skin_concern_must_be_passed_to_the_nurse_before_saving()
    {
        await NavigateAndBoot();
        await WaterTemperature.Check();
        await SkinConcern.Check();

        await ClickWhenStable(SaveLog);

        await Expect(_plan.ErrorFor(m => m.NurseTold))
            .ToHaveTextAsync("Tell the nurse on duty about the skin concern before you save.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.NurseTold)).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("The shower log is not saved yet.");
        await NurseTold.Check();
        await ClickWhenStable(SaveLog);
        await Expect(Status).ToHaveTextAsync(
            "Shower log saved for Dorothy Reyes: shower given with the shower chair; skin concern reported to the nurse.",
            new() { Timeout = 10000 });
        Assert.That(_logsSent, Has.Count.EqualTo(1), "Only the log with the nurse told is sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_declined_shower_is_saved_without_the_water_temperature()
    {
        await NavigateAndBoot();
        await Declined.Check();

        await ClickWhenStable(SaveLog);

        await Expect(Status).ToHaveTextAsync(
            "Shower log saved for Dorothy Reyes: shower declined, offer it again before 9 pm.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_sends_what_the_aide_ticked()
    {
        await NavigateAndBoot();
        await WaterTemperature.Check();
        await SkinConcern.Check();
        await NurseTold.Check();

        await ClickWhenStable(SaveLog);

        await Expect(Status).ToHaveTextAsync(
            "Shower log saved for Dorothy Reyes: shower given with the shower chair; skin concern reported to the nurse.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
