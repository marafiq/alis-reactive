using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffSetUpFamilyContactCards(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F49.M21_X1506_0)]
    public async Task a_family_contact_type_and_its_completion_hint_change_together()
    {
        var email = await NewWorld();
        await email.Page.GotoAsync(CarePages.FamilyContactPhone);
        await email.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use email contact", Exact = true }).ClickAsync();
        await Expect(Entry(email.Page)).ToHaveAttributeAsync("type", "email");
        await Expect(Entry(email.Page)).ToHaveAttributeAsync("autocomplete", "email");
        await Expect(email.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact mode", Exact = true })).ToHaveTextAsync("Email address");

        var phone = await NewWorld();
        await phone.Page.GotoAsync(CarePages.FamilyContactEmail);
        await phone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use telephone contact", Exact = true }).ClickAsync();
        await Expect(Entry(phone.Page)).ToHaveAttributeAsync("type", "tel");
        await Expect(Entry(phone.Page)).ToHaveAttributeAsync("autocomplete", "tel");
        await Expect(phone.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact mode", Exact = true })).ToHaveTextAsync("Callback number");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1507_0)]
    public async Task the_card_layout_respects_width_direction_and_spacing()
    {
        var compact = await NewWorld();
        await compact.Page.GotoAsync(CarePages.FamilyContactPhone);
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add visitor contact labels", Exact = true }).ClickAsync();
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use compact Arabic card", Exact = true }).ClickAsync();
        await Expect(ContactWrapper(compact.Page)).ToHaveAttributeAsync("style", "width: 176px;");
        await Expect(ContactWrapper(compact.Page)).ToHaveClassAsync("e-input-group e-control-wrapper e-valid-input e-prepend-wrapper e-append-wrapper compact-contact e-rtl");
        await Expect(compact.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Card layout", Exact = true })).ToHaveTextAsync("Compact Arabic contact card");

        var full = await NewWorld();
        await full.Page.GotoAsync(CarePages.FamilyContactEmail);
        await full.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add visitor contact labels", Exact = true }).ClickAsync();
        await full.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use full English card", Exact = true }).ClickAsync();
        await Expect(ContactWrapper(full.Page)).ToHaveAttributeAsync("style", "width: 40ch;");
        await Expect(ContactWrapper(full.Page)).ToHaveClassAsync("e-input-group e-control-wrapper e-prepend-wrapper e-append-wrapper full-contact");
        await Expect(full.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Card layout", Exact = true })).ToHaveTextAsync("Full English contact card");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1508_0)]
    public async Task an_empty_contact_field_shows_the_selected_caption_behavior()
    {
        var floating = await NewWorld();
        await floating.Page.GotoAsync(CarePages.FamilyContactEmail);
        await floating.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep caption raised", Exact = true }).ClickAsync();
        await Expect(floating.Page.GetByText("Contact address", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-top");
        await Expect(floating.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Caption choice", Exact = true })).ToHaveTextAsync("Always above");
        await floating.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise caption on entry", Exact = true }).ClickAsync();
        await Expect(floating.Page.GetByText("Contact address", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-bottom");
        await Entry(floating.Page).ClickAsync();
        await Expect(floating.Page.GetByText("Contact address", new PageGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-top");

        var inline = await NewWorld();
        await inline.Page.GotoAsync(CarePages.FamilyContactEmail);
        await inline.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep caption raised", Exact = true }).ClickAsync();
        await inline.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use inline caption", Exact = true }).ClickAsync();
        await Expect(Entry(inline.Page)).ToHaveAttributeAsync("placeholder", "Contact address");
        await Expect(inline.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Caption choice", Exact = true })).ToHaveTextAsync("Inline wording");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1509_0)]
    public async Task a_contact_caption_keeps_room_for_reminders_on_the_chosen_side()
    {
        var standard = await NewWorld();
        await standard.Page.GotoAsync(CarePages.FamilyContactEmail);
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use compact Arabic card", Exact = true }).ClickAsync();
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep caption raised", Exact = true }).ClickAsync();
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call before arrival", Exact = true }).ClickAsync();
        await Expect(standard.Page.GetByText("Contact address", new PageGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 83px;");

        var saved = await NewWorld();
        await saved.Page.GotoAsync(CarePages.FamilyContactPhone);
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use compact Arabic card", Exact = true }).ClickAsync();
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep caption raised", Exact = true }).ClickAsync();
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call before arrival", Exact = true }).ClickAsync();
        await Expect(saved.Page.GetByText("Callback number", new PageGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 83px;");

        var trailing = await NewWorld();
        await trailing.Page.GotoAsync(CarePages.FamilyContactEmail);
        await trailing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use compact Arabic card", Exact = true }).ClickAsync();
        await trailing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep caption raised", Exact = true }).ClickAsync();
        await trailing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Contact at check-in", Exact = true }).ClickAsync();
        await Expect(trailing.Page.GetByText("Contact address", new PageGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 147px;");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1510_0)]
    public async Task visitor_entry_and_extension_labels_can_be_added_and_removed_from_the_card()
    {
        var marked = await NewWorld();
        await marked.Page.GotoAsync(CarePages.FamilyContactPhone);
        await marked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add visitor contact labels", Exact = true }).ClickAsync();
        var labelsAndContact = Entry(marked.Page)
            .Or(marked.Page.GetByText("Visitor entrance", new PageGetByTextOptions { Exact = true }))
            .Or(marked.Page.GetByText("Extension", new PageGetByTextOptions { Exact = true }));
        await Expect(labelsAndContact.Nth(0)).ToHaveTextAsync("Visitor entrance");
        await Expect(labelsAndContact.Nth(1)).ToHaveValueAsync("Front desk");
        await Expect(labelsAndContact.Nth(2)).ToHaveTextAsync("Extension");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.FamilyContactEmail);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add visitor contact labels", Exact = true }).ClickAsync();
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use direct-call card", Exact = true }).ClickAsync();
        await Expect(Entry(plain.Page)).ToHaveValueAsync("");
        await Expect(plain.Page.GetByText("Visitor entrance", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
        await Expect(plain.Page.GetByText("Extension", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1511_0)]
    public async Task quick_clear_is_available_for_replacement_contacts_only()
    {
        var replace = await NewWorld();
        await replace.Page.GotoAsync(CarePages.FamilyContactPhone);
        await replace.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Enable quick clearing", Exact = true }).ClickAsync();
        await Expect(replace.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Clearing policy", Exact = true })).ToHaveTextAsync("Quick clearing available");
        await Entry(replace.Page).ClickAsync();
        await replace.Page.GetByLabel("close", new PageGetByLabelOptions { Exact = true }).ClickAsync();
        await Expect(Entry(replace.Page)).ToHaveValueAsync("");
        await Expect(replace.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact draft", Exact = true })).ToHaveTextAsync("Enter a replacement contact");

        var keep = await NewWorld();
        await keep.Page.GotoAsync(CarePages.FamilyContactPhone);
        await keep.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Enable quick clearing", Exact = true }).ClickAsync();
        await keep.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep existing contact", Exact = true }).ClickAsync();
        await Expect(Entry(keep.Page)).ToHaveValueAsync("Front desk");
        await Expect(keep.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Clearing policy", Exact = true })).ToHaveTextAsync("Type to edit");
        await Expect(keep.Page.GetByLabel("close", new PageGetByLabelOptions { Exact = true })).ToHaveCountAsync(0);
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1512_0)]
    public async Task visit_directions_can_hold_a_second_line_while_a_callback_uses_one()
    {
        var directions = await NewWorld();
        await directions.Page.GotoAsync(CarePages.FamilyContactPhone);
        await directions.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Make room for directions", Exact = true }).ClickAsync();
        await Entry(directions.Page).FillAsync("North wing");
        await Entry(directions.Page).PressAsync("End");
        await Entry(directions.Page).PressAsync("Enter");
        await Entry(directions.Page).PressAsync("B");
        await Expect(Entry(directions.Page)).ToHaveValueAsync("North wing\nB");
        await directions.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review entry format", Exact = true }).ClickAsync();
        await Expect(directions.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Entry format", Exact = true })).ToHaveTextAsync("Visit directions");

        var callback = await NewWorld();
        await callback.Page.GotoAsync(CarePages.FamilyContactEmail);
        await callback.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Make room for directions", Exact = true }).ClickAsync();
        await callback.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Use single-line contact", Exact = true }).ClickAsync();
        await Expect(Entry(callback.Page)).ToHaveAttributeAsync("type", "tel");
        await callback.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review entry format", Exact = true }).ClickAsync();
        await Expect(callback.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Entry format", Exact = true })).ToHaveTextAsync("Callback entry");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1516_0)]
    public async Task screen_reader_review_uses_the_instructions_on_the_real_contact_field()
    {
        var phone = await NewWorld();
        await phone.Page.GotoAsync(CarePages.FamilyContactPhone);
        await phone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review contact instructions", Exact = true }).ClickAsync();
        await Expect(phone.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact instructions", Exact = true })).ToHaveTextAsync("Call the front desk");
        await Expect(phone.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Entry hint", Exact = true })).ToHaveTextAsync("Callback number");

        var email = await NewWorld();
        await email.Page.GotoAsync(CarePages.FamilyContactEmail);
        await email.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review contact instructions", Exact = true }).ClickAsync();
        await Expect(email.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact instructions", Exact = true })).ToHaveTextAsync("Write to reception");
        await Expect(email.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Entry hint", Exact = true })).ToHaveTextAsync("Contact address");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1517_0)]
    public async Task community_language_restores_the_standard_hint_on_the_contact_field()
    {
        var community = await NewWorld();
        await community.Page.GotoAsync(CarePages.FamilyContactCommunity);
        await community.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use evening desk wording", Exact = true }).ClickAsync();
        await community.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use community language", Exact = true }).ClickAsync();
        await Expect(Entry(community.Page)).ToHaveAttributeAsync("placeholder", "Contact the family");
        await Expect(community.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wording source", Exact = true })).ToHaveTextAsync("Community wording");

        var custom = await NewWorld();
        await custom.Page.GotoAsync(CarePages.FamilyContactCommunity);
        await custom.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use evening desk wording", Exact = true }).ClickAsync();
        await Expect(Entry(custom.Page)).ToHaveAttributeAsync("placeholder", "Ask for the evening contact");
        await Expect(custom.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Wording source", Exact = true })).ToHaveTextAsync("Desk wording");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1518_0)]
    public async Task a_protected_contact_releases_only_the_selected_field_attributes()
    {
        var released = await NewWorld();
        await released.Page.GotoAsync(CarePages.FamilyContactProtected);
        await released.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release protected contact", Exact = true }).ClickAsync();
        await Expect(Entry(released.Page)).ToBeFocusedAsync();
        await Expect(Entry(released.Page)).ToHaveAttributeAsync("aria-label", "Call the front desk");
        await Expect(Entry(released.Page)).Not.ToHaveAttributeAsync("disabled", "");
        await Expect(Entry(released.Page)).Not.ToHaveAttributeAsync("readonly", "");
        await Expect(released.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact protection", Exact = true })).ToHaveTextAsync("Released without a hint");

        var protectedCard = await NewWorld();
        await protectedCard.Page.GotoAsync(CarePages.FamilyContactProtected);
        await protectedCard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use associated contact label", Exact = true }).ClickAsync();
        await Expect(Entry(protectedCard.Page)).ToHaveAttributeAsync("disabled", "");
        await Expect(Entry(protectedCard.Page)).ToHaveAttributeAsync("readonly", "");
        await Expect(Entry(protectedCard.Page)).ToHaveAttributeAsync("placeholder", "Callback number");
        await Expect(protectedCard.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact protection", Exact = true })).ToHaveTextAsync("Protected field uses contact label");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1519_0)]
    public async Task staff_can_disable_saved_contact_suggestions_or_rest_the_choice_with_the_browser()
    {
        var kiosk = await NewWorld();
        await kiosk.Page.GotoAsync(CarePages.FamilyContactPhone);
        await kiosk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use shared kiosk", Exact = true }).ClickAsync();
        await Expect(Entry(kiosk.Page)).ToHaveAttributeAsync("autocomplete", "off");
        await Expect(kiosk.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Completion policy", Exact = true })).ToHaveTextAsync("Shared kiosk");

        var office = await NewWorld();
        await office.Page.GotoAsync(CarePages.FamilyContactEmail);
        await office.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Allow saved entries", Exact = true }).ClickAsync();
        await Expect(office.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Completion policy", Exact = true })).ToHaveTextAsync("Saved entries allowed");
        await Expect(Entry(office.Page)).Not.ToHaveAttributeAsync("autocomplete", "email");
        await office.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use browser default", Exact = true }).ClickAsync();
        await Expect(office.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Completion policy", Exact = true })).ToHaveTextAsync("Browser default");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1521_0)]
    public async Task the_printed_transfer_card_uses_its_member_or_the_configured_reception_extension()
    {
        var direct = await NewWorld();
        await direct.Page.GotoAsync(CarePages.FamilyContactPhone);
        var internalCard = direct.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Telephone extension", Exact = true });
        await Expect(internalCard.GetByRole(AriaRole.Textbox)).ToHaveValueAsync("214");
        await Expect(internalCard.GetByText("Dial internally", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(internalCard.GetByText("Main building", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var reception = await NewWorld();
        await reception.Page.GotoAsync(CarePages.FamilyContactEmail);
        var receptionCard = reception.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Telephone extension", Exact = true });
        await Expect(receptionCard.GetByRole(AriaRole.Textbox)).ToHaveValueAsync("408");
        await Expect(receptionCard.GetByText("Via reception", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(receptionCard.GetByText("Evening desk", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1522_0)]
    public async Task resetting_a_contact_card_width_restores_the_standard_field_while_rebuilding_preserves_a_custom_width()
    {
        var reset = await NewWorld();
        await reset.Page.GotoAsync(CarePages.FamilyContactPhone);
        await reset.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add visitor contact labels", Exact = true }).ClickAsync();
        await reset.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use compact Arabic card", Exact = true }).ClickAsync();
        await Expect(ContactWrapper(reset.Page)).ToHaveAttributeAsync("style", "width: 176px;");
        await reset.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Restore standard width", Exact = true }).ClickAsync();
        await Expect(ContactWrapper(reset.Page)).ToHaveAttributeAsync("style", "width: 100%;");
        await Expect(Entry(reset.Page)).ToHaveValueAsync("Front desk");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.FamilyContactEmail);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use full English card", Exact = true }).ClickAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Rebuild printed card", Exact = true }).ClickAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add visitor contact labels", Exact = true }).ClickAsync();
        await Expect(ContactWrapper(kept.Page)).ToHaveAttributeAsync("style", "width: 40ch;");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1523_0)]
    public async Task reception_and_internal_extension_cards_open_with_their_own_layout_and_clear_policy()
    {
        var reception = await NewWorld();
        await reception.Page.GotoAsync(CarePages.FamilyContactEmail);
        var shortCard = reception.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Telephone extension", Exact = true });
        await Expect(shortCard.GetByText("Via receptionExtension hintEvening desk", new LocatorGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 176px;");
        await Expect(shortCard.GetByText("Extension hint", new LocatorGetByTextOptions { Exact = true })).ToHaveClassAsync("e-float-text e-label-top");
        await shortCard.GetByRole(AriaRole.Textbox).ClickAsync();
        await Expect(shortCard.GetByLabel("close", new LocatorGetByLabelOptions { Exact = true })).ToBeVisibleAsync();
        await shortCard.GetByLabel("close", new LocatorGetByLabelOptions { Exact = true }).ClickAsync();
        await Expect(shortCard.GetByRole(AriaRole.Textbox)).ToHaveValueAsync("");

        var internalLine = await NewWorld();
        await internalLine.Page.GotoAsync(CarePages.FamilyContactPhone);
        var fullCard = internalLine.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Telephone extension", Exact = true });
        await Expect(fullCard.GetByText("Dial internallyMain building", new LocatorGetByTextOptions { Exact = true })).ToHaveAttributeAsync("style", "width: 40ch;");
        await Expect(fullCard.GetByRole(AriaRole.Textbox)).ToHaveAttributeAsync("placeholder", "Extension hint");
        await fullCard.GetByRole(AriaRole.Textbox).ClickAsync();
        await Expect(fullCard.GetByRole(AriaRole.Textbox)).ToHaveValueAsync("214");
        await Expect(fullCard.GetByLabel("close", new LocatorGetByLabelOptions { Exact = true })).ToHaveCountAsync(0);
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1524_0)]
    public async Task a_missing_contact_hint_uses_the_record_and_a_deliberate_blank_uses_reception()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.FamilyContactPhone);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use resident instructions", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Printed contact instructions", Exact = true })).ToHaveTextAsync("Follow the instructions in the resident record");
        await Expect(Entry(resident.Page)).Not.ToHaveAttributeAsync("placeholder", "Callback number");

        var suppressed = await NewWorld();
        await suppressed.Page.GotoAsync(CarePages.FamilyContactEmail);
        await suppressed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Suppress printed hint", Exact = true }).ClickAsync();
        await Expect(suppressed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Printed contact instructions", Exact = true })).ToHaveTextAsync("Ask the front desk before contacting the family");
        await Expect(Entry(suppressed.Page)).Not.ToHaveAttributeAsync("placeholder", "Contact address");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1525_0)]
    public async Task the_mounted_contact_uses_clinic_or_visitor_routing_labels()
    {
        var clinic = await NewWorld();
        await clinic.Page.GotoAsync(CarePages.FamilyContactPhone);
        await clinic.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply clinic contact labels", Exact = true }).ClickAsync();
        var clinicParts = Entry(clinic.Page)
            .Or(clinic.Page.GetByText("Clinic reception", new PageGetByTextOptions { Exact = true }))
            .Or(clinic.Page.GetByText("After hours", new PageGetByTextOptions { Exact = true }));
        await Expect(clinicParts.Nth(0)).ToHaveTextAsync("Clinic reception");
        await Expect(clinicParts.Nth(1)).ToHaveValueAsync("Front desk");
        await Expect(clinicParts.Nth(2)).ToHaveTextAsync("After hours");

        var visitor = await NewWorld();
        await visitor.Page.GotoAsync(CarePages.FamilyContactPhone);
        await visitor.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply clinic contact labels", Exact = true }).ClickAsync();
        await visitor.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add visitor contact labels", Exact = true }).ClickAsync();
        var visitorParts = Entry(visitor.Page)
            .Or(visitor.Page.GetByText("Visitor entrance", new PageGetByTextOptions { Exact = true }))
            .Or(visitor.Page.GetByText("Extension", new PageGetByTextOptions { Exact = true }));
        await Expect(visitorParts.Nth(0)).ToHaveTextAsync("Visitor entrance");
        await Expect(visitorParts.Nth(1)).ToHaveValueAsync("Front desk");
        await Expect(visitorParts.Nth(2)).ToHaveTextAsync("Extension");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1526_0)]
    public async Task a_shared_reception_screen_can_withhold_and_restore_the_resident_contact()
    {
        var shared = await NewWorld();
        await shared.Page.GotoAsync(CarePages.FamilyContactPhone);
        await shared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Withhold contact", Exact = true }).ClickAsync();
        await Expect(shared.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact privacy", Exact = true })).ToHaveTextAsync("Contact withheld on the shared screen");
        await Expect(Entry(shared.Page)).ToHaveCountAsync(0);

        var staff = await NewWorld();
        await staff.Page.GotoAsync(CarePages.FamilyContactPhone);
        await staff.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Withhold contact", Exact = true }).ClickAsync();
        await staff.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show staff contact", Exact = true }).ClickAsync();
        await Expect(Entry(staff.Page)).ToHaveAttributeAsync("type", "tel");
        await Expect(Entry(staff.Page)).ToHaveValueAsync("Front desk");
        await Expect(staff.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact privacy", Exact = true })).ToHaveTextAsync("Staff can see the contact");
    }

    [Test]
    [FailsFor(Promises.M21_F49.M21_X1527_0)]
    public async Task routing_labels_use_their_own_contact_channel_again_after_a_card_is_rebuilt()
    {
        var mixed = await NewWorld();
        await mixed.Page.GotoAsync(CarePages.FamilyContactEmail);
        await mixed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply channel routing labels", Exact = true }).ClickAsync();
        var mail = mixed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Contact destination", Exact = true });
        var extension = mixed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Telephone extension", Exact = true });
        await Expect(mail.GetByText("Email reception", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(mail.GetByText("Shared inbox", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(extension.GetByText("Call reception", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(extension.GetByText("Transfer desk", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var changed = await NewWorld();
        await changed.Page.GotoAsync(CarePages.FamilyContactEmail);
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply channel routing labels", Exact = true }).ClickAsync();
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use telephone contact", Exact = true }).ClickAsync();
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Rebuild printed card", Exact = true }).ClickAsync();
        var callback = changed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Contact destination", Exact = true });
        var routing = callback.GetByRole(AriaRole.Textbox)
            .Or(callback.GetByText("Call reception", new LocatorGetByTextOptions { Exact = true }))
            .Or(callback.GetByText("Transfer desk", new LocatorGetByTextOptions { Exact = true }));
        await Expect(routing.Nth(0)).ToHaveTextAsync("Call reception");
        await Expect(routing.Nth(1)).ToHaveAttributeAsync("type", "tel");
        await Expect(routing.Nth(2)).ToHaveTextAsync("Transfer desk");
    }

    private static ILocator ContactWrapper(IPage page) => page.GetByText("Visitor entrance Extension", new PageGetByTextOptions { Exact = true });

    private static ILocator Entry(IPage page) => page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Contact destination", Exact = true }).GetByRole(AriaRole.Textbox);
}
