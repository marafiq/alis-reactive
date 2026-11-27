using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAdmissionStaffArrangeRoomChoices(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F36.M21_X1400_0)]
    public async Task a_referral_assigns_or_releases_the_single_room()
    {
        var booked = await NewWorld();
        await booked.Page.GotoAsync(CarePages.RoomReferral);
        await booked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply referral", Exact = true }).ClickAsync();
        await booked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review assignment", Exact = true }).ClickAsync();
        await Expect(booked.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync();
        await Expect(booked.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Assignment", Exact = true })).ToHaveTextAsync("Single room assigned");

        var released = await NewWorld();
        await released.Page.GotoAsync(CarePages.RoomReferralReleased);
        await released.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply referral", Exact = true }).ClickAsync();
        await released.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review assignment", Exact = true }).ClickAsync();
        await Expect(released.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
        await Expect(released.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Assignment", Exact = true })).ToHaveTextAsync("Awaiting room assignment");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1401_0)]
    public async Task the_cleaning_hold_applies_before_a_reservation_is_attempted()
    {
        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.RoomCleaning);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply availability and reserve", Exact = true }).ClickAsync();
        await Expect(held.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Reservation", Exact = true })).ToHaveTextAsync("Held for cleaning");
        await Expect(held.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToHaveAttributeAsync("disabled", "");

        var ready = await NewWorld();
        await ready.Page.GotoAsync(CarePages.RoomCleaned);
        await ready.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply availability and reserve", Exact = true }).ClickAsync();
        await Expect(ready.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Reservation", Exact = true })).ToHaveTextAsync("Single room reserved");
        await Expect(ready.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).Not.ToHaveAttributeAsync("disabled", "");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1402_0)]
    public async Task the_activity_records_the_residents_chosen_room()
    {
        var single = await NewWorld();
        await single.Page.GotoAsync(CarePages.RoomReferral);
        await single.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(single.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Placement activity", Exact = true })).ToHaveTextAsync("Single room requested");

        var companion = await NewWorld();
        await companion.Page.GotoAsync(CarePages.RoomReferral);
        await companion.Page.GetByText("Companion suite", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(companion.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Placement activity", Exact = true })).ToHaveTextAsync("Companion suite requested");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1403_0)]
    public async Task the_referral_takes_keyboard_focus_to_the_recommended_option()
    {
        var single = await NewWorld();
        await single.Page.GotoAsync(CarePages.RoomReferral);
        await single.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review recommended room", Exact = true }).ClickAsync();
        await Expect(single.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeFocusedAsync();

        var companion = await NewWorld();
        await companion.Page.GotoAsync(CarePages.RoomReferralReleased);
        await companion.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review recommended room", Exact = true }).ClickAsync();
        await Expect(companion.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Companion suite", Exact = true })).ToBeFocusedAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1404_0)]
    public async Task a_respite_stay_can_replace_a_primary_room_or_remain_independent()
    {
        var included = await NewWorld();
        await included.Page.GotoAsync(CarePages.RespiteIncluded);
        await included.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply respite arrangement", Exact = true }).ClickAsync();
        await included.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await included.Page.GetByText("Short stay", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await included.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review primary room", Exact = true }).ClickAsync();
        await Expect(included.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Primary placement", Exact = true })).ToHaveTextAsync("Short stay replaces the single room");

        var independent = await NewWorld();
        await independent.Page.GotoAsync(CarePages.RespiteIndependent);
        await independent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply respite arrangement", Exact = true }).ClickAsync();
        await independent.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await independent.Page.GetByText("Short stay", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await independent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review primary room", Exact = true }).ClickAsync();
        await Expect(independent.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Primary placement", Exact = true })).ToHaveTextAsync("Single room with independent respite");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1405_0)]
    public async Task a_contract_bed_keeps_its_reserved_code_when_the_garden_wing_is_requested()
    {
        var ordinary = await NewWorld();
        await ordinary.Page.GotoAsync(CarePages.AdmissionRoomBooking);
        await ordinary.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Allocate garden wing", Exact = true }).ClickAsync();
        await Expect(ordinary.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Proposed booking", Exact = true })).ToHaveTextAsync("Garden wing");
        await ordinary.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Confirm booking", Exact = true }).ClickAsync();
        await Expect(ordinary.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Booking receipt", Exact = true })).ToHaveTextAsync("Garden wing reserved");

        var contract = await NewWorld();
        await contract.Page.GotoAsync(CarePages.RoomContractBooking);
        await contract.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Allocate garden wing", Exact = true }).ClickAsync();
        await Expect(contract.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Proposed booking", Exact = true })).ToHaveTextAsync("Garden wing");
        await contract.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Confirm booking", Exact = true }).ClickAsync();
        await Expect(contract.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Booking receipt", Exact = true })).ToHaveTextAsync("Contract bed reserved");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1406_0)]
    public async Task the_residents_reading_card_uses_the_chosen_wording_and_order()
    {
        var arabic = await NewWorld();
        await arabic.Page.GotoAsync(CarePages.RoomCard);
        await arabic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use Arabic card", Exact = true }).ClickAsync();
        await arabic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review card", Exact = true }).ClickAsync();
        await Expect(arabic.Page.GetByText("غرفة مفردة", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await arabic.Page.GetByText("غرفة مفردة", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(arabic.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "غرفة مفردة", Exact = true })).ToBeCheckedAsync();
        await Expect(arabic.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print instruction", Exact = true })).ToHaveTextAsync("Print right to left with the room name first");

        var english = await NewWorld();
        await english.Page.GotoAsync(CarePages.RoomCard);
        await english.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use Arabic card", Exact = true }).ClickAsync();
        await english.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use English card", Exact = true }).ClickAsync();
        await english.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review card", Exact = true }).ClickAsync();
        await Expect(english.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await english.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(english.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync();
        await Expect(english.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print instruction", Exact = true })).ToHaveTextAsync("Print left to right with the room name last");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1407_0)]
    public async Task the_housekeeping_slip_reflects_the_native_availability_of_the_room()
    {
        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.RoomAvailabilityHeld);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Issue availability slip", Exact = true }).ClickAsync();
        await Expect(held.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Availability slip", Exact = true })).ToHaveTextAsync("Hold for housekeeping");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.RoomAvailabilityOpen);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Issue availability slip", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Availability slip", Exact = true })).ToHaveTextAsync("Garden suite");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1408_0)]
    public async Task the_transfer_desk_checks_the_mounted_rooms_destination()
    {
        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.RoomAvailabilityHeld);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check transfer destination", Exact = true }).ClickAsync();
        await Expect(held.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transfer destination", Exact = true })).ToHaveTextAsync("Transfer on hold");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.RoomAvailabilityOpen);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check transfer destination", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transfer destination", Exact = true })).ToHaveTextAsync("Garden suite");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1409_0)]
    public async Task the_next_visit_restores_a_provisional_choice_only_on_a_remembering_workstation()
    {
        var remembered = await NewWorld();
        await remembered.Page.GotoAsync(CarePages.RoomVisit);
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remember provisional choice", Exact = true }).ClickAsync();
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review storage policy", Exact = true }).ClickAsync();
        await Expect(remembered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Storage policy", Exact = true })).ToHaveTextAsync("Remember this room");
        await remembered.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await remembered.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open next visit", Exact = true }).ClickAsync();
        await Expect(remembered.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync();

        var defaulted = await NewWorld();
        await defaulted.Page.GotoAsync(CarePages.RoomVisit);
        await defaulted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use referral default", Exact = true }).ClickAsync();
        await defaulted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review storage policy", Exact = true }).ClickAsync();
        await Expect(defaulted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Storage policy", Exact = true })).ToHaveTextAsync("Use referral default");
        await defaulted.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await defaulted.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open next visit", Exact = true }).ClickAsync();
        await Expect(defaulted.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1410_0)]
    public async Task closing_the_preview_releases_the_original_room_choice()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.RoomPreview);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close room preview", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#preview-room")).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
        await Expect(closed.Page.GetByText("Garden suite", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);

        var continued = await NewWorld();
        await continued.Page.GotoAsync(CarePages.RoomPreview);
        await continued.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Continue room preview", Exact = true }).ClickAsync();
        await Expect(continued.Page.Locator("#preview-room")).ToBeCheckedAsync();
        await Expect(continued.Page.GetByText("Garden suite", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1411_0)]
    public async Task only_the_house_template_adds_its_print_legend_to_the_room_card()
    {
        var imported = await NewWorld();
        await imported.Page.GotoAsync(CarePages.RoomCardPrinting);
        await imported.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply imported card", Exact = true }).ClickAsync();
        await Expect(imported.Page.GetByText("Print on cream stock", new PageGetByTextOptions { Exact = true })).Not.ToBeVisibleAsync();
        await Expect(imported.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print policy", Exact = true })).ToHaveTextAsync("Imported styling filtered");

        var house = await NewWorld();
        await house.Page.GotoAsync(CarePages.RoomCardPrinting);
        await house.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply house card", Exact = true }).ClickAsync();
        await Expect(house.Page.GetByText("Print on cream stock", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(house.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print policy", Exact = true })).ToHaveTextAsync("House styling enabled");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1412_0)]
    public async Task a_room_alias_can_be_removed_or_restored_on_the_resident_card()
    {
        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.RoomCard);
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear room alias", Exact = true }).ClickAsync();
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review alias", Exact = true }).ClickAsync();
        await Expect(cleared.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Room alias", Exact = true })).ToHaveTextAsync("Room alias cleared");
        await Expect(cleared.Page.GetByText("Referral room", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);

        var restored = await NewWorld();
        await restored.Page.GotoAsync(CarePages.RoomCard);
        await restored.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear room alias", Exact = true }).ClickAsync();
        await restored.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Restore room alias", Exact = true }).ClickAsync();
        await restored.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review alias", Exact = true }).ClickAsync();
        await Expect(restored.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Room alias", Exact = true })).ToHaveTextAsync("Garden suite");
        await Expect(restored.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Garden suite", Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1413_0)]
    public async Task an_attached_room_saving_handler_survives_a_change_back_to_the_referral_default()
    {
        var previouslyRemembered = await NewWorld();
        await previouslyRemembered.Page.GotoAsync(CarePages.RoomVisit);
        await previouslyRemembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remember provisional choice", Exact = true }).ClickAsync();
        await previouslyRemembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use referral default", Exact = true }).ClickAsync();
        await previouslyRemembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review storage policy", Exact = true }).ClickAsync();
        await Expect(previouslyRemembered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Storage policy", Exact = true })).ToHaveTextAsync("Use referral default");
        await previouslyRemembered.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await previouslyRemembered.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open next visit", Exact = true }).ClickAsync();
        await Expect(previouslyRemembered.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync();

        var clean = await NewWorld();
        await clean.Page.GotoAsync(CarePages.RoomVisit);
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use referral default", Exact = true }).ClickAsync();
        await clean.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review storage policy", Exact = true }).ClickAsync();
        await Expect(clean.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Storage policy", Exact = true })).ToHaveTextAsync("Use referral default");
        await clean.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await clean.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open next visit", Exact = true }).ClickAsync();
        await Expect(clean.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1414_0)]
    public async Task a_desk_activation_records_a_selection_while_a_written_referral_only_assigns_it()
    {
        var activated = await NewWorld();
        await activated.Page.GotoAsync(CarePages.RoomCleaned);
        await activated.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply availability and reserve", Exact = true }).ClickAsync();
        await Expect(activated.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync();
        await Expect(activated.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Placement activity", Exact = true })).ToHaveTextAsync("Single room requested");

        var assigned = await NewWorld();
        await assigned.Page.GotoAsync(CarePages.RoomReferral);
        await assigned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply referral", Exact = true }).ClickAsync();
        await Expect(assigned.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Single room", Exact = true })).ToBeCheckedAsync();
        await Expect(assigned.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Placement activity", Exact = true })).ToHaveTextAsync("No request recorded");
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1415_0)]
    public async Task a_saved_room_card_opens_with_its_stored_language_layout_and_print_policy()
    {
        var house = await NewWorld();
        await house.Page.GotoAsync(CarePages.RoomCard);
        await house.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved Arabic card", Exact = true }).ClickAsync();
        await Expect(house.Page.GetByText("غرفة مفردة", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await house.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview print layout", Exact = true }).ClickAsync();
        await Expect(house.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print instruction", Exact = true })).ToHaveTextAsync("Print right to left with the room name first");
        await Expect(house.Page.GetByText("Use the house print stock", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var imported = await NewWorld();
        await imported.Page.GotoAsync(CarePages.RoomCard);
        await imported.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open saved English card", Exact = true }).ClickAsync();
        await Expect(imported.Page.GetByText("Single room", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await imported.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview print layout", Exact = true }).ClickAsync();
        await Expect(imported.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Print instruction", Exact = true })).ToHaveTextAsync("Print left to right with the room name last");
        await Expect(imported.Page.GetByText("Use the house print stock", new PageGetByTextOptions { Exact = true })).Not.ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F36.M21_X1416_0)]
    public async Task a_provisional_bed_request_stays_open_while_an_identified_room_can_be_reserved()
    {
        var provisional = await NewWorld();
        await provisional.Page.GotoAsync(CarePages.RoomInventory);
        await provisional.Page.GetByText("Next available bed", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(provisional.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Inventory code", Exact = true })).ToHaveTextAsync("Awaiting an inventory code");
        await provisional.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare bed request", Exact = true }).ClickAsync();
        await Expect(provisional.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Bed request", Exact = true })).ToHaveTextAsync("Request the next available bed");

        var identified = await NewWorld();
        await identified.Page.GotoAsync(CarePages.RoomInventory);
        await identified.Page.GetByText("Garden room", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(identified.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Inventory code", Exact = true })).ToHaveTextAsync("G-14");
        await identified.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare bed request", Exact = true }).ClickAsync();
        await Expect(identified.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Bed request", Exact = true })).ToHaveTextAsync("Prepare reservation for G-14");
    }
}
