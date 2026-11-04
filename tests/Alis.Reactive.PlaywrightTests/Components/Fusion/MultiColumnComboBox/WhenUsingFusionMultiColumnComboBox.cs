using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiColumnComboBox;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.MultiColumnComboBox;

[TestFixture]
public class WhenUsingFusionMultiColumnComboBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/MultiColumnComboBox";
    private const string WillowTonight = "Tonight 19:00–07:00, Willow (Priya Raman called out)";
    private const string MapleTomorrow = "Tomorrow 07:00–15:00, Maple (Lena Ortiz on leave)";
    private const string CedarSaturday = "Saturday 15:00–23:00, Cedar (Omar Haddad in training)";

    private PagePlan<FusionMultiColumnComboBoxModel> _plan = null!;

    private FusionMultiColumnComboBoxLocator Nurse => new(Page, _plan.FindComponent(m => m.CoveringNurse)!.ElementId);
    private ILocator ChooseGraceKim => Page.Locator("#choose-grace-kim-btn");
    private ILocator ShiftCovered => Page.Locator("#shift-covered");
    private ILocator VolunteerNote => Page.Locator("#volunteer-note");
    private ILocator NobodyYet => Page.Locator("#nobody-yet");
    private ILocator CallToConfirm => Page.Locator("#call-to-confirm");
    private ILocator NurseWeek => Page.Locator("#nurse-week");
    private ILocator AssignCoverage => Page.Locator("#assign-coverage-btn");
    private ILocator CoverageStatus => Page.Locator("#coverage-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionMultiColumnComboBoxModel>.FromPage(Page);
        await Expect(Nurse.DropDownButton).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task ChooseShift(string shift) => await _plan.NativeDropDown(m => m.OpenShift).Select(shift);

    private async Task ChooseWillowTonight()
    {
        await ChooseShift(WillowTonight);
        await Expect(NobodyYet).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task choosing_a_shift_nobody_has_volunteered_for_opens_the_nurse_list()
    {
        await NavigateAndBoot();

        await ChooseShift(WillowTonight);

        await Expect(NobodyYet).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NobodyYet).ToHaveTextAsync("Nobody has volunteered for this shift yet. Choose a nurse from the list.");
        await Expect(Nurse.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Nurse.Row("Grace Kim")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_nurse_who_volunteered_by_text_is_filled_in_and_the_list_closes()
    {
        await NavigateAndBoot();

        await ChooseShift(MapleTomorrow);

        await Expect(VolunteerNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(VolunteerNote).ToHaveTextAsync("Jordan Ellis volunteered by text for this shift.");
        await Expect(Nurse.Input).ToHaveValueAsync("Jordan Ellis");
        await Expect(Nurse.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_nurse_who_volunteered_needs_no_call_to_confirm()
    {
        await NavigateAndBoot();

        await ChooseShift(MapleTomorrow);

        await Expect(NurseWeek).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NurseWeek).ToHaveTextAsync("Jordan Ellis has worked 16 of 40 hours this week.");
        await Expect(CallToConfirm).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_shift_already_covered_closes_the_nurse_list_and_says_who_covers_it()
    {
        await NavigateAndBoot();

        await ChooseShift(CedarSaturday);

        await Expect(ShiftCovered).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ShiftCovered).ToHaveTextAsync("Ana Lima already covers this shift.");
        await Expect(Nurse.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Nurse.Wrapper).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\be-input-focus\b"));
        await Expect(Nurse.Input).ToHaveValueAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_nurse_list_shows_each_nurses_home_wing_shift_and_certifications()
    {
        await NavigateAndBoot();

        await Nurse.Open();

        await Expect(Nurse.ColumnHeaders).ToHaveTextAsync(new[] { "Name", "Home wing", "Shift", "Certifications", "Employer" });
        await Expect(Nurse.Cells("Jordan Ellis")).ToHaveTextAsync(new[] { "Jordan Ellis", "Willow", "Evenings", "LPN, Dementia care", "Maple Grove staff" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_nurse_by_hand_asks_the_coordinator_to_call_her_to_confirm()
    {
        await NavigateAndBoot();

        await Nurse.Choose("Sam Ortiz");

        await Expect(CallToConfirm).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(CallToConfirm).ToHaveTextAsync("Call Sam Ortiz to confirm before assigning the shift.");
        await Expect(Nurse.Input).ToHaveValueAsync("Sam Ortiz");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_nurse_shows_the_hours_she_has_worked_this_week()
    {
        await NavigateAndBoot();

        await Nurse.Choose("Marcus Bell");

        await Expect(NurseWeek).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NurseWeek).ToHaveTextAsync("Marcus Bell has worked 40 of 40 hours this week; this shift would be overtime.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_another_nurse_than_the_volunteer_takes_down_the_volunteer_note()
    {
        await NavigateAndBoot();
        await ChooseShift(MapleTomorrow);
        await Expect(VolunteerNote).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Nurse.Choose("Ana Lima");

        await Expect(VolunteerNote).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(CallToConfirm).ToBeVisibleAsync();
        await Expect(CallToConfirm).ToHaveTextAsync("Call Ana Lima to confirm before assigning the shift.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_grace_kim_from_priyas_text_fills_her_in()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ChooseGraceKim);

        await Expect(Nurse.Input).ToHaveValueAsync("Grace Kim", new() { Timeout = 5000 });
        await Expect(NurseWeek).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NurseWeek).ToHaveTextAsync("Grace Kim has worked 24 of 40 hours this week.");
        await Expect(CallToConfirm).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task erasing_the_nurse_takes_down_the_call_to_confirm_and_her_hours()
    {
        await NavigateAndBoot();
        await Nurse.Choose("Sam Ortiz");
        await Expect(CallToConfirm).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NurseWeek).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Nurse.Input.PressAsync("ControlOrMeta+a");
        await Nurse.Input.PressAsync("Backspace");
        await Nurse.Input.PressAsync("Enter");

        await Expect(Nurse.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(CallToConfirm).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(NurseWeek).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task including_agency_nurses_adds_them_to_the_list_under_their_agency()
    {
        await NavigateAndBoot();
        await Nurse.Open();
        await Expect(Nurse.Row("Kim Tran")).ToHaveCountAsync(0);
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Nurse.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });

        await _plan.NativeCheckBox(m => m.IncludeAgency).Check();
        await Expect(Nurse.Input).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Nurse.Open();

        await Expect(Nurse.GroupCaptions.Filter(new() { HasText = "CareBridge agency" })).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Nurse.Cells("Kim Tran")).ToHaveTextAsync(new[] { "Kim Tran", "Any", "Nights", "RN, Dementia care", "CareBridge agency" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task including_agency_nurses_moves_the_cursor_to_the_nurse_list()
    {
        await NavigateAndBoot();
        var includeAgency = _plan.NativeCheckBox(m => m.IncludeAgency);

        await includeAgency.Check();

        await Expect(Nurse.Input).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(includeAgency.Input).ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_the_volunteer_covers_the_shift()
    {
        await NavigateAndBoot();
        await ChooseShift(MapleTomorrow);
        await Expect(Nurse.Input).ToHaveValueAsync("Jordan Ellis", new() { Timeout = 5000 });

        await ClickWhenStable(AssignCoverage);

        await Expect(CoverageStatus).ToHaveTextAsync("Jordan Ellis covers the Maple day shift tomorrow, 07:00–15:00.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_nurse_not_certified_in_dementia_care_is_refused_for_willow()
    {
        await NavigateAndBoot();
        await ChooseWillowTonight();
        await Nurse.Pick("Sam Ortiz");

        await ClickWhenStable(AssignCoverage);

        await Expect(CoverageStatus).ToHaveTextAsync(
            "Sam Ortiz is not certified in dementia care; Willow needs a nurse who is.", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task assigning_without_a_shift_or_a_nurse_says_what_is_missing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AssignCoverage);

        await Expect(_plan.ErrorFor(m => m.OpenShift)).ToHaveTextAsync("Choose the open shift to cover.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.CoveringNurse)).ToHaveTextAsync("Choose the nurse who covers the shift.");
        await Expect(CoverageStatus).ToHaveTextAsync("No shift assigned yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_the_missing_nurse_clears_her_message_without_assigning_again()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AssignCoverage);
        await Expect(_plan.ErrorFor(m => m.CoveringNurse)).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Nurse.Choose("Ana Lima");

        await Expect(_plan.ErrorFor(m => m.CoveringNurse)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.OpenShift)).ToHaveTextAsync("Choose the open shift to cover.");
        await Expect(Nurse.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }
}
