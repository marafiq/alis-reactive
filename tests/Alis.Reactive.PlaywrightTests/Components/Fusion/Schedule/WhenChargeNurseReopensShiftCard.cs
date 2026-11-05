namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Schedule;

[TestFixture]
public class WhenChargeNurseReopensShiftCard : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Schedule/ShiftCard";

    private ILocator DayStatus => Page.Locator("#shift-card-day-status");
    private ILocator SignOffs => Page.Locator("#shift-card-signoffs");
    private ILocator MorningMedicationPass => Page.Locator(".e-appointment", new() { HasText = "Morning medication pass" });
    private ILocator ShiftCard => Page.Locator(".e-quick-popup-wrapper");
    private ILocator CardContent => Page.Locator("#shift-card-content");
    private ILocator AcknowledgeHandover => Page.Locator("#acknowledge-handover-btn");
    private ILocator LatestHandover => Page.Locator("#latest-handover-btn");
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
    public async Task an_acknowledged_handover_is_signed_off_once_its_acknowledgement_ends()
    {
        await NavigateAndLoadToday();
        await OpenTheMorningMedicationPassCard();

        await ClickWhenStable(AcknowledgeHandover);

        await Expect(CardStatus).ToHaveTextAsync("Handover acknowledged for Morning medication pass.");
        await Expect(SignOffs).ToHaveTextAsync("Morning medication pass handover signed off: revision 1.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_reopened_shift_card_acknowledges_its_handover_without_a_page_error()
    {
        await NavigateAndLoadToday();
        await OpenTheMorningMedicationPassCard();
        await CloseTheCard();

        await OpenTheMorningMedicationPassCard();
        await Expect(CardStatus).ToHaveTextAsync("Handover revision 1 not acknowledged yet.");
        await ClickWhenStable(AcknowledgeHandover);

        await Expect(CardStatus).ToHaveTextAsync("Handover acknowledged for Morning medication pass.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task loading_the_latest_handover_mid_acknowledgement_signs_off_only_the_latest_revision()
    {
        await NavigateAndLoadToday();
        await OpenTheMorningMedicationPassCard();
        await ClickWhenStable(AcknowledgeHandover);
        await Expect(CardStatus).ToHaveTextAsync("Handover acknowledged for Morning medication pass.");

        await ClickWhenStable(LatestHandover);
        await Expect(CardStatus).ToHaveTextAsync("Handover revision 2 not acknowledged yet.");
        await ClickWhenStable(AcknowledgeHandover);

        await Expect(SignOffs).ToHaveTextAsync("Morning medication pass handover signed off: revision 2.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
