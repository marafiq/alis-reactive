namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As the charge aide running a fire drill, I want one button that starts and ends the drill and says
/// which it will do, and a missing resident to put the call to the charge nurse one key press away, so
/// that the wing is cleared in order and no resident is left behind.
/// </summary>
/// <remarks>
/// The drill button reads its own text to know whether the drill is under way and rewrites it
/// (<c>TextContent</c> read and written, on its <c>Click</c>). Reporting a missing resident raises the
/// page's resident-missing event; the alert it opens moves the cursor to the call button (<c>Focus</c>), so
/// Enter calls the charge nurse, and the call button confirms the call in its own text. The click payload
/// carries no data, so there is nothing of it to show.
/// </remarks>
[TestFixture]
public class WhenChargeAideRunsFireDrill : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeButton";

    private ILocator Drill => Page.Locator("#drill-btn");
    private ILocator DrillStatus => Page.Locator("#drill-status");
    private ILocator ReportMissingResident => Page.Locator("#report-missing-resident-btn");
    private ILocator MissingResidentAlert => Page.Locator("#missing-resident-alert");
    private ILocator CallChargeNurse => Page.Locator("#call-charge-nurse-btn");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Drill).ToHaveTextAsync("Start Drill");
    }

    [Test]
    public async Task starting_the_drill_tells_staff_to_close_every_door()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Drill);

        await Expect(DrillStatus).ToHaveTextAsync("Drill under way. Close every door on the wing.", new() { Timeout = 5000 });
        await Expect(Drill).ToHaveTextAsync("End Drill");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ending_the_drill_records_the_all_clear()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Drill);
        await Expect(Drill).ToHaveTextAsync("End Drill", new() { Timeout = 5000 });

        await ClickWhenStable(Drill);

        await Expect(DrillStatus).ToHaveTextAsync("Drill ended at the all-clear.", new() { Timeout = 5000 });
        await Expect(Drill).ToHaveTextAsync("Start Drill");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reporting_a_missing_resident_raises_the_alert_with_the_cursor_on_the_call_button()
    {
        await NavigateAndBoot();
        await Expect(MissingResidentAlert).ToBeHiddenAsync();

        await ClickWhenStable(ReportMissingResident);

        await Expect(MissingResidentAlert).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(MissingResidentAlert.GetByText("A resident is not accounted for. Call the charge nurse now.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(CallChargeNurse).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pressing_enter_after_the_report_calls_the_charge_nurse()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ReportMissingResident);
        await Expect(MissingResidentAlert).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Keyboard.PressAsync("Enter");

        await Expect(CallChargeNurse).ToHaveTextAsync("Charge Nurse Called", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
