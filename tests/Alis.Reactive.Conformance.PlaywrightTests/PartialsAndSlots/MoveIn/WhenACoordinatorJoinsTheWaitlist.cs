using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorJoinsTheWaitlist(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X65_0)]
    public async Task accepting_the_terms_saves_a_waitlist_place_and_leaves_the_draft_and_bed_as_they_were()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(world.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await Expect(world.Page.GetByText("No bed is reserved by joining the waitlist.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("Family requests a window bed.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-result")).ToHaveTextAsync("Helen Ward is number 1 on the waitlist");
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("W100: Helen Ward, Harbor House, Assisted, position 1. Family requests a window bed.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("W100: Helen Ward, Harbor House, Assisted, position 1. Family requests a window bed.");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(world.Page.Locator("#allocation-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X66_0)]
    public async Task unchecked_terms_refuse_the_join_and_preserve_the_empty_waitlist()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(world.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await Expect(world.Page.GetByText("No bed is reserved by joining the waitlist.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("Family requests a window bed.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).GetByText("Accept the waitlist terms.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        Assert.That(world.SentTo(CareEndpoints.MoveInWaitlist), Is.EqualTo(0));
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.MoveIn);
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(open.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await open.Seed(MoveInChanges.TheBedIsTaken());
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await Expect(open.Page.GetByText("No bed is reserved by joining the waitlist.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(open.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await open.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("Family requests a window bed.");
        await open.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).CheckAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#waitlist-result")).ToHaveTextAsync("Helen Ward is number 1 on the waitlist");
        await Expect(open.Page.Locator("#waitlist-record")).ToHaveTextAsync("W100: Helen Ward, Harbor House, Assisted, position 1. Family requests a window bed.");

    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X67_0)]
    public async Task a_community_closed_after_the_offer_refuses_the_join_and_saves_no_place()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(world.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await Expect(world.Page.GetByText("No bed is reserved by joining the waitlist.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("Family requests a window bed.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).CheckAsync();
        await world.Seed(MoveInChanges.TheWaitlistCloses("C1"));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-result")).ToHaveTextAsync("Harbor House is closed to waitlist entries.");
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#waitlist-community")).ToHaveTextAsync("Harbor House is closed to waitlist entries.");
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.MoveIn);
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(open.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await open.Seed(MoveInChanges.TheBedIsTaken());
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await Expect(open.Page.GetByText("No bed is reserved by joining the waitlist.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(open.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await open.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("Family requests a window bed.");
        await open.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).CheckAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#waitlist-result")).ToHaveTextAsync("Helen Ward is number 1 on the waitlist");
        await Expect(open.Page.Locator("#waitlist-record")).ToHaveTextAsync("W100: Helen Ward, Harbor House, Assisted, position 1. Family requests a window bed.");

    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X68_0)]
    public async Task a_failed_waitlist_connection_leaves_the_draft_without_a_waitlist_place()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(world.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await Expect(world.Page.GetByText("No bed is reserved by joining the waitlist.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("Family requests a window bed.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).CheckAsync();
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInWaitlist));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-result")).ToHaveTextAsync("The waitlist request failed.");
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.MoveIn);
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(open.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await open.Seed(MoveInChanges.TheBedIsTaken());
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await Expect(open.Page.GetByText("No bed is reserved by joining the waitlist.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(open.Page.Locator("#waitlist-record")).ToHaveTextAsync("No waitlist entry.");
        await open.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("Family requests a window bed.");
        await open.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).CheckAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#waitlist-result")).ToHaveTextAsync("Helen Ward is number 1 on the waitlist");
        await Expect(open.Page.Locator("#waitlist-record")).ToHaveTextAsync("W100: Helen Ward, Harbor House, Assisted, position 1. Family requests a window bed.");

    }
}
