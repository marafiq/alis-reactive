namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Schedule;

/// <summary>
/// As a charge nurse, I want to open a shift's card on today's schedule, close it, and open it again
/// all shift long, so that every card I open still lets me acknowledge its handover.
/// </summary>
/// <remarks>
/// Schedule destroys the card's buttons each time the card closes, before the next card loads its
/// handover into the same place.
/// </remarks>
[TestFixture]
public class WhenChargeNurseReopensShiftCard : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Schedule/ShiftCard";

    private ILocator DayStatus => Page.Locator("#shift-card-day-status");
    private ILocator MorningMedicationPass => Page.Locator(".e-appointment", new() { HasText = "Morning medication pass" });
    private ILocator ShiftCard => Page.Locator(".e-quick-popup-wrapper");
    private ILocator CardContent => Page.Locator("#shift-card-content");
    private ILocator AcknowledgeHandover => Page.Locator("#acknowledge-handover-btn");
    private ILocator CardStatus => Page.Locator("#shift-card-status");

    private async Task NavigateAndLoadToday()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(DayStatus).ToHaveTextAsync("2 shifts on the floor today.");
    }

    private async Task OpenTheMorningMedicationPassCard()
    {
        await MorningMedicationPass.ScrollIntoViewIfNeededAsync();
        await MorningMedicationPass.ClickAsync();
        await Expect(CardContent).ToContainTextAsync("Handover for Morning medication pass");
    }

    private async Task CloseTheCard()
    {
        await ClickWhenStable(ShiftCard.GetByTitle("Close", new() { Exact = true }));
        await Expect(CardContent).Not.ToBeAttachedAsync();
    }

    [Test]
    public async Task an_opened_shift_card_acknowledges_its_handover()
    {
        await NavigateAndLoadToday();
        await OpenTheMorningMedicationPassCard();

        await ClickWhenStable(AcknowledgeHandover);

        await Expect(CardStatus).ToHaveTextAsync("Handover acknowledged for Morning medication pass.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_reopened_shift_card_acknowledges_its_handover_without_a_page_error()
    {
        await NavigateAndLoadToday();
        await OpenTheMorningMedicationPassCard();
        await CloseTheCard();

        await OpenTheMorningMedicationPassCard();
        await Expect(CardStatus).ToHaveTextAsync("Handover not acknowledged yet.");
        await ClickWhenStable(AcknowledgeHandover);

        await Expect(CardStatus).ToHaveTextAsync("Handover acknowledged for Morning medication pass.");
        AssertNoConsoleErrors();
    }
}
