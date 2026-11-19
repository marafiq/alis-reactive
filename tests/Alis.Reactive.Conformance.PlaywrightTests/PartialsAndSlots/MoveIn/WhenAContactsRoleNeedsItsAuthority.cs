using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAContactsRoleNeedsItsAuthority(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X45_0)]
    public async Task an_authority_loaded_for_a_guardian_is_saved_with_the_page()
    {
        var saved = await NewWorld();
        await saved.Page.GotoAsync(CarePages.MoveIn);
        await saved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        await saved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true }).FillAsync("GUARD-200");
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(saved.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await saved.Page.GotoAsync(CarePages.MoveIn);
        await Expect(saved.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, guardian GUARD-200 Remove Authority on file",
        });
        await Expect(saved.Page.Locator("#saved-version")).ToHaveTextAsync("8");

        var unsaved = await NewWorld();
        await unsaved.Page.GotoAsync(CarePages.MoveIn);
        await unsaved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        await unsaved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true }).FillAsync("GUARD-200");
        await unsaved.Page.GotoAsync(CarePages.MoveIn);
        await Expect(unsaved.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(unsaved.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X46_0)]
    public async Task an_authority_that_ends_before_it_starts_is_refused_before_it_is_sent()
    {
        var backwards = await NewWorld();
        await backwards.Page.GotoAsync(CarePages.MoveIn);
        await backwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        await backwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true }).FillAsync("GUARD-200");
        await backwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await backwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await backwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(backwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("2026-11-17");
        await backwards.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contacts", Exact = true }).ClickAsync();
        await Expect(backwards.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: An authority cannot end before it starts.");
        Assert.That(backwards.SentTo(CareEndpoints.MoveInContacts), Is.EqualTo(0));
        await Expect(backwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByText("An authority cannot end before it starts.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await backwards.Page.GotoAsync(CarePages.MoveIn);
        await Expect(backwards.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(backwards.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var forwards = await NewWorld();
        await forwards.Page.GotoAsync(CarePages.MoveIn);
        await forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        await forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true }).FillAsync("GUARD-200");
        await forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(forwards.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Ends on", Exact = true }).GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("2027-01-01");
        await forwards.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contacts", Exact = true }).ClickAsync();
        await Expect(forwards.Page.Locator("#contacts-status")).ToHaveTextAsync("Contacts saved: CONTACTS8");
        await Expect(forwards.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, guardian GUARD-200 Remove Authority on file",
        });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X47_0)]
    public async Task a_revoked_authority_reference_is_refused_by_the_server_with_its_reason()
    {
        var revoked = await NewWorld();
        await revoked.Seed(MoveInChanges.TheAuthorityIsRevoked("GUARD-200"));
        await revoked.Page.GotoAsync(CarePages.MoveIn);
        await revoked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        await revoked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true }).FillAsync("GUARD-200");
        await revoked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contacts", Exact = true }).ClickAsync();
        await Expect(revoked.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: This authority reference was revoked.");
        await Expect(revoked.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        Assert.That(revoked.SentTo(CareEndpoints.MoveInContacts), Is.EqualTo(1));
        await revoked.Page.GotoAsync(CarePages.MoveIn);
        await Expect(revoked.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(revoked.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var standing = await NewWorld();
        await standing.Page.GotoAsync(CarePages.MoveIn);
        await standing.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        await standing.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true }).FillAsync("GUARD-200");
        await standing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contacts", Exact = true }).ClickAsync();
        await Expect(standing.Page.Locator("#contacts-status")).ToHaveTextAsync("Contacts saved: CONTACTS8");
        await Expect(standing.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, guardian GUARD-200 Remove Authority on file",
        });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X48_0)]
    public async Task an_authority_that_fails_to_load_says_so_and_claims_nothing_saved()
    {
        var dropped = await NewWorld();
        await dropped.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInContactAuthority));
        await dropped.InstallTheClock();
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await dropped.HoldTheClock();
        await dropped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        await dropped.RunTheClockUntilItSends(CareEndpoints.MoveInContactAuthority, 4);
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The request failed.");
        await Expect(dropped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true })).ToHaveCountAsync(0);
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
        await answered.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        await Expect(answered.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true })).ToBeVisibleAsync();
        await Expect(answered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: none");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X49_0)]
    public async Task an_authority_whose_role_went_back_to_ordinary_while_it_loaded_stays_off_the_page()
    {
        var replaced = await NewWorld();
        var armed = await replaced.Arm(LanesGate.Respond, CareEndpoints.MoveInContactAuthority);
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await Expect(replaced.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Move-in D100", Exact = true })).ToBeVisibleAsync();
        await replaced.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        var guardian = await armed.Held();
        await Expect(replaced.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true })).ToHaveCountAsync(0);
        await replaced.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Ordinary");
        await guardian.Release();
        await Expect(replaced.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(replaced.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true })).ToHaveCountAsync(0);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contacts", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#contacts-status")).ToHaveTextAsync("Contacts saved: CONTACTS8");
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await Expect(replaced.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(replaced.Page.Locator("#saved-version")).ToHaveTextAsync("8");

        var alone = await NewWorld();
        var only = await alone.Arm(LanesGate.Respond, CareEndpoints.MoveInContactAuthority);
        await alone.Page.GotoAsync(CarePages.MoveIn);
        await Expect(alone.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Move-in D100", Exact = true })).ToBeVisibleAsync();
        await alone.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Combobox, new LocatorGetByRoleOptions { Name = "Role", Exact = true }).SelectOptionAsync("Guardian");
        var held = await only.Held();
        await Expect(alone.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true })).ToHaveCountAsync(0);
        await held.Release();
        await Expect(alone.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Reference", Exact = true })).ToBeVisibleAsync();
    }
}
