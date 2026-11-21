using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorRemovesAContact(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X50_0)]
    public async Task a_contact_removed_leaves_the_rows_and_the_rows_that_stay_keep_what_was_typed_in_them()
    {
        var removed = await NewWorld();
        await removed.Page.GotoAsync(CarePages.MoveIn);
        await removed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550199");
        await removed.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        await Expect(removed.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact removed: CONTACTS8");
        await Expect(removed.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550199, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
        });
        await Expect(removed.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await removed.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Authority on file", Exact = true }).ClickAsync();
        await Expect(removed.Page.Locator("#authority-on-file")).ToHaveTextAsync("Alice Ward: power of attorney POA-100, from 2026-01-01 to 2027-01-01, consent given");
        await removed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(removed.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550199, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
        });
        await Expect(removed.Page.Locator("#saved-version")).ToHaveTextAsync("8");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.MoveIn);
        await kept.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("5185550199");
        await kept.Page.GotoAsync(CarePages.MoveIn);
        await Expect(kept.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(kept.Page.Locator("#saved-version")).ToHaveTextAsync("7");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X51_0)]
    public async Task a_removal_whose_row_holds_no_id_is_refused_before_it_is_sent()
    {
        var nameless = await NewWorld();
        await nameless.Seed(MoveInChanges.TheContactHoldsNoId("CT2"));
        await nameless.Page.GotoAsync(CarePages.MoveIn);
        await nameless.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        await Expect(nameless.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Choose a contact.");
        Assert.That(nameless.SentTo(CareEndpoints.MoveInContactRows), Is.EqualTo(0));
        await nameless.Page.GotoAsync(CarePages.MoveIn);
        await Expect(nameless.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(nameless.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.MoveIn);
        await named.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        await Expect(named.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact removed: CONTACTS8");
        Assert.That(named.SentTo(CareEndpoints.MoveInContactOfBen), Is.EqualTo(1));
        Assert.That(named.SentTo(CareEndpoints.MoveInContactRows), Is.EqualTo(1));
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X52_0)]
    public async Task a_contact_the_payer_depends_on_stays_and_the_page_explains_why()
    {
        var depended = await NewWorld();
        await depended.Page.GotoAsync(CarePages.MoveIn);
        await depended.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Alice Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        await Expect(depended.Page.Locator("#payer-explanation")).ToHaveTextAsync("Alice Ward is the authorised payer contact for POL-100.");
        Assert.That(depended.SentTo(CareEndpoints.MoveInPayerExplanation), Is.EqualTo(1));
        await Expect(depended.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await depended.Page.GotoAsync(CarePages.MoveIn);
        await Expect(depended.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(depended.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var free = await NewWorld();
        await free.Page.GotoAsync(CarePages.MoveIn);
        await free.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        await Expect(free.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact removed: CONTACTS8");
        Assert.That(free.SentTo(CareEndpoints.MoveInPayerExplanation), Is.EqualTo(0));
        await Expect(free.Page.Locator("#payer-explanation")).ToBeEmptyAsync();
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X53_0)]
    public async Task a_removal_whose_connection_drops_says_so_and_keeps_every_row()
    {
        var dropped = await NewWorld();
        await dropped.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInContactOfBen));
        await dropped.InstallTheClock();
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await dropped.HoldTheClock();
        await dropped.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        await dropped.RunTheClockUntilItSends(CareEndpoints.MoveInContactOfBen, 4);
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The request failed.");
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        Assert.That(dropped.SentTo(CareEndpoints.MoveInContactRows), Is.EqualTo(0));
        await Expect(dropped.Page.Locator("#contacts-status")).ToBeEmptyAsync();
        await Expect(dropped.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact Remove",
        });
        await Expect(dropped.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.MoveIn);
        await answered.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        await Expect(answered.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact removed: CONTACTS8");
        await Expect(answered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: none");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X54_0)]
    public async Task a_removal_is_decided_by_the_payer_the_model_holds_when_the_server_reaches_it()
    {
        var overtaken = await NewWorld();
        var armed = await overtaken.Arm(LanesGate.Respond, CareEndpoints.MoveInContactOfBen);
        await overtaken.Page.GotoAsync(CarePages.MoveIn);
        await Expect(overtaken.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Move-in D100", Exact = true })).ToBeVisibleAsync();
        await overtaken.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        var removal = await armed.Held();
        await Expect(overtaken.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true })).ToHaveClassAsync("removing");
        await Expect(overtaken.Page.Locator("#contacts-status")).ToBeEmptyAsync();
        await overtaken.Seed(MoveInChanges.ThePayersContactIs("CT2"));
        await removal.Release();
        await Expect(overtaken.Page.Locator("#payer-explanation")).ToHaveTextAsync("Ben Ward is the authorised payer contact for POL-100.");
        await overtaken.Page.GotoAsync(CarePages.MoveIn);
        await Expect(overtaken.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100 Remove Authority on file",
            "Ben Ward, Son, 5185550201, ordinary contact, payer contact for POL-100 Remove",
        });
        await Expect(overtaken.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var unchanged = await NewWorld();
        var only = await unchanged.Arm(LanesGate.Respond, CareEndpoints.MoveInContactOfBen);
        await unchanged.Page.GotoAsync(CarePages.MoveIn);
        await Expect(unchanged.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Move-in D100", Exact = true })).ToBeVisibleAsync();
        await unchanged.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Ben Ward", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove", Exact = true }).ClickAsync();
        var held = await only.Held();
        await Expect(unchanged.Page.Locator("#contacts-status")).ToBeEmptyAsync();
        await held.Release();
        await Expect(unchanged.Page.Locator("#contacts-status")).ToHaveTextAsync("Contact removed: CONTACTS8");
        await Expect(unchanged.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Contacts on file", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Alice Ward, Daughter, 5185550101, power of attorney POA-100, payer contact for POL-100 Remove Authority on file",
        });
    }
}
