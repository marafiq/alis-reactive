using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInPacket;

namespace Alis.Reactive.PlaywrightTests.Patterns.MoveInPacket;

/// <summary>
/// As an admissions coordinator, I want everything I enter in a new resident's move-in packet, whatever
/// kind of field holds it, to reach her record when I save the packet and the business office when I send
/// it, so that nobody re-types or misses what I recorded.
/// </summary>
/// <remarks>
/// The packet opens on Margaret Thompson, record RES-1042, from the referral; the record number is carried
/// in a hidden field. Saving posts the packet as JSON; the business office's intake takes it as a form post.
/// Each side answers with what it received, one sentence for each part of the packet: the resident (hidden
/// field, text box, masked phone, radio group, select), her care (check box, check list, multi-select, text
/// area, rich text editor), her schedule (date, time, date and time, date range) and her billing (drop-down
/// list, multi-column combo box, autocomplete, numeric field, switch). The server builds each sentence from
/// the values it bound, mapping codes to names, so a value the page failed to send, sent under another name
/// or in a shape the server cannot bind reads differently, and a packet the server cannot bind at all is not
/// confirmed. Each test fills one part and reads that part's sentence; the parts it leaves empty are sent
/// empty. Every value a test reads back differs from the value the field opens with, except the record
/// number and the name, which come from the referral. Saving or sending without the resident's name sends
/// nothing: the request count shows it, because the server refuses such a packet as well.
/// </remarks>
[TestFixture]
public class WhenCoordinatorCompletesMoveInPacket : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/MoveInPacket";

    private static readonly Regex ErrorStyle = new(@"\balis-has-error\b");

    private PagePlan<MoveInPacketModel> _plan = null!;
    private ConcurrentQueue<string> _packetsSent = null!;

    private NativeTextBoxLocator ResidentName => _plan.TextBox(m => m.ResidentName);
    private ILocator SavePacket => Page.Locator("#save-packet-btn");
    private ILocator SendPacket => Page.Locator("#send-packet-btn");
    private ILocator PacketStatus => Page.Locator("#packet-status");
    private ILocator ResidentReceipt => Page.Locator("#receipt-resident");
    private ILocator CareReceipt => Page.Locator("#receipt-care");
    private ILocator ScheduleReceipt => Page.Locator("#receipt-schedule");
    private ILocator BillingReceipt => Page.Locator("#receipt-billing");

    private async Task NavigateAndBoot()
    {
        _packetsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Patterns/MoveInPacket/")) _packetsSent.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<MoveInPacketModel>.FromPage(Page);
    }

    private async Task SaveThePacket()
    {
        await ClickWhenStable(SavePacket);
        await Expect(PacketStatus).ToHaveTextAsync("Saved to Margaret Thompson's move-in packet.", new() { Timeout = 10000 });
    }

    private async Task SendThePacket()
    {
        await ClickWhenStable(SendPacket);
        await Expect(PacketStatus).ToHaveTextAsync(
            "The business office received Margaret Thompson's move-in packet.", new() { Timeout = 10000 });
    }

    private async Task FillTheResident()
    {
        await _plan.InputMask(m => m.FamilyPhone).FillAndBlur("5551234567");
        await _plan.NativeRadioGroup(m => m.CareLevel).Choose("Assisted living");
        await _plan.NativeDropDown(m => m.Mobility).Select("Uses a wheelchair");
    }

    private async Task FillTheCare()
    {
        await _plan.NativeCheckBox(m => m.HasAllergies).Check();
        var allergies = new NativeCheckListLocator(Page, _plan.FindComponent(m => m.Allergies)!.ElementId);
        await allergies.Tick("Peanuts");
        await allergies.Tick("Penicillin");
        await _plan.MultiSelect(m => m.Diet).SelectItems("Low sodium", "Diabetic");
        await _plan.TextBox(m => m.CareNotes).Fill("Hard of hearing on her left side.");
        await _plan.RichTextEditor(m => m.CarePlan).Fill("Walk with her to breakfast at eight.");
    }

    private async Task FillTheSchedule()
    {
        await _plan.DatePicker(m => m.MoveInDay).SelectDate(2026, 11, 16);
        await _plan.TimePicker(m => m.MedicationTime).SelectTime("8:30 AM");
        await _plan.DateTimePicker(m => m.PhysicianVisit).Select(2026, 11, 17, "2:00 PM");
        await _plan.DateRangePicker(m => m.TrialStay).SelectRange(2026, 11, 2, 2026, 11, 13);
    }

    private async Task FillTheBilling()
    {
        await new FusionDropDownListLocator(Page, _plan.FindComponent(m => m.Community)!.ElementId).Choose("Maple Grove Main Campus");
        await new FusionMultiColumnComboBoxLocator(Page, _plan.FindComponent(m => m.Insurance)!.ElementId).Choose("Medicare");
        await _plan.AutoComplete(m => m.Physician).TypeAndSelect("pat", "Dr. Anika Patel");
        await _plan.NumericTextBox(m => m.MonthlyRate).FillAndBlur("4250");
        await _plan.Switch(m => m.TextFamilyUpdates).Toggle();
    }

    [Test]
    public async Task the_packet_opens_on_the_referred_resident_and_is_not_saved_yet()
    {
        await NavigateAndBoot();

        await Expect(ResidentName.Input).ToHaveValueAsync("Margaret Thompson");
        await Expect(PacketStatus).ToHaveTextAsync("Not saved yet.");
        await Expect(ResidentReceipt).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_records_the_residents_record_number_phone_care_level_and_mobility()
    {
        await NavigateAndBoot();
        await FillTheResident();

        await SaveThePacket();

        await Expect(ResidentReceipt).ToHaveTextAsync(
            "Margaret Thompson (RES-1042), assisted living, uses a wheelchair. Family phone (555) 123-4567.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_records_her_allergies_diet_care_notes_and_care_plan()
    {
        await NavigateAndBoot();
        await FillTheCare();

        await SaveThePacket();

        await Expect(CareReceipt).ToHaveTextAsync(
            "Allergies: Peanuts, Penicillin. Diet: Low sodium, Diabetic. Care notes: Hard of hearing on her left side. Care plan: Walk with her to breakfast at eight.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_records_her_move_in_day_medication_time_physician_visit_and_trial_stay()
    {
        await NavigateAndBoot();
        await FillTheSchedule();

        await SaveThePacket();

        await Expect(ScheduleReceipt).ToHaveTextAsync(
            "Moves in Monday, November 16, 2026. Morning medication at 8:30 AM. First physician visit Tuesday, November 17, 2026 at 2:00 PM. Trial stay Monday, November 2 to Friday, November 13, 2026.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_records_her_community_insurer_physician_rate_and_family_texts()
    {
        await NavigateAndBoot();
        await FillTheBilling();

        await SaveThePacket();

        await Expect(BillingReceipt).ToHaveTextAsync(
            "Maple Grove Main Campus, Medicare (government), attending Dr. Anika Patel, $4,250.00 a month. Family updates by text.");
        AssertNoConsoleErrors();
    }

    // What is sent is what the packet holds when it is sent, not when the page opened or was last saved.
    [Test]
    public async Task saving_again_after_changing_the_care_level_records_the_new_one()
    {
        await NavigateAndBoot();
        await _plan.NativeRadioGroup(m => m.CareLevel).Choose("Assisted living");
        await SaveThePacket();
        await Expect(ResidentReceipt).ToHaveTextAsync(
            "Margaret Thompson (RES-1042), assisted living, mobility not recorded. No family phone.");

        await _plan.NativeRadioGroup(m => m.CareLevel).Choose("Memory care");
        await ClickWhenStable(SavePacket);

        await Expect(ResidentReceipt).ToHaveTextAsync(
            "Margaret Thompson (RES-1042), memory care, mobility not recorded. No family phone.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_business_office_receives_the_residents_record_number_phone_care_level_and_mobility()
    {
        await NavigateAndBoot();
        await FillTheResident();

        await SendThePacket();

        await Expect(ResidentReceipt).ToHaveTextAsync(
            "Margaret Thompson (RES-1042), assisted living, uses a wheelchair. Family phone (555) 123-4567.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_business_office_receives_her_allergies_diet_care_notes_and_care_plan()
    {
        await NavigateAndBoot();
        await FillTheCare();

        await SendThePacket();

        await Expect(CareReceipt).ToHaveTextAsync(
            "Allergies: Peanuts, Penicillin. Diet: Low sodium, Diabetic. Care notes: Hard of hearing on her left side. Care plan: Walk with her to breakfast at eight.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_business_office_receives_her_move_in_day_medication_time_physician_visit_and_trial_stay()
    {
        await NavigateAndBoot();
        await FillTheSchedule();

        await SendThePacket();

        await Expect(ScheduleReceipt).ToHaveTextAsync(
            "Moves in Monday, November 16, 2026. Morning medication at 8:30 AM. First physician visit Tuesday, November 17, 2026 at 2:00 PM. Trial stay Monday, November 2 to Friday, November 13, 2026.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_business_office_receives_her_community_insurer_physician_rate_and_family_texts()
    {
        await NavigateAndBoot();
        await FillTheBilling();

        await SendThePacket();

        await Expect(BillingReceipt).ToHaveTextAsync(
            "Maple Grove Main Campus, Medicare (government), attending Dr. Anika Patel, $4,250.00 a month. Family updates by text.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_without_the_residents_name_asks_for_it_and_saves_nothing()
    {
        await NavigateAndBoot();
        await ResidentName.Clear();

        await ClickWhenStable(SavePacket);

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.", new() { Timeout = 5000 });
        await Expect(ResidentName.Input).ToHaveClassAsync(ErrorStyle);
        await Expect(PacketStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_packetsSent, Is.Empty, "A packet the page can see has no resident is not saved.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_without_the_residents_name_asks_for_it_and_sends_nothing()
    {
        await NavigateAndBoot();
        await ResidentName.Clear();

        await ClickWhenStable(SendPacket);

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.", new() { Timeout = 5000 });
        await Expect(PacketStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_packetsSent, Is.Empty, "A packet the page can see has no resident is not sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_the_name_clears_the_request_for_it_at_once()
    {
        await NavigateAndBoot();
        await ResidentName.Clear();
        await ClickWhenStable(SavePacket);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.", new() { Timeout = 5000 });

        await ResidentName.Fill("Margaret");

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToBeHiddenAsync();
        await Expect(ResidentName.Input).Not.ToHaveClassAsync(ErrorStyle);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task emptying_the_corrected_name_asks_for_it_again_on_leaving_the_field()
    {
        await NavigateAndBoot();
        await ResidentName.Clear();
        await ClickWhenStable(SavePacket);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.", new() { Timeout = 5000 });
        await ResidentName.Fill("Margaret");
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToBeHiddenAsync();

        await ResidentName.Clear();
        await ResidentName.Blur();

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
