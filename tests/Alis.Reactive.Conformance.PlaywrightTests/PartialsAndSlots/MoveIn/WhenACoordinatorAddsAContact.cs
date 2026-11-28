using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorAddsAContact(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X40_0)]
    public async Task a_contact_added_joins_the_rows_and_the_rows_already_there_keep_what_was_typed_in_them()
    {
        var added = await NewWorld();
        await added.Page.GotoAsync(CarePages.MoveIn);
        await added.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550199");
        await added.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cara Ward");
        await added.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Relationship", Exact = true }).SelectOptionAsync("Friend");
        await added.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("cara@example.test");
        await added.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550301");
        await added.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550302");
        await added.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(added.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact added: CONTACTS8");
        await Expect(added.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true })).ToHaveValueAsync("");
        await Expect(added.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550199, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
            "Cara Ward, Friend, 5185550301, ordinary contact Remove",
        });
        await Expect(added.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await added.Page.GotoAsync(CarePages.MoveIn);
        await Expect(added.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550199, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
            "Cara Ward, Friend, 5185550301, ordinary contact Remove",
        });
        await Expect(added.Page.Locator("#saved-version")).ToHaveTextAsync("8");

        var typed = await NewWorld();
        await typed.Page.GotoAsync(CarePages.MoveIn);
        await typed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550199");
        await typed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cara Ward");
        await typed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(typed.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(typed.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X41_0)]
    public async Task a_malformed_email_in_a_row_refuses_the_add_before_it_is_sent()
    {
        var malformed = await NewWorld();
        await malformed.Page.GotoAsync(CarePages.MoveIn);
        await malformed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("alice-at-example");
        await malformed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cara Ward");
        await malformed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("cara@example.test");
        await malformed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550301");
        await malformed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550302");
        await malformed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(malformed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Enter a valid contact email.");
        Assert.That(malformed.SentTo(CareEndpoints.MoveInContacts), Is.EqualTo(0));
        await Expect(malformed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByText("Enter a valid contact email.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await malformed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(malformed.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(malformed.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var wellFormed = await NewWorld();
        await wellFormed.Page.GotoAsync(CarePages.MoveIn);
        await wellFormed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cara Ward");
        await wellFormed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("cara@example.test");
        await wellFormed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550301");
        await wellFormed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550302");
        await wellFormed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(wellFormed.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact added: CONTACTS8");
        Assert.That(wellFormed.SentTo(CareEndpoints.MoveInContacts), Is.EqualTo(1));
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X42_0)]
    public async Task a_fifth_contact_is_refused_by_the_server_with_its_reason()
    {
        var full = await NewWorld();
        await full.Seed(MoveInChanges.TheDraftHoldsFourContacts());
        await full.Page.GotoAsync(CarePages.MoveIn);
        await full.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Eve Ward");
        await full.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Relationship", Exact = true }).SelectOptionAsync("Friend");
        await full.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("eve@example.test");
        await full.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550501");
        await full.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550502");
        await full.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(full.Page.Locator("#contacts-refusal")).ToHaveTextAsync("An admission has at most four contacts.");
        Assert.That(full.SentTo(CareEndpoints.MoveInContacts), Is.EqualTo(1));
        await Expect(full.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
            "Cara Ward, Friend, 5185550301, ordinary contact Remove",
            "Dan Ward, Son, 5185550401, ordinary contact Remove",
        });
        await full.Page.GotoAsync(CarePages.MoveIn);
        await Expect(full.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
            "Cara Ward, Friend, 5185550301, ordinary contact Remove",
            "Dan Ward, Son, 5185550401, ordinary contact Remove",
        });
        await Expect(full.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var roomy = await NewWorld();
        await roomy.Page.GotoAsync(CarePages.MoveIn);
        await roomy.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Eve Ward");
        await roomy.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Relationship", Exact = true }).SelectOptionAsync("Friend");
        await roomy.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("eve@example.test");
        await roomy.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550501");
        await roomy.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550502");
        await roomy.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(roomy.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact added: CONTACTS8");
        await Expect(roomy.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
            "Eve Ward, Friend, 5185550501, ordinary contact Remove",
        });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X43_0)]
    public async Task an_add_whose_connection_drops_says_so_once_and_adds_nothing()
    {
        var dropped = await NewWorld();
        await dropped.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInContacts));
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await dropped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cara Ward");
        await dropped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("cara@example.test");
        await dropped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550301");
        await dropped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550302");
        await dropped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The request failed.");
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        Assert.That(dropped.SentTo(CareEndpoints.MoveInContacts), Is.EqualTo(1));
        await Expect(dropped.Page.Locator("#contacts-status")).ToBeEmptyAsync();
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(dropped.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.MoveIn);
        await answered.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cara Ward");
        await answered.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("cara@example.test");
        await answered.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550301");
        await answered.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550302");
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(answered.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact added: CONTACTS8");
        await Expect(answered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: none");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X44_0)]
    public async Task a_command_raised_while_another_is_in_flight_starts_nothing()
    {
        var skipped = await NewWorld();
        var armed = await skipped.Arm(LanesGate.Respond, CareEndpoints.MoveInContacts);
        await skipped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(skipped.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Move-in D100", Exact = true })).ToBeVisibleAsync();
        await skipped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550199");
        await skipped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contacts", Exact = true }).ClickAsync();
        var saving = await armed.Held();
        await skipped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cara Ward");
        await skipped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("cara@example.test");
        await skipped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550301");
        await skipped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550302");
        await skipped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(skipped.Page.Locator("#contacts-status")).ToBeEmptyAsync();
        await saving.Release();
        await Expect(skipped.Page.Locator("#contacts-status")).ToHaveTextAsync("Contacts saved: CONTACTS8");
        await Expect(skipped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        Assert.That(skipped.SentTo(CareEndpoints.MoveInContacts), Is.EqualTo(1));
        await Expect(skipped.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550199, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await skipped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(skipped.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550199, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(skipped.Page.Locator("#saved-version")).ToHaveTextAsync("8");

        var inTurn = await NewWorld();
        await inTurn.Page.GotoAsync(CarePages.MoveIn);
        await inTurn.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550199");
        await inTurn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contacts", Exact = true }).ClickAsync();
        await Expect(inTurn.Page.Locator("#contacts-status")).ToHaveTextAsync("Contacts saved: CONTACTS8");
        await inTurn.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cara Ward");
        await inTurn.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("cara@example.test");
        await inTurn.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550301");
        await inTurn.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "New contact", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Alternate phone", Exact = true }).FillAsync("5185550302");
        await inTurn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add contact", Exact = true }).ClickAsync();
        await Expect(inTurn.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact added: CONTACTS9");
        await Expect(inTurn.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550199, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
            "Cara Ward, Daughter, 5185550301, ordinary contact Remove",
        });
    }
}
