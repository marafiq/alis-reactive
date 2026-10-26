using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInContacts;

namespace Alis.Reactive.PlaywrightTests.Patterns.MoveInContacts;

/// <summary>
/// As the admissions coordinator, I want every contact I type on the move-in packet to reach the server
/// under its own name, whether it is the resident's nested name, an emergency contact in the list or the
/// next of kin on a card of its own, so that the packet the business office reads is the one I filled.
/// </summary>
/// <remarks>
/// Each input's id is derived from the model member, so a test reaches it as the framework names it, and
/// its label points at it. The list's rows are one partial rendered per item; the next of kin is a card
/// of another model rendered under a field prefix, with a save of its own. The alternate page renders that
/// card twice, under two prefixes, which the audit found collides on ids.
/// </remarks>
[TestFixture]
public class WhenTheCoordinatorRecordsMoveInContacts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/MoveInContacts";

    private ILocator PacketStatus => Page.Locator("#packet-status");
    private ILocator FormStatus => Page.Locator("#form-status");
    private ILocator NextOfKinStatus => Page.Locator("#nextofkin-status");
    private ILocator AlternateStatus => Page.Locator("#alternate-status");

    private ILocator FirstName => Page.Locator($"#{IdGenerator.For<MoveInContactsModel>(m => m.Resident.First)}");
    private ILocator LastName => Page.Locator($"#{IdGenerator.For<MoveInContactsModel>(m => m.Resident.Last)}");
    private ILocator FirstContactName => Page.Locator($"#{IdGenerator.For<MoveInContactsModel>(m => m.Contacts[0].Name)}");
    private ILocator FirstContactPhone => Page.Locator($"#{IdGenerator.For<MoveInContactsModel>(m => m.Contacts[0].Phone)}");
    private ILocator SecondContactName => Page.Locator($"#{IdGenerator.For<MoveInContactsModel>(m => m.Contacts[1].Name)}");
    private ILocator SecondContactPhone => Page.Locator($"#{IdGenerator.For<MoveInContactsModel>(m => m.Contacts[1].Phone)}");

    private ILocator SavePacket => Page.Locator("#save-packet");
    private ILocator SendForm => Page.Locator("#send-form");
    private ILocator SaveNextOfKin => Page.Locator("#save-nextofkin");
    private ILocator SaveAlternate => Page.Locator("#save-alternate");

    private async Task OpenThePacket()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(PacketStatus).ToHaveTextAsync("not yet");
    }

    private async Task FillTheResidentAndBothContacts()
    {
        await FirstName.FillAsync("Margaret");
        await LastName.FillAsync("Hale");
        await FirstContactName.FillAsync("Ruth Hale");
        await FirstContactPhone.FillAsync("555-0101");
        await SecondContactName.FillAsync("Tom Hale");
        await SecondContactPhone.FillAsync("555-0102");
    }

    [Test]
    public async Task the_residents_name_is_typed_into_the_fields_its_labels_point_at_and_saved_nested()
    {
        await OpenThePacket();

        await Page.GetByLabel("First name").FillAsync("Margaret");
        await Page.GetByLabel("Last name").FillAsync("Hale");
        await SavePacket.ClickAsync();

        await Expect(PacketStatus).ToHaveTextAsync("Margaret Hale; contacts: none, none.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task each_emergency_contact_is_saved_at_its_own_position_in_the_packet()
    {
        await OpenThePacket();
        await FillTheResidentAndBothContacts();

        await SavePacket.ClickAsync();

        await Expect(PacketStatus).ToHaveTextAsync("Margaret Hale; contacts: Ruth Hale (555-0101), Tom Hale (555-0102).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_the_packet_as_a_form_saves_the_same_contacts()
    {
        await OpenThePacket();
        await FillTheResidentAndBothContacts();

        await SendForm.ClickAsync();

        await Expect(FormStatus).ToHaveTextAsync("Margaret Hale; contacts: Ruth Hale (555-0101), Tom Hale (555-0102).");
        await Expect(PacketStatus).ToHaveTextAsync("not yet");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_contacts_labels_point_at_that_contacts_own_fields()
    {
        await OpenThePacket();
        await Page.GetByLabel("First name").FillAsync("Margaret");
        await Page.GetByLabel("Last name").FillAsync("Hale");

        await Page.GetByLabel("Contact 2 name").FillAsync("Tom Hale");
        await Page.GetByLabel("Contact 2 phone").FillAsync("555-0102");
        await SavePacket.ClickAsync();

        await Expect(PacketStatus).ToHaveTextAsync("Margaret Hale; contacts: none, Tom Hale (555-0102).");
        AssertNoConsoleErrors();
    }

    // Each row's Call button carries a plan of its own and names the contact in its own row.
    [Test]
    public async Task calling_a_contact_from_its_row_names_that_contact_alone()
    {
        await OpenThePacket();
        await FirstContactName.FillAsync("Ruth Hale");
        await SecondContactName.FillAsync("Tom Hale");

        await Page.Locator("#call-contact-1").ClickAsync();

        await Expect(Page.Locator("#calling-line-1")).ToBeVisibleAsync();
        await Expect(Page.Locator("#calling-1")).ToHaveTextAsync("Tom Hale");
        await Expect(Page.Locator("#calling-line-0")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_next_of_kin_is_saved_under_its_own_name_from_its_card()
    {
        await OpenThePacket();

        await Page.GetByLabel("Next of kin name").FillAsync("Ruth Hale");
        await Page.GetByLabel("Next of kin phone").FillAsync("555-0101");
        await SaveNextOfKin.ClickAsync();

        await Expect(NextOfKinStatus).ToHaveTextAsync("Next of kin: Ruth Hale (555-0101); alternate: none.");
        AssertNoConsoleErrors();
    }

    // The alternate page renders the contact card twice, under two prefixes. The server binds the alternate
    // contact apart from the next of kin, by the names the alternate card's inputs carry.
    [Test]
    public async Task an_alternate_contact_is_saved_apart_from_the_next_of_kin()
    {
        await NavigateToAndWaitForBoot($"{Path}/Alternate");
        await Page.GetByLabel("Next of kin name").FillAsync("Ruth Hale");
        await Page.GetByLabel("Next of kin phone").FillAsync("555-0101");
        await Page.GetByLabel("Alternate contact name").FillAsync("Tom Hale");
        await Page.GetByLabel("Alternate contact phone").FillAsync("555-0102");

        await SaveAlternate.ClickAsync();

        await Expect(AlternateStatus).ToHaveTextAsync("Next of kin: none; alternate: Tom Hale (555-0102).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_next_of_kin_beside_an_alternate_contact_is_still_saved_as_the_next_of_kin()
    {
        await NavigateToAndWaitForBoot($"{Path}/Alternate");
        await Page.GetByLabel("Next of kin name").FillAsync("Ruth Hale");
        await Page.GetByLabel("Next of kin phone").FillAsync("555-0101");
        await Page.GetByLabel("Alternate contact name").FillAsync("Tom Hale");
        await Page.GetByLabel("Alternate contact phone").FillAsync("555-0102");

        await SaveNextOfKin.ClickAsync();

        await Expect(NextOfKinStatus).ToHaveTextAsync("Next of kin: Ruth Hale (555-0101); alternate: none.");
        AssertNoConsoleErrors();
    }
}
