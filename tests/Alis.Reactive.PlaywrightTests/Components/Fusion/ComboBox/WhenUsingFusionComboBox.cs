using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ComboBox;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ComboBox;

/// <summary>
/// As a nurse on the morning medication pass, I want to choose the resident by scanning their wristband
/// or by typing part of their last name, and see their room and allergies before I give a dose, so that
/// every dose goes to the resident it is for and none goes to a resident whose allergies I have not read.
/// </summary>
/// <remarks>
/// The 08:00 round on the Maple wing is Bell, Arthur (Maple 202); Hale, Margaret (Maple 214, penicillin);
/// Okafor, Grace (Maple 221, sulfa drugs); Russo, Dominic (Maple 230, latex), in that order. Their
/// wristbands read R-1031, R-1042, R-1057 and R-1063; R-2001 belongs to another round. Entering the
/// resident picker opens its list; a resident chosen by name, not by wristband, must have the wristband
/// checked, and the keyboard goes away once their chart shows. Recording a dose empties the picker and
/// opens it for the next resident.
/// Not exercised: that Change is the event that commits the picker's value. Only form validation listens
/// for it, to re-check a field when it fires, and a medication pass validates no form; the value the
/// input contract reads is what the Record gather sends. Change's IsInteracted cannot be told apart from
/// the payload's e and event fields: EJ2 sets it from them. The render helper without a build step is
/// not used: a picker rendered with no residents has nobody to choose.
/// </remarks>
[TestFixture]
public class WhenUsingFusionComboBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionComboBox";

    private PagePlan<FusionComboBoxModel> _plan = null!;

    private FusionComboBoxLocator Resident => new(Page, _plan.FindComponent(m => m.Resident)!.ElementId);
    private ILocator StartRound => Page.Locator("#start-round-btn");
    private ILocator RoundStarted => Page.Locator("#round-started");
    private ILocator OpenOverdueResident => Page.Locator("#open-overdue-btn");
    private ILocator WristbandUnknown => Page.Locator("#wristband-unknown");
    private ILocator ResidentMissing => Page.Locator("#resident-missing");
    private ILocator CheckWristband => Page.Locator("#check-wristband");
    private ILocator Chart => Page.Locator("#resident-chart");
    private ILocator Room => Page.Locator("#resident-room");
    private ILocator Allergies => Page.Locator("#resident-allergies");
    private ILocator RecordMedications => Page.Locator("#record-pass-btn");
    private ILocator PassStatus => Page.Locator("#pass-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionComboBoxModel>.FromPage(Page);
        await Expect(Resident.DropDownButton).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // The cart's scanner types the wristband's code into the wristband field and presses Enter.
    private async Task ScanWristband(string code)
    {
        var wristband = _plan.TextBox(m => m.Wristband);
        await wristband.Fill(code);
        await wristband.Input.PressAsync("Enter");
    }

    private async Task ScanMargaretHale()
    {
        await ScanWristband("R-1042");
        await Expect(Room).ToHaveTextAsync("Maple 214", new() { Timeout = 5000 });
    }

    [Test]
    public async Task entering_the_resident_picker_opens_the_list_of_residents_on_the_round()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Resident.Input);

        await Expect(Resident.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Resident.PopupItems).ToHaveTextAsync(new[] { "Bell, Arthur", "Hale, Margaret", "Okafor, Grace", "Russo, Dominic" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_part_of_a_last_name_chooses_the_resident_and_shows_their_room_and_allergies()
    {
        await NavigateAndBoot();

        await Resident.TypeAndPressEnter("Hal", "Hale, Margaret");

        await Expect(Resident.Input).ToHaveValueAsync("Hale, Margaret", new() { Timeout = 5000 });
        await Expect(Room).ToHaveTextAsync("Maple 214", new() { Timeout = 5000 });
        await Expect(Allergies).ToHaveTextAsync("Penicillin (hives)");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_chosen_by_name_must_have_their_wristband_checked()
    {
        await NavigateAndBoot();

        await Resident.TypeAndPressEnter("Hal", "Hale, Margaret");

        await Expect(CheckWristband).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // Enter takes the highlighted resident and leaves the cursor in the picker; only the page moves it out.
    [Test]
    public async Task choosing_a_resident_by_name_puts_the_keyboard_away_once_their_chart_shows()
    {
        await NavigateAndBoot();

        await Resident.TypeAndPressEnter("Hal", "Hale, Margaret");

        await Expect(Room).ToHaveTextAsync("Maple 214", new() { Timeout = 5000 });
        await Expect(Resident.Input).Not.ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_picker_without_a_resident_says_none_is_chosen()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Resident.Input);

        await Resident.Input.PressAsync("Escape");
        await Resident.Input.PressAsync("Tab");

        await Expect(Resident.Input).Not.ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(ResidentMissing).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task scanning_a_wristband_chooses_its_resident()
    {
        await NavigateAndBoot();

        await ScanWristband("R-1042");

        await Expect(Resident.Input).ToHaveValueAsync("Hale, Margaret", new() { Timeout = 5000 });
        await Expect(Room).ToHaveTextAsync("Maple 214", new() { Timeout = 5000 });
        await Expect(WristbandUnknown).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_chosen_by_wristband_needs_no_further_wristband_check()
    {
        await NavigateAndBoot();

        await ScanMargaretHale();

        await Expect(CheckWristband).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_wristband_from_another_round_chooses_nobody_and_says_so()
    {
        await NavigateAndBoot();

        await ScanWristband("R-2001");

        await Expect(WristbandUnknown).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Resident.Input).ToHaveValueAsync("");
        await Expect(Chart).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_the_round_opens_its_first_resident_and_says_whom_it_started_with()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StartRound);

        await Expect(RoundStarted).ToHaveTextAsync("Round started with Bell, Arthur.", new() { Timeout = 5000 });
        await Expect(Resident.Input).ToHaveValueAsync("Bell, Arthur");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_the_overdue_resident_puts_her_in_the_picker_with_her_chart()
    {
        await NavigateAndBoot();

        await ClickWhenStable(OpenOverdueResident);

        await Expect(Resident.Input).ToHaveValueAsync("Okafor, Grace", new() { Timeout = 5000 });
        await Expect(Room).ToHaveTextAsync("Maple 221", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The clear button empties the picker and leaves its list open; only the page closes the list.
    [Test]
    public async Task clearing_the_resident_closes_the_list_and_takes_down_their_chart()
    {
        await NavigateAndBoot();
        await ScanMargaretHale();
        await ClickWhenStable(Resident.Input);
        await Expect(Resident.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(Resident.ClearButton);

        await Expect(Resident.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(Resident.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Chart).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_the_medications_names_the_resident_and_their_place_on_the_round()
    {
        await NavigateAndBoot();
        await ScanMargaretHale();

        await ClickWhenStable(RecordMedications);

        await Expect(PassStatus).ToHaveTextAsync(
            "Recorded the 08:00 medications for Hale, Margaret, resident 2 of 4 on the round.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_the_medications_are_recorded_the_picker_is_empty_and_open_for_the_next_resident()
    {
        await NavigateAndBoot();
        await ScanMargaretHale();

        await ClickWhenStable(RecordMedications);

        await Expect(Resident.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(Resident.Input).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Resident.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Chart).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_without_a_resident_asks_for_one()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RecordMedications);

        await Expect(PassStatus).ToHaveTextAsync("Choose a resident before recording medications.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
